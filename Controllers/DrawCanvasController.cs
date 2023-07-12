using Microsoft.AspNetCore.Mvc;
using WEB_API_ASP.Resources;
using Newtonsoft.Json;
using SkiaSharp;

namespace WEB_API_ASP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CanvasController : ControllerBase
    {
        public static bool FindWord(HashSet<string> set, string value)
        {
            return set.Contains(value.ToLower());
        }
        [HttpPost]
        [Route("drawcanvas")]
        public IActionResult PostCanvas(IFormFile image, [FromForm] string txtAnnotationsString)
        {
            try
            {
                string allWordsJson = "~/files/AllWordsPtBr.json\"";
                string jsonStringAllWords;
                using (StreamReader sr = new StreamReader(allWordsJson))
                {
                    jsonStringAllWords = sr.ReadToEnd();
                }
                StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
                HashSet<string> hashSetNormal = JsonConvert.DeserializeObject<HashSet<string>>(jsonStringAllWords);
                HashSet<string> lowerCaseSet = new HashSet<string>(hashSetNormal, StringComparer.OrdinalIgnoreCase);
                List<TextAnnotationGoogleVision> textAnnotations = JsonConvert.DeserializeObject<List<TextAnnotationGoogleVision>>(txtAnnotationsString);
                if (image != null && image.Length > 0 && textAnnotations != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        image.CopyTo(memoryStream);
                        byte[] imageData = memoryStream.ToArray();
                        using (var originalImage = SKBitmap.Decode(imageData))
                        using (var surface = SKSurface.Create(new SKImageInfo(originalImage.Width, originalImage.Height)))
                        using (var canvas = surface.Canvas)
                        {
                            canvas.DrawBitmap(originalImage, 0, 0);
                            SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Green, StrokeWidth = 2 };
                            foreach (var annotation in textAnnotations)
                            {
                                var vertices = annotation.boundingPoly.vertices;
                                var xValues = vertices.Select(v => v.X);
                                var yValues = vertices.Select(v => v.Y);
                                var left = xValues.Min();
                                var top = yValues.Min();
                                var right = xValues.Max();
                                var bottom = yValues.Max();
                                var rect = SKRect.Create(left, top, right - left, bottom - top);
                                if (FindWord(lowerCaseSet, annotation.description))
                                    paint.Color = SKColors.Green;
                                else
                                    paint.Color = SKColors.Red;
                                canvas.DrawRect(rect, paint);
                            }
                            using (var resultImage = surface.Snapshot())
                            using (var resultStream = new MemoryStream())
                            {
                                resultImage.Encode(SKEncodedImageFormat.Jpeg, 100).SaveTo(resultStream);
                                resultStream.Position = 0;
                                var memoryResultStream = new MemoryStream(resultStream.ToArray());
                                memoryResultStream.Seek(0, SeekOrigin.Begin);
                                return new FileStreamResult(memoryResultStream, "image/jpeg");
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest("Nenhum arquivo de imagem ou anotações de texto foi enviado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Falha interna (servidor)");
            }
        }
    }
}