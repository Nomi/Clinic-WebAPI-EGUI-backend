using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Database;
using EGUI_Stage2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace EGUI_Stage2.Controllers
{
    public class AdminActionsController : ControllerBase
    {
        UserManager<AppUser> _userManager;
        public AdminActionsController(UserManager<AppUser> userManager) { _userManager = userManager; }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var allPatients = await _userManager.GetUsersInRoleAsync("Patient");

            List<PatientWrapperWithoutVisits> result = allPatients.Select(x => new PatientWrapperWithoutVisits { Email = x.Email, IsVerified = x.IsVerified, Name = x.Name, Username = x.UserName }).ToList();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("create-doctor")]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorSignUpStructure docData)
        {
            var userExists = await _userManager.FindByNameAsync(docData.Username);
            if (userExists != null)
                return Conflict("Username taken.");

            AppUser user = new()
            {
                Email = docData.Email,
                UserName = docData.Username,
                Name = docData.Name,
                Specialty_Doc = docData.DoctorSpecialty,
                SecurityStamp = Guid.NewGuid().ToString(),
                IsVerified = true,
            };
            var result = await _userManager.CreateAsync(user, docData.Password);
            _userManager.AddToRoleAsync(user, "Doctor");
            if (!result.Succeeded)
                return BadRequest();

            return Ok("Success");
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("verify-patient")]
        public async Task<IActionResult> VerifyPatient([FromQuery] string patientUsername)
        {
            var user = await _userManager.FindByNameAsync(patientUsername);
            if (user == null || user.IsVerified)
                return NotFound();
            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok("Patient verified successfully.");
        }
    }

    public class PatientWrapperWithoutVisits
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public List<Visit>? VisitsBooked { get; set; }
    }

    public class PatientWrapper
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public List<Visit>? VisitsBooked { get; set; }

        public static PatientWrapper GetFromUser(AppUser x)
        {
            return new PatientWrapper
            {
                Email = x.Email,
                IsVerified = x.IsVerified,
                Name = x.Name,
                Username = x.UserName
            };

        }
    }

    public class DoctorSignUpStructure
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DoctorSpecialtyEnum DoctorSpecialty { get; set; }
    }
}
