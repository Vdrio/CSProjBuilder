using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static CSProjBuilder.Constants;

namespace CSProjBuilder
{
	public class Settings
	{
		//Variable that is default to false so that it is only true once setup is complete
		public bool CompletedSetup { get; set; } = false;

		//Contains a list of strings that represent the file paths of the recently opened solutions
		public List<string> PreviouslyOpenedSolutionPaths { get; set; } = new List<string>();

		//The path that lets the program know where msbuild.exe and the default .net dlls
		public string MSBuildDirectoryPath { get; set; }

		public string MSBuildPath { get { return Path.Combine(MSBuildDirectoryPath, "msbuild.exe"); } set { } }


		public static Settings CurrentSettings { get; set; }
		public string TextEditorExePath { get; set; }

		public static void Save()
		{
			try
			{
				File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(CurrentSettings));
			}
			catch(Exception ex)
			{

			}
		}
	}
}
