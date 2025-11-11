using Certiminer.Models;

namespace Certiminer.Repositories.Interfaces;

public interface IAnswerRepository
{
    Task<IReadOnlyList<string>> GetAnswerTextsByQuestionAsync(int questionId, CancellationToken ct = default);

    Task<IReadOnlyList<AnswerQuestion>> ListByQuestionAsync(int questionId, CancellationToken ct = default);
    Task<AnswerQuestion?> GetCorrectAsync(int questionId, CancellationToken ct = default); // requiere bool IsCorrect
    Task AddAsync(AnswerQuestion entity, CancellationToken ct = default);
    Task UpdateAsync(AnswerQuestion entity);
    Task DeleteAsync(int id, CancellationToken ct = default); // si tu AnswerQuestion no tiene Id, avisame y lo cambio
}
