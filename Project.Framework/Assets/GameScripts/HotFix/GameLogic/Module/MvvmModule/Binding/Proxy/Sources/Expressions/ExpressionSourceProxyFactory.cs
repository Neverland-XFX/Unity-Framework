using System;
using System.Collections.Generic;
using GameLogic.Binding.Expressions;
using GameLogic.Binding.Paths;
using GameLogic.Binding.Proxy.Sources.Object;
using GameLogic.Binding.Sources;

namespace GameLogic.Binding.Proxy.Sources.Expressions
{
public class ExpressionSourceProxyFactory : TypedSourceProxyFactory<ExpressionSourceDescription>
    {
        private ISourceProxyFactory factory;
        private IExpressionPathFinder pathFinder;
        public ExpressionSourceProxyFactory(ISourceProxyFactory factory, IExpressionPathFinder pathFinder)
        {
            this.factory = factory;
            this.pathFinder = pathFinder;
        }

        protected override bool TryCreateProxy(object source, ExpressionSourceDescription description, out ISourceProxy proxy)
        {
            proxy = null;
            var expression = description.Expression;
            List<ISourceProxy> list = new List<ISourceProxy>();
            List<Path> paths = this.pathFinder.FindPaths(expression);
            foreach (Path path in paths)
            {
                if (!path.IsStatic)
                {
                    if (source == null)
                        continue;//ignore the path

                    MemberNode memberNode = path[0] as MemberNode;
                    if (memberNode != null && memberNode.MemberInfo != null && !memberNode.MemberInfo.DeclaringType.IsAssignableFrom(source.GetType()))
                        continue;//ignore the path
                }

                ISourceProxy innerProxy = this.factory.CreateProxy(source, new ObjectSourceDescription() { Path = path });
                if (innerProxy != null)
                    list.Add(innerProxy);
            }

#if UNITY_IOS || ENABLE_IL2CPP
            Func<object[], object> del = expression.DynamicCompile();
            proxy = new ExpressionSourceProxy(description.IsStatic ? null : source, del, description.ReturnType, list);
#else
            try
            {
                var del = expression.Compile();
                Type returnType = del.ReturnType();
                Type parameterType = del.ParameterType();
                if (parameterType != null)
                {
                    proxy = (ISourceProxy)Activator.CreateInstance(typeof(ExpressionSourceProxy<,>).MakeGenericType(parameterType, returnType), source, del, list);
                }
                else
                {
                    proxy = (ISourceProxy)Activator.CreateInstance(typeof(ExpressionSourceProxy<>).MakeGenericType(returnType), del, list);
                }
            }
            catch (Exception)
            {
                //JIT Exception
                Func<object[], object> del = expression.DynamicCompile();
                proxy = new ExpressionSourceProxy(description.IsStatic ? null : source, del, description.ReturnType, list);
            }
#endif
            if (proxy != null)
                return true;

            return false;
        }
    }
}