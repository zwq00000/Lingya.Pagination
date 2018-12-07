using System.Collections.Generic;
using System.Linq;

namespace Lingya.Pagination {

    /// <summary>
    /// ���ݷ�ҳ���
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
        /// ��ҳ��Ϣ
        /// </summary>
        public Paging Page { get; set; }

        /// <summary>
        /// ��ҳ����
        /// </summary>
        public IEnumerable<TData> Values { get; set; }

    }
}