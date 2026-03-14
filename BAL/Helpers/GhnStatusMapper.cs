namespace BAL.Helpers
{
    /// <summary>
    /// Helper class để map GHN shipping status sang internal order status
    /// </summary>
    public static class GhnStatusMapper
    {
        /// <summary>
        /// Map GHN shipping status to internal order status
        /// Dựa theo docs GHN: https://api.ghn.vn/home/docs/detail?id=84
        /// </summary>
        /// <param name="ghnStatus">Trạng thái từ GHN API</param>
        /// <returns>Trạng thái internal của Order</returns>
        public static string MapGhnStatusToOrderStatus(string ghnStatus)
        {
            return ghnStatus?.ToLower() switch
            {
                "ready_to_pick" => "Confirmed",      // Chờ lấy hàng
                "picking" => "Shipping",              // Đang lấy hàng
                "picked" => "Shipping",               // Đã lấy hàng
                "storing" => "Shipping",              // Đang lưu kho
                "transporting" => "Shipping",         // Đang vận chuyển
                "sorting" => "Shipping",              // Đang phân loại
                "delivering" => "Shipping",           // Đang giao hàng
                "delivered" => "Delivered",           // Đã giao hàng
                "delivery_fail" => "Shipping",        // Giao hàng thất bại (thử lại)
                "waiting_to_return" => "Shipping",    // Chờ trả hàng
                "return" => "Cancelled",              // Đang trả hàng
                "returned" => "Cancelled",            // Đã trả hàng
                "cancel" => "Cancelled",              // Đơn bị hủy
                "exception" => "Shipping",            // Đơn exception (thử lại)
                "lost" => "Cancelled",                // Đơn bị thất lạc
                "damage" => "Shipping",               // Hàng bị hư hỏng (thử lại)
                _ => "Shipping"                       // Default cho các status chưa biết
            };
        }
    }
}
