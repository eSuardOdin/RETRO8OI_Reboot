using RETRO8OI.Helpers;

namespace RETRO8OI.MBCS;
/// <summary>
/// The basic cartridge.
/// <para>The maximum ROM is 32KiB and CPU does not need bank switching to access memory.
/// It can potentially contain up to 8KiB of RAM with the addition of a battery or not.</para>
/// <para><i>Pandocs says no licensed games use the RAM or RAM+BATTERY version.</i></para>
/// </summary>
public class NoMBC : IMBC
{
    private byte[] _rom;
    private byte[]? _ram;

    public NoMBC(byte[] rom, byte[]? ram)
    {
        this._rom = rom;
        this._ram = ram;
    }

    public byte Read(ushort address)
    {
        // Read in ROM
        if(address <= MemoryMap.ROM_BANK_N_END) return _rom[address];
        // Read in RAM
        if(_ram != null)
        {
            int ramAddress = address - MemoryMap.EXTERNAL_RAM_START;
            if(ramAddress < _ram.Length)
            {
                return _ram[ramAddress];
            }
        }
        // Bad addressing return value
        return 0xFF;
    }

    public void Write(ushort address, byte data)
    {
        if( _ram != null && 
            address >= MemoryMap.EXTERNAL_RAM_START && address <= MemoryMap.EXTERNAL_RAM_END
          )
        {
            int ramAddress = address - MemoryMap.EXTERNAL_RAM_START;
            if(ramAddress < _ram.Length)
            {
                _ram[ramAddress] = data;
            }
            
        }

    }
}
