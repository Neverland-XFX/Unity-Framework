using GameLogic.Observables;
using UnityFramework;

namespace GameLogic.Model
{
    public class BattleModel : ObservableObject
    {
        private int _score;

        public int Score
        {
            get => _score;
            set => Set(ref _score, value);
        }

        public BattleModel()
        {
            GameEvent.AddEventListener<int>(ActorEventDefine.ScoreChange, OnScoreChange);
        }
        
        private void OnScoreChange(int obj)
        {
            Score = obj;
        }
    }
}