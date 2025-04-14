---
id: agent-controller
title: Agent Controller
sidebar_label: Agent Controller
---

# Agent Controller

The **Agent Controller** manages the behavior, energy, lifecycle, and interactions of dynamic and static agents in the Stoat vs Vole Simulation Platform.

## Responsibilities
- Handle agent aging
- Manage agent energy dynamics
- Process resource exchanges between agents
- Coordinate expiration and replication
- Interface with the simulation manager

## Core Methods
- `HandleAging()`
- `HandleOutgoingRequests()`
- `HandleIncomingRequests()`
- `CheckExpirationConditions()`
- `Expire()`

## Lifecycle
Agents follow this basic update loop:
1. Age over time.
2. Consume or provide energy to nearby agents.
3. Expire and optionally replicate if conditions are met.

---

*(More detailed sections about energy transfer, request queues, and replication logic will be added here soon.)*
