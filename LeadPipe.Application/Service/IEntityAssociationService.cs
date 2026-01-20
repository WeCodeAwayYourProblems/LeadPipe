using CSharpFunctionalExtensions;

namespace LeadPipe.Application.Service;

public interface IEntityAssociationService
{
    Task<Result> AssociateAsync();
}
