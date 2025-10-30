using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;
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
    
    private static void ElementsListViewSelectedIndicesChanged(IEnumerable<int> indices)
    {
        int index = -1;
        foreach (var i in indices)
        {
            index = i;
            break;
        }
        if (index < 0)
        {
            currentElement = null;
            return;
        }
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
                case List<IElement> list:
                    {
                        // Список элементов -> список текстовых полей с именами элементов
                        List<Element> elementList = list.Cast<Element>().ToList();
                        var values = elementList.Select(el => el.Name);
                        var listView = CreateTextFieldList(param.Key, values);
                        paramsFoldout.contentContainer.Add(listView);
                    }
                    break;
                case List<Relation> relationList:
                    {
                        // Список отношений -> список текстовых полей в формате "Имя (Значение)"
                        var values = relationList.Select(r => $"{r.Character.Name} ({r.Value})");
                        var listView = CreateTextFieldList(param.Key, values);
                        paramsFoldout.contentContainer.Add(listView);
                    }
                    break;
                case List<string> stringList:
                    {
                        var listField = CreateTextField(param.Key, string.Join(", ", stringList));
                        paramsFoldout.contentContainer.Add(listField);
                    }
                    break;
                case Element element:
                    {
                        var elementField = CreateTextField(param.Key, element.Name);
                        paramsFoldout.contentContainer.Add(elementField);
                    }
                    break;
                default:
                    {
                        var stringField = CreateTextField(param.Key, param.Value.ToString());
                        paramsFoldout.contentContainer.Add(stringField);
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
            if (paramField is ListView listView)
            {
                string paramKey = listView.headerTitle;
                switch (paramKey)
                {
                    case "Relations":
                        {
                            if (currentElement.Params[paramKey] is List<Relation> oldRelations) 
                                foreach (var relation in oldRelations.ToList())
                                {
                                    Binder.Unbind(currentElement, relation.Character);
                                }
                            List<(Element, double)> relations = new List<(Element, double)>();
                            foreach (var item in listView.itemsSource)
                            {
                                if (item is TextField field)
                                {
                                    string text = field.value;
                                    int openParenIndex = text.LastIndexOf('(');
                                    int closeParenIndex = text.LastIndexOf(')');
                                    if (openParenIndex > 0 && closeParenIndex > openParenIndex)
                                    {
                                        string namePart = text.Substring(0, openParenIndex).Trim();
                                        string valuePart = text.Substring(openParenIndex + 1, 
                                            closeParenIndex - openParenIndex - 1).Trim();
                                        Element relatedElement = elements.Find(
                                            e => e.Name == namePart && e.Type == ElemType.Character);
                                        if (relatedElement != null 
                                            && double.TryParse(valuePart, out double relationValue))
                                        {
                                            Binder.Bind(currentElement, relatedElement, relationValue);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "Characters":
                    case "Items":
                    case "Locations":
                    case "Events":
                        {
                            if (currentElement.Params[paramKey] is List<Element> oldElements) 
                                foreach (var elem in oldElements.ToList())
                                {
                                    Binder.Unbind(currentElement, elem);
                                }
                            List<Element> paramElements = new List<Element>();
                            foreach (var item in listView.itemsSource)
                            {
                                if (item is TextField field)
                                {
                                    string name = field.value;
                                    Element foundElement = elements.Find(e => e.Name == name);
                                    if (foundElement != null)
                                    {
                                        Binder.Bind(currentElement, foundElement);
                                    }
                                }
                            }
                            currentElement.Params[paramKey] = paramElements;
                        }
                        break;
                }
            }
            if (paramField is TextField textField)
            {
                string paramKey = textField.label;
                switch (paramKey)
                {
                    case "Traits":
                        {
                            var traits = textField.value.Split(new[] { ',' }, 
                                    StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim()).ToList();
                            currentElement.Params[paramKey] = traits;
                        }
                        break;
                    default:
                        {
                            currentElement.Params[paramKey] = textField.value;
                        }
                        break;
                }
            }
        }
    }

    // Вспомогательные методы для создания типовых UI-элементов
    private static TextField CreateTextField(string label, string value)
    {
        var field = new TextField(label)
        {
            value = value
        };
        field.AddToClassList("TextField");
        return field;
    }

    private static ListView CreateTextFieldList(string labelBase, IEnumerable<string> values)
    {
        var listView = new ListView();
        listView.headerTitle = labelBase;
        listView.allowAdd = true;
        listView.allowRemove = true;
        listView.showAddRemoveFooter = true;
        listView.fixedItemHeight = 50;

        List<TextField> fields = values
            .Select((v, i) =>
            {
                var f = new TextField($"{labelBase} {i + 1}") { value = v };
                f.AddToClassList("TextField");
                return f;
            })
            .ToList();

        listView.makeItem = () =>
        {
            var field = new TextField();
            field.AddToClassList("TextField");
            return field;
        };
        listView.bindItem = (e, i) =>
        {
            var field = e as TextField;
            field.label = $"{labelBase} {i + 1}";
            field.value = fields[i].value;
        };
        listView.onAdd = (blv) =>
        {
            int index = blv.itemsSource.Count;
            var textField = new TextField($"{labelBase} {index + 1}");
            textField.AddToClassList("TextField");
            blv.itemsSource.Add(textField);
            blv.RefreshItems();
            blv.ScrollToItem(index);
        };
        listView.onRemove = (blv) =>
        {
            int index = blv.selectedIndex;
            blv.itemsSource.RemoveAt(index);
            blv.RefreshItems();
            blv.ScrollToItem(index);
        };
        listView.itemsSource = fields;
        return listView;
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
        int index = listView.selectedIndex;
        listView.itemsSource.RemoveAt(index - 1);
        listView.RefreshItems();
        listView.ScrollToItem(index - 2);
    };
}
