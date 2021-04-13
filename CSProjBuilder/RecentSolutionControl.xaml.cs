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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSProjBuilder
{
	/// <summary>
	/// Interaction logic for RecentSolutionControl.xaml
	/// </summary>
	public partial class RecentSolutionControl : UserControl
	{
		public CSProjSolution Solution { get; set; }
		public RecentSolutionControl()
		{
			InitializeComponent();
		}

		public RecentSolutionControl(CSProjSolution solution)
		{
			InitializeComponent();
			Solution = solution;
			ProjectLocationLabel.Content = solution.CSProjSolutionFilePath;
			ProjectNameLabel.Content = solution.SolutionName;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			WorkspaceWindow workspace = new WorkspaceWindow(Solution);
			workspace.Show();
			MainWindow.Current.Close();
		}
	}
}
