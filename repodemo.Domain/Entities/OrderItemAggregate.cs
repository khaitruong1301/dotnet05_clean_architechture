namespace repodemo.Domain.Entities;

/// <summary>
/// Order Item - Chi tiết sản phẩm trong đơn hàng
/// </summary>
public class OrderItemAggregate
{
    public int VariantId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public OrderItemAggregate(int variantId, string productName, int quantity, decimal unitPrice)
    {
        if (variantId <= 0)
            throw new ArgumentException("VariantId must be greater than 0", nameof(variantId));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be empty", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice cannot be negative", nameof(unitPrice));

        VariantId = variantId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Tính tổng giá của item
    /// </summary>
    public decimal GetTotal() => Quantity * UnitPrice;

    /// <summary>
    /// Cập nhật số lượng
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(newQuantity));
        Quantity = newQuantity;
    }
}
