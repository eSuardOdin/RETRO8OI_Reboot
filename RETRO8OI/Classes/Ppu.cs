using SDL3;

namespace RETRO8OI;

public class Ppu : IMemoryMappedDevice
{
    public MemoryBus Bus { get; private set; }

    private byte _mode;

    public byte Mode
    {
        get => _mode;
        set
        {
            if (_mode != value)
            {
                //Console.WriteLine($"Changing from mode {_mode} to mode {value}");
                _mode = value;
            
                // STAT Update
                STAT &= 0xFC;
                STAT |= (byte)(value & 0x3);
            
                OnModeSwitchEvent(value);
            }
        }
    }
    /// <summary>
    /// Responds to 0xFE00 - 0xFE9F<br/>
    /// <para>Accessible to CPU only in modes 0 (HBlank) and 1 (VBlank)</para>
    /// 
    /// </summary>
    private byte[] OAM;
    public byte[] Vram { get; private set; }
    private ushort OamDmaAddr = 0;
    private byte OamDmaCyclesDone = 0;
    private byte LCDC = 0x91;

    private bool IsLcdPpuEnabled
    {
        get => (LCDC & 0x80) == 0x80;
    }
    private bool IsWindowEnabled    // Bit 5 of LCDC
    {
        get
        {
            return (LCDC & 0x20) == 0x20; // Beware, need to take bit 0 into account too
        } 
    }
    private bool IsBackgroundAndWindowEnabled
    {
        get
        {
            return (LCDC & 0x1) == 0x1;
        }
    }

    private ushort WindowTilemapAddress
    {
        get => (LCDC & 0x40) == 0x40 ? (ushort)0x9C00 : (ushort)0x9800;
    }

    private ushort TileMapBaseAddress
    {
        get => (LCDC & 0x10) == 0x10 ? (ushort)0x8000 : (ushort)0x9000;
    }

    private ushort BGTileMapArea
    {
        get => (LCDC & 0x8) == 0x8 ? (ushort)0x9C00 : (ushort)0x9800;
    }

    private bool IsObjEightBySixteen
    {
        get => (LCDC & 0x4) == 0x4;
    }

    private bool IsObjEnabled
    {
        get => (LCDC & 0x2) == 0x2;
    }
    
    
    private byte LY = 0;
    private byte LYC = 0;
    private byte STAT = 0x85;
    private byte SCY = 0;
    private byte SCX = 0;
    private byte WY = 0;
    private byte WX = 0;
    private byte BGP = 0;
    private byte OBP0 = 0;
    private byte OBP1 = 0;
    private int VerticalCyclesCount = 0;
    private bool StatIntLine = false;
    uint[] BGPalette = new uint[4]
    {
        0xFFE0F8A0,  // Vert très clair (fond)
        0xFF88C070,  // Vert clair
        0xFF346856,  // Vert foncé
        0xFF081820 
    };
    
    // SDL Stuff
    private int Width = 160;
    private int Height = 144;
    private byte[] FrameBuffer;
    
    private IntPtr Renderer;
    private IntPtr Texture;
    private IntPtr Window;
    
    
    
    private uint GetColor(byte index)
    {
        switch ( index )
        {
            case 0b00:
                return 0xFFE0F8A0;
            case 0b01:
                return 0xFF88C070;
            case 0b10:
                return 0xFF346856;
            case 0b11:
                return 0xFF081820;
        }

        return 0;
    }

    // To be handled by CPU
    public event EventHandler<bool> OamDmaEvent;
    public event EventHandler<int> ModeSwitchEvent;
    
    public Ppu(MemoryBus bus)
    {
        Bus = bus;
        Vram = new byte[0x2000];
        OAM = new byte[0xA0];
        Mode = 0;
        FrameBuffer = new byte [Width * Height];
        
        // SDL INIT
        // Init SDL
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            return;
        }

// Creating renderer and window
        if (!SDL.CreateWindowAndRenderer("RETRO 80I",Width, Height, 0, out Window, out Renderer))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
            return;
        }

// Creating texture
        Texture = SDL.CreateTexture(Renderer, SDL.PixelFormat.ARGB8888, SDL.TextureAccess.Streaming, Width, Height);
    }

    protected virtual void OnOamDmaEvent(bool isStart)
    {
        OamDmaEvent?.Invoke(this, isStart);
    }

    protected virtual void OnModeSwitchEvent(int mode)
    {
        ModeSwitchEvent?.Invoke(this, mode);
    }
    



    public void Update(int cycles)
    {
        VerticalCyclesCount += cycles;
        // Check if LCD is enabled
        if (IsLcdPpuEnabled)
        {
            //Console.WriteLine($"LCD Enabled: \n\tENTERING: Mode {Mode}, dots to consume {cycles}");
            switch (Mode) // Switch mode
            {
                case 2: // OAM Scan
                    if (VerticalCyclesCount >= 80)
                    {
                        // Going to draw pixel
                        Mode = 0x3;
                        VerticalCyclesCount -= 80;
                    }
                    break;
                case 3: // Pixel draw
                    if (VerticalCyclesCount >= 172)
                    {
                        // Bufferize line to render
                        BufferizeScanline(LY);
                        // Going to HBlank
                        Mode = 0x0;
                        VerticalCyclesCount -= 172;
                    }
                    break;
                case 0: // HBlank
                    if (VerticalCyclesCount >= 204)
                    {
                        // Increment LY and check if it's equal LYC
                        LY++;
                        CheckLyLyc();
                        VerticalCyclesCount -= 204;
                        if (LY >= 144)
                        {
                            Mode = 0x1; // Switch to VBlank
                            
                            // Write VBlank interrupt request flag
                            byte IF = Bus.Read(0xFF0F); 
                            Bus.Write(0xFF0F, (byte)(IF | 0x1));
                        }
                        else
                        {
                            Mode = 0x2; // Switch to OAM Scan for next visible line
                        }
                    }
                    break;
                case 1: // VBlank
                    if (VerticalCyclesCount >= 456)
                    {
                        LY++;
                        CheckLyLyc();
                        VerticalCyclesCount -= 456;
                        if (LY >= 153)
                        {
                            Render();
                            Mode = 0x2; // Switch to OAM Scan
                            LY = 0;
                        }
                    }
                    break;
            }
            
        }
        else { //LCD Disabled
            VerticalCyclesCount = 0;
            LY = 0;
            STAT = (byte)(STAT & ~0x3);
            FrameBuffer = new byte [Width * Height];
        }
    }


    public void OamDmaUpdate(int cycles)
    {
        for (int i = 0; i < cycles; i++)
        {
            // Copy a byte by CPU cycle and increment values
            OAM[OamDmaCyclesDone] = Bus.Read(OamDmaAddr);
            OamDmaCyclesDone++;
            OamDmaAddr++;
            // Check if OAM DMA done
            if (OamDmaCyclesDone == OAM.Length - 1)
            {
                // Event to signal OAM DMA ended
                OnOamDmaEvent(false);
                return;
            }
        }
    }
    
    

    private void CheckLyLyc()
    {
        if (LY == LYC)
        {
            STAT |= 0x4;
            CheckStatInterrupt();   // Check for bit 6 LYC int select
            return;
        }
        // If it's not equal anymore
        if ((STAT & 0x4) == 0x4)
        {
            STAT &= 0b11111011;
        }
    }

    private void CheckStatInterrupt()
    {
        // To check later if stat line was not already high
        bool oldStatLine = StatIntLine;
        StatIntLine = (
            Mode == 0 && ((STAT & 0x8) == 0x8) ||
            Mode == 1 && ((STAT & 0x10) == 0x10) ||
            Mode == 2 && ((STAT & 0x20) == 0x20) ||
            ((STAT & 0x4) == 0x4) && ((STAT & 0x40) == 0x40)
        );
        // If rising edge on stat interrupt
        if (StatIntLine && !oldStatLine)
        {
            byte IF = Bus.Read(0xFF0F);
            Bus.Write(0xFF0F, (byte) (IF | 0x2));
        }
    }
    
    
    
    /// <summary>
    /// Put background and window palette color indices of a line into the framebuffer
    /// </summary>
    /// <param name="line">The scanline occuring</param>
    private void BufferizeBackgroundAndWindow(int line)
    {
        // Check if the line to display is window or bg
        bool isWindow = LY >= WY;
        int posY = isWindow ? LY - WY : (SCY + line) % 0xFF;
        int tileY = posY / 8;
        int row = posY % 8;
        int posX, tileX, pixX;
        byte lo = 0;
        byte hi = 0;
        byte hi_b, lo_b;
        
        
        for (int x = 0; x < Width; x++)
        {
            posX = isWindow ? (WX - 7) + x : (SCX + x) % 0xFF;
            tileX = posX / 8;
            pixX = posX % 8;
        
            // If first pixel of tile row, get tile
            if (x == 0 || pixX % 8 == 0)
            {
                var tile = new byte[16];
                // If tile to display is Background tile
                if (!isWindow)
                {
                    // Get the tilemap index with suppressing VRAM offset (0x8000)
                    byte tileIndex = Vram[(BGTileMapArea - 0x8000)+ (tileY * 0x20 + tileX)];
                    // If $8800 mode (index is signed)
                    if (TileMapBaseAddress == 0x9000)
                    {
                        sbyte index = (sbyte)tileIndex;
                        short trueIndex = (short)(index * 0x10);
                        // Because base address is 0x9000, we add an offset of 0x1000
                        Array.Copy(Vram, (0x1000 + trueIndex), tile, 0, 16);
                    }
                    // Else if $8000 mode
                    else
                    {
                        Array.Copy(Vram, tileIndex * 0x10, tile, 0, 16);
                    }
                    
                }
                // If tile is Window tile
                else
                {
                    // Get the tilemap index with suppressing VRAM offset (0x8000)
                    byte tileIndex = Vram[(WindowTilemapAddress - 0x8000)+ (tileY * 0x20 + tileX)];
                    // If $8800 mode (index is signed)
                    if (TileMapBaseAddress == 0x9000)
                    {
                        sbyte index = (sbyte)tileIndex;
                        short trueIndex = (short)(index * 0x10);
                        // Because base
                        Array.Copy(Vram, (0x1000 + trueIndex), tile, 0, 16);
                    }
                    // Else if $8000 mode
                    else
                    {
                        Array.Copy(Vram, tileIndex * 0x10, tile, 0, 16);
                    }
                }
                
                
                hi = tile[(row * 2)+1];
                lo = tile[(row * 2)];
            }
        
            // Get palette index
            hi_b = (byte)((hi >> (7 - pixX)) & 1);
            lo_b = (byte)((lo >> (7 - pixX)) & 1);
            byte paletteIndex = (byte) (lo_b | (hi_b<<1));
        
            // Get the color depending on the palette
            byte colorIndex = (byte)((BGP & (0b11 << (paletteIndex* 2))) >> (paletteIndex*2));
            
            // Put in Framebuffer
            FrameBuffer[line * Width + x] = colorIndex;
        }
    }

    
    
    private void BufferizeSprites(byte line)
    {
        // DMG can only display 10 sprites on a line
        int objInLine = 0;
        int spriteSize = IsObjEightBySixteen ? 16 : 8;
        // Check values in OAM, each object is 4 bytes long
        for (int i = 0; i < (OAM.Length / 4); i += 4)
        {
            // Get the next OAM object
            int objY = OAM[i] - 16;
            int objX = OAM[i + 1] - 8;
            int tileIndex = OAM[i + 2];
            int flags  = OAM[i + 3];
            
            // Get the objects that appears on the line
            if (line < (objY + spriteSize) && line >= objY)
            {
                // Set flag values
                objInLine++;
                bool isOverBG = (flags & 0x80) == 0x80;
                bool isFlippedX = (flags & 0x40) == 0x40;
                bool isFlippedY = (flags & 0x20) == 0x20;
                byte objPalette = (flags & 0x10) == 0x10 ? OBP1 : OBP0;
                // Framebuffer index
                int startIndex = (line * 160 + objX);
                // Get row of sprite to draw
                int spriteRow = line - objY;
                int col = 0;
                // Draw
                for (int j = 0; j < 8; j++)
                {
                    byte lo = Vram[((tileIndex + spriteRow) * 0x10) + col];
                    byte hi = Vram[((tileIndex + spriteRow) * 0x10) + col + 1];
                    // Get palette index
                    byte hi_b = (byte)((hi >> (7 - col)) & 1);
                    byte lo_b = (byte)((lo >> (7 - col)) & 1);
                    byte paletteIndex = (byte) (lo_b | (hi_b<<1));
        
                    // Get the color depending on the palette
                    byte colorIndex = (byte)((objPalette & (0b11 << (paletteIndex* 2))) >> (paletteIndex*2));
                    // Put in Framebuffer
                    FrameBuffer[startIndex + j] = colorIndex;
                    col++;
                }
            }

            if (objInLine == 10)
            {
                return;
            }
        }
        
    }

    private void BufferizeScanline(byte line)
    {
        BufferizeBackgroundAndWindow(line);
        BufferizeSprites(line);
    }
    
    
    void Render()
    {
        // Get color values in array
        uint[] pixels = new uint[Width * Height];
        for (int i = 0; i < FrameBuffer.Length; i++)
        {
            pixels[i] = BGPalette[FrameBuffer[i]];
        }

        unsafe
        {
            // Prevent garbage collector to move pixels[]
            fixed (uint* ptr = pixels)
            {
                SDL.UpdateTexture(Texture, nint.Zero, (nint)ptr, Width * sizeof(uint));
            }
        }
        SDL.RenderClear(Renderer);
        SDL.RenderTexture(Renderer, Texture, nint.Zero, nint.Zero);
        SDL.RenderPresent(Renderer);
        //throw new Exception();
    }
    
    
    // MEMORY MAPPED STUFF
    public void Write(ushort address, byte data)
    {
        // Write VRAM only if mode != 3
        if (address >= 0x8000 && address <= 0x9FFF && Mode != 3)
        {
            Vram[address - 0x8000] = data;
            return;
        }
        // Write to OAM only if mode 0 or 1 (VBlank, HBlank)
        if (address >= 0xFE00 && address <= 0xFE9F && Mode < 2)
        {
            OAM[address - 0xFE00] =  data;
            return;
        }
        // Write LCD stuff
        if (address >= 0xFF40 && address <= 0xFF4B)
        {
            switch (address)
            {
                case 0xFF46:    // OAM DMA
                    // Prepare OAM DMA
                    OnOamDmaEvent(true);
                    OamDmaAddr = (ushort)(data << 8);
                    OamDmaCyclesDone = 0;
                    return;
                case 0xFF47:    // Backgroung palette
                    BGP = data;
                    return;
                case 0xFF48:    // OBP 0 palette
                    OBP0 = data;
                    return;
                case 0xFF49:    // OBP 1 palette
                    OBP1 = data;
                    return;
                case 0xFF40:    // LCDC
                    LCDC = data;
                    return;
                case 0xFF44:    // LY
                    return;
                case 0xFF45:    // LYC
                    LYC = data;
                    CheckLyLyc();
                    return;
                case 0xFF41:    // STAT
                    STAT = data;
                    return;
                case 0xFF42:    // SCY
                    SCY = data;
                    return;
                case 0xFF43:    // SCX
                    SCX = data;
                    return;
                case 0xFF4A:    // WY
                    WY = data;
                    return;
                case 0xFF4B:    // WX
                    WX = data;
                    return;
            }
        }
    }

    public byte Read(ushort address)
    {
        // Read VRAM
        if (address >= 0x8000 && address <= 0x9FFF && Mode != 3)
        {
            //Console.WriteLine($"Reading VRAM [{address:X4}]");
            return Vram[address - 0x8000];
        }
        // Read OAM
        if (address >= 0xFE00 && address <= 0xFE9F && Mode < 2)
        {
            //Console.WriteLine($"Reading OAM [{address:X4}]");
            return OAM[address - 0xFE00];
        }
        // Read LCD stuff
        if (address >= 0xFF40 && address <= 0xFF4B)
        {
            switch (address)
            {
                case 0xFF40:
                    //Console.WriteLine($"Reading LCDC [{address:X4}]");
                    return LCDC;
                case 0xFF44:
                    //Console.WriteLine($"Reading LY [{address:X4}]");
                    return LY;
                case 0xFF45:
                    //Console.WriteLine($"Reading LYC [{address:X4}]");
                    return LYC;
                case 0xFF41:
                    //Console.WriteLine($"Reading LCD STAT [{address:X4}]");
                    return STAT;
                case 0xFF42:
                    //Console.WriteLine($"Reading SCY [{address:X4}]");
                    return SCY;
                case 0xFF43:
                    //Console.WriteLine($"Reading SCX [{address:X4}]");
                    return SCX;
                case 0xFF4A:
                    //Console.WriteLine($"Reading WY [{address:X4}]");
                    return WY;
                case 0xFF4B:
                    //Console.WriteLine($"Reading WX [{address:X4}]");
                    return WX;
            }
        }
        return 0xFF;
    }

    public bool Accept(ushort address)
    {
        return (address >= 0x8000 && address <= 0x9FFF) || 
               (address >= 0xFE00 && address <= 0xFE9F) ||
               (address >= 0xFF40 && address <= 0xFF4B);
    }



}