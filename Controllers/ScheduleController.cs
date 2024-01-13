using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Database;
using EGUI_Stage2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace EGUI_Stage2.Controllers
{
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ScheduleController(UserManager<AppUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("create-schedule")]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleCreationData scheduleCreationData)
        {
            if (scheduleCreationData.DateOfCurrentWeekMonday.DayOfWeek != DayOfWeek.Monday)
                return BadRequest();
            if (scheduleCreationData.TimingsOfEachDayOfWeek.GroupBy(x => x.dayOfWeek).Any(g => g.Count() > 1))
                return Conflict();

            var doc = await _userManager.FindByNameAsync(scheduleCreationData.DocUsername);
            if (doc == null || !(await _userManager.IsInRoleAsync(doc,"Doctor")))
                return BadRequest();

            DateTime dateOfMonday = scheduleCreationData.DateOfCurrentWeekMonday.ToDateTime(new TimeOnly(0, 0));
            var temp = await _context.Schedules.Include(x => x.Doctor) 
                    .Include(x => x.ScheduleForEachDay).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .Where(x => x.Doctor.UserName == scheduleCreationData.DocUsername).ToListAsync();

            var existingScheduleList = await _context.Schedules.Include(x => x.Doctor)
                    .Include(x => x.ScheduleForEachDay).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .Where(x => x.DateOfMonday.Date == dateOfMonday && x.Doctor.UserName == scheduleCreationData.DocUsername).ToListAsync();

            if (existingScheduleList == null || existingScheduleList.Count > 0)
                return Conflict();

            var newSchedule = scheduleCreationData.ToSchedule(doc);

            for (int i = 0; i < newSchedule.ScheduleForEachDay.Count; i++)
            {
                var currEntry = newSchedule.ScheduleForEachDay[i];
                currEntry.VisitSlots = Visit.CreateVisitSlotsForScheduleEntry(ref currEntry);
            }
            await _context.Schedules.AddAsync(newSchedule);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("copy-lastweek-schedule-to-currentweek")]
        public async Task<IActionResult> CopyLastWeekScheduleToCurrentWeek([FromQuery]string doctorUsername)
        {
            var doc = await _userManager.FindByNameAsync(doctorUsername);
            if (doc == null || !(await _userManager.IsInRoleAsync(doc, "Doctor")))
                return UnprocessableEntity("Doctor not found.");
            DateTime currentDateOfMonday = DateTime.Now.GetDateOfMondayOfWeek().Date;
            DateTime lastDateOfMonday = DateTime.Now.AddDays(-7).GetDateOfMondayOfWeek().Date;
            TimeSpan diff = lastDateOfMonday.Date - currentDateOfMonday.Date;
            var toClone = _context.Schedules
                        .Include(x => x.Doctor)
                        .Include(x => x.ScheduleForEachDay)
                            .ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                        .Where(x => x.Doctor.Id == doc.Id && x.DateOfMonday.Date == currentDateOfMonday.Date).SingleOrDefault();
            await _context.Schedules.Where(x => x.Doctor.Id == doc.Id && x.DateOfMonday.Date == lastDateOfMonday).ExecuteDeleteAsync();
            Schedule newSchedule = new Schedule
            {
                DateOfMonday = currentDateOfMonday,
                Doctor = doc,
                DoctorId = doc.Id,
                ScheduleForEachDay = new(),
            };

            if (toClone != null)
            {
                foreach (var schedEntry in toClone.ScheduleForEachDay)
                {
                    ScheduleForDay newSE = new();
                    newSE.Date = schedEntry.Date.Date.Add(diff).Date;
                    newSE.StartTime = schedEntry.StartTime;
                    newSE.EndTime = schedEntry.EndTime;
                    newSE.VisitSlots = Visit.CreateVisitSlotsForScheduleEntry(ref newSE);
                    newSchedule.ScheduleForEachDay.Add(newSE);
                }
            }
            await _context.Schedules.AddAsync(newSchedule);
            await _context.SaveChangesAsync();
            return Ok("Schedule cloned.");
        }
    }


    public class ScheduleCreationData
    {
        public string DocUsername { get; set; }
        public DateOnly DateOfCurrentWeekMonday { get; set; }
        public List<BasicScheduleDayEntry> TimingsOfEachDayOfWeek { get; set; }
        public ScheduleForDay MapToADayEntry(int index)
        {
            ScheduleForDay scheduleEntry = new ScheduleForDay();
            scheduleEntry.Date = TimingsOfEachDayOfWeek[index].date.ToDateTime(TimeOnly.MinValue).Date;
            scheduleEntry.dayOfWeek = scheduleEntry.Date.DayOfWeek;
            scheduleEntry.StartTime = TimeOnly.FromTimeSpan(TimingsOfEachDayOfWeek[index].startTimeOnly.TimeOfDay);
            scheduleEntry.EndTime = TimeOnly.FromTimeSpan(TimingsOfEachDayOfWeek[index].endTimeOnly.TimeOfDay);
            return scheduleEntry;
        }
    }

    public class BasicScheduleDayEntry
    {
        public DateOnly date { get; set; }
        [JsonIgnore]
        public DayOfWeek dayOfWeek => date.DayOfWeek;
        public DateTime startTimeOnly { get; set; }
        public DateTime endTimeOnly { get; set; }
    }

    public class GetScheduleWrapper
    {
        public string ScheduleId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorUsername { get; set; }
        public DoctorSpecialtyEnum Speciality { get; set; }
        public string? ScheduleEntryId { get; set; }
        public DayOfWeek? DayOfWeek => Date?.DayOfWeek ?? null;
        public DateOnly? Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public GetScheduleWrapper() { }
        public GetScheduleWrapper(Schedule sched, int indexEntry = -1)
        {
            ScheduleId = sched.Id;
            DoctorName = sched.Doctor.Name;
            DoctorUsername = sched.Doctor.UserName;
            Speciality = sched.Doctor.Specialty_Doc;
            
            if (indexEntry != -1)
            {
                var entry = sched.ScheduleForEachDay[indexEntry];
                ScheduleEntryId = entry.Id;
                Date = DateOnly.FromDateTime(entry.Date);
                StartTime = entry.StartTime;
                EndTime = entry.EndTime;
                return;
            }
            
            ScheduleEntryId = null;
            Date = null;
            StartTime = null;
            EndTime = null;

        }
    }
}
