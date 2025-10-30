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
    /// <summary>
    /// Список всех элементов для отображения и редактирования
    /// </summary>
    private static List<Element> _elements;
    
    /// <summary>
    /// Текущий выбранный элемент для редактирования
    /// </summary>
    private static Element _currentElement;
    
    /// <summary>
    /// Список элементов для редактирования
    /// </summary>
    private static ListView _elementsListView;
    
    /// <summary>
    /// Панель редактирования выбранного элемента
    /// </summary>
    private static VisualElement _editSelectedElement;
    
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
    
    /// <summary>
    /// Привязка элементов к списку в UI
    /// </summary>
    /// <param name="root">Корневой элемент UI</param>
    /// <param name="newElements">Список элементов для отображения</param>
    public static void BindElementsToList(VisualElement root, List<Element> newElements)
    {
        _elementsListView = root.Q<ListView>("ElementsListView");
        _editSelectedElement = root.Q<VisualElement>("EditSelectedElement");
        _typeTextField = root.Q<TextField>("TypeTextField");
        _nameTextField = root.Q<TextField>("NameTextField");
        _descriptionTextField = root.Q<TextField>("DescriptionTextField");
        _paramsFoldout = root.Q<Foldout>("ParamsFoldout");
        _timeTextField = root.Q<TextField>("TimeTextField");
        _updateElementButton = root.Q<Button>("UpdateElementButton");
        
        _elements = newElements;
        _elementsListView.makeItem = MakeElementListItem;
        _elementsListView.bindItem = BindElementListItem;
        _elementsListView.onAdd = ElementsListViewOnAdd;
        _elementsListView.onRemove = ElementsListViewOnRemove;
        _elementsListView.itemsSource = _elements;
        _elementsListView.selectedIndicesChanged += ElementsListViewSelectedIndicesChanged;

        _updateElementButton.clicked += UpdateSelectedElement;
    }
    
    /// <summary>
    /// Обработчик изменения выбранных индексов в списке элементов
    /// </summary>
    /// <param name="indices">Выбранные индексы</param>
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
            _currentElement = null;
            return;
        }
        Debug.Log($"{_elements[index].Name}, {_elements[index].Type}");
        _currentElement = _elements[index];
        LoadSelectedElement();
    }

    /// <summary>
    /// Загрузка данных выбранного элемента в панель редактирования
    /// </summary>
    private static void LoadSelectedElement()
    {
        if (_currentElement == null)
        {
            _editSelectedElement.style.display = DisplayStyle.None;
            return;
        }
        _editSelectedElement.style.display = DisplayStyle.Flex;
        _typeTextField.value = _currentElement.Type.ToString();
        _nameTextField.value = _currentElement.Name;
        _descriptionTextField.value = _currentElement.Description;
        _timeTextField.value = _currentElement.Time.ToString();
        _paramsFoldout.contentContainer.Clear();
        foreach (var param in _currentElement.Params)
        {
            switch (param.Value)
            {
                case List<IElement> list:
                    {
                        // Список элементов -> список текстовых полей с именами элементов
                        List<Element> elementList = list.Cast<Element>().ToList();
                        var values = elementList.Select(el => el.Name);
                        var listView = CreateTextFieldList(param.Key, values);
                        _paramsFoldout.contentContainer.Add(listView);
                    }
                    break;
                case List<Relation> relationList:
                    {
                        // Список отношений -> список текстовых полей в формате "Имя (Значение)"
                        var values = relationList.Select(r => $"{r.Character.Name} ({r.Value})");
                        var listView = CreateTextFieldList(param.Key, values);
                        _paramsFoldout.contentContainer.Add(listView);
                    }
                    break;
                case List<string> stringList:
                    {
                        var listField = CreateTextField(param.Key, string.Join(", ", stringList));
                        _paramsFoldout.contentContainer.Add(listField);
                    }
                    break;
                case Element element:
                    {
                        var elementField = CreateTextField(param.Key, element.Name);
                        _paramsFoldout.contentContainer.Add(elementField);
                    }
                    break;
                default:
                    {
                        var stringField = CreateTextField(param.Key, param.Value.ToString());
                        _paramsFoldout.contentContainer.Add(stringField);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Обновление данных выбранного элемента из панели редактирования
    /// </summary>
    private static void UpdateSelectedElement()
    {
        if (_currentElement == null) return;
        _currentElement.Name = _nameTextField.value;
        _currentElement.Description = _descriptionTextField.value;
        if (int.TryParse(_timeTextField.value, out int time))
        {
            _currentElement.Time = time;
        }
        foreach (VisualElement paramField in _paramsFoldout.contentContainer.Children())
        {
            if (paramField is ListView listView)
            {
                string paramKey = listView.headerTitle;
                switch (paramKey)
                {
                    case "Relations":
                        {
                            if (_currentElement.Params[paramKey] is List<Relation> oldRelations) 
                                foreach (var relation in oldRelations.ToList())
                                {
                                    Binder.Unbind(_currentElement, relation.Character);
                                }
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
                                        Element relatedElement = _elements.Find(
                                            e => e.Name == namePart && e.Type == ElemType.Character);
                                        if (relatedElement != null 
                                            && double.TryParse(valuePart, out double relationValue))
                                        {
                                            Binder.Bind(_currentElement, relatedElement, relationValue);
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
                            if (_currentElement.Params[paramKey] is List<Element> oldElements) 
                                foreach (var elem in oldElements.ToList())
                                {
                                    Binder.Unbind(_currentElement, elem);
                                }
                            List<Element> paramElements = new List<Element>();
                            foreach (var item in listView.itemsSource)
                            {
                                if (item is TextField field)
                                {
                                    string name = field.value;
                                    Element foundElement = _elements.Find(e => e.Name == name);
                                    if (foundElement != null)
                                    {
                                        Binder.Bind(_currentElement, foundElement);
                                    }
                                }
                            }
                            _currentElement.Params[paramKey] = paramElements;
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
                            _currentElement.Params[paramKey] = traits;
                        }
                        break;
                    default:
                        {
                            _currentElement.Params[paramKey] = textField.value;
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Создание текстового поля с заданной меткой и значением
    /// </summary>
    /// <param name="label">Метка поля</param>
    /// <param name="value">Значение поля</param>
    /// <returns>Созданное текстовое поле</returns>
    private static TextField CreateTextField(string label, string value)
    {
        var field = new TextField(label)
        {
            value = value
        };
        field.AddToClassList("TextField");
        return field;
    }

    /// <summary>
    /// Создание списка текстовых полей с возможностью добавления и удаления
    /// </summary>
    /// <param name="labelBase">Базовая метка для полей</param>
    /// <param name="values">Значения для полей</param>
    /// <returns>Созданный список текстовых полей</returns>
    private static ListView CreateTextFieldList(string labelBase, IEnumerable<string> values)
    {
        var listView = new ListView
        {
            headerTitle = labelBase,
            allowAdd = true,
            allowRemove = true,
            showAddRemoveFooter = true,
            fixedItemHeight = 50
        };

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
            if (e is TextField field)
            {
                field.label = $"{labelBase} {i + 1}";
                field.value = fields[i].value;
            }
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

    /// <summary>
    /// Создание элемента списка для отображения в ListView
    /// </summary>
    private readonly static Func<VisualElement> MakeElementListItem = () =>
    {
        var elementAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ElementListItem.uxml");
        var element = elementAsset.CloneTree();
        return element;
    };

    /// <summary>
    /// Привязка данных элемента к элементу списка в UI
    /// </summary>
    private readonly static Action<VisualElement, int> BindElementListItem = (e, i) =>
    {
        var labelUI = e.Q<Label>("ElementName");
        var iconUI = e.Q<VisualElement>("ElementIcon");
        VectorImage icon = ScriptableObject.CreateInstance<VectorImage>();
        Element element = _elements[i];
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

    /// <summary>
    /// Обработчик добавления нового элемента в список элементов
    /// </summary>
    private readonly static Action<BaseListView> ElementsListViewOnAdd = listView =>
    {
        int index = listView.itemsSource.Count;
        var newElement = new Element(ElemType.Character, index.ToString());
        listView.itemsSource.Add(newElement);
        listView.RefreshItems();
        listView.ScrollToItem(index);
    };

    /// <summary>
    /// Обработчик удаления выбранного элемента из списка элементов
    /// </summary>
    private readonly static Action<BaseListView> ElementsListViewOnRemove = listView =>
    {
        int index = listView.selectedIndex;
        listView.itemsSource.RemoveAt(index - 1);
        listView.RefreshItems();
        listView.ScrollToItem(index - 2);
    };
}
