using HybridCLR.Editor;
using Obfuz4HybridCLR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityFramework.Editor
{
    public static class BuildCommand
    {
        [MenuItem("Build/CompileAndObfuscateAndCopyToStreamingAssets")]
        public static void CompileAndObfuscateAndCopyToStreamingAssets()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            ObfuscateUtil.ObfuscateHotUpdateAssemblies(target,"test");

            Directory.CreateDirectory(Application.streamingAssetsPath);

            string hotUpdateDllPath = $"{SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target)}";
            foreach (string assName in SettingsUtil.HotUpdateAssemblyNamesIncludePreserved)
            {
                string srcFile = $"{hotUpdateDllPath}/{assName}.dll";
                string dstFile = $"{Application.streamingAssetsPath}/{assName}.dll.bytes";
                File.Copy(srcFile, dstFile, true);
                Debug.Log($"[CompileAndObfuscate] Copy {srcFile} to {dstFile}");
            }
        }
    }   
}