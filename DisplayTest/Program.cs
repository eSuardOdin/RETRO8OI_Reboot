using SDL3;

Console.WriteLine($"Tilemap = {args[0]}");
Console.WriteLine($"Tiles = {args[1]}");

// Get tilemap
byte[] Tilemap = File.ReadAllBytes(args[0]);
byte[] Tiles = File.ReadAllBytes(args[1]);
byte[] frameBuffer = new byte[0x400];
int sx = 0;
int sy = 0;

int width = 160;
int height = 144;
int tileWH = 8;
int scale = 2;
int fullWidth = 32 * tileWH * scale;
int fullHeight = 32 * tileWH * scale;
IntPtr Window;
IntPtr Renderer;
if (!SDL.Init(SDL.InitFlags.Video))
{
    SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
    return;
}


//if (!SDL.CreateWindowAndRenderer("SDL3 Create Window",tileWH*scale, tileWH*scale, 0, out Window, out Renderer))
if (!SDL.CreateWindowAndRenderer("SDL3 Create Window",fullWidth, fullHeight, 0, out Window, out Renderer))
{
    SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
    return;
}

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
        SDL.Delay(120);
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
            SDL.SetRenderDrawColor(renderer, 0, 0, 0, 255);
            return;
        case 2:
            SDL.SetRenderDrawColor(renderer, 60, 60, 60, 255);
            return;
        case 1:
            SDL.SetRenderDrawColor(renderer, 160, 160, 160, 255);
            return;
        case 0:
            SDL.SetRenderDrawColor(renderer, 255, 255, 255, 255);
            return;
    }
}


/*
 * Pour chaque ligne :
   
   Je vais chercher les bytes row et row+1 (row étant le numéro de ligne % 8)
   
   Je dois chopper une ligne (+ un éventuel SCX)
   La ligne correspond à l'index de ligne * 0x20 et le row de la sprite est la ligne % 8
   
   On a donc les deux octets de la row = GetLine(Tilemap[Tiles[ligne]])
   
*/
void GetLine(int lineIndex)
{
    byte least_significant_row;
    byte highest_significant_row;
    // Get the palette indices of the whole line
    byte[] line = new byte[256];

    int tilemapLine = lineIndex * 0x20;
    
    for (int i = tilemapLine; i < tilemapLine + 0x20; i++)
    {
        
    }
}

