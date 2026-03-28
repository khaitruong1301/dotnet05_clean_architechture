using Azure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using repodemo.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

public interface ICartService
{
    Task<ResponseData<CartDTO>> GetCartByUserId(string token);
    Task<ResponseData<CartDTO>> AddToCart(string token, ItemAddToCartDTO model);
    Task<ResponseData<CartDTO>> RemoveFromCart(string token,int variantId);
    Task<ResponseData<CartDTO>> UpdateCartItem(string token, ItemUpdateToCartDTO cartItem);
}


public class CartService : ICartService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly CartRepository _cartRepository;
    private readonly CartItemRepository _cartItemRepository;
    private readonly ProductVariantRepository _productVariantRepository;
    private readonly JwtService _jwtService;

    public CartService(UnitOfWork unitOfWork, CartRepository cartRepository, CartItemRepository cartItemRepository, ProductVariantRepository productVariantRepository, JwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productVariantRepository = productVariantRepository;
        _jwtService = jwtService;
    }

    public async Task<ResponseData<CartDTO>> GetCartByUserId(string token)
    {

        string userID = _jwtService.DecodePayloadToken(token); 
        //token không hợp lệ
        if(string.IsNullOrEmpty(userID))
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 401,
                data = null,
                message = "Unauthorized",
                dateTime = DateTime.Now
            };
        }
        //Dựa vào token lấy ra giỏ hàng trong database
        Cart? cart  =  _cartRepository.WhereAsync(c => c.UserId == Guid.Parse(userID)).Result.FirstOrDefault();
        //Map dữ liệu giỏ hàng lên dto trả về frontend
        if(cart == null)
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = new CartDTO
                {
                    UserId = Guid.Parse(userID),
                    Items = new List<CartItemDTO>()
                },
                message = "Cart not found",
                dateTime = DateTime.Now
            };
        }

        //Lấy ra các cart item 
        List<CartItemDTO> cartItems = cart.CartItems.Select(ci => new CartItemDTO
        {
            Id = ci.CartId,
            ProductVariantId = ci.VariantId,
            Quantity = ci.Quantity,
            ProductVariantImage = ci.Variant.Product.Image,
            Name = ci.Variant.VariantName,
            Price = ci.Variant.Price,
            Stock = ci.Variant.Stock
        }).ToList();

        CartDTO cartDTO = new CartDTO
        {
            CartId = cart.Id,
            UserId = Guid.Parse(userID),
            Items = cartItems,
            TotalPrice = cartItems.Sum(ci => ci.Price * ci.Quantity)
        };
        return new ResponseData<CartDTO>
        {
            statusCode = 200,
            data = cartDTO,
            message = "Cart retrieved successfully",
            dateTime = DateTime.Now
        };
    }

    public async Task<ResponseData<CartDTO>> AddToCart(string token, ItemAddToCartDTO model)
    {
        //Kiểm tra token hợp lệ và lấy userId từ token
        string userID = _jwtService.DecodePayloadToken(token);
        if (string.IsNullOrEmpty(userID))
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 401,
                data = new CartDTO
                {
                    UserId = Guid.Parse(userID),
                    Items = new List<CartItemDTO>()
                },
                message = "Unauthorized",
                dateTime = DateTime.Now
            };
        }
        //Kiểm tra mã sản phẩm có tồn tại 
        ProductVariant? productVariant = await _productVariantRepository.SingleOrDefaultAsync(pv => pv.Id == model.ProductVariantId);
        if (productVariant == null)        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = new CartDTO
                {
                    UserId = Guid.Parse(userID),
                    Items = new List<CartItemDTO>()
                },
                message = "Product variant not found",
                dateTime = DateTime.Now
            };
        }   

        //Kiểm tra có giỏ hàng user hay không
        Cart? cart =  _cartRepository.WhereAsync(c => c.UserId == Guid.Parse(userID)).Result.FirstOrDefault();
        if(cart == null) //Lần đầu bỏ sản phẩm vào giỏ hàng
        {
           //Tạo giỏ hàng
           Cart newCart = new Cart
           {
               UserId = Guid.Parse(userID),
               CartItems = new List<CartItem>
               {
                   new CartItem
                   {
                       VariantId = model.ProductVariantId,
                       Quantity = model.Quantity
                   }
               }
           };
           await _cartRepository.AddAsync(newCart);
           await _unitOfWork.SaveChangesAsync();
           return new ResponseData<CartDTO> //Lần đầu bỏ 1 sản phẩm vào giỏ hàng thì sẽ trả về giỏ hàng có 1 sản phẩm
           {
               statusCode = 200,
               data = new CartDTO
               {
                   CartId = newCart.Id,
                   UserId = Guid.Parse(userID),
                   Items = newCart.CartItems.Select(ci => new CartItemDTO
                   {
                       Id = ci.CartId,
                       ProductVariantId = ci.VariantId,
                       Quantity = ci.Quantity,
                          Name = productVariant.VariantName,
                            Price = productVariant.Price,
                            Stock = productVariant.Stock,
                            ProductVariantImage = ci.Variant.Product.Image

                   }).ToList(),
                   TotalPrice = newCart.CartItems.Sum(ci => ci.Quantity * productVariant.Price)
               },
               message = "Success",
               dateTime = DateTime.Now
           };
        }
        //Lần 2 bỏ sản phẩm vào giỏ hàng kiểm tra giỏ hàng có sản phẩm đó chưa nếu có thì tăng số lượng, chưa có thì thêm vào
        CartItem? cartItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == model.ProductVariantId);
        if(cartItem != null) //Giỏ hàng đã có sản phẩm đó rồi thì tăng số lượng lên
        {
            cartItem.Quantity += model.Quantity;
            await _cartItemRepository.UpdateAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }else
        {
            CartItem newCartItem = new CartItem
            {
                CartId = cart.Id,
                VariantId = model.ProductVariantId,
                Quantity = model.Quantity
            };
            await _cartItemRepository.AddAsync(newCartItem);
            await _unitOfWork.SaveChangesAsync();
        }

        return new ResponseData<CartDTO>
        {
            statusCode = 200,
            data = new CartDTO
            {
                CartId = cart.Id,
                UserId = Guid.Parse(userID),
                Items = cart.CartItems.Select(ci => new CartItemDTO
                {
                    Id = ci.CartId,
                    ProductVariantId = ci.VariantId,
                    Quantity = ci.Quantity,
                    Name = ci.Variant.VariantName,
                    Price = ci.Variant.Price,
                    Stock = ci.Variant.Stock,
                    ProductVariantImage = ci.Variant.Product.Image
                }).ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Variant.Price)
            },
            message = "Item added to cart successfully",
            dateTime = DateTime.Now
        };
      
    }

    public async Task<ResponseData<CartDTO>> RemoveFromCart(string token, int variantId)
    {
        //Kiểm tra token lấy ra userid
        string userID = _jwtService.DecodePayloadToken(token);
        if (string.IsNullOrEmpty(userID))
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 401,
                data = new CartDTO
                {
                    UserId = Guid.Parse(userID),
                    Items = new List<CartItemDTO>()
                },
                message = "Unauthorized",
                dateTime = DateTime.Now
            };
        }
        //Kiểm tra variaid có tồn tại không
        ProductVariant? productVariant = await _productVariantRepository.SingleOrDefaultAsync(pv => pv.Id == variantId);
        if (productVariant == null)
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = null,
                message = "Product variant not found",
                dateTime = DateTime.Now
            };
        }
        //Kiểm tra giỏ hàng có tồn tại không
        Cart? cart = _cartRepository.WhereAsync(c => c.UserId == Guid.Parse(userID)).Result.FirstOrDefault();
        if (cart == null)        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = null,
                message = "Cart not found",
                dateTime = DateTime.Now
            };
        }
        //Nếu giỏ hàng tồn tại thì kiểm tra sản phẩm đó có trong giỏ hàng không => neu có thì xoá sản phẩm đó đi
        CartItem? cartItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == variantId);
        if(cartItem == null)
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = null,
                message = "Cart item not found",
                dateTime = DateTime.Now
            };
        }
        await _cartItemRepository.RemoveAsync(cartItem);
        await _unitOfWork.SaveChangesAsync();

        //Sau khi xoá trả về giỏ hàng CartDTO mới
         cart = _cartRepository.WhereAsync(c => c.UserId == Guid.Parse(userID)).Result.FirstOrDefault();
        CartDTO cartDTO = new CartDTO
        {
            CartId = cart.Id,
            UserId = Guid.Parse(userID),
            Items = cart.CartItems.Select(ci => new CartItemDTO
            {
                Id = ci.CartId,
                ProductVariantId = ci.VariantId,
                Quantity = ci.Quantity,
                Name = ci.Variant.VariantName,
                Price = ci.Variant.Price,
                Stock = ci.Variant.Stock,
                ProductVariantImage = ci.Variant.Product.Image
            }).ToList(),
            TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Variant.Price)
        };
        return new ResponseData<CartDTO>
        {
            statusCode = 200,
            data = cartDTO,
            message = "Item removed from cart successfully",
            dateTime = DateTime.Now
        };        
    }

 
    public  async Task<ResponseData<CartDTO>> UpdateCartItem(string token, ItemUpdateToCartDTO cartItem)
    {
        //kiểm tra userID từ token
        string userID = _jwtService.DecodePayloadToken(token);
        if (string.IsNullOrEmpty(userID))
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 401,
                data = null,
                message = "Unauthorized",
                dateTime = DateTime.Now
            };
        }
        //Kiểm tra variaid có tồn tại không
        ProductVariant? productVariant = await _productVariantRepository.SingleOrDefaultAsync(pv => pv.Id == cartItem.ProductVariantId);
        if (productVariant == null)        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = null,
                message = "Product variant not found",
                dateTime = DateTime.Now
            }; 
        }
        //Kiểm tra giỏ hàng có tồn tại không
        Cart? cart = _cartRepository.WhereAsync(c => c.UserId == Guid.Parse(userID)).Result.FirstOrDefault();
        if (cart == null)        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = null,
                message = "Cart not found",
                dateTime = DateTime.Now
            };
        }
        //Nếu giỏ hàng tồn tại thì kiểm tra sản phẩm đó có trong giỏ hàng không => neu có thì cập nhật số lượng sản phẩm đó đi
        CartItem? existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == cartItem.ProductVariantId);
        if (existingCartItem == null)
        {
            return new ResponseData<CartDTO>
            {
                statusCode = 404,
                data = null,
                message = "Cart item not found",
                dateTime = DateTime.Now
            };
        }
        existingCartItem.Quantity = cartItem.Quantity;
        await _cartItemRepository.UpdateAsync(existingCartItem);
        await _unitOfWork.SaveChangesAsync();
        //Sau khi cập nhật trả về giỏ hàng CartDTO mới
        cart = _cartRepository.WhereAsync(c => c.UserId == Guid.Parse(userID)).Result.FirstOrDefault();
        CartDTO cartDTO = new CartDTO
        {
            CartId = cart.Id,
            UserId = Guid.Parse(userID),
            Items = cart.CartItems.Select(ci => new CartItemDTO
            {
                Id = ci.CartId,
                ProductVariantId = ci.VariantId,
                Quantity = ci.Quantity,
                Name = ci.Variant.VariantName,
                Price = ci.Variant.Price,
                Stock = ci.Variant.Stock,
                ProductVariantImage = ci.Variant.Product.Image
            }).ToList(),
            TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * ci.Variant.Price)
        };
        return new ResponseData<CartDTO>
        {
            statusCode = 200,
            data = cartDTO,
            message = "Cart item updated successfully",
            dateTime = DateTime.Now
        };
        

    }
}