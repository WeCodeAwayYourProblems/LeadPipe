using System.Runtime.CompilerServices;

// Here, add the name of every csproj file with the .Test in the name that needs access to internal classes and items
[assembly: InternalsVisibleTo("Template.Domain.Test")]