using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lingya.Pagination {

    /// <summary>
    /// ���ݷ�ҳ���
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [DataContract]
    public class PageResult<TData> {

        public PageResult(Paging page) {
            Page = page;
        }

        public PageResult(Paging page, IQueryable<TData> source) : this(page) {
            Values = source.Skip(Page.Skip).Take(page.PageSize);
        }

        public PageResult(Paging page, IEnumerable<TData> data) : this(page) {
            Values = data;
        }

        /// <summary>
        /// ��ҳ��Ϣ
        /// </summary>
        [DataMember(Name = "page")]
        public Paging Page { get; }

        /// <summary>
        /// ��ҳ����
        /// </summary>
        [DataMember(Name = "values")]
        public IEnumerable<TData> Values { get; }

    }
}