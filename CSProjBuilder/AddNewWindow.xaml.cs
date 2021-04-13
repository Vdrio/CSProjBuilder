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

namespace CSProjBuilder
{
	/// <summary>
	/// Interaction logic for AddNewWindow.xaml
	/// </summary>
	/// 
	public enum NewFileType
	{
		None = 0, 
		Class = 1,
		Window = 2,
		UserControl = 3
	}
	public partial class AddNewWindow : Window
	{
		public CSProjSolution CurrentSolution { get; set; }
		public WorkspaceWindow CurrentWorkspace { get; set; }

		public NewFileType SelectedFileType { get; set; }
		public AddNewWindow()
		{
			InitializeComponent();
		}

		public AddNewWindow(CSProjSolution solution, WorkspaceWindow workspace)
		{
			InitializeComponent();
			CurrentSolution = solution;
			CurrentWorkspace = workspace;
		}

		//Class
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			NewFileNameText.Text = "Class1.cs";
			SelectedFileType = NewFileType.Class;
		}

		//Window
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			NewFileNameText.Text = "Window1.xaml";
			SelectedFileType = NewFileType.Window;
		}

		//User Control
		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			NewFileNameText.Text = "UserControl1.xaml";
			SelectedFileType = NewFileType.UserControl;
		}

		//Add
		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(NewFileNameText.Text))
			{
				if (SelectedFileType == NewFileType.Window)
				{
					if (!NewFileNameText.Text.Contains(".xaml"))
					{
						NewFileNameText.Text += ".xaml";
					}
					File.WriteAllText(Path.Combine(CurrentSolution.SourceDirectory, NewFileNameText.Text), Constants.CreateNewXamlWindowFile(CurrentSolution, NewFileNameText.Text));
					File.WriteAllText(Path.Combine(CurrentSolution.SourceDirectory, NewFileNameText.Text + ".cs"), Constants.CreateNewXamlCsFile(CurrentSolution, NewFileNameText.Text));
					CurrentSolution.AddProjectFile(NewFileNameText.Text, false);
					CurrentSolution.AddProjectFile(NewFileNameText.Text + ".cs");
					CurrentWorkspace.AddProjectFile(NewFileNameText.Text);
					CurrentWorkspace.AddProjectFile(NewFileNameText.Text + ".cs");
					CurrentSolution.Save();
					this.Close();
				}
				else if (SelectedFileType == NewFileType.UserControl)
				{
					if (!NewFileNameText.Text.Contains(".xaml"))
					{
						NewFileNameText.Text += ".xaml";
					}
					File.WriteAllText(Path.Combine(CurrentSolution.SourceDirectory, NewFileNameText.Text), Constants.CreateNewXamlControlFile(CurrentSolution, NewFileNameText.Text));
					File.WriteAllText(Path.Combine(CurrentSolution.SourceDirectory, NewFileNameText.Text + ".cs"), Constants.CreateNewXamlCsFile(CurrentSolution, NewFileNameText.Text, true));
					CurrentSolution.AddProjectFile(NewFileNameText.Text, false);
					CurrentSolution.AddProjectFile(NewFileNameText.Text + ".cs");
					CurrentWorkspace.AddProjectFile(NewFileNameText.Text);
					CurrentWorkspace.AddProjectFile(NewFileNameText.Text + ".cs");
					CurrentSolution.Save();
					this.Close();
				}
				else if (SelectedFileType == NewFileType.Class)
				{
					if (!NewFileNameText.Text.Contains(".cs"))
					{
						NewFileNameText.Text += ".cs";
					}
					File.WriteAllText(Path.Combine(CurrentSolution.SourceDirectory, NewFileNameText.Text), Constants.CreateNewClass(CurrentSolution, NewFileNameText.Text));
					CurrentSolution.AddProjectFile(NewFileNameText.Text);
					CurrentWorkspace.AddProjectFile(NewFileNameText.Text);
					CurrentSolution.Save();
					this.Close();
				}
				else
				{
					MessageBox.Show("You must select a new file type.", "Need User Input", MessageBoxButton.OK);
				}
			}
			else
			{
				MessageBox.Show("You must select a new file type.", "Need User Input", MessageBoxButton.OK);
			}
		}
	}
}
