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
            IF = data;
        }
        else
        {
            IE = data;
        }
    }

    public byte Read(ushort address)
    {
        if (address == 0xFF0F)
        {
            return IF;
        }

        return IE;
    }

    public bool Accept(ushort address)
    {
        return address == 0xFFFF || address == 0xFF0F;
    }
}