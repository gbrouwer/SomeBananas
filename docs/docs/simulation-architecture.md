---
id: simulation-architecture
title: Simulation Architecture
sidebar_label: Simulation Architecture
---

# Simulation Architecture Overview

The Stoat vs Vole Simulation Platform models complex ecological systems with emergent behavior. The architecture is designed to be modular, scalable, and flexible for advanced agent-based simulations and machine learning training.

---

## High-Level Structure

| Component | Responsibility |
|:---|:---|
| **AgentManager** | Instantiates, resets, and manages all agent lifecycles |
| **CoverManager** | Manages the logical grid for agent placement, regrowth, and spatial dynamics |
| **AgentController** | Controls the internal state and behavior of each agent (energy, aging, replication, expiration) |
| **GlobalSettings** | Centralized configuration for world size, environmental bounds, etc. |
| **WallSpawner** | Procedurally generates environmental boundaries |
| **Training Hooks** | ML-Agents integration for dynamic agent learning and training pipelines |

---

## Agent Types

- **Static Agents**: (e.g., Flowers)  
  - Non-moving, energy providing agents.
  - Can expire and replicate.
- **Dynamic Agents**: (e.g., Voles)  
  - Actively move, seek energy sources, and interact with environment.

---

## Lifecycle Flow

1. **Initialization**:  
   - Environment size determined by GlobalSettings.
   - Walls spawned by WallSpawner.
   - Agents spawned using AgentManager + CoverManager grid.

2. **Simulation Step (per frame)**:  
   - Agents age and consume energy (AgentController).
   - Resource exchanges occur between agents.
   - Expired agents removed or replaced via clustering logic.

3. **Training (optional)**:  
   - ML-Agents allows dynamic agents to learn behavior policies through reinforcement learning.

---

## Future Extensions

- Dynamic environmental changes (e.g., seasons)
- New agent types (predators, diseases)
- Complex energy/resource networks
- Procedural terrain variation

---

*(More detailed subsystem diagrams and API links will be added soon.)*
