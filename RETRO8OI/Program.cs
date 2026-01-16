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
        Gameboy gameboy = new();
        gameboy.LoadCart(args[0]);
        
        gameboy.Run();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
    
    
    
    
}
