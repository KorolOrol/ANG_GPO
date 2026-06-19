using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Cassius2.Views.Windows;

public partial class AboutWindow : Window
{
	public AboutWindow()
	{
		InitializeComponent();
		VersionText.Text = GetVersionText();
	}

	private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
	{
		Close();
	}

	private static string GetVersionText()
	{
		var assembly = Assembly.GetExecutingAssembly();
		// var info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		// if (info != null && !string.IsNullOrWhiteSpace(info.InformationalVersion))
		// {
		// 	return info.InformationalVersion;
		// }

		return assembly.GetName().Version?.ToString() ?? "Unknown";
	}
}
