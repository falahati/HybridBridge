using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;

namespace HybridBridge.Win81
{
    /// <summary>
    ///     A simple implementation of <see cref="IUriToStreamResolver" /> interface
    ///     that search for files in <c>ms-appdata</c> directory
    /// </summary>
    public class UriToAppDataResolver : IUriToStreamResolver
    {
        /// <summary>
        ///     Handles http requests
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the request</param>
        /// <returns>A IInputStream compatible stream</returns>
        /// <exception cref="Exception"><see cref="Uri" /> is not supported</exception>
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new Exception();
            }
            return GetContent(uri.AbsolutePath).AsAsyncOperation();
        }

        private async Task<IInputStream> GetContent(string path)
        {
            // We use a package folder as the source, but the same principle should apply 
            // when supplying content from other locations 
            try
            {
                var localUri = new Uri(@"ms-appdata://" + path);
                var f = await StorageFile.GetFileFromApplicationUriAsync(localUri);
                var stream = await f.OpenAsync(FileAccessMode.Read);
                return stream;
            }
            catch (Exception)
            {
                throw new Exception("Invalid path");
            }
        }
    }
}