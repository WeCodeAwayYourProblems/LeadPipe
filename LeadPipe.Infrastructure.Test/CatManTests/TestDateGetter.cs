using CSharpFunctionalExtensions;
using LeadPipe.Infrastructure.Data.Source;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Repository.Sqlite;
using LeadPipe.Infrastructure.Interfaces.Service;
using NSubstitute;

namespace LeadPipe.Infrastructure.Test.CatManTests;

public class TestDateGetter
{
    private readonly ICatManService _cat = Substitute.For<ICatManService>();
    private readonly ISyncStateRepository _repo = Substitute.For<ISyncStateRepository>();
    private CatManDataSource Source => new(_cat, _repo);
    [Fact]
    public void DateGetter_WorksProperly()
    {
        // Assemble
        DateTimeOffset expected = DateTimeOffset.UtcNow;
        long unixTime = expected.ToUnixTimeSeconds();
        Result<List<CatManDto>> setup = Result.Success<List<CatManDto>>([new CatManDto() { unix_time = unixTime }]);

        // Act
        DateTimeOffset actual = Source.GetDate(setup);

        // Assert
        Assert.Equal(expected.ToUnixTimeSeconds(), actual.ToUnixTimeSeconds());
    }
}
