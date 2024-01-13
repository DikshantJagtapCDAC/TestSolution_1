using AutoMapper;
using CompanyEmployees.Entities.DataTransferObjects;
using CompanyEmployees.Entities.Models;
using CompanyEmployees.JwtFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace CompanyEmployees.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly JwtHandler _jwtHandler;

        public AccountsController(UserManager<User> userManager, IMapper mapper, JwtHandler jwtHandler)
        {
            _userManager = userManager;
            _mapper = mapper;
            _jwtHandler = jwtHandler;
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration == null || !ModelState.IsValid)
                return BadRequest();

            //// Convert the Base64 string to a byte array
            //byte[] photoBytes = Convert.FromBase64String(userForRegistration.PhotoUrl);

            var user = _mapper.Map<User>(userForRegistration);
            var result = await _userManager.CreateAsync(user, userForRegistration.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new RegistrationResponseDto { Errors = errors });
            }

            await _userManager.AddToRoleAsync(user, "Viewer");

            return StatusCode(201);
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserForAuthenticationDto userForAuthentication)
        {
            var user = await _userManager.FindByNameAsync(userForAuthentication.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, userForAuthentication.Password))
                return Unauthorized(new AuthResponseDto { ErrorMessage = "Invalid Authentication" });

            var signingCredentials = _jwtHandler.GetSigningCredentials();
            var claims = await _jwtHandler.GetClaims(user);
            var tokenOptions = _jwtHandler.GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = token });
        }


        [HttpGet("profile")]
        [Authorize(Roles = "Viewer")]
        public async Task<IActionResult> GetUserProfile()
        {
            // Retrieve the current user from the User Claims
            var userName = User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var userProfile = _mapper.Map<UserForRegistrationDto>(user);
            userProfile.PhotoUrl = user.PhotoUrl;

            return Ok(userProfile);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Administrator")] // Only Admin can access all user details
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users;
            var userProfiles = _mapper.Map<IEnumerable<UserResponseDto>>(users);
            return Ok(userProfiles);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")] // Only Admin can delete user
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")] // Only Admin can edit user details
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserForUpdate userDto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            userDto.password = user.PasswordHash;
            userDto.conformPassword = user.PasswordHash;

            _mapper.Map(userDto, user);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")] // Only Admin can get user by ID
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var userProfile = _mapper.Map<UserResponseDto>(user);
            return Ok(userProfile);
        }

        [HttpGet("Privacy")]
        [Authorize(Roles = "Administrator")]
        public IActionResult Privacy()
        {
            var claims = User.Claims
                .Select(c => new { c.Type, c.Value })
                .ToList();

            return Ok(claims);
        }

        [HttpPut("update-workdays/{userId}")]
        [Authorize(Roles = "Administrator")] // Only Admin can update workdays
        public async Task<IActionResult> UpdateWorkdays(string userId, [FromBody] WorkdaysDto workdaysDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Update the workdays information
            user.Workdays = workdaysDto.Workdays;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return NoContent();
        }

        [HttpGet("calculate-salary/{userId}")]
        [Authorize(Roles = "Administrator")] // Only Admin can calculate salary
        public IActionResult CalculateSalary(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            decimal salary = CalculateSalary(user.Workdays); // Implement your salary calculation logic

            return Ok(new { UserId = user.Id, UserName = user.UserName, Salary = salary });
        }

        private decimal CalculateSalary(int workdays)
        {
            // Implement your salary calculation logic based on workdays
            // This can include a fixed daily wage or any other business rules
            decimal dailyWage = 100; // Set your daily wage
            decimal salary = workdays * dailyWage;

            return salary;
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input" });
            }

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // You can send the token to the frontend or include it in the response
            return Ok(new { Message = "Password reset token generated", ResetToken = token });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input" });
            }

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            // Generate a new JWT token and return it to the client
            var signingCredentials = _jwtHandler.GetSigningCredentials();
            var claims = await _jwtHandler.GetClaims(user);
            var tokenOptions = _jwtHandler.GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Ok(new { Message = "Password reset successful", Token = token });
        }




    }
}
