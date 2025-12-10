using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Model;
using UnityEngine;
using UnityEngine.TextCore.Text;
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
    private Slider _maxTraitsSlider;
    
    /// <summary>
    /// Слайдер для максимального количества фобий.
    /// </summary>
    private Slider _maxPhobiasSlider;
    
    /// <summary>
    /// Текстовое поле для фамилии персонажа.
    /// </summary>
    private TextField _surnameTextField;
    
    /// <summary>
    /// Текстовое поле для возраста персонажа.
    /// </summary>
    private TextField _ageTextField;
    
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
    /// Текстовое поле для значения отношений.
    /// </summary>
    private TextField _relationValueTextField;
    
    /// <summary>
    /// Выпадающий список для выбора режима генерации.
    /// </summary>
    private DropdownField _generationModeDropdown;
    
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
        
        _nameTextField = root.Q<TextField>("NameTextField");
        _maxTraitsSlider = root.Q<Slider>("MaxTraitsSlider");
        _maxPhobiasSlider = root.Q<Slider>("MaxPhobiasSlider");
        _surnameTextField = root.Q<TextField>("SurnameTextField");
        _ageTextField = root.Q<TextField>("AgeTextField");
        _descriptionTextField = root.Q<TextField>("DescriptionTextField");
        _motherDropdown = root.Q<DropdownField>("MotherDropdown");
        _fatherDropdown = root.Q<DropdownField>("FatherDropdown");
        _relationToCharacterDropdown = root.Q<DropdownField>("RelationToCharacterDropdown");
        _relationValueTextField = root.Q<TextField>("RelationValueTextField");
        _generationModeDropdown = root.Q<DropdownField>("GenerationModeDropdown");
        _generateButton = root.Q<Button>("GenerateButton");
        _editGeneratedElement = root.Q<VisualElement>("EditGeneratedElement");
        
        // Инициализация режимов генерации
        _generationModeDropdown.choices = new[] { "Chaotic", "Logic", "ByParentsChaotic", "ByParentsLogic" }.ToList();
        // _generationModeDropdown.value = "Random";
        
        // Обработчик кнопки генерации
        _generateButton.clicked += OnGenerateButtonClicked;
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
        var characters = _plot.Elements
            .Where(e => e.Type == ElemType.Character)
            .Select(e => e.Name)
            .ToList();
        
        _motherDropdown.choices = characters;
        _fatherDropdown.choices = characters;
        _relationToCharacterDropdown.choices = characters;
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки генерации персонажа.
    /// </summary>
    private void OnGenerateButtonClicked()
    {
        string name = string.IsNullOrWhiteSpace(_nameTextField.value) 
            ? "Generated Character" 
            : _nameTextField.value;
        
        int traitsCnt = (int)_maxTraitsSlider.value;
        int phobiasCnt = (int)_maxPhobiasSlider.value;
        
        Debug.Log($"Generating character: {name} with max {traitsCnt} traits and {phobiasCnt} phobias\n" +
            $"Mode: {_generationModeDropdown.value}");
        
        _generatedPrCharacter = new PrCharacter(name);

        if (!string.IsNullOrWhiteSpace(_surnameTextField.value))
        {
            _generatedPrCharacter.Surname = _surnameTextField.value;
        }
        
        if (!string.IsNullOrWhiteSpace(_ageTextField.value) && int.TryParse(_ageTextField.value, out int age))
        {
            _generatedPrCharacter.Age = age;
        }
        
        if (!string.IsNullOrWhiteSpace(_descriptionTextField.value))
        {
            _generatedPrCharacter.Description = _descriptionTextField.value;
        }

        if (!string.IsNullOrWhiteSpace(_fatherDropdown.value)) { _generatedPrCharacter.FatherID = Convert.ToInt32(_fatherDropdown.value); }

        if (!string.IsNullOrWhiteSpace(_motherDropdown.value)) { _generatedPrCharacter.MotherID = Convert.ToInt32(_motherDropdown.value); }

        if (_generationModeDropdown.value == "Chaotic")
        {
            PrGenerator.CreateByChaoticRandomTraits(_generatedPrCharacter, traitsCnt);
        }

        if (_generationModeDropdown.value == "Logic")
        {
            PrGenerator.CreateByLogicRandomTraits(_generatedPrCharacter, traitsCnt);
        }

        if (_generationModeDropdown.value == "ByParentsChaotic")
        {
            if (_generatedPrCharacter.FatherID.HasValue && _generatedPrCharacter.MotherID.HasValue)
            {
                int motherId = _generatedPrCharacter.MotherID.Value;
                int fatherId = _generatedPrCharacter.FatherID.Value;

                if (GlobalData.Characters[motherId] != null && GlobalData.Characters[fatherId] != null)
                {
                    PrCharacter mother = GlobalData.Characters[motherId];
                    PrCharacter father = GlobalData.Characters[fatherId];

                    if (traitsCnt != 0) { PrGenerator.CreateByTwoParentsHalfTraits(_generatedPrCharacter, mother, father, name); }

                    PrGenerator.CreateByTwoParentsHalfRandomTraits(_generatedPrCharacter, traitsCnt, mother, father, name);
                }
            }
        }

        if (_generationModeDropdown.value == "ByParentsLogic")
        {
            if (_generatedPrCharacter.FatherID.HasValue && _generatedPrCharacter.MotherID.HasValue)
            {
                int motherId = _generatedPrCharacter.MotherID.Value;
                int fatherId = _generatedPrCharacter.FatherID.Value;

                if (GlobalData.Characters[motherId] != null && GlobalData.Characters[fatherId] != null)
                {
                    PrCharacter mother = GlobalData.Characters[motherId];
                    PrCharacter father = GlobalData.Characters[fatherId];

                    if (traitsCnt != 0) { PrGenerator.CreateByTwoParentsLogicTraits(_generatedPrCharacter, mother, father, name); }

                    PrGenerator.CreateByTwoParentsLogicRandomTraits(_generatedPrCharacter, traitsCnt, mother, father, name);
                }
            }
        }

        // TODO: input? anchor?

        if (phobiasCnt != 0) { PrGenerator.CreateByChaoticRandomPhobias(_generatedPrCharacter, phobiasCnt); }

        // Отображаем сгенерированного персонажа в редакторе
        _generatedCharacter = PrGenerator.Translate(_generatedPrCharacter);
        _editSelectedElementController.SelectedElement = _generatedCharacter;

        _plot.Add(_generatedCharacter);

        Debug.Log($"Character generated: {_generatedCharacter.Name}");
    }
}
