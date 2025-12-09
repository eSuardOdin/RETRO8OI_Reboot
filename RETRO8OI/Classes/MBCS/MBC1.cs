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
    public byte RamGateRegister { get; private set; }

    /// <summary>
    /// Accessed from 0x2000 to 0x3FFF - Write ONLY register
    /// <para>Used as the lower 5 bits of the ROM bank number when the CPU accesses 0x4000-0x7FFF</para>
    /// <para><i>This register does not allow the value 0x0 and write 0x1 if any 0x0</i><br/>
    /// This causes a bug when accessing 0x00, 0x20, 0x40 and 0x60 from 0x4000-0x7FFF memory area<br/>
    /// Bank 0x01, 0x21, 0x41, 0x61 are accessed instead.</para>
    /// </summary>
    public byte BankRegister1 { get; private set; }

    /// <summary>
    /// Accessed from 0x4000 to 0x5FFF - Write ONLY register
    ///<para>Used as the upper 2 bits of the ROM bank number when the CPU accesses 0x4000-0x7FFF</para>
    /// <para><i>This register allows the writing of 0x0</i></para>
    /// </summary>
    public byte BankRegister2 { get; private set; }

    /// <summary>
    /// Accessed from 0x6000 to 0x7FFF - Write ONLY register
    /// <para>1-bit register to specify what bank Register 2 affects access to :<br/>
    /// Mode == 0 : BankRegister2 affect only accesses to 0x4000 - 0x7FFF<br/>
    /// Mode == 1 : BankRegister2 affects accesses to 0x000 - 0x3FFF (ROM 0), 0x4000 - 0x7FFF (ROM 1),
    /// 0xA000 - 0xBFFF (SRAM)</para>
    /// </summary>
    public byte Mode { get; private set; }
    
    
    public byte[] Rom { get; private set; }
    public byte[]? Ram { get; private set; }
    public MBC1(byte[] rom, byte[]? ram)
    {
        // Disable Ram Gate Register by default
        RamGateRegister = 0x0;
        // Initial value of Bank Register 1
        BankRegister1 = 0x1;
        // Initial value of Bank Register 2
        BankRegister2 = 0x0;
        // Initial value of Mode
        Mode = 0x0;
        
        // Set ROM and RAM
        Rom = rom;
        Ram = ram;
        
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
            WriteRam(data, address);
        }
        else
        {
            throw new InvalidBusRoutingException($"Address {address:X2} is out of cartridge mapped memory range.");
        }
    }


    public byte Read(ushort address)
    {
        // Get first banking ROM space (0000-3FFF)
        if(address <= 0x3FFF)
        {
            // If mode 0, just read address
            if (Mode == 0x0)
            {
                return Rom[address];    
            }
            // Else, add bank number * 0x4000
            return Rom[address + (GetRomBankMod1() * 0x4000)];
        }
        // Get second banking ROM space (4000-7FFF)
        if (address <= 0x7FFF)
        {
            int offset = address - 0x4000;
            return Rom[offset + (GetRomBank() * 0x4000)];
            
        }
        // RAM Space 
        if (RamGateRegister == 0xA && Ram != null && address >= 0xA000 && address <= 0xBFFF)
        {
            int offset = address - 0xA000;
            if (Mode == 0x0)
            {
                return Ram[offset % Ram.Length];
            }
            // Wrapping in case of not existing bank
            return Ram[(offset + (GetRamBank() * 0x2000)) % Ram.Length];
        }
        throw new InvalidBusRoutingException($"Address {address:X2} is out of cartridge mapped memory range.");
    }


    /// <summary>
    /// Writes the data to ram gate register by masking relevant bits.
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteRamGateRegister(byte data)
    {
        RamGateRegister = (byte)(data & 0xF);
    }
    
    /// <summary>
    /// Sets the bank Register 1<br/>
    /// Prevent the writing of 0x00.
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteBankRegister1(byte data)
    {
        // Get only the 5 relevant bits
        data &= 0b00011111;
        // Disable writing 0
        if (data == 0x0)
        {
            data = 0x1;
        }
        // Get only the 5 first bits
        BankRegister1 = (byte)(data & 0x3F);
    }
    
    /// <summary>
    /// Sets the bank Register 1<br/>
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteBankRegister2(byte data)
    {
        // Get only the 2 first bits
        BankRegister2 = (byte)(data & 0x3);
    }


    /// <summary>
    /// Sets or unset mode Register
    /// </summary>
    /// <param name="data">The data to write</param>
    private void WriteMode(byte data)
    {
        Mode = (byte)(data & 0x1);
    }

    /// <summary>
    /// Writes the specified data to RAM with banking depending on mode Register.
    /// </summary>
    /// <param name="data"></param>
    private void WriteRam(byte data, ushort address)
    {
        // Ignore writing if Ram gate Register disabled or no RAM
        if(RamGateRegister != 0xA || Ram == null) return;

        if (Mode == 0x0)
        {
            Ram[address] = data;
        }
        else
        {
            int offset =  address - 0xA000;
            Ram[(offset + (GetRamBank() * 0x2000)) % Ram.Length] = data;
        }
    }


    private ushort GetRomBank()
    {
        // Get the number of banks to mask it against the bank register
        ushort bankMask = (ushort)((Rom.Length / 0x4000) - 1);
        ushort bankRegister = (ushort)(BankRegister1 | (BankRegister2 << 5));
        return (ushort)(bankRegister & bankMask);
    }

    private ushort GetRomBankMod1()
    {
        // Get the number of banks to mask it against the bank register
        ushort bankMask = (ushort)((Rom.Length / 0x4000) - 1);
        ushort bankRegister = (ushort)(0x0 | (BankRegister2 << 5));
        return (ushort)(bankRegister & bankMask);
    }
        
    private byte GetRamBank()
    {
        if(Ram == null) return 0;
        return (byte)(BankRegister2 & 0x3);
    }


    public void DebugRegisters()
    {
        Console.WriteLine();
        Console.WriteLine("-------- MBC1 REGISTERS --------");
        Console.WriteLine($"RAM Gate Register      0b{RamGateRegister:b8} / 0x{RamGateRegister:X2}");
        Console.WriteLine($"BANK 1 Register        0b{BankRegister1:b8} / 0x{BankRegister1:X2}");
        Console.WriteLine($"BANK 2 Register        0b{BankRegister2:b8} / 0x{BankRegister2:X2}");
        Console.WriteLine($"Mode                   0b{Mode:b8} / 0x{Mode:X2}");
        Console.WriteLine("-------- MBC1 BANKS     --------");
        Console.WriteLine($"0x0000-0x3FFF Bank     0b{GetRomBankMod1():b8} / 0x{GetRomBankMod1():X2}");
        Console.WriteLine($"0x4000-0x7FFF Bank     0b{GetRomBank():b8} / 0x{GetRomBank():X2}");
        Console.WriteLine("********************************************************************");
        
    }
}