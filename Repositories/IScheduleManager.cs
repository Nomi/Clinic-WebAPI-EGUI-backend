using EGUI_Stage2.Models;
using System.Linq.Expressions;

namespace EGUI_Stage2.Repositories
{
    public interface IScheduleManager //: IDisposable
    {
        Task<IEnumerable<Schedule>> GetSchedulesAsync(Expression<Func<Schedule, bool>> predicate = null);
        Task<List<Schedule>> GetSchedulesFromWeekOffsetAsync(int weekOffset = 0, bool getConsequentWeeksAsWell = false);
        Task InsertScheduleAsync(Schedule schedule);
        Task<bool> DeleteScheduleAsync(string ScheduleId);
        Task UpdateScheduleTimingsAsync(Schedule modifiedSchedule);
        Task CopySchedule(User doc, int sourceWeekOffset, int targetWeekOffset);
        Task DeleteAllSchedulesForCurrentOrPreviousCalendarWeekAsync(int weekOffset = 0);
        Task EditVisitSlotNotes(int visitSlotId, string visitSlotText);
        Task SubscribeOrUnsubscribeFromVisitSlot(int visitSlotId, User patient);
        void SaveAsync();
    }
}
