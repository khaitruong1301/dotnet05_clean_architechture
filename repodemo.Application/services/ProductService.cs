//Trong service sau này file có thể lên đến hàng ngàn dòng code, nên sẽ rất khó để quản lý, nên chúng ta sẽ chia nhỏ service thành nhiều file khác nhau, mỗi file sẽ chứa một phần của service, ví dụ: ProductService.cs, OrderService.cs, UserService.cs,... để dễ quản lý hơn



using repodemo.Infrastructure.Models;

public interface IProductService
{
    Task<List<Product>> GetAllProduct();
    Task<Product> GetProductById(int id);
}


public class ProductService : IProductService
{
    ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    public async Task<List<Product>> GetAllProduct()
    {
       
       return await _productRepository.GetAllProduct();
    }

    //Viết hàm lấy ra sản phẩm theo id
    public async Task<Product?> GetProductById(int id)
    {
        var lstProduct = await _productRepository.GetAllProduct();
        return lstProduct.SingleOrDefault(p => p.Id == id);
    }   
}