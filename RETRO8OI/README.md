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

## Fix instructions :
E8: ADD SP, e8
D8: LD HL, SP + e8

### Test 11
CB 0E CB 2E CB 3E

