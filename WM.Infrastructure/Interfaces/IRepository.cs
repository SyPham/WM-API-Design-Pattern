using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WM.Infrastructure.Interfaces
{
    public interface IRepository<T, K> where T : class
    {
        Task<T> FindByIdAsync(K id, params Expression<Func<T, object>>[] includeProperties);

        Task<T> FindSingleAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);

        IQueryable<T> FindAll(params Expression<Func<T, object>>[] includeProperties);

        IQueryable<T> FindAll(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);

        Task AddAsync(T entity);
        Task AddMultipleAsync(List<T> entity);
        void Update(T entity);

        void Remove(T entity);

        Task RemoveAsync(K id);

        void RemoveMultiple(List<T> entities);
    }
}
