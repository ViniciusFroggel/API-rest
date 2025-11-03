public class RegisterDto
{
    public string NomeCompleto { get; set; }
    public string PhoneNumber { get; set; }
    public string Senha { get; set; }
    public string TipoUsuario { get; set; } = "Cliente";
    public int Nivel { get; set; } = 0; // 0=cliente, 1=func, 2=admin, 3=adminmaster

}
