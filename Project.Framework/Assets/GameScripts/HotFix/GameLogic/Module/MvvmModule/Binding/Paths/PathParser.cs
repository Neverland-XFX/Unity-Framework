﻿
using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Text;
#if UNITY_IOS || ENABLE_IL2CPP
using GameLogic.Binding.Expressions;
#endif


namespace GameLogic.Binding.Paths
{
public class PathParser : IPathParser
    {
        public virtual Path Parse(string pathText)
        {
            return TextPathParser.Parse(pathText);
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

        public virtual Path ParseStaticPath(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            var current = expression.Body;
            var unary = current as UnaryExpression;
            if (unary != null)
                current = unary.Operand;

            if (current is MemberExpression)
            {
                Path path = new Path();
                this.Parse(current, path);
                return path;
            }

            if (current is MethodCallExpression)
            {
                Path path = new Path();
                this.Parse(current, path);
                return path;
            }

            var binary = current as BinaryExpression;
            if (binary != null && binary.NodeType == ExpressionType.ArrayIndex)
            {
                Path path = new Path();
                this.Parse(current, path);
                return path;
            }

            throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
        }

        public virtual Path ParseStaticPath(string pathText)
        {
            string typeName = this.ParserTypeName(pathText);
            string memberName = this.ParserMemberName(pathText);
            Type type = TypeFinderUtils.FindType(typeName);

            Path path = new Path();
            path.Append(new MemberNode(type, memberName, true));
            return path;
        }

        protected string ParserTypeName(string pathText)
        {
            if (pathText == null)
                throw new ArgumentNullException("pathText");

            pathText = pathText.Replace(" ", "");
            if (string.IsNullOrEmpty(pathText))
                throw new ArgumentException("The pathText is empty");

            int index = pathText.LastIndexOf('.');
            if (index <= 0)
                throw new ArgumentException("pathText");

            return pathText.Substring(0, index);
        }

        protected string ParserMemberName(string pathText)
        {
            if (pathText == null)
                throw new ArgumentNullException("pathText");

            pathText = pathText.Replace(" ", "");
            if (string.IsNullOrEmpty(pathText))
                throw new ArgumentException("The pathText is empty");

            int index = pathText.LastIndexOf('.');
            if (index <= 0)
                throw new ArgumentException("pathText");

            return pathText.Substring(index + 1);
        }

        public virtual string ParseMemberName(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return ParseMemberName0(expression.Body);
        }

        protected string ParseMemberName0(Expression expression)
        {
            if (expression == null || !(expression is MemberExpression || expression is MethodCallExpression || expression is UnaryExpression))
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
                if (unaryExpression.Operand is MethodCallExpression methodCall && methodCall.Method.Name.Equals("CreateDelegate"))
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

        //public virtual string ParseMemberName(LambdaExpression expression)
        //{
        //    if (expression == null)
        //        throw new ArgumentNullException("expression");

        //    var method = expression.Body as MethodCallExpression;
        //    if (method != null)
        //        return method.Method.Name;

        //    //Delegate.CreateDelegate(Type type, object firstArgument, MethodInfo method)
        //    var unary = expression.Body as UnaryExpression;
        //    if (unary != null && unary.NodeType == ExpressionType.Convert)
        //    {
        //        MethodCallExpression methodCall = (MethodCallExpression)unary.Operand;
        //        if (methodCall.Method.Name.Equals("CreateDelegate"))
        //        {
        //            var info = this.GetDelegateMethodInfo(methodCall);
        //            if (info != null)
        //                return info.Name;
        //        }

        //        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));
        //    }

        //    var body = expression.Body as MemberExpression;
        //    if (body == null)
        //        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

        //    if (!(body.Expression is ParameterExpression))
        //        throw new ArgumentException(string.Format("Invalid expression:{0}", expression));

        //    return body.Member.Name;
        //}
    }
}