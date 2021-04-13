using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace CSProjBuilder
{
	public enum SolutionTypeEnum
	{
		None = 0,
		Console = 1,
		WPF = 2,
	}
	public class CSProjSolution
	{
		public string SolutionName { get; set; }
		public string WorkingDirectoryPath { get; set; }
		public string CSProjFileName { get; set; }
		public string CSProjSolutionFileName { get; set; }
		public string IconFileName { get; set; }
		public SolutionTypeEnum SolutionType { get; set; }
		public DateTime LastModified { get; set; }

		[JsonIgnore]
		public string IconFilePath { get { return System.IO.Path.Combine(WorkingDirectoryPath, "img", IconFileName); } }
		[JsonIgnore]
		public string CSProjFilePath { get { return System.IO.Path.Combine(WorkingDirectoryPath, CSProjFileName); } }
		[JsonIgnore]
		public string CSProjSolutionFilePath { get { return System.IO.Path.Combine(WorkingDirectoryPath, CSProjSolutionFileName); } }
		[JsonIgnore]
		public string SourceDirectory { get { return System.IO.Path.Combine(WorkingDirectoryPath, "source"); } }

		public List<string> Sources { get; set; }
		public List<string> References { get; set; }

		public CSProjSolution()
		{

		}

		public CSProjSolution(string projectName, string projectDirectory, bool? createProjectFolder, SolutionTypeEnum solutionType)
		{
			SolutionName = projectName;
			projectName.Replace(" ", "");
			if (createProjectFolder == true)
			{
				projectDirectory = Path.Combine(projectDirectory, projectName);
				Directory.CreateDirectory(projectDirectory);
			}
			WorkingDirectoryPath = projectDirectory;
			Directory.CreateDirectory(Path.Combine(WorkingDirectoryPath, "lib"));
			Directory.CreateDirectory(Path.Combine(WorkingDirectoryPath, "img"));
			Directory.CreateDirectory(Path.Combine(WorkingDirectoryPath, "source"));
			Directory.CreateDirectory(Path.Combine(WorkingDirectoryPath, "debug"));
			Directory.CreateDirectory(Path.Combine(WorkingDirectoryPath, "release"));
			SolutionType = solutionType;
			CSProjFileName = projectName + ".csproj";
			CSProjSolutionFileName = projectName + ".cspsln";
			var projStream = File.CreateText(CSProjFilePath);
			projStream.Close();
			var solStream = File.CreateText(CSProjSolutionFilePath);
			solStream.Close();
			if (solutionType == SolutionTypeEnum.WPF)
			{
				Directory.CreateDirectory(Path.Combine(WorkingDirectoryPath, "PreviewTemp"));
				CreateDefaultWPFEnvironment();
			}
			Save();
		}

		private void CreateDefaultWPFEnvironment()
		{
			File.WriteAllText(Path.Combine(WorkingDirectoryPath, "App.xaml"), Constants.DefaultAppXamlFile.Replace("$ProjectName$", SolutionName));
			File.WriteAllText(Path.Combine(WorkingDirectoryPath, "App.xaml.cs"), Constants.DefaultAppXamlCsFile.Replace("$ProjectName$", SolutionName));
			File.WriteAllText(Path.Combine(SourceDirectory, "MainWindow.xaml"), Constants.CreateNewXamlWindowFile(this, "MainWindow.xaml"));
			File.WriteAllText(Path.Combine(SourceDirectory, "MainWindow.xaml.cs"), Constants.CreateNewXamlCsFile(this, "MainWindow.xaml"));
			AddProjectFile("App.xaml", false);
			AddProjectFile("App.xaml.cs", false);
			AddProjectFile("MainWindow.xaml", false);
			AddProjectFile("MainWindow.xaml.cs");
		}

		public void Save()
		{
			string json = JsonConvert.SerializeObject(this);
			File.WriteAllText(CSProjSolutionFilePath, json);
		}

		public void AddProjectFile(string fileName, bool rebuildCsProj = true)
		{
			if (Sources == null)
			{
				Sources = new List<string>();
			}
			if (!Sources.Contains(fileName))
			{
				Sources.Add(fileName);
				if (rebuildCsProj)
				{
					RebuildCSProjFile();
				}
			}
		}

		internal void RebuildCSProjFile(bool isRelease = false)
		{
			if (Sources == null)
			{
				Sources = new List<string>();
			}
			if (References == null)
			{
				References = new List<string>();
			}
			if (!File.Exists(CSProjFilePath))
			{
				File.CreateText(CSProjFilePath);
			}
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "\t";
			XmlWriter csProj = XmlWriter.Create(CSProjFilePath, settings);
			csProj.WriteStartDocument();
			csProj.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
			csProj.WriteAttributeString("ToolsVersion", "4.0");
			csProj.WriteStartElement("Target");
			csProj.WriteAttributeString("Name", "Build");
			csProj.WriteStartElement("Csc");
			if (isRelease)
			{
				csProj.WriteAttributeString("OutputAssembly", "release\\" + SolutionName + ".exe");
			}
			else
			{
				csProj.WriteAttributeString("OutputAssembly", "debug\\" + SolutionName + ".exe");
			}
			csProj.WriteAttributeString("Sources", "@(Compile)");
			csProj.WriteAttributeString("References", "@(Reference)");

			//end csc
			csProj.WriteEndElement();

			//end target
			csProj.WriteEndElement();
			csProj.WriteStartElement("PropertyGroup");
			csProj.WriteElementString("OutputType", "WinExe");
			csProj.WriteElementString("OutputPath", "debug\\");
			csProj.WriteEndElement();
			if (IconFileName != null)
			{
				csProj.WriteStartElement("PropertyGroup");
				csProj.WriteElementString("ApplicationIcon", "img\\" + IconFileName);
				csProj.WriteEndElement();
			}
			csProj.WriteStartElement("ItemGroup");
			if (SolutionType == SolutionTypeEnum.WPF)
			{
				csProj.WriteStartElement("ApplicationDefinition");
				csProj.WriteAttributeString("Include", "App.xaml");
				csProj.WriteElementString("Generator", "MSBuild:Compile");
				csProj.WriteEndElement();
			}
			foreach(string s in Sources.FindAll(x=> !x.Contains("App.xaml")))
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
			foreach (string r in References)
			{
				csProj.WriteStartElement("Reference");
				csProj.WriteAttributeString("Include", "lib\\" + r);
				csProj.WriteEndElement();
			}
			WriteDefaultWindowsReferences(csProj, SolutionType == SolutionTypeEnum.WPF);

			//End ItemGroup
			csProj.WriteEndElement();

			csProj.WriteStartElement("Import");
			csProj.WriteAttributeString("Project", "$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
			csProj.WriteEndDocument();
			csProj.Close();
			
		}

		internal void AddReferenceFile(string fullFileName, bool rebuildCsProj = true)
		{
			if (References == null)
			{
				References = new List<string>();
			}
			string fileName = Path.GetFileName(fullFileName);
			References.Add(fileName);
			if (!File.Exists(Path.Combine(WorkingDirectoryPath, "lib", fileName)))
			{
				File.Copy(fullFileName, Path.Combine(WorkingDirectoryPath, "lib", fileName));
			}
			if (rebuildCsProj)
			{
				RebuildCSProjFile();
			}
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

		public void DeleteProjectFile(string fileName)
		{
			Sources.Remove(fileName);
			RebuildCSProjFile();
		}

	}
}
