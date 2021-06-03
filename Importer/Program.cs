using Importer.DMI.Metadata;

namespace Importer
{
    public class Program
    {
        public static void Main()
        {
            new MetadataParser().TryGetFileMetadata(
                ".\\solsticeicons\\dmi\\icons\\obj\\computer.dmi", out var metadata, out var error);
        }
    }
}
