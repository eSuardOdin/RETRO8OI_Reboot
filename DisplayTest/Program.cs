using SDL3;

Console.WriteLine($"Tilemap = {args[0]}");
Console.WriteLine($"Tiles = {args[1]}");

// Get tilemap
byte[] Tilemap = File.ReadAllBytes(args[0]);
byte[] Tiles = File.ReadAllBytes(args[1]);
int sx = 0;
int sy = 0;

int width = 160;
int height = 144;
int tileWH = 8;
int scale = 4;
int fullWidth = 32 * tileWH * scale;
int fullHeight = 32 * tileWH * scale;
IntPtr Window;
IntPtr Renderer;
if (!SDL.Init(SDL.InitFlags.Video))
{
    SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
    return;
}


if (!SDL.CreateWindowAndRenderer("SDL3 Create Window",tileWH*scale, tileWH*scale, 0, out Window, out Renderer))
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
    /*
    for (int i = 0; i < 0x100; i++)
    {
        GetTile(i, 0, 0);
        Console.WriteLine($"Tilemap[{i:X2}] = {Tilemap[i]:x2}");
        SDL.RenderPresent(Renderer);
        SDL.Delay(100);
    }
    */
    GetTile(2, 0, 0);
    SDL.RenderPresent(Renderer);
    SDL.Delay(100);
    //running = false;
    /*for (offY = 0; offY < 32; offY++)
    {
        for (offX = 0; offX < 32; offX++)
        {
            GetTile(Tilemap[(offX+offY)*16], offX * tileWH * width, offY * tileWH * width);
            tileIndex++;
        }

    }
    */


}



void GetTile(int index, int offsetX, int offsetY)
{
    var res = new byte[16]; 
    //Array.Copy(Tiles, Tilemap[index], res, 0, 16);
    Array.Copy(Tiles, index * 0x10, res, 0, 16);
    // Pour chaque ligne à render
    for (int y = 0; y < 16; y+=2)
    {
        byte hi = res[y];
        byte lo = res[y+1];
        for (int x = 0; x < 8; x++)
        {
            byte pix_index = (byte)( ((lo & (1 << x)) >> x) | ((hi & (1 << x)) >> (x - 1)) );
            SetpixelColor(pix_index, Renderer);
            SDL.FRect rect = new SDL.FRect
            {
                X = x * scale,
                Y = (y/2) * scale,
                W = scale,
                H = scale
            };
            SDL.RenderFillRect(Renderer, ref rect);
        }
        
    }
    
}


void SetpixelColor(byte px, IntPtr renderer)
{
    switch (px)
    {
        case 0:
            SDL.SetRenderDrawColor(renderer, 0, 0, 0, 255);
            return;
        case 1:
            SDL.SetRenderDrawColor(renderer, 60, 60, 60, 255);
            return;
        case 2:
            SDL.SetRenderDrawColor(renderer, 160, 160, 160, 255);
            return;
        case 3:
            SDL.SetRenderDrawColor(renderer, 255, 255, 255, 255);
            return;
    }
    
}