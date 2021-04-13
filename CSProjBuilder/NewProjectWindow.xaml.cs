using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace CSProjBuilder
{
	/// <summary>
	/// Interaction logic for NewProjectWindow.xaml
	/// </summary>
	public partial class NewProjectWindow : Window
	{
		public SolutionTypeEnum SolutionType { get; set; }
		public NewProjectWindow()
		{
			InitializeComponent();
		}

		public NewProjectWindow(SolutionTypeEnum solutionType)
		{
			InitializeComponent();
			SolutionType = solutionType;
		}

		//Create
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (ProjectNameText.Text.Contains(" "))
			{
				MessageBox.Show("Project name can't contain spaces or special characters.", "Illegal Character(s)", MessageBoxButton.OK);
				return;
			}
			string projectDirectory = ProjectDirectoryText.Text;
			if (CreateFolderCheck.IsChecked == true)
			{
				projectDirectory = Path.Combine(projectDirectory, ProjectNameText.Text);
			}
			if (CreateFolderCheck.IsChecked == true && Directory.Exists(projectDirectory))
			{
				MessageBox.Show("A folder with the specified project name already exists.", "Already Exists", MessageBoxButton.OK);
				return;
			}
			CSProjSolution solution = new CSProjSolution(ProjectNameText.Text, ProjectDirectoryText.Text, CreateFolderCheck.IsChecked, SolutionType);
			WorkspaceWindow workspace = new WorkspaceWindow(solution);
			workspace.Show();
			this.Close();
			if (MainWindow.Current != null)
			{
				MainWindow.Current.Close();
			}
			Settings.CurrentSettings.PreviouslyOpenedSolutionPaths.Insert(0, solution.CSProjSolutionFilePath);
			Settings.Save();
		}

		//BrowseDirectory
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog fileDialog = new FolderBrowserDialog();
			if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				if (!string.IsNullOrWhiteSpace(fileDialog.SelectedPath))
				{
					ProjectDirectoryText.Text = fileDialog.SelectedPath;
				}
			}
		}
	}
}
