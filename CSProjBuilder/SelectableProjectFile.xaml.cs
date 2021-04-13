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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Path = System.IO.Path;

namespace CSProjBuilder
{
	/// <summary>
	/// Interaction logic for SelectableProjectFile.xaml
	/// </summary>
	public partial class SelectableProjectFile : UserControl
	{
		public string FileName { get; set; }

		public CSProjSolution CurrentSolution { get; set; }
		public SelectableProjectFile()
		{
			InitializeComponent();
		}

		public SelectableProjectFile(CSProjSolution solution, string fileName, bool isXamlFile)
		{
			InitializeComponent();
			FileName = fileName;
			CurrentSolution = solution;
			if (isXamlFile)
			{
				XamlFileLabel.Content = fileName;
				NonXamlFileLabel.Visibility = Visibility.Collapsed;
				XamlFileLabel.Visibility = Visibility.Visible;
				PreviewButton.Visibility = Visibility.Visible;
			}
			else
			{
				NonXamlFileLabel.Content = fileName;
			}
		}


		//Open Button
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!FileName.Contains("App.xaml"))
			{
				Process.Start(Settings.CurrentSettings.TextEditorExePath, Path.Combine(CurrentSolution.SourceDirectory, FileName));
			}
			else
			{
				Process.Start(Settings.CurrentSettings.TextEditorExePath, Path.Combine(CurrentSolution.WorkingDirectoryPath, FileName));
			}
			
		}

		//Preview
		private void PreviewButton_Click(object sender, RoutedEventArgs e)
		{
			BuildPreviewCSProj();
		}

		private void BuildPreviewCSProj()
		{
			System.IO.DirectoryInfo di = new DirectoryInfo(Path.Combine(CurrentSolution.WorkingDirectoryPath, "PreviewTemp"));

			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete();
			}
			foreach (DirectoryInfo dir in di.GetDirectories())
			{
				foreach (FileInfo file in dir.GetFiles())
				{
					file.Delete();
				}
			}
			string workingDir = WorkspaceWindow.Current.CurrentSolution.WorkingDirectoryPath;
			string solName = WorkspaceWindow.Current.CurrentSolution.SolutionName;
			if (!Directory.Exists(Path.Combine(workingDir, "PreviewTemp", "source")))
			{
				Directory.CreateDirectory(Path.Combine(workingDir, "PreviewTemp", "source"));
			}
			File.WriteAllText(Path.Combine(workingDir, "PreviewTemp", "App.xaml"), Constants.DefaultAppXamlFile.Replace("$ProjectName$", solName));
			File.WriteAllText(Path.Combine(workingDir, "PreviewTemp", "App.xaml.cs"), Constants.DefaultAppXamlCsFile.Replace("$ProjectName$", solName));
			File.WriteAllText(Path.Combine(workingDir, "PreviewTemp", "source", "MainWindow.xaml"), File.ReadAllText(Path.Combine(workingDir, "source", FileName)));
			File.WriteAllText(Path.Combine(workingDir, "PreviewTemp", "source", "MainWindow.xaml.cs"), File.ReadAllText(Path.Combine(workingDir, "source", FileName + ".cs")));
			if (!File.Exists(WorkspaceWindow.Current.CurrentSolution.CSProjFilePath))
			{
				File.CreateText(WorkspaceWindow.Current.CurrentSolution.CSProjFilePath);
			}
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			XmlWriter csProj = XmlWriter.Create(Path.Combine(WorkspaceWindow.Current.CurrentSolution.WorkingDirectoryPath, "PreviewTemp", WorkspaceWindow.Current.CurrentSolution.CSProjFileName), settings);
			csProj.WriteStartDocument();
			csProj.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
			csProj.WriteAttributeString("ToolsVersion", "4.0");
			csProj.WriteStartElement("Target");
			csProj.WriteAttributeString("Name", "Build");
			csProj.WriteStartElement("Csc");

			csProj.WriteAttributeString("OutputAssembly", "debug\\" + WorkspaceWindow.Current.CurrentSolution.SolutionName + ".exe");
			
			csProj.WriteAttributeString("Sources", "@(Compile)");
			csProj.WriteAttributeString("References", "@(Reference)");

			csProj.WriteEndElement();
			csProj.WriteEndElement();
			csProj.WriteStartElement("ItemGroup");

				csProj.WriteStartElement("ApplicationDefinition");
				csProj.WriteAttributeString("Include", "App.xaml");
				csProj.WriteElementString("Generator", "MSBuild:Compile");
				csProj.WriteEndElement();
			
			foreach (string s in new List<string> { "MainWindow.xaml.cs" })
			{
				if (s.Contains(".xaml") && !s.Contains(".cs"))
				{
					continue;
				}

				csProj.WriteStartElement("Compile");
				csProj.WriteAttributeString("Include", "source\\" + s);
				if (s.Contains(".xaml.cs"))
				{
					csProj.WriteElementString("DependentUpon", "source\\" + s.Replace(".cs", ""));
				}
				csProj.WriteEndElement();
				if (s.Contains(".xaml.cs"))
				{
					csProj.WriteStartElement("Page");
					csProj.WriteAttributeString("Include", "source\\" + s.Replace(".cs", ""));
					csProj.WriteElementString("Generator", "MSBuild:Compile");
					csProj.WriteEndElement();
				}
			}
			csProj.WriteEndElement();

			csProj.WriteStartElement("ItemGroup");
			foreach (string r in WorkspaceWindow.Current.CurrentSolution.References)
			{
				csProj.WriteStartElement("Reference");
				csProj.WriteAttributeString("Include", "lib\\" + r);
				csProj.WriteEndElement();
			}
			WriteDefaultWindowsReferences(csProj, true);

			//End ItemGroup
			csProj.WriteEndElement();

			csProj.WriteStartElement("Import");
			csProj.WriteAttributeString("Project", "$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
			csProj.WriteEndDocument();
			csProj.Close();
			Build();
		}

		Process Build(bool run = true)
		{
			WorkspaceWindow.Current.OutputPanel.Children.Clear();
			var p = new Process();
			p.StartInfo.FileName = Settings.CurrentSettings.MSBuildPath;
			p.StartInfo.Arguments = Path.Combine(CurrentSolution.WorkingDirectoryPath, "PreviewTemp", CurrentSolution.CSProjFileName);
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardInput = false;
			p.OutputDataReceived += WorkspaceWindow.Current.P_OutputDataReceived;
			p.ErrorDataReceived += WorkspaceWindow.Current.P_ErrorDataReceived;
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
			var p = new Process();
			p.StartInfo.FileName = System.IO.Path.Combine(CurrentSolution.WorkingDirectoryPath, "PreviewTemp", "obj", "debug", CurrentSolution.SolutionName + ".exe");
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
		}

		private void WriteDefaultWindowsReferences(XmlWriter csProj, bool isWPF)
		{
			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.Data.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.Windows.Forms.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.Xml.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "Microsoft.CSharp.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.Core.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.Xml.Linq.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.Data.DataSetExtensions.dll"));
			csProj.WriteEndElement();

			csProj.WriteStartElement("Reference");
			csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "System.Xaml.dll"));
			csProj.WriteElementString("RequiredTargetFramework", "4.0");
			csProj.WriteEndElement();



			if (isWPF)
			{
				csProj.WriteStartElement("Reference");
				csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "WPF", "WindowsBase.dll"));
				csProj.WriteEndElement();

				csProj.WriteStartElement("Reference");
				csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "WPF", "PresentationCore.dll"));
				csProj.WriteEndElement();

				csProj.WriteStartElement("Reference");
				csProj.WriteAttributeString("Include", Path.Combine(Settings.CurrentSettings.MSBuildDirectoryPath, "WPF", "PresentationFramework.dll"));
				csProj.WriteEndElement();
			}
		}
	}
}
