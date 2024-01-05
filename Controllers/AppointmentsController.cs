using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Data;
using EGUI_Stage2.DTOs;
using EGUI_Stage2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EGUI_Stage2.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;

namespace EGUI_Stage2.Controllers
{
    [Authorize]
    public class VisitsController : ControllerBase
    {
        private readonly IScheduleManager _scheduleManager;
        private readonly UserManager<User> _userManager;

        public VisitsController(IScheduleManager scheduleEntryManager, UserManager<User> userManager)
        {
            _scheduleManager = scheduleEntryManager;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        [Route("get-detailed-future-schedule-for-doctor")]
        public async Task<IActionResult> GetDoctorDetailedFutureSchedule([FromQuery] string docUsername)//, [FromQuery] int upperBoundWeekOffset = -1
        {
            var doc = await _userManager.FindByNameAsync(docUsername);
            if (doc == null || !(await _userManager.IsInRoleAsync(doc, UserRoles.doctor)))
                return UnprocessableEntity(new Response { Status = "Error", Message = "No Doctor with given username exists." });
            DateTime dateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(DateTime.Today.Date).Date;
            var res = await _scheduleManager.GetSchedulesAsync(x => x.Doctor.UserName == docUsername && x.DateOfMonday.Date >= dateOfMonday.Date);


            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            DoctorDTO doctorDTO = new(doc);
            doctorDTO.SetAndFilterOutPastAppointments(res, user);

            return Ok(doctorDTO);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.patient)]
        [Route("subscribe-or-unsubscribe-appointment")]
        public async Task<IActionResult> SubsribeOrUnsubscribePatientToVisit([FromQuery] int visitSlotId, [FromQuery] bool isUnsubscribe = false) //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        {

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            var res = (await _scheduleManager.GetSchedulesAsync(x => x.ScheduleEntries.Any(y => y.VisitSlots.Any(z => z.Id == visitSlotId && (z.Patient == null || z.Patient.Id == user.Id)))));
            if (res == null || res.ToList().Count == 0)
                return BadRequest("Either the visitSlotId is wrong or the visitSlot is registered by a different patient already.");
            string msgPrefix = isUnsubscribe ? "Unsubscribed" : "Subscribed";
            if (isUnsubscribe)
                await _scheduleManager.SubscribeOrUnsubscribeFromVisitSlot(visitSlotId, null);
            else
                await _scheduleManager.SubscribeOrUnsubscribeFromVisitSlot(visitSlotId, user);

            return Ok(msgPrefix + " successfully!");
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.doctor)]
        [Route("set-visit-description")]
        public async Task<IActionResult> SetVisitDescription([FromQuery] int visitSlotId, [FromBody] string content) //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        {

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            var res = (await _scheduleManager.GetSchedulesAsync(x => x.Doctor.Id == user.Id && x.ScheduleEntries.Any(y => y.VisitSlots.Any(z => z.Id == visitSlotId))));
            if (res == null || res.ToList().Count == 0)
                return BadRequest("Either the visitSlotId is wrong or the visitSlot is not registered to you (as the doctor).");

            await _scheduleManager.EditVisitSlotNotes(visitSlotId, content);
            return Ok("Set description successfully!");
        }
    }
}
