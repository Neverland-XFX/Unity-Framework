using GameLogic.Observables;
using UnityFramework;

namespace GameLogic.Model
{
    public class BattleModel : ObservableObject
    {
        private string _score;

        public string Score
        {
            get => _score;
            set => Set(ref _score, value);
        }

        public BattleModel()
        {
            GameEvent.AddEventListener<int>(ActorEventDefine.ScoreChange, OnScoreChange);
            // GameEvent.AddEventListener<int>(ActorEventDefine.GameOver, OnGameOver);
        }
        
        private void OnScoreChange(int obj)
        {
            Score = obj.ToString();
        }
    }
}