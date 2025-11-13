using System.Collections.Generic;
using AIGenerator;
using AIGenerator.TextGenerator;
using BaseClasses.Model;
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
    
    private static List<OpenAIGenerator> _textGenerators = new List<OpenAIGenerator>();
    private static LlmAiGenerator _aiGenerator;
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
        _selectSystemPromptButton = root.Q<Button>("SelectSystemPromptButton");
        _selectedSystemPromptPathLabel = root.Q<Label>("SelectedSystemPromptPathLabel");
        _aiPriorityToggle = root.Q<Toggle>("AIPriorityToggle");
        _basedOnGenerationRadioButtonGroup = root.Q<RadioButtonGroup>("BasedOnGenerationRadioButtonGroup");
        _basedOnNewElementGenerationRadioButton = root.Q<RadioButton>("BasedOnNewElementGenerationRadioButton");
        _newElementTypeDropdown = root.Q<DropdownField>("NewElementTypeDropdown");
        _basedOnExistingElementGenerationRadioButton = root.Q<RadioButton>("BasedOnExistingElementGenerationRadioButton");
        _existingElementDropdown = root.Q<DropdownField>("ExistingElementDropdown");
        _aiGenerateButton = root.Q<Button>("AIGenerateButton");
        _editGeneratedElement = root.Q<VisualElement>("EditGeneratedElement");
        _typeTextField = _editGeneratedElement.Q<TextField>("TypeTextField");
        _nameTextField = _editGeneratedElement.Q<TextField>("NameTextField");
        _descriptionTextField = _editGeneratedElement.Q<TextField>("DescriptionTextField");
        _paramsFoldout = _editGeneratedElement.Q<Foldout>("ParamsFoldout");
        _timeTextField = _editGeneratedElement.Q<TextField>("TimeTextField");
        _updateElementButton = _editGeneratedElement.Q<Button>("UpdateElementButton");
    }
}
