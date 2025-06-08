using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UnityFramework
{
    public class TimerPool<T> where T : class, new()
    {
        private readonly Stack<T> _stack = new Stack<T>();
        private readonly Action<T> _reset;
        private readonly Func<T> _create;

        public TimerPool(Func<T> create = null, Action<T> reset = null)
        {
            _create = create ?? (() => new T());
            _reset = reset;
        }

        public T Get()
        {
            if (_stack.Count > 0)
                return _stack.Pop();
            return _create();
        }

        public void Release(T obj)
        {
            _reset?.Invoke(obj);
            _stack.Push(obj);
        }
    }

    public delegate void TimerHandler(params object[] args);


    internal class TimerModule : Module, IUpdateModule, ITimerModule
    {
        private class Timer
        {
            public int Id;
            public float Remaining;
            public TimerOptions Options;
            public TimerHandler Handler;
            public bool IsRunning;
        }

        private int _nextId = 0;
        private readonly Dictionary<int, Timer> _timers = new();
        private readonly Dictionary<bool, List<Timer>> _timerBuckets = new()
        {
            { false, new List<Timer>() },
            { true, new List<Timer>() }
        };
        private readonly TimerPool<Timer> _pool = new(
            create: () => new Timer(),
            reset: t =>
            {
                t.Handler = null;
                t.Options = default;
                t.IsRunning = false;
            });

        private int GenId()
        {
            int id = Interlocked.Increment(ref _nextId);
            if (id <= 0)
            {
                interlocked_id_reset:
                _nextId = 0;
                id = Interlocked.Increment(ref _nextId);
                if (id <= 0) goto interlocked_id_reset;
            }
            return id;
        }

        public int AddTimer(TimerHandler callback, float time, bool isLoop = false, bool isUnscaled = false, params object[] args)
            => AddTimer(callback, new TimerOptions(time, isLoop, isUnscaled, args));

        public int AddTimer(TimerHandler callback, TimerOptions options)
        {
            var t = _pool.Get();
            t.Id = GenId();
            t.Handler = callback;
            t.Options = options;
            t.Remaining = options.Delay;
            t.IsRunning = true;

            _timers[t.Id] = t;
            _timerBuckets[options.Unscaled].Add(t);
            return t.Id;
        }

        public void Pause(int id)
        {
            if (_timers.TryGetValue(id, out var t)) t.IsRunning = false;
        }

        public void Resume(int id)
        {
            if (_timers.TryGetValue(id, out var t)) t.IsRunning = true;
        }

        public bool IsRunning(int id)
            => _timers.TryGetValue(id, out var t) && t.IsRunning;

        public float GetLeftTime(int id)
            => _timers.TryGetValue(id, out var t) ? t.Remaining : 0f;

        public void Restart(int id)
        {
            if (_timers.TryGetValue(id, out var t))
                t.Remaining = t.Options.Delay;
        }

        public void ResetTimer(int id, TimerHandler callback, float time, bool isLoop = false, bool isUnscaled = false)
            => Reset(id, callback, new TimerOptions(time, isLoop, isUnscaled));

        public void ResetTimer(int id, float time, bool isLoop, bool isUnscaled)
            => Reset(id, null, new TimerOptions(time, isLoop, isUnscaled));

        private void Reset(int id, TimerHandler callback, TimerOptions opt)
        {
            if (_timers.TryGetValue(id, out var t))
            {
                bool changingScale = t.Options.Unscaled != opt.Unscaled;
                if (changingScale) _timerBuckets[t.Options.Unscaled].Remove(t);

                t.Handler = callback ?? t.Handler;
                t.Options = opt;
                t.Remaining = opt.Delay;
                t.IsRunning = true;

                if (changingScale)
                    _timerBuckets[opt.Unscaled].Add(t);
            }
        }

        public void RemoveTimer(int id)
        {
            if (_timers.Remove(id, out var t))
            {
                _timerBuckets[t.Options.Unscaled].Remove(t);
                _pool.Release(t);
            }
        }

        public void RemoveAllTimer()
        {
            foreach (var list in _timerBuckets.Values)
                list.Clear();
            _timers.Clear();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            UpdateTimers(false, elapseSeconds);
            UpdateTimers(true, realElapseSeconds);
        }

        private void UpdateTimers(bool unscaled, float delta)
        {
            var list = _timerBuckets[unscaled];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var t = list[i];
                if (!t.IsRunning) continue;

                t.Remaining -= delta;
                if (t.Remaining > 0f) continue;

                t.Handler?.Invoke(t.Options.Args);

                if (t.Options.Loop)
                {
                    t.Remaining += t.Options.Delay;
                }
                else
                {
                    _timers.Remove(t.Id);
                    // list.RemoveAt(i);
                    _pool.Release(t);
                }
            }
        }

        public override void OnInit() { }
        public override void Shutdown()
        {
            RemoveAllTimer();
            // 可关闭额外线程 / System.Timers
        }
    }
}
