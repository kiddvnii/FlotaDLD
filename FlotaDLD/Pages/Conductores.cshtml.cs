using FlotaLuchitoWeb.Clases;
using FlotaLuchitoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebEjemplo.Pages
{
    public class ConductoresModel : PageModel
    {
        private readonly DataBaseHelper _db;

        public ConductoresModel(DataBaseHelper db)
        {
            _db = db;
        }

        [BindProperty]
        public Conductorcs Conductor { get; set; } = new();

        public List<Conductorcs> Conductores { get; set; } = new();
        public string MensajeOk { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            // Si el wn te tira un id en la URL, es que el loco quiere chantarle una edición
            if (id.HasValue)
            {
                Conductor = await _db.ObtenerConductorPorIdAsync(id.Value) ?? new Conductorcs();
            }

            await CargarListadoAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync()
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            // Terrible vío: ni ahí con dejar pasar licencias vencías, hay que andar por la ley o si no te para el paco
            if (Conductor.FechaVencimientoLicencia.HasValue && Conductor.FechaVencimientoLicencia.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("Conductor.FechaVencimientoLicencia", "La licencia no puede estar vencida");
            }

            // Cuidado washito: el RUT es único, no nos querai pasar un gol con un rut repetío
            if (!string.IsNullOrWhiteSpace(Conductor.Rut) && await _db.ExisteRutAsync(Conductor.Rut, Conductor.Id))
            {
                ModelState.AddModelError("Conductor.Rut", "Ya existe un conductor con ese RUT");
            }

            if (!ModelState.IsValid)
            {
                await CargarListadoAsync();
                return Page();
            }

            if (Conductor.Id == 0)
            {
                await _db.InsertarConductorAsync(Conductor);
                TempData["Ok"] = "Conductor guardado correctamente";
            }
            else
            {
                await _db.ActualizarConductorAsync(Conductor);
                TempData["Ok"] = "Conductor actualizado correctamente";
            }

            return RedirectToPage("/Conductores");
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            await _db.EliminarConductorAsync(id);
            TempData["Ok"] = "Conductor eliminado correctamente";
            return RedirectToPage("/Conductores");
        }

        private async Task CargarListadoAsync()
        {
            Conductores = await _db.ObtenerConductoresAsync();
            MensajeOk = TempData["Ok"]?.ToString() ?? string.Empty;
        }

        private bool UsuarioLogueado()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Usuario"));
        }
    }
}
