---
apply: always
---

---
id: lib_reflex_bindings
type: library
domain: dependency_injection
tags:
- bindings
- reflex
related:
- lib_reflex_container_scopes
---

# Reflex — Bindings
Reflex поддерживает несколько видов регистрации зависимостей:

## RegisterValue
Регистрирует готовый объект как Singleton:
builder.RegisterValue(instance, typeof(IMyContract), typeof(IAnother));

Всегда ведёт себя как Singleton.

## RegisterType
Регистрация типа, который контейнер сам создаёт:
builder.RegisterType<MyService>(typeof(IMyService));

Контейнер вызывает конструктор и резолвит зависимости сам.

## RegisterFactory
Регистрация через фабрику:
builder.RegisterFactory(container => new MyObject(...), typeof(IMyContract));

Полный контроль над созданием с доступом к контейнеру.

## Lifetimes
- Singleton — одна копия на весь контейнер
- Transient — новый объект при каждом разрешении
- Scoped — один объект на контейнер (например, сцена)
