using Certiminer.Data;
using Certiminer.Models;
using Certiminer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Repositories;

public sealed class TestAttemptRepository : ITestAttemptRepository
{
    private readonly ApplicationDbContext _db;
    public TestAttemptRepository(ApplicationDbContext db) => _db = db;

    public Task<TestAttempt?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.TestAttempts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<TestAttempt>> ListByUserAsync(string userId, CancellationToken ct = default) =>
        await _db.TestAttempts.Where(a => a.UserId == userId)
                              .OrderByDescending(a => a.Id)
                              .ToListAsync(ct);

    public Task AddAsync(TestAttempt entity, CancellationToken ct = default) =>
        _db.TestAttempts.AddAsync(entity, ct).AsTask();
    public async Task<bool> HasCompletedAsync(string userId, int testId, CancellationToken ct = default) =>
    await _db.TestAttempts
             .AsNoTracking()
             .AnyAsync(a => a.UserId == userId && a.TestId == testId, ct);


    public Task UpdateAsync(TestAttempt entity)
    {
        _db.TestAttempts.Update(entity);
        return Task.CompletedTask;
    }
}
