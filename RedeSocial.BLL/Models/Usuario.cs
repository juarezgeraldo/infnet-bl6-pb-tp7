using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RedeSocial.BLL.Models
{
    public class Usuario
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
        public Perfil Perfil { get; set; }

    }
}