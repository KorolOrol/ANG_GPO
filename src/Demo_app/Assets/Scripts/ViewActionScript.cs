using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class ViewActionScript
{
    private static List<Element> elements;
    private static Element currentElement;
    private static ListView elementsListView;
    private static VisualElement editSelectedElement;
    private static TextField typeTextField;
    private static TextField nameTextField;
    private static TextField descriptionTextField;
    private static Foldout paramsFoldout;
    private static TextField timeTextField;
    private static Button updateElementButton;
    
    public static void BindElementsToList(VisualElement root, List<Element> newElements)
    {
        elementsListView = root.Q<ListView>("ElementsListView");
        editSelectedElement = root.Q<VisualElement>("EditSelectedElement");
        typeTextField = root.Q<TextField>("TypeTextField");
        nameTextField = root.Q<TextField>("NameTextField");
        descriptionTextField = root.Q<TextField>("DescriptionTextField");
        paramsFoldout = root.Q<Foldout>("ParamsFoldout");
        timeTextField = root.Q<TextField>("TimeTextField");
        updateElementButton = root.Q<Button>("UpdateElementButton");
        
        elements = newElements;
        elementsListView.makeItem = MakeElementListItem;
        elementsListView.bindItem = BindElementListItem;
        elementsListView.onAdd = ElementsListViewOnAdd;
        elementsListView.onRemove = ElementsListViewOnRemove;
        elementsListView.itemsSource = elements;
        elementsListView.selectedIndicesChanged += ElementsListViewSelectedIndicesChanged;

        updateElementButton.clicked += UpdateSelectedElement;
    }
    
    private static void ElementsListViewSelectedIndicesChanged(IEnumerable<int> ints)
    {
        if (!ints.Any())
        {
            currentElement = null;
            return;
        }
        int index = ints.FirstOrDefault();
        Debug.Log($"{elements[index].Name}, {elements[index].Type}");
        currentElement = elements[index];
        LoadSelectedElement();
    }

    private static void LoadSelectedElement()
    {
        if (currentElement == null)
        {
            editSelectedElement.style.display = DisplayStyle.None;
            return;
        }
        editSelectedElement.style.display = DisplayStyle.Flex;
        typeTextField.value = currentElement.Type.ToString();
        nameTextField.value = currentElement.Name;
        descriptionTextField.value = currentElement.Description;
        timeTextField.value = currentElement.Time.ToString();
        paramsFoldout.contentContainer.Clear();
        foreach (var param in currentElement.Params)
        {
            switch (param.Value)
            {
                case List<string> stringList:
                    {
                        var listField = new TextField(param.Key)
                        {
                            value = string.Join(", ", stringList)
                        };
                        paramsFoldout.contentContainer.Add(listField);
                    }
                    break;
                case List<Element> elementList:
                    {
                        var listView = new ListView();
                        listView.allowAdd = true;
                        listView.allowRemove = true;
                        listView.showAddRemoveFooter = true;
                        List<TextField> fields = new List<TextField>();
                        for (int i = 0; i < elementList.Count; i++)
                        {
                            var field = new TextField($"{param.Key} {i + 1}")
                            {
                                value = elementList[i].Name
                            };
                            fields.Add(field);
                        }
                        listView.makeItem = () =>
                        {
                            var field = new TextField();
                            return field;
                        };
                        listView.bindItem = (e, i) =>
                        {
                            var field = e as TextField;
                            field.label = $"{param.Key} {i + 1}";
                            field.value = fields[i].value;
                        };
                        listView.itemsSource = fields;
                        paramsFoldout.contentContainer.Add(listView);
                    }
                    break;
                case Element element:
                    {
                        var elementField = new TextField(param.Key)
                        {
                            value = element.Name
                        };
                        paramsFoldout.contentContainer.Add(elementField);
                    }
                    break;
            }
        }
    }

    private static void UpdateSelectedElement()
    {
        if (currentElement == null) return;
        currentElement.Name = nameTextField.value;
        currentElement.Description = descriptionTextField.value;
        if (int.TryParse(timeTextField.value, out int time))
        {
            currentElement.Time = time;
        }
        foreach (VisualElement paramField in paramsFoldout.contentContainer.Children())
        {
            
        }
    }

    private readonly static Func<VisualElement> MakeElementListItem = () =>
    {
        var elementAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ElementListItem.uxml");
        var element = elementAsset.CloneTree();
        return element;
    };

    private readonly static Action<VisualElement, int> BindElementListItem = (e, i) =>
    {
        var labelUI = e.Q<Label>("ElementName");
        var iconUI = e.Q<VisualElement>("ElementIcon");
        VectorImage icon = ScriptableObject.CreateInstance<VectorImage>();
        Element element = elements[i];
        switch (element.Type)
        {
            case ElemType.Character:
                icon = AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeCharacterIcon.svg");
                break;
            case ElemType.Item:
                icon = AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeItemIcon.svg");
                break;
            case ElemType.Location:
                icon = AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeLocationIcon.svg");
                break;
            case ElemType.Event:
                icon = AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeEventIcon.svg");
                break;
        }
        labelUI.text = element.Name;
        iconUI.style.backgroundImage = new StyleBackground(Background.FromVectorImage(icon));
    };

    private readonly static Action<BaseListView> ElementsListViewOnAdd = (BaseListView listView) =>
    {
        int index = listView.itemsSource.Count;
        var newElement = new Element(ElemType.Character, index.ToString());
        listView.itemsSource.Add(newElement);
        listView.RefreshItems();
        listView.ScrollToItem(index);
    };

    private readonly static Action<BaseListView> ElementsListViewOnRemove = (BaseListView listView) =>
    {
        int index = listView.itemsSource.Count;
        listView.itemsSource.RemoveAt(index - 1);
        listView.RefreshItems();
        listView.ScrollToItem(index - 2);
    };
}
