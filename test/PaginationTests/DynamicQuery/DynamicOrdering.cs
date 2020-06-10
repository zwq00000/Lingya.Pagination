using System.Linq.Expressions;

namespace PaginationTests {
    internal class DynamicOrdering {
        public Expression Selector;
        public bool Ascending;
    }
}