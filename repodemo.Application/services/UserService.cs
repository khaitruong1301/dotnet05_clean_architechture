using repodemo.Infrastructure.Models;
public interface IUserService
{
    Task<ResponseData<AddUserDTO>> AddUser(AddUserDTO userDTO);
    Task<ResponseData<string>> Login(UserLoginDTO userLogin);
    Task<ResponseData<UserProfileDTO>> GetProfileByToken(string token);

    Task<ResponseData<RegisterDTO>> Register(RegisterDTO model);

}

public class UserService : IUserService
{
    private readonly UserRepository _userRepository;
    private readonly UnitOfWork _unitOfWork;
    private readonly RoleRepository _roleRepository;

    private readonly JwtService _jwtService;

    public UserService(UserRepository userRepository, RoleRepository roleRepository, UnitOfWork unitOfWork, JwtService jwtService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;

    }

    public async Task<ResponseData<AddUserDTO>> AddUser(AddUserDTO userDTO)
    {
        try
        {
            //Check email đã tồn tại, username tồn tại, phone tồn tại=> nếu đã tồn tại thì trả về lỗi, chưa tồn tại thì mới thêm 
            var existingUserByEmail = await _userRepository.SingleOrDefaultAsync(u => u.Email == userDTO.Email || u.Username == userDTO.Username || u.Phone == userDTO.Phone);
            if (existingUserByEmail != null)
            {                return await Task.Run(() => new ResponseData<AddUserDTO>
                {
                    statusCode = 400,
                    data = userDTO,
                    message = "Email, username hoặc số điện thoại đã tồn tại. Vui lòng sử dụng email, username và số điện thoại khác.",
                    dateTime = DateTime.Now
                });
            }


            //Map data từ dto sang model entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = userDTO.Username,
                FullName = userDTO.FullName,
                Alias = FunctionUtility.GenerateSlug(userDTO.FullName),
                Email = userDTO.Email,
                Phone = userDTO.Phone,
                Avatar = @$"https://i.pravatar.cc?u={userDTO.Email}",
                CreatedAt = DateTime.Now,
                Deleted = false,
                PasswordHash = PasswordHelper.HashPassword(userDTO.Password),
                Address = userDTO.Address
            };

            //Lấy ra danh sách role id từ role name mà client gửi lên
            foreach (var roleName in userDTO.RoleNames)
            {
                Role? role = await _roleRepository.SingleOrDefaultAsync(role => role.RoleName == roleName);
                if (role != null)
                {
                    user.Roles.Add(role);
                }
            }

            //Gọi repository để thêm user vào database
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return await Task.Run(() => new ResponseData<AddUserDTO>
            {
                statusCode = 200,
                data = userDTO,
                message = "Thêm user thành công",
                dateTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return await Task.Run(() => new ResponseData<AddUserDTO>
            {
                statusCode = 500,
                data = userDTO,
                message = $"Thêm user thất bại. Lỗi: {ex.Message}",
                dateTime = DateTime.Now
            });
        }


    }

    public Task<ResponseData<UserProfileDTO>> GetProfileByToken(string token)
    {
        try
        {
            string userId = _jwtService.DecodePayloadToken(token);
            var user = _userRepository.SingleOrDefaultAsync(u => u.Id.ToString() == userId).Result;
            if (user == null)
            {
                return Task.Run(() => new ResponseData<UserProfileDTO>
                {
                    statusCode = 401,
                    data = null,
                    message = "Không tìm thấy user",
                    dateTime = DateTime.Now
                });
            }

            UserProfileDTO userProfile = new UserProfileDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Address = user.Address,
            };

            return Task.Run(() => new ResponseData<UserProfileDTO>
            {
                statusCode = 200,
                data = userProfile,
                message = "Lấy thông tin user thành công",
                dateTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return Task.Run(() => new ResponseData<UserProfileDTO>
            {
                statusCode = 500,
                data = null,
                message = $"Lấy thông tin user thất bại. Lỗi: {ex.Message}",
                dateTime = DateTime.Now
            });
        }
    }

    public Task<ResponseData<string>> Login(UserLoginDTO userLogin)
    {
        try
        {
            string? token = _jwtService.GenerateToken(userLogin);
            if (token == null)
            {
                return Task.Run(() => new ResponseData<string>
                {
                    statusCode = 401,
                    data = null,
                    message = "Đăng nhập thất bại. Email hoặc mật khẩu không đúng.",
                    dateTime = DateTime.Now
                });
            }

            return Task.Run(() => new ResponseData<string>
            {
                statusCode = 200,
                data = token,
                message = "Đăng nhập thành công",
                dateTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return Task.Run(() => new ResponseData<string>
            {
                statusCode = 500,
                data = null,
                message = $"Đăng nhập thất bại. Lỗi: {ex.Message}",
                dateTime = DateTime.Now
            });
        }
    }

    public async Task<ResponseData<RegisterDTO>> Register(RegisterDTO model)
    {
         try
        {
            //Check email đã tồn tại, username tồn tại, phone tồn tại=> nếu đã tồn tại thì trả về lỗi, chưa tồn tại thì mới thêm 
            var existingUserByEmail = await _userRepository.SingleOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username || u.Phone == model.Phone);
            if (existingUserByEmail != null)
            {                return await Task.Run(() => new ResponseData<RegisterDTO>
                {
                    statusCode = 400,
                    data = model,
                    message = "Email, username hoặc số điện thoại đã tồn tại. Vui lòng sử dụng email, username và số điện thoại khác.",
                    dateTime = DateTime.Now
                });
            }


            //Map data từ dto sang model entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = model.Username,
                FullName = model.FullName,
                Alias = FunctionUtility.GenerateSlug(model.FullName),
                Email = model.Email,
                Phone = model.Phone,
                Avatar = @$"https://i.pravatar.cc?u={model.Email}",
                CreatedAt = DateTime.Now,
                Deleted = false,
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Address = model.Address
            };

            user.Roles.Add(_roleRepository.SingleOrDefaultAsync(role => role.Id == UserRoleConst.User).Result); //Mặc định khi đăng ký sẽ có role là User
            
            //Gọi repository để thêm user vào database
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            model.Password = "******";
            return await Task.Run(() => new ResponseData<RegisterDTO>
            {
                statusCode = 200,
                data = model,
                message = "Đăng ký thành công",
                dateTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return await Task.Run(() => new ResponseData<RegisterDTO>
            {
                statusCode = 500,
                data = model,
                message = $"Đăng ký thất bại. Lỗi: {ex.Message}",
                dateTime = DateTime.Now
            });
        }
    }
}