using LeadPipe.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.Test.RepositoryTests;
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
