﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Localization
{
	public partial class LocalizationEditor
	{
		enum eLocalSpreadsheeet { CSV, XLS, XLSX, NONE }

		void OnGUI_Spreadsheet_Local()
		{
			GUILayout.Space(10);
			GUILayout.BeginVertical();

				GUILayout.BeginHorizontal();
					GUILayout.Label ("File:", GUILayout.ExpandWidth(false));

					mProp_Spreadsheet_LocalFileName.stringValue = EditorGUILayout.TextField(mProp_Spreadsheet_LocalFileName.stringValue);
					/*if (GUILayout.Button("...", "toolbarbutton", GUILayout.ExpandWidth(false)))
					{
						string sFileName = mProp_Spreadsheet_LocalFileName.stringValue;

						string sPath = string.Empty;
						try {
						sPath = System.IO.Path.GetDirectoryName(sFileName);
						}
						catch( System.Exception e){}

						if (string.IsNullOrEmpty(sPath))
							sPath = Application.dataPath + "/";

						sFileName = System.IO.Path.GetFileName(sFileName);
						if (string.IsNullOrEmpty(sFileName))
							sFileName = "Localization.csv";

						string FullFileName = EditorUtility.SaveFilePanel("Select CSV File", sPath, sFileName, "csv");
						//string FullFileName = EditorUtility.OpenFilePanel("Select CSV,  XLS or XLSX File", sFileName, "csv;*.xls;*.xlsx");

						if (!string.IsNullOrEmpty(FullFileName))
						{
							Prop_LocalFileName.stringValue = TryMakingPathRelativeToProject(FullFileName);
						}
					}*/
				GUILayout.EndHorizontal();

				//--[ Find current extension ]---------------
				eLocalSpreadsheeet CurrentExtension = eLocalSpreadsheeet.NONE;
				//string FileNameLower = Prop_LocalFileName.stringValue.ToLower();
				/*if (FileNameLower.EndsWith(".csv"))  */CurrentExtension = eLocalSpreadsheeet.CSV;
			/*if (FileNameLower.EndsWith(".xls"))  CurrentExtension = eLocalSpreadsheeet.XLS;
			    if (FileNameLower.EndsWith(".xlsx")) CurrentExtension = eLocalSpreadsheeet.XLSX;*/

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					switch (CurrentExtension)
					{
						case eLocalSpreadsheeet.NONE :
						case eLocalSpreadsheeet.CSV  : 
								{
									string 	FileTypesDesc = "Select or Drag any file of the following types:\n\n";
											FileTypesDesc+= "*.csv  (Comma Separated Values)\n";
											FileTypesDesc+= "*.txt  (CSV file renamed as txt)\n";
											//FileTypesDesc+= "\n*.xls  (Excel 97-2003)";
											//FileTypesDesc+= "\n*.xlsx (Excel Open XML format)";
									EditorGUILayout.HelpBox(FileTypesDesc, MessageType.None);
								}
								break;
						case eLocalSpreadsheeet.XLS 	: EditorGUILayout.HelpBox("Excel 97-2003", MessageType.None); break;
						case eLocalSpreadsheeet.XLSX 	: EditorGUILayout.HelpBox("Excel Open XML format", MessageType.None); break;
					}
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			//--[ Allow Dragging files ]-----------------
			if (GUILayoutUtility.GetLastRect().Contains (UnityEngine.Event.current.mousePosition) && IsValidDraggedLoadSpreadsheet())
			{
				if (UnityEngine.Event.current.type == EventType.DragUpdated)
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
				
				if (UnityEngine.Event.current.type == EventType.DragPerform)
				{
					mProp_Spreadsheet_LocalFileName.stringValue = TryMakingPathRelativeToProject( DragAndDrop.paths[0] );
					DragAndDrop.AcceptDrag();
					UnityEngine.Event.current.Use();
				}
			}

			GUILayout.Space(10);

			OnGUI_Spreadsheet_Local_ImportExport( CurrentExtension, mProp_Spreadsheet_LocalFileName.stringValue );

			//if (Application.platform == RuntimePlatform.OSXEditor)

			//-- CSV Separator ----------------
			GUI.changed = false;
			var CSV_Separator = mProp_Spreadsheet_LocalCSVSeparator.stringValue;
			if (string.IsNullOrEmpty (CSV_Separator))
				CSV_Separator = ",";

			GUILayout.Space(10);
			GUILayout.BeginVertical("Box");
				GUILayout.BeginHorizontal();
					GUILayout.Label("Separator:");
					GUILayout.FlexibleSpace();

					if (GUILayout.Toggle(CSV_Separator==",", "Comma(,)") && CSV_Separator!=",")
						CSV_Separator = ",";

					GUILayout.FlexibleSpace();
				
					if (GUILayout.Toggle(CSV_Separator==";", "Semicolon(;)") && CSV_Separator!=";")
						CSV_Separator = ";";

					GUILayout.FlexibleSpace();
				
					if (GUILayout.Toggle(CSV_Separator=="\t", "TAB(\\t)") && CSV_Separator!="\t")
						CSV_Separator = "\t";

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					//--[ Encoding ]---------------
					var encodings = Encoding.GetEncodings ().OrderBy(e=>e.Name).ToArray();
					var encodingNames = encodings.Select(e=>e.Name).ToArray();

					int idx = Array.IndexOf (encodingNames, mProp_Spreadsheet_LocalCSVEncoding.stringValue);
					if (idx == -1)
						idx = Array.IndexOf (encodingNames, "utf-8");
					EditorGUIUtility.labelWidth = 80;

					idx = EditorGUILayout.Popup ("Encoding:", idx, encodingNames);
					if (GUILayout.Button("Default", GUILayout.ExpandWidth(false)))
				    	idx = Array.IndexOf (encodingNames, "utf-8");
            
					if (idx>=0 && mProp_Spreadsheet_LocalCSVEncoding.stringValue != encodings [idx].Name)
						mProp_Spreadsheet_LocalCSVEncoding.stringValue = encodings [idx].Name;
				GUILayout.EndHorizontal();
			    
			GUILayout.EndVertical();

			if (GUI.changed)
			{
				mProp_Spreadsheet_LocalCSVSeparator.stringValue = CSV_Separator;
			}

			GUILayout.Space(10);
			EditorGUILayout.HelpBox("On some Mac OS, there is a Unity Bug that makes the IDE crash when selecting a CSV file in the Open/Save File Dialog.\nJust by clicking the file, unity tries to preview the content and crashes.\n\nIf any of your the team members use Mac, its adviced to import/export the CSV Files with TXT extension.", MessageType.Warning);
			GUILayout.Space(10);

			OnGUI_ShowMsg();
		}

		bool IsValidDraggedLoadSpreadsheet()
		{
			if (DragAndDrop.paths==null || DragAndDrop.paths.Length!=1)
				return false;

			string sPath = DragAndDrop.paths[0].ToLower();
			if (sPath.EndsWith(".csv")) return true;
			if (sPath.EndsWith(".txt")) return true;
			//if (sPath.EndsWith(".xls")) return true;
			//if (sPath.EndsWith(".xlsx")) return true;

			/*int iChar = sPath.LastIndexOfAny( "/\\.".ToCharArray() );
			if (iChar<0 || sPath[iChar]!='.')
				return true;
			return false;*/
			return false;
		}

		string TryMakingPathRelativeToProject( string FileName )
		{
			string ProjectPath = Application.dataPath.ToLower();
			string FileNameLower = FileName.ToLower();

			if (FileNameLower.StartsWith(ProjectPath))
				FileName = FileName.Substring(ProjectPath.Length+1);
			else
			if (FileNameLower.StartsWith("assets/"))
				FileName = FileName.Substring("assets/".Length);
			return FileName;
		}

		void OnGUI_Spreadsheet_Local_ImportExport( eLocalSpreadsheeet CurrentExtension, string File )
		{
			GUI.enabled = CurrentExtension!=eLocalSpreadsheeet.NONE;

			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			GUI.backgroundColor = Color.Lerp(Color.gray, Color.white, 0.5f);
			eSpreadsheetUpdateMode Mode = SynchronizationButtons("Import");
			if ( Mode!= eSpreadsheetUpdateMode.None)
				Import_Local(File, CurrentExtension, Mode);

			GUILayout.FlexibleSpace();
			
			GUI.backgroundColor = Color.Lerp(Color.gray, Color.white, 0.5f);
			Mode = SynchronizationButtons("Export", true);
			if ( Mode != eSpreadsheetUpdateMode.None)
				Export_Local(File, CurrentExtension, Mode);

			GUILayout.Space(10);
			GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                GUILayout.BeginVertical();
                EditorGUIUtility.labelWidth += 10;
                EditorGUILayout.PropertyField(mProp_Spreadsheet_SpecializationAsRows, new GUIContent("Show Specializations as Rows", "true: Make each specialization a separate row (e.g. Term[VR]..., Term[Touch]....\nfalse: Merge specializations into same cell separated by [i2s_XXX]"));
                EditorGUILayout.PropertyField(mProp_Spreadsheet_SortRows, new GUIContent("Sort Rows", "true: Sort each term by its name....\nfalse: Keep the terms order"));
                EditorGUIUtility.labelWidth -= 10;
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

			
			GUI.enabled = true;
		}

		void Import_Local( string File, eLocalSpreadsheeet CurrentExtension, eSpreadsheetUpdateMode UpdateMode )
		{
			try
			{
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				ClearErrors();
				
				if (string.IsNullOrEmpty(File))
					File = Application.dataPath + "/Localization.csv";
				else
					if (!Path.IsPathRooted(File))
						File = string.Concat(Application.dataPath, "/", File);

				// On Mac there is an issue with previewing CSV files, so its forced to only TXT
				if (Application.platform == RuntimePlatform.OSXEditor)
					File = EditorUtility.OpenFilePanel("Select a CSV file renamed as TXT", File, "txt");
				else
					File = EditorUtility.OpenFilePanel("Select a CSV file or a CSV file renamed as TXT", File, "csv;*.txt");
				//File = EditorUtility.OpenFilePanel("Select CSV,  XLS or XLSX File", File, "csv;*.xls;*.xlsx");
				if (!string.IsNullOrEmpty(File))
				{
					mLanguageSource.Spreadsheet_LocalFileName = TryMakingPathRelativeToProject(File);
					switch (CurrentExtension)
					{
						case eLocalSpreadsheeet.CSV		: Import_CSV(File, UpdateMode); break;
					}
					ParseTerms(true, false, true);
					EditorUtility.SetDirty (target);
					AssetDatabase.SaveAssets();
				}
			}
			catch (Exception ex) 
			{ 
				ShowError("Unable to import file");
				Debug.LogError(ex.Message); 
			}
		}

		void Import_CSV( string FileName, eSpreadsheetUpdateMode UpdateMode )
		{
            LanguageSourceData source = GetSourceData();
            var encoding = Encoding.GetEncoding (mProp_Spreadsheet_LocalCSVEncoding.stringValue);
			if (encoding == null)
				encoding = Encoding.UTF8;
			string CSVstring = LocalizationReader.ReadCSVfile (FileName, encoding);

			char Separator = mProp_Spreadsheet_LocalCSVSeparator.stringValue.Length>0 ? mProp_Spreadsheet_LocalCSVSeparator.stringValue[0] : ',';
			string sError = source.Import_CSV( string.Empty, CSVstring, UpdateMode, Separator);
			if (!string.IsNullOrEmpty(sError))
				ShowError(sError);

			mSelectedCategories = source.GetCategories();
		}

		void Export_Local( string File, eLocalSpreadsheeet CurrentExtension, eSpreadsheetUpdateMode UpdateMode )
		{
			try
			{
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
				ClearErrors();
				
				string sPath = string.Empty;
				if (!Path.IsPathRooted(File))
					File = string.Concat(Application.dataPath, "/", File);
				
				try {
					sPath = Path.GetDirectoryName(File);
				}
				catch( Exception){}
				
				if (string.IsNullOrEmpty(sPath))
					sPath = Application.dataPath + "/";
				
				File = Path.GetFileName(File);
				if (string.IsNullOrEmpty(File))
					File = "Localization.csv";
				
				if (Application.platform == RuntimePlatform.OSXEditor)
					File = EditorUtility.SaveFilePanel("Select a CSV file renamed as TXT", sPath, File, "txt");
				else
					File = EditorUtility.SaveFilePanel("Select a CSV or TXT file", sPath, File, "csv;*.txt");
				if (!string.IsNullOrEmpty(File))
				{
					mLanguageSource.Spreadsheet_LocalFileName = TryMakingPathRelativeToProject(File);

					char Separator = mProp_Spreadsheet_LocalCSVSeparator.stringValue.Length>0 ? mProp_Spreadsheet_LocalCSVSeparator.stringValue[0] : ',';
					var encoding = Encoding.GetEncoding (mProp_Spreadsheet_LocalCSVEncoding.stringValue);
					if (encoding == null)
						encoding = Encoding.UTF8;

					switch (CurrentExtension)
					{
						case eLocalSpreadsheeet.CSV : Export_CSV(File, UpdateMode, Separator, encoding); break;
					}
				}
			}
			catch (Exception)
			{
				ShowError("Unable to export file\nCheck it is not READ-ONLY and that\nits not opened in an external viewer");
			}
		}

		public void Export_CSV( string FileName, eSpreadsheetUpdateMode UpdateMode, char Separator, Encoding encoding )
		{
            LanguageSourceData source = GetSourceData();

            string CSVstring = source.Export_CSV(null, Separator, mProp_Spreadsheet_SpecializationAsRows.boolValue, mProp_Spreadsheet_SortRows.boolValue);
			File.WriteAllText (FileName, CSVstring, encoding);
		}
	}
}