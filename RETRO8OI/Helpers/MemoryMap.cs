namespace RETRO8OI.Helpers;
public static class MemoryMap
{
    // ROM
    public const ushort ROM_BANK_0_START = 0x0000;
    public const ushort ROM_BANK_0_END = 0x3FFF;
    public const ushort ROM_BANK_N_START = 0x4000;
    public const ushort ROM_BANK_N_END = 0x7FFF;
    
    // VRAM
    public const ushort VRAM_START = 0x8000;
    public const ushort VRAM_END = 0x9FFF;
    public const ushort VRAM_SIZE = 0x2000; // 8KB
    
    // External RAM (Cartridge)
    public const ushort EXTERNAL_RAM_START = 0xA000;
    public const ushort EXTERNAL_RAM_END = 0xBFFF;
    public const ushort EXTERNAL_RAM_SIZE = 0x2000; // 8KB
    
    // Work RAM (WRAM)
    public const ushort WRAM_START = 0xC000;
    public const ushort WRAM_END = 0xDFFF;
    public const ushort WRAM_SIZE = 0x2000; // 8KB
    
    // Echo RAM
    public const ushort ECHO_RAM_START = 0xE000;
    public const ushort ECHO_RAM_END = 0xFDFF;
    
    // OAM (Object Attribute Memory)
    public const ushort OAM_START = 0xFE00;
    public const ushort OAM_END = 0xFE9F;
    public const ushort OAM_SIZE = 0xA0; // 160 bytes
    
    // Unusable
    public const ushort UNUSABLE_START = 0xFEA0;
    public const ushort UNUSABLE_END = 0xFEFF;
    
    // I/O Registers
    public const ushort IO_START = 0xFF00;
    public const ushort IO_END = 0xFF7F;
    
    // High RAM (HRAM)
    public const ushort HRAM_START = 0xFF80;
    public const ushort HRAM_END = 0xFFFE;
    public const ushort HRAM_SIZE = 0x7F; // 127 bytes
    
    // Interrupt Enable Register
    public const ushort IE_REGISTER = 0xFFFF;
}