using repodemo.Infrastructure.Models;

public class CartDTO
{
    public int CartId { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
    public decimal TotalPrice { get; set; }
}


public class CartItemDTO
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ProductVariantImage {get;set;}
    public int Quantity { get; set; }

}


public class ItemAddToCartDTO
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }=1;
}


public class ItemUpdateToCartDTO
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }=1;
}