using GameLogic.Binding.Builder;
using GameLogic.ViewModel;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.UI)]
    class BattleWindow : UIWindow
    {
        private BattleViewModel _battleViewModel;
        #region 脚本工具生成的代码

        private Text _vmTextScore;

        protected override void ScriptGenerator()
        {
            _vmTextScore = FindChildComponent<Text>("ScoreView/m_vmTextScore");
        }

        #endregion

        protected override void OnCreate()
        {
            base.OnCreate();
            _battleViewModel = new BattleViewModel();
            BindingSet<BattleWindow, BattleViewModel> bindingSet = this.CreateBindingSet(_battleViewModel);
            bindingSet.Bind(_vmTextScore).From(v=>v.text).To(vm => vm.Score).TwoWay();
            bindingSet.Build();
        }

        protected override void OnRefresh()
        {
            _vmTextScore.text = "Score : 0";
        }

    }
}