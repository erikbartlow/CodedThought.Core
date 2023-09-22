
namespace CodedThought.Core.Transport {
    public class CurlWrapper {
        private List<string> _urlArgs;
        public CurlWrapper() {
            _urlArgs = new List<string>();
        }
        /// <summary>
        /// Gets or sets the destination path.
        /// </summary>
        /// <value>
        /// The destination path.
        /// </value>
        public string DestinationPath { get; set; }
        /// <summary>
        /// Gets or sets the name of the destination file.
        /// </summary>
        /// <value>
        /// The name of the destination file.
        /// </value>
        public string DestinationFileName { get; set; }
        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        /// <value>
        /// The source URL.
        /// </value>
        public string SourceUrl { get; set; }
        /// <summary>
        /// Gets or sets the source URL arguments.
        /// </summary>
        /// <value>
        /// The source URL arguments.
        /// </value>
        public List<string> SourceUrlArguments {
            get { return _urlArgs; }
            set { _urlArgs = value; }
        }
        /// <summary>
        /// Gets or sets the source user name.
        /// </summary>
        /// <value>
        /// The source user name.
        /// </value>
        public string SourceUsername { get; set; }
        /// <summary>
        /// Gets or sets the source password.
        /// </summary>
        /// <value>
        /// The source password.
        /// </value>
        public string SourcePassword { get; set; }
        /// <summary>
        /// Gets the full destination file path.
        /// </summary>
        /// <value>
        /// The full destination file path.
        /// </value>
        public string FullDestinationFilePath {
            get {
                return Path.Combine(DestinationPath, DestinationFileName);
            }
        }
        /// <summary>
        /// Gets the full source URL.
        /// </summary>
        /// <value>
        /// The full source URL.
        /// </value>
        public string FullSourceUrl {
            get {
                return String.Format("{0}?{1}", SourceUrl, string.Join("&", _urlArgs)); //https://hrdd.corp.CodedThought.com:8443/ib/DownloadSecure?id=72126&file=2&password=dh7495hng734js83
            }
        }

        /// <summary>
        /// Download the file.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pathToSave"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task DownloadFile(string url, string pathToSave, string fileName) {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new InvalidOperationException("URI is invalid.");

            var content = await GetUrlContent(url);
            if (content != null) {
                if (!Directory.Exists(pathToSave)) Directory.CreateDirectory(pathToSave);
                await File.WriteAllBytesAsync($"{pathToSave}/{fileName}", content);
            }
        }

        public async Task<byte[]> GetUrlContent(string url) {
            using var client = new HttpClient();
            return await client.GetByteArrayAsync(url);
        }
    }
}
