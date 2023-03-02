using RedeSocial.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RedeSocial.API.Model
{
    public class Usuario
    {
        [Required]
        public string NomeUsuario { get; set; }
        [Required]
        public string SenhaUsuario { get; set;}
        [Required]
        public string EmailUsuario { get; set;}
        public string TelefoneUsuario { get; set;}
        [Required]
        public Perfil Perfil { get; set;}
    }
}
