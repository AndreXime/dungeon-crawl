// Catacumbas — ossos, frio, corredores estreitos (exploração)

// Passos ocos e secos, como botas em pedra
const drums = s("bd ~ ~ ~, ~ ~ sd ~")
  .bank("RolandTR909")
  .gain(0.55)

// Cliques de osso / metal espaçados
const metals = s("hh ~ ~ hh")
  .cut(1)
  .gain(0.22)
  .hpf(6000)

// Baixo gelado em E menor
const bass = note("e2 b1 g1 f#1")
  .slow(2)
  .sound("sawtooth")
  .lpf(280)
  .lpq(4)
  .legato(1)
  .gain(0.55)

// Arpejo oco, quase litania
const arpeggio = note("e3 g3 b3 d4 e4 d4 b3 g3")
  .slow(2)
  .sound("triangle")
  .lpf(900)
  .decay(0.2)
  .sustain(0)
  .room(0.7)
  .gain(0.32)

// Melodia fantasma, lenta
const lead = note("e4 g4 a4 b4 a4 g4 f#4 e4")
  .slow(4)
  .sound("sine")
  .vib(2)
  .delay(0.45)
  .room(0.75)
  .gain(0.42)

stack(drums, metals, bass, arpeggio, lead).cpm(58)
