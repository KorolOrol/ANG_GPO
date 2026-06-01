using System;
using System.Reflection;
using Avalonia.Controls;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services;
using Cassius2.Models;
using Cassius2.ViewModels.Controls;
using Cassius2.Views.Windows;

namespace Cassius2.Views.Controls;

public partial class ElementEditor : UserControl
{
    public ElementEditorViewModel ViewModel { get; }

    public event Action? ElementUpdated;

    public ElementEditor()
    {
        InitializeComponent();
        ViewModel = new ElementEditorViewModel();
        DataContext = ViewModel;

        ViewModel.RequestUpdate += () => ElementUpdated?.Invoke();

        ViewModel.RequestAddParamAsync = async () =>
        {
            if (TopLevel.GetTopLevel(this) is not Window window) return null;

            var editWindow = new ParamEditWindow();
            editWindow.SetupForAdd();
            bool result = await editWindow.ShowDialog<bool>(window);
            if (!result || editWindow.ParsedValue == null) return null;
            var valType = TypeNameHelper.ResolveType(editWindow.ParamValueType);
            var dynMethod = typeof(ElementEditor).GetMethod(nameof(CreateParamKey), 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var genericMethod = dynMethod.MakeGenericMethod(valType);
                
            var vm = new ParamListItemViewModel();
            if (genericMethod.Invoke(this, [editWindow.ParamNamespace, editWindow.ParamName]) is not IParamKey key)
                return null;
            vm.Setup(key, editWindow.ParsedValue);
            return vm;
        };

        ViewModel.RequestAddRelationAsync = async () =>
        {
            if (TopLevel.GetTopLevel(this) is not Window window) return null;

            var editWindow = new RelationEditWindow();
            editWindow.SetupForAdd(AppState.Plot.Elements);
            bool result = await editWindow.ShowDialog<bool>(window);
            if (!result || editWindow.SelectedTarget == null || editWindow.ParsedValue == null) return null;
            var valType = TypeNameHelper.ResolveType(editWindow.ParamValueType);
            var dynMethod = typeof(ElementEditor).GetMethod(nameof(CreateParamKey), 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            var genericMethod = dynMethod.MakeGenericMethod(valType);

            if (genericMethod.Invoke(this, [editWindow.ParamNamespace, editWindow.ParamName]) is not IParamKey key)
                return null;
            if (ViewModel.Element == null) return null;
            var relation = new Relation(ViewModel.Element, editWindow.SelectedTarget, key, editWindow.ParsedValue);
            var vm = new RelationListItemViewModel();
            vm.Setup(relation);
            return vm;
        };
    }

    private IParamKey CreateParamKey<T>(string ns, string name)
    {
        return new ParamKey<T>(name, ns);
    }
    
    public void LoadElement(IElement element)
    {
        ViewModel.LoadElement(element);
    }

    public bool HasChanges()
    {
        return ViewModel.HasChanges();
    }
}