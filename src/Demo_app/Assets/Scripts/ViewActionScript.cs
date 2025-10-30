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
        foreach (int i in indices)
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
        foreach (KeyValuePair<string, object> param in _currentElement.Params)
        {
            switch (param.Value)
            {
                case List<IElement> list:
                    {
                        // Список элементов -> список текстовых полей с именами элементов
                        List<Element> elementList = list.Cast<Element>().ToList();
                        IEnumerable<string> values = elementList.Select(el => el.Name);
                        var listView = CreateTextFieldList(param.Key, values);
                        _paramsFoldout.contentContainer.Add(listView);
                    }
                    break;
                case List<Relation> relationList:
                    {
                        // Список отношений -> список текстовых полей в формате "Имя (Значение)"
                        IEnumerable<string> values = relationList
                            .Select(r => $"{r.Character.Name} ({r.Value})");
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
                        var stringField = CreateTextField(param.Key, 
                            param.Value is not null ? param.Value.ToString() : string.Empty);
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
        foreach (var paramField in _paramsFoldout.contentContainer.Children())
        {
            switch (paramField)
            {
                case TextField textField:
                    {
                        string key = textField.label;
                        string value = textField.value;
                        if (_currentElement.Params.TryGetValue(key, out object existingValue))
                        {
                            switch (existingValue)
                            {
                                case Element existingElement:
                                    {
                                        var element = _elements.FirstOrDefault(e => e.Name == value);
                                        if (element != null)
                                        {
                                            Binder.Unbind(_currentElement, existingElement);
                                            Binder.Bind(_currentElement, element);
                                        }
                                    }
                                    break;
                                case List<string>:
                                    {
                                        List<string> stringList = value
                                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim())
                                            .ToList();
                                        _currentElement.Params[key] = stringList;
                                    }
                                    break;
                                default:
                                    _currentElement.Params[key] = value;
                                    break;
                            }
                        }
                    }
                    break;
                case ListView listView:
                    {
                        string key = listView.headerTitle;
                        if (_currentElement.Params.TryGetValue(key, out object existingValue))
                        {
                            switch (existingValue)
                            {
                                case List<IElement> existingElements:
                                    {
                                        List<string> elementNames = listView.itemsSource
                                            .Cast<string>()
                                            .ToList();
                                        List<Element> newElements = _elements
                                            .Where(e => elementNames.Contains(e.Name))
                                            .ToList();
                                        foreach (var oldElem in existingElements.ToList()
                                            .Where(oldElem => !newElements.Contains(oldElem)))
                                        {
                                            Binder.Unbind(_currentElement, oldElem);
                                        }
                                        foreach (var newElem in newElements
                                            .Where(newElem => !existingElements.Contains(newElem)))
                                        {
                                            Binder.Bind(_currentElement, newElem);
                                        }
                                    }
                                    break;
                                case List<Relation> existingRelations:
                                    {
                                        List<string> relationStrings = listView.itemsSource
                                            .Cast<string>()
                                            .ToList();
                                        List<(Element, double)> newRelations = relationStrings
                                            .Select(rs =>
                                            {
                                                int startIdx = rs.LastIndexOf('(');
                                                int endIdx = rs.LastIndexOf(')');
                                                if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx)
                                                    return (_currentElement, 0);
                                                string name = rs[..startIdx].Trim();
                                                string valueStr = rs
                                                    .Substring(startIdx + 1, endIdx - startIdx - 1)
                                                    .Trim();
                                                if (!double.TryParse(valueStr, out double value))
                                                    return (_currentElement, 0);
                                                var element = _elements.FirstOrDefault(e => e.Name == name);
                                                return element == null ? 
                                                    (_currentElement, 0) : (element, value);
                                            })
                                            .Where(t => t != (_currentElement, 0))
                                            .ToList();
                                        foreach (var oldRel in existingRelations.ToList()
                                            .Where(oldRel => newRelations
                                                .All(nr => nr.Item1 != oldRel.Character)))
                                        {
                                            Binder.Unbind(_currentElement, oldRel.Character);
                                        }
                                        foreach (var newRel in newRelations)
                                        {
                                            Binder.Bind(_currentElement, newRel.Item1, newRel.Item2);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    break;
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

        List<string> items = values.ToList();

        listView.makeItem = () =>
        {
            var field = new TextField();
            field.AddToClassList("TextField");
            field.RegisterValueChangedCallback(evt =>
            {
                if (field.userData is int idx and >= 0 && idx < listView.itemsSource.Count)
                    items[idx] = evt.newValue;
            });
            return field;
        };
        listView.bindItem = (e, i) =>
        {
            if (e is not TextField field) return;
            field.label = $"{labelBase} {i + 1}";
            field.userData = i;
            field.SetValueWithoutNotify(items[i]);
        };
        listView.onAdd = blv =>
        {
            int index = blv.itemsSource.Count;
            var src = (List<string>)blv.itemsSource;
            src.Add(string.Empty);
            blv.RefreshItems();
            blv.ScrollToItem(index);
        };
        listView.onRemove = blv =>
        {
            int index = blv.selectedIndex;
            blv.itemsSource.RemoveAt(index);
            blv.RefreshItems();
            blv.ScrollToItem(index);
        };
        listView.itemsSource = items;
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
        var icon = ScriptableObject.CreateInstance<VectorImage>();
        var element = _elements[i];
        icon = element.Type switch
        {
            ElemType.Character =>
                AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeCharacterIcon.svg"),
            ElemType.Item => AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeItemIcon.svg"),
            ElemType.Location => AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeLocationIcon.svg"),
            ElemType.Event => AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeEventIcon.svg"),
            _ => icon
        };
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
