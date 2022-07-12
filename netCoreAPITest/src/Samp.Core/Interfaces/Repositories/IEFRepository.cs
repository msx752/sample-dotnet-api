﻿using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Samp.Core.Interfaces.Repositories
{
    public interface IEFRepository : IDisposable
    {
    }

    public interface IEFRepository<T>
        : IEFRepository
        where T : class
    {
        T Add(T entity);

        void Add(params T[] entities);

        void Add(IEnumerable<T> entities);

        IQueryable<T> All(bool includesInActives = false);

        T Delete(T entity);

        void Delete(params T[] entities);

        void Delete(IEnumerable<T> entities);

        T FirstOrDefault(Expression<Func<T, bool>> predicate, bool includesInAvtives = false);

        T GetById(object id, bool includesInActives = false);

        T Search(bool includesInActives = false, params object[] keyValues);

        T Single(Expression<Func<T, T>> selector, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool disableTracking = true, bool includesInActives = false);

        T Update(T entity);

        void Update(params T[] entities);

        void Update(IEnumerable<T> entities);

        IEnumerable<T> Where(Expression<Func<T, bool>> predicate, bool includesInActives = false);
    }
}