using System.ComponentModel;

namespace Lingya.Pagination {
    /// <summary>
    /// 分页参数
    /// </summary>
    public class PageParameter {
        private int _page;
        private int _pageSize;

        /// <summary>
        /// 默认分页大小
        /// </summary>
        private const int DEFAULT_PAGE_SIZE = 20;

        /// <inheritdoc />
        public PageParameter() {
            Page = 1;
            PageSize = DEFAULT_PAGE_SIZE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageParameter" /> class.
        /// </summary>
        /// <param name="pageSize">页面大小.</param>
        /// <param name="pageNum">当前页码.</param>
        public PageParameter(int pageSize = 20, int pageNum = default(int)) {
            PageSize = pageSize < 10 ? 10 : pageSize;
            Page = pageNum < 0 ? 0 : pageNum;
        }

        /// <summary>
        /// 页面大小
        /// </summary>
        /// <value>页面大小</value>
        [DefaultValue(20)]
        public int PageSize {
            get => _pageSize;
            set {
                if (value < 5) {
                    _pageSize = 5;
                } else {
                    _pageSize = value;
                }
            }
        }

        /// <summary>
        /// 当前页码
        /// </summary>
        /// <value>当前页码</value>
        [DefaultValue(1)]
        public int Page {
            get => _page;
            set {
                if (value < 1) {
                    _page = 1;
                } else {
                    _page = value;
                }
            }
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// 搜索值
        /// </summary>
        public string SearchKey { get; set; }

        /// <summary>
        /// 逆序
        /// </summary>
        [DefaultValue(false)]
        public bool Descending { get; set; }
    }
}