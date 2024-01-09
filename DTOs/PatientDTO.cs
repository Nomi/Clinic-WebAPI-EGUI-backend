using EGUI_Stage2.Models;

namespace EGUI_Stage2.DTOs
{
    public class PatientDTO
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public List<VisitSlot>? Appointments { get; set; }

        public PatientDTO() { }
        public PatientDTO(User patient)
        {
            Username = patient.UserName;
            Name = patient.Name;
            Email = patient.Email;
            IsVerified = patient.IsVerified;
            Appointments = null;
        }
    }
}
