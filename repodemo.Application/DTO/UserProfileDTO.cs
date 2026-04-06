public class UserProfileDTO
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string? Address { get; set; }
    //Lịch sử mua hàng, lịch sử thanh toán,...

    public List<OrderDTO> lstOrder {get;set;} = new List<OrderDTO>();


}

public class OrderDTO
{
    public int Id {get;set;} = 0;
    public string Alias{get;set;} = "";
    public DateTime? CreateAt {get;set;} //Ngày đặt
    public decimal TotalAmount {get;set;} //Tổng tiền
    public List<OrderItemDTO> lstItem {get;set;} = new List<OrderItemDTO>();
}

public class OrderItemDTO {
    public int? OrderId {get;set;}
    public string Alias {get;set;}
    public int? VariantId{get;set;}
    public string? VariantName {get;set;}
    public string? UrlImageVariant {get;set;}
    public int? Quantity {get;set;}
    public decimal? UnitPrice {get;set;}
    //Thêm stattus order mà không cần cập nhật lại entity model (repository)


 }