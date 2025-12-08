using RETRO8OI.Exceptions;

namespace RETRO8OI.MBCS;

public class MBC1 : IMBC
{
    /// <summary>
    /// Accessed from 0x0000 to 0x1FFF - Write ONLY register
    /// <para>This 4 bits register enables/disables access to the cart SRAM when written to.</para>
    /// <para>0b1010 (0x6) => SRAM enable<br/>
    /// Any value => SRAM disable</para>
    /// <para><i>RAM access is DISABLED by default</i></para>
    /// </summary>
    private byte _ramGateRegister;

    /// <summary>
    /// Accessed from 0x2000 to 0x3FFF - Write ONLY register
    /// <para>Used as the lower 5 bits of the ROM bank number when the CPU accesses 0x4000-0x7FFF</para>
    /// <para><i>This register does not allow the value 0x0 and write 0x1 if any 0x0</i><br/>
    /// This causes a bug when accessing 0x00, 0x20, 0x40 and 0x60 from 0x4000-0x7FFF memory area<br/>
    /// Bank 0x01, 0x21, 0x41, 0x61 are accessed instead.</para>
    /// </summary>
    private byte _bankRegister1;

    /// <summary>
    /// Accessed from 0x4000 to 0x5FFF - Write ONLY register
    ///<para>Used as the upper 2 bits of the ROM bank number when the CPU accesses 0x4000-0x7FFF</para>
    /// <para><i>This register allows the writing of 0x0</i></para>
    /// </summary>
    private byte _bankRegister2;

    /// <summary>
    /// Accessed from 0x6000 to 0x7FFF - Write ONLY register
    /// <para>1-bit register to specify what bank Register 2 affects access to :<br/>
    /// _mode == 0 : _bankRegister2 affect only accesses to 0x4000 - 0x7FFF<br/>
    /// _mode == 1 : _bankRegister2 affects accesses to 0x000 - 0x3FFF (ROM 0), 0x4000 - 0x7FFF (ROM 1),
    /// 0xA000 - 0xBFFF (SRAM)</para>
    /// </summary>
    private byte _mode;
    
    
    private byte[] _rom;
    private byte[]? _ram;
    public MBC1(byte[] rom, byte[]? ram)
    {
        // Disable Ram Gate Register by default
        _ramGateRegister = 0x0;
        // Initial value of Bank Register 1
        _bankRegister1 = 0x1;
        // Initial value of Bank Register 2
        _bankRegister2 = 0x0;
        // Initial value of Mode
        _mode = 0x0;
        
        // Set ROM and RAM
        _rom = rom;
        _ram = ram;
        
        
        // Test
        _bankRegister1 = 0b10010;
        _bankRegister2 = 0b01;
        Console.WriteLine($"{GetFullBank():X2}");
    }
    
    /// <summary>
    /// Writing to the appropriate registers depending on target address
    /// or writing to RAM if any.
    /// </summary>
    /// <param name="address">16-bit address</param>
    /// <param name="data">data to change registers</param>
    /// <exception cref="InvalidBusRoutingException">Address out of range</exception>
    public void Write(ushort address, byte data)
    {
        // Write to ROM
        if (address <= 0x1FFF)
        {
            WriteRamGateRegister(data);
        }
        else if (address <= 0x3FFF)
        {
            WriteBankRegister1(data);
        }
        else if (address <= 0x5FFF)
        {
            WriteBankRegister2(data);
        }
        else if (address <= 0x7FFF)
        {
            WriteMode(data);
        }
        // Write to RAM
        else if (address >= 0xA000 && address <= 0xBFFF)
        {
            WriteRam(data);
        }
        else
        {
            throw new InvalidBusRoutingException($"Address {address:X2} is out of cartridge mapped memory range.");
        }
    }


    public byte Read(ushort address)
    {
        // If mode 0
        if (_mode == 0x0)
        {
            // First bank
            if(address <= 0x3FFF) return _rom[address];

            if (address <= 0x7FFF)
            {
                //Second bank with no switching
                if (GetFullBank() == 0x0)
                {
                    return _rom[address];
                }
                // Second bank with switching if any
                return _rom[address + (0x4000 * (GetFullBank() - 1))];
            }

            
        }
        // Read ROM Bank 0

        return 0xFF;
    }


    /// <summary>
    /// Writes the data to ram gate register by masking relevant bits.
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteRamGateRegister(byte data)
    {
        _ramGateRegister = (byte)(data & 0xF);
    }
    
    /// <summary>
    /// Sets the bank Register 1<br/>
    /// Prevent the writing of 0x00.
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteBankRegister1(byte data)
    {
        // Disable writing 0
        if (data == 0x0)
        {
            data = 0x1;
        }
        // Get only the 5 first bits
        _bankRegister1 = (byte)(data & 0x3F);
    }
    
    /// <summary>
    /// Sets the bank Register 1<br/>
    /// Prevent the writing of 0x00.
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteBankRegister2(byte data)
    {
        // Get only the 2 first bits
        _bankRegister2 = (byte)(data & 0x3);
    }


    /// <summary>
    /// Sets or unset mode Register
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteMode(byte data)
    {
        _mode = (byte)(data & 0x1);
    }

    /// <summary>
    /// Writes the specified data to RAM with banking depending on mode Register.
    /// </summary>
    /// <param name="data"></param>
    private void WriteRam(byte data)
    {
        // Ignore writing if Ram gate Register disabled or no RAM
        if(_ramGateRegister != 0xA || _ram == null) return;
        
    }


    private byte GetFullBank() => (byte) (_bankRegister1 | (_bankRegister2 << 5));
}