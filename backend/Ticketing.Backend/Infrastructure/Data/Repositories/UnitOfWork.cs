using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IFieldDefinitionRepository? _fieldDefinitions;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IFieldDefinitionRepository FieldDefinitions
    {
        get
        {
            return _fieldDefinitions ??= new FieldDefinitionRepository(_context);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}


