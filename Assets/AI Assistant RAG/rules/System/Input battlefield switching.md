---
apply: always
---

---
id: input_battlefield_switching
type: system
domain: input
tags:
- input
- battlefield
- camera
related:
- entity_cell_grid
- entity_cursor_plane
---

# Battlefield Switching

## Переключение между игровыми полями

**Shift** — переключение между:
- своим игровым полем
- вражеским игровым полем

### Поведение при переключении
Система сохраняет состояние для каждого поля отдельно:
- позиция камеры
- угол поворота камеры
- уровень Cursor Plane

При возврате на поле камера и Cursor Plane восстанавливаются в том положении, в котором находились ранее.
