using Microsoft.AspNetCore.Identity;

namespace SistemaBarbearia.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; }
        public string TipoUsuario { get; set; } // Cliente, Funcionario, Admin, AdminMaster
        public int Nivel { get; set; } // 0=Cliente, 1=Funcionario, 2=Admin, 3=AdminMaster
    }
}
