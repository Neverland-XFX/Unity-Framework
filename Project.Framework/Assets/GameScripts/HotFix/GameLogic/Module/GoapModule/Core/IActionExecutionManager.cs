using System.Collections.Generic;

namespace GameLogic.GoapModule
{

    public interface IActionExecutionManager
    {
        IActionInstance GetInstance(string actionId);
        IEnumerable<IActionInstance> GetAllInstances();
        void Update();
        // 你可以让它负责实例化/管理当前正在执行的action instance
        IActionInstance StartAction(string actionId,IActionTemplate template, IActionContext context);
        void StopAction(string actionId);
    }
}