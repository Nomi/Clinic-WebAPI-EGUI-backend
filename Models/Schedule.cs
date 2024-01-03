using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EGUI_Stage2.Models
{
    public class Schedule
    {
        [Key]
        public string Id;

        public DateTime DateOfMonday { get; set; }

        public List<ScheduleEntry>? ScheduleEntries { get; set; }

        // Foreign key to link to the User (Doctor)
        public string DoctorId { get; set; }
        public virtual User Doctor { get; set; }
    }
}
