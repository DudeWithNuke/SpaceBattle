---
apply: by model decision
---

# Reflex — Settings и Debug

## ReflexSettings
Это ScriptableObject, который тегируется в Resources.  
Содержит список RootScope префабов, которые будут строить корневой контейнер.

## Debugger
Меню: Window → Analysis → Reflex Debugger  
Показывает:
- иерархию контейнеров
- bindings
- разрешенные экземпляры

Важно: в релизе `REFLEX_DEBUG` повышает аллокации и снижает производительность.
