using System.ComponentModel.DataAnnotations;

namespace RedeSocial.API.Model
{
    public class DetalheUsuario
    {
        [Required]
        public string NomeUsuario { get; set; }
        [Required]
        public string SenhaUsuario { get; set;}
        [Required]
        public string EmailUsuario { get; set;}
    }
}
