using Cysharp.Threading.Tasks;
using GameLogic.Binding.Builder;
using GameLogic.Binding.Contexts;
using GameLogic.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using UnityFramework;

namespace GameLogic
{
    [Window(UILayer.UI)]
    class BattleWindow : UIWindow
    {
        private BattleViewModel _battleViewModel;
        #region 脚本工具生成的代码

        private Text _vmTextScore;
        private GameObject _goOverView;
        private Button _vmBtnRestart;
        private Button _vmBtnHome;

        protected override void ScriptGenerator()
        {
            _vmTextScore = FindChildComponent<Text>("ScoreView/m_vmTextScore");
            _goOverView = FindChild("m_goOverView").gameObject;
            _vmBtnRestart = FindChildComponent<Button>("m_goOverView/m_vmBtnRestart");
            _vmBtnHome = FindChildComponent<Button>("m_goOverView/m_vmBtnHome");
        }

        #endregion

        protected override void RegisterEvent()
        {
            AddUIEvent(ActorEventDefine.GameOver, OnGameOver);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _battleViewModel = new BattleViewModel();
            BindingSet<BattleWindow, BattleViewModel> bindingSet = this.CreateBindingSet(_battleViewModel);
            bindingSet.Bind(_vmTextScore).From(v => v.text).To(vm => vm.Score).TwoWay();
            bindingSet.Bind(_vmBtnRestart).From(v => v.onClick).To(vm => vm.RestartCommand);
            bindingSet.Bind(_vmBtnHome).From(v => v.onClick).To(vm => vm.HomeCommand);
            bindingSet.Build();
        }

        protected override void OnRefresh()
        {
            _vmTextScore.text = "Score : 0";
            _goOverView.SetActive(false);
        }

        #region 事件


        private async UniTaskVoid OnClickHomeBtn()
        {
            await UniTask.Yield();
            // yield return YooAssets.LoadSceneAsync("scene_home");	
            // yield return UniWindow.OpenWindowAsync<UIHomeWindow>("UIHome");
            //
            // // 释放资源
            // var package = YooAssets.GetPackage("DefaultPackage");
            // package.UnloadUnusedAssets();
        }

        #endregion

        private void OnScoreChange(int currentScores)
        {
            _vmTextScore.text = $"Score : {currentScores}";
        }

        private void OnGameOver()
        {
            _goOverView.SetActive(true);
        }
    }
}