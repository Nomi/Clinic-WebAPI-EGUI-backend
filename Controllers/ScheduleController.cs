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
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleManager _scheduleManager;
        private readonly UserManager<User> _userManager;

        public ScheduleController(IScheduleManager scheduleEntryManager, UserManager<User> userManager)
        {
            _scheduleManager = scheduleEntryManager;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.admin)]
        [Route("create-schedule")]
        public async Task<IActionResult> CreateScheduleEntry([FromBody] CreateScheduleDTO scheduleDTO)
        {
            if (scheduleDTO.Entries.GroupBy(x => x.dayOfWeek).Any(g => g.Count() > 1))
                return Conflict("ERROR: Request contains multiple schedule entries for the same day of the week.");

            var doc = await _userManager.FindByNameAsync(scheduleDTO.DoctorUsername);
            if (doc == null || !(await _userManager.IsInRoleAsync(doc,UserRoles.doctor)))
                return UnprocessableEntity(new Response { Status = "Error", Message = "A Doctor with given username does not exist." });

            DateTime dateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(scheduleDTO.DateOfMonday.ToDateTime(TimeOnly.MinValue).Date);
            var lol = (await _scheduleManager.GetSchedulesAsync(x=> x.Doctor.UserName == scheduleDTO.DoctorUsername)).ToList();//x=> x.DateOfMonday.Date.CompareTo(dateOfMonday)==0

            var existingScheduleList = (await _scheduleManager.GetSchedulesAsync(x => x.DateOfMonday.Date == dateOfMonday && x.Doctor.UserName == scheduleDTO.DoctorUsername)).ToList();
            if (existingScheduleList == null || existingScheduleList.Count > 0)
                return Conflict("Schedule for given week (defined by date of monday) exists for the doctor.");

            var nSchedule = scheduleDTO.ToSchedule(doc);
            await _scheduleManager.InsertScheduleAsync(nSchedule);
            return Ok("Schedule entry created.");
        }

        [HttpGet]
        [Route("get-week-schedule")]
        public async Task<IActionResult> GetWeekSchedule([FromQuery] int weekOffset = 0)
        {
            var res = (await _scheduleManager.GetSchedulesFromWeekOffsetAsync(weekOffset));
            var result = new List<GetScheduleDTO>();
            foreach(var schedule in res)
            {
                for (int i = 0; i < schedule.ScheduleEntries.Count; i++)
                { 
                    result.Add(new GetScheduleDTO(schedule, i));
                }
                if(schedule.ScheduleEntries.Count==0)
                {
                    result.Add(new GetScheduleDTO(schedule));
                }
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.admin)]
        [Route("modify-schedule-timings")]
        public async Task<IActionResult> UpdateScheduleEntry([FromBody] UpdateScheduleEntryDTO scheduleDTO)
        {
            if (scheduleDTO.Entries.GroupBy(x => x.dayOfWeek).Any(g => g.Count() > 1))
                return Conflict("ERROR: Request contains multiple schedule entries for the same day of the week.");

            var test = (await _scheduleManager.GetSchedulesAsync(x => x.Id == scheduleDTO.ScheduleId)).SingleOrDefault();
            if (test == null || test == default(Schedule))
                return UnprocessableEntity("ERROR: Schedule with given ID not found.");

            var nSchedule = scheduleDTO.ToSchedule(test.Doctor);

            await _scheduleManager.UpdateScheduleTimingsAsync(nSchedule);
            return Ok("Schedule entry updated.");
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.admin)]
        [Route("clone-schedule")]
        public async Task<IActionResult> CloneSchedule([FromQuery]string doctorUsername,[FromQuery] int sourceWeekOffset, [FromQuery] int targetWeekOffset)
        {
            var doc = await _userManager.FindByNameAsync(doctorUsername);
            if (doc == null || !(await _userManager.IsInRoleAsync(doc, UserRoles.doctor)))
                return UnprocessableEntity(new Response { Status = "Error", Message = "No Doctor with given username exists." });
            await _scheduleManager.CopySchedule(doc, sourceWeekOffset, targetWeekOffset);
            return Ok("Schedule cloned.");
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
            doctorDTO.SetAndFilterOutPastAppointments(res,user);

            return Ok(doctorDTO);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.patient)]
        [Route("subscribe-or-unsubscribe-appointment")]
        public async Task<IActionResult> SubsribeOrUnsubscribePatientToVisit([FromQuery] int visitSlotId, [FromQuery] bool isUnsubscribe = false) //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        { 

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            var res =(await _scheduleManager.GetSchedulesAsync(x => x.ScheduleEntries.Any(y => y.VisitSlots.Any(z => z.Id == visitSlotId && (z.Patient == null ||z.Patient.Id==user.Id)))));
            if (res == null || res.ToList().Count == 0)
                return BadRequest("Either the visitSlotId is wrong or the visitSlot is registered by a different patient already.");
            string msgPrefix = isUnsubscribe ? "Unsubscribed" : "Subscribed";
            if(isUnsubscribe)
                await _scheduleManager.SubscribeOrUnsubscribeFromVisitSlot(visitSlotId, null);
            else
                await _scheduleManager.SubscribeOrUnsubscribeFromVisitSlot(visitSlotId, user);

            return Ok(msgPrefix + " successfully!");
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.doctor)]
        [Route("set-visit-description-toggle")]
        public async Task<IActionResult> SetVisitDescription([FromQuery] int visitSlotId, [FromBody] string content) //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        {

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            var res = (await _scheduleManager.GetSchedulesAsync(x => x.Doctor.Id == user.Id && x.ScheduleEntries.Any(y => y.VisitSlots.Any(z => z.Id == visitSlotId))));
            if (res == null || res.ToList().Count == 0)
                return BadRequest("Either the visitSlotId is wrong or the visitSlot is not registered to you (as the doctor).");

            await _scheduleManager.EditVisitSlotNotes(visitSlotId, content);
            return Ok("Set description successfully!");
        }

        //[HttpGet]
        //[Authorize(Roles = UserRoles.patient)]
        //[Route("get-my-appointments-as-patient")]
        //public async Task<IActionResult> SetVisitDescription() //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        //{

        //    var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
        //    v
        //}

        ////[HttpPut]
        ////[Authorize(Roles = UserRoles.admin)]
        ////[Route("create-schedule-entry")]
        ////public async Task<IActionResult> UpdateScheduleEntry([FromQuery] int scheduleEntryId)
        ////{
        ////    ScheduleEntry scheduleDTO = (await _scheduleEntryManager.GetScheduleEntriesAsync(x => x.Id == scheduleEntryId)).First();
        ////    if (scheduleDTO==null)
        ////        return UnprocessableEntity(new Response { Status = "Error", Message = "Schedule with given ID does not exist." });

        ////    await _scheduleEntryManager.UpdateScheduleAsync(scheduleDTO);
        ////    return Ok("Schedule entry updated.");
        ////}

        ////[HttpGet]
        ////[Route("get-my-appointments")]
        ////public async Task<IActionResult> GetCurrentPatientVisitSchedule()
        ////{
        ////    //check if verified (role?)
        ////}
    }
}
