using System.ComponentModel;
using System.Runtime.Serialization;

namespace Lingya.Pagination {
    /// <summary>
    /// 分页参数
    /// </summary>
    [DataContract]
    public class PageParameter {
        private int _page;
        private int _pageSize;

        /// <summary>
        /// 默认分页大小
        /// </summary>
        private const int DefaultPageSize = 20;

        /// <summary>
        /// 最小页面大小
        /// </summary>
        private const int MinPageSize = 5;

        /// <summary>
        /// 默认构造方法
        /// </summary>
        public PageParameter() {
            Page = 1;
            PageSize = DefaultPageSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageParameter" /> class.
        /// </summary>
        /// <param name="pageSize">页面大小.</param>
        /// <param name="pageNum">当前页码.</param>
        public PageParameter(int pageSize = 20, int pageNum = default(int)) {
            PageSize = pageSize < MinPageSize ? MinPageSize : pageSize;
            Page = pageNum < 0 ? 0 : pageNum;
        }

        /// <summary>
        /// 页面大小
        /// </summary>
        /// <value>页面大小</value>
        [DefaultValue(20)]
        [DataMember(Name = "pageSize",IsRequired = false)]
        public int? PageSize {
            get => _pageSize;
            set {
                var size = value ?? MinPageSize;
                _pageSize = size < MinPageSize ? MinPageSize : size;
            }
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        /// <value>当前页码</value>
        [DefaultValue(1)]
        [DataMember(Name = "page", IsRequired = false)]
        public int? Page {
            get => _page;
            set {
                var page = value ?? 1;
                _page = page<1?1:page;
            }
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        [DataMember(Name = "sortBy",IsRequired = false)]
        public string SortBy { get; set; }

        /// <summary>
        /// 搜索值
        /// </summary>
        [DataMember(Name = "searchKey", IsRequired = false)]
        public string SearchKey { get; set; }

        /// <summary>
        /// 逆序
        /// </summary>
        [DefaultValue(false)]
        [DataMember(Name = "desc", IsRequired = false)]
        public bool Descending { get; set; } = false;
    }
}