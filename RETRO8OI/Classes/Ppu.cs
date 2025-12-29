namespace RETRO8OI;

public class Ppu : IMemoryMappedDevice
{
    public MemoryBus Bus { get; private set; }

    public byte Mode
    {
        get => (byte)(STAT & 0x3);
        set => STAT = (byte)((STAT & 0xFC) | value);
    }
    /// <summary>
    /// Responds to 0xFE00 - 0xFE9F<br/>
    /// <para>Accessible to CPU only in modes 0 (HBlank) and 1 (VBlank)</para>
    /// 
    /// </summary>
    private byte[] OAM;
    public byte[] Vram { get; private set; }
    private byte OamDma = 0;
    private byte LCDC = 0;
    private byte LY;
    private byte LYC;
    private byte STAT = 0;
    private byte SCY = 0;
    private byte SCX = 0;
    private byte WY = 0;
    private byte WX = 0;
    private byte BGP = 0;
    private byte OBP0 = 0;
    private byte OBP1 = 0;
    private int VerticalCyclesCount = 0;

    public Ppu(MemoryBus bus)
    {
        Bus = bus;
        Vram = new byte[0x2000];
        OAM = new byte[0xA0];
        Mode = 0;
    }
    
    public void Write(ushort address, byte data)
    {
        // Write VRAM only if mode != 3
        if (address >= 0x8000 && address <= 0x9FFF && Mode != 3)
        {
            Console.WriteLine($"Writing [{data:X2}] to VRAM [{address:X4}]");
            Vram[address - 0x8000] =  data;
            return;
        }
        // Write to OAM only if mode 0 or 1 (VBlank, HBlank)
        if (address >= 0xFE00 && address <= 0xFE9F && Mode < 2)
        {
            Console.WriteLine($"Writing [{data:X2}] to OAM [{address:X4}]");
            OAM[address - 0xFE00] =  data;
            return;
        }
        // Write LCD stuff
        if (address >= 0xFF40 && address <= 0xFF4B)
        {
            switch (address)
            {
                case 0xFF40:
                    Console.WriteLine($"Writing [{data:X2}] to LCDC [{address:X4}]");
                    LCDC = data;
                    return;
                case 0xFF44:
                    Console.WriteLine($"Writing [{data:X2}] to LY [{address:X4}]");
                    LY = data;
                    if((STAT & 0x40 ) == 0x40 && LY == LYC) Bus.Write(0xFF0F, 0x2); // STAT Interrupt if LY == LYC
                    return;
                case 0xFF45:
                    Console.WriteLine($"Writing [{data:X2}] to LYC [{address:X4}]");
                    LYC = data;
                    if((STAT & 0x40 ) == 0x40 && LY == LYC) Bus.Write(0xFF0F, 0x2); // STAT Interrupt if LY == LYC
                    return;
                case 0xFF41:
                    Console.WriteLine($"Writing [{data:X2}] to LCD STAT [{address:X4}]");
                    STAT = data;
                    return;
                case 0xFF42:
                    Console.WriteLine($"Writing [{data:X2}] to SCY [{address:X4}]");
                    SCY = data;
                    return;
                case 0xFF43:
                    Console.WriteLine($"Writing [{data:X2}] to SCX [{address:X4}]");
                    SCX = data;
                    return;
                case 0xFF4A:
                    Console.WriteLine($"Writing [{data:X2}] to WY [{address:X4}]");
                    WY = data;
                    return;
                case 0xFF4B:
                    Console.WriteLine($"Writing [{data:X2}] to WX [{address:X4}]");
                    WX = data;
                    return;
            }
        }
    }

    public byte Read(ushort address)
    {
        // Read VRAM
        if (address >= 0x8000 && address <= 0x9FFF && Mode != 3)
        {
            Console.WriteLine($"Reading VRAM [{address:X4}]");
            return Vram[address - 0x8000];
        }
        // Read OAM
        if (address >= 0xFE00 && address <= 0xFE9F && Mode < 2)
        {
            Console.WriteLine($"Reading OAM [{address:X4}]");
            return OAM[address - 0xFE00];
        }
        // Read LCD stuff
        if (address >= 0xFF40 && address <= 0xFF4B)
        {
            switch (address)
            {
                case 0xFF40:
                    Console.WriteLine($"Reading LCDC [{address:X4}]");
                    return LCDC;
                case 0xFF44:
                    Console.WriteLine($"Reading LY [{address:X4}]");
                    return LY;
                case 0xFF45:
                    Console.WriteLine($"Reading LYC [{address:X4}]");
                    return LYC;
                case 0xFF41:
                    Console.WriteLine($"Reading LCD STAT [{address:X4}]");
                    return STAT;
                case 0xFF42:
                    Console.WriteLine($"Reading SCY [{address:X4}]");
                    return SCY;
                case 0xFF43:
                    Console.WriteLine($"Reading SCX [{address:X4}]");
                    return SCX;
                case 0xFF4A:
                    Console.WriteLine($"Reading WY [{address:X4}]");
                    return WY;
                case 0xFF4B:
                    Console.WriteLine($"Reading WX [{address:X4}]");
                    return WX;
            }
        }
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0x8000 && address <= 0x9FFF) || 
               (address >= 0xFE00 && address <= 0xFE9F) ||
               (address >= 0xFF40 && address <= 0xFF4B);
    }





    public void Update(int cycles)
    {
        VerticalCyclesCount += cycles;
        // Check if LCD is enabled
        if ((LCDC & 0x80) == 0x80)
        {
            switch (Mode) // Switch mode
            {
                case 2: // OAM Scan
                    if (VerticalCyclesCount >= 80)
                    {
                        // Going to draw pixel
                        Mode = 0x3;
                        VerticalCyclesCount -= 80;
                    }
                    break;
                case 3: // Pixel draw
                    if (VerticalCyclesCount >= 172)
                    {
                        // Going to HBlank
                        Mode = 0x0;
                        VerticalCyclesCount -= 172;
                    }
                    break;
                case 0: // HBlank
                    if (VerticalCyclesCount >= 204)
                    {
                        LY++;
                        if((STAT & 0x40 ) == 0x40 && LY == LYC) Bus.Write(0xFF0F, 0x2); // STAT Interrupt if LY == LYC
                        if (LY > 144)
                        {
                            Mode = 0x1; // Switch to VBlank
                            VerticalCyclesCount -= 204;
                            // VBlankInterruptFlag()
                        }
                        else
                        {
                            Mode = 0x2; // Switch to OAM Scan for next visible line
                        }
                    }
                    break;
                case 1: // VBlank
                    if (VerticalCyclesCount >= 456)
                    {
                        LY++;
                        VerticalCyclesCount -= 456;
                        if (LY >= 153)
                        {
                            LY -= 153;
                            Mode = 0x2; // Switch to OAM Scan
                            // Write VBlank interrupt request flag
                            Bus.Write(0xFF0F, 0x1);
                        }
                    }
                    break;
            }  
        }
    }
}