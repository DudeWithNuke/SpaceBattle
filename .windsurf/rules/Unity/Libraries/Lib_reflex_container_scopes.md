---
apply: always
---

---
id: lib_reflex_container_scopes
type: library
domain: dependency_injection
tags:
- scope
- reflex
related:
- lib_reflex_overview
---

# Reflex — Container Scopes

## RootScope
Корневой контейнер проекта:
- создаётся только при загрузке первой сцены, содержащей SceneScope
- наследует все ProjectScope установки
- может быть расширен через `ContainerScope.OnRootContainerBuilding`
- уничтожается при закрытии приложения

## SceneScope
Сцена может иметь собственный контейнер:
- наследует RootScope
- регистрирует зависимости через `IInstaller` компоненты на объекте SceneScope
- SceneScope создаётся **до Awake**

## Manual Scoping
Можно расширять контейнеры вручную:
using var scoped = parentContainer.Scope(builder => { /* extra registrations */ });
Это создаёт дочерний контейнер со своими регистрациями.
