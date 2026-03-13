namespace RETRO8OI;

/// <summary>
/// Emulates the chip responsible for generating sound on the DMG
/// </summary>
public class Apu : IMemoryMappedDevice
{
    
    // --- Global control registers ---
    // FF26 - NR52: Audio master control
    public  byte NR52 { get; private set; }
    private bool IsAudioEnabled => (NR52 & 0x80) == 0x80;
    private bool IsCh1Enabled => (NR52 & 0x1) == 0x1;
    private bool IsCh2Enabled => (NR52 & 0x2) == 0x2;
    private bool IsCh3Enabled => (NR52 & 0x4) == 0x4;
    private bool IsCh4Enabled => (NR52 & 0x8) == 0x8;
    
    // FF25 - NR51: Sound panning (enable R/L channels of the four wave channels)
    public byte NR51 { get; private set; }
    
    // FF24 - NR50: Master volume + VIN panning
    public byte NR50 { get; private set; }
    public int VolumeR => NR50 & 0x7;
    public int VolumeL => (NR51 >> 4) & 0x7;
    
    
    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case 0xFF26:
                NR52 &= (byte)(data & 0x80);
                break;
            case 0xFF25:
                NR51 = data;
                break;
            case 0xFF24:
                NR50 = data;
                break;
        }
    }

    public byte Read(ushort address)
    {
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0xFF10 && address <= 0xFF26) || (address >= 0xFF30 && address <= 0xFF3F);
    }
}