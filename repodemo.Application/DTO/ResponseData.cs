
//Tạo 1 class reponse T
public class ResponseData<T>
{
    public int statusCode { get; set; }
    public string message { get; set; }
    public T data { get; set; }
    public DateTime dateTime { get; set; } = DateTime.Now;
    
}