using LetterService.Models.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace LetterService.Database.Repositories;

public class UnitOfWork : IDisposable
{
    #region  Fields
    
    private ApplicationContext _context;
    private IDbContextTransaction _transaction;
    
    #endregion
    
    #region Repositories
    
    private GenericRepository<Letter> _letterRepository;
    private GenericRepository<FavouritesInfo> _favouritesRepository;
    
    #endregion

    #region Constructor

    public UnitOfWork(ApplicationContext context)
    {
        _context = context;
    }

    #endregion

    #region Properties
    
    
    public GenericRepository<Letter> LetterRepository
    {
        get
        {
            if (this._letterRepository == null)
            {
                this._letterRepository = new GenericRepository<Letter>(_context);
            }
            return _letterRepository;
        }
    }
    public GenericRepository<FavouritesInfo> FavouritesRepository
    {
        get
        {
            if (this._favouritesRepository == null)
            {
                this._favouritesRepository = new GenericRepository<FavouritesInfo>(_context);
            }
            return _favouritesRepository;
        }
    }
    #endregion

    #region Methods

    public bool Save()
    {
        return _context.SaveChanges() > 0;
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    
    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
        this.disposed = true;
    }
    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction has already been started.");
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    public async Task CommitAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("A transaction has not been started.");
    
        try
        {
            await _transaction.CommitAsync();
            _transaction.Dispose();
            _transaction = null;
        }
        catch (Exception)
        {
            if (_transaction is not null)
                await _transaction.RollbackAsync();
            throw;
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
