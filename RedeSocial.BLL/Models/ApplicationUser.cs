using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedeSocial.BLL.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; }
        public string NomeCompleto { get; set; }

    }
}
