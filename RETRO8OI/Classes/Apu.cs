namespace RETRO8OI;

/// <summary>
/// Emulates the chip responsible for generating sound on the DMG
/// </summary>
public class Apu : IMemoryMappedDevice
{
    
    // --- Global control registers ---
    // ** FF26 - NR52: Audio master control **                          (https://gbdev.io/pandocs/Audio_Registers.html#ff25--nr51-sound-panning)
    public  byte NR52 { get; private set; }
    private bool IsAudioEnabled => (NR52 & 0x80) == 0x80;
    private bool IsCh1Enabled => (NR52 & 0x1) == 0x1;
    private bool IsCh2Enabled => (NR52 & 0x2) == 0x2;
    private bool IsCh3Enabled => (NR52 & 0x4) == 0x4;
    private bool IsCh4Enabled => (NR52 & 0x8) == 0x8;
    
    // ** FF25 - NR51: Sound panning **                         
    public byte NR51 { get; private set; }
    
    // ** FF24 - NR50: Master volume + VIN panning **                   (https://gbdev.io/pandocs/Audio_Registers.html#ff24--nr50-master-volume--vin-panning)
    public byte NR50 { get; private set; }
    public int VolumeR => NR50 & 0x7;
    public int VolumeL => (NR51 >> 4) & 0x7;
    
    
    // --- Channel 1 Control registers ---
    // ** FF10 - NR10: Channel 1 sweep **                               (https://gbdev.io/pandocs/Audio_Registers.html#sound-channel-1--pulse-with-period-sweep)
    public byte NR10 { get; private set; }
    private int Ch1Pace => (NR10 & 0x70) >> 4;
    private bool Ch1DirectionIsSubstraction => (NR10 & 0x8) == 0x8;
    private int Ch1IndividualStep => NR10 & 0x7;
    
    // ** FF11 - NR11: Channel 1 length timer and duty cycle **
    public byte NR11 { get; private set; }
    private int Ch1WaveDuty => (NR11 & 0xC0) >> 6;
    private int Ch1InitLenTimer => NR11 & 0x3F;
    
    // ** FF12 - NR12: Channel 1 volume & envelope **                   (https://gbdev.io/pandocs/Audio_Registers.html#ff12--nr12-channel-1-volume--envelope)
    public byte NR12 { get; private set; }
    private int Ch1InitVolume => (NR12 & 0xF0) >> 4;
    private bool Ch1IsEnvDirectionIncreasing => (NR12 & 0x4) == 0x4;
    private int Ch1SweepSpace => NR12 & 0x7;
    
    
    // ** FF13 - NR13: Channel 1 period low [write-only] **             (https://gbdev.io/pandocs/Audio_Registers.html#ff13--nr13-channel-1-period-low-write-only)
    public byte NR13 { get; private set; }
    
    // ** FF14 - NR14: Channel 1 period high & control **               (https://gbdev.io/pandocs/Audio_Registers.html#ff14--nr14-channel-1-period-high--control)
    public byte NR14 { get; private set; }
    private bool Ch1IsLengthEnable => (NR14 & 0x40)  == 0x40;

    private int Ch1Period => ((NR14 & 0x7) << 8) | NR13;
    private int Ch1CurrentPeriodDivider; 
    
    
    
    
    
    
    
    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            // Global registers
            case 0xFF26:
                NR52 &= (byte)(data & 0x80);
                break;
            case 0xFF25:
                NR51 = data;
                break;
            case 0xFF24:
                NR50 = data;
                break;
            // Ch1 registers
            case 0xFF10:
                NR10 = data;
                break;
            case 0xFF11:
                NR11 = data;
                break;
            case 0xFF12:
                NR12 = data;
                break;
            case 0xFF13:
                NR13 = data;
                break;
            case 0xFF14:
                NR14 = data;
                break;
        }
    }

    public byte Read(ushort address)
    {
        switch (address)
        {
            
            case 0xFF26:
                return NR52;
            case 0xFF25:
                return NR51;
            case 0xFF24:
                return NR50;
        }
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0xFF10 && address <= 0xFF26) || (address >= 0xFF30 && address <= 0xFF3F);
    }
}