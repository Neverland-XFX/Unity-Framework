using Cysharp.Threading.Tasks;
using GameLogic.Model;
using GameLogic.Repositories;
using UnityFramework;

namespace GameLogic.Services
{
    public class BattleService : IBattleService
    {
        
        private IBattleRepository repository;

        public BattleService(IBattleRepository repository)
        {
            this.repository = repository;
        }

        public virtual async UniTask<BattleModel> Update(BattleModel battle)
        {
            await this.repository.Update(battle);
            GameEvent.Publish(new BattleEventArgs(BattleEventType.Update, battle));
            return battle;
        }

        public string GetScore()
        {
            var battleModelTask = repository.Get();
            var model = battleModelTask.GetAwaiter().GetResult();
            return model.Score;
        }
    }
}