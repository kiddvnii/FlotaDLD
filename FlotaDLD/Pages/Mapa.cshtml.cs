using FlotaLuchitoWeb.Clases;
using FlotaLuchitoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebEjemplo.Pages
{
    // El modelito pa pintar el mapa interactivo en la pantalla, hermano.
    // Con esta wa cachamos los viajes de cada nave y los dibujamos al toque.
    public class MapaModel : PageModel
    {
        private readonly DataBaseHelper _db;

        // Aki nos inyectan el helper de la BD pa hacerle las consultas pulentas
        public MapaModel(DataBaseHelper db)
        {
            _db = db;
        }

        // Listado de naves pa rellenar el dropdown del filtro
        public List<string> Vehiculos { get; set; } = new();
        
        // Los viajes que vamos a enlistar en el costao del mapa
        public List<Viajescs> Viajes { get; set; } = new();
        
        // Guarda el viaje que vamos a trazar de una al entrar a la pantalla (si el logi eligió uno)
        public Viajescs? ViajeSeleccionado { get; set; }
        
        // Guarda la patente de la nave que andamos sapeando en el filtro
        public string? VehiculoSeleccionado { get; set; }

        // Metodo GET: la primera wa que corre cuando cargamos la pagina.
        // Soporta:
        // - viajeId: pa enfocarnos en una ruta entera detoná.
        // - vehiculo: pa traer el historial de viajes de una pura nave.
        public async Task<IActionResult> OnGetAsync(int? viajeId, string? vehiculo)
        {
            // Sapeamos si el loco está logueao, si no, yera, lo mandamos de una patá al login
            if (!UsuarioLogueado())
            {
                return RedirectToPage("/Login");
            }

            // Traemos todas las naves disponibles pa rellenar el select
            Vehiculos = await _db.ObtenerVehiculosComboAsync();

            // CASO A: Si el logi quiere ver un viaje específico
            if (viajeId.HasValue)
            {
                ViajeSeleccionado = await _db.ObtenerViajePorIdAsync(viajeId.Value);
                if (ViajeSeleccionado != null)
                {
                    // Dejamos seleccionada la patente del viaje
                    VehiculoSeleccionado = ViajeSeleccionado.Vehiculo;
                    
                    // Cargamos todo el historial de viajes de esa misma nave pa mostrarlo en el panel
                    Viajes = (await _db.ObtenerViajesAsync())
                        .Where(v => v.Vehiculo == ViajeSeleccionado.Vehiculo)
                        .ToList();
                }
            }
            // CASO B: Si filtraron por una nave específica
            else if (!string.IsNullOrWhiteSpace(vehiculo))
            {
                VehiculoSeleccionado = vehiculo;
                
                // Nos traemos solo las rutas de esa pura nave
                Viajes = (await _db.ObtenerViajesAsync())
                    .Where(v => v.Vehiculo == vehiculo)
                    .ToList();
            }
            // CASO C: Si el loco entró a la mala sin filtros
            else
            {
                // Le tiramos todos los viajes de la flota pa que se entretenga
                Viajes = await _db.ObtenerViajesAsync();
            }

            return Page();
        }

        // Comprobamos si el perkin inició sesión o anda queriendo colarse
        private bool UsuarioLogueado()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Usuario"));
        }
    }
}
