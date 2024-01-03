using EGUI_Stage2.Models;
using System.ComponentModel.DataAnnotations;

namespace EGUI_Stage2.DTOs
{
    public class DoctorRegistrationDTO : PatientRegistrationDTO
    {
        [Required(ErrorMessage = "Speciality is required (use \"None\" for none)")]
        public DoctorSpecialtyEnum DoctorSpecialty { get; set; }
    }
}
