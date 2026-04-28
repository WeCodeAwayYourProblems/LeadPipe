using CSharpFunctionalExtensions;
using LeadPipe.Core;
using LeadPipe.Infrastructure.Data.DataSource;
using LeadPipe.Infrastructure.Dto;
using LeadPipe.Infrastructure.Interfaces.Repository;
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
        long unixTime = expected.ToUnixTime();
        Result<List<CatManDto>> setup = Result.Success<List<CatManDto>>([new CatManDto() { unix_time = unixTime }]);

        // Act
        DateTimeOffset actual = Source.GetDate(setup);

        // Assert
        Assert.Equal(expected.ToUnixTime(), actual.ToUnixTime());
    }
}
