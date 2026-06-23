// ==========================================================================================
// SERVICIO DE ASISTENTE DE CHAT (ChatAssistantService.cs)
// ==========================================================================================
// Este es el cerebro del ayudante virtual, pa que los usuarios no anden perdíos en la página
// y encuentren las cosas al tiro. Analiza lo que escribe el compadre en el chat y le 
// responde de vuelta con la pulenta info que anda buscando.
// ==========================================================================================

namespace FlotaLuchitoWeb.Services
{
    /// <summary>
    /// Servicio estrella del chat pa guiar a la gallada por la aplicación sin que se peguen un show.
    /// </summary>
    public class ChatAssistantService
    {
        // El cajón donde guardamos las respuestas organizadas por temas.
        private readonly Dictionary<string, List<string>> _responses;

        // El constructor del servicio: arma el diccionario y le mete las respuestas altiro.
        public ChatAssistantService()
        {
            _responses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            InitializeResponses();
        }

        /// <summary>
        /// Rellena el diccionario con las respuestas que le vamos a dar a los chiquillos.
        /// </summary>
        private void InitializeResponses()
        {
            // Respuestas simpáticas cuando la gente viene recién saludando.
            _responses.Add("saludo", new List<string>
            {
                "¡Hola! Soy el asistente de Flota DLD. ¿En qué puedo ayudarte hoy? 👋",
                "¡Bienvenido! Estoy aquí para ayudarte a navegar por la aplicación. ¿Qué te gustaría hacer?",
            });

            // Respuestas pa cachar qué onda con los choferes.
            _responses.Add("conductor", new List<string>
            {
                "Para gestionar conductores, dirígete a la sección de 'Conductores' donde puedes agregar, editar o eliminar conductores. 🚗",
                "En el módulo de Conductores puedes ver el listado completo, registrar nuevos conductores y actualizar su información.",
            });

            // Respuestas pa guiar a la sección de las máquinas (vehículos).
            _responses.Add("vehiculo", new List<string>
            {
                "En la sección 'Vehículos' puedes administrar toda la flota. Allí podrás agregar nuevos vehículos o editarlos. 🚙",
                "El módulo de Vehículos te permite gestionar el inventario completo de la flota.",
            });

            // Respuestas pa controlar los recorridos o viajes.
            _responses.Add("viaje", new List<string>
            {
                "La sección 'Viajes' es donde registras y controlas todos los desplazamientos de la flota. ✈️",
                "En Viajes puedes crear nuevos registros de desplazamientos y llevar el control de los mismos.",
            });

            // Respuestas pa cuando andan más perdíos que el teniente Bello.
            _responses.Add("ayuda", new List<string>
            {
                "Te ayudaré gustosamente. Cuéntame qué necesitas: ¿buscas información sobre Conductores, Vehículos o Viajes? 🤔",
                "¿En qué parte de la aplicación necesitas asistencia?",
            });

            // Información sobre el menú global.
            _responses.Add("menu", new List<string>
            {
                "En el menú principal tienes acceso a: Conductores, Vehículos y Viajes. ¿Cuál te interesa? 📋",
                "Desde el menú puedes acceder a todas las funcionalidades principales de la aplicación.",
            });

            // Guía pa volver a la base.
            _responses.Add("inicio", new List<string>
            {
                "¿Te gustaría volver al menú principal o necesitas ayuda con algo específico?",
                "Puedo guiarte hacia Conductores, Vehículos o Viajes. ¿Cuál prefieres?",
            });

            // Respuestas por defecto cuando el bot no entiende ni una cuestión.
            _responses.Add("default", new List<string>
            {
                "Entiendo que necesitas ayuda. Puedo guiarte sobre: Conductores, Vehículos o Viajes. ¿Cuál deseas?",
                "No estoy seguro de lo que preguntas, pero puedo ayudarte con Conductores, Vehículos o Viajes. 😊",
                "Parece que necesitas asistencia. Pregúntame sobre Conductores, Vehículos o Viajes.",
            });
        }


        /// <summary>
        /// Recibe la cháchara del usuario y cacha qué responderle.
        /// </summary>
        /// <param name="userMessage">El mensaje que mandó el compadre.</param>
        /// <returns>Una respuesta al azar de la categoría que corresponda.</returns>
        public string GetResponse(string userMessage)
        {
            // Si el mensaje está vacío o es puro espacio, le tiramos un saludo por defecto pa no ser rotos.
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return GetRandomResponse("saludo");
            }

            // Pasamos todo a minúsculas pa que no haya atados con las mayúsculas.
            string lowerMessage = userMessage.ToLower().Trim();
            // Analizamos el mensaje pa ver en qué categoría cae.
            string category = DeterminateCategory(lowerMessage);
            // Sacamos una respuesta buena de esa categoría.
            return GetRandomResponse(category);
        }

        /// <summary>
        /// Analiza la frase pa ver si habla de choferes, micros (autos), viajes o si anda pidiendo auxilio.
        /// </summary>
        /// <param name="message">El mensaje ya limpiecito y en minúsculas.</param>
        /// <returns>La categoría detectada.</returns>
        private string DeterminateCategory(string message)
        {
            // Diccionario con las palabras clave pa cachar la intención de la consulta.
            var keywords = new Dictionary<string, string[]>
            {
                { "conductor", new[] { "conductor", "conductores", "chofer", "choferes", "driver" } },
                { "vehiculo", new[] { "vehiculo", "vehículos", "auto", "autos", "carro", "carros", "máquina", "máquinas" } },
                { "viaje", new[] { "viaje", "viajes", "ruta", "rutas", "desplazamiento", "desplazamientos", "trayecto" } },
                { "ayuda", new[] { "ayuda", "ayudame", "help", "socorro", "asistencia", "necesito" } },
                { "menu", new[] { "menu", "menú", "inicio", "principal", "opciones" } },
            };

            // Recorre el diccionario pa ver si el mensaje contiene alguna de las palabras clave.
            foreach (var kvp in keywords)
            {
                if (kvp.Value.Any(word => message.Contains(word)))
                {
                    return kvp.Key;
                }
            }

            // Si es un saludo rápido, lo pillamos altiro aquí.
            if (message.Contains("hola") || message.Contains("hí") || message.Contains("buenos"))
            {
                return "saludo";
            }

            // Si no le apuntó a nada de arriba, cae al saco del default.
            return "default";
        }

        /// <summary>
        /// Saca una respuesta al azar de la tómbola pa que el asistente no parezca un robot aburrido de oficina.
        /// </summary>
        /// <param name="category">La categoría de donde queremos sacar la respuesta.</param>
        /// <returns>Una respuesta al azar.</returns>
        private string GetRandomResponse(string category)
        {
            // Si la categoría que nos pasaron no existe en el mapa, nos vamos a la segura con 'default'.
            if (!_responses.ContainsKey(category))
            {
                category = "default";
            }

            var responses = _responses[category];
            // Genera un número al azar pa sacar una respuesta distinta cada vez.
            int randomIndex = new Random().Next(responses.Count);
            return responses[randomIndex];
        }

        /// <summary>
        /// El torpedo de opciones disponibles por si el usuario anda más perdido.
        /// </summary>
        /// <returns>Una string con las sugerencias de la casa.</returns>
        public string GetSuggestions()
        {
            return "Puedo ayudarte con: 📚 Conductores • 🚗 Vehículos • ✈️ Viajes • ❓ Ayuda • 📋 Menú";
        }
    }
}
