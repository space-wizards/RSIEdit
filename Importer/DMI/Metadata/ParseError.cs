using JetBrains.Annotations;

namespace Importer.DMI.Metadata
{
    [PublicAPI]
    public class ParseError
    {
        public ParseError(MetadataErrors error, string message)
        {
            Error = error;
            Message = message;
        }

        public MetadataErrors Error { get; }

        public string Message { get; }
    }
}
