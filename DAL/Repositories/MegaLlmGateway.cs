//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;

//namespace DAL.Repositories
//{
//    public class MegaLlmGateway
//    {
//        private readonly HttpClient _http;
//        private readonly string _apiKey;
//        private readonly string _model;

//        public MegaLlmGateway(HttpClient http, IConfiguration config)
//        {
//            _http = http;

//            _apiKey = Environment.GetEnvironmentVariable("MEGALLM_API_KEY")
//                     ?? throw new InvalidOperationException("Missing MEGALLM_API_KEY env var");

//            var baseUrl = config["MegaLLM:BaseUrl"] ?? "https://ai.megallm.io/v1";
//            _http.BaseAddress = new Uri(baseUrl);

//            _model = config["MegaLLM:Model"] ?? "gpt-4o-mini";
//        }

//        public async Task<string> AskAsync(List<object> messages, CancellationToken ct = default)
//        {
//            var payload = new
//            {
//                model = _model,
//                messages = messages,
//                temperature = 0.6,
//                max_tokens = 400
//            };

//            using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
//            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
//            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

//            using var res = await _http.SendAsync(req, ct);
//            var body = await res.Content.ReadAsStringAsync(ct);

//            if (!res.IsSuccessStatusCode)
//                throw new Exception($"MegaLLM error {(int)res.StatusCode}: {body}");

//            using var doc = JsonDocument.Parse(body);
//            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
//        }
//    }
//}
