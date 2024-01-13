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
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        IConfiguration _configuration;
        public UsersController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [Route("signin")]
        public async Task<IActionResult> signin([FromBody] UserSignInStructure signInDetails)
        {
            var user = await _userManager.FindByNameAsync(signInDetails.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, signInDetails.Password) )
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = AuthService.GetJWTToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            return BadRequest("Invalid details");
        }


        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> PatientSignUp([FromBody] PatientSignUpStructure patientSignUpData)
        {
            var userExists = await _userManager.FindByNameAsync(patientSignUpData.Username);
            if (userExists != null)
                return Conflict();

            AppUser user = new()
            {
                Email = patientSignUpData.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = patientSignUpData.Username,
                Name = patientSignUpData.Name,
            };
            user.IsVerified = false;
            var result = await _userManager.CreateAsync(user, patientSignUpData.Password);
            _userManager.AddToRoleAsync(user, "Patient");

            if (!result.Succeeded)
                return BadRequest("Failed.");

            return Ok("Successfully created.");
        }

        [HttpGet]
        [Authorize]
        [Route("doctors")]
        public async Task<IActionResult> GetDoctors([FromQuery]DoctorSpecialtyEnum doctorSpecialty)
        {
            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            if (!user.IsVerified && this.User.IsInRole("Patient"))
                return Forbid("Account not verified");

            var doctors = (await _userManager.GetUsersInRoleAsync("Doctor")).Where(x=>x.Specialty_Doc==doctorSpecialty).ToList();
            
            return Ok(doctors.Select(x => new DoctorWrapper(x)).ToList());
        }
    }

    public class UserSignInStructure
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PatientSignUpStructure
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class DoctorWrapper
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DoctorSpecialtyEnum Speciality { get; set; }
        public List<Visit> Appointments { get; set; }

        public DoctorWrapper() { }
        public DoctorWrapper(AppUser docUsr)
        {
            Name = docUsr.Name;
            Username = docUsr.UserName;
            Email = docUsr.Email;
            Speciality = docUsr.Specialty_Doc;
        }
    }

}

