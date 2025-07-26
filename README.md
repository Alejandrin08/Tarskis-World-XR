# Tarski's World XR

**Tarski's World XR** es una aplicación interactiva para aprender lógica de primer orden en un entorno de realidad mixta con Meta Quest 3. Inspirado en dicho entorno educativo, este proyecto adapta las ideas de Tarski al espacio físico del usuario.

## Objetivo

Facilitar el aprendizaje de la lógica de primer orden permitiendo manipular figuras 3D.

## Características

- **Figuras 3D**: Representación de objetos geométricos que pueden colocarse sobre superficies reales.
- **Colocación en el entorno real**: Integración con `OVR Raycast Manager` y la API de profundidad de Meta para detectar superficies.
- **Interacción física**: El usuario puede agarrar, mover y soltar figuras con las manos.

## Tecnologías

- Unity con XR Toolkit
- MRUK para mapeo espacial
- OVR Raycast Manager
- Meta Depth API
- Sistema lógico interno para representar y validar predicados
