using System.ComponentModel;

namespace RETRO8OI;

public class Gameboy
{

    
    public int Cycles { get; set; }
    public MemoryBus Bus { get; private set; }
    public Ram Ram { get; private set; }
    //public OAM Oam { get; private set; }
    //public LCD Lcd { get; private set; }
    public InterruptRegisters InterruptRegisters { get; private set; }
    public Cpu Cpu { get; private set; }
    public Ppu Ppu { get; private set; }
    public Apu Apu { get; private set; }
    public Cartridge Cartridge { get; private set; }
    public Display Display { get; private set; }
    public Joypad Joypad { get; private set; }
    public Serial Serial { get; private set; }
    
    

    public Gameboy(String cartPath)
    {
        // Init of GB
        Bus = new MemoryBus();
        Ram = new Ram();
        //Lcd = new LCD();
        //Oam = new OAM();
        Ppu = new Ppu(Bus);
        Apu = new Apu();
        InterruptRegisters = new InterruptRegisters();
        Cartridge = new Cartridge(cartPath);
        Joypad = new Joypad();
        Serial = new Serial();
        
        
        Cpu = new Cpu(Bus);
        //Display = new Display(Bus, Ram);
        Cycles = 0;
        
        // Binding Memory devices to BUS
        Bus.Map(Ram);
        Bus.Map(Cartridge);
        Bus.Map(InterruptRegisters);
        //Bus.Map(Oam);
        //Bus.Map(Lcd);
        Bus.Map(Ppu);
        Bus.Map(Apu);
        Bus.Map(Joypad);
        Bus.Map(Serial);
    }

}