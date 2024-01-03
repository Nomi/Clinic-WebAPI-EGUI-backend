using EGUI_Stage2.Models;

namespace EGUI_Stage2.DTOs
{
    public class GetScheduleDTO
    {
        public string ScheduleId { get; set; }
        public string DoctorName {get;set;}
        public string DoctorUsername {get;set;}
        public DoctorSpecialtyEnum Speciality {get;set;}
        public string? ScheduleEntryId { get; set; }
        public DayOfWeek? DayOfWeek => Date?.DayOfWeek ?? null;
        public DateOnly? Date  {get;set;}
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public GetScheduleDTO() { }
        public GetScheduleDTO(Schedule sched, int indexEntry = -1)
        {
            ScheduleId = sched.Id;
            DoctorName = sched.Doctor.Name;
            DoctorUsername = sched.Doctor.UserName;
            Speciality = sched.Doctor.Specialty_Doc;
            if(indexEntry!=-1)
            {
                var entry = sched.ScheduleEntries[indexEntry];
                ScheduleEntryId = entry.Id;
                Date = DateOnly.FromDateTime(entry.Date);
                StartTime = entry.StartTime;
                EndTime = entry.EndTime;
            }
            else
            {
                ScheduleEntryId = null;
                Date = null;
                StartTime = null;
                EndTime = null;
            }
        }
    }
}
