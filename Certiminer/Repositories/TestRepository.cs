using Certiminer.Data;
using Certiminer.Models;
using Certiminer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Repositories;

public sealed class TestRepository : ITestRepository
{
    private readonly ApplicationDbContext _db;
    public TestRepository(ApplicationDbContext db) => _db = db;

    public Task<Test?> GetByIdAsync(int id, bool includeQuestions = false, CancellationToken ct = default)
    {
        IQueryable<Test> q = _db.Tests;
        if (includeQuestions) q = q.Include(t => t.Questions);
        return q.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<string?> GetTitleAsync(int id, CancellationToken ct = default) =>
    await _db.Tests
             .Where(t => t.Id == id)
             .Select(t => t.Title)
             .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Test>> ListAsync(CancellationToken ct = default) =>
        await _db.Tests.OrderBy(t => t.Title).ToListAsync(ct);

    // Usa Test.FolderId (según tu OnModelCreating)
    public async Task<IReadOnlyList<Test>> ListByFolderAsync(int folderId, CancellationToken ct = default) =>
        await _db.Tests.Where(t => t.FolderId == folderId)
                       .OrderBy(t => t.Title)
                       .ToListAsync(ct);

    public Task AddAsync(Test entity, CancellationToken ct = default) =>
        _db.Tests.AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(Test entity)
    {
        _db.Tests.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Tests.FindAsync(new object?[] { id }, ct);
        if (e is not null) _db.Tests.Remove(e);
    }
}
