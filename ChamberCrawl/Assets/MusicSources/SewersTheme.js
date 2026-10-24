// Esgotos — limo, água rasa, escorregadio (exploração)

// Batida grudenta, kick atrasado
const drums = s("bd ~ bd ~, ~ sd ~ sd")
  .bank("RolandTR909")
  .gain(0.65)

// Pingos / respingos metálicos
const metals = s("hh hh ~ hh")
  .cut(1)
  .gain(0.2)
  .delay(0.35)
  .room(0.4)

// Baixo viscoso em G menor
const bass = note("g2 d2 c2 d2")
  .slow(2)
  .sound("sawtooth")
  .lpf(360)
  .lpq(6)
  .legato(1.1)
  .gain(0.62)

// Bolhas / arpejo irregular
const arpeggio = note("g3 bb3 d4 f4 d4 bb3 g3 d3")
  .slow(2)
  .sound("square")
  .lpf(700)
  .decay(0.25)
  .sustain(0.05)
  .delay(0.4)
  .room(0.55)
  .gain(0.34)

// Melodia úmida e inquieta
const lead = note("g4 bb4 c5 d5 c5 bb4 a4 g4")
  .slow(4)
  .sound("triangle")
  .vib(5)
  .delay(0.5)
  .room(0.65)
  .gain(0.4)

stack(drums, metals, bass, arpeggio, lead).cpm(72)
