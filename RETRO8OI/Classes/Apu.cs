namespace RETRO8OI;

public class Apu : IMemoryMappedDevice
{
    public void Write(ushort address, byte data)
    {
        // Promess, will do.
    }

    public byte Read(ushort address)
    {
        // Promess, will do.
        return 0x0;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0xFF10 && address <= 0xFF26) || (address >= 0xFF30 && address <= 0xFF3F);
    }
}