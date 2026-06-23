using FlotaLuchitoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebEjemplo.Pages
{
    public class LoginModel : PageModel
    {
        private readonly DataBaseHelper _db;
        // Aki nos prestan la base de datos de una, pa no andar picando código como perkin
        public LoginModel(DataBaseHelper db)
        {
            _db = db;
        }
        [BindProperty]
        public string UsuarioLogin { get; set; } = string.Empty;
        [BindProperty]
        public string Password { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public void OnGet()
        {
            HttpContext.Session.Clear();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UsuarioLogin))
            {
                ModelState.AddModelError(nameof(UsuarioLogin), "Ingrese el usuario");
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(nameof(Password), "Ingrese la contraseña");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var loginCorrecto = await _db.ValidarUsuarioAsync(UsuarioLogin, Password);
            if (!loginCorrecto)
            {
                Mensaje = "Usuario o contraseña incorrectos";
                return Page();
            }
            HttpContext.Session.SetString("Usuario", UsuarioLogin.Trim());
            return RedirectToPage("/Menu");
        }
    }
}
