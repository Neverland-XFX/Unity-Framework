using System.ComponentModel;
using GameLogic.Model;

namespace GameLogic.ViewModel
{
    
    public class BattleViewModel : ViewModelBase
    {
        private BattleModel _battleModel;
        public string Score
        {
            get => _battleModel.Score;
            set
            {
                _battleModel.Score = value;
                RaisePropertyChanged(nameof(Score));
            }
        }

        public bool IsGameOver
        {
            get => _battleModel.IsGameOver;
            set
            {
                _battleModel.IsGameOver = value;
                RaisePropertyChanged(nameof(IsGameOver));
            }
        }

        public BattleViewModel()
        {
            _battleModel = new BattleModel();
            _battleModel.PropertyChanged += OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsGameOver))
            {
                GameModule.UI.ShowUIAsync<GameOverTipWindow>();
            }
            RaisePropertyChanged(e.PropertyName);
        }

    }
}