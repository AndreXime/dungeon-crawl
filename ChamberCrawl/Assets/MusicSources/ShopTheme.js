// Tema da loja — tudo trava na mesma grade de 4 tempos

// Kick no 1 e 3, snare no 2 e 4 (1 ciclo = 1 compasso)
const drums = s("bd ~ sd ~")
  .bank("RolandTR909")
  .gain(0.4)

const metals = s("~ hh ~ hh")
  .cut(1)
  .gain(0.18)

// 4 notas / 2 ciclos = meio tempo por nota, no grid
const bass = note("a2 e2 d2 e2")
  .slow(2)
  .sound("triangle")
  .lpf(380)
  .legato(1)
  .gain(0.48)

// 8 notas / 2 ciclos = mesma subdivisão do baixo
const arpeggio = note("a3 c4 e4 a4 e4 c4 a3 e3")
  .slow(2)
  .sound("triangle")
  .lpf(1400)
  .decay(0.18)
  .sustain(0)
  .room(0.45)
  .gain(0.32)

// 8 notas / 4 ciclos = 1 nota por meio ciclo, alinhada ao baixo
const lead = note("a4 c5 e5 d5 c5 a4 e4 a4")
  .slow(4)
  .sound("sine")
  .lpf(1600)
  .delay(0.2)
  .room(0.5)
  .gain(0.35)

stack(drums, metals, bass, arpeggio, lead).cpm(70)
