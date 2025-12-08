namespace RETRO8OI;

/// <summary>
/// Used to represent a memory that can be written to or read from by the bus
/// </summary>
public interface IMemoryMappedDevice
{
    public void Write(ushort address, byte data);
    public byte Read(ushort address);
    public bool Accept(ushort address);
}