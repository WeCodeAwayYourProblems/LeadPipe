using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public sealed class MySqlSchema2Context(
    DbContextOptions<MySqlSchema2Context> options,
    IMySqlSettings settings,
    bool allowWrites = false
    ) : MySqlBaseContext(options, settings, allowWrites)
{ }
