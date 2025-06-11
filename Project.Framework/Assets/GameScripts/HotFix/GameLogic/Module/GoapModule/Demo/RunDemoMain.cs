using System.Linq;
using UnityEngine;
using UnityFramework;

namespace GameLogic.GoapModule.Demo
{
    public class RunDemoMain
    {
        public static class GoapDemo
        {
            public static void RunDemo(GameObject go)
            {
                var enemy = go.AddComponent<Enemy>();
                var agent = enemy.GetAgent();
            }
        }
    }
}