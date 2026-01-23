using CSharpFunctionalExtensions;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.MySql;

public static class ResultAssertions
{
    public static void ShouldBeSuccess<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    public static void ShouldBeFailure<T>(Result<T> result)
    {
        Assert.True(result.IsFailure);
    }
}
