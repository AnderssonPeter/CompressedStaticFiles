namespace CompressedStaticFiles.CompressionTypes
{
    public interface ICompressionType
    {
        /// <summary>
        /// The Content-Encoding entity header that is used to compress the media-type.
        /// For gzip the value is 'gzip' and for brotli it is 'br'
        /// </summary>
        string Encoding { get; }

        /// <summary>
        /// The file extension of the compressed files.
        /// For gzip the value is '.gz' and for brotli it is '.br'
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// The Content-Type entity header that is used to indicate the media type of the resource.
        /// For gzip the value is 'application/x-gzip' and for brotli it is 'application/brotli'
        /// </summary>
        string ContentType { get; }
    }
}
