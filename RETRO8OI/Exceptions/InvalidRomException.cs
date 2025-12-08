namespace RETRO8OI.Exceptions;

public class InvalidRomException : Exception
{
    public InvalidRomException(string? message) : base(message) {}
}