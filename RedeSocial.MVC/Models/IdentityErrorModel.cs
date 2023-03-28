namespace RedeSocial.MVC.Models
{
    public class IdentityErrorModel
    {
        public string Mensagem { get; set; }
        public IEnumerable<Erro> Erros { get; set; }
    }

    public class Erro
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
