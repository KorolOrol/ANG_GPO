using Avalonia.Controls;
using Cassius2.ViewModels.Windows;

namespace Cassius2.Views.Windows;

public partial class ConfirmWindow : Window
{
    public ConfirmWindowViewModel ViewModel { get; }

    public ConfirmWindow()
    {
        InitializeComponent();
        ViewModel = new ConfirmWindowViewModel();
        DataContext = ViewModel;
        ViewModel.CloseAction = (result) => Close(result);
    }
}
