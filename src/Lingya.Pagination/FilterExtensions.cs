using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lingya.Pagination
{
    public static class FilterExtensions
    {

        private static IEnumerable<MemberExpression> MakeMemberAccess(this ParameterExpression parameter, params string[] properties) {
            foreach (var propertyName in properties) {
                var property = parameter.Type.GetProperty(propertyName,
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property == null) {
                    continue;
                }
                yield return Expression.MakeMemberAccess(parameter,
                                                   property);
            }
        }

        private static IEnumerable<MemberExpression> MakeMemberAccess(this ParameterExpression parameter,
            params PropertyInfo[] properties) {
            foreach (var property in properties) {
                if (property == null) {
                    continue;
                }
                yield return Expression.MakeMemberAccess(parameter,
                                                   property);
            }
        }


        /// <summary>
        /// 生成属性访问 Lambda表达式
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static LambdaExpression MakeMemberAccess(this Type elementType, string propertyName) {
            var property = elementType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null) {
                return null;
            }

            var param = Expression.Parameter(elementType, "i");
            var propertyAccess = Expression.MakeMemberAccess(param, property);
            var sortExpression = Expression.Lambda(propertyAccess, param);
            return sortExpression;
        }
    }
}