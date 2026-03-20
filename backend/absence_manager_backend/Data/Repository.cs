using Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public class Repository<T> where T : class, IIdEntity
    {
        RepositoryContext ctx;

        public Repository(RepositoryContext ctx)
        {
            this.ctx = ctx;
        }
        public Repository()
        {
        }

        public virtual void Add(T entity)
        {
            ctx.Set<T>().Add(entity);
            ctx.SaveChanges();
        }

        public virtual void DeleteById(string id)
        {
            var entity = FindById(id);
            ctx.Set<T>().Remove(entity);
            ctx.SaveChanges();
        }

        public virtual void Delete(T entity)
        {
            ctx.Set<T>().Remove(entity);
            ctx.SaveChanges();
        }

        public virtual void Update(T entity)
        {
            var old = FindById(entity.Id);

            ctx.Entry(old).CurrentValues.SetValues(entity);

            ctx.SaveChanges();
        }

        public virtual T FindById(string id)
        {
            var entity = ctx.Set<T>().Find(id);
            if (entity == null) throw new KeyNotFoundException($"{typeof(T).Name} not found: {id}");
            return entity;
        }

        public virtual IQueryable<T> GetAll()
        {
            return ctx.Set<T>();
        }
    }
}