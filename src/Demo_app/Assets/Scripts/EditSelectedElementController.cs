using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;
using UnityEngine.UIElements;

/// <summary>
/// Контроллер панели редактирования выбранного элемента
/// </summary>
public class EditSelectedElementController
{
    /// <summary>
    /// Поле типа выбранного элемента
    /// </summary>
    private readonly TextField _typeTextField;
    
    /// <summary>
    /// Поле имени выбранного элемента
    /// </summary>
    private readonly TextField _nameTextField;
    
    /// <summary>
    /// Поле описания выбранного элемента
    /// </summary>
    private readonly TextField _descriptionTextField;
    
    /// <summary>
    /// Складной элемент для параметров выбранного элемента
    /// </summary>
    private readonly Foldout _paramsFoldout;
    
    /// <summary>
    /// Поле времени выбранного элемента
    /// </summary>
    private readonly TextField _timeTextField;
    
    /// <summary>
    /// Кнопка обновления выбранного элемента
    /// </summary>
    private readonly Button _updateElementButton;
    
    /// <summary>
    /// Выбранный элемент для редактирования
    /// </summary>
    private Element _selectedElement;

    /// <summary>
    /// Выбранный элемент для редактирования
    /// </summary>
    public Element SelectedElement
    {
        get => _selectedElement;
        set
        {
            _selectedElement = value;
            LoadSelectedElement();
        }
    }

    /// <summary>
    /// Сюжет, содержащий элементы
    /// </summary>
    private readonly Plot _plot;
    
    /// <summary>
    /// Событие, вызываемое перед загрузкой данных выбранного элемента
    /// </summary>
    public event Action BeforeLoadSelectedElement;
    
    /// <summary>
    /// Событие, вызываемое после обновления данных выбранного элемента
    /// </summary>
    public event Action AfterUpdateSelectedElement;

    /// <summary>
    /// Конструктор контроллера панели редактирования выбранного элемента
    /// </summary>
    /// <param name="editSelectedElementRoot">Корневой элемент панели редактирования выбранного элемента</param>
    /// <param name="plot">Сюжет, содержащий элементы</param>
    public EditSelectedElementController(VisualElement editSelectedElementRoot, Plot plot)
    {
        _plot = plot;
        _typeTextField = editSelectedElementRoot.Q<TextField>("TypeTextField");
        _nameTextField = editSelectedElementRoot.Q<TextField>("NameTextField");
        _descriptionTextField = editSelectedElementRoot.Q<TextField>("DescriptionTextField");
        _paramsFoldout = editSelectedElementRoot.Q<Foldout>("ParamsFoldout");
        _timeTextField = editSelectedElementRoot.Q<TextField>("TimeTextField");
        _updateElementButton = editSelectedElementRoot.Q<Button>("UpdateElementButton");
        
        _updateElementButton.clicked += UpdateSelectedElement;
    }
    
    /// <summary>
    /// Загрузка данных выбранного элемента в панель редактирования
    /// </summary>
    private void LoadSelectedElement()
    {
        BeforeLoadSelectedElement?.Invoke();
        _typeTextField.value = _selectedElement.Type.ToString();
        _nameTextField.value = _selectedElement.Name;
        _descriptionTextField.value = _selectedElement.Description;
        _timeTextField.value = _selectedElement.Time.ToString();
        _paramsFoldout.contentContainer.Clear();
        foreach (KeyValuePair<string, object> param in _selectedElement.Params)
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
    private void UpdateSelectedElement()
    {
        if (_selectedElement == null) return;
        _selectedElement.Name = _nameTextField.value;
        _selectedElement.Description = _descriptionTextField.value;
        if (int.TryParse(_timeTextField.value, out int time))
        {
            _selectedElement.Time = time;
        }
        foreach (var paramField in _paramsFoldout.contentContainer.Children())
        {
            switch (paramField)
            {
                case TextField textField:
                    UpdateSelectedElementByTextField(textField);
                    break;
                case ListView listView:
                    UpdateSelectedElementByListView(listView);
                    break;
            }
        }
        AfterUpdateSelectedElement?.Invoke();
    }
    
    /// <summary>
    /// Обновление параметров выбранного элемента, заданных с помощью TextField
    /// </summary>
    /// <param name="textField">TextField с параметром</param>
    private void UpdateSelectedElementByTextField(TextField textField)
    {
        string key = textField.label;
        string value = textField.value;
        if (!_selectedElement.Params.TryGetValue(key, out object existingValue)) return;
        switch (existingValue)
        {
            case Element existingElement:
                {
                    var element = _plot.Elements.FirstOrDefault(e => e.Name == value);
                    if (element != null)
                    {
                        Binder.Unbind(_selectedElement, existingElement);
                        Binder.Bind(_selectedElement, element);
                    }
                }
                break;
            case null:
                {
                    var element = _plot.Elements.FirstOrDefault(e => e.Name == value);
                    if (element != null)
                    {
                        Binder.Bind(_selectedElement, element);
                    }
                }
                break;
            case List<string>:
                {
                    List<string> stringList = value
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList();
                    _selectedElement.Params[key] = stringList;
                }
                break;
            default:
                _selectedElement.Params[key] = value;
                break;
        }
    }

    /// <summary>
    /// Обновление параметров выбранного элемента, заданных с помощью ListView
    /// </summary>
    /// <param name="listView">ListView с параметром</param>
    private void UpdateSelectedElementByListView(ListView listView)
    {
        string key = listView.headerTitle;
        if (!_selectedElement.Params.TryGetValue(key, out object existingValue)) return;
        switch (existingValue)
        {
            case List<IElement> existingElements:
                {
                    List<string> elementNames = listView.itemsSource
                        .Cast<string>()
                        .ToList();
                    List<IElement> newElements = _plot.Elements
                        .Where(e => elementNames.Contains(e.Name))
                        .ToList();
                    foreach (var oldElem in existingElements.ToList()
                        .Where(oldElem => !newElements.Contains(oldElem)))
                    {
                        Binder.Unbind(_selectedElement, oldElem);
                    }
                    foreach (var newElem in newElements
                        .Where(newElem => !existingElements.Contains(newElem)))
                    {
                        Binder.Bind(_selectedElement, newElem);
                    }
                }
                break;
            case List<Relation> existingRelations:
                {
                    List<string> relationStrings = listView.itemsSource
                        .Cast<string>()
                        .ToList();
                    List<(IElement element, double value)> newRelations = relationStrings
                        .Select(rs =>
                        {
                            int startIdx = rs.LastIndexOf('(');
                            int endIdx = rs.LastIndexOf(')');
                            if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx)
                                return (_selectedElement, 0);
                            string name = rs[..startIdx].Trim();
                            string valueStr = rs
                                .Substring(startIdx + 1, endIdx - startIdx - 1)
                                .Trim();
                            if (!double.TryParse(valueStr, out double value))
                                return (_selectedElement, 0);
                            var element = _plot.Elements
                                .FirstOrDefault(e => e.Name == name);
                            return element == null ? 
                                (_selectedElement, 0) : (element, value);
                        })
                        .Where(t => t != (_selectedElement, 0))
                        .ToList();
                    foreach (var oldRel in existingRelations.ToList()
                        .Where(oldRel => newRelations
                            .All(nr => nr.Item1 != oldRel.Character)))
                    {
                        Binder.Unbind(_selectedElement, oldRel.Character);
                    }
                    foreach (var newRel in newRelations)
                    {
                        Binder.Bind(_selectedElement, newRel.Item1, newRel.Item2);
                    }
                }
                break;
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
            List<string> src = (List<string>)blv.itemsSource;
            src.Add(string.Empty);
            blv.RefreshItems();
            blv.ScrollToItem(index);
        };
        listView.onRemove = blv =>
        {
            int index = blv.selectedIndex;
            if (index == -1) index = blv.itemsSource.Count - 1;
            blv.itemsSource.RemoveAt(index);
            blv.RefreshItems();
            blv.ScrollToItem(index);
        };
        listView.itemsSource = items;
        return listView;
    }

}
