# 🕯️ Slenderman – Survival Horror en Unity

Proyecto de survival horror en primera persona inspirado en el mito de Slenderman. Combina exploración nocturna, un sistema de miedo progresivo y un enemigo que patrulla, persigue, se teletransporta y cuenta con un "modo loco" de caza agresiva.

## 👣 Características clave
- IA de Slenderman con patrulla inteligente, persecución por rangos, teletransporte y congelación cuando lo miras (opcional).
- Modo loco: ráfagas de velocidad y agresividad con efectos de partículas y audio dedicados.
- Sistema de miedo del jugador: interferencias, viñeteado, aberración cromática, grano, temblor de cámara, desaturación, reducción de luz y velocidad.
- Estamina, sprint y fatiga; pánico temporal que aumenta la velocidad al sobrepasar el umbral de miedo.
- HUD con barras de miedo/estamina, avisos de peligro y retroalimentación audiovisual (respiración, corazón, susurros).
- Escenas incluidas: `MainScene` (juego), `DefeatScene`, `VictoryScene`, `SampleScene` (pruebas).

## 🛠️ Requisitos técnicos
- Unity 6 (6000.2.9f1) o superior.
- Render Pipeline: Universal RP 17.2.0.
- Paquetes usados: Input System 1.14.2, Timeline, UGUI, Visual Scripting, AI Navigation, Multiplayer Center (solo presente, no configurado).

## 📁 Estructura rápida del proyecto
- `Assets/Scenes/` – escenas principales del juego.
- `Assets/Scripts/` – lógica de gameplay (IA de Slenderman, jugador, menú, temporizador nocturno, etc.).
- `Assets/Resources`, `Shaders`, `Sounds`, `Images`, `Videos` – contenido artístico y de audio.

## 🎮 Controles (PC)
- Movimiento: `WASD`.
- Mirar: ratón.
- Correr: `Left Shift` (consume estamina).

## 🚀 Cómo abrir el proyecto
1) Clona o descarga el repositorio.
2) Abre la carpeta `slenderman/` con Unity Hub y selecciona el editor 6000.2.9f1.
3) Carga `Assets/Scenes/MainScene.unity` para probar el gameplay principal.

## 🏗️ Cómo compilar el juego
1) En Unity, ve a `File > Build Settings` y añade las escenas necesarias (MainScene, DefeatScene, VictoryScene).
2) Ajusta la plataforma destino (PC, Mac & Linux Standalone por defecto) y pulsa `Build`.

## 🎨 Notas de diseño y filosofía del juego
- La IA alterna patrullaje y persecución según distancia; al estar fuera de rango puede teletransportarse cerca del jugador.
- Mirar a Slenderman puede congelarlo si está activo el sistema de "freeze when looked at".
- El miedo escala con la cercanía: incrementa interferencias y reduce control/visión; al máximo dispara el modo pánico.
- La luz, el sonido y la interfaz están diseñados para que el jugador se sienta inseguro constantemente.
- El “modo loco” se activa cuando Slenderman entra en estado de caza agresiva, rompiendo momentáneamente el ritmo habitual del juego.

## 🧪 Características en desarrollo / próximas mejoras
- ✔️ Mejoras en la navegación de IA (NavMesh dinámico).
- ✔️ Sistema de coleccionables (páginas).
- ⏳ Nuevo mapa más grande, con zonas diferenciadas.
- ⏳ Mejoras en el sistema de sonido 3D.
- ⏳ Cinemáticas para introducción y final.
- ⏳ Ajustes finos de dificultad y balance.

## 👤 Créditos y licencia
- Código y contenido bajo licencia MIT (ver `LICENSE`).
- Autores: Álvaro Mendo, Antonio Gabriel Cabello y David Blanco (2025).
