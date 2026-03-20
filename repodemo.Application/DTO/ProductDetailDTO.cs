public class ProductDetailDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal PriceDefault { get; set; }
    public string ImageUrl { get; set; }
    public string AdditionalData { get;set;}
    public List<ProductImageDTO> ProductImages { get; set; } = new List<ProductImageDTO>();
    public List<ProductVariantDTO> ProductVariants { get; set; } = new List<ProductVariantDTO>();

}

public class ProductVariantDTO
{
    public int Id { get; set; }
    public string Alias { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ProductVariantImage {get;set;}

}

public class ProductImageDTO
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
}