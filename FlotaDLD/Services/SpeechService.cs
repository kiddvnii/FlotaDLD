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
    [SupportedOSPlatform("windows")] // Te lo advertí: esto corre con las tripas de Windows, en Linux se cae al litro.
    public class SpeechService
    {
        /// <summary>
        /// Convierte un texto plano a un arreglo de bytes que representan un archivo de audio WAV.
        /// </summary>
        /// <param name="text">El texto que el robot tiene que leer.</param>
        /// <returns>Arreglo de bytes del audio grabado.</returns>
        public byte[] SpeakTextToWavBytes(string text)
        {
            // 1. Levantamos el motor de síntesis de voz nativo de Windows (SpeechSynthesizer).
            using (var synthesizer = new SpeechSynthesizer())
            {
                // 2. Buscamos si el Windows tiene instalada alguna voz en español (idioma "es"). 
                // Si no tiene, el robot va a hablar como gringo intentando hablar español y va a sonar terrible de mal.
                var spanishVoice = synthesizer.GetInstalledVoices()
                    .FirstOrDefault(v => v.Enabled && v.VoiceInfo.Culture.Name.StartsWith("es", System.StringComparison.OrdinalIgnoreCase));
                
                // Si pillamos una voz en español, la dejamos seleccionada al tiro pa' que hable fluido.
                if (spanishVoice != null)
                {
                    synthesizer.SelectVoice(spanishVoice.VoiceInfo.Name);
                }
                
                // 3. Abrimos una tubería de memoria (MemoryStream) pa' meter el audio directamente en la RAM.
                // Así no llenamos el disco con archivos temporales basura que después nadie borra y andan dando jugo.
                using (var stream = new MemoryStream())
                {
                    // Configuramos el sintetizador pa' que guarde el audio en el flujo de memoria como formato WAV.
                    synthesizer.SetOutputToWaveStream(stream);
                    
                    // El robot lee el texto y escribe los bytes en el flujo de memoria.
                    synthesizer.Speak(text);
                    
                    // Soltamos la salida pa' que no quede tomada la memoria de forma innecesaria.
                    synthesizer.SetOutputToNull();
                    
                    // Escupimos los bytes fresquitos listos pa' ser inyectados en la respuesta HTTP y que el navegador le ponga play.
                    return stream.ToArray();
                }
            }
        }
    }
}
