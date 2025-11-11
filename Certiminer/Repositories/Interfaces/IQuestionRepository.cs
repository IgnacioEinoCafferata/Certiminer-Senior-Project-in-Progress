using Certiminer.Models;

namespace Certiminer.Repositories.Interfaces;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Question>> ListByTestAsync(int testId, CancellationToken ct = default); // usa Question.TestId
    Task AddAsync(Question entity, CancellationToken ct = default);
    Task UpdateAsync(Question entity);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
