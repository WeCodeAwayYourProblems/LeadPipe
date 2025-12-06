using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Interfaces;

internal interface ITransform<TIn, TOut> : ILoad<TIn>
{
    Task<Result<List<TOut>>> TransformAsync(List<TIn> data);
}
