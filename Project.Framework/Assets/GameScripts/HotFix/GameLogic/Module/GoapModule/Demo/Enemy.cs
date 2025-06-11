using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityFramework;

namespace GameLogic.GoapModule.Demo
{
    public class Enemy : MonoBehaviour
    {
        private IGoapAgent _agent;
        private GameObject _player;

        private void Start()
        {
            _player = GameObject.FindWithTag("Player");
            _agent = new EnemyAgent(transform,_player.transform);
            
            
            // 设置初始世界状态
            _agent.WorldState.Set("Hungry", true);
            _agent.WorldState.Set("HasPath", true);
            _agent.WorldState.Set("AtTarget", false);
            _agent.Blackboard.Set("Stamina",5.0f);
            
            // 找到目标
            var goal = _agent.GoalDirectory.GetAllGoals().First(g => g.GoalId == "SatisfyHunger");
            var plan = StartPlanner(goal);

            Log.Debug("------ GOAP 规划结果 ------");
            foreach (var action in plan)
                Log.Debug($"Action: {action.ActionId}");

            _agent.PlanRunner.Initialize(_agent.ActionExecutionManager,plan);
            _agent.PlanRunner.Start();

            _agent.PlanRunner.OnPlanCompleted += () =>
            {
                Log.Debug("当前plan已完成,进行下一个plan规划");
                var plan2 = StartPlanner(goal);

                Log.Debug("------ GOAP 规划结果 ------");
                foreach (var action in plan2)
                    Log.Debug($"Action: {action.ActionId}");
                _agent.PlanRunner.Initialize(_agent.ActionExecutionManager,plan);
                _agent.PlanRunner.Start();
            };
            _agent.PlanRunner.OnPlanInterrupted += () =>
            {
                Log.Debug($"当前plan已中断,重新进行plan规划");
                var plan3 = StartPlanner(goal);

                Log.Debug("------ GOAP 规划结果 ------");
                foreach (var action in plan3)
                    Log.Debug($"Action: {action.ActionId}");

                _agent.PlanRunner.Initialize(_agent.ActionExecutionManager,plan);
                _agent.PlanRunner.Start();
            };
        }

        private List<IActionTemplate> StartPlanner(IGoalTemplate goal)
        {
            // 规划
            var planner = new FastGoapPlanner();
            var plan = planner.BuildPlan(_agent, goal).ToList();
            return plan;
        }

        public IGoapAgent GetAgent()
        {
            return _agent;
        }

        private void Update()
        {
            _agent.Update(_player);
        }
    }
}