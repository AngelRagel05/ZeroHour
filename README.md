# ZeroHour

ZeroHour es un juego 2D de plataformas con estética retro en el que el objetivo es terminar el nivel antes de que el tiempo se agote, recogiendo llaves y evitando peligros del escenario.

## Estado del proyecto

Proyecto funcional en Unity con:
- flujo completo de partida (jugar, pausar, ganar, reiniciar, volver al menú)
- sistema de tiempo y ranking Top 5 persistente
- audio dinámico con mezcla por tipo de sonido
- menús estilizados y fondos temáticos personalizados

## Mecánicas principales

- Movimiento lateral con aceleración en suelo/aire
- Salto con `coyote time`, `jump buffer` y `jump cut`
- Detección robusta de suelo y paredes
- Daño con empuje y feedback visual
- Coleccionables tipo llave con contador en HUD
- Generación de llaves desde Tilemap (`Tilemap_Coleccionables`)

## Sistema de audio

### Clips y categorías
- Música gameplay: `S_Background_00..03`
- Música menú/pausa: `S_Pause`
- Música victoria: `S_Victory`
- SFX: salto, correr, daño, coleccionable

### Funcionalidad implementada
- `BackgroundMusicPlayer` persistente entre escenas
- Música aleatoria de gameplay por partida
- Reglas por escena:
  - `MainMenu`: reproduce clip de pausa
  - `SampleScene`: reproduce playlist gameplay
  - `Victory`: reproduce clip de victoria
- Pausa/reanudación conservando posición de reproducción
- Volumen en tiempo real y persistente por canal (PlayerPrefs)

### Control de volumen en tiempo real
Panel de sonido disponible en menús y pausa con sliders independientes:
- Música
- Salto
- Correr
- Daño
- Victoria
- Coleccionable
- Pausa

## UI y menús

- HUD in-game con tiempo, llaves y estados
- Menú de pausa con acciones:
  - Continuar
  - Reiniciar
  - Salir
  - Sonido
- Menú principal y pantalla de victoria estilizados
- Fondos temáticos incluidos:
  - `Assets/Sprites/MenuBackgrounds/MainMenu_BG.png`
  - `Assets/Sprites/MenuBackgrounds/Pause_BG.png`
  - `Assets/Sprites/MenuBackgrounds/Victory_BG.png`

## Escenas

- `MainMenu`
- `SampleScene`
- `Victory`

La escena `GameOver` fue retirada del build por no uso.

## Scripts clave

- `GameManager.cs`: estado global de partida, pausa, victoria, ranking, SFX
- `BackgroundMusicPlayer.cs`: música por escena y continuidad de reproducción
- `UIHUD.cs`: HUD, menús in-game, panel de sonido
- `SimpleSceneMenu.cs`: menú principal/victoria y panel de sonido
- `VagabundoController.cs`: control de personaje y salto
- `Collectible.cs`: recogida de llaves
- `Tilemap_Coleccionables.cs`: instanciación de llaves desde Tilemap
- `AudioSettingsStore.cs`: persistencia de volúmenes

## Controles

- Mover: `A / D` o flechas
- Saltar: `Space`
- Correr: `Shift`
- Pausa: `Esc`

## Requisitos

- Unity `6.2` (proyecto migrado en entorno Unity 6)

## Notas de configuración

- Verificar referencias de `AudioSource` y `AudioClip` en `GameManager` y `MusicPlayer`
- Asignar fondos en menús (Canvas > Image a pantalla completa)
- Si no existe `MusicPlayer` en `MainMenu`, `SimpleSceneMenu` puede crearlo automáticamente
