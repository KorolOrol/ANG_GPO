using System;
using BaseClasses.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cassius2.ViewModels.Controls;

public partial class RelationListItemViewModel : ViewModelBase
{
    public Relation? RelationData { get; private set; }

    [ObservableProperty]
    private string _displayText = "";

    [ObservableProperty]
    private object? _value;

    public event Action<Relation, object?>? RelationEdited;

    public void Setup(Relation relation)
    {
        RelationData = relation;
        Value = relation.Value;
        DisplayText = $"{relation.Param.Namespace}.{relation.Param.Name} -> " +
                      $"{relation.Target.Name}: {relation.Value?.ToString() ?? "null"}";
    }

    public void NotifyEdited(object? newValue)
    {
        if (RelationData == null) return;
        Value = newValue;
        RelationEdited?.Invoke(RelationData, newValue);
    }
}
