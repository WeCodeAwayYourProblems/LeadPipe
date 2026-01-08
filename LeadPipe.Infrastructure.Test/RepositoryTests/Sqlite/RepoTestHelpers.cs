using LeadPipe.Infrastructure.Sqlite.Context;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Test.RepositoryTests.Sqlite;
public class RepoTestHelpers
{
    internal static PlumbingContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PlumbingContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PlumbingContext(options);
    }
}
