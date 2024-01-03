using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Data;
using EGUI_Stage2.DTOs;
using EGUI_Stage2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EGUI_Stage2.Controllers
{
    //[Authorize]
    public class UsersController : ControllerBase
    {
        //Maybe the token stuff would've been done better using a TokenService class.
        private readonly UserManager<User> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly AppDbContext _appDbContext;
        IConfiguration _configuration;
        public UsersController(
            UserManager<User> userManager,
            //RoleManager<IdentityRole> roleManager,
            //AppDbContext appDbContext,
            IConfiguration configuration)
        {
            _userManager = userManager;
            //_roleManager = roleManager;
            //_appDbContext = appDbContext;
            _configuration = configuration;
        }

        //[AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserAuthenticateDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && user.IsVerified && await _userManager.CheckPasswordAsync(user, model.Password) )
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

                var token = AuthHelper.GetToken(authClaims,_configuration);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized("Either you have enetered invalid parameters OR your Patient account has not been verified from our side yet.");
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRegistrationDTO userData)
        {
            var userExists = await _userManager.FindByNameAsync(userData.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Username already exists!" });

            User user = new()
            {
                Email = userData.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = userData.Username,
                Name = userData.Name,
            };
            user.IsVerified = false;
            var result = await _userManager.CreateAsync(user, userData.Password);
            
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "Patient created successfully!" });
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.admin)]
        [Route("register-doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] DoctorRegistrationDTO userData)
        {
            var userExists = await _userManager.FindByNameAsync(userData.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Username already exists!" });

            User user = new()
            {
                Email = userData.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = userData.Username,
                Name = userData.Name,
                Specialty_Doc = userData.DoctorSpecialty
            };
            user.IsVerified = true;
            var result = await _userManager.CreateAsync(user, userData.Password);
            _userManager.AddToRoleAsync(user, UserRoles.doctor);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Doctor creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "Doctor created successfully!" });
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.admin)]
        [Route("verify-patient")]
        public async Task<IActionResult> VerifyPatient([FromQuery] string patientUsername)
        {
            var user = await _userManager.FindByNameAsync(patientUsername);
            if (user == null || user.IsVerified)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "An unverified user with the given username does not exist." });
            
            await _userManager.AddToRoleAsync(user, UserRoles.patient);
            await _userManager.RemoveFromRoleAsync(user, UserRoles.unverifiedPatient);

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(new Response { Status = "Success", Message = "Patient verified successfully." });
        }


        [HttpGet]
        [Authorize]
        [Route("list-of-doctors")]
        public async Task<IActionResult> ListOfDoctors()
        {
            if (this.User.IsInRole(UserRoles.unverifiedPatient))
                return Forbid("Your Patient account has not been verified yet (from our side).");

            var res = await _userManager.GetUsersInRoleAsync(UserRoles.doctor);

            List<DoctorDTO> result = res.Select(x => new DoctorDTO(x)).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [Route("get-doctors-by-speciality")]
        public async Task<IActionResult> ListOfDoctors([FromQuery]DoctorSpecialtyEnum doctorSpecialty)
        {
            if (this.User.IsInRole(UserRoles.unverifiedPatient))
                return Forbid("Your Patient account has not been verified yet (from our side).");


            var res = (await _userManager.GetUsersInRoleAsync(UserRoles.doctor)).Where(x=>x.Specialty_Doc==doctorSpecialty).ToList();

            List<DoctorDTO> result = res.Select(x => new DoctorDTO(x)).ToList();

            return Ok(result);
        }

        //[HttpGet]
        //[Route("test-get-visit-slots-for-hardcoded-doctor")]
        //public async Task<IActionResult> TestGetVisitSlotsForHardcodedDoctor()
        //{

        //    var res = await _appDbContext.VisitSlots.Include(x => x.ParentSchedule.Doctor).Where(x => x.ParentSchedule.Doctor.UserName == "doctor").ToListAsync();

        //    return Ok(res);
        //}
    }


}

