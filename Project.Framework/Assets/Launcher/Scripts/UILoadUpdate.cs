using UnityEngine;
using UnityEngine.UI;

namespace Launcher
{
    /// <summary>
    /// UI更新界面。
    /// </summary>
    public class UILoadUpdate : UIBase
    {
        [SerializeField]
        public Button _btn_clear;

        [SerializeField]
        public Scrollbar _obj_progress;

        [SerializeField]
        public Text _label_desc;

        [SerializeField]
        public Text _label_appid;

        [SerializeField]
        public Text _label_resid;

        public virtual void Start()
        {
            _btn_clear.onClick.AddListener(OnClear);
            _btn_clear.gameObject.SetActive(true);
            OnUpdateUIProgress(0f);
        }

        public override void OnEnter(object param)
        {
            if (param == null)
            {
                return;
            }

            base.OnEnter(param);
            _label_desc.text = param.ToString();
        }

        internal void OnRefreshVersion(string appId, string resId)
        {
            _label_appid.text = string.Format(LoadText.Instance.Label_App_id, appId);
            _label_resid.text = string.Format(LoadText.Instance.Label_Res_id, resId);
        }

        /// <summary>
        /// 清空本地缓存
        /// </summary>
        public virtual void OnClear()
        {
            LauncherMgr.ShowMessageBox(LoadText.Instance.Label_Clear_Comfirm, MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Clear,
                () =>
                {
                    // GameModule.Resource.ClearUnusedCacheFilesAsync();
                    Application.Quit();
                }, () => { });
        }

        /// <summary>
        /// 下载进度更新。
        /// </summary>
        /// <param name="progress">当前进度。</param>
        internal virtual void OnUpdateUIProgress(float progress)
        {
            _obj_progress.gameObject.SetActive(true);

            _obj_progress.size = progress;
        }
    }
}