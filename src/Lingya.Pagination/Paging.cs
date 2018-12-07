using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace Lingya.Pagination
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class Paging {
        /// <inheritdoc />
        public Paging() {
            Page = 1;
            PageSize = 20;
            Skip = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paging" /> class.
        /// </summary>
        /// <param name="total">总数量.</param>
        /// <param name="pageSize">页面大小.</param>
        /// <param name="pageNum">当前页码.</param>
        public Paging(int total, int pageSize = 20, int pageNum = 1) {
            Total = total<0?0:total;
            PageSize = pageSize<10?10:pageSize;
            Page = pageNum<1?1:pageNum;
            Pages = (int)Math.Ceiling(total / (double)pageSize);

            var skip = PageSize * (Page - 1);
            if (skip > Total) {
                skip = 0;
                Page = 1;
            }

            if (skip < 0) {
                skip = 0;
            }

            Skip = skip;
        }

        [IgnoreDataMember]
        public int Skip { get; private set; }

        /// <summary>
        /// 总数量
        /// </summary>
        /// <value>总数量</value>
        [DataMember(Name = "total",IsRequired = false)]
        public int Total { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        /// <value>总页数</value>
        [DataMember(Name = "pages", IsRequired = false)]
        public int Pages { get; set; }

        /// <summary>
        /// 页面大小
        /// </summary>
        /// <value>页面大小</value>
        [DataMember(Name = "pageSize")]
        public int PageSize { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        /// <value>当前页码</value>
        [DataMember(Name = "page",IsRequired = true,EmitDefaultValue = true)]
        [DefaultValue(20)]
        public int Page { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Paging {\n");
            sb.Append("  Total: ").Append(Total).Append("\n");
            sb.Append("  Pages: ").Append(Pages).Append("\n");
            sb.Append("  PageSize: ").Append(PageSize).Append("\n");
            sb.Append("  Page: ").Append(Page).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
