---
apply: always
---

---
id: lib_reflex_resolving
type: library
domain: dependency_injection
tags:
- resolving
- reflex
related:
- lib_reflex_injection
---

# Reflex — Resolving Dependencies

## Resolve
`container.Resolve<T>()` возвращает последний зарегистрированный binding.  
Это метод, когда ожидается ровно одна реализация.

## Single
`container.Single<T>()` валидирует, что ровно один binding реализует контракт.  
Бросает исключение, если больше одного.

## All
`container.All<T>()` возвращает все реализации контракта.  
Полезно, когда у тебя много зарегистрированных реализаций одного интерфейса.
