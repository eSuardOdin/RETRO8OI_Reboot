namespace RETRO8OI.UnitTest;

public class NoMBCTest
{
    /// <summary>
    /// This tests NoMBC ROM bank 0 Reading
    /// </summary>
    [Fact]
    public void TestRomBank0()
    {
        Cartridge cart = new("/home/wan/repos/emu/roms/NoMBC/Tetris.gb");
        // First bank
        ushort address = 0x0000;
        Assert.Equal(0xC3, cart.Read(address));
        address = 0x00C0;
        Assert.Equal(0xFE, cart.Read(address));
        address = 0x1A99;
        Assert.Equal(0xE8, cart.Read(address));
        address = 0x3FFF;
        Assert.Equal(0x2F, cart.Read(address));
    }
    /// <summary>
    /// This tests NoMBC ROM bank 1 Reading
    /// </summary>
    [Fact]
    public void TestRomBank1()
    {
        Cartridge cart = new("/home/wan/repos/emu/roms/NoMBC/Tetris.gb");
        // First bank
        ushort address = 0x4000;
        Assert.Equal(0x2F, cart.Read(address));
        address = 0x5CE8;
        Assert.Equal(0x00, cart.Read(address));
        address = 0x6502;
        Assert.Equal(0x69, cart.Read(address));
        address = 0x7FFF;
        Assert.Equal(0x00, cart.Read(address));
    }


    [Fact]
    public void RamReturnsFF()
    {
        Cartridge cart = new("/home/wan/repos/emu/roms/NoMBC/Tetris.gb");
        // First bank
        ushort address = 0xA000;
        Assert.Equal(0xFF, cart.Read(address));
        address = 0xAFED;
        Assert.Equal(0xFF, cart.Read(address));
        address = 0xB03C;
        Assert.Equal(0xFF, cart.Read(address));
        address = 0xBFFF;
        Assert.Equal(0xFF, cart.Read(address));
    }


    [Fact]
    public void OutOfRangeReturnsFF()
    {
        Cartridge cart = new("/home/wan/repos/emu/roms/NoMBC/Tetris.gb");
        ushort address = 0xE000;
        Assert.Equal(0xFF, cart.Read(address));
        address = 0xFFED;
        Assert.Equal(0xFF, cart.Read(address));
        address = 0xFF3C;
        Assert.Equal(0xFF, cart.Read(address));
        address = 0xCFFF;
        Assert.Equal(0xFF, cart.Read(address));
    }

}