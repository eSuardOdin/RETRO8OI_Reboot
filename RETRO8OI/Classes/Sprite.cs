namespace RETRO8OI;

/// <summary>
/// Represents an object in OAM
/// </summary>
public class Sprite
{
    public byte Y { get; private set; }
    public byte X { get; private set; }
    public byte TileIndex { get; private set; }
    private byte Flags;
    
    public bool IsPal1 { get =>  (Flags & 0x10) == 0x10; }
    public bool IsFlippedX { get =>  (Flags & 0x20) == 0x20; }
    public bool IsFlippedY { get =>  (Flags & 0x40) == 0x40; }
    
}