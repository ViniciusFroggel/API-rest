using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaBarbearia.Enums;
using SistemaBarbearia.Models;

namespace SistemaBarbearia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,AdminMaster")]
    public class UserAdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserAdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        //criar usuario admin/func
        [HttpPost("criar-usuario")]
        public async Task<IActionResult> CriarUsuario([FromBody] CreateUserModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Impedir criação de AdminMaster via UserAdmin
            if (model.TipoUsuario == "AdminMaster")
                return BadRequest("Não é permitido criar outro AdminMaster");

            // Apenas AdminMaster pode criar Admin; Admin comum não pode criar outro Admin
            if (model.TipoUsuario == "Admin" && currentUser.Nivel != UserLevels.AdminMaster)
                return Forbid("Apenas AdminMaster pode criar Admin");

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest("Usuário já existe");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                NomeCompleto = model.NomeCompleto,
                TipoUsuario = model.TipoUsuario,
                Nivel = model.TipoUsuario switch
                {
                    "Admin" => UserLevels.Admin,
                    "Funcionario" => UserLevels.Funcionario,
                    _ => UserLevels.Cliente
                }
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, model.TipoUsuario);

            return Ok($"Usuário {model.Email} criado com sucesso como {model.TipoUsuario}");
        }


        //alterar nivel de usuario
        [HttpPut("alterar-perfil")]
        public async Task<IActionResult> AlterarPerfil([FromQuery] string email, [FromQuery] string novoPerfil)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized("Usuário atual não encontrado.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("Usuário não encontrado.");

            // Bloqueia alteração para AdminMaster
            if (novoPerfil.Equals("AdminMaster", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Não é permitido alterar o perfil para AdminMaster.");

            // Impede mexer com AdminMaster existente
            if (user.Nivel == UserLevels.AdminMaster)
                return BadRequest("Não é permitido alterar perfil do AdminMaster existente.");

            // Apenas AdminMaster pode promover alguém a Admin
            if (novoPerfil.Equals("Admin", StringComparison.OrdinalIgnoreCase) && currentUser.Nivel != UserLevels.AdminMaster)
                return Forbid("Apenas AdminMaster pode promover alguém a Admin.");

            // Admin não promove outro Admin
            if (novoPerfil.Equals("Admin", StringComparison.OrdinalIgnoreCase) && currentUser.Nivel == UserLevels.Admin)
                return Forbid("Admin não pode promover outro Admin.");

            // Remove roles antigas
            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            // Atualiza TipoUsuario e Nivel
            user.TipoUsuario = novoPerfil;
            user.Nivel = novoPerfil switch
            {
                "Admin" => UserLevels.Admin,
                "Funcionario" => UserLevels.Funcionario,
                _ => UserLevels.Cliente
            };

            await _userManager.UpdateAsync(user);
            await _userManager.AddToRoleAsync(user, novoPerfil);

            return Ok($"Perfil do usuário {user.Email} alterado para {novoPerfil}");
        }



        // Remover acesso pelo email
        [HttpPost("remover-acesso")]
        public async Task<IActionResult> RemoverAcesso([FromQuery] string email)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return NotFound("Usuário não encontrado");

            if (user.Nivel == UserLevels.AdminMaster)
                return BadRequest("Não é permitido remover acesso do AdminMaster");

            if (currentUser.Nivel == UserLevels.Admin && user.Nivel >= UserLevels.Admin)
                return BadRequest("Admin não tem permissão para remover acesso de um Admin");

            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            user.TipoUsuario = "Cliente";
            user.Nivel = UserLevels.Cliente;
            await _userManager.UpdateAsync(user);

            await _userManager.AddToRoleAsync(user, "Cliente");

            return Ok($"Acesso removido. O usuário {user.Email} agora é Cliente.");
        }

        // Ver roles
        [HttpGet("roles")]
        public async Task<IActionResult> VerRoles([FromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("Usuário não encontrado");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }
    }

    public class CreateUserModel
    {
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TipoUsuario { get; set; }
    }
}
