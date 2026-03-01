using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace PotentialOverlay.Services
{
    public class OcrService
    {
        private OcrEngine? _ocrEngine;

        public OcrService()
        {
            var lang = new Windows.Globalization.Language("ko-KR");
            if (OcrEngine.IsLanguageSupported(lang))
            {
                _ocrEngine = OcrEngine.TryCreateFromLanguage(lang);
            }
        }

        public bool IsAvailable => _ocrEngine != null;

        public async Task<string> RecognizeAsync(Rectangle region)
        {
            if (_ocrEngine == null || region.Width <= 0 || region.Height <= 0) return "";

            try
            {
                using (Bitmap bmp = new Bitmap(region.Width, region.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(region.Left, region.Top, 0, 0, region.Size);
                    }

                    using (var stream = new MemoryStream())
                    {
                        bmp.Save(stream, ImageFormat.Bmp);
                        stream.Position = 0;
                        var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                        var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                        var result = await _ocrEngine.RecognizeAsync(softwareBitmap);
                        
                        return result.Text.Replace(" ", "").Trim();
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
}