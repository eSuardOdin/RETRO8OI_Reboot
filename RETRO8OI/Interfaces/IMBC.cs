namespace RETRO8OI;

/// <summary>
/// MBC is the chip granting access to ROM/RAM data for the CPU even with the limit of 16-bit addressing.<br/>
/// It does so by bank switching of the memory at 
/// </summary>
public interface IMBC
{
    public void Write(ushort address, byte data);
    public byte Read(ushort address);
}