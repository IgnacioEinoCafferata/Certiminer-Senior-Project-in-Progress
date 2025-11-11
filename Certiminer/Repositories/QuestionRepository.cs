using Certiminer.Data;
using Certiminer.Models;
using Certiminer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Repositories;

public sealed class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _db;
    public QuestionRepository(ApplicationDbContext db) => _db = db;

    public Task<Question?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Questions.FirstOrDefaultAsync(q => q.Id == id, ct);

    // Usa FK: Question.TestId
    public async Task<IReadOnlyList<Question>> ListByTestAsync(int testId, CancellationToken ct = default) =>
        await _db.Questions
                 .AsNoTracking()
                 .Where(q => q.TestId == testId)
                 .OrderBy(q => q.Order) // o .OrderBy(q => q.Id);
                 .ToListAsync(ct);


    public Task AddAsync(Question entity, CancellationToken ct = default) =>
        _db.Questions.AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(Question entity)
    {
        _db.Questions.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.Questions.FindAsync(new object?[] { id }, ct);
        if (e is not null) _db.Questions.Remove(e);
    }
}
