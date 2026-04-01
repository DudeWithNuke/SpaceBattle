# Поэтапный план рефакторинга `PlaceableObject` и `PlaceableObjectManipulation` (без shape-инициализации)

## Этап 1. Убрать дублирование снятия занятых клеток и cleanup в `GridInteraction`

```markdown
Отрефактори `Assets/Scripts/PlaceableObject/GridInteraction.cs` без изменения поведения:
1) Выдели общий приватный метод для логики "снять занятие клеток + сбросить selected-визуал".
2) Используй его и в `ApplyPick(...)`, и в `CleanupOnDestroy(...)`.
3) Сохрани текущую семантику:
   - `ClearHover()` вызывается как и раньше,
   - `ReleaseCells` вызывается только при `_usesCellOccupancy`,
   - `SetSelected(false, ...)` выполняется только при `_usesCellOccupancy`.
4) Не меняй публичный API класса.

После правки покажи:
- какой метод выделен,
- какие участки кода заменены,
- почему поведение осталось тем же.
```

Критерий готовности:
- В `ApplyPick` и `CleanupOnDestroy` больше нет дублирующегося цикла по occupied cells для снятия `selected`.

---

## Этап 2. Развести precheck в `Selection` и доменную валидацию в `PlaceableObject`

```markdown
Сделай минимальный контрактный рефакторинг проверки размещения между:
- `Assets/Scripts/PlaceableObjectManipulation/Selection.cs`
- `Assets/Scripts/PlaceableObject/PlaceableObject.cs`

Требования:
1) В `Selection.TryPlaceCurrent()` оставь только precheck уровня orchestration:
   - есть `CurrentPickedObject`,
   - нет transition слоя,
   - объект не движется,
   - объект на целевой позиции,
   - у moving есть валидная цель.
2) Финальная доменная валидация возможности размещения должна оставаться в `PlaceableObject.TryPlace()`.
3) Не добавляй новую функциональность и не меняй внешнее поведение результата `TryPlaceCurrent()`.
4) Явно задокументируй комментариями разделение ответственности (коротко, без лишних комментариев).

После правки кратко перечисли:
- что считается precheck,
- что считается domain-check,
- почему дублирование уменьшилось.
```

Критерий готовности:
- Проверки не расползаются между слоями: orchestration-check в `Selection`, placement-check в `PlaceableObject`.

---

## Этап 3. Централизовать трекинг жизненного цикла spawned-объектов

```markdown
Отрефактори подписки/отписки на события `PlaceableObject` между:
- `Assets/Scripts/PlaceableObjectManipulation/Selection.cs`
- `Assets/Scripts/PlaceableObjectManipulation/ButtonsController.cs`

Без изменения поведения:
1) Введи единый компонент/сервис внутри `PlaceableObjectManipulation` (например, `SpawnedObjectLifecycleTracker`), который:
   - регистрирует spawned object,
   - централизованно подписывается на `OnPicked/OnPlaced/OnDestroyed`,
   - пробрасывает события заинтересованным потребителям.
2) `Selection` и `ButtonsController` должны перестать дублировать однотипную подписку/отписку вручную.
3) Не меняй внешний публичный API `Selection` и `ButtonsController` больше необходимого.
4) Не трогай логику за пределами папки `Assets/Scripts/PlaceableObjectManipulation`.

После правки покажи:
- какой новый класс добавлен,
- какие дублирующиеся участки убраны,
- почему риски утечек событий стали ниже.
```

Критерий готовности:
- Нет двух независимых одинаковых реализаций subscribe/unsubscribe на жизненный цикл `PlaceableObject`.

---

## Этап 4. Убрать дублирование grid/origin utility-логики

```markdown
Сделай локальный utility-рефакторинг для координатных и boundary-операций:
1) Вынеси повторяющуюся логику
   - выбора origin по `IsPlayerObject`,
   - world<->cell преобразований,
   - проверки нахождения в пределах grid
   в один utility/service в `Assets/Scripts/PlaceableObjectManipulation` (или в существующий общий utility, если он уже есть).
2) Примени его минимум в:
   - `Assets/Scripts/PlaceableObjectManipulation/Moving.cs`
   - `Assets/Scripts/PlaceableObject/PlaceableObject.cs`
   - (опционально, где уместно) `Assets/Scripts/PlaceableObject/GridInteraction.cs`
3) Не меняй фактическую математику, оффсеты и текущие параметры из settings.
4) Сохрани поведение и сигнатуры публичных методов.

После правки кратко укажи:
- какой utility добавлен,
- какие функции в него перенесены,
- какие дубли удалены.
```

Критерий готовности:
- Повторяющийся код origin/conversion/bounds больше не копипастится по нескольким классам.

---

## Этап 5. Решить судьбу `StateCoordinator` как тонкого proxy-слоя

```markdown
Проведи целевой рефакторинг `Assets/Scripts/PlaceableObjectManipulation/StateCoordinator.cs`:
1) Оцени, дает ли класс дополнительную инвариантную ценность кроме proxy-вызовов.
2) Если нет — убери лишнюю прослойку и переведи вызовы на более прямую зависимость (минимально инвазивно).
3) Если оставляешь класс — добавь в нем реальную точку централизации состояния (например, единый guard/логирование/метрики), чтобы это был не пустой дубликат API.
4) Не меняй поведение игрового процесса.

После правки дай короткий отчет:
- удален ли `StateCoordinator` или усилен,
- какие вызовы были перенаправлены,
- какие компромиссы приняты.
```

Критерий готовности:
- Нет "пустой" прослойки, которая только дублирует методы `PlaceableObject` без добавленной ответственности.

---

## Финальный проход (после этапов 1–5)

```markdown
Сделай финальный проход по `Assets/Scripts/PlaceableObject` и `Assets/Scripts/PlaceableObjectManipulation`:
1) Удали мертвый код после рефакторинга.
2) Проверь корректность подписок/отписок.
3) Проверь, что поведение фильтрации Stored/Placed и selection flow не регресснуло.
4) Подготовь короткий отчет:
   - измененные файлы,
   - оставшиеся риски,
   - ручные тест-сценарии в Unity.
```

Критерий готовности:
- Локальный, последовательный рефакторинг без функционального расширения.
