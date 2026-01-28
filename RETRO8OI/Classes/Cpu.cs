
namespace RETRO8OI;




public class Cpu
{
    // CPU Events
    // public event EventHandler<bool> StopModeToggled; // TODO When working on STOP instruction 
    
    public MemoryBus Bus { get; private set; }

    private Ppu Ppu;
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
    private ushort SP;
    public ushort PC { get; private set; }

    private bool IsOamDma;

    private int PpuMode = 0;
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

    private bool IME;
    public bool IMEEnable;
    private bool Halted;
    private bool HaltBug;
    
    // Constructor
    public Cpu(MemoryBus bus, Ppu ppu)
    {
        IsOamDma = false;
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
        HaltBug = false;
        
        // Awful but used to get ppu mode
        Ppu = ppu;
        ppu.ModeSwitchEvent += SetPpuMode;
    }

    private void SetPpuMode(object? sender, int e)
    {
        PpuMode = e;
    }


    public int ExecuteNextInstruction()
    {
        
        // Enable IME if EI on previous instruction
        if (IMEEnable)
        {
            IME = true;
            IMEEnable = false;
        }
        // Get the current opcode
        byte opcode = Bus.Read(PC++);
        // If halt bug
        if (HaltBug)
        {
            PC--;
            HaltBug = false;
        }
        
        
        int cycles = 0;
        // --- Decode ---
        switch ((opcode & 0xF0)>>4)
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
                    case 0x0:   // RET NZ
                        return RET(true, !FlagZ);
                    case 0x1:   // POP BC
                        BC = POP();
                        return 12;
                    case 0x2:   // JP NZ, a16
                        return JP(true, !FlagZ);
                    case 0x3:   // JP a16
                        return JP(true, true);  // isConditional variable is lame
                    case 0x4:   // CALL NZ, a16
                        return CALL(true, !FlagZ);
                    case 0x5:   // PUSH BC
                        PUSH(BC);
                        return 16;
                    case 0x6:   // ADD A, n8
                        ADD(Bus.Read(PC++));
                        return 8;
                    case 0x7:   // RST 0X00
                        return RST(0x0000);
                    case 0x8:   // RET Z
                        return RET(true, FlagZ);
                    case 0x9:   // RET
                        return RET(false, true);
                    case 0xA:   // JP Z, a16
                        return JP(true, FlagZ);
                    case 0xB:   // Prefixed opcode
                        return PREFIX();
                    case 0xC:   // CALL Z, a16
                        return CALL(true, FlagZ);
                    case 0xD:   // CALL a16
                        return CALL(false, true);
                    case 0xE:   // ADC A, n8
                        ADC(Bus.Read(PC++));
                        return 8;
                    case 0xF:   // RST 0x08
                        return RST(0x0008);
                }
                break;
            
            
            case 0xD:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // RET NC
                        return RET(true, !FlagC);
                    case 0x1:   // POP DE
                        DE = POP();
                        return 12;
                    case 0x2:   // JP NC, a16
                        return JP(true, !FlagC);
                    case 0x3:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0x4:   // CALL NC, a16
                        return CALL(true, !FlagC);
                    case 0x5:   // PUSH DE
                        PUSH(DE);
                        return 16;
                    case 0x6:   // SUB A, n8
                        SUB(Bus.Read(PC++));
                        return 8;
                    case 0x7:   // RST 0X10
                        return RST(0x0010);
                    case 0x8:   // RET C
                        return RET(true, FlagC);
                    case 0x9:   // RETI
                        RET(false, false);
                        IME = true;
                        return 16;
                    case 0xA:   // JP C, a16
                        return JP(true, FlagC);
                    case 0xB:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0xC:   // CALL C, a16
                        return CALL(true, FlagC);
                    case 0xD:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0xE:   // SBC A, n8
                        SBC(Bus.Read(PC++));
                        return 8;
                    case 0xF:   // RST 0X18
                        return RST(0x0018);
                }
                break;
            
            
            case 0xE:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // LDH [a8] A
                        return LDH(true, Bus.Read(PC++));
                    case 0x1:   // POP HL
                        HL = POP();
                        return 12;
                    case 0x2:   // LDH [C], A
                        Bus.Write((ushort)(0xFF00 | C), A);
                        return 8;
                    case 0x3:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0x4:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0x5:   // PUSH HL
                        PUSH(HL);
                        return 16;
                    case 0x6:   // AND A, n8
                        AND(Bus.Read(PC++));
                        return 8;
                    case 0x7:   // RST 0X20
                        return RST(0x0020);
                    case 0x8:   // ADD SP, e8
                        sbyte data = (sbyte)Bus.Read(PC++);
                        // Set flags
                        FlagC = (SP + data) > 0xFFFF;
                        FlagH = ((SP >> 8) & 0xF) + (data & 0xF) > 0xF;
                        FlagZ = false;
                        FlagN = false;
                        SP = (ushort)(SP + data);
                        return 16;
                    case 0x9:   // JP HL
                        PC = HL;
                        return 4;
                    case 0xA:   // LD [a16] A
                        byte lo = Bus.Read(PC);
                        byte hi = Bus.Read((ushort)(PC + 1));
                        PC += 2;
                        Bus.Write((ushort)((hi << 8) | lo), A);
                        return 16;
                    case 0xB:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0xC:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0xD:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0xE:   // XOR A, n8
                        XOR(Bus.Read(PC++));
                        return 8;
                    case 0xF:   // RST 0X28
                        return RST(0x0028);
                }
                break;
            
            
            case 0xF:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // LDH A, [a8]
                        return LDH(false, Bus.Read(PC++));
                    case 0x1:   // POP AF
                        AF = POP();
                        return 12;
                    case 0x2:   // LDH A, [C]
                        A = Bus.Read((ushort)(0xFF00 | C));
                        return 8;
                    case 0x3:   // DI
                        IME = false;
                        return 4;
                    case 0x4:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0x5:   // PUSH AF
                        PUSH(AF);
                        return 16;
                    case 0x6:   // OR A, n8
                        OR(Bus.Read(PC++));
                        return 8;
                    case 0x7:   // RST 0X30
                        return RST(0x0030);
                    case 0x8:   // LD HL, SP + e8
                        sbyte data = (sbyte)Bus.Read(PC++);
                        // Set flags
                        FlagC = (SP + data) > 0xFFFF;
                        FlagH = ((SP >> 8) & 0xF) + (data & 0xF) > 0xF;
                        FlagZ = false;
                        FlagN = false;
                        HL = (ushort)(SP + data);
                        return 12;
                    case 0x9:   // LD SP, HL
                        SP = HL;
                        return 8;
                    case 0xA:   // LD A, [a16]
                        byte lo = Bus.Read(PC);
                        byte hi = Bus.Read((ushort)(PC + 1));
                        PC += 2;
                        A = Bus.Read((ushort)( (hi << 8) | lo) );
                        return 16;
                    case 0xB:   // EI
                        IMEEnable = true;
                        return 4;
                    case 0xC:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0xD:   // ILLEGAL
                        throw new NotImplementedException($"Instruction [{opcode}] not implemented (ILLEGAL OPCODE).");
                    case 0xE:   // CP A, n8
                        CP(Bus.Read(PC++));
                        return 8;
                    case 0xF:   // RST 0x38
                        return RST(0x0038);
                }
                break;
        }
        
        return opcode;
    }

    
    // Flagged and/or Conditionals Instructions

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

    private int LDH(bool isLoadFromAccumulator, byte address)
    {
        // If accumulator data to 0xFFxx
        if (isLoadFromAccumulator)
        {
            Bus.Write((ushort)(0xFF00 | address), A);
        }
        // If [0xFFxx] to accumulator
        else
        {
            A = Bus.Read((ushort)(0xFF00 | address));
        }
        return 12;
    }
    
    
    private int HLADD(ushort operandReg)
    {
        // Setting flags
        FlagN = false;
        FlagH = (((HL & 0x0FFF) + (operandReg & 0x0FFF)) & 0x1000) == 0x1000; // Check if nibble overflow
        FlagC = (HL + operandReg) > 0xFFFF;
        
        HL = (ushort)(HL + operandReg);
        return 8;
    }

    
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

    /// <summary>
    /// Fetches the word next to it and sets PC
    /// to it if not conditional jump or condition
    /// true.
    /// </summary>
    /// <param name="isConditional">If the jump is conditional</param>
    /// <param name="condition">The condition to check</param>
    /// <returns>The number of instruction cycles</returns>
    private int JP(bool isConditional, bool condition)
    {
        // Address read no matter the condition
        byte lo = Bus.Read(PC);
        byte hi =  Bus.Read((ushort)(PC + 1));
        ushort jpAddress = (ushort)((hi << 8) | lo);
        // Increment the program counter
        PC += 2;
        
        // Get the return address from stack
        if (condition || !isConditional)
        {
            PC = jpAddress;
            return 16;
        }
        return 12;
    }
    
    /// <summary>
    /// Returns from a function - conditional or not
    /// </summary>
    /// <param name="isConditional">If the instruction is conditional</param>
    /// <param name="condition">The condition to check</param>
    /// <returns>The number of instruction cycle</returns>
    private int RET(bool isConditional, bool condition)
    {
        // Get the return address from stack
        if (condition || !isConditional)
        {
            PC = POP();
            return isConditional ? 20 : 16;
        }
        return 8;
    }

    /// <summary>
    /// Fetches the address NN next to PC, then if condition is met or call is not
    /// conditional :
    /// <list type="number">
    /// <item>Puts PC+1 to the stack in order to return from the function call</item>
    /// <item>Sets PC to NN to do an implicit jump</item>
    /// </list>
    /// </summary>
    ///<param name="isConditional">If the instruction is conditionnal</param>
    /// <param name="condition">The condition to check</param>
    /// <returns>The number of instruction cycle</returns>
    private int CALL(bool isConditional, bool condition)
    {
        byte hi = Bus.Read((ushort)(PC + 1));
        byte lo = Bus.Read(PC);
        ushort address = (ushort)((hi << 8) | lo);
        PC += 2;
        if (condition || !isConditional)
        {
            PUSH(PC);
            PC = address;
            return 16;
        }
        return 12;
    }

    /// <summary>
    /// Unconditional call for Interrupt routines
    /// </summary>
    /// <param name="address">The interrupt routine to</param>
    private int RST(ushort address)
    {
        PUSH(PC);
        PC = address;
        return 16;
    }
    
    
    /// <summary>
    /// Pops a word from the stack and increments it
    /// </summary>
    /// <returns>The word popped</returns>
    private ushort POP()
    {
        byte lo = Bus.Read(SP);
        byte hi = Bus.Read((ushort)(SP + 1));
        SP += 2;
        return (ushort)(hi << 8 | lo);
    }

    /// <summary>
    /// Decrements the stack and pushes a word to it
    /// </summary>
    /// <param name="value">The word to push</param>
    private void PUSH(ushort value)
    {
        SP -= 2;
        Bus.Write(SP, (byte)value);
        Bus.Write((ushort)(SP+1), (byte)(value >> 8));
        //Console.WriteLine($"Pushed {(Bus.Read((ushort)((SP+1) << 8)) | Bus.Read(SP)):X4} to [{SP:X4}]");
    }
    
    private void HALT()
    {
        if (IME)
        {
            Halted = true;
            PC--;   // To stay on HALT instruction
        }
        else
        {
            byte IE = Bus.Read(0xFFFF);
            byte IF = Bus.Read(0xFF0F);
            if ((IE & IF & (byte)0x1F) != 0)
            {
                HaltBug = true;
            }
            else
            {
                Halted = true;
            }
        }
    }
    
    
    // ---- Prefix OPCODES ----
    private int PREFIX()
    {
        byte opcode = Bus.Read(PC++); 
        // Switch OPCODE
        switch ((opcode & 0xF0) >> 4)
        {
            case 0x0:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // RLC B
                        B = RLC(B);
                        return 8;
                    case 0x1:   // RLC C
                        C = RLC(C);
                        return 8;
                    case 0x2:   // RLC D
                        D = RLC(D);
                        return 8;
                    case 0x3:   // RLC E
                        E = RLC(E);
                        return 8;
                    case 0x4:   // RLC H
                        H = RLC(H);
                        return 8;
                    case 0x5:   // RLC L
                        L = RLC(L);
                        return 8;
                    case 0x6:   // RLC [HL]
                        Bus.Write(HL, RLC(Bus.Read(HL)));
                        return 16;
                    case 0x7:   // RLC A
                        A = RLC(A);
                        return 8;
                    case 0x8:   // RRC B
                        B = RRC(B);
                        return 8;
                    case 0x9:   // RRC C
                        C = RRC(C);
                        return 8;
                    case 0xA:   // RRC D
                        D = RRC(D);
                        return 8;
                    case 0xB:   // RRC E
                        E = RRC(E);
                        return 8;
                    case 0xC:   // RRC H
                        H = RRC(H);
                        return 8;
                    case 0xD:   // RRC L
                        L = RRC(L);
                        return 8;
                    case 0xE:   // RRC [HL]
                        Bus.Write(HL, RRC(Bus.Read(HL)));
                        return 16;
                    case 0xF:   // RRC A
                        A = RRC(A);
                        return 8;
                }
                break;
            
            
            case 0x1:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // RL B
                        B = RL(B);
                        return 8;
                    case 0x1:   // RL C
                        C = RL(C);
                        return 8;
                    case 0x2:   // RL D
                        D = RL(D);
                        return 8;
                    case 0x3:   // RL E
                        E = RL(E);
                        return 8;
                    case 0x4:   // RL H
                        H = RL(H);
                        return 8;
                    case 0x5:   // RL L
                        L = RL(L);
                        return 8;
                    case 0x6:   // RL [HL]
                        Bus.Write(HL, RL(Bus.Read(HL)));
                        return 16;
                    case 0x7:   // RL A
                        A = RL(A);
                        return 8;
                    case 0x8:   // RR B
                        B = RR(B);
                        return 8;
                    case 0x9:   // RR C
                        C = RR(C);
                        return 8;
                    case 0xA:   // RR D
                        D = RR(D);
                        return 8;
                    case 0xB:   // RR E
                        E = RR(E);
                        return 8;
                    case 0xC:   // RR H
                        H = RR(H);
                        return 8;
                    case 0xD:   // RR L
                        L = RR(L);
                        return 8;
                    case 0xE:   // RR [HL]
                        Bus.Write(HL, RR(Bus.Read(HL)));
                        return 16;
                    case 0xF:   // RR A
                        A = RR(A);
                        return 8;
                }
                break;
            
            
            case 0x2:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // SLA B
                        B = SLA(B);
                        return 8;
                    case 0x1:   // SLA C
                        C = SLA(C);
                        return 8;
                    case 0x2:   // SLA D
                        D = SLA(D);
                        return 8;
                    case 0x3:   // SLA E
                        E = SLA(E);
                        return 8;
                    case 0x4:   // SLA H
                        H = SLA(H);
                        return 8;
                    case 0x5:   // SLA L
                        L = SLA(L);
                        return 8;
                    case 0x6:   // SLA [HL]
                        Bus.Write(HL, SLA(Bus.Read(HL)));
                        return 16;
                    case 0x7:   // SLA A
                        A = SLA(A);
                        return 8;
                    case 0x8:   // SRA B
                        B = SRA(B);
                        return 8;
                    case 0x9:   // SRA C
                        C = SRA(C);
                        return 8;
                    case 0xA:   // SRA D
                        D = SRA(D);
                        return 8;
                    case 0xB:   // SRA E
                        E = SRA(E);
                        return 8;
                    case 0xC:   // SRA H
                        H = SRA(H);
                        return 8;
                    case 0xD:   // SRA L
                        L = SRA(L);
                        return 8;
                    case 0xE:   // SRA [HL]
                        Bus.Write(HL, SRA(Bus.Read(HL)));
                        return 16;
                    case 0xF:   // SRA A
                        A = SRA(A);
                        return 8;
                }
                break;
            
            
            case 0x3:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // SWAP B
                        B = SWAP(B);
                        return 8;
                    case 0x1:   // SWAP C
                        C = SWAP(C);
                        return 8;
                    case 0x2:   // SWAP D
                        D = SWAP(D);
                        return 8;
                    case 0x3:   // SWAP E
                        E = SWAP(E);
                        return 8;
                    case 0x4:   // SWAP H
                        H = SWAP(H);
                        return 8;
                    case 0x5:   // SWAP L
                        L = SWAP(L);
                        return 8;
                    case 0x6:   // SWAP [HL]
                        Bus.Write(HL, SWAP(Bus.Read(HL)));
                        return 16;
                    case 0x7:   // SWAP A
                        A = SWAP(A);
                        return 8;
                    case 0x8:   // SRL B
                        B = SRL(B);
                        return 8;
                    case 0x9:   // SRL C
                        C = SRL(C);
                        return 8;
                    case 0xA:   // SRL D
                        D = SRL(D);
                        return 8;
                    case 0xB:   // SRL E
                        E = SRL(E);
                        return 8;
                    case 0xC:   // SRL H
                        H = SRL(H);
                        return 8;
                    case 0xD:   // SRL L
                        L = SRL(L);
                        return 8;
                    case 0xE:   // SRL [HL]
                        Bus.Write(HL, SRL(Bus.Read(HL)));
                        return 16;
                    case 0xF:   // SRL A
                        A = SRL(A);
                        return 8;
                }
                break;
            
            
            case 0x4:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // BIT 0, B
                        BIT(0x1,B);
                        return 8;
                    case 0x1:   // BIT 0, C
                        BIT( 0x1,C);
                        return 8;
                    case 0x2:   // BIT 0, D
                        BIT( 0x1,D);
                        return 8;
                    case 0x3:   // BIT 0, E
                        BIT( 0x1,E);
                        return 8;
                    case 0x4:   // BIT 0, H
                        BIT( 0x1,H);
                        return 8;
                    case 0x5:   // BIT 0, L
                        BIT( 0x1,L);
                        return 8;
                    case 0x6:   // BIT 0, [HL]
                        BIT( 0x1,Bus.Read(HL));
                        return 12;
                    case 0x7:   // BIT 0, A
                        BIT( 0x1,A);
                        return 8;
                    case 0x8:   // BIT 1, B
                        BIT( 0x2,B);
                        return 8;
                    case 0x9:   // BIT 1, C
                        BIT( 0x2,C);
                        return 8;
                    case 0xA:   // BIT 1, D
                        BIT( 0x2,D);
                        return 8;
                    case 0xB:   // BIT 1, E
                        BIT( 0x2,E);
                        return 8;
                    case 0xC:   // BIT 1, H
                        BIT( 0x2,H);
                        return 8;
                    case 0xD:   // BIT 1, L
                        BIT( 0x2,L);
                        return 8;
                    case 0xE:   // BIT 1, [HL]
                        BIT(0x2, Bus.Read(HL));
                        return 12;
                    case 0xF:   // BIT 1, A
                        BIT( 0x2,A);
                        return 8;
                }
                break;
            
            
            case 0x5:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // BIT 2, B
                        BIT(0x4,B);
                        return 8;
                    case 0x1:   // BIT 2, C
                        BIT( 0x4,C);
                        return 8;
                    case 0x2:   // BIT 2, D
                        BIT( 0x4,D);
                        return 8;
                    case 0x3:   // BIT 2, E
                        BIT( 0x4,E);
                        return 8;
                    case 0x4:   // BIT 2, H
                        BIT( 0x4,H);
                        return 8;
                    case 0x5:   // BIT 2, L
                        BIT( 0x4,L);
                        return 8;
                    case 0x6:   // BIT 2, [HL]
                        BIT( 0x4,Bus.Read(HL));
                        return 12;
                    case 0x7:   // BIT 2, A
                        BIT( 0x4,A);
                        return 8;
                    case 0x8:   // BIT 3, B
                        BIT( 0x8,B);
                        return 8;
                    case 0x9:   // BIT 3, C
                        BIT( 0x8,C);
                        return 8;
                    case 0xA:   // BIT 3, D
                        BIT( 0x8,D);
                        return 8;
                    case 0xB:   // BIT 3, E
                        BIT( 0x8,E);
                        return 8;
                    case 0xC:   // BIT 3, H
                        BIT( 0x8,H);
                        return 8;
                    case 0xD:   // BIT 3, L
                        BIT( 0x8,L);
                        return 8;
                    case 0xE:   // BIT 3, [HL]
                        BIT(0x8, Bus.Read(HL));
                        return 12;
                    case 0xF:   // BIT 3, A
                        BIT( 0x8,A);
                        return 8;
                }
                break;
            
            
            case 0x6:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // BIT 4, B
                        BIT(0x10,B);
                        return 8;
                    case 0x1:   // BIT 4, C
                        BIT( 0x10,C);
                        return 8;
                    case 0x2:   // BIT 4, D
                        BIT( 0x10,D);
                        return 8;
                    case 0x3:   // BIT 4, E
                        BIT( 0x10,E);
                        return 8;
                    case 0x4:   // BIT 4, H
                        BIT( 0x10,H);
                        return 8;
                    case 0x5:   // BIT 4, L
                        BIT( 0x10,L);
                        return 8;
                    case 0x6:   // BIT 4, [HL]
                        BIT( 0x10,Bus.Read(HL));
                        return 12;
                    case 0x7:   // BIT 4, A
                        BIT( 0x10,A);
                        return 8;
                    case 0x8:   // BIT 5, B
                        BIT( 0x20,B);
                        return 8;
                    case 0x9:   // BIT 5, C
                        BIT( 0x20,C);
                        return 8;
                    case 0xA:   // BIT 5, D
                        BIT( 0x20,D);
                        return 8;
                    case 0xB:   // BIT 5, E
                        BIT( 0x20,E);
                        return 8;
                    case 0xC:   // BIT 5, H
                        BIT( 0x20,H);
                        return 8;
                    case 0xD:   // BIT 5, L
                        BIT( 0x20,L);
                        return 8;
                    case 0xE:   // BIT 5, [HL]
                        BIT(0x20, Bus.Read(HL));
                        return 12;
                    case 0xF:   // BIT 5, A
                        BIT( 0x20,A);
                        return 8;
                }
                break;
            
            
            case 0x7:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // BIT 6, B
                        BIT(0x40,B);
                        return 8;
                    case 0x1:   // BIT 6, C
                        BIT( 0x40,C);
                        return 8;
                    case 0x2:   // BIT 6, D
                        BIT( 0x40,D);
                        return 8;
                    case 0x3:   // BIT 6, E
                        BIT( 0x40,E);
                        return 8;
                    case 0x4:   // BIT 6, H
                        BIT( 0x40,H);
                        return 8;
                    case 0x5:   // BIT 6, L
                        BIT( 0x40,L);
                        return 8;
                    case 0x6:   // BIT 6, [HL]
                        BIT( 0x40,Bus.Read(HL));
                        return 12;
                    case 0x7:   // BIT 6, A
                        BIT( 0x40,A);
                        return 8;
                    case 0x8:   // BIT 7, B
                        BIT( 0x80,B);
                        return 8;
                    case 0x9:   // BIT 7, C
                        BIT( 0x80,C);
                        return 8;
                    case 0xA:   // BIT 7, D
                        BIT( 0x80,D);
                        return 8;
                    case 0xB:   // BIT 7, E
                        BIT( 0x80,E);
                        return 8;
                    case 0xC:   // BIT 7, H
                        BIT( 0x80,H);
                        return 8;
                    case 0xD:   // BIT 7, L
                        BIT( 0x80,L);
                        return 8;
                    case 0xE:   // BIT 7, [HL]
                        BIT(0x80, Bus.Read(HL));
                        return 12;
                    case 0xF:   // BIT 7, A
                        BIT( 0x80,A);
                        return 8;
                }
                break;
            
            
            case 0x8:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // RES 0, B
                        B = RES(0x1,B);
                        return 8;
                    case 0x1:   // RES 0, C
                        C = RES( 0x1,C);
                        return 8;
                    case 0x2:   // RES 0, D
                        D = RES( 0x1,D);
                        return 8;
                    case 0x3:   // RES 0, E
                        E = RES( 0x1,E);
                        return 8;
                    case 0x4:   // RES 0, H
                        H = RES( 0x1,H);
                        return 8;
                    case 0x5:   // RES 0, L
                        L = RES( 0x1,L);
                        return 8;
                    case 0x6:   // RES 0, [HL]
                        Bus.Write(HL, RES( 0x1,Bus.Read(HL)));
                        return 16;
                    case 0x7:   // RES 0, A
                        A = RES( 0x1,A);
                        return 8;
                    case 0x8:   // RES 1, B
                        B = RES( 0x2,B);
                        return 8;
                    case 0x9:   // RES 1, C
                        C = RES( 0x2,C);
                        return 8;
                    case 0xA:   // RES 1, D
                        D = RES( 0x2,D);
                        return 8;
                    case 0xB:   // RES 1, E
                        E = RES( 0x2,E);
                        return 8;
                    case 0xC:   // RES 1, H
                        C = RES( 0x2,H);
                        return 8;
                    case 0xD:   // RES 1, L
                        L = RES( 0x2,L);
                        return 8;
                    case 0xE:   // RES 1, [HL]
                        Bus.Write(HL, RES( 0x2,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // RES 1, A
                        A = RES( 0x2,A);
                        return 8;
                }
                break;
            
            
            case 0x9:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // RES 2, B
                        B = RES(0x4,B);
                        return 8;
                    case 0x1:   // RES 2, C
                        C = RES( 0x4,C);
                        return 8;
                    case 0x2:   // RES 2, D
                        D = RES( 0x4,D);
                        return 8;
                    case 0x3:   // RES 2, E
                        E = RES( 0x4,E);
                        return 8;
                    case 0x4:   // RES 2, H
                        H = RES( 0x4,H);
                        return 8;
                    case 0x5:   // RES 2, L
                        L = RES( 0x4,L);
                        return 8;
                    case 0x6:   // RES 2, [HL]
                        Bus.Write(HL, RES( 0x4,Bus.Read(HL)));
                        return 16;
                    case 0x7:   // RES 2, A
                        A = RES( 0x4,A);
                        return 8;
                    case 0x8:   // RES 3, B
                        B = RES( 0x8,B);
                        return 8;
                    case 0x9:   // RES 3, C
                        C = RES( 0x8,C);
                        return 8;
                    case 0xA:   // RES 3, D
                        D = RES( 0x8,D);
                        return 8;
                    case 0xB:   // RES 3, E
                        E = RES( 0x8,E);
                        return 8;
                    case 0xC:   // RES 3, H
                        H = RES( 0x8,H);
                        return 8;
                    case 0xD:   // RES 3, L
                        L = RES( 0x8,L);
                        return 8;
                    case 0xE:   // RES 3, [HL]
                        Bus.Write(HL, RES( 0x8,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // RES 3, A
                        A = RES( 0x8,A);
                        return 8;
                }
                break;
            
            
            case 0xA:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // RES 4, B
                        B = RES(0x10,B);
                        return 8;
                    case 0x1:   // RES 4, C
                        C = RES( 0x10,C);
                        return 8;
                    case 0x2:   // RES 4, D
                        D = RES( 0x10,D);
                        return 8;
                    case 0x3:   // RES 4, E
                        E = RES( 0x10,E);
                        return 8;
                    case 0x4:   // RES 4, H
                        H = RES( 0x10,H);
                        return 8;
                    case 0x5:   // RES 4, L
                        L = RES( 0x10,L);
                        return 8;
                    case 0x6:   // RES 4, [HL]
                        Bus.Write(HL, RES( 0x10,Bus.Read(HL)));;
                        return 16;
                    case 0x7:   // RES 4, A
                        A = RES( 0x10,A);
                        return 8;
                    case 0x8:   // RES 5, B
                        B = RES( 0x20,B);
                        return 8;
                    case 0x9:   // RES 5, C
                        C = RES( 0x20,C);
                        return 8;
                    case 0xA:   // RES 5, D
                        D = RES( 0x20,D);
                        return 8;
                    case 0xB:   // RES 5, E
                        E = RES( 0x20,E);
                        return 8;
                    case 0xC:   // RES 5, H
                        H = RES( 0x20,H);
                        return 8;
                    case 0xD:   // RES 5, L
                        L = RES( 0x20,L);
                        return 8;
                    case 0xE:   // RES 5, [HL]
                        Bus.Write(HL, RES( 0x20,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // RES 5, A
                        A = RES( 0x20,A);
                        return 8;
                }
                break;
            
            
            case 0xB:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // RES 6, B
                        B = RES(0x40,B);
                        return 8;
                    case 0x1:   // RES 6, C
                        C = RES( 0x40,C);
                        return 8;
                    case 0x2:   // RES 6, D
                        D = RES( 0x40,D);
                        return 8;
                    case 0x3:   // RES 6, E
                        E = RES( 0x40,E);
                        return 8;
                    case 0x4:   // RES 6, H
                        H = RES( 0x40,H);
                        return 8;
                    case 0x5:   // RES 6, L
                        L = RES( 0x40,L);
                        return 8;
                    case 0x6:   // RES 6, [HL]
                        Bus.Write(HL, RES( 0x40,Bus.Read(HL)));
                        return 16;
                    case 0x7:   // RES 6, A
                        A = RES( 0x40,A);
                        return 8;
                    case 0x8:   // RES 7, B
                        B = RES( 0x80,B);
                        return 8;
                    case 0x9:   // RES 7, C
                        C = RES( 0x80,C);
                        return 8;
                    case 0xA:   // RES 7, D
                        D = RES( 0x80,D);
                        return 8;
                    case 0xB:   // RES 7, E
                        E = RES( 0x80,E);
                        return 8;
                    case 0xC:   // RES 7, H
                        H = RES( 0x80,H);
                        return 8;
                    case 0xD:   // RES 7, L
                        L = RES( 0x80,L);
                        return 8;
                    case 0xE:   // RES 7, [HL]
                        Bus.Write(HL, RES( 0x80,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // RES 7, A
                        A = RES( 0x80,A);
                        return 8;
                }
                break;
            
            
            case 0xC:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // SET 0, B
                        B = SET(0x1,B);
                        return 8;
                    case 0x1:   // SET 0, C
                        C = SET( 0x1,C);
                        return 8;
                    case 0x2:   // SET 0, D
                        D = SET( 0x1,D);
                        return 8;
                    case 0x3:   // SET 0, E
                        E = SET( 0x1,E);
                        return 8;
                    case 0x4:   // SET 0, H
                        H = SET( 0x1,H);
                        return 8;
                    case 0x5:   // SET 0, L
                        L = SET( 0x1,L);
                        return 8;
                    case 0x6:   // SET 0, [HL]
                        Bus.Write(HL, SET( 0x1,Bus.Read(HL)));
                        return 16;
                    case 0x7:   // SET 0, A
                        A = SET( 0x1,A);
                        return 8;
                    case 0x8:   // SET 1, B
                        B = SET( 0x2,B);
                        return 8;
                    case 0x9:   // SET 1, C
                        C = SET( 0x2,C);
                        return 8;
                    case 0xA:   // SET 1, D
                        D = SET( 0x2,D);
                        return 8;
                    case 0xB:   // SET 1, E
                        E = SET( 0x2,E);
                        return 8;
                    case 0xC:   // SET 1, H
                        C = SET( 0x2,H);
                        return 8;
                    case 0xD:   // SET 1, L
                        L = SET( 0x2,L);
                        return 8;
                    case 0xE:   // SET 1, [HL]
                        Bus.Write(HL, SET( 0x2,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // SET 1, A
                        A = SET( 0x2,A);
                        return 8;
                }
                break;
            
            
            case 0xD:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // SET 2, B
                        B = SET(0x4,B);
                        return 8;
                    case 0x1:   // SET 2, C
                        C = SET( 0x4,C);
                        return 8;
                    case 0x2:   // SET 2, D
                        D = SET( 0x4,D);
                        return 8;
                    case 0x3:   // SET 2, E
                        E = SET( 0x4,E);
                        return 8;
                    case 0x4:   // SET 2, H
                        H = SET( 0x4,H);
                        return 8;
                    case 0x5:   // SET 2, L
                        L = SET( 0x4,L);
                        return 8;
                    case 0x6:   // SET 2, [HL]
                        Bus.Write(HL, SET( 0x4,Bus.Read(HL)));
                        return 16;
                    case 0x7:   // SET 2, A
                        A = SET( 0x4,A);
                        return 8;
                    case 0x8:   // SET 3, B
                        B = SET( 0x8,B);
                        return 8;
                    case 0x9:   // SET 3, C
                        C = SET( 0x8,C);
                        return 8;
                    case 0xA:   // SET 3, D
                        D = SET( 0x8,D);
                        return 8;
                    case 0xB:   // SET 3, E
                        E = SET( 0x8,E);
                        return 8;
                    case 0xC:   // SET 3, H
                        H = SET( 0x8,H);
                        return 8;
                    case 0xD:   // SET 3, L
                        L = SET( 0x8,L);
                        return 8;
                    case 0xE:   // SET 3, [HL]
                        Bus.Write(HL, SET( 0x8,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // SET 3, A
                        A = SET( 0x8,A);
                        return 8;
                }
                break;
            
            
            case 0xE:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // SET 4, B
                        B = SET(0x10,B);
                        return 8;
                    case 0x1:   // SET 4, C
                        C = SET( 0x10,C);
                        return 8;
                    case 0x2:   // SET 4, D
                        D = SET( 0x10,D);
                        return 8;
                    case 0x3:   // SET 4, E
                        E = SET( 0x10,E);
                        return 8;
                    case 0x4:   // SET 4, H
                        H = SET( 0x10,H);
                        return 8;
                    case 0x5:   // SET 4, L
                        L = SET( 0x10,L);
                        return 8;
                    case 0x6:   // SET 4, [HL]
                        Bus.Write(HL, SET( 0x10,Bus.Read(HL)));;
                        return 16;
                    case 0x7:   // SET 4, A
                        A = SET( 0x10,A);
                        return 8;
                    case 0x8:   // SET 5, B
                        B = SET( 0x20,B);
                        return 8;
                    case 0x9:   // SET 5, C
                        C = SET( 0x20,C);
                        return 8;
                    case 0xA:   // SET 5, D
                        D = SET( 0x20,D);
                        return 8;
                    case 0xB:   // SET 5, E
                        E = SET( 0x20,E);
                        return 8;
                    case 0xC:   // SET 5, H
                        H = SET( 0x20,H);
                        return 8;
                    case 0xD:   // SET 5, L
                        L = SET( 0x20,L);
                        return 8;
                    case 0xE:   // SET 5, [HL]
                        Bus.Write(HL, SET( 0x20,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // SET 5, A
                        A = SET( 0x20,A);
                        return 8;
                }
                break;
            
            
            case 0xF:
                switch (opcode & 0x0F)
                {
                    case 0x0:   // SET 6, B
                        B = SET(0x40,B);
                        return 8;
                    case 0x1:   // SET 6, C
                        C = SET( 0x40,C);
                        return 8;
                    case 0x2:   // SET 6, D
                        D = SET( 0x40,D);
                        return 8;
                    case 0x3:   // SET 6, E
                        E = SET( 0x40,E);
                        return 8;
                    case 0x4:   // SET 6, H
                        H = SET( 0x40,H);
                        return 8;
                    case 0x5:   // SET 6, L
                        L = SET( 0x40,L);
                        return 8;
                    case 0x6:   // SET 6, [HL]
                        Bus.Write(HL, SET( 0x40,Bus.Read(HL)));
                        return 16;
                    case 0x7:   // SET 6, A
                        A = SET( 0x40,A);
                        return 8;
                    case 0x8:   // SET 7, B
                        B = SET( 0x80,B);
                        return 8;
                    case 0x9:   // SET 7, C
                        C = SET( 0x80,C);
                        return 8;
                    case 0xA:   // SET 7, D
                        D = SET( 0x80,D);
                        return 8;
                    case 0xB:   // SET 7, E
                        E = SET( 0x80,E);
                        return 8;
                    case 0xC:   // SET 7, H
                        H = SET( 0x80,H);
                        return 8;
                    case 0xD:   // SET 7, L
                        L = SET( 0x80,L);
                        return 8;
                    case 0xE:   // SET 7, [HL]
                        Bus.Write(HL, SET( 0x80,Bus.Read(HL)));
                        return 16;
                    case 0xF:   // SET 7, A
                        A = SET( 0x80,A);
                        return 8;
                }
                break;
        }
        return 0;
    }

    /// <summary>
    /// Rotates the register left, bit n°7 goes into FlagC and bit n°0
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The rotated value</returns>
    private byte RLC(byte registerVal)
    {
        FlagC = (registerVal & 0x80) == 0x80;
        FlagN = false;
        FlagH = false;
        byte retVal = FlagC ? (byte)((registerVal << 1) | 0x1) : (byte)(registerVal << 1);
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    /// <summary>
    /// Rotates the register left, bit n°7 goes into FlagC and FlagC goes to bit n°0
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The rotated value</returns>
    private byte RL(byte registerVal)
    {
        byte added = FlagC ? (byte)0x1 : (byte)0x0;
        FlagC = (registerVal & 0x80) == 0x80;
        FlagN = false;
        FlagH = false;
        byte retVal = (byte)((registerVal << 1) | added);
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    /// <summary>
    /// Rotates the register left arithmetically, bit n°7 goes into FlagC and 0 goes to bit n°0
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The rotated value</returns>
    private byte SLA(byte registerVal)
    {
        FlagC = (registerVal & 0x80) == 0x80;
        FlagN = false;
        FlagH = false;
        byte retVal = (byte)(registerVal << 1);
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    
    /// <summary>
    /// Rotates the register right, bit n°0 goes into FlagC and bit n°7
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The rotated value</returns>
    private byte RRC(byte registerVal)
    {
        FlagC = (registerVal & 0x80) == 0x80;
        FlagN = false;
        FlagH = false;
        byte retVal = FlagC ? (byte)((registerVal >> 1) | 0x1) : (byte)(registerVal << 1);
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    /// <summary>
    /// Rotates the register left, bit n°0 goes into FlagC and FlagC goes to bit n°7
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The rotated value</returns>
    private byte RR(byte registerVal)
    {
        byte added = FlagC ? (byte)0x80 : (byte)0x0;
        FlagC = (registerVal & 0x1) == 0x1;
        FlagN = false;
        FlagH = false;
        byte retVal = (byte)((registerVal >> 1) | added);
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    /// <summary>
    /// Rotates the register right arithmetically, bit n°0 goes into FlagC, bit n°7 STAYS THE SAME
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The rotated value</returns>
    private byte SRA(byte registerVal)
    {
        FlagC = (registerVal & 0x10) == 0x10;
        byte bit7 = (registerVal & 0x80) == 0x80 ? (byte)0x80 : (byte)0x0;
        FlagN = false;
        FlagH = false;
        byte retVal = (byte)((registerVal >> 1) | bit7);
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    /// <summary>
    /// Rotates the register left logically, bit n°0 goes into FlagC and 0 goes to bit n°7
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The rotated value</returns>
    private byte SRL(byte registerVal)
    {
        FlagC = (registerVal & 0x80) == 0x80;
        FlagN = false;
        FlagH = false;
        byte retVal = (byte)(registerVal >> 1);
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    /// <summary>
    /// Swap low nibble with high nibble of register
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    /// <returns>The swapped value</returns>
    private byte SWAP(byte registerVal)
    {
        FlagC = false;
        FlagN = false;
        FlagH = false;
        byte retVal = (byte)(((registerVal & 0xF) << 4) | ((registerVal & 0xF0) >> 4));
        FlagZ = retVal == 0x0;
        return retVal;
    }
    
    /// <summary>
    /// Tests if bit is set in the register, sets the FlagZ if not set
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    private void BIT(byte index, byte registerVal)
    {
        FlagN = false;
        FlagH = true;
        FlagZ = (registerVal & index) == index;
    }
    
    /// <summary>
    /// Sets the bit at index value
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    private byte SET(byte index, byte registerVal)
    {
        return (byte)(registerVal | index);
    }
    
    /// <summary>
    /// Unsets the bit at index value
    /// </summary>
    /// <param name="registerVal">The value of register (or memory address pointed to by HL)</param>
    private byte RES(byte index, byte registerVal)
    {
        byte notIndex = (byte)(~index);
        return (byte)(registerVal & notIndex);
    }



    /// <summary>
    /// This one is just to prevent write to forbidden mem in case OAM DMA is
    /// currently happening
    /// </summary>
    /// <param name="address"></param>
    /// <param name="data"></param>
    private void WriteBus(ushort address, byte data)
    {
        if (!IsOamDma || (address >= 0xFF80 && address <= 0xFFFE))
        {
            Bus.Write(address, data);
        }
        // If OAM DMA
        if (IsOamDma)
        {
            return;
        }
        // Check if VRAM Accessing
        if (address >= 0x8000 && address <= 0x9FFF)
        {
            if (PpuMode != 3)
            {
                Bus.Write(address, data);
            }
            return;
        }
        // Check if OAM Accessing
        if (address >= 0xFE00 && address <= 0xFE9F)
        {
            if (PpuMode < 2)
            {
                Bus.Write(address, data);
            }
        }
    }

    /// <summary>
    /// Just to check if bus is busy by OAM DMA before reading
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private byte ReadBus(ushort address)
    {
        // If OAM DMA
        if (IsOamDma)
        {
            if (address >= 0xFF80 && address <= 0xFFFE)
            {
                return Bus.Read(address);
            }

            return 0xFF;
        }
        // Check if VRAM Accessing
        if (address >= 0x8000 && address <= 0x9FFF)
        {
            if (PpuMode != 3)
            {
                return Bus.Read(address);
            }
            return 0xFF;
        }
        // Check if OAM Accessing
        if (address >= 0xFE00 && address <= 0xFE9F)
        {
            if (PpuMode < 2)
            {
                return Bus.Read(address);
            }
        }

        return 0xFF;
    }

    public void SetCpuOamDma(bool isOamDma)
    {
        IsOamDma = isOamDma;
    }
    
    

    public int HandleInterrupts()
    {
        if (!IME) return 0;
        // Compare interrupts flags and enables
        byte IE = Bus.Read(0xFFFF);
        byte IF = Bus.Read(0xFF0F);

        //Console.WriteLine($"Before handling interrupt: IE={IE:X2}, IF={IF:X2}, IME={IME}");
                
        for (int b = 0; b < 5; b++)
        {
            if(((IE & (1 << b)) == (1 << b) && (IF & (1 << b)) == (1 << b)))
            {
                return ExecInterrupt(b);
            }
        }
        
        return 0;
    }


    private int ExecInterrupt(int b)
    {
        int cycles = 0;
        String intStr;
        switch (b)
        {
            case 0 :
                intStr = $"VBlank";
                break;
            case 1 :
                intStr = "LCD";
                break;
            case 2 :
                intStr = "Timer";
                break;
            case 3 :
                intStr = "Serial";
                break;
            case 4 :
                intStr = "Joypad";
                break;
            default:
                intStr = "WTF";
                break;
        }
        //Console.WriteLine($"Before interrupt, LCDC = {Bus.Read(0xFF40):X4}, SP = {SP:X4}, PC = {PC:X4}");
        //Console.WriteLine($"Executing {intStr} interrupt. PC before: 0x{PC:X4}");
        //throw new Exception();
        if (Halted)
        {
            Halted = false;
            PC++;
            cycles += 4;
        }
        if (IME)
        {
            
            // Get return from interrupt vector value 
            PUSH(PC);
            PC = (ushort)(0x40 + (b * 8));
            // Disable IME
            IME = false;
            // Disable bit of interrupt flag
            byte IE = Bus.Read(0xFFFF);
            byte IF = Bus.Read(0xFF0F);
            IF &= (byte)~(1 << b);
            Bus.Write(0xFF0F, IF);
            //Console.WriteLine($"IME enabled, jumping to 0x{PC:X4}.");
            cycles += 16;
            //Console.WriteLine($"After handling interrupt: IE={IE:X2}, IF={IF:X2}, IME={IME}");
            //Console.WriteLine($"After interrupt, LCDC = {Bus.Read(0xFF40):X4}, SP = {SP:X4} ({(Bus.Read((ushort)((SP+1) << 8)) | Bus.Read(SP)):X4}), PC = {PC:X4}");

        }
        return cycles;
    }
    
}