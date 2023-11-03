using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Editor.Models;

public sealed class RepositoryLicenses
{
    public readonly List<(string, string)> Repositories = new();

    public readonly List<(Regex, string)> RepositoriesRegex = new();
}

[JsonSerializable(typeof(List<(string, string)>))]
[JsonSourceGenerationOptions]
internal sealed partial class RepoLicensesSourceGenerationContext : JsonSerializerContext
{
}
