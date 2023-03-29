using RedeSocial.BLL.Models;

namespace RedeSocial.MVC.Models
{
    public class AdicionarUsuarioViewModel
    {
        public Usuario Usuario { get; set; }
        public IEnumerable<Perfil> Perfis { get; set; }
    }
}
