using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GameLogic;
using GameLogic.Binding;
using GameLogic.Contexts;
using UnityFramework;
using GameLogic.Binding.Services;
using GameLogic.Execution;
using GameLogic.Repositories;
using GameLogic.Services;
using UnityFramework.Localization;

#pragma warning disable CS0436


/// <summary>
/// 游戏App。
/// </summary>
public partial class GameApp
{
    private static List<Assembly> _hotfixAssembly;

    /// <summary>
    /// 热更域App主入口。
    /// </summary>
    /// <param name="objects"></param>
    public static void Entrance(object[] objects)
    {
        GameEventHelper.Init();
        _hotfixAssembly = (List<Assembly>)objects[0];
        Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
        Log.Warning("======= Entrance GameApp =======");
        Utility.Unity.AddDestroyListener(Release);
        StartGameLogic();
    }
    
    public static ApplicationContext ApplicationContext;
    // private static ISubscription<BattleEventArgs> _battleSubscription;
    
    /// <summary>
    /// 开始游戏业务层逻辑。
    /// <remarks>显示UI、加载场景等。</remarks>
    /// </summary>
    private static void StartGameLogic()
    {
        InitMvvmModule();
        ApplicationContext = Context.GetApplicationContext();
        var container = ApplicationContext.GetContainer();
        var bundle = new BindingServiceBundle(container);
        bundle.Start();
        IBattleRepository accountRepository = new BattleRepository();
        container.Register<IBattleService>(new BattleService(accountRepository));
        
        UIModule.Instance.Active();
        StartBattleRoom().Forget();
    }

    private static void InitMvvmModule()
    {
        Context.OnInitialize();
        UISynchronizationContext.OnInitialize();
    }

    private static async UniTaskVoid StartBattleRoom()
    {
        await GameModule.Scene.LoadSceneAsync("scene_battle");
        BattleSystem.Instance.LoadRoom().Forget();
    }
    
    private static void Release()
    {
        SingletonSystem.Release();
        Log.Warning("======= Release GameApp =======");
    }
}