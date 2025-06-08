using Cysharp.Threading.Tasks;
using GameLogic.Model;

namespace GameLogic.Services
{
    public enum BattleEventType
    {
        Update,
    }
    public class BattleEventArgs
    {
        public BattleEventArgs(BattleEventType battleEventType,BattleModel battleModel)
        {
            BattleEventType = battleEventType;
            BattleModel = battleModel;
        }

        public BattleEventType BattleEventType { get; set; }
        public BattleModel BattleModel { get; private set; }
    }

    public interface IBattleService
    {
        UniTask<BattleModel> Update(BattleModel battle);

        string GetScore();
    }
}