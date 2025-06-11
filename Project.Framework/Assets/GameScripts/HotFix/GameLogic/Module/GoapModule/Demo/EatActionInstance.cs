using Cysharp.Threading.Tasks;
using UnityFramework;

namespace GameLogic.GoapModule.Demo
{
    public class EatActionInstance : ActionInstanceBase
    {
        private bool _isEating;
        public EatActionInstance(IGoapAgent agent, IActionTemplate template, IActionContext context) : base(agent, template, context)
        {
        }
        
        
        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if(_isEating)
                return;
            _isEating = true;
            TestEat().Forget();
        }

        public async UniTask TestEat()
        {
            Log.Warning($"等待一秒当进食");
            await UniTask.Delay(1000);
            Complete();
        }
    }
}