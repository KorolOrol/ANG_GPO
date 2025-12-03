using System;
using System.Collections.Generic;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services;
using DataBase;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
        _aboutButton = _root.Q<Button>("AboutButton");
        
        _saveButton.clicked += OnSaveButtonClicked;
        _newButton.clicked += OnNewButtonClicked;
        _openButton.clicked += OnOpenButtonClicked;
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
        
        _plot.Add(FullElementConstructor.CreateFullElement(ElemType.Character, "1"));
        _plot.Add(FullElementConstructor.CreateFullElement(ElemType.Item, "2"));
        _plot.Add(FullElementConstructor.CreateFullElement(ElemType.Location, "3"));
        _plot.Add(FullElementConstructor.CreateFullElement(ElemType.Event, "4"));
        _plot.Add(FullElementConstructor.CreateFullElement(ElemType.Character, "5"));
        Binder.Bind(_plot.Elements[0], _plot.Elements[1]);
        Binder.Bind(_plot.Elements[0], _plot.Elements[2]);
        Binder.Bind(_plot.Elements[0], _plot.Elements[3]);
        Binder.Bind(_plot.Elements[0], _plot.Elements[4], 50);
        
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
        string path = EditorUtility.SaveFilePanel("Save File", "", "New Save", "ang");
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
        string path = EditorUtility.OpenFilePanel("Open File", "", "ang");
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
    /// Обработчик нажатия кнопки информации о приложении.
    /// </summary>
    private static void OnAboutButtonClicked()
    {
        EditorUtility.DisplayDialog("About", "Narrative Generator v1.0", "OK");
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
