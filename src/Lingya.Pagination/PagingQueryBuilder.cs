using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lingya.Pagination
{

    internal class PagingQueryBuilder<TSource> : IPagingQueryBuilder<TSource> {
        private IQueryable<TSource> _query;
        private readonly PageParameter parameter;

        private IList<Expression> whereExpression;

        public PagingQueryBuilder (IQueryable<TSource> queryable, PageParameter parameter) {
            this._query = queryable;
            this.parameter = parameter;
            if (parameter.HasSearchKey ()) {
                whereExpression = new List<Expression> ();
            }
        }

        public IQueryable<TSource> Query => _query;

        public IPagingQueryBuilder<TSource> ContainsFor (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others) {
            if (!parameter.HasSearchKey ()) {
                return this;
            }

            this._query = this._query.Contains (parameter.SearchKey, expression, others);
            return this;
        }

        public IPagingQueryBuilder<TSource> EndsFor (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others) {
            if (!parameter.HasSearchKey ()) {
                return this;
            }

            this._query = this._query.EndsWith (parameter.SearchKey, expression, others);
            return this;
        }

        public IPagingQueryBuilder<TSource> StartsFor (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others) {
            if (!parameter.HasSearchKey ()) {
                return this;
            }

            this._query = this._query.StartsWith (parameter.SearchKey, expression, others);
            return this;
        }

        public PageResult<TSource> ToPaging () {
            return this._query.ToPaging (this.parameter);
        }

        public async Task<PageResult<TSource>> ToPagingAsync () {
            return await this._query.ToPagingAsync (this.parameter);
        }

        public PageResult<TResult> ToPaging<TResult> (Expression<Func<TSource, TResult>> selector) {
            return this._query.ToPaging (this.parameter, selector);
        }

        public async Task<PageResult<TResult>> ToPagingAsync<TResult> (Expression<Func<TSource, TResult>> selector) {
            return await this._query.ToPagingAsync (this.parameter, selector);
        }
    }

    

}