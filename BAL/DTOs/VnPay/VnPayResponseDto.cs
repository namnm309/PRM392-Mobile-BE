namespace BAL.DTOs.VnPay
{
    public class VnPayResponseDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
    }

    public class VnPayIpnResponseDto
    {
        public string RspCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
