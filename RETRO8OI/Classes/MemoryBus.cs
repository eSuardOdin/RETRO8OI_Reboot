namespace RETRO8OI;

public class MemoryBus
{
    public readonly List<IMemoryMappedDevice> MemoryMappedDevices;

    public bool IsDmaTransfer { get; private set; }

    public MemoryBus()
    {
        IsDmaTransfer = false;
        MemoryMappedDevices = new List<IMemoryMappedDevice>();
    }
    
    public byte Read(ushort address)
    {
        foreach (IMemoryMappedDevice dev in MemoryMappedDevices)
        {
            if (dev.Accept(address))
            {
                return dev.Read(address);
            }
        }
        throw new Exception($"Address [0x{address:X4}] not mapped yet.");
        return 0xFF;
    }

    public void Write(ushort address, byte data)
    {
        foreach (IMemoryMappedDevice dev in MemoryMappedDevices)
        {
            if (dev.Accept(address))
            {
                dev.Write(address, data);
                return;
            }
        }
        //throw new Exception($"Address [0x{address:X4}] not mapped yet.");
    }

    public void Map(IMemoryMappedDevice dev)
    {
        MemoryMappedDevices.Add(dev);
    }
    
    
    public void DmaTransferSR(bool enable)
    {
        IsDmaTransfer = enable;
    }
}