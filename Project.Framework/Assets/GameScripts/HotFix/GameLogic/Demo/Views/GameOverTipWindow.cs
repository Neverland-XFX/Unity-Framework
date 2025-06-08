using GameLogic.Binding.Builder;
using GameLogic.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.UI)]
    class GameOverTipWindow : UIWindow
    {
        #region 脚本工具生成的代码
        private GameObject _goOverView;
        private Button _vmBtnRestart;
        private Button _vmBtnHome;
        private GameOverTipViewModel _gameOverTipViewModel;

        protected override void ScriptGenerator()
        {
            _goOverView = FindChild("m_goOverView").gameObject;
            _vmBtnRestart = FindChildComponent<Button>("m_goOverView/m_vmBtnRestart");
            _vmBtnHome = FindChildComponent<Button>("m_goOverView/m_vmBtnHome");
        }
        #endregion
        protected override void OnCreate()
        {
            base.OnCreate();
            _gameOverTipViewModel = new GameOverTipViewModel();
            BindingSet<GameOverTipWindow, GameOverTipViewModel> bindingSet = this.CreateBindingSet(_gameOverTipViewModel);
            bindingSet.Bind(_vmBtnRestart).From(v => v.onClick).To(vm => vm.RestartCommand);
            bindingSet.Bind(_vmBtnHome).From(v => v.onClick).To(vm => vm.HomeCommand);
            bindingSet.Build();
        }


    }
}