using System.Collections.Generic;
using System.Linq;

namespace GameLogic.GoapModule
{
    public class FastGoapPlanner : IGoapPlanner
    {
        /// <summary>
        /// 逆向回溯+剪枝的GOAP Planner
        /// </summary>
        public IEnumerable<IActionTemplate> BuildPlan(
            IGoapAgent agent,
            IGoalTemplate goal)
        {
            var allActions = agent.ActionDirectory.GetAllHandlers().ToList();
            var worldState = agent.WorldState;
            var required = new HashSet<ICondition>(goal.SatisfiedStates);

            var visited = new HashSet<string>();
            var plan = new List<IActionTemplate>();
            bool success = BuildPlanSmart(
                allActions, worldState, required, plan, visited);

            if (success) plan.Reverse();
            return plan;
        }

        private bool BuildPlanSmart(
            List<IActionTemplate> actions,
            IGoapWorldState currentWorld,
            HashSet<ICondition> requiredGoals,
            List<IActionTemplate> plan,
            HashSet<string> visited)
        {
            if (requiredGoals.All(cond => cond.IsMet(null, currentWorld)))
                return true;

            // 剪枝
            var hash = currentWorld.ToHashString() + "--" + string.Join(";", requiredGoals.Select(c => c.Key));
            if (visited.Contains(hash))
                return false;
            visited.Add(hash);

            // 针对所有未满足的条件，找那些effects交集最多的action
            var scoredActions = actions
                .Select(a => new
                {
                    Action = a,
                    Covered = a.Effects.Count(e => requiredGoals.Any(req =>
                        req.Key == e.Key && Equals((e)?.Value, (req)?.Value)))
                })
                .Where(x => x.Covered > 0)
                .OrderByDescending(x => x.Covered)
                .ThenByDescending(x => x.Action.Priority)
                .ThenBy(x => x.Action.Cost)
                .ToList();

            foreach (var x in scoredActions)
            {
                var action = x.Action;
                // 一次解决多个条件
                var coveredConds = action.Effects
                    .Where(eff => requiredGoals.Any(req =>
                        req.Key == eff.Key && Equals((eff)?.Value, (req)?.Value)))
                    .ToList();

                var nextRequired = new HashSet<ICondition>(requiredGoals);
                foreach (var c in coveredConds)
                    nextRequired.RemoveWhere(cond => cond.Key == c.Key);

                foreach (var pre in action.Preconditions)
                    nextRequired.Add(pre);

                // 模拟action后的新world
                var simulated = currentWorld.Clone();
                foreach (var eff in action.Effects)
                    simulated.Set(eff.Key, (eff)?.Value);

                plan.Add(action);
                if (BuildPlanSmart(actions, simulated, nextRequired, plan, visited))
                    return true;
                plan.RemoveAt(plan.Count - 1); // 回溯
            }

            // 若所有都不行，失败回溯
            return false;
        }
    }
}