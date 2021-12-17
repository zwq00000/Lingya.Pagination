using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lingya.Pagination {

    internal class PagingQueryBuilder<TSource> : IPagingQueryBuilder<TSource> {
        private IQueryable<TSource> _query;
        private readonly PageParameter parameter;

        public PagingQueryBuilder (IQueryable<TSource> queryable, PageParameter parameter) {
            this._query = queryable;
            this.parameter = parameter;
        }

        public IQueryable<TSource> Query => _query;

        public PageParameter Parameter => parameter;

        public void ApplyFilter (Expression<Func<TSource, bool>> predicate) {
            this._query = _query.Where (predicate);
        }
    }
}