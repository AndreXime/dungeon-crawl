// Santuário Sombrio — ritual maligno, carmesim, runas (exploração final)

// Batida de culto: kick pesado, snare raro como golpe de faca
const drums = s("bd ~ bd ~, ~ ~ sd ~")
  .bank("RolandTR909")
  .gain(0.72)

// Chocalho seco, quase ossos
const metals = s("hh ~ hh ~")
  .cut(1)
  .gain(0.16)
  .hpf(7000)
  .room(0.35)

// Baixo Locrian em D — tritone (ab) e semitom (eb) puxando o mal
const bass = note("d1 ab1 eb2 d1")
  .slow(2)
  .sound("sawtooth")
  .lpf(220)
  .lpq(8)
  .legato(1.35)
  .gain(0.7)

// Cluster dissonante (menor + tritone), como coro invertido
const arpeggio = note("d3 eb3 ab3 a3 ab3 eb3 d3 ab2")
  .slow(2)
  .sound("square")
  .lpf(650)
  .decay(0.35)
  .sustain(0.15)
  .room(0.9)
  .gain(0.3)

// Melodia torta: sobe no diabo (ab) e cai no semitom
const lead = note("d4 eb4 ab4 a4 ab4 f4 eb4 d4")
  .slow(4)
  .sound("sawtooth")
  .lpf(900)
  .vib(6)
  .delay(0.6)
  .room(0.95)
  .gain(0.4)

stack(drums, metals, bass, arpeggio, lead).cpm(52)
