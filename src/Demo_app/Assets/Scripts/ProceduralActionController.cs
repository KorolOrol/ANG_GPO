using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Model;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Контроллер для процедурной генерации персонажей.
/// </summary>
public class ProceduralActionController : IActionController
{
    /// <summary>
    /// Ссылка на текущий сюжет.
    /// </summary>
    private Plot _plot;
    
    /// <summary>
    /// Текстовое поле для имени персонажа.
    /// </summary>
    private TextField _nameTextField;
    
    /// <summary>
    /// Слайдер для максимального количества черт характера.
    /// </summary>
    private SliderInt _maxTraitsSlider;
    
    /// <summary>
    /// Слайдер для максимального количества фобий.
    /// </summary>
    private SliderInt _maxPhobiasSlider;
    
    /// <summary>
    /// Текстовое поле для фамилии персонажа.
    /// </summary>
    private TextField _surnameTextField;
    
    /// <summary>
    /// Текстовое поле для возраста персонажа.
    /// </summary>
    private TextField _ageTextField;

    /// <summary>
    /// Выпадающий список для выбора пола персонажа.
    /// </summary>
    private DropdownField _genderDropdown;

    /// <summary>
    /// Текстовое поле для описания персонажа.
    /// </summary>
    private TextField _descriptionTextField;
    
    /// <summary>
    /// Выпадающий список для выбора матери персонажа.
    /// </summary>
    private DropdownField _motherDropdown;
    
    /// <summary>
    /// Выпадающий список для выбора отца персонажа.
    /// </summary>
    private DropdownField _fatherDropdown;
    
    /// <summary>
    /// Выпадающий список для выбора персонажа для связи.
    /// </summary>
    private DropdownField _relationToCharacterDropdown;

    /// <summary>
    /// Контейнер для всех Toggles отношений
    /// </summary>
    private VisualElement _relationTogglesContainer;

    /// <summary>
    /// Состояния Toggles для каждого персонажа
    /// </summary>
    private readonly Dictionary<string, bool> _relationToggleStates = new Dictionary<string, bool>();

    /// <summary>
    /// Выпадающий список для выбора режима генерации.
    /// </summary>
    private DropdownField _generationModeDropdown;

    /// <summary>
    /// Текстовое поля для входящих черт
    /// </summary>
    private TextField _inputTraitOrTraits;

    /// <summary>
    /// Кнопка для запуска генерации персонажа.
    /// </summary>
    private Button _generateButton;
    
    /// <summary>
    /// Элемент визуального интерфейса для редактирования сгенерированного персонажа.
    /// </summary>
    private VisualElement _editGeneratedElement;
    
    /// <summary>
    /// Контроллер для редактирования выбранного элемента.
    /// </summary>
    private EditSelectedElementController _editSelectedElementController;
    
    /// <summary>
    /// Сгенерированный персонаж.
    /// </summary>
    private Element _generatedCharacter;

    /// <summary>
    /// Сгенерированный персонаж.
    /// </summary>
    private PrCharacter _generatedPrCharacter;

    /// <summary>
    /// Флаг инициализации визуальных элементов.
    /// </summary>
    private bool _isVisualElementsInitiated;

    /// <summary>
    /// Инициализация контроллера процедурной генерации.
    /// </summary>
    /// <param name="root">Корневой элемент визуального интерфейса.</param>
    /// <param name="plot">Текущий сюжет.</param>
    public void Initiate(VisualElement root, Plot plot)
    {
        InitiateVisualElements(root);
        _plot = plot;
        _editSelectedElementController = new EditSelectedElementController(_editGeneratedElement, _plot);
        
        UpdateCharacterDropdowns();
    }
    
    /// <summary>
    /// Инициализация визуальных элементов UI и их действий.
    /// </summary>
    /// <param name="root">Корневой элемент визуального интерфейса.</param>
    private void InitiateVisualElements(VisualElement root)
    {
        if (_isVisualElementsInitiated) return;
        _isVisualElementsInitiated = true;
        
        _nameTextField = root.Q<TextField>("PrNameTextField");
        _maxTraitsSlider = root.Q<SliderInt>("MaxTraitsSlider");
        _maxPhobiasSlider = root.Q<SliderInt>("MaxPhobiasSlider");

        _surnameTextField = root.Q<TextField>("SurnameTextField");
        _ageTextField = root.Q<TextField>("AgeTextField");
        _genderDropdown = root.Q<DropdownField>("GenderDropdown");

        _descriptionTextField = root.Q<TextField>("PrDescriptionTextField");

        _motherDropdown = root.Q<DropdownField>("MotherDropdown");
        _fatherDropdown = root.Q<DropdownField>("FatherDropdown");

        _relationToCharacterDropdown = root.Q<DropdownField>("RelationToCharacterDropdown");
        _relationTogglesContainer = root.Q<VisualElement>("RelationTogglesContainer");

        _generationModeDropdown = root.Q<DropdownField>("GenerationModeDropdown");

        _generateButton = root.Q<Button>("GenerateButton");
        _inputTraitOrTraits = root.Q<TextField>("InputTraitOrTraits");
        _inputTraitOrTraits.style.display = DisplayStyle.None;

        _editGeneratedElement = root.Q<VisualElement>("EditGeneratedElement");
        
        // Инициализация режимов генерации
        _generationModeDropdown.choices = new[] { " ", "Chaotic", "Logic", "ByParentsChaotic", "ByParentsLogic", "ByInputTrait", "ByInputTraits" }.ToList();
        _genderDropdown.choices = new[] { " ", "Male", "Female" }.ToList();

        _generationModeDropdown.RegisterValueChangedCallback(OnGenerationModeChanged);
        _relationToCharacterDropdown.RegisterValueChangedCallback(OnRelationDropdownChanged);

        // Обработчик кнопки генерации
        _generateButton.clicked += OnGenerateButtonClicked;
    }

    /// <summary>
    /// Создает Toggles для всех доступных персонажей
    /// </summary>
    private void CreateRelationToggles()
    {
        _relationTogglesContainer.Clear();

        List<string> characters = _plot.Elements
            .Where(e => e.Type == ElemType.Character)
            .Select(e => e.Name)
            .ToList();

        foreach (string characterName in characters)
        {
            var toggle = new Toggle
            {
                text = $"Generate relation to {characterName}",
                name = $"RelationToggle_{characterName}",
                value = _relationToggleStates.GetValueOrDefault(characterName, false)
            };

            toggle.RegisterValueChangedCallback(evt =>
            {
                _relationToggleStates[characterName] = evt.newValue;
            });

            _relationTogglesContainer.Add(toggle);
        }

        UpdateVisibleToggle();
    }

    /// <summary>
    /// Показывает только Toggle для выбранного персонажа
    /// </summary>
    private void UpdateVisibleToggle()
    {
        foreach (var toggle in _relationTogglesContainer.Children().OfType<Toggle>())
        {
            toggle.style.display = toggle.name == $"RelationToggle_{_relationToCharacterDropdown.value}"
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    /// <summary>
    /// При изменении режима генерации появлятся или скрывается строка для ввода черт характера
    /// </summary>
    /// <param name="evt"></param>
    private void OnGenerationModeChanged(ChangeEvent<string> evt)
    {
        _inputTraitOrTraits.style.display = 
            evt.newValue is "ByInputTrait" or "ByInputTraits" ? 
                DisplayStyle.Flex : 
                DisplayStyle.None;
    }

    /// <summary>
    /// При изменении персонажа в RelationDropdown меняет Toggle под него
    /// </summary>
    /// <param name="evt"></param>
    private void OnRelationDropdownChanged(ChangeEvent<string> evt)
    {
        UpdateVisibleToggle();
    }

    /// <summary>
    /// Получение действия для обновления выпадающих списков персонажей.
    /// </summary>
    /// <returns>Действие обновления списков.</returns>
    public Action GetUpdateAction()
    {
        return UpdateCharacterDropdowns;
    }
    
    /// <summary>
    /// Обновление выпадающих списков с персонажами.
    /// </summary>
    private void UpdateCharacterDropdowns()
    {
        List<string> characters = _plot.Elements
            .Where(e => e.Type == ElemType.Character)
            .Select(e => e.Name)
            .ToList();
        
        _motherDropdown.choices = characters;
        _fatherDropdown.choices = characters;
        _relationToCharacterDropdown.choices = characters;

        if (_relationTogglesContainer.childCount != characters.Count)
        {
            CreateRelationToggles();
        }
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки генерации персонажа.
    /// </summary>
    private void OnGenerateButtonClicked()
    {
        string name = string.IsNullOrWhiteSpace(_nameTextField.value) 
            ? "Generated Character" 
            : _nameTextField.value;
        
        int traitsCnt = _maxTraitsSlider.value;
        int phobiasCnt = _maxPhobiasSlider.value;
        
        Debug.Log($"Generating character: {name} with max {traitsCnt} traits and {phobiasCnt} phobias\n" +
            $"Mode: {_generationModeDropdown.value}");
        
        _generatedPrCharacter = new PrCharacter(name);

        if (!string.IsNullOrWhiteSpace(_surnameTextField.value)) 
            _generatedPrCharacter.Surname = _surnameTextField.value;

        if (!string.IsNullOrWhiteSpace(_genderDropdown.value)) 
            _generatedPrCharacter.Gender = _genderDropdown.value == "Male";
        
        if (!string.IsNullOrWhiteSpace(_ageTextField.value) && int.TryParse(_ageTextField.value, out int age))
            _generatedPrCharacter.Age = age;
        
        if (!string.IsNullOrWhiteSpace(_descriptionTextField.value)) _generatedPrCharacter.Description = _descriptionTextField.value;

        if (!string.IsNullOrWhiteSpace(_fatherDropdown.value))
        {
            var foundCharacter = GlobalData.Characters.FirstOrDefault(p =>
                p.Name != null && p.Name.Contains(_fatherDropdown.value.Remove(_fatherDropdown.value.Length - 1)));

            if (foundCharacter != null)
            {
                _generatedPrCharacter.FatherID = foundCharacter.ID;
            }
        }

        if (!string.IsNullOrWhiteSpace(_motherDropdown.value))
        {
            var foundCharacter = GlobalData.Characters.FirstOrDefault(p =>
                p.Name != null && p.Name.Contains(_motherDropdown.value.Remove(_motherDropdown.value.Length - 1)));

            if (foundCharacter != null) _generatedPrCharacter.MotherID = foundCharacter.ID;
        }

        if (traitsCnt != 0)
        {
            switch (_generationModeDropdown.value)
            {
                case "Chaotic":
                    PrGenerator.CreateByChaoticRandomTraits(_generatedPrCharacter, traitsCnt);
                    break;
                case "Logic":
                    PrGenerator.CreateByLogicRandomTraits(_generatedPrCharacter, traitsCnt);
                    break;
                case "ByInputTrait":
                    {
                        string[] words = _inputTraitOrTraits.value.Split(new[]
                            {
                                ' ',
                                '\t',
                                '\n',
                                '\r'
                            },
                            StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length == 1)
                        {
                            PrGenerator.CreateByInputTrait(_generatedPrCharacter,
                                _inputTraitOrTraits.value,
                                traitsCnt);
                        }
                        break;
                    }
                case "ByInputTraits":
                    {
                        List<string> traitsList = _inputTraitOrTraits.value.Split(new[]
                                { ',' },
                                StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .ToList();
                        if (traitsCnt >= traitsList.Count)
                        {
                            PrGenerator.CreateByInputTraits(_generatedPrCharacter, traitsList, traitsCnt);
                        }
                        break;
                    }
                case "ByParentsChaotic":
                    {
                        if (_generatedPrCharacter.FatherID.HasValue && _generatedPrCharacter.MotherID.HasValue)
                        {
                            int motherId = _generatedPrCharacter.MotherID.Value;
                            int fatherId = _generatedPrCharacter.FatherID.Value;

                            var mother = GlobalData.Characters[motherId];
                            var father = GlobalData.Characters[fatherId];

                            PrGenerator.CreateByTwoParentsHalfRandomTraits(_generatedPrCharacter,
                                traitsCnt,
                                mother,
                                father,
                                name);
                        }
                        break;
                    }
                case "ByParentsLogic":
                    {
                        if (_generatedPrCharacter.FatherID.HasValue && _generatedPrCharacter.MotherID.HasValue)
                        {
                            int motherId = _generatedPrCharacter.MotherID.Value;
                            int fatherId = _generatedPrCharacter.FatherID.Value;

                            var mother = GlobalData.Characters[motherId];
                            var father = GlobalData.Characters[fatherId];

                            PrGenerator.CreateByTwoParentsLogicRandomTraits(_generatedPrCharacter,
                                traitsCnt,
                                mother,
                                father,
                                name);
                        }
                        break;
                    }
            }

        }

        if (phobiasCnt != 0) { PrGenerator.CreateByChaoticRandomPhobias(_generatedPrCharacter, phobiasCnt); }

        if (!string.IsNullOrWhiteSpace(_relationToCharacterDropdown.value))
        {
            bool shouldGenerateRelation = _relationToggleStates.ContainsKey(_relationToCharacterDropdown.value) &&
                _relationToggleStates[_relationToCharacterDropdown.value];

            foreach (var foundCharacter in _relationToggleStates.Where(relation => relation.Value)
                .Select(relation => GlobalData.Characters.FirstOrDefault(p =>
                    p.Name != null && p.Name.Contains(relation.Key.Remove(relation.Key.Length - 1))))
                .Where(foundCharacter => foundCharacter != null))
            {
                PrGenerator.GetRelations(_generatedPrCharacter, foundCharacter);
            }
        }

        // Отображаем сгенерированного персонажа в редакторе
        _generatedCharacter = PrGenerator.Translate(_generatedPrCharacter);
        _editSelectedElementController.SelectedElement = _generatedCharacter;

        _plot.Add(_generatedCharacter);

        Debug.Log($"Character generated: {_generatedCharacter.Name}");
    }
}
