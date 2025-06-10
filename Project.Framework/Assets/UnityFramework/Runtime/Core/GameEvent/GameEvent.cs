using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace UnityFramework
{
    /// <summary>
    /// 游戏全局事件类。
    /// </summary>
    public class GameEvent
    {
        /// <summary>
        /// 全局事件管理器。
        /// </summary>
        private static readonly EventMgr _eventMgr = new EventMgr();
        
        /// <summary>
        /// 全局事件管理器。
        /// </summary>
        public static EventMgr EventMgr => _eventMgr;

        private static readonly ConcurrentDictionary<Type, SubjectBase> Notifiers = new ConcurrentDictionary<Type, SubjectBase>();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, SubjectBase>> ChannelNotifiers = new ConcurrentDictionary<string, ConcurrentDictionary<Type, SubjectBase>>();

        #region 消息分发
        /// <summary>
        /// Subscribe a message.
        /// </summary>
        /// <param name="type">The type of message that the recipient subscribes for.</param>
        /// <param name="action">The action that will be executed when a message of type T is sent.</param>
        /// <returns>Disposable object that can be used to unsubscribe the message from the messenger.
        /// if the disposable object is disposed,the message is automatically unsubscribed.</returns>
        public static ISubscription<object> Subscribe(Type type, Action<object> action)
        {
            if (!Notifiers.TryGetValue(type, out var notifier))
            {
                notifier = new Subject<object>();
                if (!Notifiers.TryAdd(type, notifier))
                    Notifiers.TryGetValue(type, out notifier);
            }
            return (notifier as Subject<object>)?.Subscribe(action);
        }

        /// <summary>
        /// Subscribe a message.
        /// </summary>
        /// <typeparam name="T">The type of message that the recipient subscribes for.</typeparam>
        /// <param name="action">The action that will be executed when a message of type T is sent.</param>
        /// <returns>Disposable object that can be used to unsubscribe the message from the messenger.
        /// if the disposable object is disposed,the message is automatically unsubscribed.</returns>
        public static ISubscription<T> Subscribe<T>(Action<T> action)
        {
            Type type = typeof(T);
            if (!Notifiers.TryGetValue(type, out var notifier))
            {
                notifier = new Subject<T>();
                if (!Notifiers.TryAdd(type, notifier))
                    Notifiers.TryGetValue(type, out notifier);
            }
            return (notifier as Subject<T>)?.Subscribe(action);
        }

        /// <summary>
        /// Subscribe a message.
        /// </summary>
        /// <param name="channel">A name for a messaging channel.If a recipient subscribes
        /// using a channel, and a sender sends a message using the same channel, then this
        /// message will be delivered to the recipient. Other recipients who did not
        /// use a channel when subscribing (or who used a different channel) will not
        /// get the message. </param>
        /// <param name="type">The type of message that the recipient subscribes for.</param>
        /// <param name="action">The action that will be executed when a message of type T is sent.</param>
        /// <returns>Disposable object that can be used to unsubscribe the message from the messenger.
        /// if the disposable object is disposed,the message is automatically unsubscribed.</returns>
        public static ISubscription<object> Subscribe(string channel, Type type, Action<object> action)
        {
            if (!ChannelNotifiers.TryGetValue(channel, out var dict))
            {
                dict = new ConcurrentDictionary<Type, SubjectBase>();
                if (!ChannelNotifiers.TryAdd(channel, dict))
                    ChannelNotifiers.TryGetValue(channel, out dict);
            }

            if (!dict.TryGetValue(type, out var notifier))
            {
                notifier = new Subject<object>();
                if (!dict.TryAdd(type, notifier))
                    dict.TryGetValue(type, out notifier);
            }
            return (notifier as Subject<object>)?.Subscribe(action);
        }
        public static ISubscription<T> Subscribe<T>(string channel, Action<T> action)
        {
            if (!ChannelNotifiers.TryGetValue(channel, out var dict))
            {
                dict = new ConcurrentDictionary<Type, SubjectBase>();
                if (!ChannelNotifiers.TryAdd(channel, dict))
                    ChannelNotifiers.TryGetValue(channel, out dict);
            }

            if (!dict.TryGetValue(typeof(T), out var notifier))
            {
                notifier = new Subject<T>();
                if (!dict.TryAdd(typeof(T), notifier))
                    dict.TryGetValue(typeof(T), out notifier);
            }
            return (notifier as Subject<T>)?.Subscribe(action);
        }
        
        /// <summary>
        /// Publish a message to subscribed recipients. 
        /// </summary>
        /// <param name="message"></param>
        public static void Publish(object message)
        {
            Publish<object>(message);
        }

        public static void Publish<T>(T message)
        {
            if (message == null || Notifiers.Count <= 0)
                return;

            Type messageType = message.GetType();
            foreach (var kv in Notifiers)
            {
                if (kv.Key.IsAssignableFrom(messageType))
                    kv.Value.Publish(message);
            }
        }


        /// <summary>
        /// Publish a message to subscribed recipients. 
        /// </summary>
        /// <param name="channel">A name for a messaging channel.If a recipient subscribes
        /// using a channel, and a sender sends a message using the same channel, then this
        /// message will be delivered to the recipient. Other recipients who did not
        /// use a channel when subscribing (or who used a different channel) will not
        /// get the message. </param>
        /// <param name="message">The message to send to subscribed recipients.</param>
        public static void Publish(string channel, object message)
        {
            Publish<object>(channel, message);
        }

        /// <summary>
        /// Publish a message to subscribed recipients. 
        /// </summary>
        /// <typeparam name="T">The type of message that will be sent.</typeparam>
        /// <param name="channel">A name for a messaging channel.If a recipient subscribes
        /// using a channel, and a sender sends a message using the same channel, then this
        /// message will be delivered to the recipient. Other recipients who did not
        /// use a channel when subscribing (or who used a different channel) will not
        /// get the message. </param>
        /// <param name="message">The message to send to subscribed recipients.</param>
        public static void Publish<T>(string channel, T message)
        {
            if (string.IsNullOrEmpty(channel) || message == null)
                return;

            ConcurrentDictionary<Type, SubjectBase> dict = null;
            if (!ChannelNotifiers.TryGetValue(channel, out dict) || dict.Count <= 0)
                return;

            Type messageType = message.GetType();
            foreach (var kv in dict)
            {
                if (kv.Key.IsAssignableFrom(messageType))
                    kv.Value.Publish(message);
            }
        }
        #endregion
        
        #region 细分的注册接口

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件Handler。</param>
        /// <returns>是否监听成功。</returns>
        public static bool AddEventListener(int eventType, Action handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1>(int eventType, Action<TArg1> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2>(int eventType, Action<TArg1, TArg2> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2, TArg3>(int eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4>(int eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        public static void RemoveEventListener(int eventType, Action handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public static void RemoveEventListener<TArg1>(int eventType, Action<TArg1> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2>(int eventType, Action<TArg1, TArg2> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2, TArg3>(int eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4>(int eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        public static void RemoveEventListener(int eventType, Delegate handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        //----------------------------string Event----------------------------//
        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <returns></returns>
        public static bool AddEventListener(string eventType, Action handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1>(string eventType, Action<TArg1> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2>(string eventType, Action<TArg1, TArg2> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2, TArg3>(string eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4>(string eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <returns></returns>
        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(string eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            return _eventMgr.Dispatcher.AddEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        public static void RemoveEventListener(string eventType, Action handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public static void RemoveEventListener<TArg1>(string eventType, Action<TArg1> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2>(string eventType, Action<TArg1, TArg2> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2, TArg3>(string eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4>(string eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(string eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        public static void RemoveEventListener(string eventType, Delegate handler)
        {
            _eventMgr.Dispatcher.RemoveEventListener(RuntimeId.ToRuntimeId(eventType), handler);
        }

        #endregion

        #region 分发消息接口

        public static TArg1 Get<TArg1>()
        {
            return _eventMgr.GetInterface<TArg1>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        public static void Send(int eventType)
        {
            _eventMgr.Dispatcher.Send(eventType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public static void Send<TArg1>(int eventType, TArg1 arg1)
        {
            _eventMgr.Dispatcher.Send(eventType, arg1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public static void Send<TArg1, TArg2>(int eventType, TArg1 arg1, TArg2 arg2)
        {
            _eventMgr.Dispatcher.Send(eventType, arg1, arg2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public static void Send<TArg1, TArg2, TArg3>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            _eventMgr.Dispatcher.Send(eventType, arg1, arg2, arg3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public static void Send<TArg1, TArg2, TArg3, TArg4>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            _eventMgr.Dispatcher.Send(eventType, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public static void Send<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            _eventMgr.Dispatcher.Send(eventType, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        public static void Send(int eventType, Delegate handler)
        {
            _eventMgr.Dispatcher.Send(eventType, handler);
        }

        //-------------------------------string Send-------------------------------//
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        public static void Send(string eventType)
        {
            _eventMgr.Dispatcher.Send(RuntimeId.ToRuntimeId(eventType));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        public static void Send<TArg1>(string eventType, TArg1 arg1)
        {
            _eventMgr.Dispatcher.Send(RuntimeId.ToRuntimeId(eventType), arg1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        public static void Send<TArg1, TArg2>(string eventType, TArg1 arg1, TArg2 arg2)
        {
            _eventMgr.Dispatcher.Send(RuntimeId.ToRuntimeId(eventType), arg1, arg2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        public static void Send<TArg1, TArg2, TArg3>(string eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            _eventMgr.Dispatcher.Send(RuntimeId.ToRuntimeId(eventType), arg1, arg2, arg3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        public static void Send<TArg1, TArg2, TArg3, TArg4>(string eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            _eventMgr.Dispatcher.Send(RuntimeId.ToRuntimeId(eventType), arg1, arg2, arg3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        public static void Send<TArg1, TArg2, TArg3, TArg4, TArg5>(string eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            _eventMgr.Dispatcher.Send(RuntimeId.ToRuntimeId(eventType), arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <param name="arg5">事件参数5。</param>
        /// <param name="arg6">事件参数6。</param>
        /// <typeparam name="TArg1">事件参数1类型。</typeparam>
        /// <typeparam name="TArg2">事件参数2类型。</typeparam>
        /// <typeparam name="TArg3">事件参数3类型。</typeparam>
        /// <typeparam name="TArg4">事件参数4类型。</typeparam>
        /// <typeparam name="TArg5">事件参数5类型。</typeparam>
        /// <typeparam name="TArg6">事件参数6类型。</typeparam>
        public static void Send<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
        {
            _eventMgr.Dispatcher.Send(eventType, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件处理回调。</param>
        public static void Send(string eventType, Delegate handler)
        {
            _eventMgr.Dispatcher.Send(RuntimeId.ToRuntimeId(eventType), handler);
        }

        #endregion

        /// <summary>
        /// 清除事件。
        /// </summary>
        public static void Shutdown()
        {
            _eventMgr.Init();
        }
    }
    
    
    public abstract class SubjectBase
    {
        public abstract void Publish(object message);
    }

    public class Subject<T> : SubjectBase
    {
        private readonly ConcurrentDictionary<string, WeakReference<Subscription>> _subscriptions = new ConcurrentDictionary<string, WeakReference<Subscription>>();
        public bool IsEmpty() { return _subscriptions.Count <= 0; }

        public override void Publish(object message)
        {
            this.Publish((T)message);
        }

        public void Publish(T message)
        {
            if (_subscriptions.Count <= 0)
                return;

            foreach (var kv in _subscriptions)
            {
                Subscription subscription;
                kv.Value.TryGetTarget(out subscription);
                if (subscription != null)
                    subscription.Publish(message);
                else
                    _subscriptions.TryRemove(kv.Key, out _);
            }
        }

        public ISubscription<T> Subscribe(Action<T> action)
        {
            return new Subscription(this, action);
        }

        void Add(Subscription subscription)
        {
            var reference = new WeakReference<Subscription>(subscription, false);
            this._subscriptions.TryAdd(subscription.Key, reference);
        }

        void Remove(Subscription subscription)
        {
            this._subscriptions.TryRemove(subscription.Key, out _);
        }

        class Subscription : ISubscription<T>
        {
            private Subject<T> _subject;
            private Action<T> _action;
            private SynchronizationContext _context;
            public string Key { get; private set; }

            public Subscription(Subject<T> subject, Action<T> action)
            {
                this._subject = subject;
                this._action = action;
                this.Key = Guid.NewGuid().ToString();
                this._subject.Add(this);
            }

            public void Publish(T message)
            {
                try
                {
                    if (this._context != null)
                        _context.Post(state => _action?.Invoke((T)state), message);
                    else
                        _action?.Invoke(message);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            public ISubscription<T> ObserveOn(SynchronizationContext context)
            {
                this._context = context ?? throw new ArgumentNullException(nameof(context));
                return this;
            }

            #region IDisposable Support
            private int _disposed = 0;

            protected virtual void Dispose(bool disposing)
            {
                try
                {
#if UNITY_WEBGL
                    if (this._disposed==1)
                        return;

                    _disposed = 1;
                    if (subject != null)
                        subject.Remove(this);

                    context = null;
                    action = null;
                    subject = null;
#else
                    if (Interlocked.CompareExchange(ref this._disposed, 1, 0) == 0)
                    {
                        if (_subject != null)
                            _subject.Remove(this);

                        _context = null;
                        _action = null;
                        _subject = null;
                    }
#endif
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            ~Subscription()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
    }public interface ISubscription<T> : IDisposable
    {
        /// <summary>
        /// Changes the thread of message consumption.
        /// For example, sending a message to the UI thread for execution.
        /// </summary>
        /// <example>
        /// <code Message="(m=&gt;{}).ObserveOn(SynchronizationContext.Current);">
        /// messenger.Subscribe
        /// </code>
        /// </example>
        /// <param name="context"></param>
        /// <returns></returns>
        ISubscription<T> ObserveOn(SynchronizationContext context);
    }
}