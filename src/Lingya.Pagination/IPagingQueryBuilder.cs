using System;
using System.Linq;
using System.Linq.Expressions;

namespace Lingya.Pagination {
    /// <summary>
    /// 分页查询构建器
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public interface IPagingQueryBuilder<TSource> {
        /// <summary>
        /// 查询对象
        /// </summary>
        /// <value></value>
        IQueryable<TSource> Query { get; }

        /// <summary>
        /// 分页参数
        /// </summary>
        /// <value></value>
        PageParameter Parameter { get; }

        /// <summary>
        /// 应用过滤条件
        /// </summary>
        /// <param name="predicate">数据过滤表达式</param>
        void ApplyFilter (Expression<Func<TSource, bool>> predicate);
    }

}