using System.IO.MemoryMappedFiles;

namespace RETRO8OI;

public class OAM : IMemoryMappedDevice
{
    private byte[] OamData;

    public OAM()
    {
        OamData = new byte[0x10];
    }
    public void Write(ushort address, byte data)
    {
        throw new NotImplementedException();
    }

    public byte Read(ushort address)
    {
        throw new NotImplementedException();
    }

    public bool Accept(ushort address)
    {
        return (address >= 0xFE00 && address <= 0xFE9F);
    }
}