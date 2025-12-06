using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces.Core;

internal interface ITransform<TIn, TOut> : ILoad<TIn>
{
    Task<Result<List<TOut>>> TransformAsync(List<TIn> data);
}
