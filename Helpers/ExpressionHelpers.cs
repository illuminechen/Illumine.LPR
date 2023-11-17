using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Illumine.LPR
{
    public static class ExpressionHelpers
    {
        public static T GetPropertyValue<T>(this Expression<Func<T>> lambda) => lambda.Compile()();

        public static void SetPropertyValue<T>(this Expression<Func<T>> lambda, T value)
        {
            MemberExpression body = lambda.Body as MemberExpression;
            ((PropertyInfo)body.Member).SetValue(Expression.Lambda(body.Expression).Compile().DynamicInvoke(), (object)value);
        }
    }
}
