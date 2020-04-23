namespace CompressedStaticFiles.CompressionTypes
{
    public class Brotli : ICompressionType
    {
        public string Encoding => "br";
        public string Extension => ".br";
        public string ContentType => "application/brotli";
    }
}
