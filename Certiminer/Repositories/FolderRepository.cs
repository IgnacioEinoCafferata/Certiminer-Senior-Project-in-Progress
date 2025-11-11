using Certiminer.Data;
using Certiminer.Models;
using Certiminer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Repositories;

public sealed class FolderRepository : IFolderRepository
{
    private readonly ApplicationDbContext _db;
    public FolderRepository(ApplicationDbContext db) => _db = db;

    public Task<Folder?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Folders.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<Folder>> ListAsync(CancellationToken ct = default) =>
        await _db.Folders.OrderBy(f => f.Kind).ThenBy(f => f.Name).ToListAsync(ct);

    public Task AddAsync(Folder entity, CancellationToken ct = default) =>
        _db.Folders.AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(Folder entity)
    {
        _db.Folders.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Folders.FindAsync(new object?[] { id }, ct);
        if (e is not null) _db.Folders.Remove(e);
    }
}
