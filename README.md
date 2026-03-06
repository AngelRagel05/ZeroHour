# ZeroHour

Juego de plataformas 2D en Unity con enfoque dinámico: avanzar rápido, esquivar hazards y recoger llaves para completar el nivel.

## Autor

- Desarrollado por: **Ángel Jiménez Ragel**
- Correo: **jimenezragelangel@gmail.com**
- Teléfono: **603758003**
- GitHub: **https://github.com/AngelRagel05**

## Cómo jugar desde GitHub

### Opción 1: Ejecutable (recomendado)
1. Descarga el repositorio en `.zip` desde GitHub y extráelo.
2. Abre la carpeta de build de Windows (por ejemplo `Builds/Windows`, si está incluida en el repo o release).
3. Ejecuta `ZeroHour.exe`.
4. Mantén el `.exe` junto a su carpeta `*_Data`.

### Opción 2: Jugar desde Unity
1. Descarga el repositorio en `.zip` y extráelo.
2. Abre Unity Hub y selecciona `Open`.
3. Elige la carpeta del proyecto.
4. Abre la escena `Assets/Scenes/MainMenu.unity`.
5. Pulsa `Play`.

## Resumen del proyecto

ZeroHour está construido como un nivel autónomo y jugable con flujo completo de partida:
- inicio en `MainMenu`
- gameplay en `SampleScene`
- final en `Victory`

Incluye HUD, sistema de tiempo, llaves coleccionables, pausa funcional, ranking persistente y audio configurable por canales.

## Características implementadas

### Jugabilidad
- Movimiento lateral fluido con físicas 2D.
- Salto mejorado (`coyote time`, `jump buffer`, `jump cut`).
- Hazards y daño con feedback visual.
- Sistema de llaves como coleccionables.
- Condición de victoria al completar objetivos del nivel.

### UI y flujo de escenas
- Menú principal estilizado.
- HUD en partida con tiempo y contador de llaves.
- Menú de pausa con `Continuar`, `Reiniciar`, `Salir` y `Sonido`.
- Escena final de victoria.
- Ranking Top 5 persistente para el podio final.

### Audio
- Playlist de música gameplay (`S_Background_00..03`) con selección aleatoria por partida.
- Música de menú/pausa (`S_Pause`) y victoria (`S_Victory`).
- SFX de salto, correr, daño y coleccionable.
- Panel de sonido con control en tiempo real por canal.
- Persistencia de volúmenes mediante `PlayerPrefs`.

### Arte y presentación
- Fondos temáticos por menú:
  - `Assets/Sprites/MenuBackgrounds/MainMenu_BG.png`
  - `Assets/Sprites/MenuBackgrounds/Pause_BG.png`
  - `Assets/Sprites/MenuBackgrounds/Victory_BG.png`
- UI actualizada para mejorar legibilidad y presentación general.

## Escenas

- `MainMenu`
- `SampleScene`
- `Victory`

La escena `GameOver` fue retirada del proyecto por no uso.

## Scripts principales

- `GameManager.cs`: lógica central de partida, pausa, victoria, ranking y SFX.
- `BackgroundMusicPlayer.cs`: control de música por escena y continuidad de reproducción.
- `UIHUD.cs`: HUD in-game, pausa y panel de sonido.
- `SimpleSceneMenu.cs`: control de botones de menú y navegación entre escenas.
- `VagabundoController.cs`: control del personaje y mecánicas de movimiento/salto.
- `Collectible.cs`: detección y recogida de llaves.
- `Tilemap_Coleccionables.cs`: generación de coleccionables desde Tilemap.
- `AudioSettingsStore.cs`: guardado/carga de volúmenes de audio.

## Controles

- Mover: `A / D` o flechas
- Saltar: `Space`
- Correr: `Shift`
- Pausa: `Esc`

## Requisitos técnicos

- Unity `6.2` (Unity 6 LTS)

## Notas de configuración

- Revisar referencias de `AudioSource` y `AudioClip` en `GameManager` y `MusicPlayer`.
- Verificar que cada Canvas de menú tenga su fondo asignado.
- Si `MusicPlayer` no existe en `MainMenu`, `SimpleSceneMenu` puede crearlo automáticamente.
