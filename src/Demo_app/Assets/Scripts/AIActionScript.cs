using System.Collections.Generic;
using System.Linq;
using AIGenerator;
using AIGenerator.TextGenerator;
using BaseClasses.Model;
using UnityEditor;
using UnityEngine.UIElements;

public static class AIActionScript
{
    private static Plot _plot;
    private static DropdownField _selectTextGeneratorDropdown;
    private static TextField _nameTextGeneratorTextField;
    private static TextField _endpointTextGeneratorTextField;
    private static RadioButtonGroup _apiKeyTextGeneratorRadioButtonGroup;
    private static RadioButton _publicApiKeyTextGeneratorRadioButton;
    private static TextField _publicApiKeyTextGeneratorTextField;
    private static RadioButton _envApiKeyTextGeneratorRadioButton;
    private static TextField _envApiKeyTextGeneratorTextField;
    private static TextField _modelTextGeneratorTextField;
    private static Toggle _structuredOutputTextGeneratorToggle;
    private static Button _saveTextGeneratorButton;
    private static Button _deleteTextGeneratorButton;
    private static Button _selectSystemPromptButton;
    private static Label _selectedSystemPromptPathLabel;
    private static Toggle _aiPriorityToggle;
    private static RadioButtonGroup _basedOnGenerationRadioButtonGroup;
    private static RadioButton _basedOnNewElementGenerationRadioButton;
    private static DropdownField _newElementTypeDropdown;
    private static RadioButton _basedOnExistingElementGenerationRadioButton;
    private static DropdownField _existingElementDropdown;
    private static Button _aiGenerateButton;
    private static VisualElement _editGeneratedElement;
    
    /// <summary>
    /// Поле типа выбранного элемента
    /// </summary>
    private static TextField _typeTextField;
    
    /// <summary>
    /// Поле имени выбранного элемента
    /// </summary>
    private static TextField _nameTextField;
    
    /// <summary>
    /// Поле описания выбранного элемента
    /// </summary>
    private static TextField _descriptionTextField;
    
    /// <summary>
    /// Складной элемент для параметров выбранного элемента
    /// </summary>
    private static Foldout _paramsFoldout;
    
    /// <summary>
    /// Поле времени выбранного элемента
    /// </summary>
    private static TextField _timeTextField;
    
    /// <summary>
    /// Кнопка обновления выбранного элемента
    /// </summary>
    private static Button _updateElementButton;
    
    private readonly static Dictionary<string, OpenAIGenerator> TextGenerators = new Dictionary<string, OpenAIGenerator>();
    private static LlmAiGenerator _aiGenerator;
    private static string _selectedSystemPromptPath;
    private static Element _generatedElement;
    
    public static void Initiate(VisualElement root, Plot plot)
    {
        _plot = plot;
        _selectTextGeneratorDropdown = root.Q<DropdownField>("SelectTextGeneratorDropdown");
        _nameTextGeneratorTextField = root.Q<TextField>("NameTextGeneratorTextField");
        _endpointTextGeneratorTextField = root.Q<TextField>("EndpointTextGeneratorTextField");
        _apiKeyTextGeneratorRadioButtonGroup = root.Q<RadioButtonGroup>("ApiKeyTextGeneratorRadioButtonGroup");
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
        _basedOnGenerationRadioButtonGroup = root.Q<RadioButtonGroup>("BasedOnGenerationRadioButtonGroup");
        _basedOnNewElementGenerationRadioButton = root.Q<RadioButton>("BasedOnNewElementGenerationRadioButton");
        _newElementTypeDropdown = root.Q<DropdownField>("NewElementTypeDropdown");
        _basedOnExistingElementGenerationRadioButton = 
            root.Q<RadioButton>("BasedOnExistingElementGenerationRadioButton");
        _existingElementDropdown = root.Q<DropdownField>("ExistingElementDropdown");
        _aiGenerateButton = root.Q<Button>("AIGenerateButton");
        _editGeneratedElement = root.Q<VisualElement>("EditGeneratedElement");
        _typeTextField = _editGeneratedElement.Q<TextField>("TypeTextField");
        _nameTextField = _editGeneratedElement.Q<TextField>("NameTextField");
        _descriptionTextField = _editGeneratedElement.Q<TextField>("DescriptionTextField");
        _paramsFoldout = _editGeneratedElement.Q<Foldout>("ParamsFoldout");
        _timeTextField = _editGeneratedElement.Q<TextField>("TimeTextField");
        _updateElementButton = _editGeneratedElement.Q<Button>("UpdateElementButton");
        
        _selectTextGeneratorDropdown.choices = new List<string>(TextGenerators.Keys).Append("New").ToList();
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
    }

    private static void OnSaveTextGeneratorButtonOnClicked()
    {
        if (_selectTextGeneratorDropdown.value == "New")
        {
            string endpoint = _endpointTextGeneratorTextField.value;
            string model = _modelTextGeneratorTextField.value;
            bool useStructuredOutput = _structuredOutputTextGeneratorToggle.value;
            string name = string.IsNullOrWhiteSpace(_nameTextGeneratorTextField.value)
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
            TextGenerators.Add(name, generator);
            _selectTextGeneratorDropdown.choices.Add(name);
        }
        else
        {
            var generator = TextGenerators[_selectTextGeneratorDropdown.value];
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
                            generator = new OpenAIGenerator(_envApiKeyTextGeneratorTextField.value, generator.Endpoint) { Model = generator.Model, UseStructuredOutput = generator.UseStructuredOutput };
                            TextGenerators[_selectTextGeneratorDropdown.value] = generator;
                        }
                        break;
                    }
            }
        }
    }

    private static void OnDeleteTextGeneratorButtonOnClicked()
    {
        if (_selectTextGeneratorDropdown.value == "New") return;
        TextGenerators.Remove(_selectTextGeneratorDropdown.value);
        _selectTextGeneratorDropdown.choices.Remove(_selectTextGeneratorDropdown.value);
        _selectTextGeneratorDropdown.value = "New";
    }

    private static void SelectTextGeneratorDropdownOnChange(ChangeEvent<string> evt)
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
            var generator = TextGenerators[evt.newValue];
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
}
