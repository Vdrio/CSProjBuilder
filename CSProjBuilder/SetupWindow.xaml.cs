using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CSProjBuilder
{
	/// <summary>
	/// Interaction logic for SetupWindow.xaml
	/// </summary>
	public partial class SetupWindow : Window
	{
		public SetupWindow()
		{
			InitializeComponent();
			this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
			if (Settings.CurrentSettings.MSBuildDirectoryPath != null)
			{
				msBuildDirectoryPath = Settings.CurrentSettings.MSBuildDirectoryPath;
				BuildPathText.Text = Settings.CurrentSettings.MSBuildDirectoryPath;
			}
			if (Settings.CurrentSettings.TextEditorExePath != null)
			{
				NppPathText.Text = Settings.CurrentSettings.TextEditorExePath;
				nppExePath = Settings.CurrentSettings.TextEditorExePath;
			}
		}

		void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!Settings.CurrentSettings.CompletedSetup)
			{
				e.Cancel = true;
				MessageBox.Show("You must select the location of MSBuild.exe before using this application.", "Must Select MSBuild Path", MessageBoxButton.OK);
			}
		}

		string msBuildDirectoryPath = null;
		string nppExePath = null;
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog fileDialog = new FolderBrowserDialog();
			if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				if (!string.IsNullOrWhiteSpace(fileDialog.SelectedPath))
				{
					msBuildDirectoryPath = fileDialog.SelectedPath;
					BuildPathText.Text = msBuildDirectoryPath;
				}
			}
		}
		

		//Apply Button
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			if (msBuildDirectoryPath == null || nppExePath == null)
			{
				MessageBox.Show("You must select the directory that contains msbuild.exe and select the text editor exe path before using this application.", "Must Complete Setup", MessageBoxButton.OK);
			}
			else
			{
				Settings.CurrentSettings.MSBuildDirectoryPath = msBuildDirectoryPath;
				Settings.CurrentSettings.TextEditorExePath = nppExePath;
				Settings.CurrentSettings.CompletedSetup = true;
				Settings.Save();
				this.Close();
			}
		}

		//Notepad++ Browse
		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "Executables (*.exe) | *.exe";
			if (fileDialog.ShowDialog() == true)
			{
				nppExePath = fileDialog.FileName;
				NppPathText.Text = nppExePath;
			}

		}
	}
}
