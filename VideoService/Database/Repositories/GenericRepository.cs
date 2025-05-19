using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace VideoService.Database.Repositories;

public class GenericRepository<TEntity> where TEntity : class
{
    internal ApplicationContext context;
    internal DbSet<TEntity> dbSet;

    public GenericRepository(ApplicationContext context)
    {
        this.context = context;
        this.dbSet = context.Set<TEntity>();
    }

    public virtual IQueryable<TEntity> Get(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = "")
    {
        IQueryable<TEntity> query = dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
                     (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            return orderBy(query);
        }
        else
        {
            return query;
        }
    }

    public virtual TEntity? GetByID(object id)
    {
        return dbSet.Find(id);
    }
    
    public async virtual Task<TEntity?> GetByIDAsync(object id)
    {
        return await dbSet.FindAsync(id);
    }
    public virtual void Insert(TEntity entity)
    {
        dbSet.Add(entity);
    }
    public virtual void InsertRange(IEnumerable<TEntity> entities)
    {
        dbSet.AddRange(entities);
    }
    public async virtual Task<EntityEntry<TEntity>> InsertAsync(TEntity entity)
    {
        var entry = await dbSet.AddAsync(entity);
        return entry;
    }
    public virtual void Delete(object id)
    {
        TEntity? entityToDelete = dbSet.Find(id);

        if (entityToDelete == null)
        {
            // It's probably a good idea to throw an error here
            // but I'm leaving it as is for now
            return;
        }

        Delete(entityToDelete);
    }
    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        dbSet.RemoveRange(entities);
    }
    public virtual void Delete(TEntity entityToDelete)
    {
        if (context.Entry(entityToDelete).State == EntityState.Detached)
        {
            dbSet.Attach(entityToDelete);
        }
        dbSet.Remove(entityToDelete);
    }

    public virtual void Update(TEntity entityToUpdate)
    {
        dbSet.Attach(entityToUpdate);
        context.Entry(entityToUpdate).State = EntityState.Modified;
    }
}
