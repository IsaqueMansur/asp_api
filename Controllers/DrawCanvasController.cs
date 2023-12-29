using Microsoft.AspNetCore.Mvc;
using WEB_API_ASP.Resources;
using Newtonsoft.Json;
using SkiaSharp;
using System;

namespace WEB_API_ASP.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CanvasController : ControllerBase
    {
        public static IFormFile ConvertFromBase64(string base64String, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64String))
                {
                    throw new ArgumentException("A string base64 está vazia ou nula.");
                }
                var base64Parts = base64String.Split(',');
                var base64Data = base64Parts.Length > 1 ? base64Parts[1] : base64Parts[0];

                byte[] bytes = Convert.FromBase64String(base64Data);

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    byte[] copiedBytes = new byte[ms.Length];
                    ms.Read(copiedBytes, 0, (int)ms.Length); 
                    IFormFile file = new FormFile(new MemoryStream(copiedBytes), 0, copiedBytes.Length, "image", fileName);
                    return file;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao converter para bytes: " + ex.Message);
                throw;
            }
        }





        private static bool FindWord(HashSet<string> set, string value)
        {
            return set.Contains(value.ToLower());
        }

        [HttpPost]
        [Route("drawcanvas")]
        public IActionResult PostCanvas(
            [FromForm] string imageBase64,
            [FromForm] string txtAnnotationsString, 
            [FromForm] string colorConfig,
            [FromForm] string mode,
            [FromForm] int lineEspessure
            )
        {
            try
            {
                IFormFile image = ConvertFromBase64(imageBase64, "file_name");
                string allWordsJson = "./files/AllWordsPtBr.json";
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
                            SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Green, StrokeWidth = lineEspessure };
                            foreach (var annotation in textAnnotations)
                            {
                                var vertices = annotation.boundingPoly.vertices;
                                var xValues = vertices.Select(v => v.X);
                                var yValues = vertices.Select(v => v.Y);
                                var left = xValues.Min() - 1;
                                var top = yValues.Min() - 1;
                                var right = xValues.Max() + 1;
                                var bottom = yValues.Max() + 1;
                                var rect = SKRect.Create(left, top, right - left, bottom - top);
                                var wordExists = FindWord(lowerCaseSet, annotation.description);
                                if (FindWord(lowerCaseSet, annotation.description))
                                {
                                    switch (colorConfig)
                                    {
                                        case "1":
                                            paint.Color = SKColors.Green;
                                            break;
                                        case "2":
                                            paint.Color = SKColors.Blue;
                                            break;
                                        case "3":
                                            paint.Color = SKColors.DarkViolet;
                                            break;
                                        case "4":
                                            paint.Color = SKColors.Cyan;
                                            break;
                                        default:
                                            paint.Color = SKColors.Green;
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (colorConfig)
                                    {
                                        case "1":
                                            paint.Color = SKColors.Red;
                                            break;
                                        case "2":
                                            paint.Color = SKColors.Yellow;
                                            break;
                                        case "3":
                                            paint.Color = SKColors.DarkOrange;
                                            break;
                                        case "4":
                                            paint.Color = SKColors.Magenta;
                                            break;
                                        default: paint.Color = SKColors.Red;
                                            break;
                                    }
                                }
                                if (FindWord(lowerCaseSet, annotation.description) && mode == "all")
                                {
                                    canvas.DrawRect(rect, paint);
                                }
                                if (!FindWord(lowerCaseSet, annotation.description))
                                {
                                    canvas.DrawRect(rect, paint);
                                }                            
                                
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