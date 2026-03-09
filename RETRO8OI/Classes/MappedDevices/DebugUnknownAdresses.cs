namespace RETRO8OI;
/// <summary>
/// This class is just intended as a debug whenever I encounter a strange address
/// </summary>
public class DebugUnknownAdresses : IMemoryMappedDevice
{
    public void Write(ushort address, byte data)
    {
        Console.WriteLine($"WRITE [{address:X4}] not implemented");
    }

    public byte Read(ushort address)
    {
        Console.WriteLine($"READ [{address:X4}] not implemented");
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        //return address == 0xFF03 || address == 0xFF08 || address == 0xFF09 ||  address == 0xFF0A;
        return true;
    }
}