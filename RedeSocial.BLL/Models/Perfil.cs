using Microsoft.AspNetCore.Identity;

namespace RedeSocial.BLL.Models
{
    public class Perfil
    {
        public int Id { get; set; }
        public string NomePerfil { get; set; }
        public bool isAdministrador { get; set; }
    }
}
