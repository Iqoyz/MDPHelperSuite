using QRCoder;
using Color = System.Drawing.Color;

namespace fyp_MDPHelperApp.Services;

public class QrCodeHandler
{
    public static ImageSource GenerateQrCode(
        string inputText,
        int pixelsPerModule = 20,
        string foregroundColor = "#000000", // Default black
        string backgroundColor = "#FFFFFF" // Default white
    )
    {
        if (string.IsNullOrWhiteSpace(inputText))
            throw new ArgumentException("Input text cannot be null or empty.", nameof(inputText));

        try
        {
            // Create the QR code generator
            using var qrGenerator = new QRCodeGenerator();

            // Generate QR code data
            var qrCodeData = qrGenerator.CreateQrCode(inputText, QRCodeGenerator.ECCLevel.L);


            // Create a bitmap QR code with the specified colors
            using var qRCode = new BitmapByteQRCode(qrCodeData);
            var qrCodeBytes = qRCode.GetGraphic(pixelsPerModule, foregroundColor, backgroundColor);

            // Convert to ImageSource and return
            return ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating QR code: {ex.Message}");
            throw;
        }
    }

    public static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}