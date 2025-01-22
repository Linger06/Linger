namespace Linger.FileSystem.Exceptions;
public class OutOfReTryCountException(string? message, Exception? innerException) : Exception(message, innerException)
{
}
