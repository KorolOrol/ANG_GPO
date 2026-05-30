using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using Cassius2.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels.Controls;

public partial class ElementEditorViewModel : ViewModelBase
{
    public IElement? Element { get; private set; }

    private readonly ParamBag _tempParams = new();
    private readonly ObservableCollection<Relation> _tempRelations = [];

    public event Action? RequestUpdate;

    [ObservableProperty]
    private string _elementType = "";

    [ObservableProperty]
    private string _elementName = "";

    [ObservableProperty]
    private string _elementDescription = "";

    [ObservableProperty]
    private string _elementTime = "";

    public ObservableCollection<ParamListItemViewModel> Params { get; } = [];
    public ObservableCollection<RelationListItemViewModel> Relations { get; } = [];

    [ObservableProperty]
    private ParamListItemViewModel? _selectedParam;

    [ObservableProperty]
    private RelationListItemViewModel? _selectedRelation;

    public Func<Task<ParamListItemViewModel?>>? RequestAddParamAsync;
    public Func<Task<RelationListItemViewModel?>>? RequestAddRelationAsync;

    public void LoadElement(IElement element)
    {
        Element = element;
        ElementType = element.Type.ToString();
        ElementName = element.Name;
        ElementDescription = element.Description;
        ElementTime = element.Time.ToString();

        _tempParams.Clear();
        foreach (var p in element.Params.Enumerate())
        {
            _tempParams.Add(p.Key, p.Value);
        }

        _tempRelations.Clear();
        foreach (var r in AppState.Plot.Relations.Where(x => Equals(x.Source, element)))
        {
            _tempRelations.Add(r);
        }

        RefreshParams();
        RefreshRelations();
    }

    private void RefreshParams()
    {
        Params.Clear();
        foreach (var param in _tempParams.Enumerate())
        {
            var vm = new ParamListItemViewModel();
            vm.Setup(param.Key, param.Value);
            vm.ParamEdited += (k, v) =>
            {
                _tempParams.Set(k, v);
                RefreshParams();
            };
            Params.Add(vm);
        }
    }

    private void RefreshRelations()
    {
        Relations.Clear();
        foreach (var rel in _tempRelations)
        {
            var vm = new RelationListItemViewModel();
            vm.Setup(rel);
            vm.RelationEdited += (r, v) =>
            {
                int index = _tempRelations.IndexOf(r);
                if (index >= 0)
                {
                    _tempRelations[index] = new Relation(r.Source, r.Target, r.Param, v);
                }
                RefreshRelations();
            };
            Relations.Add(vm);
        }
    }

    [RelayCommand]
    private async Task AddParamAsync()
    {
        if (RequestAddParamAsync != null)
        {
            var newParam = await RequestAddParamAsync();
            if (newParam is { Key: not null })
            {
                _tempParams.Add(newParam.Key, newParam.Value);
                RefreshParams();
            }
        }
    }

    [RelayCommand]
    private void RemoveParam()
    {
        if (SelectedParam?.Key == null) return;
        _tempParams.Remove(SelectedParam.Key);
        RefreshParams();
    }

    [RelayCommand]
    private async Task AddRelationAsync()
    {
        if (RequestAddRelationAsync != null)
        {
            var newRel = await RequestAddRelationAsync();
            if (newRel is { RelationData: not null })
            {
                _tempRelations.Add(newRel.RelationData);
                RefreshRelations();
            }
        }
    }

    [RelayCommand]
    private void RemoveRelation()
    {
        if (SelectedRelation?.RelationData == null) return;
        _tempRelations.Remove(SelectedRelation.RelationData);
        RefreshRelations();
    }

    [RelayCommand]
    private void Update()
    {
        if (Element == null) return;

        Element.Name = ElementName;
        Element.Description = ElementDescription;

        Element.Params.Clear();
        foreach (var p in _tempParams.Enumerate())
        {
            Element.Params.Add(p.Key, p.Value);
        }

        var oldRelations = AppState.Plot.Relations.Where(r => Equals(r.Source, Element)).ToList();
        foreach (var r in oldRelations)
        {
            AppState.Plot.Unbind(r.Source, r.Target, r.Param);
        }

        foreach (var r in _tempRelations)
        {
            AppState.Plot.Bind(r.Source, r.Target, r.Param, r.Value);
        }

        RequestUpdate?.Invoke();
    }

    public bool HasChanges()
    {
        if (Element == null) return false;

        if (Element.Name != ElementName) return true;
        if (Element.Description != ElementDescription) return true;

        if (Element.Params.Count != _tempParams.Count) return true;

        foreach (var p in Element.Params.Enumerate())
        {
            if (!_tempParams.TryGetValue(p.Key,
                    out var val) ||
                (val != null && !val.Equals(p.Value)) ||
                (val == null && p.Value != null))
                return true;
        }

        var oldRels = AppState.Plot.Relations.Where(r => Equals(r.Source, Element)).ToList();
        return oldRels.Count != _tempRelations.Count ||
               _tempRelations.Any(r => !oldRels.Any(or =>
                   or.Target.Equals(r.Target) &&
                   or.Param.Equals(r.Param) &&
                   ((or.Value == null && r.Value == null) || (or.Value != null && or.Value.Equals(r.Value)))));
    }
}
