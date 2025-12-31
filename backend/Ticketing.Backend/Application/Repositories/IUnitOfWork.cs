namespace Ticketing.Backend.Application.Repositories;

/// <summary>
/// Unit of Work pattern to coordinate multiple repository operations
/// and ensure a single transaction boundary for SaveChanges operations
/// </summary>
public interface IUnitOfWork
{
    IFieldDefinitionRepository FieldDefinitions { get; }
    
    /// <summary>
    /// Saves all changes made in this unit of work to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


