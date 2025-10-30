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
    private readonly static List<Element> Elements = new List<Element>();
    
    /// <summary>
    /// Инициализация главного окна и установка обработчиков кнопок.
    /// </summary>
    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;

        _actions = new Dictionary<Button, VisualElement>
        {
            { _root.Q<Button>("ViewActionButton"), _root.Q<VisualElement>("ViewAction") }
        };

        foreach (var pair in _actions)
        {
            pair.Key.clicked += () => OnActionButtonClicked(pair.Key);
        }
        
        Elements.Add(new Element(ElemType.Character, "1"));
        Elements.Add(new Element(ElemType.Item, "2"));
        Elements.Add(new Element(ElemType.Location, "3"));
        Elements.Add(new Element(ElemType.Event, "4"));
        Elements.Add(new Element(ElemType.Character, "5"));
        Binder.Bind(Elements[0], Elements[1]);
        Binder.Bind(Elements[0], Elements[2]);
        Binder.Bind(Elements[0], Elements[3]);
        Binder.Bind(Elements[0], Elements[4], 50);
        
        ViewActionScript.BindElementsToList(_root, Elements);
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
    }
}
