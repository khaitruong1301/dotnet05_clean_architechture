//Trong service sau này file có thể lên đến hàng ngàn dòng code, nên sẽ rất khó để quản lý, nên chúng ta sẽ chia nhỏ service thành nhiều file khác nhau, mỗi file sẽ chứa một phần của service, ví dụ: ProductService.cs, OrderService.cs, UserService.cs,... để dễ quản lý hơn



using Azure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using repodemo.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;

public interface IProductService
{
    Task<ResponseData<List<Product>>> GetAllProduct();
    Task<ResponseData<Product>?> GetProductById(int id);
    //Viết service vừa tìm kiếm vừa phân trang sản phẩm

    Task<ResponseData<List<Product>>> SearchProduct(string keyword, int pageNumber, int pageSize);


    Task<ResponseData<ProductDetailDTO>> GetProductDetailById(int id);

}


public class ProductService : IProductService
{
    ProductRepository _productRepository;
    ProductVariantRepository _productVariantRepository;
    ProductImageRepository _productImageRepository;
    public ProductService(ProductRepository productRepository, ProductVariantRepository productVariantRepository, ProductImageRepository productImageRepository)
    {
        _productRepository = productRepository;
        _productVariantRepository = productVariantRepository;
        _productImageRepository = productImageRepository;
    }
    
    public async Task<ResponseData<List<Product>>> GetAllProduct()
    {
       var lstProduct = await _productRepository.GetAllAsync();
       ResponseData<List<Product>> response = new ResponseData<List<Product>>()
       {
           statusCode = 200,
           message = "Lấy danh sách sản phẩm thành công",
           data = lstProduct.Skip(0).Take(10).ToList() //Giả sử chỉ lấy 10 sản phẩm đầu tiên    
       };
       return response;
    }

    //Viết hàm lấy ra sản phẩm theo id
    public async Task<ResponseData<Product>?> GetProductById(int id)
    {
       //Kiểm tra product id đó có tồn tại hay không
       Product? product = await _productRepository.SingleOrDefaultAsync(prod => prod.Id == id);
       if(product == null)       {
           return new ResponseData<Product>()
           {
               statusCode = 404,
               message = "Không tìm thấy sản phẩm",
               data = null
           };
       }
       return new ResponseData<Product>()
       {
           statusCode = 200,
           message = "Lấy sản phẩm thành công",
           data = product
       };
    }

    public async Task<ResponseData<ProductDetailDTO>> GetProductDetailById(int id)
    {

        //Lấy ra danh sách sản phẩm bảng Product, ProductVariant, ProductImage dựa trên product id
        //Kiểm tra id có tồn tại trong product hay không
        Product? prod = await _productRepository.SingleOrDefaultAsync(p => p.Id == id);
        if(prod == null)
        {
            return new ResponseData<ProductDetailDTO>()
            {
                statusCode = 404,
                message = "Không tìm thấy sản phẩm",
                data = null
            };
        }
        //Sau đó map dữ liệu vào ProductDetailDTO rồi trả về cho client

        ProductDetailDTO prodDetail = new ProductDetailDTO()
        {
            Id = prod.Id,
            Name = prod.Name,
            Description = prod.Description,
            PriceDefault = prod.DisplayPrice,
            ImageUrl = prod.Image,
            AdditionalData = prod.AdditionalData
        };

        //Lấy danh sách image của sản phẩm 
        var lstProductImage =  _productImageRepository.WhereAsync(item => item.ProductId == id).Result.Select(n => new ProductImageDTO()
        {
            Id = n.Id,
            ImageUrl = n.ImageUrl
        }).ToList();
        //Đưa list image vào product detail DTO
        prodDetail.ProductImages = lstProductImage;

        //Lấy danh sách variant của sản phẩm
        var lstProductVariant = _productVariantRepository.WhereAsync(item => item.ProductId == id).Result.Select(n => new ProductVariantDTO()
        {
            Id = n.Id,
            Name = n.VariantName,
            Alias = n.Alias,
            Price = n.Price,
            Stock = n.Stock,
            ProductVariantImage = n.Image
        }).ToList();

        //Đưa list variant vào product detail DTO
        prodDetail.ProductVariants = lstProductVariant;

        return new ResponseData<ProductDetailDTO>()
        {
            statusCode = 200,
            message = "Lấy chi tiết sản phẩm thành công",
            data = prodDetail
        };
    }

    public async Task<ResponseData<List<Product>>> SearchProduct(string keyword, int pageNumber, int pageSize)
    {

        keyword = FunctionUtility.GenerateSlug(keyword); //chuyển keyword thành slug để tìm kiếm, ví dụ: "Áo thun nam" -> "ao-thun-nam"
        
        //Lấy dữ liệu dựa trên contains keyword
        var productList = await _productRepository.WhereAsync(prod => prod.Alias.Contains(keyword));

        if(productList.Count == 0)
        {
            return new ResponseData<List<Product>>()
            {
                statusCode = 404,
                message = "Không tìm thấy sản phẩm nào",
                data = new List<Product>()
            };
        }
        return new ResponseData<List<Product>>()
        {
            statusCode = 200,
            message = "Tìm kiếm sản phẩm thành công",
            data = productList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
    }

//viết hàm lấy chi tiết sản phẩm theo id, trong đó chi tiết sản phẩm bao gồm thông tin sản phẩm, danh sách hình ảnh của sản phẩm, danh sách variant của sản phẩm, mỗi variant bao gồm thông tin variant và hình ảnh của variant đó



    


}






