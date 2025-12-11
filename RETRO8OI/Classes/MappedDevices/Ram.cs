namespace RETRO8OI;

public class Ram : IMemoryMappedDevice
{
    public byte[] Vram { get; private set; }
    public byte[] Wram { get; private set; }

    public Ram()
    {
        Vram = new byte[0x2000];
        Wram = new byte[0x2000];
    }
    
    public void Write(ushort address, byte data)
    {
        // Write VRAM
        if (address >= 0x8000 && address <= 0x9FFF)
        {
            Vram[address - 0x8000] =  data;
        }
        // Write WRAM
        if (address >= 0xC000 && address <= 0xDFFF)
        {
            Wram[address - 0xC000] = data;
        }
        // Echo RAM
        if (address >= 0xE000 && address <= 0xFDFF)
        {
            Wram[address - 0xE000] = data;
        }
    }

    public byte Read(ushort address)
    {
        // Read VRAM
        if (address >= 0x8000 && address <= 0x9FFF)
        {
            return Vram[address - 0x8000];
        }
        // Read WRAM
        if (address >= 0xC000 && address <= 0xDFFF)
        {
            return Wram[address - 0xC000];
        }
        // Echo RAM
        if (address >= 0xE000 && address <= 0xFDFF)
        {
            return Wram[address - 0xE000];
        }

        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0x8000 && address <= 0x9FFF) || (address >= 0xC000 && address <= 0xDFFF) || (address >= 0xE000 && address <= 0xFDFF);
    }
}