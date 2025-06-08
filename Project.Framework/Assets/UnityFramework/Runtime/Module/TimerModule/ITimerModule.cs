namespace UnityFramework
{
    public interface ITimerModule
    {
        int AddTimer(TimerHandler callback, float time, bool isLoop = false, bool isUnscaled = false, params object[] args);

        int AddTimer(TimerHandler callback, TimerOptions options); // 新增：支持结构体配置方式

        void Pause(int timerId);
        void Resume(int timerId);
        bool IsRunning(int timerId);
        float GetLeftTime(int timerId);
        void Restart(int timerId);

        void ResetTimer(int timerId, TimerHandler callback, float time, bool isLoop = false, bool isUnscaled = false);
        void ResetTimer(int timerId, float time, bool isLoop, bool isUnscaled);

        void RemoveTimer(int timerId);
        void RemoveAllTimer();
    }    
    public struct TimerOptions
    {
        public float Delay;
        public bool Loop;
        public bool Unscaled;
        public object[] Args;

        public TimerOptions(float delay, bool loop = false, bool unscaled = false, object[] args = null)
        {
            Delay = delay;
            Loop = loop;
            Unscaled = unscaled;
            Args = args;
        }
    }
}