namespace RETRO8OI;

public class Timer : IMemoryMappedDevice
{
    private MemoryBus Bus;
    private const int DIV_M_CYCLES = 256;
    private byte TIMA { get; set; }
    private byte TMA { get; set; }
    private byte TAC { get; set; }
    private int TimaTicks = 0;

    private bool isTimaEnabled => (TAC & 0x3) == 0x3;

    private int TIMA_M_CYCLES
    {
        get
        {
            switch (TAC & 0x3)
            {
                case 0x0: return 256;
                case 0x1: return 4;
                case 0x2: return 16;
                case 0x3: return 64;
                default: return 0;  // Whatever dumby
            }
        }
    }

    private byte DIV { get; set; }
    private int DivTicks = 0;
    private bool IsStopMode = false;    // To do, handle stop instruction impact on DIV register


    public Timer(MemoryBus bus)
    {
        Bus = bus;
    }

    public void UpdateTimers(int cycles)
    {
        UpdateTima(cycles);
        UpdateDiv(cycles);
    }
    
    /// <summary>
    /// DIV register is incremented at 16384Hz, each 256 M-Cycles
    /// </summary>
    /// <param name="cycles">Elapsed CPU cycles to get back</param>
    private void UpdateDiv(int cycles)
    {
        DivTicks += cycles;
        if (DivTicks >= DIV_M_CYCLES)
        {
            //Console.WriteLine($"DIV register incremented, now 0x{DIV:X2}.");
            DIV++;
            DivTicks -= DIV_M_CYCLES;
        }
    }


    private void UpdateTima(int cycles)
    {
        if (isTimaEnabled)
        {
            TimaTicks += cycles;
            if (TimaTicks >= TIMA_M_CYCLES)
            {
                
                if (TIMA == 0xFF)
                {
                    TIMA = TMA;
                    //Console.WriteLine($"TIMA Overflowed, now 0x{TIMA:X2}.");
                    // Request TIMER interrupt
                    Bus.Write(0xFF0F, 0x3);
                }
                else
                {
                    //Console.WriteLine($"TIMA Incremented, now 0x{TIMA:X2}.");
                    TIMA++;
                }
            }
        }
        
    }
    
    public void Write(ushort address, byte data)
    {
        switch  (address)
        {
            case 0xFF04:
                //Console.WriteLine($"Writing in DIV, reset DIV.");
                DIV = 0;
                break;
            case 0xFF05:
                break;
            case 0xFF06:
                TMA = data;
                //Console.WriteLine($"Writing in TMA, TMA now equal 0x{TMA:X2}.");
                break;
            case 0xFF07:
                TAC = data;
                //Console.WriteLine($"Writing in TAC, TAC now equal 0x{TMA:X2}.");
                break;
        }
    }

    public byte Read(ushort address)
    {
        switch  (address)
        {
            case 0xFF04:
                return DIV;
            case 0xFF05:
                return TIMA;
            case 0xFF06:
                return TMA;
            case 0xFF07:
                return TAC;
        }

        return 0xFF;
    }

    public bool Accept(ushort address) => address >= 0xFF04 && address <= 0xFF07;
    
    
    
}