using FlotaLuchitoWeb.Clases;
using FlotaLuchitoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebEjemplo.Pages
{
    public class ViajesModel : PageModel
    {
        private readonly DataBaseHelper _db;

        public ViajesModel(DataBaseHelper db)
        {
            _db = db;
        }

        [BindProperty]
        public Viajescs Viaje { get; set; } = new();

        public List<Viajescs> Viajes { get; set; } = new();
        public List<string> Conductores { get; set; } = new();
        public List<string> Vehiculos { get; set; } = new();
        public string MensajeOk { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            if (id.HasValue)
            {
                Viaje = await _db.ObtenerViajePorIdAsync(id.Value) ?? new Viajescs();
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

            // ALERTA CHORO: Akí parseamos las coordenadas a lo choro a mano con 'System.Globalization.CultureInfo.InvariantCulture' porque si no, esta wa se confunde con las comas y los puntos decimales y te deja la mansa escoba transformando la latitud en un número más largo que esperanza de pobre.

            // Sacamos la latitud de ande salimos
            if (double.TryParse(Request.Form["latitudOrigen"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double latOrig))
                Viaje.LatitudOrigen = latOrig;
            else
                Viaje.LatitudOrigen = null;

            // Sacamos la longitud de ande salimos
            if (double.TryParse(Request.Form["longitudOrigen"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lngOrig))
                Viaje.LongitudOrigen = lngOrig;
            else
                Viaje.LongitudOrigen = null;

            // Sacamos la latitud de ande vamos a parar
            if (double.TryParse(Request.Form["latitudDestino"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double latDest))
                Viaje.LatitudDestino = latDest;
            else
                Viaje.LatitudDestino = null;

            // Sacamos la longitud de ande vamos a parar
            if (double.TryParse(Request.Form["longitudDestino"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lngDest))
                Viaje.LongitudDestino = lngDest;
            else
                Viaje.LongitudDestino = null;

            // Víctima de la moda: no podís llegar antes de salir, po logi, ni que tuvierai el Delorean
            if (Viaje.FechaSalida.HasValue && Viaje.FechaLlegada.HasValue && Viaje.FechaLlegada.Value.Date < Viaje.FechaSalida.Value.Date)
            {
                ModelState.AddModelError("Viaje.FechaLlegada", "La llegada no puede ser antes de la salida");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            if (Viaje.Id == 0)
            {
                await _db.InsertarViajeAsync(Viaje);
                TempData["Ok"] = "Viaje guardado correctamente";
            }
            else
            {
                await _db.ActualizarViajeAsync(Viaje);
                TempData["Ok"] = "Viaje actualizado correctamente";
            }

            return RedirectToPage("/Viajes");
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            await _db.EliminarViajeAsync(id);
            TempData["Ok"] = "Viaje eliminado correctamente";
            return RedirectToPage("/Viajes");
        }



        private async Task CargarDatosAsync()
        {
            Viajes = await _db.ObtenerViajesAsync();
            Conductores = await _db.ObtenerConductoresComboAsync();
            Vehiculos = await _db.ObtenerVehiculosComboAsync();
            MensajeOk = TempData["Ok"]?.ToString() ?? string.Empty;
        }

        private bool UsuarioLogueado()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Usuario"));
        }

        public static string FormatearTiempo(int segundos)
        {
            int minutosTotales = (int)Math.Round(segundos / 60.0);
            int horas = minutosTotales / 60;
            int minutos = minutosTotales % 60;
            if (horas == 0)
            {
                return $"{minutos} min";
            }
            if (minutos == 0)
            {
                return $"{horas} h";
            }
            return $"{horas} h {minutos} min";
        }
    }
}
