using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainWindowScript : MonoBehaviour
{
    private VisualElement root;
    private Dictionary<Button, VisualElement> actions;
    private VisualElement currentAction;
    private static List<Element> elements = new List<Element>();
    
    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        actions = new Dictionary<Button, VisualElement>
        {
            { root.Q<Button>("ViewActionButton"), root.Q<VisualElement>("ViewAction") }
        };

        foreach (var pair in actions)
        {
            pair.Key.clicked += () => OnActionButtonClicked(pair.Key);
        }
        
        elements.Add(new Element(ElemType.Character, "1"));
        elements.Add(new Element(ElemType.Item, "2"));
        elements.Add(new Element(ElemType.Location, "3"));
        elements.Add(new Element(ElemType.Event, "4"));
        elements.Add(new Element(ElemType.Character, "5"));
        Binder.Bind(elements[0], elements[1]);
        Binder.Bind(elements[0], elements[2]);
        Binder.Bind(elements[0], elements[3]);
        Binder.Bind(elements[0], elements[4], 50);
        
        ViewActionScript.BindElementsToList(root, elements);
    }

    private void OnActionButtonClicked(Button button)
    {
        if (currentAction != null)
        {
            currentAction.style.display = DisplayStyle.None;
        }

        currentAction = actions[button];
        currentAction.style.display = DisplayStyle.Flex;
    }
}
