namespace RETRO8OI.UnitTest;

public class CartridgeTest
{
    /// <summary>
    /// This tests the fact that only gameboy cartridge are loaded
    /// </summary>
    [Fact]
    public void LoadCartTest()
    {
        Cartridge cart = new("/home/wan/repos/emu/roms/Tetris.gb");
        //Cartridge cartFail = new("/home/wan/repos/emu/roms/Nogb.gb");
        
    }
}