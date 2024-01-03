using System.ComponentModel.DataAnnotations;

namespace EGUI_Stage2.Models
{
    public class ScheduleEntry
    {
        [Key]
        public string Id;
        public DateTime Date { get; set; }

        public DayOfWeek dayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public IEnumerable<VisitSlot>? VisitSlots { get; set; }

        public Schedule Schedule { get; set; }
    }

}