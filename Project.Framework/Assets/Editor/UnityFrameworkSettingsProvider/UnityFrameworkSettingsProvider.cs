using UnityFramework;
using UnityEditor;  

public static class UnityFrameworkSettingsProvider  
{  
    [MenuItem("UnityFramework/Settings/UnityFramework UpdateSettings", priority = -1)]
    public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/UnityFramework/UpdateSettings");
    
    private const string SettingsPath = "Project/UnityFramework/UpdateSettings";  

    [SettingsProvider]  
    public static SettingsProvider CreateMySettingsProvider()  
    {  
        return new SettingsProvider(SettingsPath, SettingsScope.Project)  
        {  
            label = "UnityFramework/UpdateSettings",  
            guiHandler = (searchContext) =>  
            {  
                var settings = Settings.UpdateSetting;  
                var serializedObject = new SerializedObject(settings);  

                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectName"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HotUpdateAssemblies"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AOTMetaAssemblies"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LogicMainDllName"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AssemblyTextAssetExtension"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AssemblyTextAssetPath"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateStyle"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableUpdateData"));
                if (settings.EnableUpdateData)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ServerStateDatePath"));  
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ServerStateDataFileName"));  
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateDataPath"));  
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateDataFileName"));  
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ResDownLoadPath"));  
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FallbackResDownLoadPath"));  
                serializedObject.ApplyModifiedProperties();  
            },  
            keywords = new[] { "UnityFramework", "Settings", "Custom" }  
        };  
    }
}  
