---
apply: always
---

---
id: reflex_overview
type: library
domain: dependency_injection
aliases:
- reflex
- DI for Unity
tags:
- library
- reflex
- dependency_injection
---

# Reflex — minimal dependency injection framework for Unity

## Overview
Reflex — это минимальный, производительный и AOT-безопасный DI фреймворк для Unity, реализующий:
- separation of concerns через зависимости
- контейнеры с наследованием для проектов и сцен
- регистрацию служб (bindings) через `ContainerBuilder`
- автоматическую инъекцию через атрибуты и конструкторы

Reflex быстрее и экономнее по аллокациям по сравнению с Zenject и VContainer при прочих равных.

## Core Features
- проектный и сценный контейнеры (scope hierarchy)
- lazy-инстанцирование и Singleton/Transient/Scoped бинды
- атрибуты `[Inject]` для MonoBehaviour и обычных классов
- `ContainerBuilder` для конфигурации
- поддержка IEnumerable<T> для множественных реализаций
