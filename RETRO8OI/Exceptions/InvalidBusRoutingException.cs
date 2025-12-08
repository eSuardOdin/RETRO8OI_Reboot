namespace RETRO8OI.Exceptions;

/// <summary>
/// Thrown if we try to R/W a mapped memory device with an address out of its accepted range - SHOULD NOT HAPPEN... but
/// </summary>
public class InvalidBusRoutingException : Exception
{
    public InvalidBusRoutingException(string? message) : base(message) {}
}