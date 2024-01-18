using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml;
using SignalRNetCore.Models;
using System.Diagnostics;

namespace SignalRNetCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<ExcelHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, IHubContext<ExcelHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UploadExcel()
        {
                        
                 int realRow = 1;
           
                var file = HttpContext.Request.Form.Files[0];

                var filePath = Path.GetTempFileName();
                var progressMessage = string.Empty;
              
                List<Person> people = new();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var allSheets = package.Workbook.Worksheets;
                        var rowCount =  worksheet.Dimension.Rows;

                        

                        for (int row = 2; row <= rowCount; row++) // Assuming the first row is the header
                        {

                            try
                            {
                                var firstName = Convert.ToString(worksheet.Cells[row, 2].Value);
                                var lastName = Convert.ToString(worksheet.Cells[row, 3].Value);
                                var gender = Convert.ToString(worksheet.Cells[row, 4].Value);
                                var country = Convert.ToString(worksheet.Cells[row, 5].Value);

                                if (string.IsNullOrWhiteSpace(firstName))
                                {
                                    progressMessage = $"<p style='color:red !important'>Row {realRow} cannot be processed</p>";
                                    await _hubContext.Clients.All.SendAsync("ProgressChanged", progressMessage);
                                    realRow++;
                                    continue;
                                }

                                people.Add(new Person
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    Gender = gender,
                                    Country = country,
                                });

                                // Simulate some processing delay
                                await Task.Delay(500);

                                // Send progress update to clients
                                progressMessage = $"<p style='color:white'>Processing row {realRow} of {rowCount - 1} - {firstName} {lastName} {gender} {country}</p>";
                                realRow++;
                                await _hubContext.Clients.All.SendAsync("ProgressChanged", progressMessage);
                            }
                            catch (Exception ex)
                            {
                                progressMessage = $"<p style='color:red !important'>Row {realRow} cannot be processed. {ex.Message}</p>";
                                await _hubContext.Clients.All.SendAsync("ProgressChanged", progressMessage);
                                realRow++;
                                continue;
                            }
                            

                        }
                        await _hubContext.Clients.All.SendAsync("ProgressChanged", "<p>Upload finshed</p>");


                    }
                }
               

            return Ok("Upload completed");

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
