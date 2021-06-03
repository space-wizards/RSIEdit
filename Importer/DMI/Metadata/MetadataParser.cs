using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MetadataExtractor.Formats.Png;
using static Importer.DMI.Metadata.MetadataErrors;

namespace Importer.DMI.Metadata
{
    public class MetadataParser : IMetadataParser
    {
        private const string Header = "BEGIN DMI";

        private bool TryGetFileDmiTag(string filePath, [NotNullWhen(true)] out RawMetadata? rawData)
        {
            var data = PngMetadataReader.ReadMetadata(filePath);

            foreach (var datum in data)
            {
                foreach (var tag in datum.Tags)
                {
                    var hasTags = tag.Description?.Contains(Header) ?? false;

                    if (hasTags)
                    {
                        rawData = new RawMetadata(tag.Description!);
                        return true;
                    }
                }
            }

            rawData = null;
            return false;
        }

        public bool TryGetFileMetadata(
            string filePath,
            [NotNullWhen(true)] out IMetadata? metadata,
            [NotNullWhen(false)] out ParseError? error)
        {
            if (!TryGetFileDmiTag(filePath, out var raw))
            {
                metadata = null;
                error = NoDmiTag.WithMessage($"No dmi tag found in file {filePath}");
                return false;
            }

            if (!raw.Next() || !raw.TryVersion(out var version))
            {
                metadata = null;
                error = NoVersion.WithMessage($"No version found in file {filePath}");
                return false;
            }

            var states = new List<DmiState>();

            do
            {
                if (raw.TryState(out var state))
                {
                    states.Add(state);
                }
            } while (raw.Next());

            metadata = new Metadata(version, states);
            error = null;
            return true;
        }
    }
}
