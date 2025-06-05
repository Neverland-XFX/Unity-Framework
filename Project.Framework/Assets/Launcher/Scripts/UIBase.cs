using UnityEngine;

namespace Launcher
{
    /// <summary>
    /// 热更UI基类。
    /// </summary>
    public class UIBase : MonoBehaviour
    {
        protected object Param;
        public virtual void OnEnter(object param)
        {
            Param = param;
        }
    }
}