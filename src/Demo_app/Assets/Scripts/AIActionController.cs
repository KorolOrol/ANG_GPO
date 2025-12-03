using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIGenerator;
using AIGenerator.TextGenerator;
using BaseClasses.Enum;
using BaseClasses.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Скрипт для действия с ИИ в пользовательском интерфейсе.
/// </summary>
public class AIActionController : IActionController
{
    /// <summary>
    /// Ссылка на текущий сюжет.
    /// </summary>
    private Plot _plot;
    
    /// <summary>
    /// Выпадающий список для выбора текстового генератора.
    /// </summary>
    private DropdownField _selectTextGeneratorDropdown;
    
    /// <summary>
    /// Текстовое поле для имени текстового генератора.
    /// </summary>
    private TextField _nameTextGeneratorTextField;
    
    /// <summary>
    /// Текстовое поле для конечной точки текстового генератора.
    /// </summary>
    private TextField _endpointTextGeneratorTextField;
    
    /// <summary>
    /// Радиокнопка для выбора публичного API ключа текстового генератора.
    /// </summary>
    private RadioButton _publicApiKeyTextGeneratorRadioButton;
    
    /// <summary>
    /// Текстовое поле для публичного API ключа текстового генератора.
    /// </summary>
    private TextField _publicApiKeyTextGeneratorTextField;
    
    /// <summary>
    /// Радиокнопка для выбора переменной окружения API ключа текстового генератора.
    /// </summary>
    private RadioButton _envApiKeyTextGeneratorRadioButton;
    
    /// <summary>
    /// Текстовое поле для переменной окружения API ключа текстового генератора.
    /// </summary>
    private TextField _envApiKeyTextGeneratorTextField;
    
    /// <summary>
    /// Текстовое поле для модели текстового генератора.
    /// </summary>
    private TextField _modelTextGeneratorTextField;
    
    /// <summary>
    /// Переключатель для структурированного вывода текстового генератора.
    /// </summary>
    private Toggle _structuredOutputTextGeneratorToggle;
    
    /// <summary>
    /// Кнопка для сохранения текстового генератора.
    /// </summary>
    private Button _saveTextGeneratorButton;
    
    /// <summary>
    /// Кнопка для удаления текстового генератора.
    /// </summary>
    private Button _deleteTextGeneratorButton;
    
    /// <summary>
    /// Кнопка для выбора файла системного запроса.
    /// </summary>
    private Button _selectSystemPromptButton;
    
    /// <summary>
    /// Метка для отображения выбранного пути системного запроса.
    /// </summary>
    private Label _selectedSystemPromptPathLabel;
    
    /// <summary>
    /// Переключатель для приоритета ИИ.
    /// </summary>
    private Toggle _aiPriorityToggle;
    
    /// <summary>
    /// Радиокнопка для генерации на основе нового элемента.
    /// </summary>
    private RadioButton _basedOnNewElementGenerationRadioButton;
    
    /// <summary>
    /// Выпадающий список для выбора типа нового элемента.
    /// </summary>
    private DropdownField _newElementTypeDropdown;
    
    /// <summary>
    /// Радиокнопка для генерации на основе существующего элемента.
    /// </summary>
    private RadioButton _basedOnExistingElementGenerationRadioButton;
    
    /// <summary>
    /// Выпадающий список для выбора существующего элемента.
    /// </summary>
    private DropdownField _existingElementDropdown;
    
    /// <summary>
    /// Кнопка для запуска генерации ИИ.
    /// </summary>
    private Button _aiGenerateButton;

    /// <summary>
    /// Метка для отображения состояния генерации.
    /// </summary>
    private Label _generatingLabel;
    
    /// <summary>
    /// Элемент визуального интерфейса для редактирования сгенерированного элемента.
    /// </summary>
    private VisualElement _editGeneratedElement;
    
    /// <summary>
    /// Контроллер для редактирования выбранного элемента.
    /// </summary>
    private EditSelectedElementController _editSelectedElementController;

    /// <summary>
    /// Словарь текстовых генераторов.
    /// </summary>
    private readonly Dictionary<string, OpenAIGenerator> _textGenerators = new Dictionary<string, OpenAIGenerator>();
    
    /// <summary>
    /// Генератор ИИ.
    /// </summary>
    private LlmAiGenerator _aiGenerator;
    
    /// <summary>
    /// Путь к выбранному файлу системного запроса.
    /// </summary>
    private string _selectedSystemPromptPath;
    
    /// <summary>
    /// Сгенерированный элемент.
    /// </summary>
    private Element _generatedElement;
    
    /// <summary>
    /// Флаг инициализации визуальных элементов.
    /// </summary>
    private bool _isVisualElementsInitiated;

    /// <summary>
    /// Инициализация скрипта действия с ИИ.
    /// </summary>
    /// <param name="root">Корневой элемент визуального интерфейса.</param>
    /// <param name="plot">Текущий сюжет.</param>
    public void Initiate(VisualElement root, Plot plot)
    {
        InitiateVisualElements(root);
        _plot = plot;
        _editSelectedElementController = new EditSelectedElementController(_editGeneratedElement, plot);
        
        _existingElementDropdown.choices = _plot.Elements.Select(e => e.ToString()).ToList();
    }
    
    /// <summary>
    /// Инициализация визуальных элементов UI и их действий.
    /// </summary>
    /// <param name="root">Корневой элемент визуального интерфейса.</param>
    private void InitiateVisualElements(VisualElement root)
    {
        if (_isVisualElementsInitiated) return;
        _isVisualElementsInitiated = true;
        
        _selectTextGeneratorDropdown = root.Q<DropdownField>("SelectTextGeneratorDropdown");
        _nameTextGeneratorTextField = root.Q<TextField>("NameTextGeneratorTextField");
        _endpointTextGeneratorTextField = root.Q<TextField>("EndpointTextGeneratorTextField");
        root.Q<RadioButtonGroup>("ApiKeyTextGeneratorRadioButtonGroup");
        _publicApiKeyTextGeneratorRadioButton = root.Q<RadioButton>("PublicApiKeyTextGeneratorRadioButton");
        _publicApiKeyTextGeneratorTextField = root.Q<TextField>("PublicApiKeyTextGeneratorTextField");
        _envApiKeyTextGeneratorRadioButton = root.Q<RadioButton>("EnvApiKeyTextGeneratorRadioButton");
        _envApiKeyTextGeneratorTextField = root.Q<TextField>("EnvApiKeyTextGeneratorTextField");
        _modelTextGeneratorTextField = root.Q<TextField>("ModelTextGeneratorTextField");
        _structuredOutputTextGeneratorToggle = root.Q<Toggle>("StructuredOutputTextGeneratorToggle");
        _saveTextGeneratorButton = root.Q<Button>("SaveTextGeneratorButton");
        _deleteTextGeneratorButton = root.Q<Button>("DeleteTextGeneratorButton");
        _selectSystemPromptButton = root.Q<Button>("SelectSystemPromptButton");
        _selectedSystemPromptPathLabel = root.Q<Label>("SelectedSystemPromptPathLabel");
        _aiPriorityToggle = root.Q<Toggle>("AIPriorityToggle");
        root.Q<RadioButtonGroup>("BasedOnGenerationRadioButtonGroup");
        _basedOnNewElementGenerationRadioButton = root.Q<RadioButton>("BasedOnNewElementGenerationRadioButton");
        _newElementTypeDropdown = root.Q<DropdownField>("NewElementTypeDropdown");
        _basedOnExistingElementGenerationRadioButton =
            root.Q<RadioButton>("BasedOnExistingElementGenerationRadioButton");
        _existingElementDropdown = root.Q<DropdownField>("ExistingElementDropdown");
        _aiGenerateButton = root.Q<Button>("AIGenerateButton");
        _generatingLabel = root.Q<Label>("GeneratingLabel");
        _editGeneratedElement = root.Q<VisualElement>("EditGeneratedElement");
        
        _selectTextGeneratorDropdown.choices = new List<string>(_textGenerators.Keys).Append("New").ToList();
        _selectTextGeneratorDropdown.RegisterValueChangedCallback(SelectTextGeneratorDropdownOnChange);
        
        _publicApiKeyTextGeneratorRadioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (!evt.newValue) return;
            _publicApiKeyTextGeneratorTextField.SetEnabled(true);
            _envApiKeyTextGeneratorTextField.SetEnabled(false);
        });
        _envApiKeyTextGeneratorRadioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (!evt.newValue) return;
            _publicApiKeyTextGeneratorTextField.SetEnabled(false);
            _envApiKeyTextGeneratorTextField.SetEnabled(true);
        });
        
        _saveTextGeneratorButton.clicked += OnSaveTextGeneratorButtonOnClicked;
        _deleteTextGeneratorButton.clicked += OnDeleteTextGeneratorButtonOnClicked;
        
        _selectSystemPromptButton.clicked += () =>
        {
            string path = EditorUtility.OpenFilePanel("Select System Prompt File", "", "");
            if (string.IsNullOrWhiteSpace(path)) return;
            _selectedSystemPromptPath = path;
            _selectedSystemPromptPathLabel.text = "System Prompt: " + path;
        };
        
        _basedOnNewElementGenerationRadioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (!evt.newValue) return;
            _newElementTypeDropdown.SetEnabled(true);
            _existingElementDropdown.SetEnabled(false);
        });
        _basedOnExistingElementGenerationRadioButton.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (!evt.newValue) return;
            _newElementTypeDropdown.SetEnabled(false);
            _existingElementDropdown.SetEnabled(true);
        });
        
        _newElementTypeDropdown.choices = Enum.GetNames(typeof(ElemType)).ToList();
        
        _aiGenerateButton.clicked += async () => await OnAIGenerateButtonOnClicked();
    }

    /// <summary>
    /// Получение действия для обновления списка существующих элементов.
    /// </summary>
    /// <returns>Действие обновления списка.</returns>
    public Action GetUpdateAction()
    {
        return () =>
        {
            _existingElementDropdown.choices = _plot.Elements.Select(e => e.ToString()).ToList();
        };
    }

    /// <summary>
    /// Обработчик нажатия кнопки сохранения текстового генератора.
    /// </summary>
    private void OnSaveTextGeneratorButtonOnClicked()
    {
        if (_selectTextGeneratorDropdown.value == "New")
        {
            string endpoint = _endpointTextGeneratorTextField.value;
            string model = _modelTextGeneratorTextField.value;
            bool useStructuredOutput = _structuredOutputTextGeneratorToggle.value;
            string name = !string.IsNullOrWhiteSpace(_nameTextGeneratorTextField.value)
                ? _nameTextGeneratorTextField.value
                : "New " + endpoint + " " + model + (useStructuredOutput ? " Structured" : " Unstructured");
            OpenAIGenerator generator = null;
            if (_publicApiKeyTextGeneratorRadioButton.value)
            {
                generator = new OpenAIGenerator { Endpoint = endpoint, ApiKey = _publicApiKeyTextGeneratorTextField.value, Model = model, UseStructuredOutput = useStructuredOutput };
            }
            else if (_envApiKeyTextGeneratorRadioButton.value)
            {
                generator = new OpenAIGenerator(_envApiKeyTextGeneratorTextField.value, endpoint) { Model = model, UseStructuredOutput = useStructuredOutput };
            }
            _textGenerators.Add(name, generator);
            _selectTextGeneratorDropdown.choices.Add(name);
            _selectTextGeneratorDropdown.value = name;
        }
        else
        {
            var generator = _textGenerators[_selectTextGeneratorDropdown.value];
            generator.Endpoint = _endpointTextGeneratorTextField.value;
            generator.Model = _modelTextGeneratorTextField.value;
            generator.UseStructuredOutput = _structuredOutputTextGeneratorToggle.value;
            switch (_publicApiKeyTextGeneratorRadioButton.value)
            {
                case false when !_envApiKeyTextGeneratorRadioButton.value:
                    return;
                case true:
                    generator.ApiKey = _publicApiKeyTextGeneratorTextField.value;
                    break;
                default:
                    {
                        if (_envApiKeyTextGeneratorRadioButton.value)
                        {
                            generator = new OpenAIGenerator(_envApiKeyTextGeneratorTextField.value, 
                                generator.Endpoint)
                            {
                                Model = generator.Model, 
                                UseStructuredOutput = generator.UseStructuredOutput
                            };
                            _textGenerators[_selectTextGeneratorDropdown.value] = generator;
                        }
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Обработчик нажатия кнопки удаления текстового генератора.
    /// </summary>
    private void OnDeleteTextGeneratorButtonOnClicked()
    {
        if (_selectTextGeneratorDropdown.value == "New") return;
        _textGenerators.Remove(_selectTextGeneratorDropdown.value);
        _selectTextGeneratorDropdown.choices.Remove(_selectTextGeneratorDropdown.value);
        _selectTextGeneratorDropdown.value = "New";
    }

    /// <summary>
    /// Обработчик изменения выбора в выпадающем списке текстового генератора.
    /// </summary>
    /// <param name="evt">Событие изменения.</param>
    private void SelectTextGeneratorDropdownOnChange(ChangeEvent<string> evt)
    {
        if (evt.newValue == "New")
        {
            _nameTextGeneratorTextField.value = "";
            _endpointTextGeneratorTextField.value = "";
            _publicApiKeyTextGeneratorTextField.value = "";
            _envApiKeyTextGeneratorTextField.value = "";
            _modelTextGeneratorTextField.value = "";
            _structuredOutputTextGeneratorToggle.value = false;
        }
        else
        {
            var generator = _textGenerators[evt.newValue];
            _nameTextGeneratorTextField.value = evt.newValue;
            _endpointTextGeneratorTextField.value = generator.Endpoint;
            _publicApiKeyTextGeneratorRadioButton.value = false;
            _publicApiKeyTextGeneratorTextField.value = "";
            _envApiKeyTextGeneratorRadioButton.value = false;
            _envApiKeyTextGeneratorTextField.value = "";
            _modelTextGeneratorTextField.value = generator.Model;
            _structuredOutputTextGeneratorToggle.value = generator.UseStructuredOutput;
        }
    }

    /// <summary>
    /// Обработчик нажатия кнопки генерации ИИ.
    /// </summary>
    async private Task OnAIGenerateButtonOnClicked()
    {
        if (_selectTextGeneratorDropdown.value == "New")
        {
            Debug.LogError("Select or create a text generator first.");
            return;
        }
        if (string.IsNullOrEmpty(_selectedSystemPromptPath))
        {
            Debug.LogError("Select a system prompt file first.");
            return;
        }
        
        _aiGenerator ??= new LlmAiGenerator(_selectedSystemPromptPath);
        
        _aiGenerator.TextAiGenerator = _textGenerators[_selectTextGeneratorDropdown.value];
        _aiGenerator.UseStructuredOutput = _structuredOutputTextGeneratorToggle.value;
        _aiGenerator.AIPriority = _aiPriorityToggle.value;

        Element baseElement = null;
        if (_basedOnNewElementGenerationRadioButton.value)
        {
            var type = (ElemType)Enum.Parse(typeof(ElemType), _newElementTypeDropdown.value);
            baseElement = FullElementConstructor.CreateFullElement(type);
        }
        else if (_basedOnExistingElementGenerationRadioButton.value)
        {
            baseElement = (Element)_plot.Elements
                .FirstOrDefault(e => e.ToString() == _existingElementDropdown.value);
            if (baseElement == null)
            {
                Debug.LogError("Selected existing element not found in the plot.");
                return;
            }
        }

        if (baseElement == null)
        {
            Debug.LogError("No base element selected for AI generation.");
            return;
        }

        _generatingLabel.style.display = DisplayStyle.Flex;
        _generatedElement = (Element)await _aiGenerator.GenerateAsync(_plot, baseElement);
        _generatingLabel.style.display = DisplayStyle.None;
        _editSelectedElementController.SelectedElement = _generatedElement;
    }
}
