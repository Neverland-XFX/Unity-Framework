using System;
using GameLogic.Binding;
using GameLogic.Contexts;
using GameLogic.Repositories;
using GameLogic.Services;

namespace GameLogic
{
    public class MvvmModule : Singleton<MvvmModule>
    {
        public ApplicationContext ApplicationContext;
        // private static ISubscription<BattleEventArgs> _battleSubscription;

        public void Init()
        {
            Context.OnInitialize();
            UISynchronizationContext.OnInitialize();
            UnityProxyRegister.Initialize();
            ApplicationContext = Context.GetApplicationContext();
            var container = ApplicationContext.GetContainer();
            var bundle = new BindingServiceBundle(container);
            bundle.Start();
        }
        
        public void Register<TIRepository, TRepository, TIService, TService>(
            Func<TIRepository, TService> serviceFactory)
            where TRepository : TIRepository, new()
            where TService : TIService
        {
            ApplicationContext = Context.GetApplicationContext();
            var container = ApplicationContext.GetContainer();
            TIRepository accountRepository = new TRepository();
            TIService service = serviceFactory(accountRepository);
            container.Register<TIService>(service);
            
        }

        public void Unregister<T>()
        {
            ApplicationContext = Context.GetApplicationContext();
            var container = ApplicationContext.GetContainer();
            container.Unregister<T>();
        }

        public void Unregister(string name)
        {
            ApplicationContext = Context.GetApplicationContext();
            var container = ApplicationContext.GetContainer();
            container.Unregister(name);
        }
    }
}