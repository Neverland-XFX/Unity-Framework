using UnityEngine;

namespace GameLogic
{
    public static class UIUtils
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go == null)
            {
                return null;
            }
            var component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
    }
}