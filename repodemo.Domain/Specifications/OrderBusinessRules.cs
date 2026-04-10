namespace repodemo.Domain.Specifications;

using repodemo.Domain.Entities;

/// <summary>
/// Business Rules cho Order - Quy tắc kinh doanh đơn hàng
/// </summary>
public class OrderBusinessRules
{
    /// <summary>
    /// Kiểm tra giỏ hàng có hợp lệ để tạo đơn hàng hay không
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateOrderCreation(OrderAggregate order)
    {
        if (order.BuyerId == Guid.Empty)
            return (false, "Invalid buyer");

        if (string.IsNullOrWhiteSpace(order.Alias))
            return (false, "Order alias is required");

        if (order.GetItemCount() == 0)
            return (false, "Order must contain at least one item");

        if (order.TotalAmount <= 0)
            return (false, "Order total amount must be greater than 0");

        return (true, string.Empty);
    }

    /// <summary>
    /// Kiểm tra số lượng hàng có hợp lệ hay không
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            return (false, "Số lượng sản phẩm phải lớn hơn 0");

        return (true, string.Empty);
    }

    /// <summary>
    /// Kiểm tra số lượng hàng có vượt quá stock hay không
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateStock(int quantity, int availableStock, string productName)
    {
        if (quantity > availableStock)
            return (false, $"Số lượng sản phẩm {productName} vượt quá số lượng trong kho. Còn lại: {availableStock}");

        return (true, string.Empty);
    }

    /// <summary>
    /// Kiểm tra giá có hợp lệ hay không
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidatePrice(decimal price)
    {
        if (price < 0)
            return (false, "Price cannot be negative");

        return (true, string.Empty);
    }

    /// <summary>
    /// Kiểm tra số lượng items trong giỏ hàng có hợp lệ hay không
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateItemCount(int itemCount)
    {
        if (itemCount == 0)
            return (false, "Giỏ hàng trống, không thể tạo đơn hàng");

        return (true, string.Empty);
    }
}
