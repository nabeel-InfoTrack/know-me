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
        public IActionResult GetCode([FromQuery] string url)
        {
            Bitmap qrCodeImage = GetCodeForConstantText(url);
            return File(BitmapToBytes(qrCodeImage), "image/jpeg");
        }

        /// <summary>
        /// Takes a JSON from UI and saves it as-is in Mongo.
        /// </summary>
        /// <param name="json">JSON from UI</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] object json)
        {
            var str = System.Text.Json.JsonSerializer.Serialize(json);
            var newGuid = Guid.NewGuid();
            await _mongoRepository.InsertAsync("Employees", str, newGuid);

            Bitmap qrCodeImage = GetCodeForConstantText($"https://localhost:44308/api/qrc/{newGuid}");
            return File(BitmapToBytes(qrCodeImage), "image/jpeg");

            //return Ok(newGuid);
        }

        /// <summary>
        /// Fetches fun facts JSON from Mongo for the given ID and return it.
        /// </summary>
        /// <param name="mongoI"></param>
        /// <returns></returns>
        [HttpGet("{mongoId}")]
        public async Task<IActionResult> GetEmployeeFunFacts([FromRoute] Guid mongoId)
        {
            object data = await _mongoRepository.FindByIdAsync<object>(mongoId, "Employees");
            return Ok(data);
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            object data = await _mongoRepository.FindAllAsync<object>("Employees");
            return Ok(data);
        }

        /// <summary>
        /// Returns a random fun question.
        /// </summary>
        /// <returns></returns>
        [HttpGet("FunQuestion")]
        public async Task<IActionResult> GetFunQuestion()
        {
            var result = await _mongoRepository.FindRandom<QuestionDto>("FunQuestions");
            return Ok(result);
        }

        /// <summary>
        /// Returns a random fun question.
        /// </summary>
        /// <returns></returns>
        [HttpPost("funQuestion")]
        public async Task<IActionResult> AddFunQuestion([FromBody] QuestionDto questionDto)
        {
            var str = System.Text.Json.JsonSerializer.Serialize(questionDto);
            await _mongoRepository.InsertAsync("FunQuestions", str);
            return Ok();
        }

        private static Bitmap GetCodeForConstantText(string url, int pixelsPerModule = 20)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(pixelsPerModule);
            return qrCodeImage;
        }

        private static byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
