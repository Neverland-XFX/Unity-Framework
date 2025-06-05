using System;
using System.Collections.Generic;
using UnityEditor;
using YooAsset.Editor;

namespace UnityFramework.Editor.Inspector
{
    [CustomEditor(typeof(ResourceModuleDriver))]
    internal sealed class ResourceModuleDriverInspector : GameFrameworkInspector
    {
        private static readonly string[] _playModeNames = new string[]
        {
            "EditorSimulateMode (编辑器下的模拟模式)",
            "OfflinePlayMode (单机模式)",
            "HostPlayMode (联机运行模式)",
            "WebPlayMode (WebGL运行模式)"
        };

        private SerializedProperty _playMode = null;
        private SerializedProperty _encryptionType = null;
        private SerializedProperty _updatableWhilePlaying = null;
        private SerializedProperty _milliseconds = null;
        private SerializedProperty _minUnloadUnusedAssetsInterval = null;
        private SerializedProperty _maxUnloadUnusedAssetsInterval = null;
        private  SerializedProperty _useSystemUnloadUnusedAssets = null;
        private SerializedProperty _assetAutoReleaseInterval = null;
        private SerializedProperty _assetCapacity = null;
        private SerializedProperty _assetExpireTime = null;
        private SerializedProperty _assetPriority = null;
        private SerializedProperty _downloadingMaxNum = null;
        private SerializedProperty _failedTryAgain = null;
        private SerializedProperty _packageName = null;
        private int _playModeIndex = 0;

        private int _packageNameIndex = 0;
        private string[] _packageNames;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ResourceModuleDriver t = (ResourceModuleDriver)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("Play Mode", t.PlayMode);
                }
                else
                {
                    int selectedIndex = EditorGUILayout.Popup("Play Mode", _playModeIndex, _playModeNames);
                    if (selectedIndex != _playModeIndex)
                    {
                        _playModeIndex = selectedIndex;
                        _playMode.enumValueIndex = selectedIndex;
                    }
                }
                
                EditorGUILayout.PropertyField(_encryptionType);
            }
            EditorGUILayout.PropertyField(_updatableWhilePlaying);
            
            EditorGUI.EndDisabledGroup();

            _packageNames = GetBuildPackageNames().ToArray();
            _packageNameIndex = Array.IndexOf(_packageNames, _packageName.stringValue);
            if (_packageNameIndex < 0)
            {
                _packageNameIndex = 0;
            }
            _packageNameIndex = EditorGUILayout.Popup("Package Name", _packageNameIndex, _packageNames);
            if (_packageName.stringValue != _packageNames[_packageNameIndex])
            {
                _packageName.stringValue = _packageNames[_packageNameIndex];
            }

            int milliseconds = EditorGUILayout.DelayedIntField("Milliseconds", _milliseconds.intValue);
            if (milliseconds != _milliseconds.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.milliseconds = milliseconds;
                }
                else
                {
                    _milliseconds.longValue = milliseconds;
                }
            }

            EditorGUILayout.PropertyField(_useSystemUnloadUnusedAssets);
            
            float minUnloadUnusedAssetsInterval =
                EditorGUILayout.Slider("Min Unload Unused Assets Interval", _minUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(minUnloadUnusedAssetsInterval - _minUnloadUnusedAssetsInterval.floatValue) > 0.01f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MinUnloadUnusedAssetsInterval = minUnloadUnusedAssetsInterval;
                }
                else
                {
                    _minUnloadUnusedAssetsInterval.floatValue = minUnloadUnusedAssetsInterval;
                }
            }

            float maxUnloadUnusedAssetsInterval =
                EditorGUILayout.Slider("Max Unload Unused Assets Interval", _maxUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(maxUnloadUnusedAssetsInterval - _maxUnloadUnusedAssetsInterval.floatValue) > 0.01f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MaxUnloadUnusedAssetsInterval = maxUnloadUnusedAssetsInterval;
                }
                else
                {
                    _maxUnloadUnusedAssetsInterval.floatValue = maxUnloadUnusedAssetsInterval;
                }
            }

            float downloadingMaxNum = EditorGUILayout.Slider("Max Downloading Num", _downloadingMaxNum.intValue, 1f, 48f);
            if (Math.Abs(downloadingMaxNum - _downloadingMaxNum.intValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.DownloadingMaxNum = (int)downloadingMaxNum;
                }
                else
                {
                    _downloadingMaxNum.intValue = (int)downloadingMaxNum;
                }
            }

            float failedTryAgain = EditorGUILayout.Slider("Max FailedTryAgain Count", _failedTryAgain.intValue, 1f, 48f);
            if (Math.Abs(failedTryAgain - _failedTryAgain.intValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FailedTryAgain = (int)failedTryAgain;
                }
                else
                {
                    _failedTryAgain.intValue = (int)failedTryAgain;
                }
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            {
                float assetAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Asset Auto Release Interval", _assetAutoReleaseInterval.floatValue);
                if (Math.Abs(assetAutoReleaseInterval - _assetAutoReleaseInterval.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetAutoReleaseInterval = assetAutoReleaseInterval;
                    }
                    else
                    {
                        _assetAutoReleaseInterval.floatValue = assetAutoReleaseInterval;
                    }
                }

                int assetCapacity = EditorGUILayout.DelayedIntField("Asset Capacity", _assetCapacity.intValue);
                if (assetCapacity != _assetCapacity.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetCapacity = assetCapacity;
                    }
                    else
                    {
                        _assetCapacity.intValue = assetCapacity;
                    }
                }

                float assetExpireTime = EditorGUILayout.DelayedFloatField("Asset Expire Time", _assetExpireTime.floatValue);
                if (Math.Abs(assetExpireTime - _assetExpireTime.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetExpireTime = assetExpireTime;
                    }
                    else
                    {
                        _assetExpireTime.floatValue = assetExpireTime;
                    }
                }

                int assetPriority = EditorGUILayout.DelayedIntField("Asset Priority", _assetPriority.intValue);
                if (assetPriority != _assetPriority.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetPriority = assetPriority;
                    }
                    else
                    {
                        _assetPriority.intValue = assetPriority;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Unload Unused Assets",
                    Utility.Text.Format("{0:F2} / {1:F2}", t.LastUnloadUnusedAssetsOperationElapseSeconds, t.MaxUnloadUnusedAssetsInterval));
                EditorGUILayout.LabelField("Applicable Game Version", t.ApplicableGameVersion ?? "<Unknwon>");
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            _playMode = serializedObject.FindProperty("playMode");
            _encryptionType = serializedObject.FindProperty("encryptionType");
            _updatableWhilePlaying = serializedObject.FindProperty("updatableWhilePlaying");
            _milliseconds = serializedObject.FindProperty("milliseconds");
            _minUnloadUnusedAssetsInterval = serializedObject.FindProperty("minUnloadUnusedAssetsInterval");
            _maxUnloadUnusedAssetsInterval = serializedObject.FindProperty("maxUnloadUnusedAssetsInterval");
            _useSystemUnloadUnusedAssets = serializedObject.FindProperty("useSystemUnloadUnusedAssets");
            _assetAutoReleaseInterval = serializedObject.FindProperty("assetAutoReleaseInterval");
            _assetCapacity = serializedObject.FindProperty("assetCapacity");
            _assetExpireTime = serializedObject.FindProperty("assetExpireTime");
            _assetPriority = serializedObject.FindProperty("assetPriority");
            _downloadingMaxNum = serializedObject.FindProperty("downloadingMaxNum");
            _failedTryAgain = serializedObject.FindProperty("failedTryAgain");
            _packageName = serializedObject.FindProperty("packageName");

            RefreshModes();
            RefreshTypeNames();
        }

        private void RefreshModes()
        {
            _playModeIndex = _playMode.enumValueIndex > 0 ? _playMode.enumValueIndex : 0;
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 获取构建包名称列表，用于下拉可选择
        /// </summary>
        /// <returns></returns>
        private List<string> GetBuildPackageNames()
        {
            List<string> result = new List<string>();
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }
            return result;
        }
    }
}