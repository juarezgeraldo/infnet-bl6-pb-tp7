using System.ComponentModel.DataAnnotations;
using RedeSocial.BLL.Models;

namespace RedeSocial.BLL.DTOs
{
    public class UsuarioDto
    {
        [Required]
        public string NomeUsuario { get; set; }
        [Required]
        public string SenhaUsuario { get; set; }
        [Required]
        public string EmailUsuario { get; set; }
        public string TelefoneUsuario { get; set; }
        [Required]
        public string NomeCompletoUsuario { get; set; }
        [Required]
        public int PerfilId { get; set; }

    }
}