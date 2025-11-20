using System;
using System.Collections.Generic;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services;
using UnityEngine;
using UnityEngine.UIElements;

public class MainWindowScript : MonoBehaviour
{
    private VisualElement _root;
    private Dictionary<Button, VisualElement> _actions;
    private VisualElement _currentAction;
    private readonly static Plot Plot = new Plot();

    public event Action UpdateActionEvent;
    
    /// <summary>
    /// Инициализация главного окна и установка обработчиков кнопок.
    /// </summary>
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;

        _actions = new Dictionary<Button, VisualElement>
        {
            { _root.Q<Button>("ViewActionButton"), _root.Q<VisualElement>("ViewAction") },
            { _root.Q<Button>("AIActionButton"), _root.Q<VisualElement>("AIAction") }
        };

        foreach (KeyValuePair<Button, VisualElement> pair in _actions)
        {
            pair.Key.clicked += () => OnActionButtonClicked(pair.Key);
        }
        
        Plot.Add(FullElementConstructor.CreateFullElement(ElemType.Character, "1"));
        Plot.Add(FullElementConstructor.CreateFullElement(ElemType.Item, "2"));
        Plot.Add(FullElementConstructor.CreateFullElement(ElemType.Location, "3"));
        Plot.Add(FullElementConstructor.CreateFullElement(ElemType.Event, "4"));
        Plot.Add(FullElementConstructor.CreateFullElement(ElemType.Character, "5"));
        Binder.Bind(Plot.Elements[0], Plot.Elements[1]);
        Binder.Bind(Plot.Elements[0], Plot.Elements[2]);
        Binder.Bind(Plot.Elements[0], Plot.Elements[3]);
        Binder.Bind(Plot.Elements[0], Plot.Elements[4], 50);
        
        ViewActionScript.Initiate(_root, Plot);
        UpdateActionEvent += ViewActionScript.GetUpdateElementsListAction();
        AIActionScript.Initiate(_root, Plot);
        UpdateActionEvent += AIActionScript.GetUpdateExistingElementsListAction();
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
