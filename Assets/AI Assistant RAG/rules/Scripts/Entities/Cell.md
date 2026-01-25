---
apply: by model decision
---

---
id: entity_cell
type: entity
domain: battlefield
aliases:
- cell
- ячейка
- клетка
tags:
- battlefield
- grid
- interaction
- placement
related:
- entity_cell_grid
- entity_cursor_plane
---

# Cell

## Definition
Cell — минимальная структурная единица игрового поля, представляющая одну позицию в трехмерной сетке Grid.
Каждая Cell имеет фиксированные координаты `(x, y, z)` и принадлежит ровно одному Grid.

---

## Cell States
Cell может находиться **ровно в одном визуально-логическом состоянии**:
- `DisabledLayer`  
  Ячейка находится вне активного слоя навигации.
- `ActiveLayer`  
  Ячейка принадлежит активному слою Cursor Plane и доступна для взаимодействия.
- `Hovered`  
  Внутри ячейки находится выбранный Placeable Object.
- `Selected`  
  Placeable Object был размещён в ячейке.
- `HoveredSelected`  
  Курсор находится над размещённым объектом, объект можно повторно подобрать.
- `EmptyAttacked`  
  Пустая ячейка подверглась атаке.
- `ShipAttacked`  
  Ячейка содержит часть корабля и была атакована.

---

## Gameplay Usage
Cell используется для:
- размещения кораблей
- атак
- разведки
- применения способностей

---

## Visual Representation
Cell визуально представлена как трёхмерный куб с wireframe-оформлением.  
Цвет и визуальное состояние wireframe напрямую зависят от текущего `CellState`.

---

## Notes
- Cell не является физическим объектом Unity по умолчанию.
- Логика взаимодействия определяется состоянием Cursor Plane.
