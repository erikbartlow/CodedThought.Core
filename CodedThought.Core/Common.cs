using CodedThought.Core.Data;
using System.Net;

namespace CodedThought.Core {

	internal class Common {

		#region Enumerators

		public enum ExtractExistingFileAction {
			Throw = 0,
			OverwriteSilently = 1,
			DoNotOverwrite = 2,
			InvokeExtractProgressEvent = 3,
		}

		#endregion Enumerators

		#region Methods

		/// <summary>Gets the type of the database.</summary>
		/// <param name="providerType">Type of the provider.</param>
		/// <returns></returns>
		public static DBSupported GetDatabaseType(string providerType) {
			return (DBSupported)Enum.Parse(typeof(DBSupported), providerType);
		}

		/// <summary>Gets the bin path.</summary>
		/// <returns></returns>
		public static string GetBinPath() {
			return AppDomain.CurrentDomain.BaseDirectory != String.Empty
				? AppDomain.CurrentDomain.BaseDirectory
				: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
		}

		/// <summary>Converts the passed string into proper names in a string. For example: FirstNameLastName passed would return "First Name Last Name".</summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static string ToProperName(string input) {
			return System.Text.RegularExpressions.Regex.Replace(input, @"([a-z])([A-Z])", @"$1 $2", System.Text.RegularExpressions.RegexOptions.None);
		}

		/// <summary>Capitalizes the text.</summary>
		/// <param name="input">The input.</param>
		/// <returns></returns>
		public static string CapitalizeText(string input) {
			string pattern = @"\w+";
			return System.Text.RegularExpressions.Regex.Replace(input, pattern, new MatchEvaluator(CapText));
		}

		/// <summary>Caps the text.</summary>
		/// <param name="m">The match expression.</param>
		/// <returns></returns>
		private static string CapText(Match m) {
			string x = m.ToString();
			return char.IsLower(x[0]) ? char.ToUpper(x[0]) + x[1..] : x;
		}

		/// <summary>Returns the difference between two dates.</summary>
		/// <param name="howtocompare"></param>
		/// <param name="startDate">   </param>
		/// <param name="endDate">     </param>
		/// <returns></returns>
		public static double DateDiff(string howtocompare, DateTime startDate, DateTime endDate) {
			try {
				TimeSpan TS = new(startDate.Ticks - endDate.Ticks);

				return howtocompare.ToLower() switch {
					"m" => Convert.ToDouble(TS.TotalMinutes),
					"s" => Convert.ToDouble(TS.TotalSeconds),
					"t" => Convert.ToDouble(TS.Ticks),
					"mm" => Convert.ToDouble(TS.TotalMilliseconds),
					"yyyy" => Convert.ToDouble(TS.TotalDays / 365),
					"q" => Convert.ToDouble((TS.TotalDays / 365) / 4),
					_ => Convert.ToDouble(TS.TotalDays)
				};

			} catch {
				return -1;
			}
		}

		/// <summary>Checks whether a date is valid or not</summary>
		/// <param name="inValue">Input value</param>
		/// <returns>True or False</returns>
		public static Boolean IsDate(object inValue) {
			Boolean bValid;
			try {
				DateTime myDT;
				if (inValue != null)
					myDT = DateTime.Parse(s: inValue.ToString());
				bValid = true;
			} catch (FormatException e) {
				Console.Write(e.Message);
				bValid = false;
			}
			return bValid;
		}

		/// <summary>Checks a value is numeric or not.</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Boolean IsNumeric(object value) {
			try {
				int i;
				if (value != null)
					i = Convert.ToInt32(value.ToString());
				return true;
			} catch (FormatException) {
				return false;
			}
		}

		/// <summary>Converts the SQL bit to boolean.</summary>
		/// <param name="value">The value.</param>
		/// <returns>Boolean</returns>
		public static Boolean ConvertSqlBitToBoolean(int value) {
			return value <= 0;
		}

		/// <summary>Renders the image to stream.</summary>
		/// <param name="imageBytes">The image bytes.</param>
		/// <returns></returns>
		public static System.Drawing.Image? RenderImageToStream(byte[] imageBytes) {
			MemoryStream stream = new();

			try {
				stream.Write(imageBytes, 0, imageBytes.Length);
				return stream != null ? System.Drawing.Image.FromStream(stream) : null;
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>Renders the image to stream from file.</summary>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
		public static System.Drawing.Image RenderImageToStreamFromFile(string filename) {
			FileStream fs = new(filename, FileMode.Open, FileAccess.Read);
			// Create a byte array of file stream length
			byte[] ImageData = new byte[fs.Length];
			//Read block of bytes from stream into the byte array
			fs.Read(ImageData, 0, Convert.ToInt32(fs.Length));
			//Close the File Stream
			fs.Close();
			return RenderImageToStream(ImageData);
		}

		/// <summary>Renders the image to byte array from file.</summary>
		/// <param name="filename">The filename.</param>
		/// <returns></returns>
		public static byte[] RenderImageToByteArrayFromFile(string filename) {
			FileStream fs = new(filename, FileMode.Open, FileAccess.Read);
			// Create a byte array of file stream length
			byte[] ImageData = new byte[fs.Length];
			//Read block of bytes from stream into the byte array
			fs.Read(ImageData, 0, Convert.ToInt32(fs.Length));
			//Close the File Stream
			fs.Close();
			return ImageData; //return the byte data
		}

		/// <summary>Renders the image to byte array from URL.</summary>
		/// <param name="url">The URL.</param>
		/// <returns></returns>
		public static byte[] RenderImageToByteArrayFromUrl(string url) {
			HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
			myRequest.Timeout = 1200000;
			// Get the web response
			HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();

			try {
				// Create a 4K buffer to chunk the file
				byte[] myBuffer = new byte[32768];

				// Make sure the response is valid
				if (HttpStatusCode.OK == myResponse.StatusCode) {
					// Open the response stream
					Stream httpStream = myResponse.GetResponseStream();
					using MemoryStream myStream = new();
					while (true) {
						Int32 bytesRead = httpStream.Read(myBuffer, 0, myBuffer.Length);
						if (bytesRead <= 0)
							return myStream.ToArray();
						myStream.Write(myBuffer, 0, bytesRead);
					}
				} else {
					return new MemoryStream().ToArray();
				}
			} catch (Exception err) {
				throw new Exception("Error saving file from URL:" + err.Message, err);
			} finally {
				myResponse?.Close();
			}
		}

		#endregion Methods
	}
}