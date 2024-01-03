using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Models;

namespace EGUI_Stage2.DTOs
{
    public class UpdateScheduleEntryDTO
    {
        public string ScheduleId { get; set; }
        public List<ScheduleEntrySimplifiedDTO> Entries { get; set; }
        public Schedule ToSchedule(User doctor)
        {
            Schedule s = new();
            s.Id = ScheduleId;
            s.ScheduleEntries = new();
            s.Doctor = doctor;
            s.DoctorId = doctor.Id;
            for (int i = 0; i < Entries.Count; i++)
            {
                s.ScheduleEntries.Add(GetEntryAsScheduleEntry(i));
                s.ScheduleEntries[i].Schedule = s;
            }
            return s;
        }
        private ScheduleEntry GetEntryAsScheduleEntry(int indexOfEntry)
        {
            ScheduleEntry s = new ScheduleEntry();
            s.Date = this.Entries[indexOfEntry].date.ToDateTime(TimeOnly.MinValue).Date;
            s.dayOfWeek = s.Date.DayOfWeek;
            s.StartTime = TimeOnly.FromTimeSpan(Entries[indexOfEntry].startTimeOnly.TimeOfDay);
            s.EndTime = TimeOnly.FromTimeSpan(Entries[indexOfEntry].endTimeOnly.TimeOfDay);
            return s;
        }
    }
}
