using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSProjBuilder
{
	public static class Constants
	{
		public static string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CSProjBuilder");
		public static string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CSProjBuilder", "settings.json");

		public static string DefaultAppXamlFile = @"
<Application x:Class=""$ProjectName$.App""
             xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
             xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
             xmlns:local=""clr-namespace:$ProjectName$""
             StartupUri=""source/MainWindow.xaml"">
    <Application.Resources>
         
    </Application.Resources>
</Application>
";

		public static string DefaultAppXamlCsFile = @"
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace $ProjectName$
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	}
}
";

		public static string NewClassFile = @"
using System;
using System.Collections.Generic;

namespace $ProjectName$
{
	class $ClassFileName$
	{

	}
}
";

		public static string NewXamlWindowFile = @"
<Window x:Class=""$ProjectName$.$XamlFileName$""
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
        xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
        xmlns:local=""clr-namespace:$ProjectName$""
        mc:Ignorable=""d"" Background=""#242424""
        Title=""$XamlFileName$"" Height=""450"" Width=""800"">
	<StackPanel>

	</StackPanel>
</Window>
";

		public static string NewXamlControlFile = @"
<UserControl x:Class=""$ProjectName$.$XamlFileName$""
             xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
             xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
             xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" 
             xmlns:d=""http://schemas.microsoft.com/expression/blend/2008"" 
             xmlns:local=""clr-namespace:$ProjectName$""
             mc:Ignorable=""d"" 
             d:DesignHeight=""450"" d:DesignWidth=""800"">
    <StackPanel>
            
    </StackPanel>
</UserControl>
";
		public static string NewXamlCsFile = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace $ProjectName$
{
	public partial class $XamlFileName$ : $InheritedType$
	{
		public $XamlFileName$()
		{
			InitializeComponent();
		}
	}
}
";

		internal static string CreateNewXamlCsFile(CSProjSolution currentSolution, string text, bool isUserControl = false)
		{
			if (isUserControl)
			{
				return NewXamlCsFile.Replace("$ProjectName$", currentSolution.SolutionName).Replace("$XamlFileName$", text.Replace(".xaml", "")).Replace("$InheritedType$", "UserControl");
			}
			else
			{
				return NewXamlCsFile.Replace("$ProjectName$", currentSolution.SolutionName).Replace("$XamlFileName$", text.Replace(".xaml", "")).Replace("$InheritedType$", "Window");
			}
		}

		internal static string CreateNewClass(CSProjSolution currentSolution, string text)
		{
			return NewClassFile.Replace("$ProjectName$", currentSolution.SolutionName).Replace("$ClassFileName$", text.Replace(".cs", ""));
		}

		internal static string CreateNewXamlWindowFile(CSProjSolution currentSolution, string text)
		{
			return NewXamlWindowFile.Replace("$ProjectName$", currentSolution.SolutionName).Replace("$XamlFileName$", text.Replace(".xaml", ""));
		}

		internal static string CreateNewXamlControlFile(CSProjSolution currentSolution, string text)
		{
			return NewXamlControlFile.Replace("$ProjectName$", currentSolution.SolutionName).Replace("$XamlFileName$", text.Replace(".xaml", ""));
		}
	}
}
