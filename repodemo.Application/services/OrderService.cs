//Xây dựng nghiệp vụ lưu đơn hàng



using repodemo.Infrastructure.Models;
public interface IOrderService
{
    Task<ResponseData<OrderDTO>> AddOrder(string token, CartDTO cartDTO);




}

public class OrderService : IOrderService
{
    private readonly UserRepository _userRepository;
    private readonly OrderRepository _orderRepository;
    private readonly OrderItemRepository _orderItemRepository;
    private readonly CartRepository _cartRepository;
    private readonly CartItemRepository _cartItemRepository;
    private readonly UnitOfWork _unitOfWork;
    private readonly JwtService _jwtService;
    public OrderService(UserRepository userRepository, OrderRepository orderRepository, JwtService jwtService, CartRepository cartRepository, CartItemRepository cartItemRepository, OrderItemRepository orderItemRepository, UnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _jwtService = jwtService;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _orderItemRepository = orderItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseData<OrderDTO>> AddOrder(string token, CartDTO cartDTO)
    {
        try
        {
            string userID = _jwtService.DecodePayloadToken(token);
            //token không hợp lệ
            if (string.IsNullOrEmpty(userID))
            {
                return new ResponseData<OrderDTO>
                {
                    statusCode = 401,
                    data = null,
                    message = "Unauthorized",
                    dateTime = DateTime.Now
                };
            }
            //Kiểm tra giỏ hàng gửi lên đúng của user hay không
            Cart? cart = await _cartRepository.SingleOrDefaultAsync(c => c.UserId.ToString() == userID && c.Id == cartDTO.CartId);
            if(cart == null)
            {
                return new ResponseData<OrderDTO>
                {
                    statusCode = 400,
                    data = null,
                    message = "Giỏ hàng không hợp lệ",
                    dateTime = DateTime.Now
                };
            }
            //Đem giỏ hàng tạo thành order 
    //             public int Id { get; set; }

    // public Guid BuyerId { get; set; }

    // public decimal TotalAmount { get; set; }

    // public DateTime? CreatedAt { get; set; }

    // public string? Alias { get; set; }

    // public string? AdditionalData { get; set; }

    // public bool? Deleted { get; set; }

            var order = new Order
            {
                BuyerId = Guid.Parse(userID),
                CreatedAt = DateTime.Now,
                TotalAmount = cartDTO.Items.Sum(item => item.Quantity),
                // string.Join("-", items.Select(x => x.name)
                Alias = FunctionUtility.GenerateSlug($@"order-{userID}-{FunctionUtility.GenerateSlug(string.Join("-", cartDTO.Items.Select(item => item.Name).ToList() ?? new List<string> { "unknown" }))}-{DateTime.Now.Ticks}"),
                AdditionalData = null,
                Deleted = false


            };

            //Kỹ phải tra từng order item gửi lên có hợp lệ hay không (variantId có tồn tại hay không, quantity có lớn hơn 0 hay không) và kiểm tra stock
                foreach (var item in cartDTO.Items)
                {
                    var cartItem = await _cartItemRepository.SingleOrDefaultAsync(ci => ci.CartId == cartDTO.CartId && ci.VariantId == item.ProductVariantId);
                    if(cartItem == null)
                    {
                        return new ResponseData<OrderDTO>
                        {
                            statusCode = 400,
                            data = null,
                            message = $"Sản phẩm {item.Name} không tồn tại trong giỏ hàng",
                            dateTime = DateTime.Now
                        };
                    }
                    if(item.Quantity <= 0)
                    {
                        return new ResponseData<OrderDTO>
                        {
                            statusCode = 400,
                            data = null,
                            message = $"Số lượng sản phẩm {item.Name} phải lớn hơn 0",
                            dateTime = DateTime.Now
                        };
                    }
                    if(item.Quantity > cartItem.Variant.Stock)
                    {
                         return new ResponseData<OrderDTO>
                         {
                             statusCode = 400,
                             data = null,
                             message = $"Số lượng sản phẩm {item.Name} vượt quá số lượng trong kho",
                             dateTime = DateTime.Now
                         };
                    }
                    {
                        return new ResponseData<OrderDTO>
                        {
                            statusCode = 400,
                            data = null,
                            message = $"Số lượng sản phẩm {item.Name} vượt quá số lượng trong kho",
                            dateTime = DateTime.Now
                        };
                    }
                }

    //              public int OrderId { get; set; }

    // public int VariantId { get; set; }

    // public int Quantity { get; set; }

    // public decimal UnitPrice { get; set; }

    // public string? Alias { get; set; }

    // public string? AdditionalData { get; set; }

    // public bool? Deleted { get; set; }


                //Thêm item giỏ hàng vao orderitem
                order.OrderItems = cartDTO.Items.Select(item => new OrderItem
                {
                    VariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Alias = FunctionUtility.GenerateSlug(@$"orderitem-{userID}-{FunctionUtility.GenerateSlug(item.Name)}-{DateTime.Now.Ticks}"),
                    AdditionalData = null,
                    Deleted = false,
                }).ToList();

                //Xoá item trong giỏ hàng
                foreach(CartItem item in cart.CartItems)
                {
                    await _cartItemRepository.RemoveAsync(item);
                }


                //Xoá giỏ hàng sau khi tạo đơn hàng thành công
                await _cartRepository.RemoveAsync(cart);

                await _orderRepository.AddAsync(order); //add order order item tự add



  
            await _orderRepository.AddAsync(order);

            await _unitOfWork.SaveChangesAsync();
            
            return new ResponseData<OrderDTO>
            {
                statusCode = 200,
                data = new OrderDTO () {
                    Id = order.Id,
                    CreateAt = order.CreatedAt,
                    TotalAmount = order.TotalAmount,
                    Alias = order.Alias,
                    lstItem = order.OrderItems.Select(oi => new OrderItemDTO
                    {
                        OrderId = oi.OrderId,
                        VariantId = oi.VariantId,
                        VariantName = oi.Variant.VariantName,
                        UrlImageVariant = oi.Variant.Image,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Alias = oi.Alias
                    }).ToList()
                },
                message = "Thêm đơn hàng thành công",
                dateTime = DateTime.Now
            };

// public class OrderItemDTO {
//     public int? OrderId {get;set;}
//     public int? VariantId{get;set;}
//     public string? VariantName {get;set;}
//     public string? UrlImageVariant {get;set;}
//     public int? Quantity {get;set;}
//     public decimal? UnitPrice {get;set;}
//     //Thêm stattus order mà không cần cập nhật lại entity model (repository)




        }
        catch (Exception ex)
        {
            return await Task.Run(() => new ResponseData<OrderDTO>
            {
                statusCode = 500,
                data = null,
                message = $"Lỗi khi thêm đơn hàng: {ex.Message}",
                dateTime = DateTime.Now
            });
        }

    }
}