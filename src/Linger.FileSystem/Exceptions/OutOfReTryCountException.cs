namespace Linger.FileSystem.Exceptions;
public class OutOfReTryCountException : Exception
{
    public OutOfReTryCountException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
