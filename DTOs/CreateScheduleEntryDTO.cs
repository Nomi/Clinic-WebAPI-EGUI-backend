using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Models;
using System.Text.Json.Serialization;

namespace EGUI_Stage2.DTOs
{
    public class CreateScheduleDTO
    {
        public string DoctorUsername { get; set; }
        public DateOnly DateOfMonday { get; set; }
        public List<ScheduleEntrySimplifiedDTO> Entries { get; set; }
        public Schedule ToSchedule(User doctor)
        {
            Schedule s = new();
            s.DoctorId = doctor.Id;
            s.Doctor = doctor;
            s.DateOfMonday = CalendarHelper.GetDateOfMondayOfWeek(DateOfMonday.ToDateTime(TimeOnly.MinValue).Date);
            s.ScheduleEntries = new();
            for(int i=0;i<Entries.Count;i++)
            {
                s.ScheduleEntries.Add(GetEntryAsScheduleEntry(i));
                s.ScheduleEntries[i].Schedule = s;
            }
            return s;
        }
        private ScheduleEntry GetEntryAsScheduleEntry(int indexOfEntry)
        {
            ScheduleEntry s = new ScheduleEntry();
            //s.dayOfWeek = Entries[indexOfEntry].dayOfWeek;
            s.Date = Entries[indexOfEntry].date.ToDateTime(TimeOnly.MinValue).Date;
            s.dayOfWeek = s.Date.DayOfWeek;
            s.StartTime = Entries[indexOfEntry].startTimeOnly;
            s.EndTime = Entries[indexOfEntry].endTimeOnly;
            return s;
        }
    }

    public class ScheduleEntrySimplifiedDTO
    {
        public DateOnly date { get; set; }
        [JsonIgnore]
        public DayOfWeek dayOfWeek => date.DayOfWeek;
        public TimeOnly startTimeOnly { get; set; }
        public TimeOnly endTimeOnly { get; set; }
    }
}
