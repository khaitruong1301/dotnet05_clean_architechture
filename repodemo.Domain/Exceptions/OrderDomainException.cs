namespace repodemo.Domain.Exceptions;

/// <summary>
/// Exception cho Domain layer
/// </summary>
public class OrderDomainException : Exception
{
    public OrderDomainException(string message) : base(message) { }
    public OrderDomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception khi order không hợp lệ
/// </summary>
public class InvalidOrderException : OrderDomainException
{
    public InvalidOrderException(string message) : base(message) { }
}

/// <summary>
/// Exception khi item không hợp lệ
/// </summary>
public class InvalidOrderItemException : OrderDomainException
{
    public InvalidOrderItemException(string message) : base(message) { }
}














