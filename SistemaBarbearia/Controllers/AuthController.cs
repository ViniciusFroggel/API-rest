using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaBarbearia.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // -------------------- REGISTRO --------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Limpar telefone para manter só números
            model.Phone = new string(model.Phone.Where(char.IsDigit).ToArray());

            var userExists = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Phone);
            if (userExists != null)
                return BadRequest("Esse número já está registrado.");

            var user = new ApplicationUser
            {
                UserName = model.Phone,
                PhoneNumber = model.Phone,
                NomeCompleto = model.NomeCompleto,
                TipoUsuario = "Cliente",
                Nivel = 0,
                PhoneNumberConfirmed = false // precisará validar sms
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "Cliente");

            return Ok("Usuário cadastrado com sucesso. Valide seu telefone para ativar a conta.");
        }

        // -------------------- LOGIN --------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            model.Phone = new string(model.Phone.Where(char.IsDigit).ToArray());

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Phone);
            if (user == null)
                return Unauthorized("Telefone não encontrado.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Senha incorreta.");

            var roles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("phone", user.PhoneNumber),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
    }

    // -------------------- MODELOS --------------------
    public class RegisterModel
    {
        public string NomeCompleto { get; set; }
        public string Phone { get; set; }  // Telefone no lugar do Email
        public string Password { get; set; }
    }

    public class LoginModel
    {
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
