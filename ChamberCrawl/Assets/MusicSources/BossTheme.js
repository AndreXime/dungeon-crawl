// Tema do boss — pesado, tenso, grade 4/4 travada

// Kick e snare marcados, pressão constante
const drums = s("bd sd bd sd")
  .bank("RolandTR909")
  .gain(0.75)

// Hats fechados no offbeat
const metals = s("~ hh ~ hh")
  .cut(1)
  .gain(0.28)

// Baixo soturno em menor, 4 notas / 2 ciclos
const bass = note("d2 a1 d2 c2")
  .slow(2)
  .sound("sawtooth")
  .lpf(320)
  .lpq(3)
  .legato(1)
  .gain(0.7)

// Ostinato ameaçador, 8 notas / 2 ciclos
const ostinato = note("d3 f3 a3 d4 a3 f3 d3 a2")
  .slow(2)
  .sound("square")
  .lpf(900)
  .decay(0.12)
  .sustain(0)
  .room(0.4)
  .gain(0.4)

// Melodia grave e lenta, 4 notas / 4 ciclos
const lead = note("d3 f3 a3 e3")
  .slow(4)
  .sound("sawtooth")
  .lpf(800)
  .vib(1.5)
  .delay(0.3)
  .room(0.55)
  .gain(0.4)

stack(drums, metals, bass, ostinato, lead).cpm(85)
