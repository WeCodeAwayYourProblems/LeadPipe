using LeadPipe.Infrastructure.MySql.Settings;
using Microsoft.EntityFrameworkCore;

namespace LeadPipe.Infrastructure.MySql.Context;

public sealed class MySqlSchema3Context(
    DbContextOptions<MySqlSchema3Context> options,
    IMySqlSettings settings
) : MySqlBaseContext(options, settings) { }
