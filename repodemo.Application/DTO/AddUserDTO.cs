//Tạo ra format trên api để thêm dữ liệu cả user và quyền

public class AddUserDTO
{
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
    public string Address { get; set; }
    public List<string> RoleNames { get; set; } = new List<string>();
    //1,2,3

    //ADMIN, USER, STOREOWNER
}