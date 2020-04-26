namespace CompressedStaticFiles.CompressionTypes
{
    internal class Gzip : ICompressionType
    {
        public string Encoding => "gzip";
        public string Extension => ".gz";
        public string ContentType => "application/x-gzip";
    }
}
