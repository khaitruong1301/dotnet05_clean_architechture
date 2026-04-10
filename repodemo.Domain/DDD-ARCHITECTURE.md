/// <summary>
/// HƯỚNG DẪN PHỐI HỢP DDD - Domain và Application Layer
/// </summary>

/*
╔════════════════════════════════════════════════════════════════════════════╗
║                        KIẾN TRÚC DDD TRONG PROJECT                         ║
╚════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────────────────────────────────────────────────┐
│ API Layer (Controller)                                                       │
│  - Chỉ nhận request, gọi Application Service, trả response                  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ Application Layer (OrderService)  ← ĐIỀU PHỐI USE CASE                      │
│  ✓ Xác thực request (Authentication)                                        │
│  ✓ Load dữ liệu từ Repository                                               │
│  ✓ Gọi Domain Service để validate business rules                            │
│  ✓ Gọi Domain Service để xử lý logic                                        │
│  ✓ Lưu Entity vào Repository                                                │
│  ✓ Quản lý transaction (UnitOfWork)                                         │
│  ✓ Ánh xạ Entity → DTO trả về response                                      │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ Domain Layer (OrderDomainService) ← XỬ LÝ BUSINESS LOGIC                    │
│  ✓ Validate business rules (OrderBusinessRules)                             │
│  ✓ Tạo/sửa Aggregate (OrderAggregate)                                       │
│  ✓ Kiểm tra invariants                                                      │
│  ⚠ KHÔNG phụ thuộc vào Repository                                           │
│  ⚠ KHÔNG phụ thuộc vào Infrastructure                                       │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│ Infrastructure Layer (Repository)  ← PERSIST DATA                           │
│  ✓ Lấy/lưu dữ liệu từ Database                                              │
│  ✓ Map Database → Entity                                                    │
│  ⚠ KHÔNG chứa business logic                                                │
└─────────────────────────────────────────────────────────────────────────────┘


╔════════════════════════════════════════════════════════════════════════════╗
║                           FLOW: TẠO ĐƠN HÀNG                                ║
╚════════════════════════════════════════════════════════════════════════════╝

Bước 1: Application Service (OrderService) nhận request
   - DecodeToken() → lấy userId
   - LoadCart() từ Repository
   
Bước 2: Validate dữ liệu input
   - Kiểm tra cart exists
   - Kiểm tra items count > 0
   
Bước 3: Gọi Domain Service để validate business rules
   - OrderDomainService.ValidateOrderItem() 
     ↳ Kiểm tra quantity > 0
     ↳ Kiểm tra stock đủ không
     ↳ Kiểm tra price valid
   
Bước 4: Application Service tạo Order Entity
   - new Order { BuyerId, Alias, TotalAmount, Items }
   
Bước 5: Application Service lưu vào DB
   - _orderRepository.AddAsync()
   - _cartRemove()
   - _unitOfWork.SaveChangesAsync()
   
Bước 6: Ánh xạ Entity → DTO
   - MapToOrderDTO()
   
Bước 7: Trả response


╔════════════════════════════════════════════════════════════════════════════╗
║                    PHÂN CHIA TRÁCH NHIỆM RÕ RÀNG                            ║
╚════════════════════════════════════════════════════════════════════════════╝

┌────────────────────────────────────────────────────────────────────────────┐
│ Domain Layer (repodemo.Domain)                                             │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ 1. OrderAggregate                                                          │
│    - Quản lý Order state (items, total)                                   │
│    - AddItem(), RemoveItem(), UpdateQuantity()                            │
│    - RecalculateTotal()                                                    │
│    - IsValid() → kiểm tra invariants                                       │
│                                                                             │
│ 2. OrderBusinessRules (Static)                                             │
│    - ValidateQuantity(qty) → bool, error                                  │
│    - ValidateStock(qty, stock) → bool, error                              │
│    - ValidatePrice(price) → bool, error                                   │
│    - ValidateItemCount(count) → bool, error                               │
│                                                                             │
│ 3. OrderDomainService                                                      │
│    - ValidateOrderItem(name, qty, stock, price)                           │
│    - CreateOrderFromCart(buyerId, items, alias)                           │
│    - GetOrderValidationErrors(order)                                       │
│    - CalculateOrderTotal(items)                                            │
│                                                                             │
│ ⚠ Không chứa: Repository, Database, HttpContext                           │
└────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────────────────┐
│ Application Layer (repodemo.Application)                                   │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ 1. OrderService (Use Case: CreateOrder)                                    │
│    DI:                                                                      │
│      - OrderDomainService (gọi để validate + business logic)               │
│      - OrderRepository (lấy/lưu Order)                                     │
│      - CartRepository (lấy giỏ hàng)                                       │
│      - CartItemRepository (xóa items)                                      │
│      - UnitOfWork (quản lý transaction)                                    │
│      - JwtService (xác thực)                                               │
│                                                                             │
│    AddOrder(token, cartDTO):                                               │
│      1. DecodeToken() → userId                                             │
│      2. LoadCart() từ Repository                                           │
│      3. ValidateCart() → basic checks                                      │
│      4. FOR EACH item:                                                     │
│            _orderDomainService.ValidateOrderItem() ← DOMAIN LOGIC          │
│      5. Create Order object                                                │
│      6. _orderRepository.AddAsync()                                        │
│      7. _cartRepository.RemoveAsync()                                      │
│      8. _unitOfWork.SaveChangesAsync()                                     │
│      9. MapToOrderDTO()                                                    │
│      10. Return ResponseData<OrderDTO>                                     │
│                                                                             │
│ ✓ Điều phối các thành phần                                                │
│ ✓ Quản lý transaction                                                      │
│ ✓ Xử lý HTTP concerns                                                      │
└────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────────────────┐
│ Infrastructure Layer (repodemo.Infrastructure)                             │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│ OrderRepository : IRepository<Order>                                       │
│   - GetAsync(id)                                                           │
│   - AddAsync(order)                                                        │
│   - UpdateAsync(order)                                                     │
│   - SingleOrDefaultAsync(predicate)                                        │
│                                                                             │
│ CartRepository, CartItemRepository, etc.                                   │
│                                                                             │
│ ✓ Chỉ chứa database access logic                                          │
│ ✓ Không chứa business logic                                                │
└────────────────────────────────────────────────────────────────────────────┘


╔════════════════════════════════════════════════════════════════════════════╗
║                            ƯỞI ĐIỂM CỦA DDD                                ║
╚════════════════════════════════════════════════════════════════════════════╝

✓ Business Logic tập trung ở Domain Layer
  → Dễ kiểm thử (Unit test)
  → Không phụ thuộc Infrastructure
  → Dễ bảo trì

✓ Application Service có trách nhiệm rõ ràng
  → Điều phối use case
  → Gọi Domain Service để validate
  → Gọi Repository để persist

✓ Separation of Concerns
  → Domain: WHAT & WHY (business rules)
  → Application: HOW (use case orchestration)
  → Infrastructure: PERSISTENCE (data access)

✓ Tái sử dụng logic
  → OrderDomainService có thể dùng từ nhiều Application Services
  → Không lặp lại code validation


╔════════════════════════════════════════════════════════════════════════════╗
║                      DEPENDENCY INJECTION SETUP                            ║
╚════════════════════════════════════════════════════════════════════════════╝

Trong Program.cs:

// Domain Services
builder.Services.AddScoped<OrderDomainService>();

// Application Services
builder.Services.AddScoped<IOrderService, OrderService>();

// Repositories
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<CartItemRepository>();

// Infrastructure
builder.Services.AddScoped<UnitOfWork>();
*/


public class DDDFlowDiagram
{
    /*
    
    LUỒNG TẠO ĐƠN HÀNG (ADD ORDER):
    
    API Controller
        ↓
    OrderService.AddOrder(token, cartDTO)
        │
        ├─→ [AUTHENTICATION]
        │   _jwtService.DecodePayloadToken(token)
        │   ↓ userId
        │
        ├─→ [LOAD DATA]
        │   _cartRepository.SingleOrDefaultAsync()
        │   ↓ Cart object
        │
        ├─→ [VALIDATION - Basic]
        │   if (cart == null) → error
        │   if (items.Count == 0) → error
        │
        ├─→ [VALIDATE EACH ITEM - With Domain Service]
        │   FOR EACH cartItem:
        │      _orderDomainService.ValidateOrderItem()
        │      ├─→ OrderBusinessRules.ValidateQuantity()
        │      ├─→ OrderBusinessRules.ValidateStock()
        │      ├─→ OrderBusinessRules.ValidatePrice()
        │      └─ If all valid → continue
        │
        ├─→ [CREATE ORDER ENTITY]
        │   new Order {
        │       BuyerId = userId,
        │       Alias = GenerateAlias(),
        │       TotalAmount = CalculateTotal(),
        │       OrderItems = cartDTO.Items mapped
        │   }
        │
        ├─→ [PERSIST DATA]
        │   _orderRepository.AddAsync(order)
        │   _cartRepository.RemoveAsync(cart)
        │   _unitOfWork.SaveChangesAsync()
        │
        ├─→ [MAP TO DTO]
        │   MapToOrderDTO(order)
        │
        └─→ API Response
            {
                statusCode: 200,
                data: OrderDTO,
                message: "Success"
            }
    
    
    DEPENDENCY DIAGRAM:
    
    API Controller
        │
        └─→ OrderService
            │
            ├─→ OrderDomainService (Domain)
            │   │
            │   └─→ OrderBusinessRules (Domain)
            │
            ├─→ OrderRepository (Infrastructure)
            │
            ├─→ CartRepository (Infrastructure)
            │
            ├─→ CartItemRepository (Infrastructure)
            │
            ├─→ UnitOfWork (Infrastructure)
            │
            └─→ JwtService (Application)
    
    */
}
