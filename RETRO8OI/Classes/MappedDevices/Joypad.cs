using System.IO.MemoryMappedFiles;
using SDL3;

namespace RETRO8OI;


public class Joypad : IMemoryMappedDevice
{
    private byte Register = 0;
    private byte Pressed = 0xF;
    private byte Pad = 0xF;
    private byte Buttons = 0xF;
    private bool IsPadFlag => (Register & 0x10) == 0;
    private bool IsButtonsFlag => (Register & 0x20) == 0;
    private bool IsInterruptRequested = false;
    /// <summary>
    /// The bus is just there to write interrupt request flags
    /// </summary>
    private MemoryBus Bus;

    private IntPtr Keys;
    public Joypad(MemoryBus bus)
    {
        Bus = bus;
    }

    
    
    
    
    public void Write(ushort address, byte data)
    {
        // Lower nibble is Read Only
        Register = (byte)((data & 0xF0) | (Register & 0x0F));
    }

    public byte Read(ushort address)
    { 
        if (IsPadFlag)
        {
            Register = (byte)(Register & 0xF0 | Pad & 0xF);
            //Console.WriteLine($"DPAD: {Register:B8}");
        }
        if (IsButtonsFlag)
        {
            Register = (byte)(Register & 0xF0 | Buttons & 0xF);
            //Console.WriteLine($"BUTTONS: {Register:B8}");
        }

        if (!IsPadFlag && !IsButtonsFlag)
        {
            return 0xFF;
        }
        return Register;
    }
    
    public bool Accept(ushort address)
    {
        return address == 0xFF00;
    }

    /// <summary>
    /// <para>
    /// On KEYDOWN :
    /// If any selector on JOYP register, will write 0 to DOWN key
    /// and ask for an interrupt in Joypad.Update()
    /// </para>
    /// <para>
    /// On KEYUP :
    /// If any selector on JOYP register, will write 1 to UP key
    /// </para>
    /// </summary>
    /// <param name="e">The sdl event</param>
    public void HandleSDLEvent(SDL.Event e)
    {
        // Check for KeyDown event
        if ((SDL.EventType)e.Type == SDL.EventType.KeyDown
            || (SDL.EventType)e.Type == SDL.EventType.KeyUp)
        {
            switch (e.Key.Scancode)
            {
                case SDL.Scancode.Right :
                    Pressed = 0x11;
                    break;
                case SDL.Scancode.Left :
                    Pressed = 0x12;
                    break;
                case SDL.Scancode.Up :
                    Pressed = 0x14;
                    break;
                case SDL.Scancode.Down :
                    Pressed = 0x18;
                    break;
                
                
                // --- BUTTONS ---
                case SDL.Scancode.C :                   // A
                    Pressed = 0x21;
                    break;
                case SDL.Scancode.X :                   // B
                    Pressed = 0x22;
                    break;
                case SDL.Scancode.LCtrl :               // Select
                    Pressed = 0x24;
                    break;
                case SDL.Scancode.Space :               // Start
                    Pressed = 0x28;
                    break;
            }
            
            // If DPAD Mask
            if ((Pressed & 0x10) == 0x10)
            {
                if ((SDL.EventType)e.Type == SDL.EventType.KeyDown)
                {
                    // If Pad = 1101 and Pressed = 0001, Pad = 1100
                    Pad &= (byte)~(Pressed & 0xF);
                    // Interrupt request
                    byte IF = Bus.Read(0xFF0F); 
                    Bus.Write(0xFF0F, (byte)(IF | 0x10));
                }
                else
                {
                    // If Pad = 1100 and Pressed = 0001, Pad = 1101
                    Pad |= (byte)(Pressed & 0xF);
                }
            }
            // If Buttons Mask
            else if ((Pressed & 0x20) == 0x20)
            {
                if ((SDL.EventType)e.Type == SDL.EventType.KeyDown)
                {
                    Buttons &= (byte)~(Pressed & 0xF);
                    // Interrupt request
                    byte IF = Bus.Read(0xFF0F); 
                    Bus.Write(0xFF0F, (byte)(IF | 0x10));
                }
                else
                {
                    Buttons |= (byte)(Pressed & 0xF);
                }
            }
        }
    }
    
}