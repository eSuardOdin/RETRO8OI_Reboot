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




# Mooneye test suite

## Acceptance

### Bits
```
mem_oam.gb PASSED
; This test checks that the OAM area has no unused bits
; On DMG the sprite flags have unused bits, but they are still
; writable and readable normally
```

```
reg_f.gb PASSED
; This test checks that bottom 4 bits of the F register always return 0
```

```
unused_hwio-GS.gb FAILED
; This test checks all unused bits in working $FFxx IO,
; and all unused $FFxx IO. Unused bits and unused IO all return 1s.
; A test looks like this:
;
;            mask      write     expected
; test REG   MASK      WRITE     EXPECTED
;
;   1. write WRITE to REG
;   2. read VALUE from REG
;   3. compare VALUE & MASK with EXPECTED & MASK
```

## MBC1

; Tests that BANK1 is mapped to correct addresses and has the right initial
; value.
```
bits_bank1.gb PASSED
; Tests that BANK1 is mapped to correct addresses and has the right initial
; value.
```
```
bits_bank2.gb PASSED
; Tests that BANK2 is mapped to correct addresses and has the right initial
; value.
```
```
bits_mode.gb FAILED
; Tests that MODE is mapped to correct addresses and has the right initial
; value.
```
```
bits_ramg.gb PASSED
; Tests that RAMG is mapped to correct addresses, and RAM disable/enable
; happens with the right data values.
```


# Misc instruction -> WTF

# 10
CB 8C:   RES 1, H
CB CC:   SET 1, H