using UnityEngine;
using UnityEngine.UI;
using System;

namespace Launcher
{
    public enum MessageShowType
    {
        None = 0,
        OneButton = 1,
        TwoButton = 2,
        ThreeButton = 3,
    }

    /// <summary>
    /// UI更新加载提示。
    /// </summary>
    public class UILoadTip : UIBase
    {
        public Button _btn_update;
        public Button _btn_ignore;
        public Button _btn_package;
        public Text _label_desc;

        public Action OnOk;
        public Action OnCancel;
        public MessageShowType Showtype = MessageShowType.None;

        void Start()
        {
            _btn_update.onClick.AddListener(OnGameUpdate);
            _btn_ignore.onClick.AddListener(OnGameIgnore);
            _btn_package.onClick.AddListener(OnInvoke);
        }

        public override void OnEnter(object data)
        {
            _btn_ignore.gameObject.SetActive(false);
            _btn_package.gameObject.SetActive(false);
            _btn_update.gameObject.SetActive(false);
            switch (Showtype)
            {
                case MessageShowType.OneButton:
                    _btn_update.gameObject.SetActive(true);
                    break;
                case MessageShowType.TwoButton:
                    _btn_update.gameObject.SetActive(true);
                    _btn_ignore.gameObject.SetActive(true);
                    break;
                case MessageShowType.ThreeButton:
                    _btn_ignore.gameObject.SetActive(true);
                    _btn_package.gameObject.SetActive(true);
                    _btn_package.gameObject.SetActive(true);
                    break;
            }

            _label_desc.text = data.ToString();
        }

        private void OnGameUpdate()
        {
            if (OnOk == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                _OnClose();
            }
        }

        private void OnGameIgnore()
        {
            if (OnCancel == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnCancel();
                _OnClose();
            }
        }

        private void OnInvoke()
        {
            if (OnOk == null)
            {
                _label_desc.text = "<color=#BA3026>该按钮不应该存在</color>";
            }
            else
            {
                OnOk();
                _OnClose();
            }
        }

        private void _OnClose()
        {
            LauncherMgr.Hide(UIDefine.UILoadTip);
        }
    }
}