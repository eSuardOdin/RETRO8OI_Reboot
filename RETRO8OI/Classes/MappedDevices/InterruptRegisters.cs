using RETRO8OI.Exceptions;

namespace RETRO8OI;

public class InterruptRegisters:IMemoryMappedDevice
{
    private byte IE;
    private byte IF;

    public InterruptRegisters()
    {
        IE = 0;
        IF = 0;
    }
    
    public void Write(ushort address, byte data)
    {
        if (address == 0xFF0F)
        {
            Console.WriteLine($"Writing [{data:X2}] to IF [{address:X4}]");
            IF |= data;
        }
        else
        {
            Console.WriteLine($"Writing [{data:X2}] to IE [{address:X4}]");
            IE |= data;
        }
    }

    public byte Read(ushort address)
    {
        if (address == 0xFF0F)
        {
            Console.WriteLine($"Reading IF [{address:X4}]");
            return IF;
        }
        Console.WriteLine($"Reading IE [{address:X4}]");
        return IE;
    }

    public bool Accept(ushort address)
    {
        return address == 0xFFFF || address == 0xFF0F;
    }
}