using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isViewActionVisible = true;

    [ObservableProperty]
    private bool _isAiActionVisible;

    [ObservableProperty]
    private bool _isProppActionVisible;

    [RelayCommand]
    private void ShowViewAction()
    {
        IsViewActionVisible = true;
        IsAiActionVisible = false;
        IsProppActionVisible = false;
    }

    [RelayCommand]
    private void ShowAiAction()
    {
        IsViewActionVisible = false;
        IsAiActionVisible = true;
        IsProppActionVisible = false;
    }

    [RelayCommand]
    private void ShowProppAction()
    {
        IsViewActionVisible = false;
        IsAiActionVisible = false;
        IsProppActionVisible = true;
    }
}