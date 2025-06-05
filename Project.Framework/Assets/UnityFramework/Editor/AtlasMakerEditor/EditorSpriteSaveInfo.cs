namespace UnityFramework.Editor
{
#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.U2D;
    using UnityEngine;
    using UnityEngine.U2D;

    public static class EditorSpriteSaveInfo
    {
        private static readonly HashSet<string> _dirtyAtlasNames = new HashSet<string>();
        private static readonly Dictionary<string, List<string>> _atlasMap = new Dictionary<string, List<string>>();
        private static bool _initialized;

        private static AtlasConfiguration Config => AtlasConfiguration.Instance;

        static EditorSpriteSaveInfo()
        {
            EditorApplication.update += OnUpdate;
            Initialize();
        }

        private static void Initialize()
        {
            if (_initialized) return;

            ScanExistingSprites();
            _initialized = true;
        }

        public static void OnImportSprite(string assetPath)
        {
            if (!ShouldProcess(assetPath)) return;

            var atlasName = GetAtlasName(assetPath);
            if (string.IsNullOrEmpty(atlasName)) return;

            if (!_atlasMap.TryGetValue(atlasName, out var list))
            {
                list = new List<string>();
                _atlasMap[atlasName] = list;
            }

            if (!list.Contains(assetPath))
            {
                list.Add(assetPath);
                MarkDirty(atlasName);
                MarkParentAtlasesDirty(assetPath);
            }
        }

        public static void OnDeleteSprite(string assetPath)
        {
            if (!ShouldProcess(assetPath)) return;

            var atlasName = GetAtlasName(assetPath);
            if (string.IsNullOrEmpty(atlasName)) return;

            if (_atlasMap.TryGetValue(atlasName, out var list))
            {
                if (list.Remove(assetPath))
                {
                    MarkDirty(atlasName);
                    MarkParentAtlasesDirty(assetPath);
                }
            }
        }

        [MenuItem("Tools/图集工具/ForceGenerateAll")]
        public static void ForceGenerateAll()
        {
            _atlasMap.Clear();
            ScanExistingSprites();
            _dirtyAtlasNames.UnionWith(_atlasMap.Keys);
            ProcessDirtyAtlases(true);
        }

        public static void ClearCache()
        {
            _dirtyAtlasNames.Clear();
            _atlasMap.Clear();
            AssetDatabase.Refresh();
        }

        public static void MarkParentAtlasesDirty(string assetPath)
        {
            var currentPath = Path.GetDirectoryName(assetPath);
            var rootPath = Config.sourceAtlasRoot.Replace("\\", "/").TrimEnd('/');

            while (currentPath != null && currentPath.StartsWith(rootPath))
            {
                var parentAtlasName = GetAtlasNameForDirectory(currentPath);
                if (!string.IsNullOrEmpty(parentAtlasName))
                {
                    MarkDirty(parentAtlasName);
                }

                currentPath = Path.GetDirectoryName(currentPath);
            }
        }

        private static void OnUpdate()
        {
            if (_dirtyAtlasNames.Count > 0)
            {
                ProcessDirtyAtlases();
            }
        }

        private static void ProcessDirtyAtlases(bool force = false)
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var atlasName in _dirtyAtlasNames.ToList())
                {
                    if (force || ShouldUpdateAtlas(atlasName))
                    {
                        GenerateAtlas(atlasName);
                    }

                    _dirtyAtlasNames.Remove(atlasName);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void GenerateAtlas(string atlasName)
        {
            var outputPath = $"{Config.outputAtlasDir}/{atlasName}.spriteatlas";
            SpriteAtlasAsset spriteAtlasAsset = default;
            SpriteAtlas atlas = new SpriteAtlas();
            if (Config.enableV2)
            {
                outputPath = $"{Config.outputAtlasDir}/{atlasName}.spriteatlasv2";
                if (!File.Exists(outputPath))
                {
                    spriteAtlasAsset = new SpriteAtlasAsset();
                }
                else
                {
                    spriteAtlasAsset = SpriteAtlasAsset.Load(outputPath);
                    atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(outputPath);
                    if (atlas != null)
                    {
                        var olds = atlas.GetPackables();
                        if (olds != null) spriteAtlasAsset.Remove(olds);
                    }
                }
            }


            var sprites = LoadValidSprites(atlasName);
            EnsureOutputDirectory();
            if (sprites.Count == 0)
            {
                DeleteAtlas(outputPath);
                return;
            }

            if (Config.enableV2)
            {
                spriteAtlasAsset.Add(sprites.ToArray());
                SpriteAtlasAsset.Save(spriteAtlasAsset, outputPath);
                AssetDatabase.Refresh();

                EditorApplication.delayCall += () =>
                {
#if UNITY_2022_1_OR_NEWER
                    SpriteAtlasImporter sai = (SpriteAtlasImporter)AssetImporter.GetAtPath(outputPath);
                    ConfigureAtlasV2Settings(sai);
#else
                    ConfigureAtlasV2Settings(spriteAtlasAsset);
                    SpriteAtlasAsset.Save(spriteAtlasAsset, outputPath);
#endif
                    AssetDatabase.WriteImportSettingsIfDirty(outputPath);
                    AssetDatabase.Refresh();
                };
            }
            else
            {
                ConfigureAtlasSettings(atlas);
                atlas.Add(sprites.ToArray());
                atlas.SetIsVariant(false);
                AssetDatabase.CreateAsset(atlas, outputPath);
            }


            if (Config.enableLogging)
                Debug.Log($"Generated atlas: {atlasName} ({sprites.Count} sprites)");
        }

        private static List<Sprite> LoadValidSprites(string atlasName)
        {
            return _atlasMap[atlasName]
                .Where(File.Exists)
                .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
                .Where(s => s != null)
                .ToList();
        }


#if UNITY_2022_1_OR_NEWER
        private static void ConfigureAtlasV2Settings(SpriteAtlasImporter atlasImporter)
        {
            void SetPlatform(string platform, TextureImporterFormat format)
            {
                var settings = atlasImporter.GetPlatformSettings(platform);
                if (settings == null) return;
                ;
                settings.overridden = true;
                settings.format = format;
                settings.compressionQuality = Config.compressionQuality;
                atlasImporter.SetPlatformSettings(settings);
            }
            
            SetPlatform("Android", Config.androidFormat);
            SetPlatform("iPhone", Config.iosFormat);
            SetPlatform("WebGL", Config.webglFormat);
            
            var packingSettings = new SpriteAtlasPackingSettings
            {
                padding = Config.padding,
                enableRotation = Config.enableRotation,
                blockOffset = Config.blockOffset,
                enableTightPacking = Config.tightPacking,
                enableAlphaDilation = true
            };
            atlasImporter.packingSettings = packingSettings;
        }
#else
        private static void ConfigureAtlasV2Settings(SpriteAtlasAsset spriteAtlasAsset)
        {
            void SetPlatform(string platform, TextureImporterFormat format)
            {
                var settings = spriteAtlasAsset.GetPlatformSettings(platform);
                if (settings == null) return;
                ;
                settings.overridden = true;
                settings.format = format;
                settings.compressionQuality = Config.compressionQuality;
                spriteAtlasAsset.SetPlatformSettings(settings);
            }

            SetPlatform("Android", Config.androidFormat);
            SetPlatform("iPhone", Config.iosFormat);
            SetPlatform("WebGL", Config.webglFormat);

            var packingSettings = new SpriteAtlasPackingSettings
            {
                padding = Config.padding,
                enableRotation = Config.enableRotation,
                blockOffset = Config.blockOffset,
                enableTightPacking = Config.tightPacking,
                enableAlphaDilation = true
            };
            spriteAtlasAsset.SetPackingSettings(packingSettings);
        }
#endif


        private static void ConfigureAtlasSettings(SpriteAtlas atlas)
        {
            void SetPlatform(string platform, TextureImporterFormat format)
            {
                var settings = atlas.GetPlatformSettings(platform);
                settings.overridden = true;
                settings.format = format;
                settings.compressionQuality = Config.compressionQuality;
                atlas.SetPlatformSettings(settings);
            }

            SetPlatform("Android", Config.androidFormat);
            SetPlatform("iPhone", Config.iosFormat);
            SetPlatform("WebGL", Config.webglFormat);

            var packingSettings = new SpriteAtlasPackingSettings
            {
                padding = Config.padding,
                enableRotation = Config.enableRotation,
                blockOffset = Config.blockOffset,
                enableTightPacking = Config.tightPacking,
            };
            atlas.SetPackingSettings(packingSettings);
        }

        private static string GetAtlasName(string assetPath)
        {
            var normalizedPath = assetPath.Replace("\\", "/");
            var rootPath = Config.sourceAtlasRoot.Replace("\\", "/").TrimEnd('/');

            if (!normalizedPath.StartsWith(rootPath + "/")) return null;

            var relativePath = normalizedPath
                .Substring(rootPath.Length + 1)
                .Split('/');

            if (relativePath.Length < 2) return null;

            var directories = relativePath.Take(relativePath.Length - 1);
            var atlasNamePart = string.Join("_", directories);
            var rootFolderName = Path.GetFileName(rootPath);

            return $"{rootFolderName}_{atlasNamePart}";
        }

        private static bool ShouldProcess(string assetPath)
        {
            return IsImageFile(assetPath) && !IsExcluded(assetPath);
        }

        private static bool IsExcluded(string path)
        {
            return path.StartsWith(Config.excludeFolder) ||
                   Config.excludeKeywords.Any(k => path.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsImageFile(string path)
        {
            var ext = Path.GetExtension(path).ToLower();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
        }

        private static void MarkDirty(string atlasName)
        {
            _dirtyAtlasNames.Add(atlasName);
        }

        private static bool ShouldUpdateAtlas(string atlasName)
        {
            // var outputPath = $"{Config.outputAtlasDir}/{atlasName}.spriteatlas";
            return true;
        }

        private static DateTime GetLatestSpriteTime(string atlasName)
        {
            return _atlasMap[atlasName]
                .Select(p => new FileInfo(p).LastWriteTime)
                .DefaultIfEmpty()
                .Max();
        }

        private static void DeleteAtlas(string path)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                if (Config.enableLogging)
                    Debug.Log($"Deleted empty atlas: {Path.GetFileName(path)}");
            }
        }

        private static void EnsureOutputDirectory()
        {
            if (!Directory.Exists(Config.outputAtlasDir))
            {
                Directory.CreateDirectory(Config.outputAtlasDir);
                AssetDatabase.Refresh();
            }
        }

        private static void ScanExistingSprites()
        {
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { Config.sourceAtlasRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (ShouldProcess(path))
                {
                    OnImportSprite(path);
                }
            }
        }

        private static string GetAtlasNameForDirectory(string directoryPath)
        {
            var normalizedPath = directoryPath.Replace("\\", "/");
            var rootPath = Config.sourceAtlasRoot.Replace("\\", "/").TrimEnd('/');

            if (!normalizedPath.StartsWith(rootPath + "/")) return null;

            var relativePath = normalizedPath
                .Substring(rootPath.Length + 1)
                .Split('/');

            var atlasNamePart = string.Join("_", relativePath);
            var rootFolderName = Path.GetFileName(rootPath);

            return $"{rootFolderName}_{atlasNamePart}";
        }
    }

#endif
}