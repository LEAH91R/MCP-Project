using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")] // הפעלת הגבלת הקצב משלב 7
    public class GeminiController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public GeminiController()
        {
            _httpClient = new HttpClient();
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] string userPrompt)
        {
            // שליפת המפתח שנטען מה-.env
            string apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest("API Key is missing. Check your .env file.");
            }

            // הוספת הנחיה סמויה שהתשובה תמיד תהיה בעברית
            string customPrompt = $"ענה על השאלה הבאה בעברית בלבד: {userPrompt}";

            // כתובת ה-API עם המודל שנמצא תקין בבדיקה שלך
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            // בניית גוף הבקשה בפורמט של גוגל
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = customPrompt } } }
                }
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, result);
                }

                // חילוץ הטקסט הנקי מתוך מבנה ה-JSON
                using var jsonDoc = JsonDocument.Parse(result);
                var chatResponse = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return Ok(chatResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }
    }
}