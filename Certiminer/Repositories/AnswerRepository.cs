using Certiminer.Data;
using Certiminer.Models;
using Certiminer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Certiminer.Repositories;

public sealed class AnswerRepository : IAnswerRepository
{
    private readonly ApplicationDbContext _db;
    public AnswerRepository(ApplicationDbContext db) => _db = db;

    // Usa tabla AnswerQuestions (plural) y FK QuestionId
    public async Task<IReadOnlyList<AnswerQuestion>> ListByQuestionAsync(int questionId, CancellationToken ct = default) =>
        await _db.AnswerQuestions
                 .Where(a => a.QuestionId == questionId)
                 .OrderBy(a => a.Id)      // si no hay Id, avisame y lo cambio
                 .ToListAsync(ct);
    public async Task<IReadOnlyList<string>> GetAnswerTextsByQuestionAsync(int questionId, CancellationToken ct = default) =>
    await _db.AnswerQuestions
             .Where(a => a.QuestionId == questionId)
             .OrderBy(a => a.Id)
             .Select(a => a.Text)
             .ToListAsync(ct);


    public Task<AnswerQuestion?> GetCorrectAsync(int questionId, CancellationToken ct = default) =>
        _db.AnswerQuestions.FirstOrDefaultAsync(a => a.QuestionId == questionId && a.IsCorrect, ct);

    public Task AddAsync(AnswerQuestion entity, CancellationToken ct = default) =>
        _db.AnswerQuestions.AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(AnswerQuestion entity)
    {
        _db.AnswerQuestions.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var e = await _db.AnswerQuestions.FindAsync(new object?[] { id }, ct);
        if (e is not null) _db.AnswerQuestions.Remove(e);
    }
}
