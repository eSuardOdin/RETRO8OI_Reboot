using RETRO8OI.Exceptions;
using SDL3;
namespace RETRO8OI;

public class Display
{
    private IntPtr Texture;
    private IntPtr Window;
    private IntPtr Renderer;
    private MemoryBus Bus;
    private Ram Vram;
    public Display(MemoryBus bus, Ram ram)
    {
        Bus = bus;
        Vram = ram;
        // Init SDL
        if (!SDL.Init(SDL.InitFlags.Video))
        {
            throw new SDLException($"SDL could not initialize: {SDL.GetError()}");
        }
        
        // Create window and renderer
        if (!SDL.CreateWindowAndRenderer("RETRO-8-0I", 64, 128, 0, out Window, out Renderer))
        {
            throw new SDLException($"Error creating window and rendering: {SDL.GetError()}");
        }
        
        // Create texture template
        Texture = SDL.CreateTexture(Renderer, SDL.PixelFormat.RGBA64, SDL.TextureAccess.Streaming, 64, 128);

    }

}