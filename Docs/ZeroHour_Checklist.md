# ZeroHour - Checklist de montaje rapido

## Escenas
- [ ] Crear escena `MainMenu`
- [ ] Renombrar nivel jugable a `Level_01` (o usar `SampleScene`)
- [ ] Crear escena `Victory`
- [ ] Crear escena `GameOver`
- [ ] Agregar las 4 escenas en Build Settings

## Nivel
- [ ] Tilemap_Background sin collider
- [ ] Tilemap_Solid con TilemapCollider2D
- [ ] Tilemap_Lava con Collider2D trigger + `LavaHazard`
- [ ] Objeto salida con Collider2D trigger + `GoalZone`
- [ ] 1-2 checkpoints con Collider2D trigger + `Checkpoint`
- [ ] KillZone inferior (trigger + `KillZone`)

## Jugador
- [ ] Rigidbody2D dinamico, gravedad activa, freeze rotation Z
- [ ] Collider2D ajustado al sprite
- [ ] `VagabundoController` asignado
- [ ] Crear child `GroundCheck` bajo los pies y asignarlo en script
- [ ] Poner layer de suelo en `groundLayer`

## Sistemas
- [ ] Crear `GameManager` en la escena del nivel
- [ ] Configurar vidas (3) y tiempo (90-120)
- [ ] Asignar nombres de escenas en GameManager
- [ ] Asignar AudioSource y clips SFX (opcional pero recomendado)

## UI
- [ ] Canvas + 2 Text: Tiempo y Vidas
- [ ] (Opcional) Text de estado
- [ ] Agregar `UIHUD` y asignar referencias

## Menus
- [ ] En MainMenu crear boton Jugar con `SceneButtons.Jugar()`
- [ ] En Victory/GameOver crear botones Menu/Reintentar con `SceneButtons`

## QA minimo
- [ ] Se puede terminar sin bugs bloqueantes
- [ ] Timer llega a 0 y pasa a GameOver
- [ ] Lava quita vidas y respawnea
- [ ] Con 0 vidas pasa a GameOver
- [ ] Al llegar a meta pasa a Victory