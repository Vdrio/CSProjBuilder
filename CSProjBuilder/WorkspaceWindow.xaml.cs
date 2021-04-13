using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace CSProjBuilder
{
	/// <summary>
	/// Interaction logic for WorkspaceWindow.xaml
	/// </summary>
	public partial class WorkspaceWindow : Window
	{
		public static WorkspaceWindow Current { get; set; }

		public CSProjSolution CurrentSolution { get; set; }
		public WorkspaceWindow()
		{
			InitializeComponent();
			Current = this;
		}

		public WorkspaceWindow(CSProjSolution solution)
		{
			InitializeComponent();
			Current = this;
			CurrentSolution = solution;
			ProjectNameLabel.Content = solution.SolutionType.ToString() + "- " + solution.SolutionName;
			if (!string.IsNullOrWhiteSpace(solution.IconFileName))
			{
				IconNameText.Text = solution.IconFileName;
				IconPreviewImage.Source = new BitmapImage(new Uri(Path.Combine(CurrentSolution.WorkingDirectoryPath, "img", solution.IconFileName)));
			}
			foreach(string s in solution.Sources)
			{
				AddProjectFile(s);
			}
		}

		internal void AddProjectFile(string text)
		{
			ProjectFilesPanel.Children.Add(new SelectableProjectFile(CurrentSolution, text, text.Contains(".xaml") && !text.Contains(".cs")));
		}

		internal void AddProjectReference(string text)
		{
			ProjectReferencesPanel.Children.Add(new SelectableProjectFile(CurrentSolution, text, text.Contains(".xaml") && !text.Contains(".cs")));
		}

		//New Project File
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			AddNewWindow addNew = new AddNewWindow(CurrentSolution, this);
			addNew.ShowDialog();
		}

		//Build
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			Build();
			//p.WaitForExit(3000);
		}

		Process Build(bool run = false)
		{
			OutputPanel.Children.Clear();
			var p = new Process();
			p.StartInfo.FileName = Settings.CurrentSettings.MSBuildPath;
			p.StartInfo.Arguments = CurrentSolution.CSProjFilePath;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardInput = false;
			p.OutputDataReceived += P_OutputDataReceived;
			p.ErrorDataReceived += P_ErrorDataReceived;
			p.EnableRaisingEvents = true;
			p.StartInfo.UseShellExecute = false;
			if (run)
			{
				p.Exited += P_Exited;
			}
			p.Start();
			p.BeginErrorReadLine();
			p.BeginOutputReadLine();
			return p;
		}

		private void P_Exited(object sender, EventArgs e)
		{
			Run();
		}

		void Run()
		{
			var p = new Process();
			p.StartInfo.FileName = System.IO.Path.Combine(CurrentSolution.WorkingDirectoryPath, "obj", "debug", CurrentSolution.SolutionName + ".exe");
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
		}

		internal void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				TextBox block = new TextBox();
				block.TextWrapping = TextWrapping.Wrap;
				block.AcceptsReturn = true;
				block.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
				block.AcceptsTab = true;
				block.BorderThickness = new Thickness(0);
				block.Text = e.Data;
				block.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
				block.IsReadOnly = true;
				SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				block.Foreground = brush;
				OutputPanel.Children.Add(block);
			}));
		}

		internal void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(new Action(() =>
			{
				TextBox block = new TextBox();
				block.TextWrapping = TextWrapping.Wrap;
				block.AcceptsReturn = true;
				block.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
				block.AcceptsTab = true;
				block.BorderThickness = new Thickness(0);
				block.Text = e.Data;
				block.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
				block.IsReadOnly = true;
				SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(229, 229, 229));
				block.Foreground = brush;
				OutputPanel.Children.Add(block);
			}));
		}

		//Build And Run
		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			var p = Build(true);
		}


		//Add Reference
		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Libraries (*.dll) | *.dll";
			if (fileDialog.ShowDialog() == true)
			{
				CurrentSolution.AddReferenceFile(fileDialog.FileName);
			}
		}

		//Browse for project icon
		private void Button_Click_4(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Icons (*.ico) | *.ico";
			if (fileDialog.ShowDialog() == true)
			{
				if (!File.Exists(Path.Combine(CurrentSolution.WorkingDirectoryPath, "img", Path.GetFileName(fileDialog.FileName))))
				{
					File.Copy(fileDialog.FileName, Path.Combine(CurrentSolution.WorkingDirectoryPath, "img", Path.GetFileName(fileDialog.FileName)));
				}
				CurrentSolution.IconFileName = Path.GetFileName(fileDialog.FileName);
				IconNameText.Text = Path.GetFileName(fileDialog.FileName);
				IconPreviewImage.Source = new BitmapImage(new Uri(Path.Combine(CurrentSolution.WorkingDirectoryPath, "img", Path.GetFileName(fileDialog.FileName))));
				CurrentSolution.RebuildCSProjFile();
				CurrentSolution.Save();
			}
		}
	}
}
