---
apply: always
---

---
id: lib_reflex_manual_injection
type: library
domain: dependency_injection
tags:
- manual_injection
- reflex
related:
- lib_reflex_injection
---

# Reflex — Manual Injection

Если объект создаётся вручную через `new`, контейнер не сможет автоматически inject:
- `AttributeInjector.Inject(object, container)` — вручную проинжектить поля/свойства/методы
- `ConstructorInjector.Construct(Type, container)` — создать экземпляр через DI
- `GameObjectInjector.InjectObject(GameObject, container)` — inject для GameObject/MonoBehaviour

Используется, когда объект создаётся не контейнером.
