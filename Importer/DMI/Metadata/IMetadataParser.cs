using System.Diagnostics.CodeAnalysis;

namespace Importer.DMI.Metadata
{
    public interface IMetadataParser
    {
        bool TryGetFileMetadata(
            string filePath,
            [NotNullWhen(true)] out IMetadata? metadata,
            [NotNullWhen(false)] out ParseError? error);
    }
}
