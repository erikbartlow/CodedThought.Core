using System.Security.Cryptography;

namespace CodedThought.Core.Security {

	public class PasswordGenerator {

		public PasswordGenerator() {
			this.Minimum = DefaultMinimum;
			this.Maximum = DefaultMaximum;
			this.ConsecutiveCharacters = false;
			this.RepeatCharacters = true;
			this.ExcludeSymbols = false;
			this.Exclusions = string.Empty;
		}

		protected int GetCryptographicRandomNumber(int lBound, int uBound) {
			// Assumes lBound >= 0 && lBound < uBound returns an int >= lBound and < uBound
			uint urndnum;
			if (lBound == uBound - 1) {
				// test for degenerate case where only lBound can be returned
				return lBound;
			}

			uint xcludeRndBase = (uint.MaxValue - (uint.MaxValue % (uint)(uBound - lBound)));

			do {
				using RandomNumberGenerator generator = RandomNumberGenerator.Create();
				byte[] rndnum = new Byte[4];
				generator.GetBytes(rndnum);
				urndnum = BitConverter.ToUInt32(rndnum, 0);
			} while (urndnum >= xcludeRndBase);

			return (int)(urndnum % (uBound - lBound)) + lBound;
		}

		protected char GetRandomCharacter() {
			int upperBound = pwdCharArray.GetUpperBound(0);

			if (true == this.ExcludeSymbols) {
				upperBound = PasswordGenerator.UBoundDigit;
			}

			int randomCharPosition = GetCryptographicRandomNumber(pwdCharArray.GetLowerBound(0), upperBound);

			char randomChar = pwdCharArray[randomCharPosition];

			return randomChar;
		}

		public string Generate() {
			// Pick random length between minimum and maximum
			int pwdLength = GetCryptographicRandomNumber(this.Minimum, this.Maximum);

			StringBuilder pwdBuffer = new() {
				Capacity = this.Maximum
			};

			// Generate random characters
			char lastCharacter, nextCharacter;

			// Initial dummy character flag
			lastCharacter = nextCharacter = '\n';

			for (int i = 0; i < pwdLength; i++) {
				nextCharacter = GetRandomCharacter();

				if (false == this.ConsecutiveCharacters) {
					while (lastCharacter == nextCharacter) {
						nextCharacter = GetRandomCharacter();
					}
				}

				if (false == this.RepeatCharacters) {
					string temp = pwdBuffer.ToString();
					int duplicateIndex = temp.IndexOf(nextCharacter);
					while (-1 != duplicateIndex) {
						nextCharacter = GetRandomCharacter();
						duplicateIndex = temp.IndexOf(nextCharacter);
					}
				}

				if ((null != this.Exclusions)) {
					while (-1 != this.Exclusions.IndexOf(nextCharacter)) {
						nextCharacter = GetRandomCharacter();
					}
				}

				pwdBuffer.Append(nextCharacter);
				lastCharacter = nextCharacter;
			}

			return null != pwdBuffer ? pwdBuffer.ToString() : String.Empty;
		}

		public string? Exclusions { get; set; }

		public int Minimum {
			get {
				return this.minSize;
			}
			set {
				this.minSize = value;
				if (PasswordGenerator.DefaultMinimum > this.minSize) {
					this.minSize = PasswordGenerator.DefaultMinimum;
				}
			}
		}

		public int Maximum {
			get {
				return this.maxSize;
			}
			set {
				this.maxSize = value;
				if (this.minSize >= this.maxSize) {
					this.maxSize = PasswordGenerator.DefaultMaximum;
				}
			}
		}

		public Boolean ExcludeSymbols { get; set; }

		public Boolean RepeatCharacters { get; set; }

		public Boolean ConsecutiveCharacters { get; set; }

		private const int DefaultMinimum = 6;
		private const int DefaultMaximum = 10;
		private const int UBoundDigit = 61;

		private int minSize;
		private int maxSize;
		private char[] pwdCharArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-_=+[]{}\\|;:'\",<.>/?".ToCharArray();
	}
}