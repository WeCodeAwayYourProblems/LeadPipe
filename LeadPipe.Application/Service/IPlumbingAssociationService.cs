using CSharpFunctionalExtensions;
using LeadPipe.Domain.ValueObjects;

namespace LeadPipe.Application.Service;

public interface IPlumbingAssociationService
{
    Task<Result> SaveLinksAsync();
}
