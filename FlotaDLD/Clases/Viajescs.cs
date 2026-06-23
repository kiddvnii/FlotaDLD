using System.ComponentModel.DataAnnotations;

namespace FlotaLuchitoWeb.Clases
{
    // El molde del pique, hermano. Cada dato es calco de lo que va en las columnas de la tabla Viajes.
    public class Viajescs
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un vehículo")]
        public string Vehiculo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un conductor")]
        public string Conductor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El origen es obligatorio")]
        [StringLength(80, ErrorMessage = "Máximo 80 caracteres")]
        public string Origen { get; set; } = string.Empty;

        [Required(ErrorMessage = "El destino es obligatorio")]
        [StringLength(80, ErrorMessage = "Máximo 80 caracteres")]
        public string Destino { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar la fecha de salida")]
        public DateTime? FechaSalida { get; set; }

        [Required(ErrorMessage = "Debe ingresar la fecha de llegada")]
        public DateTime? FechaLlegada { get; set; }

        [Required(ErrorMessage = "La distancia es obligatoria")]
        [Range(1, 999999, ErrorMessage = "La distancia debe ser mayor a 0")]
        public decimal? Distancia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el estado")]
        public string Estado { get; set; } = string.Empty;

        public double? LatitudOrigen { get; set; }
        public double? LongitudOrigen { get; set; }
        public double? LatitudDestino { get; set; }
        public double? LongitudDestino { get; set; }
        public int? TiempoEstimado { get; set; }
    }
}
