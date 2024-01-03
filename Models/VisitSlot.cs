using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Schema;

namespace EGUI_Stage2.Models
{
    public class VisitSlot
    {
        [Key]
        public int Id { get; set; }
        public User? Patient { get; set; }
        public TimeOnly StartTime { get; set; }
        [NotMapped]
        public TimeOnly EndTime => StartTime.AddMinutes(15); //15 minutes by default
        public string? Description { get; set; }
        public ScheduleEntry ParentScheduleEntry { get; set; }
        public static List<VisitSlot> CreateVisitSlotsForScheduleEntry(ref ScheduleEntry scheduleEntry)
        {
            var result = new List<VisitSlot>();

            int totalTimeInMinutesRemaining = (int)(scheduleEntry.EndTime - scheduleEntry.StartTime).TotalMinutes;

            while(totalTimeInMinutesRemaining>=ConfigHelper.VisitLengthMinutes)
            {
                totalTimeInMinutesRemaining -= ConfigHelper.VisitLengthMinutes;
                VisitSlot vS = new VisitSlot();
                vS.StartTime = scheduleEntry.StartTime.AddMinutes(result.Count * ConfigHelper.VisitLengthMinutes);
                vS.Description = string.Empty;
                vS.ParentScheduleEntry = scheduleEntry;
                result.Add(vS);
            }

            //scheduleEntry.VisitSlots = result;

            return result;

        }

    }
}

