# ZeroHour - Documento de Diseno (Borrador Entregable)

## 1. Concepto
ZeroHour es un plataformas 2D contrarreloj. El protagonista (Vagabundo) debe escapar de un edificio en llamas antes de que el tiempo llegue a 00:00.

## 2. Objetivo del nivel
- Inicio: punto de spawn al pie del edificio.
- Fin: alcanzar la zona de salida (GoalZone).
- Condiciones de derrota:
  - Tiempo agotado.
  - Vidas a 0 tras tocar lava o caer en zonas mortales.

## 3. Mecanicas principales
- Movimiento horizontal y salto con fisica 2D.
- Coyote time y jump buffer para mejorar control.
- Checkpoints para reducir frustracion.
- Hazard principal: lava (danio y respawn).
- Sistema de vidas + temporizador visible en UI.

## 4. Diseno del recorrido (1-3 minutos)
- Tramo 1 (tutorial corto): salto basico y lectura del timer.
- Tramo 2 (ritmo medio): plataformas separadas + primer contacto con lava.
- Tramo 3 (presion): saltos encadenados sobre hazard.
- Tramo 4 (final): subida corta y sprint hacia la salida.

Duracion objetivo: 90-120 segundos en primera partida.

## 5. Implementacion tecnica
- Motor: Unity 6 LTS.
- Escenario: Tilemaps separados por funcion:
  - Tilemap_Background (visual, sin collider).
  - Tilemap_Solid (suelo/plataformas con colision).
  - Tilemap_Lava (trigger de hazard).
- Scripts clave:
  - VagabundoController.cs
  - GameManager.cs
  - LavaHazard.cs
  - GoalZone.cs
  - Checkpoint.cs
  - UIHUD.cs
  - SceneButtons.cs

## 6. UI y feedback
- HUD en pantalla: tiempo restante y vidas.
- Feedback de estado: mensaje de nivel terminado.
- SFX de salto, dano, checkpoint y victoria.

## 7. Decisiones de diseno
- Sin enemigos para mantener foco en plataformas y tiempo.
- Riesgo principal: posicionamiento y precision de salto.
- Dificultad escalada por layout y presion del cronometro.

## 8. Riesgos y mitigacion
- Riesgo: colision mezclada con tiles decorativos.
- Mitigacion: separar tilemaps visuales y colisionables.
- Riesgo: nivel demasiado largo.
- Mitigacion: testear ruta ideal y ajustar timer.