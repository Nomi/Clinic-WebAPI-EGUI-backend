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

        [NotMapped] 
        public DateTime DateOfSunday => DateOfMonday.AddDays(7).Date.AddSeconds(-1); //gives the DateTime with value 11:59pm of the sunday.

        public List<ScheduleForDay>? ScheduleForEachDay { get; set; }

        // Foreign key to link to the User (Doctor)
        public string DoctorId { get; set; }
        public virtual AppUser Doctor { get; set; }
    }
}
