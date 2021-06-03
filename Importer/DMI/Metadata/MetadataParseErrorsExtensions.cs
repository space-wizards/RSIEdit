namespace Importer.DMI.Metadata
{
    public static class MetadataParseErrorsExtensions
    {
        public static ParseError WithMessage(this MetadataErrors error, string message)
        {
            return new(error, message);
        }
    }
}
