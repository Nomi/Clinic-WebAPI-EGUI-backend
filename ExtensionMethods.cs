using EGUI_Stage2.Controllers;
using EGUI_Stage2.Models;

namespace EGUI_Stage2.Auxiliary
{
    public static class ExtensionMethods
    {
        public static DateTime GetDateOfMondayOfWeek(this DateTime date)
        {
            int diff = DayOfWeek.Monday - date.DayOfWeek;
            return date.AddDays(diff).Date;
        }
        public static Schedule ToSchedule(this ScheduleCreationData data,AppUser doctor)
        {
            Schedule schedule = new();
            schedule.ScheduleForEachDay = new();
            for (int i = 0; i < data.TimingsOfEachDayOfWeek.Count; i++)
            {
                schedule.ScheduleForEachDay.Add(data.MapToADayEntry(i));
                schedule.ScheduleForEachDay[i].ScheduleCurrent = schedule;
            }
            schedule.DoctorId = doctor.Id;
            schedule.Doctor = doctor;
            schedule.DateOfMonday = data.DateOfCurrentWeekMonday.ToDateTime(new TimeOnly(0,0)).Date.GetDateOfMondayOfWeek();
            return schedule;
        }
        public static void SetAppointments(this DoctorWrapper docW, IEnumerable<Schedule> scheduleList)
        {
            docW.Appointments = new List<Visit>();
            foreach (Schedule schedule in scheduleList)
            {
                if (schedule.ScheduleForEachDay == null) continue;
                foreach (var entry in schedule.ScheduleForEachDay.Where(x => x.Date > DateTime.Now.Date))
                {
                    if (entry.VisitSlots == null)
                        continue;
                    docW.Appointments.AddRange(entry.VisitSlots);
                }
            }

            foreach (var appointment in docW.Appointments)
            {
                appointment.ParentScheduleEntry.ScheduleCurrent = null;
                appointment.ParentScheduleEntry.VisitSlots = null;
                if (appointment.Patient != null)
                {
                    appointment.Patient.PasswordHash = null;
                    appointment.Patient.DoctorSchedule = null;
                    appointment.Patient.Email = null;
                }
            }
        }
    }
}
