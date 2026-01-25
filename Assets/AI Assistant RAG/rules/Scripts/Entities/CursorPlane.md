---
apply: always
---

---
id: entity_cursor_plane
type: entity
domain: battlefield
aliases:
- cursor plane
- навигационная плоскость
- плоскость навигации
tags:
- navigation
- grid
- interaction
version: 1.0
related:
- entity_cell
- entity_cell_grid
---

# Cursor Plane

## Definition
Cursor Plane — сущность, обеспечивающая навигацию по Grid за счёт выбора активного слоя ячеек.

---

## Behavior
Cursor Plane:
- располагается на определённом уровне оси `Y`
- активирует слой Cell с совпадающим `Y`
- делает активные Cell доступными для взаимодействия
- все остальные Cell считаются неактивными

---

## Interaction Rules
- Взаимодействие возможно **только** с Cell активного слоя.
- Cursor Plane может перемещаться вдоль оси `Y`.
- Перемещение Cursor Plane изменяет доступный слой Grid.

---

## Gameplay Role
Cursor Plane используется для:
- навигации внутри трёхмерного Grid
- выбора слоя для размещения объектов
- выбора слоя для атак и разведки
