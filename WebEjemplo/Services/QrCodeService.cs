using QRCoder;

namespace FlotaLuchitoWeb.Services
{
    // ==========================================================================================
    // SERVICIO DE CÓDIGOS QR (QrCodeService.cs)
    // ==========================================================================================
    // Este servicio es pulento para fabricar códigos QR al vuelo. Agarra cualquier texto o link,
    // le da forma de cuadraditos y te lo entrega listo pa ponerlo en la pantalla y escanearlo.
    // ==========================================================================================
    public class QrCodeService
    {
        /// <summary>
        /// Genera un código QR y lo escupe en formato Base64 para cargarlo al toque en el HTML.
        /// </summary>
        /// <param name="text">El texto o link que queremos encriptar en el QR.</param>
        /// <returns>Una string en Base64 que representa la imagen PNG del QR.</returns>
        public string GenerateQrCodeBase64(string text)
        {
            // 1. Iniciamos la máquina que fabrica la estructura del código QR.
            using var qrGenerator = new QRCodeGenerator();

            // 2. Crea el QR con nivel de corrección 'Q' (es súper aperrao, si se raya un poco igual se lee).
            using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            // 3. Convierte esos datos abstractos en una imagen PNG real.
            using var qrCode = new PngByteQRCode(qrData);

            // 4. Genera el arreglo de bytes del archivo PNG. El parámetro (20) define el tamaño del pixel.
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

            // 5. Transformamos la imagen en una cadena Base64 para mandarla al frontend sin pasar por el disco.
            return Convert.ToBase64String(qrCodeAsPngByteArr);
        }
    }
}
