using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using repodemo.Infrastructure.Models;

[Authorize]
public class CartHub : Hub
{
    private readonly IHubContext<CartHub> _hubContext;
    private readonly ICartService _cartService;
    private readonly CybersoftMarketplaceContext _content;
    private readonly JwtService _jwtService;


    public CartHub(IHubContext<CartHub> hubContext, ICartService cartService, CybersoftMarketplaceContext content, JwtService jwtService)
    {
        _hubContext = hubContext;
        _cartService = cartService;
        _content = content;
        _jwtService = jwtService;
    }

    //connect
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($@"connected");
        //Khi client connect tới signalR server thì lấy userId từ url query của client (client sẽ gửi lên khi kết nối signalR) => tạo group theo userId đó => sau này dựa vào userId này để bắn dữ liệu về đúng client đang lắng nghe với userId đó

        string token = Context.GetHttpContext()?.Request.Query["access_token"].FirstOrDefault() ?? string.Empty;

        //Decode token lấy userId
        string userId = _jwtService.DecodePayloadToken(token);
        await Groups.AddToGroupAsync(Context.ConnectionId, $@"userId_{userId}");



        //Khi connect đến cho lấy giỏ hàng
        var res = await _cartService.GetCartByUserId(token);

        await Clients.Caller.SendAsync("ReceiveCartData", res.data);

        //Bắn dữ liệu cho client với userid group tương ứng
        await base.OnConnectedAsync();

    }


    //Viết hàm update data 
    public async Task updateCartData(ItemUpdateToCartDTO item)
    {
        //Lấy token từ request
        string token = Context.GetHttpContext()?.Request.Query["access_token"].FirstOrDefault() ?? string.Empty;

        //Decode token lấy userId
        string userId = _jwtService.DecodePayloadToken(token);

        var res = await _cartService.UpdateCartItem(token, item);

        //Bắn dữ liệu cho group tương ứng
        await _hubContext.Clients.Group($@"userId_{userId}").SendAsync("ReceiveCartData", res.data);
    }


    //hàm dựa vào userId (token) để truy vấn db lấy dữ liệu trả về client đang lắng nghe với userid đó tương ứng
    public async Task getCartByUserId()
    {
        //Lấy token từ request

        string token = Context.GetHttpContext()?.Request.Query["access_token"].FirstOrDefault() ?? string.Empty;

        //Decode token lấy userId
        string userId = _jwtService.DecodePayloadToken(token);

        var res = await _cartService.GetCartByUserId(token);

        //Bắn dữ liệu cho group tương ứng

        await _hubContext.Clients.Group($@"userId_{userId}").SendAsync("ReceiveCartData", res.data);

    }




    //disconnect
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

    }
}