using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Data;
using EGUI_Stage2.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Xml.Schema;

namespace EGUI_Stage2.Repositories
{
    public class ScheduleManager : IScheduleManager
    {
        private AppDbContext _dbContext;

        public ScheduleManager(AppDbContext context)
        {
            _dbContext = context;
        }
        public async Task<bool> DeleteScheduleAsync(string scheduleEntryId)
        {
            ////enabled cascade delete so these should be unneeded:
            //await _dbContext.VisitSlots.Where(x => x.ParentScheduleEntry.Schedule.Id == scheduleEntryId).ExecuteDeleteAsync();
            //await _dbContext.ScheduleEntries.Where(x => x.Schedule.Id == scheduleEntryId).ExecuteDeleteAsync();
            int res = await _dbContext.Schedules.Where(s => s.Id == scheduleEntryId).ExecuteDeleteAsync();
            await _dbContext.SaveChangesAsync();
            return (0 < res);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesAsync(Expression<Func<Schedule, bool>> filter = null)
        {
            List<Schedule> result;
            if (filter == null)
                result = await _dbContext.Schedules.Include(x => x.Doctor) //.ThenInclude(x => x.UserName).Include(x => x.Doctor).ThenInclude(x => x.Name).Include(x => x.Doctor).ThenInclude(x => x.Specialty_Doc)
                    .Include(x => x.ScheduleEntries).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .ToListAsync();
            else
                result = await _dbContext.Schedules.Include(x => x.Doctor) //.ThenInclude(x => x.UserName).Include(x => x.Doctor).ThenInclude(x => x.Name).Include(x => x.Doctor).ThenInclude(x => x.Specialty_Doc)
                    .Include(x => x.ScheduleEntries).ThenInclude(x => x.VisitSlots).ThenInclude(x => x.Patient)
                    .Where(filter).ToListAsync();
            return result;
        }

        public async Task<List<Schedule>> GetSchedulesFromWeekOffsetAsync(int weekOffset = 0, bool getConsequentWeeksAsWell = false)
        {
            DateTime dateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(DateTime.Now.AddDays(7 * weekOffset));

            List<Schedule> res;
            if (getConsequentWeeksAsWell)
                res = (await GetSchedulesAsync(x => x.DateOfMonday.Date >= dateOfMonday.Date)).ToList();
            else
                res = (await GetSchedulesAsync(x => x.DateOfMonday.Date == dateOfMonday.Date)).ToList();
            return res;
        }

        public async Task CopySchedule(User doc, int sourceWeekOffset, int targetWeekOffset)
        {
            DateTime currentDateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(DateTime.Now).Date;
            DateTime sourceDateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(DateTime.Now.AddDays(7 * sourceWeekOffset)).Date;
            DateTime targetDateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(DateTime.Now.AddDays(7 * targetWeekOffset)).Date;
            TimeSpan delta = targetDateOfMonday.Date - sourceDateOfMonday.Date;
            var toCopy = (await GetSchedulesAsync(x => x.Doctor.Id == doc.Id && x.DateOfMonday.Date == sourceDateOfMonday.Date)).FirstOrDefault();
            //await _dbContext.ScheduleEntries.Where(x => x.Schedule.Doctor.Id == doc.Id && x.Schedule.DateOfMonday == targetDateOfMonday))
            await _dbContext.Schedules.Where(x => x.Doctor.Id == doc.Id && x.DateOfMonday.Date == targetDateOfMonday).ExecuteDeleteAsync();


            Schedule sched = new Schedule
            {
                DateOfMonday = targetDateOfMonday,
                Doctor = doc,
                DoctorId = doc.Id,
                ScheduleEntries = new(),
            };

            if (toCopy != null)
            {
                foreach (var schedEntry in toCopy.ScheduleEntries)
                {
                    ScheduleEntry newSE = new();
                    newSE.Date = schedEntry.Date.Date.Add(delta).Date;
                    //newSE.dayOfWeek = newSE.Date.DayOfWeek;
                    newSE.StartTime = schedEntry.StartTime;
                    newSE.EndTime = schedEntry.EndTime;
                    newSE.VisitSlots = VisitSlot.CreateVisitSlotsForScheduleEntry(ref newSE);
                    sched.ScheduleEntries.Add(newSE);
                }
            }
            await _dbContext.Schedules.AddAsync(sched);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAllSchedulesForCurrentOrPreviousCalendarWeekAsync(int weekOffset = 0)
        {
            DateTime dateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(DateTime.Now.AddDays(7 * weekOffset));
            await _dbContext.Schedules.Where(x => x.DateOfMonday == dateOfMonday).ExecuteDeleteAsync();
            await _dbContext.SaveChangesAsync();
        }

        public async Task InsertScheduleAsync(Schedule schedule)
        {
            for (int i = 0; i < schedule.ScheduleEntries.Count; i++)
            {
                var currEntry = schedule.ScheduleEntries[i];
                currEntry.VisitSlots = VisitSlot.CreateVisitSlotsForScheduleEntry(ref currEntry);
            }
            await _dbContext.Schedules.AddAsync(schedule);
            await _dbContext.SaveChangesAsync();
        }

        public async void SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateScheduleTimingsAsync(Schedule modifiedSchedule)
        {
            var existingSchedule = await _dbContext.Schedules
                                .Where(x => x.Id == modifiedSchedule.Id)
                                .Include(x => x.ScheduleEntries).ThenInclude(x => x.VisitSlots)
                                .SingleOrDefaultAsync();

            if (existingSchedule != null)
            {
                //// Update parent
                ////_dbContext.Entry(existingSchedule).CurrentValues.SetValues(modifiedSchedule);

                //// Delete schedule entry for any day that doesn't exist in the new model OR the start time or end time are different.
                foreach (var existingChild in existingSchedule.ScheduleEntries.ToList())
                {
                    if (modifiedSchedule.ScheduleEntries.Any(c => c.dayOfWeek != existingChild.dayOfWeek || (c.dayOfWeek == existingChild.dayOfWeek && (c.StartTime != existingChild.StartTime || c.EndTime != existingChild.EndTime))))
                        _dbContext.ScheduleEntries.Remove(existingChild);
                }


                //_dbContext.ScheduleEntries.Where(x => x.Schedule.Id == existingSchedule.Id).Remove.ExecuteDeleteAsync();
                // Insert new or updated children
                for (int i = 0; i < modifiedSchedule.ScheduleEntries.Count; i++)
                {
                    var schedEnt = modifiedSchedule.ScheduleEntries[i];
                    var existingChild = existingSchedule.ScheduleEntries
                                                .Where(c => c.dayOfWeek == schedEnt.dayOfWeek)
                                                .SingleOrDefault();
                    if (existingChild != null) { continue; } //day where timing hasn't changed.
                    schedEnt.Schedule = existingSchedule;
                    schedEnt.VisitSlots = VisitSlot.CreateVisitSlotsForScheduleEntry(ref schedEnt);
                    existingSchedule.ScheduleEntries.Add(schedEnt);
                }
                _dbContext.Entry(existingSchedule).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task SubscribeOrUnsubscribeFromVisitSlot(int visitSlotId, User patient)
        {
            var vs = await _dbContext.VisitSlots.Where(v => v.Id == visitSlotId).SingleOrDefaultAsync();
            if (vs == null)
                return;
            vs.Patient = patient;
            _dbContext.Entry(vs).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        public async Task EditVisitSlotNotes(int visitSlotId, string visitSlotText)
        {
            var vs = await _dbContext.VisitSlots.Where(v => v.Id == visitSlotId).SingleOrDefaultAsync();
            if (vs == null)
                return;
            vs.Description = visitSlotText;
            _dbContext.Entry(vs).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

        }
    }
}
