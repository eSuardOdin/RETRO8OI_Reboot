using SDL3;

Console.WriteLine($"Tilemap = {args[0]}");
Console.WriteLine($"Tiles = {args[1]}");

// Get tilemap
byte[] Tilemap = File.ReadAllBytes(args[0]);
byte[] Tiles = File.ReadAllBytes(args[1]);
uint[] BGPalette = new uint[4];
uint[] BGPaletteA = new uint[4]
{
    0xFFE0F8A0,  // Vert très clair (fond)
    0xFF88C070,  // Vert clair
    0xFF346856,  // Vert foncé
    0xFF081820 
};

uint[] BGPaletteB = new uint[4]
{
    0xFF081820, 
    0xFF346856,  // Vert foncé
    0xFF88C070,  // Vert clair
    0xFFE0F8A0  // Vert très clair (fond)
};
int scx = 0;
int scy = 0;
int line = 0;
int width = 160;
int height = 144;
byte[] FrameBuffer = new byte[width * height];
int tileWH = 8;
int scale = 2;
int fullWidth = 32 * tileWH * scale;
int fullHeight = 32 * tileWH * scale;
IntPtr Window;
IntPtr Renderer;
IntPtr Texture;

byte LCDC = 0x0;
bool IsBGTileMapArea = (LCDC & 0x8) == 0x8;


// Init SDL
if (!SDL.Init(SDL.InitFlags.Video))
{
    SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
    return;
}

// Creating renderer and window
//if (!SDL.CreateWindowAndRenderer("SDL3 Create Window",fullWidth, fullHeight, 0, out Window, out Renderer))
if (!SDL.CreateWindowAndRenderer("Display Test",width, height, 0, out Window, out Renderer))
{
    SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
    return;
}

// Creating texture
Texture = SDL.CreateTexture(Renderer, SDL.PixelFormat.ARGB8888, SDL.TextureAccess.Streaming, width, height);
while (true)
{

    for (line = 0; line < height; line++)
    {
        GetLineInBuffer(line);
    }

    Render();
    scx += 1;
    scy += 3;
    if (scx % 13 == 0)
    {
        BGPalette = BGPalette == BGPaletteA ? BGPaletteB : BGPaletteA;
    }
    SDL.Delay(30);
}

void GetLineInBuffer(int line)
{
    int posY = (scy + line) % 0xFF;
    int tileY = posY / 8;
    int row = posY % 8;
    int posX, tileX, pixX;
    byte lo = 0;
    byte hi = 0;
    byte hi_b, lo_b;
    
    for (int x = 0; x < 160; x++)
    {
        posX = (scx + x) % 0xFF;
        tileX = posX / 8;
        pixX = posX % 8;
        
        // If first pixel of tile row, get tile
        if (x == 0 || pixX % 8 == 0)
        {
            var tile = new byte[16]; 
            int tile_index = tileY * 0x20 + tileX;
            tile_index += IsBGTileMapArea ? 0x40 : 0;
            Console.WriteLine($"Index is {tile_index}");
            Array.Copy(Tiles, Tilemap[tile_index] * 0x10, tile, 0, 16);
            hi = tile[(row * 2)+1];
            lo = tile[(row * 2)];
            Console.WriteLine($"New tile :\n\tHI: {hi:X2}\n\tLO: {lo:X2}");
        }
        
        // Get palette index
        hi_b = (byte)((hi >> (7 - pixX)) & 1);
        lo_b = (byte)((lo >> (7 - pixX)) & 1);
        byte pix_index = (byte) (lo_b | (hi_b<<1));
        
        // Put in Framebuffer
        FrameBuffer[line * width + x] = pix_index;
    }
}

void Render()
{
    // Get color values in array
    uint[] pixels = new uint[width * height];
    for (int i = 0; i < FrameBuffer.Length; i++)
    {
        pixels[i] = BGPalette[FrameBuffer[i]];
    }

    unsafe
    {
        // Prevent garbage collector to move pixels[]
        fixed (uint* ptr = pixels)
        {
            SDL.UpdateTexture(Texture, nint.Zero, (nint)ptr, width * sizeof(uint));
        }
    }
    SDL.RenderClear(Renderer);
    SDL.RenderTexture(Renderer, Texture, nint.Zero, nint.Zero);
    SDL.RenderPresent(Renderer);
}




/*** WORKING UGLY ***
bool running = true;
int offX = 0, offY = 0;
int tileIndex = 0;
while (running)
{
    SDL.Event e = new SDL.Event();
    while (SDL.PollEvent(out e))
    {
        if ((SDL.EventType)e.Type == SDL.EventType.Quit)
            running = false;
    }
    
    // Get all tiles in tilemap
    int tileX, tileY = 0;
    for (int i = 0; i < 0x400; i++)
    {
        tileX = i % 32;
        tileY = i / 32;
        offX = (tileX) * scale * tileWH;
        offY = (tileY) * scale * tileWH;
        GetTile(i, offX, offY);
        SDL.RenderPresent(Renderer);
        SDL.Delay(30);
    }
    
    //Console.WriteLine($"0x{Tiles[0x20]:x2} 0x{Tiles[0x21]:x2}");
    //GetTile(2, offX, offY); //Test Tile 2
    
    SDL.RenderPresent(Renderer);
    while(true) {}
}






void GetTile(int index, int offsetX, int offsetY)
{
    //Console.WriteLine($"Tile [{index}] supposed to go to X:  {offsetX}, Y: {offsetY} ");
    var res = new byte[16]; 
    Array.Copy(Tiles, Tilemap[index] * 0x10, res, 0, 16);
    //Array.Copy(Tiles, index * 0x10, res, 0, 16);
    
    // Pour chaque ligne à render
    for (int y = 0; y < 8; y++)
    {
        byte lo = res[y*2];
        byte hi = res[(y*2)+1];
        byte hi_bit, lo_bit;
        //Console.WriteLine($"### ROW {y} ");
        //Console.WriteLine($"Low(first) is {lo:b8} High(second) is {hi:b8}");
        for (int x = 0; x < 8; x++)
        {
            hi_bit = (byte)((hi >> (7 - x)) & 1);
            lo_bit = (byte)((lo >> (7 - x)) & 1);
            byte pix_index = (byte) (lo_bit | (hi_bit<<1));
            //Console.WriteLine($"Col {x}: 0b{pix_index:b2}");
            SetpixelColor(pix_index, Renderer);
            SDL.FRect rect = new SDL.FRect
            {
                X = x * scale + offsetX,
                Y = y * scale + offsetY,
                W = scale,
                H = scale
            };
            SDL.RenderFillRect(Renderer, ref rect);
        }
        
    }
    
}


byte GetPalettePixel(int x)
{
    return 0;
}


void SetpixelColor(byte px, IntPtr renderer)
{
    switch (px)
    {
        case 3:
            SDL.SetRenderDrawColor(renderer, 20, 60, 20, 255);
            return;
        case 2:
            SDL.SetRenderDrawColor(renderer, 70, 110, 40, 255);
            return;
        case 1:
            SDL.SetRenderDrawColor(renderer, 140, 175, 20, 255);
            return;
        case 0:
            SDL.SetRenderDrawColor(renderer, 160, 190, 20, 255);
            return;
    }
}

/***/
