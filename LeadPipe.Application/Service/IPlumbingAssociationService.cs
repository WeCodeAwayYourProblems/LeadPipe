using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface IPlumbingAssociationService
{
    Task<Result<List<Plumbing>>> GetPlumbingAsync();
    Task<Result<List<Sandwich>>> GetSandwichAsync();
    Task<Result<List<Call>>> GetCallAsync();
    Task<Result> SaveAllAsync(List<Plumbing> plumb, List<Sandwich> subs, List<Call> calls);
}
