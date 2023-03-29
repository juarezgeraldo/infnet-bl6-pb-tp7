using System.Security.Claims;
using System.Security.Principal;

namespace RedeSocial.MVC.Helpers
{
    public static class IdentityExtensions
    {

        public static string GetName(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.FindFirst(ClaimTypes.Name);

            return claim?.Value ?? string.Empty;
        }
    }
}
