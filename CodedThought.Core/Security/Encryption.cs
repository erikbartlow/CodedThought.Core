using System.Security.Cryptography;

namespace CodedThought.Core.Security {

	/// <summary>Summary description for Encryption.</summary>
	public class Encryption {

		#region Methods

		/// <summary>Decrypts the file.</summary>
		/// <param name="file">           The file.</param>
		/// <param name="FileDestination">The file destination.</param>
		/// <param name="Password">       The password.</param>
		public static void DecryptFile(FileStream file, string FileDestination, string Password) {
			FileStream fsIn = file;
			FileStream fsOut = new(FileDestination, FileMode.Create, FileAccess.Write);

			byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

			PasswordDeriveBytes SecretKey = new(Password, Salt);
			using Aes aesCipher = Aes.Create();
			ICryptoTransform Decryptor = aesCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

			using CryptoStream cryptoStream = new(fsIn, Decryptor, CryptoStreamMode.Read);
			int ByteData;
			while ((ByteData = cryptoStream.ReadByte()) != -1) {
				fsOut.WriteByte((byte)ByteData);
			}
			cryptoStream.Close();
			fsIn.Close();
			fsOut.Close();
		}

		/// <summary>Decrypts the specified p value.</summary>
		/// <param name="pValue">The p value.</param>
		/// <returns>System.String.</returns>
		public static string DecryptMD5(string pValue) {
			if (pValue == string.Empty) {
				return "";
			}
			byte[] array = Convert.FromBase64String(pValue);
			MD5 md5 = MD5.Create();
			byte[] key = md5.ComputeHash(Encoding.UTF8.GetBytes("9876543210a7ABbC3cDd7EeFfGg4H8hIiJj1KkLlM8m—91ÒOoPp3QqRrSs4TtUuVvWwXxY5yZ9z0123456789"));
			md5.Clear();

			TripleDES tripleDES = TripleDES.Create();
			tripleDES.Key = key;
			tripleDES.Mode = CipherMode.ECB;
			tripleDES.Padding = PaddingMode.PKCS7;

			ICryptoTransform cryptoTransform = tripleDES.CreateDecryptor();
			byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
			tripleDES.Clear();
			return Encoding.UTF8.GetString(bytes);
		}

		/// <summary>Decrypts the string.</summary>
		/// <param name="InputText">The input text.</param>
		/// <param name="Password"> The password.</param>
		/// <returns></returns>
		public static string DecryptString(string InputText, string Password) {
			byte[] EncryptedData = Convert.FromBase64String(InputText);
			byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

			// Declare the string used to hold
			// the decrypted text.
			string plaintext = null;

			// The (Secret Key) will be generated from the specified password and salt.
			PasswordDeriveBytes SecretKey = new(Password, Salt);

			// Create an Aes object
			// with the specified key and IV.
			using (Aes aesAlg = Aes.Create()) {
				// Create a decryptor to perform the stream transform.
				ICryptoTransform decryptor = aesAlg.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

				// Create the streams used for decryption.
				using MemoryStream msDecrypt = new(EncryptedData);
				using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
				using StreamReader srDecrypt = new(csDecrypt);

				// Read the decrypted bytes from the decrypting stream
				// and place them in a string.
				plaintext = srDecrypt.ReadToEnd();
			}
			// Return decrypted string.
			return plaintext;
		}

		/// <summary>Encrypts the file.</summary>
		/// <param name="FileLocation">   The file location.</param>
		/// <param name="FileDestination">The file destination.</param>
		/// <param name="Password">       The password.</param>
		public static void EncryptFile(string FileLocation, string FileDestination, string Password) {
			// First we are going to open the file streams
			FileStream fsIn = new(FileLocation, FileMode.Open, FileAccess.Read);
			FileStream fsOut = new(FileDestination, FileMode.Create, FileAccess.Write);

			byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

			PasswordDeriveBytes SecretKey = new(Password, Salt);
			using Aes aesCipher = Aes.Create();
			ICryptoTransform Encryptor = aesCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
			using CryptoStream csEncrypt = new(fsOut, Encryptor, CryptoStreamMode.Write);
			int ByteData;
			while ((ByteData = fsIn.ReadByte()) != -1) {
				csEncrypt.WriteByte((byte)ByteData);
			}
			csEncrypt.Close();
			fsIn.Close();
			fsOut.Close();
		}

		public static void EncryptFileFromStream(FileStream file, string FileDestination, string Password) {
			// First we are going to open the file streams
			FileStream fsIn = file;
			FileStream fsOut = new(FileDestination, FileMode.Create, FileAccess.Write);

			byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

			PasswordDeriveBytes SecretKey = new(Password, Salt);
			using Aes aesCipher = Aes.Create();
			ICryptoTransform Encryptor = aesCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
			using CryptoStream csEncrypt = new(fsOut, Encryptor, CryptoStreamMode.Write);
			int ByteData;
			while ((ByteData = fsIn.ReadByte()) != -1) {
				csEncrypt.WriteByte((byte)ByteData);
			}
			csEncrypt.Close();
			fsIn.Close();
			fsOut.Close();
		}

		/// <summary>Encrypts the specified p value.</summary>
		/// <param name="pValue">The p value.</param>
		/// <returns>System.String.</returns>
		public static string EncryptMD5(string pValue) {
			byte[] bytes = Encoding.UTF8.GetBytes(pValue);

			MD5 md5 = MD5.Create();
			byte[] key = md5.ComputeHash(Encoding.UTF8.GetBytes("9876543210a7ABbC3cDd7EeFfGg4H8hIiJj1KkLlM8m—91ÒOoPp3QqRrSs4TtUuVvWwXxY5yZ9z0123456789"));
			md5.Clear();

			TripleDES tripleDES = TripleDES.Create();
			tripleDES.Key = key;
			tripleDES.Mode = CipherMode.ECB;
			tripleDES.Padding = PaddingMode.PKCS7;

			ICryptoTransform cryptoTransform = tripleDES.CreateDecryptor();
			byte[] array = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
			tripleDES.Clear();
			return Convert.ToBase64String(array, 0, array.Length);
		}

		/// <summary>Encrypts the string.</summary>
		/// <param name="InputText">The input text.</param>
		/// <param name="Password"> The password.</param>
		/// <returns></returns>
		public static string EncryptString(string InputText, string Password) {
			// First we need to turn the input strings into a byte array.
			byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(InputText);

			// We are using salt to make it harder to guess our key using a dictionary attack.
			byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());
			// The (Secret Key) will be generated from the specified password and salt.
			PasswordDeriveBytes SecretKey = new(Password, Salt);

			using Aes aesCipher = Aes.Create();
			ICryptoTransform Encryptor = aesCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
			using MemoryStream msEncrypt = new();
			using CryptoStream csEncrypt = new(msEncrypt, Encryptor, CryptoStreamMode.Write);
			using (StreamWriter swEncrypt = new(csEncrypt)) {
				swEncrypt.Write(InputText);
				csEncrypt.FlushFinalBlock();
			}
			// Convert our encrypted data from a memoryStream into a byte array.
			byte[] CipherBytes = msEncrypt.ToArray();
			// Return encrypted string.
			return Convert.ToBase64String(CipherBytes);
		}

		/// <summary>
		/// Generates an encription safe key sized to the <see cref="int">key size</see> passed.
		/// </summary>
		/// <param name="keySize"></param>
		/// <returns></returns>
		public static string GenerateKey(int keySize) {
			byte[] randomBytes = new byte[keySize];
			using (var rng = RandomNumberGenerator.Create()) {
				rng.GetBytes(randomBytes);
				return Convert.ToBase64String(randomBytes);
			}
		}

		#endregion Methods

		public Encryption() {
			// TODO: Add constructor logic here
		}
	}
}