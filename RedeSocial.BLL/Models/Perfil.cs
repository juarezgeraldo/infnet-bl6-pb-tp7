using System.ComponentModel.DataAnnotations;

namespace RedeSocial.BLL.Models
{
    public class Perfil
    {
        [Required]
        public int PerfilId { get; set; }
        public string NomePerfil { get; set; }
        public bool isAdministrador { get; set; }

        //public ApplicationUser? ApplicationUser { get; set; }
    }
}
