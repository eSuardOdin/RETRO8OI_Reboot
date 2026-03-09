namespace RETRO8OI;

public class CgbAdresses : IMemoryMappedDevice
{
    public void Write(ushort address, byte data)
    {
    }

    public byte Read(ushort address)
    {
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return
        (
            address == 0xFF4C ||
            address == 0xFF4D ||
            address == 0xFF4F ||
            (address >= 0xFF51 && address <= 0xFF70)
        );
    }
}