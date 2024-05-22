namespace CodedThought.Core.Utility.Email
{

    /// <summary>Summary description for TextFileReader.</summary>
    public class TextFileReader
    {

        #region Data

        protected TextReader _tr;
        protected string _fileName;
        protected string _text;

        #endregion Data

        #region Properties

        /// <summary>Gets or sets the file name and path of the file to read.</summary>
        public string FileName
        {
            get => _fileName;
            set => _fileName = value;
        }

        /// <summary>Gets the contents of the file.</summary>
        public string Text => _text;

        #endregion Properties

        #region Methods

        /// <summary>Opens the _text file.</summary>
        public void Open()
        {
            try
            {
                _tr = new StreamReader(_fileName);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        /// <summary>Opens the _text file with the passed file name and path.</summary>
        /// <param name="filename"></param>
        public void Open(string filename)
        {
            _fileName = filename;
            try
            {
                Open();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public void Close()
        {
            try
            {
                _tr.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        /// <summary>Replaces _text within the _text with the value passed.</summary>
        /// <param name="token">     </param>
        /// <param name="tokenValue"></param>
        public void ReplaceToken(string token, string tokenValue)
        {
            if (_text == String.Empty)
            {
                _text = _tr.ReadToEnd();
            }
            _text = _text.Replace(token, tokenValue);
        }

        public string Read()
        {
            if (_text != String.Empty)
            {
                return _text;
            }
            else if (_tr != null)
            {
                _text = _tr.ReadToEnd();
            }
            return _text;
        }

        #endregion Methods

        #region Constructors

        /// <summary>Instantiates the TextFileReader class.</summary>
        public TextFileReader() => _text = String.Empty;


        /// <summary>Instantiates the TextFileReader class with the passed filename.</summary>
        /// <param name="fileName"></param>
        public TextFileReader(string fileName)
        {
            _text = String.Empty;
            try
            {
                Open(fileName);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        #endregion Constructors
    }
}