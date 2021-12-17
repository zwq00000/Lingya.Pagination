using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lingya.Pagination.Tests {
    public static class DynamicQueryable {
        public static IQueryable<T> Where<T> (this IQueryable<T> source, string predicate, params object[] values) {
            return (IQueryable<T>) Where ((IQueryable) source, predicate, values);
        }

        public static IQueryable Where (this IQueryable source, string predicate, params object[] values) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            if (predicate == null) throw new ArgumentNullException (nameof (predicate));
            LambdaExpression lambda = DynamicExpression.ParseLambda (source.ElementType, typeof (bool), predicate, values);
            return source.Provider.CreateQuery (
                Expression.Call (
                    typeof (Queryable), "Where",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Quote (lambda)));
        }

        public static IQueryable Select (this IQueryable source, string selector, params object[] values) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            if (selector == null) throw new ArgumentNullException (nameof (selector));
            LambdaExpression lambda = DynamicExpression.ParseLambda (source.ElementType, null, selector, values);
            return source.Provider.CreateQuery (
                Expression.Call (
                    typeof (Queryable), "Select",
                    new Type[] { source.ElementType, lambda.Body.Type },
                    source.Expression, Expression.Quote (lambda)));
        }

        public static IQueryable<T> DynamicOrderBy<T> (this IQueryable<T> source, string ordering, params object[] values) {
            return (IQueryable<T>) DynamicOrderBy ((IQueryable) source, ordering, values);
        }

        public static IQueryable DynamicOrderBy (this IQueryable source, string ordering, params object[] values) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            if (ordering == null) throw new ArgumentNullException (nameof (ordering));
            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter (source.ElementType, "")
            };
<<<<<<< HEAD
            ExpressionParser parser = new (parameters, ordering, values);
=======
            ExpressionParser parser = new ExpressionParser (parameters, ordering, values);
>>>>>>> af9c08603256dd2d65573c09bca64f6b666b9013
            IEnumerable<DynamicOrdering> orderings = parser.ParseOrdering ();
            Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            foreach (DynamicOrdering o in orderings) {
                queryExpr = Expression.Call (
                    typeof (Queryable), o.Ascending ? methodAsc : methodDesc,
                    new Type[] { source.ElementType, o.Selector.Type },
                    queryExpr, Expression.Quote (Expression.Lambda (o.Selector, parameters)));
                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }
            return source.Provider.CreateQuery (queryExpr);
        }

        public static IQueryable Take (this IQueryable source, int count) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            return source.Provider.CreateQuery (
                Expression.Call (
                    typeof (Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant (count)));
        }

        public static IQueryable Skip (this IQueryable source, int count) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            return source.Provider.CreateQuery (
                Expression.Call (
                    typeof (Queryable), "Skip",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant (count)));
        }

        public static IQueryable GroupBy (this IQueryable source, string keySelector, string elementSelector, params object[] values) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            if (keySelector == null) throw new ArgumentNullException (nameof (keySelector));
            if (elementSelector == null) throw new ArgumentNullException (nameof (elementSelector));
            LambdaExpression keyLambda = DynamicExpression.ParseLambda (source.ElementType, null, keySelector, values);
            LambdaExpression elementLambda = DynamicExpression.ParseLambda (source.ElementType, null, elementSelector, values);
            return source.Provider.CreateQuery (
                Expression.Call (
                    typeof (Queryable), "GroupBy",
                    new Type[] { source.ElementType, keyLambda.Body.Type, elementLambda.Body.Type },
                    source.Expression, Expression.Quote (keyLambda), Expression.Quote (elementLambda)));
        }

        public static bool Any (this IQueryable source) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            return (bool) source.Provider.Execute (
                Expression.Call (
                    typeof (Queryable), "Any",
                    new Type[] { source.ElementType }, source.Expression));
        }

        public static int Count (this IQueryable source) {
            if (source == null) throw new ArgumentNullException (nameof (source));
            return (int) source.Provider.Execute (
                Expression.Call (
                    typeof (Queryable), "Count",
                    new Type[] { source.ElementType }, source.Expression));
        }
    }
}