using RETRO8OI.Exceptions;

namespace RETRO8OI;

public class Ram : IMemoryMappedDevice
{
    public byte[] Wram { get; private set; }
    public byte[] Hram { get; private set; }

    public Ram()
    {
        Wram = new byte[0x2000];
        Hram = new byte[0x7F];
    }
    
    public void Write(ushort address, byte data)
    {
        // Write WRAM
        if (address >= 0xC000 && address <= 0xDFFF)
        {
            Console.WriteLine($"Writing [{data:X2}] to WRAM [{address:X4}]");
            Wram[address - 0xC000] = data;
            return;
        }
        // Echo RAM
        if (address >= 0xE000 && address <= 0xFDFF)
        {
            Wram[address - 0xE000] = data;
            return;
        }
        // Write HRAM
        if (address >= 0xFF80 && address <= 0xFFFE)
        {
            Console.WriteLine($"Writing [{data:X2}] to HRAM [{address:X4}]");
            Hram[address - 0xFF80] = data;
            return;
        }
        // Prohibited mem
        if (address >= 0xFEA0 && address <= 0xFEFF)
        {
            Console.WriteLine($"Writing [{data:X2}] to PROHIBITED MEMORY [{address:X4}] -> no writing");
            return;
        }
        throw new InvalidBusRoutingException($"Error writing [{data:X2}] to [{address:X4}] in Ram.");
    }

    public byte Read(ushort address)
    {
        
        // Read WRAM
        if (address >= 0xC000 && address <= 0xDFFF)
        {
            Console.WriteLine($"Reading WRAM [{address:X4}]");
            return Wram[address - 0xC000];
        }
        // Echo RAM
        if (address >= 0xE000 && address <= 0xFDFF)
        {
            return Wram[address - 0xE000];
        }
        // HRAM
        if (address >= 0xFF80 && address <= 0xFFFE)
        {
            Console.WriteLine($"Reading HRAM [{address:X4}]");
            return Hram[address - 0xFF80];
        }
        // Prohibited mem
        if (address >= 0xFEA0 && address <= 0xFEFF)
        {
            Console.WriteLine($"Reading from PROHIBITED MEMORY [{address:X4}] -> sending garbage");
            return 0xFF;
        }
        throw new InvalidBusRoutingException($"Error reading [{address:X4}] in Ram.");
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0xC000 && address <= 0xDFFF) ||
               (address >= 0xE000 && address <= 0xFDFF) || 
               (address >= 0xFEA0 && address <= 0xFEFF) ||  // Prohibited by Nintendo, used somehow by Tetris... 
               (address >= 0xFF80 && address <= 0xFFFE);
    }
}