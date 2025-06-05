using System;
using System.Collections.Generic;
using GameLogic.Binding.Contexts;
using UnityFramework;

namespace GameLogic.Binding.Builder
{
    public abstract class BindingSetBase : IBindingBuilder
    {
        protected IBindingContext context;
        protected readonly List<IBindingBuilder> builders = new List<IBindingBuilder>();

        public BindingSetBase(IBindingContext context)
        {
            this.context = context;
        }

        public virtual void Build()
        {
            foreach (var builder in this.builders)
            {
                try
                {
                    builder.Build();
                }
                catch (Exception e)
                {
                    Log.Error("{0}", e);
                }
            }
            this.builders.Clear();
        }
    }

    public class BindingSet<TTarget, TSource> : BindingSetBase where TTarget : class
    {
        private TTarget target;
        public BindingSet(IBindingContext context, TTarget target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder<TTarget, TSource> Bind()
        {
            var builder = new BindingBuilder<TTarget, TSource>(this.context, this.target);
            this.builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<TChildTarget, TSource> Bind<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = new BindingBuilder<TChildTarget, TSource>(context, target);
            this.builders.Add(builder);
            return builder;
        }

//#if UNITY_2019_1_OR_NEWER
//        public virtual BindingBuilder<TChildTarget, TSource> Bind<TChildTarget>(string targetName = null) where TChildTarget : VisualElement
//        {
//            UIDocument document = (this.target as UnityEngine.Behaviour).GetComponent<UIDocument>();
//            if (document == null)
//                throw new Exception("The UIDocument not found, this is not a UIToolkit view.");

//            VisualElement rootVisualElement = document.rootVisualElement;
//            TChildTarget target = string.IsNullOrEmpty(targetName) ? rootVisualElement.Q<TChildTarget>() : rootVisualElement.Q<TChildTarget>(targetName);
//            var builder = new BindingBuilder<TChildTarget, TSource>(context, target);
//            this.builders.Add(builder);
//            return builder;
//        }
//#endif
    }

    public class BindingSet<TTarget> : BindingSetBase where TTarget : class
    {
        private TTarget target;
        public BindingSet(IBindingContext context, TTarget target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder<TTarget> Bind()
        {
            var builder = new BindingBuilder<TTarget>(this.context, this.target);
            this.builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<TChildTarget> Bind<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = new BindingBuilder<TChildTarget>(context, target);
            this.builders.Add(builder);
            return builder;
        }

//#if UNITY_2019_1_OR_NEWER
//        public virtual BindingBuilder<TChildTarget> Bind<TChildTarget>(string targetName = null) where TChildTarget : VisualElement
//        {
//            UIDocument document = (this.target as UnityEngine.Behaviour).GetComponent<UIDocument>();
//            if (document == null)
//                throw new Exception("The UIDocument not found, this is not a UIToolkit view.");

//            VisualElement rootVisualElement = document.rootVisualElement;
//            TChildTarget target = string.IsNullOrEmpty(targetName) ? rootVisualElement.Q<TChildTarget>() : rootVisualElement.Q<TChildTarget>(targetName);
//            var builder = new BindingBuilder<TChildTarget>(context, target);
//            this.builders.Add(builder);
//            return builder;
//        }
// #endif
    }

    public class BindingSet : BindingSetBase
    {
        private object target;
        public BindingSet(IBindingContext context, object target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder Bind()
        {
            var builder = new BindingBuilder(this.context, this.target);
            this.builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder Bind(object target)
        {
            var builder = new BindingBuilder(context, target);
            this.builders.Add(builder);
            return builder;
        }
    }
}