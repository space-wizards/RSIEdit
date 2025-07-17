﻿using System.Text.Json.Serialization;

namespace Editor.Models;

public sealed class Preferences
{
    private const int CurrentVersion = 1;

    public Preferences()
    {
        Version = CurrentVersion;
    }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("defaultLicense")]
    public string? DefaultLicense { get; set; }

    [JsonPropertyName("defaultCopyright")]
    public string? DefaultCopyright { get; set; }

    [JsonPropertyName("githubToken")]
    public string? GitHubToken { get; set; }

    [JsonPropertyName("minifyJson")]
    public bool MinifyJson { get; set; } = false;

    [JsonPropertyName("easterEggs")]
    public bool EasterEggs { get; set; }
}

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(Preferences))]
internal sealed partial class PreferencesJsonContext : JsonSerializerContext
{

}
