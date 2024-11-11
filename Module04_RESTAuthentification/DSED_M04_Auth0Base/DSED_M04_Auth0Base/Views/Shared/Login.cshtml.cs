using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    public async Task<IActionResult> OnPostLogin()
    {
        // Redirige l'utilisateur pour l'authentification avec Auth0
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Auth0");
    }

    public async Task<IActionResult> OnPostLogout()
    {
        await HttpContext.SignOutAsync("Auth0");
        await HttpContext.SignOutAsync();
        return RedirectToPage("/"); // Redirige après la déconnexion
    }
}
