using System.ComponentModel;
using System.Diagnostics;
using RETRO8OI.Exceptions;

namespace RETRO8OI;

public class Gameboy
{
    private const int CPU_FREQ = 4194304; // Hz, so 4.19 MHz
    private const double FRAME_FREQ = 59.73; // Hz
    private int CYCLES_PER_FRAME = (int)(CPU_FREQ / FRAME_FREQ);
    
    
    private bool _runningGame = false;
    private bool _isOamDMA = false;
    public int Cycles { get; set; }
    public MemoryBus Bus { get; private set; }
    public Ram Ram { get; private set; }
    //public OAM Oam { get; private set; }
    //public LCD Lcd { get; private set; }
    public InterruptRegisters InterruptRegisters { get; private set; }
    public Cpu Cpu { get; private set; }
    public Ppu Ppu { get; private set; }
    public Apu Apu { get; private set; }
    public Timer Timer { get; private set; }
    public Cartridge? Cartridge { get; private set; }
    public Display Display { get; private set; }
    public Joypad Joypad { get; private set; }
    public Serial Serial { get; private set; }
    
    

    public Gameboy()
    {
        // Init of GB
        Bus = new MemoryBus();
        Ram = new Ram();
        //Lcd = new LCD();
        //Oam = new OAM();
        Timer = new Timer(Bus);
        Ppu = new Ppu(Bus);
        Ppu.OamDmaEvent += SetOamDma;
        Apu = new Apu();
        InterruptRegisters = new InterruptRegisters();
        Cartridge = null;
        Joypad = new Joypad();
        Serial = new Serial();
        
        
        Cpu = new Cpu(Bus);
        //Display = new Display(Bus, Ram);
        Cycles = 0;
        
        // Binding Memory devices to BUS
        Bus.Map(Ram);
        Bus.Map(InterruptRegisters);
        //Bus.Map(Oam);
        //Bus.Map(Lcd);
        Bus.Map(Ppu);
        Bus.Map(Apu);
        Bus.Map(Joypad);
        Bus.Map(Serial);
    }

    public void LoadCart(String cartPath)
    {
        Cartridge = new Cartridge(cartPath);
        if(Cartridge == null) return;
        Bus.Map(Cartridge);
    }
    
    private void SetOamDma(Object? sender, EventArgs e) => _isOamDMA = true;
    
    public void Run()
    {
        
        if (Cartridge == null)
        {
            throw new InvalidRomException("Cartridge not initialized");
        }
        
        // Get timespan after instructions with stopwatch
        Stopwatch sw = new();
        // CPU Cycles after executing an instruction
        int cycles = 0;
        int frame_cycles = 0;
        double frames = 0.0;
        _runningGame = true;
        
        var frameTime = TimeSpan.FromSeconds(1.0/FRAME_FREQ);
        sw.Start();
        
        while (_runningGame)
        {
            // Get true start
            var execStart = sw.Elapsed;
            
            // Execute the number of M-Cycles in a frame
            while (frame_cycles < CYCLES_PER_FRAME)
            {
                if (_isOamDMA)
                {
                    cycles = 160;
                    _isOamDMA = false;
                }
                else
                {
                    cycles = Cpu.Execute();
                }
                frame_cycles += cycles;
                //Ppu.Update(cycles);
                Timer.UpdateTimers(cycles);
            }
            //Console.WriteLine($"-- Frame lasted {(sw.Elapsed - execStart).TotalMilliseconds:F2} ms ({1000.0 / FRAME_FREQ})");
            
            // Frame number updated and remaining cycles saved for next frame
            frames++;
            frame_cycles -= CYCLES_PER_FRAME;
            
            // Wait for frame time
            var elapsed = sw.Elapsed - execStart;
            var remaining = frameTime - elapsed;
            if (remaining > TimeSpan.Zero)
            {
                if (remaining.TotalMilliseconds > 4.0)
                {
                    Thread.Sleep((int) (remaining.TotalMilliseconds - 2.0));
                }
                while((sw.Elapsed - execStart) < frameTime) {}
            }
            
            //Console.WriteLine($"Waited until {(sw.Elapsed - execStart).TotalMilliseconds:F2} ms ({1000.0 / FRAME_FREQ})");
            
            
            if (frames >= FRAME_FREQ)
            {
                Console.WriteLine("*DID A SECOND FRAME WORTH*");
                Console.WriteLine($"\n*** Second n°{sw.Elapsed.TotalSeconds:F2}, FRAMES : {frames:F2}");
                sw.Restart();
                frames -= FRAME_FREQ;
            }
            
        }
    }

}