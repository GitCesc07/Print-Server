using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using Spire.Pdf;
using Spire.Pdf.Print;

namespace PrintServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PrinterController : ControllerBase
    {
        private readonly ILogger<PrinterController> _logger;
        public PrinterController(ILogger<PrinterController> logger)
        {
            _logger = logger;
        }

        // Al realizar la petisión al enpoint, sería /Printer/printerList
        // Ya que lo que se esta listando son la impresoras.
        [HttpGet("printerList")]
        public IActionResult GetPrinters()
        {
            try
            {
                // Obtener las lista de impresoras instaladas.
                var printerList = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
                if (printerList.Count == 0)
                {
                    return NotFound("No se encontraron impresoras instaladas...");
                }

                return Ok(printerList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de impresoras...");
                return StatusCode(500, "Error interno del servidor  al obtener las impresoras...");
            }
        }

        [HttpPost("print-ticket")]
        public IActionResult PrinTicket(IFormFile file, [FromForm] string printerName)
        {
            try
            {
                // Validando que exista el archivo.
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No se ha proporcionado ningún archivo...");
                }

                // Validación que tenga nombre de la impresora a usar
                if (string.IsNullOrEmpty(printerName))
                {
                    return BadRequest("No se ingreso el nombre de la impresora...");
                }

                // Guardar temporalmente el archivo.
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Carga el pdf
                var pdfDocument = new PdfDocument();
                pdfDocument.LoadFromFile(tempFilePath);

                //Configutrtación de la impresora.
                pdfDocument.PrintSettings.PrinterName = printerName;
                pdfDocument.PrintSettings.SelectPageRange(1, pdfDocument.Pages.Count);
                //pdfDocument.PrintSettings.SelectSinglePageLayout(Spire.Pdf.Print.PdfSinglePageScalingMode.FitSize);
                pdfDocument.PrintSettings.PaperSize = PdfPaperSize.Statement;
                pdfDocument.PrintSettings.DocumentName = "Ticket Print";

                // Realizar impresión.
                pdfDocument.Print();
                
                // Liberar recursos
                pdfDocument.Close();

                // Eliminar archivo temporal
                System.IO.File.Delete(tempFilePath);
                return Ok("Impresión realizada correctamente...");
            }
            catch (Exception ex)
            {                
                return StatusCode(500, $"Error al imprimir el PDF: {ex.Message}");
                throw;
            }
        }
    }
}
