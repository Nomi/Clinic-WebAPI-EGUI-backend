
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;


namespace EGUI_Stage2.Models
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }
        [NotMapped]
        public bool IsVerified { get => this.EmailConfirmed; set => this.EmailConfirmed = value; }
        public DoctorSpecialtyEnum Specialty_Doc { get; set; }

        public virtual ICollection<Schedule>? DoctorSchedule { get; set; }

        // public virtual ICollection<Appointment> Appointments { get; set; }
    }

    public enum DoctorSpecialtyEnum : int
    {
        [Display(Name = "None")]
        None = 0,
        [Display(Name = "Home")]
        Home,
        [Display(Name = "ENT")]
        ENT,
        [Display(Name = "Dermatologist")]
        Dermatologist,
        [Display(Name = "Ophtalmologist")]
        Ophtalmologist,
        [Display(Name = "Neurologist")]
        Neurologist,
        [Display(Name = "Orthopedist")]
        Orthopedist,
        [Display(Name = "Pediatrician")]
        Pediatrician
    }

}
