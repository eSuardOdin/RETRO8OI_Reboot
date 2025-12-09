using RETRO8OI.Helpers;
using RETRO8OI.MBCS;
using Xunit.Abstractions;

namespace RETRO8OI.UnitTest;

public class MBC1Tests
{
    private readonly Cartridge cart;

    public MBC1Tests()
    {
        cart = new("/home/wan/repos/emu/roms/MBC1/LegendofZelda.gb");
    }

    private MBC1 GetMBC1()
    {
        return (MBC1)cart.Mbc;
    }

    private void ResetRegisters()
    {
        cart.Write(0x0000, 0x00);   // RAM GATE
        cart.Write(0x2000, 0x00);   // RAM BANK 1
        cart.Write(0x4000, 0x00);   // RAM BANK 2
        cart.Write(0x6000, 0x00);   // MODE
    }
    
    [Fact]
    public void RegisterInitIsOk()
    {
        Assert.Equal(0x0, GetMBC1().RamGateRegister);
        Assert.Equal(0x1, GetMBC1().BankRegister1);
        Assert.Equal(0x0, GetMBC1().BankRegister2);
        Assert.Equal(0x0, GetMBC1().Mode);
    }
    
    /// <summary>
    /// See if RamGateRegister ignores the bits out of its range
    /// </summary>
    [Fact]
    public void RamRegisterGateSetting()
    {
        cart.Write(0x0000, 0b11111111);
        Assert.Equal(0b00001111, GetMBC1().RamGateRegister);
        cart.Write(0x0000, 0xCA);
        Assert.Equal(0xA, GetMBC1().RamGateRegister);
    }
}