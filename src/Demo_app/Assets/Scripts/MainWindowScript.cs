using System;
using System.Collections.Generic;
using BaseClasses.Model;
using BaseClasses.Services;
using DataBase;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

/// <summary>
/// Скрипт главного окна приложения.
/// </summary>
public class MainWindowScript : MonoBehaviour
{
    /// <summary>
    /// Корневой элемент UI.
    /// </summary>
    private VisualElement _root;
    
    /// <summary>
    /// Словарь кнопок действий и соответствующих им визуальных элементов.
    /// </summary>
    private Dictionary<Button, VisualElement> _actions;

    /// <summary>
    /// Список контроллеров действий.
    /// </summary>
    private List<IActionController> _actionControllers;
    
    /// <summary>
    /// Текущий отображаемый визуальный элемент действия.
    /// </summary>
    private VisualElement _currentAction;

    /// <summary>
    /// Кнопка сохранения.
    /// </summary>
    private Button _saveButton;
    
    /// <summary>
    /// Кнопка создания нового сюжета.
    /// </summary>
    private Button _newButton;
    
    /// <summary>
    /// Кнопка открытия сюжета.
    /// </summary>
    private Button _openButton;
    
    /// <summary>
    /// Кнопка экспорта в JSON.
    /// </summary>
    private Button _exportJsonButton;
    
    /// <summary>
    /// Кнопка экспорта в текстовый файл.
    /// </summary>
    private Button _exportTextButton;
    
    /// <summary>
    /// Кнопка информации о приложении.
    /// </summary>
    private Button _aboutButton;
    
    /// <summary>
    /// Сюжет приложения.
    /// </summary>
    private Plot _plot = new Plot();

    /// <summary>
    /// Менеджер базы данных.
    /// </summary>
    private DataBaseManager _dbm;

    /// <summary>
    /// Событие обновления элементов действия.
    /// </summary>
    public event Action UpdateActionEvent;
    
    /// <summary>
    /// Инициализация главного окна и установка обработчиков кнопок.
    /// </summary>
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;
        
        _saveButton = _root.Q<Button>("SaveButton");
        _newButton = _root.Q<Button>("NewButton");
        _openButton = _root.Q<Button>("OpenButton");
        _exportJsonButton = _root.Q<Button>("ExportJsonButton");
        _exportTextButton = _root.Q<Button>("ExportTextButton");
        _aboutButton = _root.Q<Button>("AboutButton");
        
        _saveButton.clicked += OnSaveButtonClicked;
        _newButton.clicked += OnNewButtonClicked;
        _openButton.clicked += OnOpenButtonClicked;
        _exportJsonButton.clicked += OnExportJsonButtonClicked;
        _exportTextButton.clicked += OnExportTextButtonClicked;
        _aboutButton.clicked += OnAboutButtonClicked;

        _actions = new Dictionary<Button, VisualElement>
        {
            { _root.Q<Button>("ViewActionButton"), _root.Q<VisualElement>("ViewAction") },
            { _root.Q<Button>("AIActionButton"), _root.Q<VisualElement>("AIAction") },
            { _root.Q<Button>("ProceduralActionButton"), _root.Q<VisualElement>("ProceduralAction") },
            { _root.Q<Button>("MapActionButton"), _root.Q<VisualElement>("MapAction") }
        };

        _actionControllers = new List<IActionController>
        {
            new ViewActionController(),
            new AIActionController(),
            new ProceduralActionController(),
            new MapActionController()
        };

        foreach (KeyValuePair<Button, VisualElement> pair in _actions)
        {
            pair.Key.clicked += () => OnActionButtonClicked(pair.Key);
        }
        
        foreach (var controller in _actionControllers)
        {
            controller.Initiate(_root, _plot);
            UpdateActionEvent += controller.GetUpdateAction();
        }
    }

    /// <summary>
    /// Обработчик нажатия кнопки сохранения.
    /// </summary>
    private void OnSaveButtonClicked()
    {
        string path = FileDialogUtility.SaveFilePanel("Save File", "", "New Save", "ang");
        if (path.Length == 0) return;
        _dbm = new DataBaseManager(path);
        _dbm.StorePlot(_plot);
    }

    /// <summary>
    /// Обработчик нажатия кнопки создания нового сюжета.
    /// </summary>
    private void OnNewButtonClicked()
    {
        _plot = new Plot();
        foreach (var controller in _actionControllers)
        {
            controller.Initiate(_root, _plot);
        }
        UpdateActionEvent?.Invoke();
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки открытия сюжета.
    /// </summary>
    private void OnOpenButtonClicked()
    {
        string path = FileDialogUtility.OpenFilePanel("Open File", "", "ang");
        if (path.Length == 0) return;
        _dbm = new DataBaseManager(path);
        _plot = _dbm.ReadPlot();
        foreach (var controller in _actionControllers)
        {
            controller.Initiate(_root, _plot);
        }
        UpdateActionEvent?.Invoke();
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки экспорта в JSON.
    /// </summary>
    private void OnExportJsonButtonClicked()
    {
        string path = FileDialogUtility.SaveFilePanel("Export JSON", "", "PlotExport", "json");
        if (path.Length == 0) return;
        Serializer.Serialize(_plot, path);
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки экспорта в текстовый файл.
    /// </summary>
    private void OnExportTextButtonClicked()
    {
        string path = FileDialogUtility.SaveFilePanel("Export Text", "", "PlotExport", "txt");
        if (path.Length == 0) return;
        string text = _plot.FullInfo();
        System.IO.File.WriteAllText(path, text);
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки информации о приложении.
    /// </summary>
    private static void OnAboutButtonClicked()
    {
        FileDialogUtility.DisplayDialog("About", "Narrative Generator v1.0", "OK");
    }

    /// <summary>
    /// Обработчик нажатия кнопки действия.
    /// </summary>
    /// <param name="button">Нажатая кнопка.</param>
    private void OnActionButtonClicked(Button button)
    {
        if (_currentAction != null)
        {
            _currentAction.style.display = DisplayStyle.None;
        }

        _currentAction = _actions[button];
        _currentAction.style.display = DisplayStyle.Flex;
        UpdateActionEvent?.Invoke();
    }
}
