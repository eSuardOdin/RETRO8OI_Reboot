namespace RETRO8OI;

public class Gameboy
{
    public int Cycles { get; set; }
    public MemoryBus Bus { get; private set; }
    public Ram Ram { get; private set; }
    public Cpu Cpu { get; private set; }
    public Cartridge Cartridge { get; private set; }

    public Gameboy(String cartPath)
    {
        // Init of GB
        Bus = new MemoryBus();
        Ram = new Ram();
        Cartridge = new Cartridge(cartPath);
        Cpu = new Cpu(Bus);
        Cycles = 0;
        
        // Binding Memory devices to BUS
        Bus.Map(Ram);
        Bus.Map(Cartridge);
        
        
    }

}