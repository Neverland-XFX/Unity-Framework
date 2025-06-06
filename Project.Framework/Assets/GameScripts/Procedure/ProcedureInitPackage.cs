using System;
using Cysharp.Threading.Tasks;
using Launcher;
using UnityFramework;
using UnityEngine;
using YooAsset;
using ProcedureOwner = UnityFramework.IFsm<UnityFramework.IProcedureModule>;

namespace Procedure
{
    /// <summary>
    /// 流程 => 初始化Package。
    /// </summary>
    public class ProcedureInitPackage : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _procedureOwner = procedureOwner;

            if (_resourceModule.PlayMode == EPlayMode.HostPlayMode ||
                _resourceModule.PlayMode == EPlayMode.WebPlayMode)
            {
                CheckServerState(procedureOwner).Forget();
            }
            else
            {
                InitPackage(procedureOwner).Forget();                
            }
        }

        private async UniTaskVoid CheckServerState(ProcedureOwner procedureOwner)
        {
            var checkVersionUrl = Settings.UpdateSetting.GetServerStateDataPath();
            if (string.IsNullOrEmpty(checkVersionUrl))
            {
                Log.Error("LoadMgr.RequestVersion, remote url is empty or null");
                LauncherMgr.ShowMessageBox(LoadText.Instance.Label_RemoteUrlisNull, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    Application.Quit);
                return;
            }
            Log.Info("RequestUpdateData, proxy:" + checkVersionUrl);

            var updateDataStr = await Utility.Http.Get(checkVersionUrl);

            try
            {
                ServiceUpdateData serviceUpdateData = Utility.Json.ToObject<ServiceUpdateData>(updateDataStr);

                if (serviceUpdateData.ServerStatus == ServerStatus.Maintained)
                {
                    DisplayMaintainedTip(serviceUpdateData);   
                }
                else
                {
                    InitPackage(procedureOwner).Forget();
                }

            }
            catch (Exception e)
            {
                Log.Fatal(e);
                Application.Quit();
                throw;
            }
        }

        private void DisplayMaintainedTip(ServiceUpdateData data)
        {
            string content;
            Language language = Language.English;
            if (Utility.PlayerPrefs.HasSetting(Constant.Setting.Language))
            {
                try
                {
                    string languageString = Utility.PlayerPrefs.GetString(Constant.Setting.Language);
                    language = (Language)System.Enum.Parse(typeof(Language), languageString);
                }
                catch(System.Exception exception)
                {
                    Log.Error("Init language error, reason {0}",exception.ToString());
                }
            }
            switch (language)
            {
                case Language.ChineseSimplified:
                    content = data.ServerMaintainedContentChineseSimplified;
                    break;
                case Language.English:
                    content = data.ServerMaintainedContentEnglish;
                    break;
                case Language.ChineseTraditional:
                    content = data.ServerMaintainedContentChineseTraditional;
                    break;
                case Language.Russian:
                    content = data.ServerMaintainedContentRussian;
                    break;
                case Language.Japanese:
                    content = data.ServerMaintainedContentJapanese;
                    break;
                case Language.Korean:
                    content = data.ServerMaintainedContentKorean;
                    break;
                default:
                    content = data.ServerMaintainedContentEnglish;
                    break;
            }
            LauncherMgr.ShowMessageBox(content, MessageShowType.OneButton,
                LoadStyle.StyleEnum.Style_Default,
                Application.Quit);
        }
        
        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            if (_resourceModule.PlayMode == EPlayMode.HostPlayMode ||
                _resourceModule.PlayMode == EPlayMode.WebPlayMode)
            {
                if (Settings.UpdateSetting.EnableUpdateData)
                {
                    UpdateData updateData = await RequestUpdateData();
                    
                    if (updateData != null)
                    {
                        Settings.UpdateSetting.UpdateStyle = updateData.UpdateStyle;
                        Settings.UpdateSetting.UpdateNotice = updateData.UpdateNotice;
                        
                        if (!string.IsNullOrEmpty(updateData.HostServerURL))
                        {
                            Settings.UpdateSetting.ResDownLoadPath = updateData.HostServerURL;
                        }

                        if (!string.IsNullOrEmpty(updateData.FallbackHostServerURL))
                        {
                            Settings.UpdateSetting.FallbackResDownLoadPath = updateData.FallbackHostServerURL;
                        }
                    }
                }
            }
            try
            {
                var initializationOperation = await _resourceModule.InitPackage(_resourceModule.DefaultPackageName);

                if (initializationOperation.Status == EOperationStatus.Succeed)
                {
                    //热更新阶段文本初始化
                    LoadText.Instance.InitConfigData(null);

                    EPlayMode playMode = _resourceModule.PlayMode;

                    // 编辑器模式。
                    if (playMode == EPlayMode.EditorSimulateMode)
                    {
                        Log.Info("Editor resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    // 单机模式。
                    else if (playMode == EPlayMode.OfflinePlayMode)
                    {
                        Log.Info("Package resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    // 可更新模式。
                    else if (playMode == EPlayMode.HostPlayMode ||
                             playMode == EPlayMode.WebPlayMode)
                    {
                        // 打开启动UI。
                        LauncherMgr.Show(UIDefine.UILoadUpdate);

                        Log.Info("Updatable resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    else
                    {
                        Log.Error("UnKnow resource mode detected Please check???");
                    }
                }
                else
                {
                    // 打开启动UI。
                    LauncherMgr.Show(UIDefine.UILoadUpdate);

                    Log.Error($"{initializationOperation.Error}");

                    // 打开启动UI。
                    LauncherMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");

                    LauncherMgr.ShowMessageBox(
                        $"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{initializationOperation.Error}</color>",
                        MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_Retry
                        , () => { Retry(procedureOwner); }, UnityEngine.Application.Quit);
                }
            }
            catch (Exception e)
            {
                OnInitPackageFailed(procedureOwner, e.Message);
            }
        }

        private void OnInitPackageFailed(ProcedureOwner procedureOwner, string message)
        {
            // 打开启动UI。
            LauncherMgr.Show(UIDefine.UILoadUpdate);

            Log.Error($"{message}");

            // 打开启动UI。
            LauncherMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");

            if (message.Contains("PackageManifest_DefaultPackage.version Error : HTTP/1.1 404 Not Found"))
            {
                message = "请检查StreamingAssets/package/DefaultPackage/PackageManifest_DefaultPackage.version是否存在";
            }

            LauncherMgr.ShowMessageBox($"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{message}</color>", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () => { Retry(procedureOwner); },
                Application.Quit);
        }

        private void Retry(ProcedureOwner procedureOwner)
        {
            // 打开启动UI。
            LauncherMgr.Show(UIDefine.UILoadUpdate, $"重新初始化资源中...");

            InitPackage(procedureOwner).Forget();
        }
        
        
        /// <summary>
        /// 请求更新配置数据。
        /// </summary>
        private async UniTask<UpdateData> RequestUpdateData()
        {
            // 打开启动UI。
            LauncherMgr.Show(UIDefine.UILoadUpdate);

            var checkVersionUrl = Settings.UpdateSetting.GetUpdateDataPath();

            if (string.IsNullOrEmpty(checkVersionUrl))
            {
                Log.Error("LoadMgr.RequestVersion, remote url is empty or null");
                return null;
            }

            Log.Info("RequestUpdateData, proxy:" + checkVersionUrl);
            try
            {
                var updateDataStr = await Utility.Http.Get(checkVersionUrl);
                UpdateData updateData = Utility.Json.ToObject<UpdateData>(updateDataStr);
                return updateData;
            }
            catch (Exception e)
            {
                // 打开启动UI。
                LauncherMgr.ShowMessageBox("请求配置数据失败！点击确认重试", MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry
                    , () => { InitPackage(_procedureOwner).Forget(); }, Application.Quit);
                Log.Warning(e);
                return null;
            }
        }
    }
}