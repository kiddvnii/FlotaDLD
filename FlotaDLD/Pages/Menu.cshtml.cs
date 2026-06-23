#pragma warning disable CA1416
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FlotaLuchitoWeb.Services;

namespace WebEjemplo.Pages
{
    public class MenuModel : PageModel
    {
        private readonly ChatAssistantService _chatService;
        private readonly SpeechService _speechService;

        public string Usuario { get; set; } = string.Empty;

        public MenuModel(ChatAssistantService chatService, SpeechService speechService)
        {
            _chatService = chatService;
            _speechService = speechService;
        }

        public IActionResult OnGet()
        {
            Usuario = HttpContext.Session.GetString("Usuario") ?? string.Empty;

            // Oe hermano, si el logi no ha iniciao sesión, lo mandamos de una patá en la raja de vuelta al Login pa que no ande saquiando
            if (string.IsNullOrWhiteSpace(Usuario))
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        // El endpoint del chat pa responderle al perkin de turno lo que ande preguntando
        [ValidateAntiForgeryToken]
        public IActionResult OnPostChat([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Message))
                {
                    return BadRequest(new { error = "El mensaje no puede estar vacío" });
                }

                var response = _chatService.GetResponse(request.Message);
                return new JsonResult(new { message = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error procesando tu mensaje: " + ex.Message });
            }
        }

        // Con este endpoint le devolvemos los audios de voz pa que el robot hable terrible detonao
        public IActionResult OnGetSpeak(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("El texto no puede estar vacío");
            }

            try
            {
                byte[] audioBytes = _speechService.SpeakTextToWavBytes(text);
                return File(audioBytes, "audio/wav");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error en síntesis de voz: " + ex.Message);
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
