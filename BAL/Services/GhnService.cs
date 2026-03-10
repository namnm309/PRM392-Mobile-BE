using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BAL.Services
{
    public interface IGhnService
    {
        Task<List<GhnProvince>> GetProvincesAsync();
        Task<List<GhnDistrict>> GetDistrictsAsync(int provinceId);
        Task<List<GhnWard>> GetWardsAsync(int districtId);
        Task<List<GhnService.GhnAvailableService>> GetAvailableServicesAsync(int toDistrictId);
        Task<GhnFeeResponse> CalculateShippingFeeAsync(GhnCalculateFeeRequest request);
        Task<GhnCreateOrderResponse> CreateShippingOrderAsync(GhnCreateOrderRequest request);
        Task<GhnResolvedCodes?> ResolveGhnCodesAsync(string city, string district, string ward);
        Task<GhnOrderDetailResponse> GetOrderDetailAsync(string orderCode);
    }

    public class GhnService : IGhnService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly int _shopId;
        private readonly string _apiUrl;
        private readonly int _fromDistrictId;
        private readonly string _fromWardCode;
        private readonly string _fromName;
        private readonly string _fromPhone;
        private readonly string _fromAddress;
        private readonly string _fromWardName;
        private readonly string _fromDistrictName;
        private readonly string _fromProvinceName;
        private readonly string _returnPhone;
        private readonly string _returnAddress;
        private readonly int? _defaultPickStationId;
        private readonly ILogger<GhnService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };

        public GhnService(HttpClient httpClient, IConfiguration configuration, ILogger<GhnService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _token = configuration["GHN:Token"] ?? throw new InvalidOperationException("GHN:Token is not configured");
            _shopId = int.Parse(configuration["GHN:ShopId"] ?? throw new InvalidOperationException("GHN:ShopId is not configured"));
            _apiUrl = (configuration["GHN:ApiUrl"] ?? "https://dev-online-gateway.ghn.vn").TrimEnd('/');
            _fromDistrictId = int.Parse(configuration["GHN:FromDistrictId"] ?? "1454");
            _fromWardCode = configuration["GHN:FromWardCode"] ?? "21211";
            _fromName = configuration["GHN:FromName"] ?? "TechStore";
            _fromPhone = configuration["GHN:FromPhone"] ?? "0987654321";
            _fromAddress = configuration["GHN:FromAddress"] ?? "";
            _fromWardName = configuration["GHN:FromWardName"] ?? "";
            _fromDistrictName = configuration["GHN:FromDistrictName"] ?? "";
            _fromProvinceName = configuration["GHN:FromProvinceName"] ?? "";
            _returnPhone = configuration["GHN:ReturnPhone"] ?? _fromPhone;
            _returnAddress = configuration["GHN:ReturnAddress"] ?? _fromAddress;
            _defaultPickStationId = int.TryParse(configuration["GHN:PickStationId"], out var ps) ? ps : null;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, object? body = null)
        {
            var request = new HttpRequestMessage(method, $"{_apiUrl}{endpoint}");
            request.Headers.Add("Token", _token);
            request.Headers.Add("ShopId", _shopId.ToString());
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }
            return request;
        }

        private async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GHN API error: {StatusCode} - {Content}", response.StatusCode, content);
                throw new InvalidOperationException($"GHN API error: {content}");
            }

            var result = JsonSerializer.Deserialize<GhnApiResponse<T>>(content, JsonOptions);
            if (result == null || result.Code != 200)
            {
                throw new InvalidOperationException($"GHN API error: {result?.Message ?? content}");
            }

            return result.Data!;
        }

        public async Task<List<GhnProvince>> GetProvincesAsync()
        {
            var request = CreateRequest(HttpMethod.Post, "/shiip/public-api/master-data/province");
            return await SendAsync<List<GhnProvince>>(request);
        }

        public async Task<List<GhnDistrict>> GetDistrictsAsync(int provinceId)
        {
            var request = CreateRequest(HttpMethod.Post, "/shiip/public-api/master-data/district", new { province_id = provinceId });
            return await SendAsync<List<GhnDistrict>>(request);
        }

        public async Task<List<GhnWard>> GetWardsAsync(int districtId)
        {
            var request = CreateRequest(
                HttpMethod.Post,
                "/shiip/public-api/master-data/ward",
                new { district_id = districtId }
            );
            return await SendAsync<List<GhnWard>>(request);
        }

        public async Task<List<GhnAvailableService>> GetAvailableServicesAsync(int toDistrictId)
        {
            var request = CreateRequest(HttpMethod.Post, "/shiip/public-api/v2/shipping-order/available-services", new
            {
                shop_id = _shopId,
                from_district = _fromDistrictId,
                to_district = toDistrictId
            });
            return await SendAsync<List<GhnAvailableService>>(request);
        }

        public class GhnAvailableService
        {
            [JsonPropertyName("service_id")]
            public int ServiceId { get; set; }
            [JsonPropertyName("short_name")]
            public string ShortName { get; set; } = "";
            [JsonPropertyName("service_type_id")]
            public int ServiceTypeId { get; set; }
        }

        public async Task<GhnFeeResponse> CalculateShippingFeeAsync(GhnCalculateFeeRequest req)
        {
            var body = new
            {
                from_district_id = _fromDistrictId,
                from_ward_code = _fromWardCode,
                service_id = req.ServiceId,
                service_type_id = req.ServiceTypeId ?? 2,
                to_district_id = req.ToDistrictId,
                to_ward_code = req.ToWardCode,
                height = req.Height ?? 10,
                length = req.Length ?? 20,
                weight = req.Weight ?? 500,
                width = req.Width ?? 15,
                insurance_value = req.InsuranceValue ?? 0,
                cod_failed_amount = 0
            };

            var request = CreateRequest(HttpMethod.Post, "/shiip/public-api/v2/shipping-order/fee", body);
            return await SendAsync<GhnFeeResponse>(request);
        }

        public async Task<GhnCreateOrderResponse> CreateShippingOrderAsync(GhnCreateOrderRequest req)
        {
            var body = new Dictionary<string, object?>
            {
                ["payment_type_id"] = req.PaymentTypeId,
                ["note"] = req.Note ?? "",
                ["required_note"] = req.RequiredNote ?? "KHONGCHOXEMHANG",
                ["return_phone"] = string.IsNullOrWhiteSpace(req.ReturnPhone) ? _returnPhone : req.ReturnPhone,
                ["return_address"] = string.IsNullOrWhiteSpace(req.ReturnAddress) ? _returnAddress : req.ReturnAddress,
                ["return_district_id"] = req.ReturnDistrictId,
                ["return_ward_code"] = req.ReturnWardCode ?? "",
                ["client_order_code"] = req.ClientOrderCode ?? "",
                ["from_name"] = string.IsNullOrWhiteSpace(req.FromName) ? _fromName : req.FromName,
                ["from_phone"] = string.IsNullOrWhiteSpace(req.FromPhone) ? _fromPhone : req.FromPhone,
                ["from_address"] = string.IsNullOrWhiteSpace(req.FromAddress) ? _fromAddress : req.FromAddress,
                ["from_ward_name"] = string.IsNullOrWhiteSpace(req.FromWardName) ? _fromWardName : req.FromWardName,
                ["from_district_name"] = string.IsNullOrWhiteSpace(req.FromDistrictName) ? _fromDistrictName : req.FromDistrictName,
                ["from_province_name"] = string.IsNullOrWhiteSpace(req.FromProvinceName) ? _fromProvinceName : req.FromProvinceName,
                ["to_name"] = req.ToName,
                ["to_phone"] = req.ToPhone,
                ["to_address"] = req.ToAddress,
                ["to_ward_name"] = req.ToWardName,
                ["to_district_name"] = req.ToDistrictName,
                ["to_province_name"] = req.ToProvinceName,
                ["to_ward_code"] = req.ToWardCode,
                ["to_district_id"] = req.ToDistrictId,
                ["cod_amount"] = req.CodAmount,
                ["content"] = req.Content ?? "TechStore Order",
                ["length"] = req.Length ?? 12,
                ["width"] = req.Width ?? 12,
                ["height"] = req.Height ?? 12,
                ["weight"] = req.Weight ?? 1200,
                ["cod_failed_amount"] = req.CodFailedAmount ?? 0,
                ["service_id"] = req.ServiceId ?? 0,
                ["insurance_value"] = req.InsuranceValue ?? 0,
                ["service_type_id"] = req.ServiceTypeId ?? 2,
                ["coupon"] = req.Coupon,
                ["items"] = req.Items.Select(i => new
                    {
                        name = i.Name,
                        code = i.Code,
                        quantity = i.Quantity,
                        price = i.Price,
                        length = i.Length,
                        width = i.Width,
                        height = i.Height,
                        weight = i.Weight,
                        category = i.Category == null ? null : new
                        {
                            level1 = i.Category.Level1
                        }
                    })
                    .ToArray()
            };

            var pickStationId = req.PickStationId ?? _defaultPickStationId;
            if (pickStationId.HasValue)
            {
                body["pick_station_id"] = pickStationId.Value;
            }

            if (req.DeliverStationId.HasValue)
            {
                body["deliver_station_id"] = req.DeliverStationId.Value;
            }

            // pick_shift theo curl mẫu GHN (ca lấy hàng)
            if (req.PickShift != null && req.PickShift.Any())
            {
                body["pick_shift"] = req.PickShift;
            }

            // Log payload để debug khi GHN trả lỗi khác với Postman
            string? payloadJson = null;
            try
            {
                payloadJson = JsonSerializer.Serialize(body, JsonOptions);
                _logger.LogInformation("GHN CreateOrder payload: {Payload}", payloadJson);
            }
            catch
            {
                // ignore logging errors
            }

            try
            {
                var request = CreateRequest(HttpMethod.Post, "/shiip/public-api/v2/shipping-order/create", body);
                return await SendAsync<GhnCreateOrderResponse>(request);
            }
            catch (Exception ex)
            {
                // Đính kèm luôn JSON payload vào message để FE có thể thấy request gửi gì
                var msg = $"GHN create order failed. Payload={payloadJson ?? "(null)"}. Error={ex.Message}";
                _logger.LogError(ex, "GHN create order failed with payload {Payload}", payloadJson);
                throw new InvalidOperationException(msg, ex);
            }
        }

        public async Task<GhnResolvedCodes?> ResolveGhnCodesAsync(string city, string district, string ward)
        {
            try
            {
                var provinces = await GetProvincesAsync();
                var matchedProvince = provinces.FirstOrDefault(p =>
                    NormalizeName(p.ProvinceName).Contains(NormalizeName(city)) ||
                    NormalizeName(city).Contains(NormalizeName(p.ProvinceName)));

                if (matchedProvince == null) return null;

                var districts = await GetDistrictsAsync(matchedProvince.ProvinceId);
                var matchedDistrict = districts.FirstOrDefault(d =>
                    NormalizeName(d.DistrictName).Contains(NormalizeName(district)) ||
                    NormalizeName(district).Contains(NormalizeName(d.DistrictName)));

                if (matchedDistrict == null) return null;

                var wards = await GetWardsAsync(matchedDistrict.DistrictId);
                var matchedWard = wards.FirstOrDefault(w =>
                    NormalizeName(w.WardName).Contains(NormalizeName(ward)) ||
                    NormalizeName(ward).Contains(NormalizeName(w.WardName)));

                return new GhnResolvedCodes
                {
                    ProvinceId = matchedProvince.ProvinceId,
                    DistrictId = matchedDistrict.DistrictId,
                    WardCode = matchedWard?.WardCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve GHN codes for {City}/{District}/{Ward}", city, district, ward);
                return null;
            }
        }

        public async Task<GhnOrderDetailResponse> GetOrderDetailAsync(string orderCode)
        {
            var request = CreateRequest(
                HttpMethod.Post,
                "/shiip/public-api/v2/shipping-order/detail",
                new { order_code = orderCode }
            );
            return await SendAsync<GhnOrderDetailResponse>(request);
        }

        private static string NormalizeName(string name)
        {
            return name.ToLowerInvariant()
                .Replace("thành phố", "").Replace("tp.", "").Replace("tp ", "")
                .Replace("tỉnh", "")
                .Replace("quận", "").Replace("huyện", "").Replace("thị xã", "")
                .Replace("phường", "").Replace("xã", "").Replace("thị trấn", "")
                .Trim();
        }
    }

    #region GHN DTOs

    public class GhnApiResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    public class GhnProvince
    {
        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }
        [JsonPropertyName("ProvinceName")]
        public string ProvinceName { get; set; } = "";
        [JsonPropertyName("Code")]
        public string? Code { get; set; }
    }

    public class GhnDistrict
    {
        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }
        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }
        [JsonPropertyName("DistrictName")]
        public string DistrictName { get; set; } = "";
        [JsonPropertyName("Code")]
        public string? Code { get; set; }
        [JsonPropertyName("SupportType")]
        public int SupportType { get; set; }
    }

    public class GhnWard
    {
        [JsonPropertyName("WardCode")]
        public string WardCode { get; set; } = "";
        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }
        [JsonPropertyName("WardName")]
        public string WardName { get; set; } = "";
    }

    public class GhnCalculateFeeRequest
    {
        public int ToDistrictId { get; set; }
        public string ToWardCode { get; set; } = "";
        public int? ServiceId { get; set; }
        public int? ServiceTypeId { get; set; }
        public int? Weight { get; set; }
        public int? Height { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? InsuranceValue { get; set; }
    }

    public class GhnFeeResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("service_fee")]
        public int ServiceFee { get; set; }
        [JsonPropertyName("insurance_fee")]
        public int InsuranceFee { get; set; }
        [JsonPropertyName("cod_fee")]
        public int CodFee { get; set; }
    }

        public class GhnCreateOrderRequest
    {
        public int PaymentTypeId { get; set; } = 2;
        public string? Note { get; set; }
        public string? RequiredNote { get; set; } = "KHONGCHOXEMHANG";
        
        // From fields (optional - sẽ lấy từ config nếu không truyền)
        public string? FromName { get; set; }
        public string? FromPhone { get; set; }
        public string? FromAddress { get; set; }
        public string? FromWardName { get; set; }
        public string? FromDistrictName { get; set; }
        public string? FromProvinceName { get; set; }

        // Return fields (optional)
        public string? ReturnPhone { get; set; }
        public string? ReturnAddress { get; set; }
        public int? ReturnDistrictId { get; set; }
        public string? ReturnWardCode { get; set; }
        
        // To fields (required)
        public string ToName { get; set; } = "";
        public string ToPhone { get; set; } = "";
        public string ToAddress { get; set; } = "";
        public string ToWardName { get; set; } = ""; // REQUIRED by GHN
        public string ToDistrictName { get; set; } = ""; // REQUIRED by GHN
        public string ToProvinceName { get; set; } = ""; // REQUIRED by GHN
        public string ToWardCode { get; set; } = "";
        public int ToDistrictId { get; set; }
        
        public string? ClientOrderCode { get; set; }
        public int CodAmount { get; set; }
        public string? Content { get; set; }
        public int? Weight { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? CodFailedAmount { get; set; }
        public int? PickStationId { get; set; }
        public int? DeliverStationId { get; set; }
        public int? InsuranceValue { get; set; }
        public int? ServiceId { get; set; } = 0;
        public int? ServiceTypeId { get; set; } = 2; // Optional fallback
        public long? PickupTime { get; set; } // Unix timestamp - thời gian lấy hàng mong muốn
            public List<int>? PickShift { get; set; }
        public string? Coupon { get; set; }
        public List<GhnOrderItem> Items { get; set; } = new();
    }

    public class GhnOrderItem
    {
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public GhnItemCategory? Category { get; set; }
    }

    public class GhnItemCategory
    {
        public string? Level1 { get; set; }
    }

    public class GhnCreateOrderResponse
    {
        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; } = "";

        [JsonPropertyName("expected_delivery_time")]
        public DateTime? ExpectedDeliveryTime { get; set; }

        // GHN đôi khi trả total_fee là number, đôi khi là string → dùng JsonElement để tránh lỗi deserialize
        [JsonPropertyName("total_fee")]
        public JsonElement? TotalFee { get; set; }

        [JsonPropertyName("sort_code")]
        public string? SortCode { get; set; }
    }

    public class GhnResolvedCodes
    {
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public string? WardCode { get; set; }
    }

    public class GhnOrderDetailResponse
    {
        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; } = "";
        
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
        
        [JsonPropertyName("log")]
        public List<GhnStatusLog> Log { get; set; } = new();
        
        [JsonPropertyName("expected_delivery_time")]
        public DateTime? ExpectedDeliveryTime { get; set; }
        
        [JsonPropertyName("cod_amount")]
        public decimal CodAmount { get; set; }
        
        [JsonPropertyName("to_name")]
        public string ToName { get; set; } = "";
        
        [JsonPropertyName("to_phone")]
        public string ToPhone { get; set; } = "";
        
        [JsonPropertyName("to_address")]
        public string ToAddress { get; set; } = "";
        
        [JsonPropertyName("client_order_code")]
        public string? ClientOrderCode { get; set; }
    }

    public class GhnStatusLog
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
        
        [JsonPropertyName("updated_date")]
        public DateTime UpdatedDate { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class GhnAvailableService
    {
        [JsonPropertyName("service_id")]
        public int ServiceId { get; set; }
        
        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = "";
        
        [JsonPropertyName("service_type_id")]
        public int ServiceTypeId { get; set; }
    }

    #endregion
}
