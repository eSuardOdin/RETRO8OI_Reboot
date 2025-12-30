using RETRO8OI.Exceptions;
using RETRO8OI.Helpers;
using RETRO8OI.MBCS;

namespace RETRO8OI;

public struct Header
{
    /// <summary>
    /// 4 bytes of entrypoint<br/>
    /// Located at $100 - $103
    /// </summary>
    public byte[] EntryPoint;

    /// <summary>
    /// 48 bytes of logo<br/>
    /// Located at $104 - $133
    /// </summary>
    public byte[] Logo;

    /// <summary>
    /// ASCII title contained in cart header<br/>
    /// Located at $134 - $143
    /// </summary>
    public byte[] Title;

    /// <summary>
    /// Useless<br/>
    /// Located at $13F - $142
    /// </summary>
    public byte[] Manufacturer;
    
    /// <summary>
    /// Checks if Color mode (CGB Mode) or monochrome compatibility <br/>
    /// Located at $143
    /// </summary>
    public byte CgbFlag;

    /// <summary>
    /// Two chars licensee code. Considered only if old licensee code is 0x33<br/>
    /// Located at $144 - $145
    /// </summary>
    public ushort NewLicCode;

    /// <summary>
    /// Specifies if game supports SGB functions (if set to $03)<br/>
    /// Located at $146
    /// </summary>
    public byte SgbFlag;

    /// <summary>
    /// Type of the cartridge<br/>
    /// Located at $147
    /// </summary>
    public byte Type;

    /// <summary>
    /// Size of the ROM, size is 32 * 1024 * (1 << romSize)<br/>
    /// Located at $148
    /// </summary>
    public byte RomSize;

    /// <summary>
    /// Size of the RAM<br/>
    /// Located at $149
    /// </summary>
    public byte RamSize;

    /// <summary>
    /// Specifies if intended to be sold in Japan or elsewhere<br/>
    /// Located at $14A
    /// </summary>
    public byte DestinationCode;

    /// <summary>
    /// Value $33 indicates that the New licensee code is to be considered<br/>
    /// Located at $14B
    /// </summary>
    public byte OldLicCode;

    /// <summary>
    /// Version number of the game<br/>
    /// Located at $14C
    /// </summary>
    public byte MaskRomVersionNumber;

    /// <summary>
    /// This byte contains an 8-bit checksum computed from the cartridge header bytes $0134–014C<br/>
    /// Located at $14D
    /// </summary>
    public byte HeaderChecksum;

    /// <summary>
    /// These bytes contain a 16-bit (big-endian) checksum simply computed as the sum of all the bytes of the cartridge ROM (except these two checksum bytes).<br/>
    /// Located at $14E - $14F
    /// </summary>
    public ushort GlobalCheckSum;
}

public class Cartridge : IMemoryMappedDevice
{
    public String Filename { get; private set; }
    private Header _header;
    public byte[] Rom { get; private set; }
    public byte[]? Ram { get; private set; }
    public bool HasBattery { get; private set; }
    private IMBC _mbc;
    public IMBC Mbc => _mbc; // For debugging purpose
    
    public Cartridge(String filename)
    {
        Rom = File.ReadAllBytes(filename);
        Filename = Path.GetFileName(filename);
        FillHeader();
        ComputeHeaderChecksum();
        // Check if battery present
        CheckHasBattery();
        // Init Ram (and load it from file if saved previously)
        InitRam();
        GetMBC();
        PrintCartInfos();
    }

    


    /// <summary>
    /// Values the IMemoryMappedDevices responds to
    /// </summary>
    /// <param name="address">The 16-bit address tested</param>
    /// <returns>If address is in the acceptable range for the device</returns>
    public bool Accept(ushort address) =>  address <= 0x7FFF || (address >= 0xA000 && address <= 0xBFFF);
    
    /// <summary>
    /// Tells the MBC to write data
    /// </summary>
    /// <param name="address">address to write (offset by banking if any)</param>
    /// <param name="data">data to write</param>
    public void Write(ushort address, byte data) => _mbc.Write(address, data);
    
    /// <summary>
    /// Tells the MBC to read data
    /// </summary>
    /// <param name="address">address to read from</param>
    /// <returns>The data at location (offset by banking if any)</returns>
    public byte Read(ushort address) => _mbc.Read(address);


    /// <summary>
    /// Uses ROM data to fill header of cartridge
    /// </summary>
    private void FillHeader()
    {
        // Get entrypoint
        _header.EntryPoint = new byte[4];
        Array.Copy(Rom, 0x100, _header.EntryPoint, 0, 4);
        
        // Nintendo Logo
        _header.Logo = new byte[0x133-0x103];
        Array.Copy(Rom, 0x104, _header.Logo, 0, 0x133 - 0x103);
        
        // Title
        _header.Title = new byte[0x143 - 0x133];
        Array.Copy(Rom, 0x134, _header.Title, 0, 0x143 - 0x133);
        
        // Manufacturer
        _header.Manufacturer = new byte[0x142 - 0x13E];
        Array.Copy(Rom, 0x134, _header.Title, 0, 0x142 - 0x13E);

        // CGB flag
        _header.CgbFlag = Rom[0x143];
        
        // New licensee code
        _header.NewLicCode = (ushort)((Rom[0x144] << 8) | Rom[0x145]);
        
        // SGB Flag
        _header.SgbFlag = Rom[0x146];
        
        // Cart type
        _header.Type = Rom[0x147];
        
        // Rom size
        _header.RomSize = Rom[0x148];
        
        // Ram size
        _header.RamSize = Rom[0x149];
        
        // Dest code
       _header.DestinationCode = Rom[0x14A];
       
       // Old licensee code
       _header.OldLicCode = Rom[0x14B];
       
       // Mask rom
       _header.MaskRomVersionNumber = Rom[0x14C];

       // Header checksum
       _header.HeaderChecksum = Rom[0x14D];
       
       // Global checksum
       _header.GlobalCheckSum = (ushort)((Rom[0x14E] << 8) | Rom[0x14F]);
    }

    /// <summary>
    /// Computes the checksum provided in Cart header
    /// </summary>
    /// <exception cref="InvalidRomException">If checksums does not match</exception>
    private void ComputeHeaderChecksum()
    {
        byte checksum = 0;
        for (ushort address = 0x0134; address <= 0x14C; address++)
        {
            checksum = (byte) (checksum - Rom[address] - 1);
        }

        if (checksum != _header.HeaderChecksum)
        {
            throw new InvalidRomException($"Header checksum {checksum:X2} doesn't match the cartridge value {_header.HeaderChecksum:X2}");
        }
    }

    /// <summary>
    /// Checks if battery is present for saving
    /// </summary>
    private void CheckHasBattery()
    {
        switch (_header.Type)
        {
            case 0x03:
            case 0x06:
            case 0x09:
            case 0x0D:
            case 0x0F:
            case 0x10:
            case 0x13:
            case 0x1B:
            case 0x1E:
            case 0x22:
            case 0xFF:
                HasBattery = true;
                break;
            default:
                HasBattery = false;
                break;
        }
    }

    /// <summary>
    /// Init the Ram memory;
    /// <para>If any battery present and a savefile present, load it as the Ram</para>
    /// </summary>
    private void InitRam()
    {
        // Check battery TODO
        switch (_header.RamSize)
        {
            // 8 KiB
            case 0x02:
                Ram = new byte[0x8 * (1 << 10)];
                break;
            // 32 KiB
            case 0x03:
                Ram = new byte[0x32 * (1 << 10)];
                break;
            // 128 KiB
            case 0x04:
                Ram = new byte[0x128 * (1 << 10)];
                break;
            // 64 KiB
            case 0x05:
                Ram = new byte[0x64 * (1 << 10)];
                break;
            // No RAM
            default:
                Ram = null;
                break;
        }
        
    }
    
    private void GetMBC()
    {
        switch (_header.Type)
        {
            case 0x00:
            case 0x08:
            case 0x09:
                _mbc = new NoMBC(Rom, Ram);
                break;
            case 0x01:
            case 0x02:
            case 0x03:
                _mbc = new MBC1(Rom, Ram);
                break;
            default:
                throw new InvalidMBCException($"The MBC type {_header.Type} ({HeaderConstants.GetCartType(_header.Type)}) is invalid or not implemented yet");
        }
        // No MBC
        
    }
    
    /// <summary>
    /// Debuggish thing, not sure i'll keep header as is anyway.
    /// </summary>
    private void PrintCartInfos()
    {
        // File name
        //Console.WriteLine($"Loaded cart:{Filename}");
        // Title
        //Console.WriteLine($"Title:{System.Text.Encoding.ASCII.GetString(_header.Title)}");
        // Licensee code
        var lic = _header.OldLicCode == 0x33 ? HeaderConstants.GetNewLicenseeName(_header.NewLicCode) : HeaderConstants.GetOldLicenseeName(_header.OldLicCode);
        //Console.WriteLine($"Licensee code: {lic}");
        // Cart type
        //Console.WriteLine($"Type: {HeaderConstants.GetCartType(_header.Type)}");
        
        // Sizes
        //Console.WriteLine($"Cart ROM size = {32*1024*(1<<_header.RomSize)} ({_header.RomSize.ToString("X2")})");
        switch (_header.RamSize)
        {
            case 0x02:
                //Console.WriteLine($"Cart RAM size = 8 KiB");
                break;
            case 0x03:
                //Console.WriteLine($"Cart RAM size = 32 KiB");
                break;
            case 0x04:
                //Console.WriteLine($"Cart RAM size = 128 KiB");
                break;
            case 0x05:
                //Console.WriteLine($"Cart RAM size = 64 KiB");
                break;
            default:
                //Console.WriteLine($"Cart RAM size = 0 KiB");
                break;
        }
    }
    
}