using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Schema;

namespace EGUI_Stage2.Models
{
    public class Visit
    {
        [Key]
        public int Id { get; set; }
        public AppUser? Patient { get; set; }
        public TimeOnly StartTime { get; set; }
        [NotMapped]
        public TimeOnly EndTime => StartTime.AddMinutes(15); //15 minutes by default
        public string? Description { get; set; }
        public ScheduleForDay ParentScheduleEntry { get; set; }
        public static List<Visit> CreateVisitSlotsForScheduleEntry(ref ScheduleForDay scheduleEntry)
        {
            var result = new List<Visit>();

            int totalTimeInMinutesRemaining = (int)(scheduleEntry.EndTime - scheduleEntry.StartTime).TotalMinutes;

            while(totalTimeInMinutesRemaining>=15)
            {
                totalTimeInMinutesRemaining -= 15;
                Visit vS = new Visit();
                vS.StartTime = scheduleEntry.StartTime.AddMinutes(result.Count * 15);
                vS.Description = string.Empty;
                vS.ParentScheduleEntry = scheduleEntry;
                result.Add(vS);
            }

            //scheduleEntry.VisitSlots = result;

            return result;

        }

    }
}

