namespace RedeSocial.API.Models
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public Perfil perfil { get; set; }
    }
}
