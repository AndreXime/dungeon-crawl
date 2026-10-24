// Arsenal — ferro, tochas, disciplina militar (exploração)

// Marcha: kick firme, snare no 2 e 4
const drums = s("bd sd bd sd")
  .bank("RolandTR909")
  .gain(0.7)

// Hats de disciplina
const metals = s("hh hh hh hh")
  .cut(1)
  .gain(0.18)

// Baixo de bigorna em A menor / poder
const bass = note("a1 e2 a1 g1")
  .slow(2)
  .sound("sawtooth")
  .lpf(450)
  .lpq(2)
  .legato(1)
  .gain(0.68)

// Ostinato metálico
const arpeggio = note("a2 c3 e3 a3 e3 c3 a2 e2")
  .slow(2)
  .sound("square")
  .lpf(1100)
  .decay(0.1)
  .sustain(0)
  .room(0.25)
  .gain(0.38)

// Fanfarra curta e seca
const lead = note("a3 c4 e4 d4 c4 a3 g3 a3")
  .slow(4)
  .sound("sawtooth")
  .lpf(1400)
  .vib(1)
  .delay(0.15)
  .room(0.35)
  .gain(0.45)

stack(drums, metals, bass, arpeggio, lead).cpm(88)
