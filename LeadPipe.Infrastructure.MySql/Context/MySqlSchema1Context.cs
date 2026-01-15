using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public sealed class MySqlSchema1Context(
    DbContextOptions<MySqlSchema1Context> options,
    IMySqlSettings settings
    ) : MySqlBaseContext(options, settings) { }
