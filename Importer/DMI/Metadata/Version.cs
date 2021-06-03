namespace Importer.DMI.Metadata
{
    public record Version(double VersionNumber = 0, int Width = -1, int Height = -1)
    {
        public bool Valid()
        {
            return VersionNumber != 0 && Width != -1 && Height != -1;
        }
    }
}
