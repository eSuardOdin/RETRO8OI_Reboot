namespace RETRO8OI;

enum Register8 { A, B, C, D, E, H, L, F }
enum Register16 { AF, BC, DE, HL }

public class Cpu
{
    public MemoryBus Bus { get; private set; }
    
    // Registers
    private byte _f; // Backing field for flag reg
    private byte F
    {
        get => _f;
        set => _f = (byte)(value & 0xF0);
    }
    private byte A, B, C, D, E, H, L; 
    private ushort AF{ get { return (ushort)(A << 8 | F); } set { A = (byte)(value >> 8); F = (byte)(value & 0xFF); } }
    private ushort BC { get { return (ushort)(B << 8 | C); } set { B = (byte)(value >> 8); C = (byte)(value & 0xFF); } }
    private ushort DE { get { return (ushort)(D << 8 | E); } set { D = (byte)(value >> 8); E = (byte)(value & 0xFF); } }
    private ushort HL { get { return (ushort)(H << 8 | L); } set { H = (byte)(value >> 8); L = (byte)(value & 0xFF); } }
    private ushort SP, PC;
    
    // Flags
    private bool FlagZ 
    { 
        get => (F & 0x80) != 0;
        set => F = value ? (byte)(F | 0x80) : (byte)(F & ~0x80);
    }
    private bool FlagN 
    { 
        get => (F & 0x40) != 0;
        set => F = value ? (byte)(F | 0x40) : (byte)(F & ~0x40);
    }
    private bool FlagH 
    { 
        get => (F & 0x20) != 0;
        set => F = value ? (byte)(F | 0x20) : (byte)(F & ~0x20);
    }
    private bool FlagC 
    { 
        get => (F & 0x10) != 0;
        set => F = value ? (byte)(F | 0x10) : (byte)(F & ~0x10);
    }

    private bool IME, IMEEnable;
    private bool Halted;
    
    // Constructor
    public Cpu(MemoryBus bus)
    {
        Bus = bus;
        // Boot ROM exit status
        AF = 0x01B0;
        BC = 0x0013;
        DE = 0x00D8;
        HL = 0x014d;
        SP = 0xFFFE;
        PC = 0x100;

        IME = false;
        IMEEnable = false;
        Halted = false;
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
                        Bus.Write(BC, A);
                        return 8;
                    case 0x3:   // INC BC
                        BC++;
                        return 8;
                    case 0x4:   // INC B
                        B = INC(B);
                        return 4;
                    case 0x5:   // DEC B
                        B = DEC(B);
                        return 4;
                    case 0x6:   // LD B, n8
                        B = Bus.Read(PC++);
                        return 8;
                    case 0x7:   // RLCA : Rotate left circular
                        FlagZ = false;
                        FlagN = false;
                        FlagH = false;
                        // Get bit to go in carry
                        FlagC =  (A & 0x80) == 0x80;
                        byte lo = FlagC ? (byte)0x1 : (byte)0x0;
                        A = (byte)((A << 1) | lo);
                        return 4;
                    case 0x8:   // LD [a16] SP
                        ushort address = (ushort)(Bus.Read((ushort)(PC + 1)) << 8 | Bus.Read(PC));
                        Bus.Write(address, (byte)(SP & 0xFF));
                        Bus.Write((ushort)(address+1), (byte)((SP >> 8) & 0xFF));
                        return 20;
                    case 0x9:   // ADD HL, BC
                        return HLADD(BC);
                    case 0xA:   // LD A, [BC]
                        A = Bus.Read(BC);
                        return 8;
                    case 0xB:   // DEC BC
                        BC--;
                        return 8;
                    case 0xC:   // INC C
                        C = INC(C);
                        return 4;
                    case 0xD:   // DEC C
                        C = DEC(C);
                        return 4;
                    case 0xE:   // LD C, n8
                        C = Bus.Read(PC++);
                        return 8;
                    case 0xF:   // RRCA : Rotate left circular
                        FlagZ = false;
                        FlagN = false;
                        FlagH = false;
                        // Get bit to go in carry
                        FlagC =  (A & 0x1) == 0x1;
                        byte hi = FlagC ? (byte)0x80 : (byte)0x00;
                        A = (byte)( hi | (A >> 1) );
                        return 4;
                }
                break;
            
            
            case 0x1:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // STOP n8
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented.");
                    case 0x1:   // LD DE, n16
                        DE = (ushort)(Bus.Read((ushort)(PC+1)) << 8 | Bus.Read(PC));
                        PC += 2;
                        return 12;
                    case 0x2:   // LD [DE], A
                        Bus.Write(DE, A);
                        return 8;
                    case 0x3:   // INC DE
                        DE++;
                        return 8;
                    case 0x4:   // INC D
                        D = INC(D);
                        return 4;
                    case 0x5:   // DEC D
                        D = DEC(D);
                        return 4;
                    case 0x6:   // LD D, n8
                        D = Bus.Read(PC++);
                        return 8;
                    case 0x7:   // RLA
                        // Get carry bit
                        byte lo = FlagC ? (byte)0x1 : (byte)0x0;
                        // Set flags
                        FlagZ = false;
                        FlagN = false;
                        FlagH = false;
                        FlagC = (A & 0x80) == 0x80;
                        A = (byte)((A << 1) | lo); 
                        return 4;
                    case 0x8:   // JR e8
                        return JR(true);
                    case 0x9:   // ADD HL, DE
                        return HLADD(DE);
                    case 0xA:   // LD A, [DE]
                        A = Bus.Read(DE);
                        return 8;
                    case 0xB:   // DEC DE
                        DE--;
                        return 8;
                    case 0xC:   // INC E
                        E = INC(E);
                        return 4;
                    case 0xD:   // DEC E
                        E = DEC(E);
                        return 4;
                    case 0xE:   // LD E, n8
                        E = Bus.Read(PC++);
                        return 8;
                    case 0xF:   // RRA
                        // Get carry bit
                        byte hi = FlagC ? (byte)0x80 : (byte)0x0;
                        // Set flags
                        FlagZ = false;
                        FlagN = false;
                        FlagH = false;
                        FlagC = (A & 0x1) == 0x1;
                        A = (byte)(hi | (A >> 1)); 
                        return 4;
                }
                break;
            
            
            case 0x2:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // JR NZ, e8
                        return JR(!FlagZ);
                    case 0x1:   // LD HL, n16
                        HL = (ushort)(Bus.Read((ushort)(PC+1)) << 8 | Bus.Read(PC));
                        PC += 2;
                        return 12;
                    case 0x2:   // LD [HL+], A
                        Bus.Write(HL, A);
                        HL++;
                        return 8;
                    case 0x3:   // INC HL
                        HL++;
                        return 8;
                    case 0x4:   // INC H
                        H = INC(H);
                        return 4;
                    case 0x5:   // DEC H
                        H = DEC(H);
                        return 4;
                    case 0x6:   //LD H, n8
                        H = Bus.Read(PC++);
                        return 8;
                    case 0x7:   // DAA
                        return DAA();
                    case 0x8:   // JR Z, e8
                        return JR(FlagZ);
                    case 0x9:   // ADD HL, HL
                        return HLADD(HL);
                    case 0xA:   // LD A, [HL+]
                        A = Bus.Read(HL++);
                        return 8;
                    case 0xB:   // DEC HL
                        HL--;
                        return 8;
                    case 0xC:   // INC L
                        L = INC(L);
                        return 4;
                    case 0xD:   // DEC L
                        L = DEC(L);
                        return 4;
                    case 0xE:   // LD L, n8
                        L = Bus.Read(PC++);
                        return 8;
                    case 0xF:   // CPL
                        A = (byte)~A;
                        FlagN = true;
                        FlagH = true;
                        return 4;
                }
                break;
            
            
            case 0x3:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // JR NC, e8
                        return JR(!FlagC);
                    case 0x1:   // LD SP, n16
                        SP = (ushort)(Bus.Read((ushort)(PC+1)) << 8 | Bus.Read(PC));
                        PC += 2;
                        return 12;
                    case 0x2:   // LD [HL-], A
                        Bus.Write(HL, A);
                        HL--;
                        return 8;
                    case 0x3:   // INC SP
                        SP++;
                        return 8;
                    case 0x4:   // INC [HL]
                        Bus.Write(HL, INC(Bus.Read(HL)));
                        return 12;
                    case 0x5:   // DEC [HL]
                        Bus.Write(HL, DEC(Bus.Read(HL)));
                        return 12;
                    case 0x6:   // LD [HL], n8
                        Bus.Write(HL, Bus.Read(PC++));
                        return 12;
                    case 0x7:   // SCF
                        FlagC = true;
                        FlagN = false;
                        FlagH = false;
                        return 4;
                    case 0x8:   // JR C, e8
                        return JR(FlagC);
                    case 0x9:   // ADD HL, SP
                        return HLADD(SP);
                    case 0xA:   // LD A, [HL-]
                        A = Bus.Read(HL--);
                        return 8;
                    case 0xB:   // DEC SP
                        SP--;
                        return 8;
                    case 0xC:   // INC A
                        A = INC(A);
                        return 4;
                    case 0xD:   // DEC A
                        A = DEC(A);
                        return 4;
                    case 0xE:   // LD A, n8
                        A = Bus.Read(PC++);
                        return 8;
                    case 0xF:   // CCF
                        FlagC = !FlagC;
                        FlagN = false;
                        FlagH = false;
                        return 4;
                }
                break;
            
            
            case 0x4:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // LD B, B
                        return 4;
                    case 0x1:   // LD B, C
                        B = C;
                        return 4;
                    case 0x2:   // LD B, D
                        B = D;
                        return 4;
                    case 0x3:   // LD B, E
                        B = E;
                        return 4;
                    case 0x4:   // LD B, H
                        B = H;
                        return 4;
                    case 0x5:   // LD B, L
                        B = L;
                        return 4;
                    case 0x6:   // LD B, [HL]
                        B = Bus.Read(HL);
                        return 8;
                    case 0x7:   // LD B, A
                        B = A;
                        return 4;
                    case 0x8:   // LD C, B
                        C = B;
                        return 4;
                    case 0x9:   // LD C, C
                        return 4;
                    case 0xA:   // LD C, D
                        C = D;
                        return 4;
                    case 0xB:   // LD C, E
                        C = E;
                        return 4;
                    case 0xC:   // LD C, H
                        C = H;
                        return 4;
                    case 0xD:   // LD C, L
                        C = L;
                        return 4;
                    case 0xE:   // LD C, [HL]
                        C = Bus.Read(HL);
                        return 8;
                    case 0xF: // LD C, A
                        C = A;
                        return 4;
                }
                break;
            
            
            case 0x5:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // LD D, B
                        D = B;
                        return 4;
                    case 0x1:   // LD D, C
                        D = C;
                        return 4;
                    case 0x2:   // LD D, D
                        return 4;
                    case 0x3:   // LD D, E
                        D = E;
                        return 4;
                    case 0x4:   // LD D, H
                        D = H;
                        return 4;
                    case 0x5:   // LD D, L
                        D = L;
                        return 4;
                    case 0x6:   // LD D, [HL]
                        D = Bus.Read(HL);
                        return 8;
                    case 0x7:   // LD D, A
                        D = A;
                        return 4;
                    case 0x8:   // LD E, B
                        E = B;
                        return 4;
                    case 0x9:   // LD E, C
                        E = C;
                        return 4;
                    case 0xA:   // LD E, D
                        E = D;
                        return 4;
                    case 0xB:   // LD E, E
                        return 4;
                    case 0xC:   // LD E, H
                        E = H;
                        return 4;
                    case 0xD:   // LD E, L
                        E = L;
                        return 4;
                    case 0xE:   // LD E, [HL]
                        E = Bus.Read(HL);
                        return 8;
                    case 0xF: // LD E, A
                        E = A;
                        return 4;
                }
                break;
            
            
            case 0x6:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // LD H, B
                        H = B;
                        return 4;
                    case 0x1:   // LD H, C
                        H = C;
                        return 4;
                    case 0x2:   // LD H, D
                        H = D;
                        return 4;
                    case 0x3:   // LD H, E
                        H = E;
                        return 4;
                    case 0x4:   // LD H, H
                        return 4;
                    case 0x5:   // LD H, L
                        H = L;
                        return 4;
                    case 0x6:   // LD H, [HL]
                        H = Bus.Read(HL);
                        return 8;
                    case 0x7:   // LD H, A
                        H = A;
                        return 4;
                    case 0x8:   // LD L, B
                        L = B;
                        return 4;
                    case 0x9:   // LD L, C
                        L = C;
                        return 4;
                    case 0xA:   // LD L, D
                        L = D;
                        return 4;
                    case 0xB:   // LD L, E
                        L = E;
                        return 4;
                    case 0xC:   // LD L, H
                        L = H;
                        return 4;
                    case 0xD:   // LD L, L
                        return 4;
                    case 0xE:   // LD L, [HL]
                        L = Bus.Read(HL);
                        return 8;
                    case 0xF: // LD L, A
                        L = A;
                        return 4;
                }
                break;
            
            
            case 0x7:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // LD [HL], B
                        Bus.Write(HL, B);
                        return 8;
                    case 0x1:   // LD [HL], C
                        Bus.Write(HL, C);
                        return 8;
                    case 0x2:   // LD [HL], D
                        Bus.Write(HL, D);
                        return 8;
                    case 0x3:   // LD [HL], E
                        Bus.Write(HL, E);
                        return 8;
                    case 0x4:   // LD [HL], H
                        Bus.Write(HL, H);
                        return 8;
                    case 0x5:   // LD [HL], L
                        Bus.Write(HL, L);
                        return 8;
                    case 0x6:   // HALT
                        HALT();
                        return 4;
                    case 0x7:   // LD [HL], A
                        Bus.Write(HL, A);
                        return 8;
                    case 0x8:   // LD A, B
                        A = B;
                        return 4;
                    case 0x9:   // LD A, C
                        A = C;
                        return 4;
                    case 0xA:   // LD A, D
                        A = D;
                        return 4;
                    case 0xB:   // LD A, E
                        A = E;
                        return 4;
                    case 0xC:   // LD A, H
                        A = H;
                        return 4;
                    case 0xD:   // LD A, L
                        A = L;
                        return 4;
                    case 0xE:   // LD A, [HL]
                        A = Bus.Read(HL);
                        return 8;
                    case 0xF: // LD A, A
                        return 4;
                }
                break;
            
            
            case 0x8:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // ADD A, B
                        ADD(B);
                        return 4;
                    case 0x1:   // ADD A, C
                        ADD(C);
                        return 4;
                    case 0x2:   // ADD A, D
                        ADD(D);
                        return 4;
                    case 0x3:   // ADD A, E
                        ADD(E);
                        return 4;
                    case 0x4:   // ADD A, H
                        ADD(H);
                        return 4;
                    case 0x5:   // ADD A, L
                        ADD(L);
                        return 4;
                    case 0x6:   // ADD A, [HL]
                        ADD(Bus.Read(HL));
                        return 8;
                    case 0x7:   // ADD A, A
                        ADD(A);
                        return 4;
                    case 0x8:   // ADC A, B
                        ADC(B);
                        return 4;
                    case 0x9:   // ADC A, C
                        ADC(C);
                        return 4;
                    case 0xA:   // ADC A, D
                        ADC(D);
                        return 4;
                    case 0xB:   // ADC A, E
                        ADC(E);
                        return 4;
                    case 0xC:   // ADC A, H
                        ADC(H);
                        return 4;
                    case 0xD:   // ADC A, L
                        ADC(L);
                        return 4;
                    case 0xE:   // ADC A, [HL]
                        ADC(Bus.Read(HL));
                        return 8;
                    case 0xF:   // ADC A, A
                        ADC(A);
                        return 4;
                }
                break;
            
            
            case 0x9:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // SUB A, B
                        SUB(B);
                        return 4;
                    case 0x1:   // SUB A, C
                        SUB(C);
                        return 4;
                    case 0x2:   // SUB A, D
                        SUB(D);
                        return 4;
                    case 0x3:   // SUB A, E
                        SUB(E);
                        return 4;
                    case 0x4:   // SUB A, H
                        SUB(H);
                        return 4;
                    case 0x5:   // SUB A, L
                        SUB(L);
                        return 4;
                    case 0x6:   // SUB A, [HL]
                        SUB(Bus.Read(HL));
                        return 8;
                    case 0x7:   // SUB A, A
                        SUB(A);
                        return 4;
                    case 0x8:   // SBC A, B
                        SBC(B);
                        return 4;
                    case 0x9:   // SBC A, C
                        SBC(C);
                        return 4;
                    case 0xA:   // SBC A, D
                        SBC(D);
                        return 4;
                    case 0xB:   // SBC A, E
                        SBC(E);
                        return 4;
                    case 0xC:   // SBC A, H
                        SBC(H);
                        return 4;
                    case 0xD:   // SBC A, L
                        SBC(L);
                        return 4;
                    case 0xE:   // SBC A, [HL]
                        SBC(Bus.Read(HL));
                        return 8;
                    case 0xF:   // SBC A, A
                        SBC(A);
                        return 4;
                }
                break;
            
            
            case 0xA:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // AND A, B
                        AND(B);
                        return 4;
                    case 0x1:   // AND A, C
                        AND(C);
                        return 4;
                    case 0x2:   // AND A, D
                        AND(D);
                        return 4;
                    case 0x3:   // AND A, E
                        AND(E);
                        return 4;
                    case 0x4:   // AND A, H
                        AND(H);
                        return 4;
                    case 0x5:   // AND A, L
                        AND(L);
                        return 4;
                    case 0x6:   // AND A, [HL]
                        AND(Bus.Read(HL));
                        return 8;
                    case 0x7:   // AND A, A
                        AND(A);
                        return 4;
                    case 0x8:   // XOR A, B
                        XOR(B);
                        return 4;
                    case 0x9:   // XOR A, C
                        XOR(C);
                        return 4;
                    case 0xA:   // XOR A, D
                        XOR(D);
                        return 4;
                    case 0xB:   // XOR A, E
                        XOR(E);
                        return 4;
                    case 0xC:   // XOR A, H
                        XOR(H);
                        return 4;
                    case 0xD:   // XOR A, L
                        XOR(L);
                        return 4;
                    case 0xE:   // XOR A, [HL]
                        XOR(Bus.Read(HL));
                        return 8;
                    case 0xF:   // XOR A, A
                        XOR(A);
                        return 4;
                }
                break;
            
            
            case 0xB:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // OR A, B
                        OR(B);
                        return 4;
                    case 0x1:   // OR A, C
                        OR(C);
                        return 4;
                    case 0x2:   // OR A, D
                        OR(D);
                        return 4;
                    case 0x3:   // OR A, E
                        OR(E);
                        return 4;
                    case 0x4:   // OR A, H
                        OR(H);
                        return 4;
                    case 0x5:   // OR A, L
                        OR(L);
                        return 4;
                    case 0x6:   // OR A, [HL]
                        OR(Bus.Read(HL));
                        return 8;
                    case 0x7:   // OR A, A
                        OR(A);
                        return 4;
                    case 0x8:   // CP A, B
                        CP(B);
                        return 4;
                    case 0x9:   // CP A, C
                        CP(C);
                        return 4;
                    case 0xA:   // CP A, D
                        CP(D);
                        return 4;
                    case 0xB:   // CP A, E
                        CP(E);
                        return 4;
                    case 0xC:   // CP A, H
                        CP(H);
                        return 4;
                    case 0xD:   // CP A, L
                        CP(L);
                        return 4;
                    case 0xE:   // CP A, [HL]
                        CP(Bus.Read(HL));
                        return 8;
                    case 0xF:   // CP A, A
                        CP(A);
                        return 4;
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

    
    // Flagged and/or Conditionals Instructions

    #region 8-bit

    /// <summary>
    /// Decimal Adjust Accumulator.
    /// <para>Designed to be used after performing an arithmetic instruction whose inputs were in BCD,
    /// adjusts the result to be in BCD too.</para>
    /// <i>See doc on:</i> https://rgbds.gbdev.io/docs/v1.0.0/gbz80.7#DAA
    /// </summary>
    /// <returns>Number of cycles used</returns>
    private int DAA()
    {
        byte adjustement = 0;
        if (FlagN)
        {
            if (FlagH)
            {
                adjustement += 0x6;
            }
            if (FlagC)
            {
                adjustement += 0x60;
            }
            A -= adjustement;
        }
        else
        {
            if (FlagH || (A & 0xF) > 0x9)
            {
                adjustement += 0x6;
            }
            if (FlagC|| A > 0x99)
            {
                adjustement += 0x60;
                FlagC = true;
            }

            A += adjustement;
        }
        return 4;
    }
    
    /// <summary>
    /// Increments the value, set according flags and returns
    /// incremented value to be assigned to register
    /// </summary>
    /// <param name="regValue">The value to increment</param>
    /// <returns>Resulting value</returns>
    private byte INC(byte regValue)
    {
        int newVal = regValue + 1;
        // Flag setting
        FlagZ = (byte)newVal == 0;
        FlagN = false;
        FlagH = (regValue & 0x0F) == 0xF; // Check if nibble overflow
        return (byte)newVal;
    }

    /// <summary>
    /// Decrements the value, set according flags and returns
    /// decremented value to be assigned to register
    /// </summary>
    /// <param name="regValue">The value to decrement</param>
    /// <returns>Resulting value</returns>
    private byte DEC(byte regValue)
    {
        int newVal = regValue - 1;
        // Flag setting
        FlagZ = (byte)newVal == 0;
        FlagN = true;
        FlagH = (regValue & 0x0F) == 0;
        return (byte)newVal;
    }

    /// <summary>
    /// Add the value of a register to Accumulator
    /// </summary>
    /// <param name="operand">Value of the register</param>
    private void ADD(byte operand)
    {
        int newVal = A + operand;
        // Set flags
        FlagZ = (byte)newVal == 0;
        FlagN = false;
        FlagH = (((A & 0x0F) + (operand & 0x0F)) & 0x10) == 0x10;
        FlagC = newVal > 0xFF;

        A = (byte)newVal;
    }

    /// <summary>
    /// Adds the value + carry bit to accumulator
    /// </summary>
    /// <param name="operand">Value to add</param>
    private void ADC(byte operand)
    {
        int newVal = A + operand;
        newVal += FlagC ? 1 : 0;
        // Set flags
        FlagZ = (byte)newVal == 0;
        FlagN = false;
        FlagH = (((A & 0x0F) + (operand & 0x0F)) & 0x10) == 0x10;
        FlagC = newVal > 0xFF;

        A = (byte)newVal;
    }


    private void SUB(byte operand)
    {
        int newVal = A - operand;
        // Set flags
        FlagZ = (byte)newVal == 0;
        FlagN = true;
        FlagH = (A & 0x0F) < (operand & 0x0F);
        FlagC = operand > A;

        A = (byte)newVal;
    }
    
    
    private void SBC(byte operand)
    {
        int cb = FlagC ? 1 : 0;
        int newVal = A - operand - cb;
        // Set flags
        FlagZ = (byte)newVal == 0;
        FlagN = true;
        FlagH = (A & 0x0F) < ((operand + cb) & 0x0F);
        FlagC = (operand + cb) > A;

        A = (byte)newVal;
    }


    private void AND(byte operand)
    {
        A &= operand;
        FlagZ = A == 0;
        FlagN = false;
        FlagH = true;
        FlagC = false;
    }
    
    
    private void XOR(byte operand)
    {
        A ^= operand;
        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }
    
    
    private void OR(byte operand)
    {
        A |= operand;
        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }


    /// <summary>
    /// Compares value in register operand by substracting it to Accumulator
    ///<br/>Sets the flags but discard the result.
    /// </summary>
    /// <param name="operand">Value to compare to Accumulator</param>
    private void CP(byte operand)
    {
        int newVal = A - operand;
        // Set flags
        FlagZ = (byte)newVal == 0;
        FlagN = true;
        FlagH = (A & 0x0F) < (operand & 0x0F);
        FlagC = operand > A;
    }
    #endregion


    #region 16-bit

    private int HLADD(ushort operandReg)
    {
        // Setting flags
        FlagN = false;
        FlagH = (((HL & 0x0FFF) + (operandReg & 0x0FFF)) & 0x1000) == 0x1000; // Check if nibble overflow
        FlagC = (HL + operandReg) > 0xFFFF;
        
        HL = (ushort)(HL + operandReg);
        return 8;
    }

    #endregion
    
    private int JR(bool condition)
    {
        sbyte signedOffest = (sbyte)Bus.Read(PC++);
        if (condition)
        {
            PC = (ushort)(PC + signedOffest);    
            return 12;
        }
        return 8;
    }


    private void HALT()
    {
        // TODO (See cycle accurate gameboy emulator)
    }
}