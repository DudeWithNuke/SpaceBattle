---
apply: always
---

---
id: lib_reflex_injection
type: library
domain: dependency_injection
tags:
- injection
- reflex
  related:
- lib_reflex_bindings
---

# Reflex — Dependency Injection

## Constructor Injection
Контейнер вызывает конструктор с зависимостями:
class Foo { public Foo(IBar bar) { ... } }

Рекомендуется для обычных классов.

## Attribute Injection — MonoBehaviour
Reflex использует `[Inject]` для полей, свойств и методов:
class Foo : MonoBehaviour {
[Inject] private readonly IBar _bar;
}

Работает также на неполях.

## Inject Constructor Attribute
Если есть несколько конструкторов, можно явно выбрать через:
[ReflexConstructor]
public Foo(IBar bar) {...}

Если его не использовать, выбирается конструктор с наибольшим количеством параметров.



