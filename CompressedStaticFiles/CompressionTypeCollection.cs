using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CompressedStaticFiles.CompressionTypes;

namespace CompressedStaticFiles
{
    public class CompressionTypeCollection : Collection<ICompressionType>
    {
        public IEnumerable<string> Encodings => this.Select(c => c.Encoding);
        public IEnumerable<string> Extensions => this.Select(c => c.Extension);

        /// <summary>
        /// Adds a type representing an <see cref="ICompressionType"/>.
        /// </summary>
        public void Add<TCompressionType>() where TCompressionType : ICompressionType
        {
            Add(typeof(TCompressionType));
        }

        /// <summary>
        /// Adds a type representing an <see cref="ICompressionType"/>.
        /// </summary>
        /// <param name="compressionType">Type representing an <see cref="ICompressionType"/>.</param>
        public void Add(Type compressionType)
        {
            if (compressionType == null)
            {
                throw new ArgumentNullException(nameof(compressionType));
            }

            if (!typeof(ICompressionType).IsAssignableFrom(compressionType))
            {
                throw new ArgumentException($"The provider must implement {nameof(ICompressionType)}.", nameof(compressionType));
            }

            Add((ICompressionType)Activator.CreateInstance(compressionType));
        }
    }
}
