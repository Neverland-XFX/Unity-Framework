using Cysharp.Threading.Tasks;
using GameLogic.Model;

namespace GameLogic.Repositories
{
    public class BattleRepository : IBattleRepository
    {
        
        private BattleModel _cache = new() { Score = "0"};

        public virtual UniTask<BattleModel> Get()
        {
            return UniTask.FromResult(_cache);
        }

        public virtual UniTask<BattleModel> Update(BattleModel model)
        {
            _cache = model;
            return UniTask.FromResult(_cache);
        }
    }
}