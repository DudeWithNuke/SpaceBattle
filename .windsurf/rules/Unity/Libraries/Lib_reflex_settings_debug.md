---
id: lib_reflex_settings_debug
type: library
domain: dependency_injection
tags:
  - settings
  - debug
  - reflex
related:
  - lib_reflex_overview
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
