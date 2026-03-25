public class ProductPagingDTO
{
    public int Id { get; set; }

    public int ShopId { get; set; }
    public string ShopName { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Alias { get; set; }

    public string? AdditionalData { get; set; }

    public bool? Deleted { get; set; }

    public string? Image { get; set; }

    public decimal DisplayPrice { get; set; }

}