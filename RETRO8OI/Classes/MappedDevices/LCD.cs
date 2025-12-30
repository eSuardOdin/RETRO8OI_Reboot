using RETRO8OI.Exceptions;

namespace RETRO8OI;

public class LCD : IMemoryMappedDevice
{
    private byte OamDma = 0;
    private byte LCDC = 0;
    private byte LY = 0;
    private byte LYC = 0;
    private byte STAT = 0;
    private byte SCY = 0;
    private byte SCX = 0;
    private byte WY = 0;
    private byte WX = 0;
    private byte BGP = 0;
    private byte OBP0 = 0;
    private byte OBP1 = 0;
    
    public bool IsLcdPpuEnabled => (LCDC & 0x8) == 0x8;

    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case 0xFF40:
                //Console.WriteLine($"Writing [{data:X2}] to LCDC [{address:X4}]");
                LCDC = data;
                return;
            case 0xFF44:
                //Console.WriteLine($"Writing [{data:X2}] to LY [{address:X4}]");
                LY = data;
                return;
            case 0xFF45:
                //Console.WriteLine($"Writing [{data:X2}] to LYC [{address:X4}]");
                LYC = data;
                return;
            case 0xFF41:
                //Console.WriteLine($"Writing [{data:X2}] to LCD STAT [{address:X4}]");
                STAT = data;
                return;
            case 0xFF42:
                //Console.WriteLine($"Writing [{data:X2}] to SCY [{address:X4}]");
                SCY = data;
                return;
            case 0xFF43:
                //Console.WriteLine($"Writing [{data:X2}] to SCX [{address:X4}]");
                SCX = data;
                return;
            case 0xFF4A:
                //Console.WriteLine($"Writing [{data:X2}] to WY [{address:X4}]");
                WY = data;
                return;
            case 0xFF4B:
                //Console.WriteLine($"Writing [{data:X2}] to WX [{address:X4}]");
                WX = data;
                return;
        }

        throw new InvalidBusRoutingException($"Error writing [{data:X2}] to [{address:X4}] in LCD.");

    }

    public byte Read(ushort address)
    {
        switch (address)
        {
            case 0xFF40:
                //Console.WriteLine($"Reading LCDC [{address:X4}]");
                return LCDC;
            case 0xFF44:
                //Console.WriteLine($"Reading LY [{address:X4}]");
                return LY;
            case 0xFF45:
                //Console.WriteLine($"Reading LYC [{address:X4}]");
                return LYC;
            case 0xFF41:
                //Console.WriteLine($"Reading LCD STAT [{address:X4}]");
                return STAT;
            case 0xFF42:
                //Console.WriteLine($"Reading SCY [{address:X4}]");
                return SCY;
            case 0xFF43:
                //Console.WriteLine($"Reading SCX [{address:X4}]");
                return SCX;
            case 0xFF4A:
                //Console.WriteLine($"Reading WY [{address:X4}]");
                return WY;
            case 0xFF4B:
                //Console.WriteLine($"Reading WX [{address:X4}]");
                return WX;
        }

        throw new InvalidBusRoutingException($"Error reading [{address:X4}] in LCD.");
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0xFF40 && address <= 0xFF4B);
    }
}