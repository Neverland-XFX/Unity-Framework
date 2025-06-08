using GameLogic.Observables;
using UnityFramework;

namespace GameLogic.Model
{
    public class BattleModel : ObservableObject
    {
        private bool _isGameOver;
        private string _score;

        public bool IsGameOver
        {
            get => _isGameOver;
            set=>Set(ref _isGameOver,value);
        }
        public string Score
        {
            get => _score;
            set => Set(ref _score, value);
        }

        public BattleModel()
        {
            GameEvent.AddEventListener<int>(ActorEventDefine.ScoreChange, OnScoreChange);
            GameEvent.AddEventListener(ActorEventDefine.GameOver, OnGameOver);
        }

        private void OnGameOver()
        {
            IsGameOver = true;
        }

        private void OnScoreChange(int obj)
        {
            Score = obj.ToString();
        }
    }
}