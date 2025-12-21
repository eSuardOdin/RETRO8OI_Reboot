# Mapped memory

## Cartridge
`0000-7FFF` for ROM R/W
`A000-BFFF` for External RAM R/W

## Interrupt registers
`FF0F` for Interrupt Flag register
`FFFF` for Interrupt Enable register

## RAM
`8000-9FFF` for VRAM R/W
`C000-DFFF` for WRAM R/W
`E000-FDFF` for Echo RAM -> Usage prohibited by Nintendo (May remove)




# Todo 
The Ppu will be implementing OAM and Vram so it's current mode will take care of what R/W to VRam/OAM would return -> To refactor.
