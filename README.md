# Instructions for Creating and Using a Custom C# Project Template with dotnet CLI
================================================================================

## Creating the Template
---------------------
### 1. Ensure the root folder of your C# project contains a `.template.config` folder with a `template.json` file.
   Example `template.json` content:

   ```{
      "$schema": "https://json.schemastore.org/template",
      "author": "WeCodeAwayYourProblems",
      "identity": "CleanArch.Template",
      "name": "Clean Architecture Template",
      "shortName": "clean-arch-template",
      "sourceName": "Template", // Template will be replaced throughout the solution with whatever name you choose for your new solution
      "preferNameDirectory": true,
      "tags": {
         "language": "C#",
         "type": "project"
      }
   }
   ```

### 2. Pack your template using the dotnet CLI:

```dotnet new -i <path-to-your-template-folder>```

   Alternatively, if you want to pack it as a NuGet package:

```dotnet pack```

## Using the Template
------------------
### 1. After installing the template, you can create a new project using:

```dotnet new clean-arch-template -n MyNewProject```

   This will create a new project named 'MyNewProject' using your custom template.

   This will also replace whatever you put as "sourceName" with 'MyNewProject'.
   In explanation, if you put "sourceName": "Template" in your template.json, then every instance of Template will be replaced with 'MyNewProject'.

### 2. To uninstall the template:

```dotnet new -u <path-to-your-template-folder> or <template-package-name>```

## Notes
-----
- Make sure your template folder is structured correctly and includes all necessary files.
- You can test your template locally before publishing it to NuGet.


-----

# EF Core Migrations Guide for Template Users

This template includes two database context patterns:
- **DwhContext<T>** (for MySQL)
- **SqliteContext<T>** (for SQLite)

By default, these contexts are *generic*, which makes them great for simple querying, but not ideal for EF migrations. If you want to use migrations, follow these steps.

---

## 1. Install EF Core Tools
Make sure you have the EF Core CLI tools installed:

```dotnet tool install --global dotnet-ef```

Verify installation:

```dotnet ef --version```

---

## 2. Install Database Providers
The project already references MySQL and SQLite EF Core providers. If needed, install them manually:

```
dotnet add package MySQl.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

---

## 3. Create a Concrete Migration Context
Migrations work best when EF Core can "see" the **entire schema** at once.  
Generic contexts (like `DwhContext<T>` or `SqliteContext<T>`) are not well-suited for migrations.

To use migrations, create a concrete `DbContext` that includes **all entities you want in the migration**:

```public class MySqlMigrationContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseMySql(
            "Server=localhost;Database=mydb;User=myuser;Password=mypass;",
            new MySqlServerVersion(new Version(8, 0, 34))
        );
    }
}

public class SqliteMigrationContext : DbContext
{
    public DbSet<CacheItem> CacheItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=local.db");
    }
}
```

---

## 4. Add a Migration
Run the following from the project root:

### For MySQL

```dotnet ef migrations add InitMySql -c MySqlMigrationContext -o Migrations/MySql```

### For SQLite

```dotnet ef migrations add InitSqlite -c SqliteMigrationContext -o Migrations/Sqlite```

---

## 5. Apply the Migration
Apply migrations to the database:

### For MySQL

```dotnet ef database update -c MySqlMigrationContext```

### For SQLite

```dotnet ef database update -c SqliteMigrationContext```

---

## 6. Notes
- Always specify the `-c` (`--context`) argument to tell EF which context to use.
- The `-o` (`--output-dir`) argument sets the folder where migrations are stored.
- Each provider (MySQL, SQLite) should have its own separate migrations folder.
- If you only need lightweight/local SQLite storage, you can skip migrations and just call:

```context.Database.EnsureCreated();```

---

## 7. Summary
- Keep using `DwhContext<T>` and `SqliteContext<T>` for querying.
- Use `MySqlMigrationContext` and `SqliteMigrationContext` for migrations.
- Run `dotnet ef migrations add ...` with `-c` and `-o` options to manage changes.
