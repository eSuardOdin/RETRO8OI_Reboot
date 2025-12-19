namespace RETRO8OI;

public class Serial : IMemoryMappedDevice
{
    private byte SB = 0;
    private byte SC = 0;
    
    public void Write(ushort address, byte data)
    {
        
        if (address == 0xFF01)
        {
            Console.WriteLine($"Writing [{data:X2}] to Serial transfer data [{address:X4}]");
            SB = data;
            return;
        }
        Console.WriteLine($"Writing [{data:X2}] to Serial transfer control [{address:X4}]");
        SC = data;
    }

    public byte Read(ushort address)
    {
        if (address == 0xFF01)
        {
            Console.WriteLine($"Reading Serial transfer data [{address:X4}]");
            return SB;    
        }
        Console.WriteLine($"Reading Serial transfer control [{address:X4}]");
        return SC;
    }

    public bool Accept(ushort address)
    {
        return address == 0xFF01 || address == 0xFF02;
    }
}
