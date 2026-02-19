# Адаптация алгоритма Польти + Пропп к базовой модели проекта

## 1) Цель адаптации

В проекте базовая модель (`BaseClasses`) уже задаёт универсальный контейнер истории:
- `Plot` хранит все элементы истории и их временную ось.
- `Element` задаёт сущности типа `Character`, `Location`, `Item`, `Event` и словарь `Params` для расширяемых полей.

Поэтому алгоритм лучше внедрять не как отдельную жёсткую иерархию классов, а как **правила заполнения `Element.Params` + последовательность добавления `Event` в `Plot`**.

---

## 2) Маппинг теории на текущие сущности

### 2.1 Польти (драматическая ситуация) → метаданные истории и ролей

Рекомендуемое хранение:
- В `Plot` через служебный элемент `Event` с `Name = "StoryFrame"` (или отдельный корневой `Event`) хранить:
  - `PoltiSituationId`
  - `PoltiSituationName`
  - `ConflictType`
  - `TargetEnding`
- В `Character.Params` хранить назначенную роль из Польти:
  - `PoltiRole` (например, `Persecutor`, `Victim`, `Rescuer`)
  - `Goal`, `Fear`, `InternalConflict`

Это не ломает текущую модель и использует уже существующий словарь `Params`.

### 2.2 Пропп (функции) → события временной шкалы

Каждая функция Проппа представляется `Element` типа `Event`:
- `Name`: короткий идентификатор функции (`"Interdiction"`, `"Violation"`, `"Struggle"` и т.д.)
- `Description`: текст события
- `Params`:
  - `ProppFunctionId`
  - `ProppFunctionName`
  - `Act` (I/II/III)
  - `SceneIndex`
  - `Cause`
  - `Consequence`
  - `StakesDelta`
  - `PoltiConflictProgress`

Порядок задаётся полем `Time` (через `Plot.Add()` он уже выставляется автоматически).

---

## 3) Минимальная структура пайплайна в терминах текущего кода

### Шаг A. Инициализация каркаса
1. Создать `Plot`.
2. Добавить персонажей (`Element` с `ElemType.Character`) и базовые параметры арок в `Params`.
3. Добавить стартовое событие-фрейм с выбранной ситуацией Польти.

### Шаг B. Построение цепочки Проппа
1. Сгенерировать список функций Проппа с учётом допустимых переходов.
2. Для каждой функции создать `Event`.
3. Заполнить причинно-следственные поля (`Cause`, `Consequence`) и прогресс конфликта Польти.
4. Добавить событие в `Plot` через `Add`.

### Шаг C. Связи между сущностями
Через `Binder.Bind(...)` закреплять:
- `Character ↔ Event` (кто участвует),
- `Location ↔ Event` (где происходит),
- `Item ↔ Event` (что задействовано).

Это превращает линейную последовательность функций в граф истории.

### Шаг D. Валидация и коррекция
Перед финальным выводом:
- проверить обязательные роли Польти,
- проверить корректность переходов функций Проппа,
- проверить рост ставок (`StakesDelta`),
- проверить, что кульминация действительно разрешает конфликт Польти.

При нарушении — удалить/перестроить проблемные события и повторить проверку.

---

## 4) Контракт генератора под интерфейсы BaseClasses

Чтобы встроить алгоритм в текущую архитектуру, удобно реализовать:

- `IGenerator` для генерации одного события (`Event`) по входному контексту.
- `IChainGenerator` для сборки полной цепочки событий (основная логика Проппа).

Рекомендуемый контекст в `preparedElement.Params`:
- `Genre`, `Tone`, `Length`, `TargetEnding`,
- `PoltiSituationId` (если зафиксировано заранее),
- `RequiredCharacters`,
- `RandomSeed`.

Результат:
- Возвращаемый элемент + добавленные в `Plot` события и связи.

---

## 5) Рекомендуемый шаблон Params (базовая модель)

### Для `Character`
- `PoltiRole`: string
- `Goal`: string
- `Fear`: string
- `InternalConflict`: string
- `ArcState`: string

### Для `Event` (функция Проппа)
- `ProppFunctionId`: int
- `ProppFunctionName`: string
- `Act`: int
- `SceneIndex`: int
- `Cause`: string
- `Consequence`: string
- `StakesDelta`: int
- `PoltiConflictProgress`: string
- `ValidationFlags`: List<string>

### Для корневого сюжетного фрейма
- `PoltiSituationId`: int
- `PoltiSituationName`: string
- `ConflictType`: string
- `TargetEnding`: string
- `Theme`: string

---

## 6) Практический результат для проекта

Такая адаптация даёт:
1. Совместимость с уже существующими `Plot/Element/Binder/Serializer`.
2. Прозрачную сериализацию сюжета без отдельного формата.
3. Возможность постепенно усложнять генератор, не меняя базовую модель.
4. Единый источник правды: теоретические правила хранятся как данные/валидации, а не как жёстко зашитые классы.

---

## 7) Структура классов, необходимая для реализации

Ниже приведён минимально достаточный набор классов, который укладывается в текущую архитектуру `BaseClasses`.

### 7.1 Доменные справочники (теоретические правила)

#### `PoltiSituationDefinition`
Назначение: описание одной драматической ситуации Польти.

Поля:
- `int Id`
- `string Name`
- `string ConflictType`
- `List<PoltiRoleType> RequiredRoles`
- `List<string> TypicalTriggers`
- `List<string> TypicalResolutions`

#### `ProppFunctionDefinition`
Назначение: описание одной функции Проппа и её ограничений в цепочке.

Поля:
- `int Id`
- `string Name`
- `bool IsOptional`
- `List<int> AllowedNextFunctionIds`
- `List<int> AllowedPreviousFunctionIds`
- `string NarrativePurpose`

#### `NarrativeKnowledgeBase`
Назначение: единая точка доступа к данным Польти и Проппа.

Поля:
- `IReadOnlyList<PoltiSituationDefinition> PoltiSituations`
- `IReadOnlyList<ProppFunctionDefinition> ProppFunctions`

Методы:
- `PoltiSituationDefinition GetPoltiById(int id)`
- `ProppFunctionDefinition GetProppById(int id)`
- `bool IsTransitionAllowed(int fromFunctionId, int toFunctionId)`

### 7.2 Контекст генерации

#### `NarrativeGenerationRequest`
Назначение: нормализованный вход генератора.

Поля:
- `string Genre`
- `string Tone`
- `int TargetScenes`
- `string TargetEnding`
- `int? FixedPoltiSituationId`
- `int RandomSeed`
- `Dictionary<string, object> Constraints`

#### `NarrativeGenerationContext`
Назначение: рабочее состояние генерации в рантайме.

Поля:
- `Plot Plot`
- `NarrativeGenerationRequest Request`
- `PoltiSituationDefinition SelectedPolti`
- `List<ProppFunctionDefinition> ProppPlan`
- `Dictionary<string, object> RuntimeState`

### 7.3 Сервисы планирования и валидации

#### `IPoltiSelector` / `PoltiSelector`
Назначение: выбор драматической ситуации по ограничениям запроса.

Метод:
- `PoltiSituationDefinition Select(NarrativeGenerationRequest request, NarrativeKnowledgeBase kb)`

#### `IProppSequencePlanner` / `ProppSequencePlanner`
Назначение: построение последовательности функций Проппа.

Метод:
- `List<ProppFunctionDefinition> BuildPlan(PoltiSituationDefinition polti, NarrativeGenerationRequest request, NarrativeKnowledgeBase kb)`

#### `IBeatInstantiator` / `BeatInstantiator`
Назначение: материализация функции Проппа в `Element` типа `Event`.

Метод:
- `IElement CreateEvent(ProppFunctionDefinition fn, NarrativeGenerationContext context, int sceneIndex)`

Правило заполнения `Params`:
- `ProppFunctionId`, `ProppFunctionName`, `Act`, `SceneIndex`, `Cause`, `Consequence`, `StakesDelta`, `PoltiConflictProgress`.

#### `IStoryValidator` / `StoryValidator`
Назначение: проверка целостности после генерации.

Методы:
- `ValidationResult ValidatePoltiRoles(Plot plot, PoltiSituationDefinition polti)`
- `ValidationResult ValidateProppTransitions(Plot plot, NarrativeKnowledgeBase kb)`
- `ValidationResult ValidateCausality(Plot plot)`
- `ValidationResult ValidateClimaxResolution(Plot plot, PoltiSituationDefinition polti)`

#### `ValidationResult`
Поля:
- `bool IsValid`
- `List<string> Errors`
- `List<string> Warnings`

### 7.4 Оркестратор (главный генератор)

#### `StructuredNarrativeGenerator : IChainGenerator`
Назначение: связать все этапы в единую цепочку под интерфейс базовой модели.

Зависимости (через DI/конструктор):
- `NarrativeKnowledgeBase`
- `IPoltiSelector`
- `IProppSequencePlanner`
- `IBeatInstantiator`
- `IStoryValidator`

Ключевой метод:
- `Task<IElement> GenerateChainAsync(Plot plot, IElement preparedElement, Queue<(IElement, IElement, int)> generationQueue = null, int recursion = 3)`

Пошагово внутри:
1. `preparedElement.Params` → `NarrativeGenerationRequest`.
2. Выбор `SelectedPolti`.
3. Построение `ProppPlan`.
4. Создание и добавление `Event` через `plot.Add(...)`.
5. Связывание `Character/Location/Item` через `Binder.Bind(...)`.
6. Валидация и точечный repair (перестройка проблемных событий).
7. Возврат финального кульминационного или последнего события цепочки.

### 7.5 Адаптер для работы с существующими `Element.Params`

#### `NarrativeParamsMapper`
Назначение: безопасное чтение/запись typed-данных в `Dictionary<string, object>`.

Методы:
- `NarrativeGenerationRequest ReadRequest(IElement preparedElement)`
- `void WriteStoryFrame(IElement frameEvent, PoltiSituationDefinition polti, NarrativeGenerationRequest request)`
- `void WriteCharacterRole(IElement character, string poltiRole, string goal, string fear, string internalConflict)`
- `void WriteProppEventFields(IElement eventElement, ProppFunctionDefinition fn, int act, int sceneIndex, string cause, string consequence, int stakesDelta, string poltiProgress)`

### 7.6 Рекомендуемая файловая раскладка

Если выносить в отдельный модуль генерации, структура может быть такой:

```text
src/AI/AIGenerator/StructuredNarrative/
  Definitions/
    PoltiSituationDefinition.cs
    ProppFunctionDefinition.cs
    NarrativeKnowledgeBase.cs
  Contracts/
    NarrativeGenerationRequest.cs
    NarrativeGenerationContext.cs
    ValidationResult.cs
  Services/
    PoltiSelector.cs
    ProppSequencePlanner.cs
    BeatInstantiator.cs
    StoryValidator.cs
    NarrativeParamsMapper.cs
  Generators/
    StructuredNarrativeGenerator.cs
```

Эта схема даёт чистое разделение:
- данные правил,
- контракты,
- бизнес-логика,
- orchestration-слой под существующие интерфейсы `BaseClasses`.


---

## 8) Где и как используется нейросетевая генерация

Коротко: нейросеть здесь должна использоваться как **семантический слой**, а не как источник «сырой структуры».

- **Структура сюжета** (выбор ситуации Польти, последовательность функций Проппа, валидные переходы, проверки целостности) строится детерминированным пайплайном из разделов 3 и 7.
- **Нейросеть** подключается для генерации содержательного текста и soft-решений там, где жёсткие правила недостаточны.

### 8.1 Точки интеграции LLM в пайплайн

#### Точка A: выбор/уточнение конфигурации (до построения цепочки)
Когда использовать:
- пользователь даёт «размытый» запрос (например, только тема и настроение),
- нужен подбор наиболее подходящей ситуации Польти.

Как использовать:
- LLM получает `NarrativeGenerationRequest` и краткий список кандидатов,
- возвращает ранжирование или рекомендацию (`PoltiSituationId`, rationale).

Ограничение:
- финальный выбор всё равно проходит валидацию в `PoltiSelector`.

#### Точка B: инстанцирование событий Проппа (основной контент)
Когда использовать:
- есть готовая функция Проппа и контекст персонажей/конфликта.

Как использовать:
- `BeatInstantiator` формирует prompt из:
  - `ProppFunctionDefinition`,
  - текущего `world state` (`NarrativeGenerationContext`),
  - целевого тона/жанра.
- LLM генерирует:
  - `Description`,
  - `Cause`, `Consequence`,
  - `PoltiConflictProgress`,
  - при необходимости реплики/микросцену.

Ограничение:
- `StoryValidator` проверяет результат и отклоняет невалидные события.

#### Точка C: генерация арок персонажей и мотивации
Когда использовать:
- при заполнении `Character.Params` (`Goal`, `Fear`, `InternalConflict`).

Как использовать:
- LLM создаёт психологически связные формулировки,
- `NarrativeParamsMapper` записывает их в стандартизированные поля.

#### Точка D: стилизация финального вывода
Когда использовать:
- после валидации структуры, перед экспортом текста для сценариста.

Как использовать:
- LLM переформулирует структурированные биты в:
  - синопсис,
  - посценник,
  - «режиссёрскую» версию,
  - игровой квестовый формат.

Ограничение:
- стилизация не меняет `Id`, `Time`, связность событий и обязательные поля.

### 8.2 Какие классы отвечают за LLM-слой

Чтобы интеграция была изолированной и управляемой, добавляется отдельный сервисный слой:

#### `INarrativeTextGenerator`
Назначение: абстракция над LLM-провайдером.

Методы (пример):
- `Task<PoltiSelectionResult> SuggestPoltiAsync(NarrativeGenerationRequest request, IReadOnlyList<PoltiSituationDefinition> candidates)`
- `Task<BeatTextResult> GenerateBeatTextAsync(ProppFunctionDefinition fn, NarrativeGenerationContext context)`
- `Task<CharacterArcTextResult> GenerateCharacterArcAsync(IElement character, NarrativeGenerationContext context)`
- `Task<string> StylizeAsync(Plot plot, string outputFormat, string stylePreset)`

#### `LlmNarrativeTextGenerator`
Назначение: реализация `INarrativeTextGenerator` поверх текущего AI-модуля (`AIGenerator`, `LlmAiGenerator`, `ITextAIGenerator`).

Принцип:
- использует существующие клиентские механизмы вызова модели,
- возвращает строго типизированные DTO для безопасной записи в `Params`.

#### DTO-ответы LLM
- `PoltiSelectionResult` (`PoltiSituationId`, `Confidence`, `Reasoning`)
- `BeatTextResult` (`Description`, `Cause`, `Consequence`, `StakesDeltaHint`, `PoltiConflictProgress`)
- `CharacterArcTextResult` (`Goal`, `Fear`, `InternalConflict`, `ArcStateHint`)

### 8.3 Режимы работы: deterministic-first

Рекомендуемый порядок выполнения:
1. **Rule-first**: собрать каркас сюжета без LLM (Польти/Пропп/валидаторы).
2. **LLM-fill**: заполнить семантические поля (`Description`, мотивации, причины/следствия).
3. **Re-validate**: повторная проверка после LLM-заполнения.
4. **Repair**: при ошибке — регенерировать только проблемный бит, а не всю историю.

Это делает систему устойчивой: модель добавляет качество текста, но не ломает структуру.

### 8.4 Что именно не стоит отдавать LLM

Чтобы не потерять управляемость, не стоит полностью делегировать модели:
- выбор недопустимых переходов функций Проппа,
- назначение `Time` и графовых связей `Binder`,
- итоговое решение валидаторов,
- обязательные поля схемы `Params`.

LLM — генератор контента; движок правил — гарантия корректности.
