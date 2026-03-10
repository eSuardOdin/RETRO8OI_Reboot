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

# TODO

My emulator does not render big sprites. Check with BGB their location on ROM to see why.

Pokemon Yellow does not work at all (Maybe because of CGB enhancement)



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
```
ram_256kb.gb PASSED
; Tests banking behaviour of a MBC1 cart with a 256 kbit RAM
; Expected behaviour:
;   * RAM is disabled initially
;   * When RAM is disabled, writes have no effect
;   * In mode 0 everything accesses bank 0
;   * In mode 1 access is done based on $4000 bank number
;   * If we switch back from mode 1, we once again access bank 0
```
```
ram_64kb.gb PASSED
; Tests banking behaviour of a MBC1 cart with a 64 kbit RAM
; Expected behaviour:
;   * RAM is disabled initially
;   * When RAM is disabled, writes have no effect
;   * Since we have only one 8 kB bank, we always access it regardless of what
;      we do with $4000 and $6000
```
```
rom_16Mb.gb PASSED
; Tests banking behaviour of a MBC1 cart with a 16 Mbit ROM
```
```
rom_512Kb.gb
; Tests banking behaviour of a MBC1 cart with a 512 Kbit ROM
```
```
rom_1Mb.gb PASSED
; Tests banking behaviour of a MBC1 cart with a 1 Mbit ROM
```
```
rom_2Mb.gb PASSED
; Tests banking behaviour of a MBC1 cart with a 2 Mbit ROM
```
```
rom_4Mb.gb PASSED
; Tests banking behaviour of a MBC1 cart with a 4 Mbit ROM
```
```
rom_8Mb.gb PASSED
; Tests banking behaviour of a MBC1 cart with a 8 Mbit ROM
```

