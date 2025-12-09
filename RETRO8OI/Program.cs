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
        var cart = new Cartridge(args[0]);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}



var zeldaTest = new Cartridge("/home/wan/repos/emu/roms/LegendofZelda.gb");
// Test MBC1
// View base register state
((MBC1)zeldaTest.Mbc).DebugRegisters();
// Test mode 1
ushort address = 0x6000;
byte modeOn = 0b11001001;
byte modeOff = 0b11001010;
Console.WriteLine();
Console.WriteLine($"*** Writing 0b{modeOn:b8} to 0x{address:X4} to change mode to SET. ***");
zeldaTest.Write(address, modeOn);
((MBC1)zeldaTest.Mbc).DebugRegisters();
// Test mode 0
address = 0x7FFF;
Console.WriteLine();
Console.WriteLine($"*** Writing 0b{modeOff:b8} to 0x{address:X4} to change mode to UNSET. ***");
zeldaTest.Write(address, modeOff);
((MBC1)zeldaTest.Mbc).DebugRegisters();
// Try writing 0 to bank register 1
address = 0x2000;
byte data = 0x0;
Console.WriteLine();
Console.WriteLine($"*** Writing 0b{data:b8} to 0x{address:X4} to TRY to unset BANK REGISTER 1 to 0 (must stay at 1). ***");
zeldaTest.Write(address, data);
((MBC1)zeldaTest.Mbc).DebugRegisters();
// Max value in BANK REGISTER 1
address = 0x3FFF;
data = 0xFF;
Console.WriteLine();
Console.WriteLine($"*** Writing 0b{data:b8} to 0x{address:X4} to BANK REGISTER 1 (Intended value: 0b00011111). ***");
zeldaTest.Write(address, data);
((MBC1)zeldaTest.Mbc).DebugRegisters();
// Try writing to BANK REGISTER 2 - Ignored because not enough space to get this much banking
address = 0x4000;
data = 0b11011010;
Console.WriteLine();
Console.WriteLine($"*** Writing 0b{data:b8} to 0x{address:X4} to BANK REGISTER 2 (Intended value: 0b00000010). ***");
zeldaTest.Write(address, data);
((MBC1)zeldaTest.Mbc).DebugRegisters();




// Try reading ROM
// Reading normal first two banks (disable bank before)
address = 0x2000;
zeldaTest.Write(address, 0);
((MBC1)zeldaTest.Mbc).DebugRegisters();

// First bank
/*address = 0x0000;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");
address = 0x3E19;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");
address = 0x3FFF;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");*/
// Second bank
address = 0x4000;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");
address = 0x40F9;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");
address = 0x4E19;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");
address = 0x6000;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");
address = 0x7FFF;
Console.WriteLine($"*** Reading ROM value at 0x{address:X4} NO BANKING ***");
Console.WriteLine($"*** Value is 0x{zeldaTest.Read(address):X2} ***");
Console.WriteLine($"*************************************************");