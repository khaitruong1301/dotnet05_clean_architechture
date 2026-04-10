namespace repodemo.Domain.Entities;

/// <summary>
/// Order Aggregate Root - Đơn hàng
/// </summary>
public class OrderAggregate
{
    public int Id { get; private set; }
    public Guid BuyerId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public string? Alias { get; private set; }
    public string? AdditionalData { get; private set; }
    public bool? Deleted { get; private set; }
    
    private readonly List<OrderItemAggregate> _items = new();
    public IReadOnlyList<OrderItemAggregate> Items => _items.AsReadOnly();

    // Constructor cho tạo mới
    public OrderAggregate(Guid buyerId, string alias)
    {
        if (buyerId == Guid.Empty)
            throw new ArgumentException("BuyerId cannot be empty", nameof(buyerId));
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException("Alias cannot be empty", nameof(alias));

        BuyerId = buyerId;
        Alias = alias;
        TotalAmount = 0;
        CreatedAt = DateTime.Now;
        Deleted = false;
    }

    // Constructor cho load từ DB
    private OrderAggregate() { }

    /// <summary>
    /// Thêm item vào đơn hàng
    /// </summary>
    public void AddItem(int productVariantId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0");

        if (unitPrice < 0)
            throw new InvalidOperationException("Price cannot be negative");

        var item = new OrderItemAggregate(productVariantId, productName, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotal();
    }

    /// <summary>
    /// Xóa item khỏi đơn hàng
    /// </summary>
    public void RemoveItem(int variantId)
    {
        var item = _items.FirstOrDefault(x => x.VariantId == variantId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotal();
        }
    }

    /// <summary>
    /// Cập nhật số lượng item
    /// </summary>
    public void UpdateItemQuantity(int variantId, int newQuantity)
    {
        if (newQuantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than 0");

        var item = _items.FirstOrDefault(x => x.VariantId == variantId);
        if (item != null)
        {
            item.UpdateQuantity(newQuantity);
            RecalculateTotal();
        }
    }

    /// <summary>
    /// Tính lại tổng tiền đơn hàng
    /// </summary>
    public void RecalculateTotal()
    {
        TotalAmount = _items.Sum(x => x.GetTotal());
    }

    /// <summary>
    /// Kiểm tra đơn hàng có hợp lệ hay không
    /// </summary>
    public bool IsValid()
    {
        return BuyerId != Guid.Empty 
            && !string.IsNullOrWhiteSpace(Alias)
            && _items.Count > 0
            && TotalAmount > 0;
    }

    /// <summary>
    /// Lấy số lượng items trong đơn hàng
    /// </summary>
    public int GetItemCount() => _items.Count;

    /// <summary>
    /// Xóa tất cả items
    /// </summary>
    public void ClearItems()
    {
        _items.Clear();
        TotalAmount = 0;
    }
}
