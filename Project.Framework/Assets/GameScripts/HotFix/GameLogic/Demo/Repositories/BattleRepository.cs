using System;
using System.Collections.Generic;
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

        public virtual UniTask<BattleModel> Update(BattleModel Battle)
        {
            _cache = Battle;
            return UniTask.FromResult(_cache);
        }
    }
}