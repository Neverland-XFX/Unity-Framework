using Cysharp.Threading.Tasks;
using GameLogic.Binding;
using GameLogic.Binding.Services;
using GameLogic.Commands;
using GameLogic.Model;
using GameLogic.Prefs;
using GameLogic.Services;

namespace GameLogic.ViewModel
{
    public class BattleViewModel : ViewModelBase
    {
        private SimpleCommand _homeCommand;
        private SimpleCommand _resetCommand;
        private string _score;
        private BattleModel _battleModel;

        
        public ICommand HomeCommand => this._homeCommand;
        public ICommand RestartCommand => this._resetCommand;
        public string Score
        {
            get => this._score;
            set => this.Set(ref this._score, value);
        }
        public BattleModel BattleModel => this._battleModel;

        public BattleViewModel() 
        {
            _homeCommand = new SimpleCommand(GoHome);
            _resetCommand = new SimpleCommand(UniTask.UnityAction(Reset));
            _battleModel = new BattleModel();
            _battleModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(Score))
                {
                    IServiceContainer container = GameApp.ApplicationContext.GetContainer();
                    var obj = container.Resolve(nameof(IBattleService));
                    var temp = obj as IBattleService;
                    Score = temp?.GetScore().ToString();
                }
            };
        }

        private async UniTaskVoid Reset()
        {
            await GameModule.Scene.LoadSceneAsync("scene_battle");
    
            BattleSystem.Instance.DestroyRoom();
            BattleSystem.Instance.LoadRoom().Forget();
        }

        private void GoHome()
        {
            
        }
    }
}