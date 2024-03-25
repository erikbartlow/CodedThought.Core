using System.IO.Compression;

namespace CodedThought.Core.Data.ExcelSystem.Core
{
    public class ZipWorker : IDisposable
    {
        #region Members and Properties

        private const string FILE_rels = "workbook.{0}.rels";
        private const string FILE_sharedStrings = "sharedStrings.{0}";
        private const string FILE_sheet = "sheet{0}.{1}";
        private const string FILE_styles = "styles.{0}";
        private const string FILE_workbook = "workbook.{0}";
        private const string FOLDER_rels = "_rels";
        private const string FOLDER_worksheets = "worksheets";
        private const string FOLDER_xl = "xl";
        private const string TMP = "TMP_Z";
        private string _exceptionMessage;
        private string _format = "xml";
        private bool _isCleaned;
        private bool _isValid;
        private string _tempEnv;
        private string _tempPath;
        private string _xlPath;
        private byte[] buffer;

        private bool disposed;
        //private bool _isBinary12Format;

        /// <summary>Gets the exception message.</summary>
        /// <value>The exception message.</value>
        public string ExceptionMessage => _exceptionMessage;

        /// <summary>Gets a value indicating whether this instance is valid.</summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => _isValid;

        /// <summary>Gets the temp path for extracted files.</summary>
        /// <value>The temp path for extracted files.</value>
        public string TempPath => _tempPath;

        #endregion Members and Properties

        public bool Extract(Stream fileStream)
        {
            try
            {
                CleanFromTemp(false);
                NewTempPath();
                _isValid = true;
                using (ZipArchive archive = new(fileStream))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        entry.ExtractToFile($"{_tempPath}\\{entry.Name}");
                    }
                    return _isValid && CheckFolderTree();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Extracts the specified file name.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public bool Extract(String fileName)
        {
            try
            {
                CleanFromTemp(false);
                NewTempPath();
                _isValid = true;

                string zipFileNameAndPath = fileName;
                using (ZipArchive archive = ZipFile.OpenRead(zipFileNameAndPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        entry.ExtractToFile($"{_tempPath}\\{entry.Name}");
                    }
                    return _isValid && CheckFolderTree();
                }
            }
            catch (Exception ex)
            {
                _isValid = false;
                _exceptionMessage = ex.Message;

                CleanFromTemp(true); //true tells CleanFromTemp not to raise an IO Exception if this operation fails. If it did then the real error here would be masked

                throw;
            }
        }

        /// <summary>Gets the shared strings stream.</summary>
        /// <returns></returns>
        public Stream GetSharedStringsStream() => GetStream(Path.Combine(_xlPath, string.Format(FILE_sharedStrings, _format)));

        /// <summary>Gets the styles stream.</summary>
        /// <returns></returns>
        public Stream GetStylesStream() => GetStream(Path.Combine(_xlPath, string.Format(FILE_styles, _format)));

        /// <summary>Gets the workbook rels stream.</summary>
        /// <returns></returns>
        public Stream GetWorkbookRelsStream() => GetStream(Path.Combine(_xlPath, Path.Combine(FOLDER_rels, string.Format(FILE_rels, _format))));

        /// <summary>Gets the workbook stream.</summary>
        /// <returns></returns>
        public Stream GetWorkbookStream() => GetStream(Path.Combine(_xlPath, string.Format(FILE_workbook, _format)));

        /// <summary>Gets the worksheet stream.</summary>
        /// <param name="sheetId">The sheet id.</param>
        /// <returns></returns>
        public Stream GetWorksheetStream(int sheetId) => GetStream(Path.Combine(
                Path.Combine(_xlPath, FOLDER_worksheets),
                string.Format(FILE_sheet, sheetId, _format)));

        public Stream GetWorksheetStream(string sheetPath)
        {
            //its possible sheetPath starts with /xl. in this case trim the /xl
            if (sheetPath.StartsWith("/xl/"))
                sheetPath = sheetPath.Substring(4);
            return GetStream(Path.Combine(_xlPath, sheetPath));
        }

        /// <summary>Gets the stream.</summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private static Stream GetStream(string filePath)
        {
            return File.Exists(filePath) ? File.Open(filePath, FileMode.Open, FileAccess.Read) : (Stream?) null;
        }

        private bool CheckFolderTree()
        {
            _xlPath = Path.Combine(_tempPath, FOLDER_xl);

            return Directory.Exists(_xlPath) &&
                Directory.Exists(Path.Combine(_xlPath, FOLDER_worksheets)) &&
                File.Exists(Path.Combine(_xlPath, FILE_workbook)) &&
                File.Exists(Path.Combine(_xlPath, FILE_styles));
        }

        private void CleanFromTemp(bool catchIoError)
        {
            if (string.IsNullOrEmpty(_tempPath))
                return;

            _isCleaned = true;

            try
            {
                if (Directory.Exists(_tempPath))
                {
                    Directory.Delete(_tempPath, true);
                }
            }
            catch (IOException ex)
            {
                //TODO: minimally add some logging so we know this happened. log4net?
                if (!catchIoError)
                    throw ex;
            }
        }

        private void NewTempPath()
        {
            string tempID = Guid.NewGuid().ToString("N");
            _tempPath = Path.Combine(_tempEnv, TMP + DateTime.Now.ToFileTimeUtc().ToString() + tempID);

            _isCleaned = false;

            Directory.CreateDirectory(_tempPath);
        }

        /// <summary>Initializes a new instance of the <see cref="ZipWorker" /> class.</summary>
        public ZipWorker()
        {
            _tempEnv = System.IO.Path.GetTempPath();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (!_isCleaned)
                        CleanFromTemp(false);
                }

                buffer = null;

                disposed = true;
            }
        }

        ~ZipWorker()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
}