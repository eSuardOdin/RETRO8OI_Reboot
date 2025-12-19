// See https://aka.ms/new-console-template for more information

using RETRO8OI;
using RETRO8OI.MBCS;

if (args.Length != 1)
{
    Console.WriteLine("Usage: ./RETRO8OI <ROMFILE>");
}
else if (!File.Exists(args[0]))
{
    Console.WriteLine("The ROM file at {0} was not found.", args[0]);
}
else
{
    try
    {
        Cartridge cartridge = new Cartridge(args[0]);
        Gameboy gameboy = new(args[0]);

        for(int i = 0; i < 0x10000; i++)
        {
            gameboy.Cpu.Execute();
            //gameboy.Cpu.PrintRegisters();
            //gameboy.Display.DrawVram();
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
    
    
    
    
}
