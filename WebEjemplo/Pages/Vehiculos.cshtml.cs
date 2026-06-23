using FlotaLuchitoWeb.Clases;
using FlotaLuchitoWeb.Data;
using FlotaLuchitoWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebEjemplo.Pages
{
    public class VehiculosModel : PageModel
    {
        private readonly DataBaseHelper _db;
        private readonly QrCodeService _qrService;

        public VehiculosModel(DataBaseHelper db, QrCodeService qrService)
        {
            _db = db;
            _qrService = qrService;
        }

        [BindProperty]
        public Vehiculo Vehiculo { get; set; } = new();

        public List<Vehiculo> Vehiculos { get; set; } = new();
        public List<string> Conductores { get; set; } = new();
        public string MensajeOk { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            if (id.HasValue)
            {
                Vehiculo = await _db.ObtenerVehiculoPorIdAsync(id.Value) ?? new Vehiculo();
            }

            await CargarDatosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync()
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            // Validación pa que el longi no nos meta un año terrible descascarado del futuro o entero del año del loly
            if (Vehiculo.Anio.HasValue && Vehiculo.Anio.Value > DateTime.Today.Year + 1)
            {
                ModelState.AddModelError("Vehiculo.Anio", "El año no puede ser mayor al próximo año");
            }

            // Cuidao ahí: la patente es la firma de la nave, no podís tener dos naves con el mismo fierro
            if (!string.IsNullOrWhiteSpace(Vehiculo.Patente) && await _db.ExistePatenteAsync(Vehiculo.Patente, Vehiculo.Id))
            {
                ModelState.AddModelError("Vehiculo.Patente", "Ya existe un vehículo con esa patente");
            }


            if (Vehiculo.EsNuevo)
            {
                Vehiculo.Kilometraje = 0;
                Vehiculo.TieneMultas = false;
                ModelState.Remove("Vehiculo.Kilometraje");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            if (Vehiculo.Id == 0)
            {
                await _db.InsertarVehiculoAsync(Vehiculo);
                TempData["Ok"] = "Vehículo guardado correctamente";
            }
            else
            {
                await _db.ActualizarVehiculoAsync(Vehiculo);
                TempData["Ok"] = "Vehículo actualizado correctamente";
            }

            return RedirectToPage("/Vehiculos");
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            await _db.EliminarVehiculoAsync(id);
            TempData["Ok"] = "Vehículo eliminado correctamente";
            return RedirectToPage("/Vehiculos");
        }

        public async Task<IActionResult> OnGetQrCodeAsync(int id)
        {
            if (!UsuarioLogueado())
            {
                return new JsonResult(new { error = "No autorizado" });
            }

            var vehiculo = await _db.ObtenerVehiculoPorIdAsync(id);
            if (vehiculo == null)
            {
                return new JsonResult(new { error = "Vehículo no encontrado" });
            }

            // Armamos la info de la nave pa tirarla en los cuadraditos del QR
            string data = $"Patente: {vehiculo.Patente}\n" +
                          $"Tipo: {vehiculo.TipoVehiculo}\n" +
                          $"Marca/Modelo: {vehiculo.Marca} {vehiculo.Modelo}\n" +
                          $"Conductor: {vehiculo.Conductor}\n" +
                          $"Estado: {vehiculo.Estado}\n" +
                          $"Kilometraje: {(vehiculo.EsNuevo ? "0" : vehiculo.Kilometraje?.ToString())} km\n" +
                          $"Multas: {(vehiculo.TieneMultas ? "Sí" : "No")}\n" +
                          $"Mantención: {(vehiculo.VaAMantencion ? "Sí" : "No")}";

            try
            {
                string base64 = _qrService.GenerateQrCodeBase64(data);
                return new JsonResult(new { base64 = base64, patente = vehiculo.Patente });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = "Error al generar el QR: " + ex.Message });
            }
        }

        private async Task CargarDatosAsync()
        {
            Vehiculos = await _db.ObtenerVehiculosAsync();
            Conductores = await _db.ObtenerConductoresComboAsync();
            MensajeOk = TempData["Ok"]?.ToString() ?? string.Empty;
        }

        private bool UsuarioLogueado()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Usuario"));
        }
    }
}
