using System.Collections.Generic;
using System.Linq;

namespace Lingya.Pagination {

    /// <summary>
    /// 数据分页结果
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class PageResult<TData> {

        public PageResult(Paging page) {
            Page = page;
        }

        public PageResult(Paging page, IQueryable<TData> source):this(page) {
            Values = source.Skip(Page.Skip).Take(page.PageSize);
        }

        public PageResult(Paging page, IEnumerable<TData> data) : this(page) {
            Values = data;
        }

        /// <summary>
        /// 分页信息
        /// </summary>
        public Paging Page { get; set; }

        /// <summary>
        /// 分页数据
        /// </summary>
        public IEnumerable<TData> Values { get; set; }

    }
}