using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using static CSProjBuilder.Constants;
using System.IO;
using Newtonsoft.Json;

namespace CSProjBuilder
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public static MainWindow Current { get; set; }
		public MainWindow()
		{
			InitializeComponent();
			Current = this;
			LoadSettings();
		}

		private void LoadSettings()
		{
			if (!Directory.Exists(SettingsDirectory))
			{
				Directory.CreateDirectory(SettingsDirectory);
			}
			if (File.Exists(SettingsFilePath))
			{
				string settingsJson = File.ReadAllText(SettingsFilePath);
				if (!string.IsNullOrWhiteSpace(settingsJson))
				{
					Settings.CurrentSettings = JsonConvert.DeserializeObject<Settings>(settingsJson);
				}
				else
				{
					Settings.CurrentSettings = new Settings();
					File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(Settings.CurrentSettings));
				}
			}
			else
			{
				Settings.CurrentSettings = new Settings();
				Settings.Save();
			}

			if (string.IsNullOrWhiteSpace(Settings.CurrentSettings.MSBuildDirectoryPath) || !Settings.CurrentSettings.CompletedSetup || string.IsNullOrWhiteSpace(Settings.CurrentSettings.TextEditorExePath))
			{
				SetupWindow setup = new SetupWindow();
				setup.ShowDialog();
			}

			LoadRecentProjects();
		}

		private void LoadRecentProjects()
		{
			if (Settings.CurrentSettings.PreviouslyOpenedSolutionPaths == null)
			{
				Settings.CurrentSettings.PreviouslyOpenedSolutionPaths = new List<string>();
			}
			if (Settings.CurrentSettings.PreviouslyOpenedSolutionPaths.Count > 0)
			{
				foreach(string path in Settings.CurrentSettings.PreviouslyOpenedSolutionPaths)
				{
					string projJson = File.ReadAllText(path);
					CSProjSolution solution = JsonConvert.DeserializeObject<CSProjSolution>(projJson);
					if (solution != null)
					{
						//add to recent project list
						RecentSolutionControl control = new RecentSolutionControl(solution);
						RecentSolutionsStack.Children.Add(control);
					}
				}
			}
		}


		//Change MSBuild Location
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SetupWindow setup = new SetupWindow();
			setup.ShowDialog();
		}

		//WPF App
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			NewProjectWindow newProject = new NewProjectWindow(SolutionTypeEnum.WPF);
			newProject.ShowDialog();
		}
	}
}
