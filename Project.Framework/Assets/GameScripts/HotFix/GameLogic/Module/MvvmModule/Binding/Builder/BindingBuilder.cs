using System;
using System.Linq.Expressions;
using System.Reflection;
using GameLogic.Binding.Contexts;
using GameLogic.Binding.Converters;
using GameLogic.Binding.Paths;
using GameLogic.Interactivity;
using UnityEngine.UIElements;


#if UNITY_IOS || ENABLE_IL2CPP
using GameLogic.Binding.Expressions;
#endif

namespace GameLogic.Binding.Builder
{
    public class BindingBuilder<TTarget, TSource> : BindingBuilderBase where TTarget : class
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(BindingBuilder<TTarget, TSource>));

        public BindingBuilder(IBindingContext context, TTarget target) : base(context, target)
        {
            this.description.TargetType = typeof(TTarget);
        }
        public BindingBuilder<TTarget, TSource> From(string targetName)
        {
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = null;
            return this;
        }

        public BindingBuilder<TTarget, TSource> From(string targetName, string updateTrigger)
        {
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<TTarget, TSource> From<TResult>(Expression<Func<TTarget, TResult>> memberExpression)
        {
            string targetName = ParseMemberName(memberExpression);
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = null;
            return this;
        }

        public BindingBuilder<TTarget, TSource> From<TResult>(Expression<Func<TTarget, TResult>> memberExpression, string updateTrigger)
        {
            string targetName = this.ParseMemberName(memberExpression);
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<TTarget, TSource> From<TResult, TEvent>(Expression<Func<TTarget, TResult>> memberExpression, Expression<Func<TTarget, TEvent>> updateTriggerExpression)
        {
            string targetName = this.ParseMemberName(memberExpression);
            string updateTrigger = this.ParseMemberName(updateTriggerExpression);
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<TTarget, TSource> From(Expression<Func<TTarget, EventHandler<InteractionEventArgs>>> memberExpression)
        {
            string targetName = this.ParseMemberName(memberExpression);
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = null;
            this.OneWayToSource();
            return this;
        }

#if UNITY_2019_1_OR_NEWER
        public BindingBuilder<TTarget, TSource> From<TResult>(Expression<Func<TTarget, TResult>> memberExpression, Expression<Func<TTarget, Func<EventCallback<ChangeEvent<TResult>>, bool>>> updateTriggerExpression)
        {
            string targetName = this.ParseMemberName(memberExpression);
            string updateTrigger = this.ParseMemberName(updateTriggerExpression);
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<TTarget, TSource> From<TResult>(Expression<Func<TTarget, Func<EventCallback<ChangeEvent<TResult>>, bool>>> memberExpression)
        {
            string targetName = this.ParseMemberName(memberExpression);
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = null;
            this.OneWayToSource();
            return this;
        }
#endif
        public string ParseMemberName(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return ParseMemberName0(expression.Body);
        }

        private string ParseMemberName0(Expression expression)
        {
            if (expression == null || !(expression is MemberExpression || expression is MethodCallExpression ||
                                        expression is UnaryExpression))
                return null;

            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name.Equals("get_Item") && methodCallExpression.Arguments.Count == 1)
                {
                    string temp = null;
                    var argument = methodCallExpression.Arguments[0];
                    if (!(argument is ConstantExpression))
                        argument = ConvertMemberAccessToConstant(argument);

                    object value = (argument as ConstantExpression).Value;
                    if (value is string strIndex)
                    {
                        temp = string.Format("[\"{0}\"]", strIndex);
                    }
                    else if (value is int intIndex)
                    {
                        temp = string.Format("[{0}]", intIndex);
                    }

                    var memberExpression = methodCallExpression.Object as MemberExpression;
                    if (memberExpression == null || !(memberExpression.Expression is ParameterExpression))
                        return temp;

                    return this.ParseMemberName0(memberExpression) + temp;
                }

                return methodCallExpression.Method.Name;
            }

            //Delegate.CreateDelegate(Type type, object firstArgument, MethodInfo method)
            //For<TTarget, TResult>(v => v.OnOpenLoginWindow); Support for method name parsing.
            if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Operand is MethodCallExpression methodCall &&
                    methodCall.Method.Name.Equals("CreateDelegate"))
                {
                    var info = this.GetDelegateMethodInfo(methodCall);
                    if (info != null)
                        return info.Name;
                }

                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
            }

            var body = expression as MemberExpression;
            if (body == null || !(body.Expression is ParameterExpression))
                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

            return body.Member.Name;
        }

        private MethodInfo GetDelegateMethodInfo(MethodCallExpression expression)
        {
            var target = expression.Object;
            var arguments = expression.Arguments;
            if (target == null)
            {
                foreach (var expr in arguments)
                {
                    if (!(expr is ConstantExpression))
                        continue;

                    var value = (expr as ConstantExpression).Value;
                    if (value is MethodInfo)
                        return (MethodInfo)value;
                }
                return null;
            }
            else if (target is ConstantExpression)
            {
                var value = (target as ConstantExpression).Value;
                if (value is MethodInfo)
                    return (MethodInfo)value;
            }
            return null;
        }

        private static Expression ConvertMemberAccessToConstant(Expression argument)
        {
            if (argument is ConstantExpression)
                return argument;

            var boxed = Expression.Convert(argument, typeof(object));
#if UNITY_IOS || ENABLE_IL2CPP
            var fun = (Func<object[], object>)Expression.Lambda<Func<object>>(boxed).DynamicCompile();
            var constant = fun(new object[] { });
#else
            var fun = Expression.Lambda<Func<object>>(boxed).Compile();
            var constant = fun();
#endif

            return Expression.Constant(constant);
        }


        public BindingBuilder<TTarget, TSource> To(string path)
        {
            this.SetMemberPath(path);
            return this;
        }

        public BindingBuilder<TTarget, TSource> To<TResult>(Expression<Func<TSource, TResult>> path)
        {
            this.SetMemberPath(this.Parse(path));
            return this;
        }

        public BindingBuilder<TTarget, TSource> To<TParameter>(Expression<Func<TSource, Action<TParameter>>> path)
        {
            this.SetMemberPath(this.Parse(path));
            return this;
        }

        public BindingBuilder<TTarget, TSource> To(Expression<Func<TSource, Action>> path)
        {
            this.SetMemberPath(this.Parse(path));
            return this;
        }

        public virtual Path Parse(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            Path path = new Path();
            var body = expression.Body as MemberExpression;
            if (body != null)
            {
                this.Parse(body, path);
                return path;
            }

            var method = expression.Body as MethodCallExpression;
            if (method != null)
            {
                this.Parse(method, path);
                return path;
            }

            var unary = expression.Body as UnaryExpression;
            if (unary != null && unary.NodeType == ExpressionType.Convert)
            {
                this.Parse(unary.Operand, path);
                return path;
            }

            var binary = expression.Body as BinaryExpression;
            if (binary != null && binary.NodeType == ExpressionType.ArrayIndex)
            {
                this.Parse(binary, path);
                return path;
            }
            return path;
            //throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
        }

        private void Parse(Expression expression, Path path)
        {
            if (expression == null || !(expression is MemberExpression || expression is MethodCallExpression || expression is BinaryExpression))
                return;

            if (expression is MemberExpression memberExpression)
            {
                var memberInfo = memberExpression.Member;
                if (memberInfo.IsStatic())
                {
                    path.Prepend(new MemberNode(memberInfo));
                    return;
                }
                else
                {
                    path.Prepend(new MemberNode(memberInfo));
                    if (memberExpression.Expression != null)
                        this.Parse(memberExpression.Expression, path);
                    return;
                }
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name.Equals("get_Item") && methodCallExpression.Arguments.Count == 1)
                {
                    var argument = methodCallExpression.Arguments[0];
                    if (!(argument is ConstantExpression))
                        argument = ConvertMemberAccessToConstant(argument);

                    object value = (argument as ConstantExpression).Value;
                    if (value is string)
                    {
                        path.PrependIndexed((string)value);
                    }
                    else if (value is Int32)
                    {
                        path.PrependIndexed((int)value);
                    }
                    if (methodCallExpression.Object != null)
                        this.Parse(methodCallExpression.Object, path);
                    return;
                }

                //Delegate.CreateDelegate(Type type, object firstArgument, MethodInfo method)
                if (methodCallExpression.Method.Name.Equals("CreateDelegate"))
                {
                    var info = this.GetDelegateMethodInfo(methodCallExpression);
                    if (info == null)
                        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

                    if (info.IsStatic)
                    {
                        path.Prepend(new MemberNode(info));
                        return;
                    }
                    else
                    {
                        path.Prepend(new MemberNode(info));
                        this.Parse(methodCallExpression.Arguments[1], path);
                        return;
                    }
                }

                if (methodCallExpression.Method.ReturnType.Equals(typeof(void)))
                {
                    var info = methodCallExpression.Method;
                    if (info.IsStatic)
                    {
                        path.Prepend(new MemberNode(info));
                        return;
                    }
                    else
                    {
                        path.Prepend(new MemberNode(info));
                        if (methodCallExpression.Object != null)
                            this.Parse(methodCallExpression.Object, path);
                        return;
                    }
                }

                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
            }

            if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
                {
                    var left = binaryExpression.Left;
                    var right = binaryExpression.Right;
                    if (!(right is ConstantExpression))
                        right = ConvertMemberAccessToConstant(right);

                    object value = (right as ConstantExpression).Value;
                    if (value is string)
                    {
                        path.PrependIndexed((string)value);
                    }
                    else if (value is int)
                    {
                        path.PrependIndexed((int)value);
                    }

                    if (left != null)
                        this.Parse(left, path);
                    return;
                }

                throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
            }
        }

        public BindingBuilder<TTarget, TSource> ToExpression<TResult>(Expression<Func<TSource, TResult>> expression)
        {
            this.SetExpression(expression);
            this.OneWay();
            return this;
        }

        public BindingBuilder<TTarget, TSource> TwoWay()
        {
            this.SetMode(BindingMode.TwoWay);
            return this;
        }

        public BindingBuilder<TTarget, TSource> OneWay()
        {
            this.SetMode(BindingMode.OneWay);
            return this;
        }

        public BindingBuilder<TTarget, TSource> OneWayToSource()
        {
            this.SetMode(BindingMode.OneWayToSource);
            return this;
        }

        public BindingBuilder<TTarget, TSource> OneTime()
        {
            this.SetMode(BindingMode.OneTime);
            return this;
        }

        //public BindingBuilder<TTarget, TSource> CommandParameter(object parameter)
        //{
        //    this.SetCommandParameter(parameter);
        //    return this;
        //}

        public BindingBuilder<TTarget, TSource> CommandParameter<T>(T parameter)
        {
            this.SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget, TSource> CommandParameter<TParam>(Func<TParam> parameter)
        {
            this.SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget, TSource> WithConversion(string converterName)
        {
            var converter = this.ConverterByName(converterName);
            return this.WithConversion(converter);
        }

        public BindingBuilder<TTarget, TSource> WithConversion(IConverter converter)
        {
            this.description.Converter = converter;
            return this;
        }

        public BindingBuilder<TTarget, TSource> WithScopeKey(object scopeKey)
        {
            this.SetScopeKey(scopeKey);
            return this;
        }
    }

    public class BindingBuilder<TTarget> : BindingBuilderBase where TTarget : class
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(BindingBuilder<TTarget>));

        public BindingBuilder(IBindingContext context, TTarget target) : base(context, target)
        {
            this.description.TargetType = typeof(TTarget);
        }

        public BindingBuilder<TTarget> For(string targetPropertyName)
        {
            this.description.TargetName = targetPropertyName;
            this.description.UpdateTrigger = null;
            return this;
        }

        public BindingBuilder<TTarget> For(string targetPropertyName, string updateTrigger)
        {
            this.description.TargetName = targetPropertyName;
            this.description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder<TTarget> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression)
        {
            return this;
        }

        public BindingBuilder<TTarget> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression,
            string updateTrigger)
        {
            return this;
        }

        public BindingBuilder<TTarget> For<TResult, TEvent>(Expression<Func<TTarget, TResult>> memberExpression,
            Expression<Func<TTarget, TEvent>> updateTriggerExpression)
        {
            return this;
        }

        public BindingBuilder<TTarget> For(
            Expression<Func<TTarget, EventHandler<InteractionEventArgs>>> memberExpression)
        {
            return this;
        }

#if UNITY_2019_1_OR_NEWER
        public BindingBuilder<TTarget> For<TResult>(Expression<Func<TTarget, TResult>> memberExpression,
            Expression<Func<TTarget, Func<EventCallback<ChangeEvent<TResult>>, bool>>> updateTriggerExpression)
        {
            return this;
        }

        public BindingBuilder<TTarget> For<TResult>(
            Expression<Func<TTarget, Func<EventCallback<ChangeEvent<TResult>>, bool>>> memberExpression)
        {
            return this;
        }
#endif

        public BindingBuilder<TTarget> To(string path)
        {
            this.SetStaticMemberPath(path);
            this.OneWay();
            return this;
        }

        public BindingBuilder<TTarget> To<TResult>(Expression<Func<TResult>> path)
        {
            return this;
        }

        public BindingBuilder<TTarget> To<TParameter>(Expression<Func<Action<TParameter>>> path)
        {
            return this;
        }

        public BindingBuilder<TTarget> To(Expression<Func<Action>> path)
        {
            return this;
        }

        public BindingBuilder<TTarget> ToValue(object value)
        {
            this.SetLiteral(value);
            return this;
        }

        public BindingBuilder<TTarget> ToExpression<TResult>(Expression<Func<TResult>> expression)
        {
            this.SetExpression(expression);
            this.OneWay();
            return this;
        }

        public BindingBuilder<TTarget> TwoWay()
        {
            this.SetMode(BindingMode.TwoWay);
            return this;
        }

        public BindingBuilder<TTarget> OneWay()
        {
            this.SetMode(BindingMode.OneWay);
            return this;
        }

        public BindingBuilder<TTarget> OneWayToSource()
        {
            this.SetMode(BindingMode.OneWayToSource);
            return this;
        }

        public BindingBuilder<TTarget> OneTime()
        {
            this.SetMode(BindingMode.OneTime);
            return this;
        }

        //public BindingBuilder<TTarget> CommandParameter(object parameter)
        //{
        //    this.SetCommandParameter(parameter);
        //    return this;
        //}

        public BindingBuilder<TTarget> CommandParameter<T>(T parameter)
        {
            this.SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget> CommandParameter<TParam>(Func<TParam> parameter)
        {
            this.SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder<TTarget> WithConversion(string converterName)
        {
            var converter = ConverterByName(converterName);
            return this.WithConversion(converter);
        }

        public BindingBuilder<TTarget> WithConversion(IConverter converter)
        {
            this.description.Converter = converter;
            return this;
        }

        public BindingBuilder<TTarget> WithScopeKey(object scopeKey)
        {
            this.SetScopeKey(scopeKey);
            return this;
        }
        
        
        
    }

    public class BindingBuilder : BindingBuilderBase
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(BindingBuilder));

        public BindingBuilder(IBindingContext context, object target) : base(context, target)
        {
        }

        public BindingBuilder For(string targetName, string updateTrigger = null)
        {
            this.description.TargetName = targetName;
            this.description.UpdateTrigger = updateTrigger;
            return this;
        }

        public BindingBuilder To(string path)
        {
            this.SetMemberPath(path);
            return this;
        }

        public BindingBuilder ToStatic(string path)
        {
            this.SetStaticMemberPath(path);
            return this;
        }

        public BindingBuilder ToValue(object value)
        {
            this.SetLiteral(value);
            return this;
        }

        public BindingBuilder TwoWay()
        {
            this.SetMode(BindingMode.TwoWay);
            return this;
        }

        public BindingBuilder OneWay()
        {
            this.SetMode(BindingMode.OneWay);
            return this;
        }

        public BindingBuilder OneWayToSource()
        {
            this.SetMode(BindingMode.OneWayToSource);
            return this;
        }

        public BindingBuilder OneTime()
        {
            this.SetMode(BindingMode.OneTime);
            return this;
        }

        public BindingBuilder CommandParameter(object parameter)
        {
            this.SetCommandParameter(parameter);
            return this;
        }

        public BindingBuilder WithConversion(string converterName)
        {
            var converter = this.ConverterByName(converterName);
            return this.WithConversion(converter);
        }

        public BindingBuilder WithConversion(IConverter converter)
        {
            this.description.Converter = converter;
            return this;
        }

        public BindingBuilder WithScopeKey(object scopeKey)
        {
            this.SetScopeKey(scopeKey);
            return this;
        }


    }
}