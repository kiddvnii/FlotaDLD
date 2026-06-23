using System.ComponentModel.DataAnnotations;

namespace FlotaLuchitoWeb.Clases
{
    // El molde de la nave, hermano. Sirve pa chantar los datos en el formulario y pa guardarlos en SQL.
    public class Vehiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el tipo de vehículo")]
        public string TipoVehiculo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un conductor")]
        public string Conductor { get; set; } = string.Empty;

        [Required(ErrorMessage = "La patente es obligatoria")]
        [StringLength(12, ErrorMessage = "Máximo 12 caracteres")]
        public string Patente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(1980, 2100, ErrorMessage = "Ingrese un año válido")]
        public int? Anio { get; set; }

        [Required(ErrorMessage = "El color es obligatorio")]
        [StringLength(40, ErrorMessage = "Máximo 40 caracteres")]
        public string Color { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar el combustible")]
        public string Combustible { get; set; } = string.Empty;

        [Range(0, 9999999, ErrorMessage = "El kilometraje no puede ser negativo")]
        public decimal? Kilometraje { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado")]
        public string Estado { get; set; } = string.Empty;

        public bool EsNuevo { get; set; }

        // Estas dos casillas nos las pidieron pa cachar al tiro si la máquina tiene partes o va pal taller.
        public bool TieneMultas { get; set; }
        public bool VaAMantencion { get; set; }
    }
}
