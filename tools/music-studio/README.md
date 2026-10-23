# Music Studio

A música do ChamberCrawl é gerada em [https://strudel.cc/](https://strudel.cc/).

Os padrões ficam em `ChamberCrawl/Assets/MusicSources/`. Exporte o áudio pelo Strudel e salve o WAV em `ChamberCrawl/Assets/Music/`.

## Export (Strudel)

Na aba **export**, use:

| Campo | Valor | Nota |
|---|---|---|
| **File name** | nome do tema (ex. `FirstFloorTheme`) | mesmo nome do `.js` / WAV |
| **Start cycle** | `0` | sempre do início |
| **End cycle** | `16` | loop limpo (~15–20 s). Temas com `.slow(4)` fecham a cada 4 ciclos. `32` se quiser mais longo |
| **Sample rate** | `44100` | sobra para BGM e arquivo menor que 48000 |
| **Maximum polyphony** | `128` | `1024` é overkill |
| **Multi Channel Orbits** | **desmarcado** | senão o WAV fica gordo / multi-canal |

Depois: **Export to WAV** → `ChamberCrawl/Assets/Music/<Nome>.wav`.
