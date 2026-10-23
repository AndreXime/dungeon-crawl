// Trilha sonora medieval para Dungeon Crawler (Ritmo de Exploração)

// Batida pausada e pesada de caminhada
const drums = s("bd ~ ~ bd, ~ ~ sn ~")
  .bank("RolandTR909")
  .gain(0.8)

// Toques metálicos espaçados no lugar do ritmo acelerado
const metals = s("hh ~ hh ~")
  .cut(1)
  .gain(0.3)
  .delay(0.2)

// Linha de baixo soturna com notas mais longas
const bass = note("d2 a1 f1 e1")
  .slow(2)
  .sound("sawtooth")
  .lpf(400)
  .lpq(2)
  .legato(1)
  .gain(0.6)

// Arpejo espaçado e misterioso
const arpeggio = note("d3 f3 a3 c4 d4 c4 a3 f3")
  .slow(2)
  .sound("square")
  .lpf(1200)
  .decay(0.15)
  .sustain(0)
  .room(0.5)
  .gain(0.4)

// Melodia lenta e tensa
const lead = note("d4 e4 f4 a4 g4 f4 e4 d4")
  .slow(4)
  .sound("triangle")
  .vib(4)
  .delay(0.3)
  .room(0.6)
  .gain(0.5)

// Andamento reduzido para 65 CPM
stack(drums, metals, bass, arpeggio, lead).cpm(65)