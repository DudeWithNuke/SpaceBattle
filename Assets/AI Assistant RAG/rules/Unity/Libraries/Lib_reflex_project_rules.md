---
apply: always
---

---
id: lib_reflex_project_rules
type: rules
domain: dependency_injection
tags:
- reflex
- project_rules
---

# Reflex — Project Rules

## 1) Использовать только Reflex для DI
Запрос в коде всегда должен опираться на RAG-описанные APIs Reflex.  
Запрещено:
- пытаться писать код “как Zenject/VContainer”
- использовать их синтаксис или идеи из них

## 2) MonoBehaviour НЕ является DI-контейнером
MonoBehaviour служат View/Adapter, а не контейнерами.

## 3) Все сервисы регистрируются через ContainerBuilder
RootScope и SceneScope создаются для DI, а не для логики.

## 4) Создание объектов
- не использовать `new` в местах, где DI ожидается
- использовать `ConstructorInjector` или контейнер

## 5) Инъекция полей/методов
Использовать только `[Inject]` из Reflex

---

## 6) Ошибки compile → привязка к RAG
Если модель пишет неизвестные методы,
указывай в promt **использовать только API из этих RAG-файлов**.
