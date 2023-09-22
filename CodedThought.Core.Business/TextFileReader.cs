namespace CodedThought.Core.Business {

	/// <summary>Summary description for TextFileReader.</summary>
	public class TextFileReader {

		#region Data

		protected TextReader tr;
		protected string fileName;
		protected string text;

		#endregion Data

		#region Properties

		/// <summary>Gets or sets the file name and path of the file to read.</summary>
		public string FileName {
			get { return this.fileName; }
			set { this.fileName = value; }
		}

		/// <summary>Gets the contents of the file.</summary>
		public string Text {
			get {
				return this.text;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>Opens the text file.</summary>
		public void Open() {
			try {
				tr = new StreamReader(this.fileName);
			} catch (Exception e) {
				throw new Exception(e.Message, e);
			}
		}

		/// <summary>Opens the text file with the passed file name and path.</summary>
		/// <param name="filename"></param>
		public void Open(string filename) {
			this.fileName = filename;
			try {
				this.Open();
			} catch (Exception e) {
				throw new Exception(e.Message, e);
			}
		}

		public void Close() {
			try {
				tr.Close();
			} catch (Exception e) {
				throw new Exception(e.Message, e);
			}
		}

		/// <summary>Replaces text within the text with the value passed.</summary>
		/// <param name="token">     </param>
		/// <param name="tokenValue"></param>
		public void ReplaceToken(string token, string tokenValue) {
			if (this.text == String.Empty) {
				this.text = tr.ReadToEnd();
			}
			this.text = this.text.Replace(token, tokenValue);
		}

		public string Read() {
			if (this.text != String.Empty) {
				return this.text;
			} else if (tr != null) {
				this.text = tr.ReadToEnd();
			}
			return this.text;
		}

		#endregion Methods

		#region Constructors

		/// <summary>Instantiates the TextFileReader class.</summary>
		public TextFileReader() {
			this.text = String.Empty;
		}

		/// <summary>Instantiates the TextFileReader class with the passed filename.</summary>
		/// <param name="fileName"></param>
		public TextFileReader(string fileName) {
			this.text = String.Empty;
			try {
				this.Open(fileName);
			} catch (Exception e) {
				throw new Exception(e.Message, e);
			}
		}

		#endregion Constructors
	}
}