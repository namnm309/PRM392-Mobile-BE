using BAL.DTOs.Common;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class ShippingController : ControllerBase
    {
        private readonly IGhnService _ghnService;
        private readonly ILogger<ShippingController> _logger;

        public ShippingController(IGhnService ghnService, ILogger<ShippingController> logger)
        {
            _ghnService = ghnService;
            _logger = logger;
        }

        [HttpGet("provinces")]
        [ProducesResponseType(typeof(ApiResponse<List<GhnProvinceDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<GhnProvinceDto>>>> GetProvinces()
        {
            try
            {
                var provinces = await _ghnService.GetProvincesAsync();
                var result = provinces
                    .Select(p => new GhnProvinceDto
                    {
                        ProvinceId = p.ProvinceId,
                        ProvinceName = p.ProvinceName,
                        Code = p.Code
                    })
                    .ToList();

                return Ok(ApiResponse<List<GhnProvinceDto>>.SuccessResponse(result, "Provinces retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provinces");
                return StatusCode(500, ApiResponse<List<GhnProvince>>.ErrorResponse("Failed to get provinces from GHN"));
            }
        }

        [HttpGet("districts")]
        [ProducesResponseType(typeof(ApiResponse<List<GhnDistrictDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<GhnDistrictDto>>>> GetDistricts([FromQuery] int provinceId)
        {
            try
            {
                var districts = await _ghnService.GetDistrictsAsync(provinceId);
                var result = districts
                    .Select(d => new GhnDistrictDto
                    {
                        DistrictId = d.DistrictId,
                        ProvinceId = d.ProvinceId,
                        DistrictName = d.DistrictName,
                        Code = d.Code,
                        SupportType = d.SupportType
                    })
                    .ToList();

                return Ok(ApiResponse<List<GhnDistrictDto>>.SuccessResponse(result, "Districts retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving districts for province {ProvinceId}", provinceId);
                return StatusCode(500, ApiResponse<List<GhnDistrict>>.ErrorResponse("Failed to get districts from GHN"));
            }
        }

        [HttpGet("wards")]
        [ProducesResponseType(typeof(ApiResponse<List<GhnWardDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<GhnWardDto>>>> GetWards([FromQuery] int districtId)
        {
            try
            {
                var wards = await _ghnService.GetWardsAsync(districtId);
                var result = wards
                    .Select(w => new GhnWardDto
                    {
                        WardCode = w.WardCode,
                        DistrictId = w.DistrictId,
                        WardName = w.WardName
                    })
                    .ToList();

                return Ok(ApiResponse<List<GhnWardDto>>.SuccessResponse(result, "Wards retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wards for district {DistrictId}", districtId);
                return StatusCode(500, ApiResponse<List<GhnWard>>.ErrorResponse("Failed to get wards from GHN"));
            }
        }

        [HttpGet("services")]
        [ProducesResponseType(typeof(ApiResponse<List<GhnService.GhnAvailableService>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<GhnService.GhnAvailableService>>>> GetServices([FromQuery] int toDistrictId)
        {
            try
            {
                var services = await _ghnService.GetAvailableServicesAsync(toDistrictId);
                return Ok(ApiResponse<List<GhnService.GhnAvailableService>>.SuccessResponse(services, "Services retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for district {DistrictId}", toDistrictId);
                return StatusCode(500, ApiResponse<List<GhnService.GhnAvailableService>>.ErrorResponse("Failed to get services from GHN"));
            }
        }

        [HttpPost("calculate-fee")]
        [ProducesResponseType(typeof(ApiResponse<GhnFeeResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<GhnFeeResponse>>> CalculateFee([FromBody] GhnCalculateFeeRequest request)
        {
            try
            {
                if (request.ToDistrictId <= 0 || string.IsNullOrWhiteSpace(request.ToWardCode))
                {
                    return BadRequest(ApiResponse<GhnFeeResponse>.ErrorResponse("ToDistrictId and ToWardCode are required"));
                }

                var fee = await _ghnService.CalculateShippingFeeAsync(request);
                return Ok(ApiResponse<GhnFeeResponse>.SuccessResponse(fee, "Shipping fee calculated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating shipping fee");
                return StatusCode(500, ApiResponse<GhnFeeResponse>.ErrorResponse("Failed to calculate shipping fee"));
            }
        }

        [HttpPost("resolve-codes")]
        [ProducesResponseType(typeof(ApiResponse<GhnResolvedCodes>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<GhnResolvedCodes>>> ResolveCodes([FromBody] ResolveCodesRequest request)
        {
            try
            {
                var codes = await _ghnService.ResolveGhnCodesAsync(request.City, request.District, request.Ward);
                if (codes == null)
                {
                    return NotFound(ApiResponse<GhnResolvedCodes>.ErrorResponse("Could not resolve GHN codes for the given address"));
                }

                return Ok(ApiResponse<GhnResolvedCodes>.SuccessResponse(codes, "GHN codes resolved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving GHN codes");
                return StatusCode(500, ApiResponse<GhnResolvedCodes>.ErrorResponse("Failed to resolve GHN codes"));
            }
        }
    }

    public class ResolveCodesRequest
    {
        public string City { get; set; } = "";
        public string District { get; set; } = "";
        public string Ward { get; set; } = "";
    }
}
