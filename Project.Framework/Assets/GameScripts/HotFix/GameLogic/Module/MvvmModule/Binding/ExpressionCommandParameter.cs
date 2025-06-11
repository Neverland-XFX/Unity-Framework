using System;
using GameLogic.Commands;

namespace GameLogic.Binding
{
    public class ExpressionCommandParameter<TParam> : ICommandParameter<TParam>
    {
        private Func<TParam> expression;
        public ExpressionCommandParameter(Func<TParam> expression)
        {
            this.expression = expression;
        }

        object ICommandParameter.GetValue()
        {
            return GetValue();
        }

        public Type GetValueType()
        {
            return typeof(TParam);
        }

        public TParam GetValue()
        {
            return expression();
        }
    }
}
