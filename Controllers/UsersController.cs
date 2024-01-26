using EGUI_Stage2.Auxiliary;
using EGUI_Stage2.Data;
using EGUI_Stage2.DTOs;
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
    //[Authorize]
    //[Route("api/[controller]")]
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
                    role = userRoles.FirstOrDefault("patient"),
                    expiration = token.ValidTo
                }); ;
            }
            return Unauthorized("Either you have enetered invalid parameters OR your Patient account has not been verified from our side yet.");
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRegistrationDTO userData)
        {
            var userExists = await _userManager.FindByNameAsync(userData.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status409Conflict, new Response { Status = "Error", Message = "Username already exists!" });

            User user = new()
            {
                Email = userData.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = userData.Username,
                Name = userData.Name,
            };
            user.IsVerified = false;
            var result = await _userManager.CreateAsync(user, userData.Password);
            _userManager.AddToRoleAsync(user, UserRoles.doctor);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "Patient created successfully!" });
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.admin)]
        [Route("register-doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] DoctorRegistrationDTO userData)
        {
            var userExists = await _userManager.FindByNameAsync(userData.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status409Conflict, new Response { Status = "Error", Message = "Username already exists!" });

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
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Doctor creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "Doctor created successfully!" });
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.admin)]
        [Route("verify-patient")]
        public async Task<IActionResult> VerifyPatient([FromQuery] string patientUsername)
        {
            var user = await _userManager.FindByNameAsync(patientUsername);
            if (user == null || user.IsVerified)
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Error", Message = "An unverified user with the given username does not exist." });
            
            await _userManager.AddToRoleAsync(user, UserRoles.patient);
            //await _userManager.RemoveFromRoleAsync(user, UserRoles.unverifiedPatient);

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(new Response { Status = "Success", Message = "Patient verified successfully." });
        }


        [HttpGet]
        [Authorize]
        [Route("list-of-doctors")]
        public async Task<IActionResult> ListOfDoctors()
        {
            //if (this.User.IsInRole(UserRoles.unverifiedPatient)) //can replace using technique to get current user (used elsewhere) 
            //    return Forbid("Your Patient account has not been verified yet (from our side).");
            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            if (!user.IsVerified && this.User.IsInRole(UserRoles.patient))
                return StatusCode((int)HttpStatusCode.Forbidden, new Response { Status = "Error", Message = "Your Patient account is not verified." });


            var res = await _userManager.GetUsersInRoleAsync(UserRoles.doctor);

            List<DoctorDTO> result = res.Select(x => new DoctorDTO(x)).ToList();

            return Ok(result);
        }
        [HttpGet]
        [Authorize(Roles = UserRoles.admin)]
        [Route("list-of-patients")]
        public async Task<IActionResult> ListOfPatients([FromQuery]bool onlyGetUnverified=false)
        {
            //if (this.User.IsInRole(UserRoles.unverifiedPatient)) //can replace using technique to get current user (used elsewhere) 
            //    return Forbid("Your Patient account has not been verified yet (from our side).");
            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            if (!user.IsVerified&&this.User.IsInRole(UserRoles.patient))
                return StatusCode((int)HttpStatusCode.Forbidden, new Response { Status = "Error", Message = "Your Patient account is not verified." });

            var res = await _userManager.GetUsersInRoleAsync(UserRoles.patient);

            List<PatientDTO> result;
            if (onlyGetUnverified)
                result = res.Where(x => !x.IsVerified).Select(x => new PatientDTO(x)).ToList();
            else
                result = res.Select(x => new PatientDTO(x)).ToList();

            return Ok(result);
        }
        [HttpGet]
        [Authorize]
        [Route("get-doctors-by-speciality")]
        public async Task<IActionResult> ListOfDoctorsBySpeciality([FromQuery]DoctorSpecialtyEnum doctorSpecialty) //can replace with other trickt to get current user and read isVerified bool
        {
            //if (this.User.IsInRole(UserRoles.unverifiedPatient)) //can use another trick to get current user and check IsVerified boolean
            //    return Forbid("Your Patient account has not been verified yet (from our side).");
            var user = await _userManager.FindByNameAsync(this.User.Identity.Name);
            if (!user.IsVerified && this.User.IsInRole(UserRoles.patient))
                return StatusCode((int)HttpStatusCode.Forbidden, new Response { Status = "Error", Message = "Your Patient account is not verified." });

            var res = (await _userManager.GetUsersInRoleAsync(UserRoles.doctor)).Where(x=>x.Specialty_Doc==doctorSpecialty).ToList();

            List<DoctorDTO> result = res.Select(x => new DoctorDTO(x)).ToList();

            return Ok(result);
        }

        //[HttpGet]`
        //[Route("test-get-visit-slots-for-hardcoded-doctor")]
        //public async Task<IActionResult> TestGetVisitSlotsForHardcodedDoctor()
        //{

        //    var res = await _appDbContext.VisitSlots.Include(x => x.ParentSchedule.Doctor).Where(x => x.ParentSchedule.Doctor.UserName == "doctor").ToListAsync();

        //    return Ok(res);
        //}
    }


}

