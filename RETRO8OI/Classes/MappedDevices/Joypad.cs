using System.IO.MemoryMappedFiles;
using SDL3;

namespace RETRO8OI;


public class Joypad : IMemoryMappedDevice
{
    private byte Register = 0;
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



    public void Update(ReadOnlySpan<bool> keys)
    {
        if (IsPadFlag)
        {
            if (keys[(int)SDL.Scancode.Right])
            {
                if ((Register & 0x1) == 0x1) // Right
                {
                    //Console.WriteLine("Key pressed detected : DPAD RIGHT");
                    Register &= 0xFE;
                    IsInterruptRequested = true;
                }
            }
            else Register |= 0x1;
            
            if (keys[(int)SDL.Scancode.Left])           // Left
            {
                if ((Register & 0x2) == 0x2)
                {
                    //Console.WriteLine("Key pressed detected : DPAD LEFT");
                    Register &= 0xFD;
                    IsInterruptRequested = true;
                }
                
            }
            else Register |= 0x2;
            
            if (keys[(int)SDL.Scancode.Up])             // Up
            {
                if ((Register & 0x4) == 0x4)
                {
                    //Console.WriteLine("Key pressed detected : DPAD UP");
                    Register &= 0xFB;
                    IsInterruptRequested = true;
                }
            }
            else Register |= 0x4;
            
            if (keys[(int)SDL.Scancode.Down])           // Down
            {
                if ((Register & 0x8) == 0x8)
                {
                    //Console.WriteLine("Key pressed detected : DPAD DOWN");
                    Register &= 0xF7;
                    IsInterruptRequested = true;
                }
            }
            else Register |= 0x8;
        }

        else if (IsButtonsFlag)
        {
            if (keys[(int)SDL.Scancode.C])              // A
            {
                if ((Register & 0x1) == 0x1)
                {
                    //Console.WriteLine("Key pressed detected : A");
                    Register &= 0xFE;
                    IsInterruptRequested = true;
                }
            }
            else Register |= 0x1;
            
            if (keys[(int)SDL.Scancode.X])              // B
            {
                if ((Register & 0x2) == 0x2)
                {
                    //Console.WriteLine("Key pressed detected : B");
                    Register &= 0xFD;
                    IsInterruptRequested = true;
                }
            }
            else Register |= 0x2;
            
            if (keys[(int)SDL.Scancode.LCtrl])          // Select
            {
                if ((Register & 0x4) == 0x4)
                {
                    //Console.WriteLine("Key pressed detected : Select");
                    Register &= 0xFB;
                    IsInterruptRequested = true;
                }
            }
            else Register |= 0x4;
            
            if (keys[(int)SDL.Scancode.Space])          // Start
            {
                if ((Register & 0x8) == 0x8)
                {
                    //Console.WriteLine("Key pressed detected : Start");
                    Register &= 0xF7;
                    IsInterruptRequested = true;
                }
            }
            else Register |= 0x8;
            
        }

        // Write joypad interrupt if any. I filter them but that does not
        // simulate the bounce on true hardware. See if refactoring needed.
        if (IsInterruptRequested)
        {
            IsInterruptRequested = false;
            byte IF = Bus.Read(0xFF0F); 
            Bus.Write(0xFF0F, (byte)(IF | 0x10));
        }
    }
    
    
    
    public void Write(ushort address, byte data)
    {
        // Lower nibble is Read Only
        Register = (byte)((data & 0xF0) | (Register & 0x0F));
    }

    public byte Read(ushort address)
    { 
        //if(!IsPadFlag && !IsButtonsFlag) return 0xFF;
        // Bit 7 and 6 always returns as 1
        Console.WriteLine($"Reading {Register:X2} (0b{Register:B8}) from Joypad");
        return (byte)(Register | 0xC0);
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
        if ((SDL.EventType)e.Type == SDL.EventType.KeyDown)
        {
            if (IsPadFlag)
            {
                switch (e.Key.Scancode)
                {
                    case SDL.Scancode.Right :
                        if ((Register & 0x1) == 0x1)
                        {
                            Console.WriteLine("Key pressed detected : DPAD RIGHT");
                            Register &= 0xFE;
                            IsInterruptRequested = true;
                        }
                        break;
                    case SDL.Scancode.Left :
                        if ((Register & 0x2) == 0x2)
                        {
                            Console.WriteLine("Key pressed detected : DPAD LEFT");
                            Register &= 0xFD;
                            IsInterruptRequested = true;
                        }
                        break;
                    case SDL.Scancode.Up :
                        if ((Register & 0x4) == 0x4)
                        {
                            Console.WriteLine("Key pressed detected : DPAD UP");
                            Register &= 0xFB;
                            IsInterruptRequested = true;
                        }
                        break;
                    case SDL.Scancode.Down :
                        if ((Register & 0x8) == 0x8)
                        {
                            Console.WriteLine("Key pressed detected : DPAD DOWN");
                            Register &= 0xF7;
                            IsInterruptRequested = true;
                        }
                        break;
                }
            }
            if (IsButtonsFlag)
            {
                switch (e.Key.Scancode)
                {
                    case SDL.Scancode.C :                   // A
                        if ((Register & 0x1) == 0x1)
                        {
                            Console.WriteLine("Key pressed detected : A");
                            Register &= 0xFE;
                            IsInterruptRequested = true;
                        }
                        break;
                    case SDL.Scancode.X :                   // B
                        if ((Register & 0x2) == 0x2)
                        {
                            Console.WriteLine("Key pressed detected : B");
                            Register &= 0xFD;
                            IsInterruptRequested = true;
                        }
                        break;
                    case SDL.Scancode.LCtrl :               // Select
                        if ((Register & 0x4) == 0x4)
                        {
                            Console.WriteLine("Key pressed detected : SELECT");
                            Register &= 0xFB;
                            IsInterruptRequested = true;
                        }
                        break;
                    case SDL.Scancode.Space :               // Start
                        if ((Register & 0x8) == 0x8)
                        {
                            Console.WriteLine("Key pressed detected : START");
                            Register &= 0xF7;
                            IsInterruptRequested = true;
                        }
                        break;
                }
            }

        }
        // Check for KeyUp
        if ((SDL.EventType)e.Type == SDL.EventType.KeyUp)
        {
            if (IsPadFlag)
            {
                switch (e.Key.Scancode)
                {
                    case SDL.Scancode.Right :
                        if ((Register & 0x1) == 0)
                        {
                            Console.WriteLine("Key released detected : DPAD RIGHT");
                            Register |= 0x1;
                        }
                        break;
                    case SDL.Scancode.Left :
                        if ((Register & 0x2) == 0)
                        {
                            Console.WriteLine("Key released detected : DPAD LEFT");
                            Register |=  0x2;
                        }
                        break;
                    case SDL.Scancode.Up :
                        if ((Register & 0x4) == 0)
                        {
                            Console.WriteLine("Key released detected : DPAD UP");
                            Register |= 0x4;
                        }
                        break;
                    case SDL.Scancode.Down :
                        if ((Register & 0x8) == 0)
                        {
                            Console.WriteLine("Key released detected : DPAD DOWN");
                            Register |= 0x8;
                        }
                        break;
                }
            }
            if (IsButtonsFlag)
            {
                switch (e.Key.Scancode)
                {
                    case SDL.Scancode.C :                   // A
                        if ((Register & 0x1) == 0)
                        {
                            Console.WriteLine("Key released detected : A");
                            Register |= 0x1;
                        }
                        break;
                    case SDL.Scancode.X :                   // B
                        if ((Register & 0x2) == 0)
                        {
                            Console.WriteLine("Key released detected : B");
                            Register |=  0x2;
                        }
                        break;
                    case SDL.Scancode.LCtrl :               // Select
                        if ((Register & 0x4) == 0)
                        {
                            Console.WriteLine("Key released detected : SELECT");
                            Register |= 0x4;
                        }
                        break;
                    case SDL.Scancode.Space :               // Start
                        if ((Register & 0x8) == 0)
                        {
                            Console.WriteLine("Key released detected : START");
                            Register |= 0x8;
                        }
                        break;
                }
            }

        }
    }
    
}