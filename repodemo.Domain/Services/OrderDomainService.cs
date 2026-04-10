namespace repodemo.Domain.Services;

using repodemo.Domain.Entities;
using repodemo.Domain.Specifications;

/// <summary>
/// Domain Service - Xử lý logic kinh doanh Order
/// </summary>
public class OrderDomainService
{
    /// <summary>
    /// Tạo đơn hàng từ giỏ hàng
    /// </summary>
    public OrderAggregate CreateOrderFromCart(Guid buyerId, List<(int VariantId, string ProductName, int Quantity, decimal UnitPrice)> cartItems, string orderAlias)
    {
        // Validate order creation
        var order = new OrderAggregate(buyerId, orderAlias);
        
        foreach (var item in cartItems)
        {
            order.AddItem(item.VariantId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        var (isValid, errorMessage) = OrderBusinessRules.ValidateOrderCreation(order);
        if (!isValid)
            throw new InvalidOperationException(errorMessage);

        return order;
    }

    /// <summary>
    /// Tính lại tổng tiền dựa trên items
    /// </summary>
    public decimal CalculateOrderTotal(List<(int Quantity, decimal UnitPrice)> items)
    {
        return items.Sum(item => item.Quantity * item.UnitPrice);
    }

    /// <summary>
    /// Lấy danh sách lỗi validate cho đơn hàng
    /// </summary>
    public List<string> GetOrderValidationErrors(OrderAggregate order)
    {
        var errors = new List<string>();

        var (isValid, errorMessage) = OrderBusinessRules.ValidateOrderCreation(order);
        if (!isValid)
            errors.Add(errorMessage);

        return errors;
    }

    /// <summary>
    /// Kiểm tra item có hợp lệ cho order hay không
    /// </summary>
    public (bool IsValid, string ErrorMessage) ValidateOrderItem(string productName, int quantity, int availableStock, decimal unitPrice)
    {
        var (quantityValid, quantityError) = OrderBusinessRules.ValidateQuantity(quantity);
        if (!quantityValid)
            return (false, quantityError);

        var (stockValid, stockError) = OrderBusinessRules.ValidateStock(quantity, availableStock, productName);
        if (!stockValid)
            return (false, stockError);

        var (priceValid, priceError) = OrderBusinessRules.ValidatePrice(unitPrice);
        if (!priceValid)
            return (false, priceError);

        return (true, string.Empty);
    }
}
