using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic.Model;

namespace GameLogic.Repositories
{
    public interface IBattleRepository
    {
        UniTask<BattleModel> Get ();

        UniTask<BattleModel> Update (BattleModel account);
    }
}