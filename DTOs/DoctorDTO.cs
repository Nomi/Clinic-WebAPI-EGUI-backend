using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Models;
using System.Globalization;

namespace EGUI_Stage2.DTOs
{
    public class DoctorDTO
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DoctorSpecialtyEnum Speciality { get; set; }
        public List<VisitSlot> AppointmentSlots { get; set; }

        public DoctorDTO() { }
        public DoctorDTO(User doc)
        {
            Name = doc.Name;
            Username = doc.UserName;
            Email = doc.Email;
            Speciality = doc.Specialty_Doc;
            //Schedule = doc?.DoctorSchedule?.Where(x => x.DateOfMonday.Date >= CalendarHelper.GetDateOfMondayOfWeek(DateTime.Today).Date).ToList();
        }
        public void SetAndFilterOutPastAppointments(IEnumerable<Schedule> schedules, User currentUser)
        {
            AppointmentSlots = new List<VisitSlot>();
            foreach(Schedule s in schedules)
            {
                if (s.ScheduleEntries == null) continue;
                foreach(var entry in s.ScheduleEntries.Where(x=>x.Date>DateTime.Now))
                {
                    AppointmentSlots.AddRange(entry.VisitSlots.Where(x => x.StartTime > TimeOnly.FromDateTime(DateTime.Now) && (x.Patient == null || x?.Patient.Id == currentUser.Id)));
                }
            }

            foreach(var appointment in AppointmentSlots)
            {
                appointment.ParentScheduleEntry.Schedule = null;
                appointment.ParentScheduleEntry.VisitSlots = null;
                if(appointment.Patient!= null)
                {
                    appointment.Patient.PasswordHash = null;
                    appointment.Patient.DoctorSchedule = null;
                    appointment.Patient.Email = null;
                }
            }
        }
    }
}
