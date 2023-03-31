using Microsoft.AspNetCore.Mvc.Rendering;
using RedeSocial.BLL.Models;

namespace RedeSocial.MVC.Models
{
    public class AlterarPerfilViewModel
    {
        public AlterarPerfilDto AlterarPerfil { get; set; }

        public IEnumerable<Perfil> Perfis { get; set; }
    }
}
