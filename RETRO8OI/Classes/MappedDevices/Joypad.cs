namespace RETRO8OI;

public class Joypad : IMemoryMappedDevice
{
    private byte Register = 0;
    
    public void Write(ushort address, byte data)
    {
        Console.WriteLine($"Writing [{data:X2}] to Joypad register [{address:X4}]");
        Register = data;
    }

    public byte Read(ushort address)
    { 
        Console.WriteLine($"Reading Joypad register [{address:X4}]");
        return Register;
    }

    public bool Accept(ushort address)
    {
        return address == 0xFF00;
    }
}