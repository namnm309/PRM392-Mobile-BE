using BAL.Services;

namespace BAL.DTOs.Common
{
    public class GhnProvinceDto
    {
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public string? Code { get; set; }
    }

    public class GhnDistrictDto
    {
        public int DistrictId { get; set; }
        public int ProvinceId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public int SupportType { get; set; }
    }

    public class GhnWardDto
    {
        public string WardCode { get; set; } = string.Empty;
        public int DistrictId { get; set; }
        public string WardName { get; set; } = string.Empty;
    }
}

