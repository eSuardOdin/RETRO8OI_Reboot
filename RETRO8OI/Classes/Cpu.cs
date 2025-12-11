namespace RETRO8OI;

enum Register8 { A, B, C, D, E, H, L, F }
enum Register16 { AF, BC, DE, HL }

public class Cpu
{
    public MemoryBus Bus { get; private set; }
    
    // Registers
    private byte A;
    private byte F 
    { 
        get;
        set => F = (byte)(value & 0xF0);
    }
    private byte B; private byte C; private byte D; private byte E; private byte H; private byte L; 
    private ushort AF{ get { return (ushort)(A << 8 | F); } set { A = (byte)(value >> 8); F = (byte)(value & 0xFF); } }
    private ushort BC { get { return (ushort)(B << 8 | C); } set { B = (byte)(value >> 8); C = (byte)(value & 0xFF); } }
    private ushort DE { get { return (ushort)(D << 8 | E); } set { D = (byte)(value >> 8); E = (byte)(value & 0xFF); } }
    private ushort HL { get { return (ushort)(H << 8 | L); } set { H = (byte)(value >> 8); L = (byte)(value & 0xFF); } }
    private ushort SP;
    private ushort PC;
    
    public Cpu(MemoryBus bus)
    {
        Bus = bus;
        PC = 0x100;
    }


    public int Execute()
    {
        // Get the current opcode
        byte opcode = Bus.Read(PC++);
        int cycles = 0;
        
        // --- Decode ---
        switch (opcode & 0xF0)
        {
            case 0x0:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // NOP
                        return 4;
                    case 0x1:   // LD BC,n16
                        BC = (ushort)(Bus.Read((ushort)(PC+1)) << 8 | Bus.Read(PC));
                        PC += 2;
                        return 12;
                    case 0x2:   //LD [BC], A
                        A = Bus.Read(BC);
                        return 8;
                    case 0x3:   // INC BC
                        BC += 1;
                        return 8;
                    case 0x4:   // INC B
                        break;
                    case 0x5:
                        break;
                    case 0x6:   // LD B, n8
                        B = Bus.Read(PC++);
                        return 8;
                    case 0x7:
                        break;
                    case 0x8:   // LD [a16] SP
                        ushort address = (ushort)(Bus.Read((ushort)(PC + 1)) << 8 | Bus.Read(PC));
                        Bus.Write(address, (byte)(SP & 0xFF));
                        Bus.Write((ushort)(address+1), (byte)((SP >> 8) & 0xFF));
                        return 20;
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x1:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x2:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x3:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x4:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x5:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x6:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x7:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x8:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0x9:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0xA:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0xB:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0xC:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0xD:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0xE:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            case 0xF:
                switch (opcode & 0x0F)
                {
                    case 0x0:
                        break;
                    case 0x1:
                        break;
                    case 0x2:
                        break;
                    case 0x3:
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        break;
                    case 0x6:
                        break;
                    case 0x7:
                        break;
                    case 0x8:
                        break;
                    case 0x9:
                        break;
                    case 0xA:
                        break;
                    case 0xB:
                        break;
                    case 0xC:
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        break;
                    case 0xF:
                        break;
                    default:
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                }
                break;
            
            
            default:
                throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
        }
        
        
        Console.WriteLine($"[{PC:X2}] => {opcode:X2}");
        
        
        return opcode;
    }
    
    
}