using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lingya.Pagination.Tests {
    public static class DynamicExpression {
        public static Expression Parse (Type resultType, string expression, params object[] values) {
            ExpressionParser parser = new (null, expression, values);
            return parser.Parse (resultType);
        }

        public static LambdaExpression ParseLambda (Type itType, Type resultType, string expression, params object[] values) {
            return ParseLambda (new ParameterExpression[] { Expression.Parameter (itType, "") }, resultType, expression, values);
        }

        public static LambdaExpression ParseLambda (ParameterExpression[] parameters, Type resultType, string expression, params object[] values) {
            ExpressionParser parser = new (parameters, expression, values);
            return Expression.Lambda (parser.Parse (resultType), parameters);
        }

        public static Expression<Func<T, S>> ParseLambda<T, S> (string expression, params object[] values) {
            return (Expression<Func<T, S>>) ParseLambda (typeof (T), typeof (S), expression, values);
        }

        public static Type CreateClass (params DynamicProperty[] properties) {
            return ClassFactory.Instance.GetDynamicClass (properties);
        }

        public static Type CreateClass (IEnumerable<DynamicProperty> properties) {
            return ClassFactory.Instance.GetDynamicClass (properties);
        }
    }
}