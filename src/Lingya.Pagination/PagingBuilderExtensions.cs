using System.Linq;

namespace Lingya.Pagination {
    public static class PagingBuilderExtensions {

        /// <summary>
        /// 创建分页查询构建器
        /// </summary>
        /// <param name="source">query source</param>
        /// <param name="parameter">paging parameter</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns>IPagingQueryBuilder</returns>
        public static IPagingQueryBuilder<TSource> PagingBuilder<TSource> (this IQueryable<TSource> source,
            PageParameter parameter) {

            if (parameter == null) {
                parameter = new PageParameter ();
            }

            return new PagingQueryBuilder<TSource> (source, parameter);
        }
    }

}