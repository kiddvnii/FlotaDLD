using System.ComponentModel.DataAnnotations;

namespace FlotaLuchitoWeb.Clases
{
    // Molde del logi que va a ingresar al sistema, entero basico.
    // Pa que cachi: el usuario en la BD es el luchito con la pass 1234, terrible tirao.
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string UsuarioLogin { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }
}
