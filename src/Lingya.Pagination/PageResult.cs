using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lingya.Pagination {

    /// <summary>
    /// 数据分页结果
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [DataContract]
    public class PageResult<TData> {

        public PageResult (Paging page) {
            Page = page;
        }

        public PageResult (Paging page, IQueryable<TData> source) : this (page) {
            Values = source.Skip (Page.Skip).Take (page.PageSize);
        }

        public PageResult (Paging page, IEnumerable<TData> data) : this (page) {
            Values = data;
        }

        /// <summary>
        /// 分页信息
        /// </summary>
        [DataMember (Name = "page")]
        public Paging Page { get; }

        /// <summary>
        /// 分页数据
        /// </summary>
        [DataMember (Name = "values")]
        public IEnumerable<TData> Values { get; }

    }

    /// <summary>
    /// 包含聚合数据的 分页结果
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    [DataContract]
    public class PageResult<TResult, TAggregate> : PageResult<TResult> {

        public PageResult (PageResult<TResult> result) : base (result.Page, result.Values) { }

        public PageResult (PageResult<TResult> result, TAggregate aggregate) : this (result) {
            this.Aggregate = aggregate;
        }

        /// <summary>
        /// 聚合数据
        /// </summary>
        /// <value></value>
        [DataMember(Name = "aggregate")]
        public TAggregate Aggregate { get; set; }
    }
}