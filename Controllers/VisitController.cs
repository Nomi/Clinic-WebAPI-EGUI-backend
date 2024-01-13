using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Database;
using EGUI_Stage2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
//using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EGUI_Stage2.Controllers
{
    [Authorize]
    public class VisitsController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public VisitsController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        [Route("get-doctor-schedule")]
        public async Task<IActionResult> GetDoctorSchedule([FromQuery] string docUsername)
        {
            var doc = await _userManager.FindByNameAsync(docUsername);
            if (doc == null || !(await _userManager.IsInRoleAsync(doc, "Doctor")))
                return BadRequest("Doctor not found");
            DateTime dateOfMondayOfCurrentWeek = DateTime.Today.Date.GetDateOfMondayOfWeek().Date;
            var schedule = await _context.Schedules.Include(x => x.Doctor)
                    .Include(x => x.ScheduleForEachDay).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .Where(x => x.Doctor.UserName == docUsername).ToListAsync();

            DoctorWrapper doctorData = new(doc);
            doctorData.SetAppointments(schedule);

            return Ok(doctorData);
        }

        [HttpPatch]
        [Authorize(Roles = "Patient")]
        [Route("subscribe-to-appointment")]
        public async Task<IActionResult> SubscribeToAppointment([FromQuery] int visitSlotId) //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        {

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            var result = await _context.Schedules.Include(x => x.Doctor)
                    .Include(x => x.ScheduleForEachDay).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .Where(x => x.ScheduleForEachDay.Any(y => y.VisitSlots.Any(z => z.Id == visitSlotId && (z.Patient == null || z.Patient.Id == user.Id)))).ToListAsync();
            if (result == null || result.ToList().Count == 0)
                return BadRequest("Visit does not exist or has another user signed up.");
            var visitSched = await _context.Visits.Where(v => v.Id == visitSlotId).SingleOrDefaultAsync();
            visitSched.Patient = user;
            _context.Entry(visitSched).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Subscribed.");
        }

        [HttpPatch]
        [Authorize(Roles = "Patient")]
        [Route("unsubscribe-from-appointment")]
        public async Task<IActionResult> UnsubscribeFromAppointment([FromQuery] int visitSlotId) //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        {

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            var result = await _context.Schedules.Include(x => x.Doctor)
                    .Include(x => x.ScheduleForEachDay).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .Where(x => x.ScheduleForEachDay.Any(y => y.VisitSlots.Any(z => z.Id == visitSlotId && (z.Patient == null || z.Patient.Id == user.Id)))).ToListAsync();
            if (result == null || result.ToList().Count == 0)
                return BadRequest("Visit does not exist or has another user signed up.");
            var visit = await _context.Visits.Where(v => v.Id == visitSlotId).SingleOrDefaultAsync();
            visit.Patient = null;
            _context.Entry(visit).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Unsubscribed.");
        }

        [HttpPut]
        [Authorize(Roles = "Doctor")]
        [Route("write-description")]
        public async Task<IActionResult> SetVisitDescription([FromQuery] int visitSlotId, [FromBody] string content) //doesn't prevent signups to past visitSlots, because that'd be too much scope (would need similar attention in other places).
        {

            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            var result = await _context.Schedules.Include(x => x.Doctor)
                    .Include(x => x.ScheduleForEachDay).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .Where(x => x.Doctor.Id == user.Id && x.ScheduleForEachDay.Any(y => y.VisitSlots.Any(z => z.Id == visitSlotId))).ToListAsync();
            if (result == null || result.ToList().Count == 0)
                return BadRequest("visitSlotId wrong OR you are not the doctor assigned to it.");

            var visit = await _context.Visits.Where(v => v.Id == visitSlotId).SingleOrDefaultAsync();
            visit.Description = content;
            _context.Entry(visit).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Written descriptiton successfully.");
        }
    }
}
