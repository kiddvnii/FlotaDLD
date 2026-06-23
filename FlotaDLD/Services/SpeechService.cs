using System.IO;
using System.Speech.Synthesis;
using System.Linq;
using System.Runtime.Versioning;

namespace FlotaLuchitoWeb.Services
{
    // ==========================================================================================
    // SERVICIO DE SÍNTESIS DE VOZ (SpeechService.cs)
    // ==========================================================================================
    // Este servicio agarra un texto cualquiera y lo transforma en audio hablado para que
    // el sistema no sea tan mudo. Ojo: corre sobre las librerías nativas de Windows,
    // así que si lo subes a un servidor Linux va a guatear/dar la hora altiro.
    // ==========================================================================================
    [SupportedOSPlatform("windows")] // Te lo advertí: esto es exclusivo pa Windows.
    public class SpeechService
    {
        /// <summary>
        /// Convierte un texto plano a un arreglo de bytes que representan un archivo de audio WAV.
        /// </summary>
        /// <param name="text">El texto que el robot tiene que leer.</param>
        /// <returns>Arreglo de bytes del audio grabado.</returns>
        public byte[] SpeakTextToWavBytes(string text)
        {
            // 1. Levantamos el motor de síntesis de voz de Windows.
            using (var synthesizer = new SpeechSynthesizer())
            {
                // 2. Buscamos si el sistema operativo tiene alguna voz configurada en español ("es").
                var spanishVoice = synthesizer.GetInstalledVoices()
                    .FirstOrDefault(v => v.Enabled && v.VoiceInfo.Culture.Name.StartsWith("es", System.StringComparison.OrdinalIgnoreCase));
                
                // Si pillamos una voz en español, la dejamos seleccionada pa que no hable como gringo.
                if (spanishVoice != null)
                {
                    synthesizer.SelectVoice(spanishVoice.VoiceInfo.Name);
                }
                
                // 3. Abrimos una tubería de memoria (MemoryStream) pa guardar el audio en la RAM y no crear archivos basura.
                using (var stream = new MemoryStream())
                {
                    // Configuramos el sintetizador pa que guarde el audio en el flujo de memoria como WAV.
                    synthesizer.SetOutputToWaveStream(stream);
                    
                    // El robot lee el texto y escribe el audio en el flujo.
                    synthesizer.Speak(text);
                    
                    // Soltamos la salida pa que no quede tomada la memoria.
                    synthesizer.SetOutputToNull();
                    
                    // Escupimos los bytes fresquitos listos pa mandarlos al navegador.
                    return stream.ToArray();
                }
            }
        }
    }
}
