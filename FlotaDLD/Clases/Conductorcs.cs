using System.ComponentModel.DataAnnotations;

namespace FlotaLuchitoWeb.Clases
{
    // El molde del chofer, hermano. Es la misma wa que guardamos en la tabla Conductores de la BD.
    public class Conductorcs
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUT es obligatorio")]
        [StringLength(15, ErrorMessage = "Máximo 15 caracteres")]
        public string Rut { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Ingrese un teléfono válido")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de licencia es obligatorio")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres")]
        public string NumeroLicencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar el tipo de licencia")]
        public string TipoLicencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar la fecha de vencimiento")]
        public DateTime? FechaVencimientoLicencia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado")]
        public string Estado { get; set; } = string.Empty;
    }
}
