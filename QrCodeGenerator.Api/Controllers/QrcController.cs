using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace QrCodeGenerator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrcController : ControllerBase
    {
        const string TEXT = "www.google.com";
        private readonly IMongoRepository _mongoRepository;

        public QrcController(IMongoRepository mongoRepository)
        {
            _mongoRepository = mongoRepository;
        }

        /// <summary>
        /// Returns QR Code for the given ?
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetCode([FromQuery]string url)
        {
            Bitmap qrCodeImage = GetCodeForConstantText(url);
            //Bitmap qrCodeImage = GetCodeForHomeWiFi();
            return File(BitmapToBytes(qrCodeImage), "image/jpeg");
        }

        /// <summary>
        /// Takes a JSON from UI and saves it as-is in Mongo.
        /// </summary>
        /// <param name="json">JSON from UI</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody]object json)
        {
            var str = System.Text.Json.JsonSerializer.Serialize(json);
            var newGuid = Guid.NewGuid();
            await _mongoRepository.InsertAsync("Employees", str, newGuid);
            return Ok(newGuid);
        }

        /// <summary>
        /// Fetches fun facts JSON from Mongo for the given ID and return it.
        /// </summary>
        /// <param name="mongoI"></param>
        /// <returns></returns>
        [HttpGet("{mongoId}")]
        public async Task<IActionResult> GetEmployeeFunFacts([FromRoute]Guid mongoId)
        {
            object data = await _mongoRepository.FindByIdAsync<object>(mongoId, "Employees");
            return Ok(data);
        }

        /// <summary>
        /// Returns a random fun question.
        /// </summary>
        /// <returns></returns>
        [HttpGet("FunQuestion")]
        public async Task<IActionResult> GetFunQuestion()
        {
            var result = await _mongoRepository.FindRandom<object>("FunQuestions");
            return Ok(result);
        }

        /// <summary>
        /// Returns a random fun question.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddFunQuestion()
        {
            return Ok(string.Empty);
        }

        private static Bitmap GetCodeForConstantText(string url, int pixelsPerModule = 20)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(pixelsPerModule);
            return qrCodeImage;
        }

        private static Bitmap GetCodeForHomeWiFi(int pixelsPerModule = 20)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            PayloadGenerator.WiFi wifiPayload = new PayloadGenerator.WiFi("Lahore", "69051685", PayloadGenerator.WiFi.Authentication.WPA);
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(wifiPayload.ToString(), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(pixelsPerModule);
            return qrCodeImage;
        }

        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
