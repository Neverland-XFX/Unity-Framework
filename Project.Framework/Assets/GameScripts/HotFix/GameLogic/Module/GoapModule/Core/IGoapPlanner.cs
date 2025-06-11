using System.Collections.Generic;

namespace GameLogic.GoapModule
{

    public interface IGoapPlanner
    {
        IEnumerable<IActionTemplate> BuildPlan(
            IGoapAgent agent, 
            IGoalTemplate goal);
    }
}