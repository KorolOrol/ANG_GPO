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
    
    public static void BindElementsToList(ListView elementsListView, List<Element> newElements)
    {
        elements = newElements;
        elementsListView.makeItem = MakeElementListItem;
        elementsListView.bindItem = BindElementListItem;
        elementsListView.onAdd = ElementsListViewOnAdd;
        elementsListView.onRemove = ElementsListViewOnRemove;
        elementsListView.itemsSource = elements;
        elementsListView.selectedIndicesChanged += ElementsListViewSelectedIndicesClicked;
    }
    
    private static void ElementsListViewSelectedIndicesClicked(IEnumerable<int> ints)
    {
        int index = ints.First();
        Debug.Log($"{elements[index].Name}, {elements[index].Type}");
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
