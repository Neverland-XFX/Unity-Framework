using System.ComponentModel;
using Cysharp.Threading.Tasks;
using GameLogic.Commands;
// using GameLogic.GoapModule.Demo;
using GameLogic.Model;

namespace GameLogic.ViewModel
{
    public class GameOverTipViewModel : ViewModelBase
    {
        
        private SimpleCommand _homeCommand;
        private SimpleCommand _resetCommand;
        private GameOverTipModel _gameOverTipModel;
        
        public ICommand HomeCommand => _homeCommand;
        public ICommand RestartCommand => _resetCommand;

        public GameOverTipViewModel() 
        {
            _homeCommand = new SimpleCommand(GoHome);
            _resetCommand = new SimpleCommand(UniTask.UnityAction(Reset));
            _gameOverTipModel = new GameOverTipModel();
            _gameOverTipModel.PropertyChanged += OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }


        private async UniTaskVoid Reset()
        {
            await GameModule.Scene.LoadSceneAsync("scene_battle");
    
            BattleSystem.Instance.DestroyRoom();
            BattleSystem.Instance.LoadRoom().Forget();
            GameModule.UI.CloseUI<GameOverTipWindow>();
        }

        private void GoHome()
        {
            // RunDemoMain.GoapDemo.RunDemo();
        }
    }
}