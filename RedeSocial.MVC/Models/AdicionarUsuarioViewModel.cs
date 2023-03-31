using RedeSocial.BLL.DTOs;
using RedeSocial.BLL.Models;

namespace RedeSocial.MVC.Models
{
    public class AdicionarUsuarioViewModel
    {
        public UsuarioDto Usuario { get; set; }
        public IEnumerable<Perfil> Perfis { get; set; }
    }
}
