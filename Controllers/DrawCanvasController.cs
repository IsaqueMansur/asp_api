using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using WEB_API_ASP.Resources;
using Newtonsoft.Json;

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
                        using (var imageStream = new MemoryStream(imageData))
                        using (var originalImage = Image.FromStream(imageStream))
                        using (var graphics = Graphics.FromImage(originalImage))
                        {
                            Pen pen;
                            foreach (var annotation in textAnnotations)
                            {
                                var vertices = annotation.boundingPoly.vertices;
                                var xValues = vertices.Select(v => v.X);
                                var yValues = vertices.Select(v => v.Y);
                                var left = xValues.Min();
                                var top = yValues.Min();
                                var right = xValues.Max();
                                var bottom = yValues.Max();
                                var rectangle = new Rectangle(left, top, right - left, bottom - top);
                                if (FindWord(lowerCaseSet, annotation.description))
                                    pen = new Pen(Color.Green, 2);
                                else
                                    pen = new Pen(Color.Red, 2);
                                graphics.DrawRectangle(pen, rectangle);
                                pen.Dispose();
                            }
                            var resultStream = new MemoryStream();
                            originalImage.Save(resultStream, ImageFormat.Jpeg);
                            resultStream.Position = 0;
                            return new FileStreamResult(resultStream, "image/jpeg");
                        }
                    }
                }
                else
                {
                    return BadRequest("Nenhum arquivo de imagem ou anotações de texto foram enviados.");
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