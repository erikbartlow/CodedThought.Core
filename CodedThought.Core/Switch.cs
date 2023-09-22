namespace CodedThought.Core {

	public static class Switch {

		public static Switch<T> On<T>(T value) {
			return new Switch<T>(value);
		}
	}

	public class Switch<T> {
		private bool hasBeenHandled;

		/// <summary>The value</summary>
		private readonly T value;

		/// <summary>Initializes a new instance of the <see cref="Switch{T}" /> class.</summary>
		/// <param name="value">The value.</param>
		public Switch(T value) {
			this.value = value;
		}

		/// <summary>Switch case alternative.</summary>
		/// <param name="compare">The compare.</param>
		/// <param name="action"> The action.</param>
		/// <returns></returns>
		public Switch<T> Case(T compare, Action action) {
			if (AreEqual(value, compare)) {
				hasBeenHandled = true;
				action();
			}
			return this;
		}

		/// <summary>The default action.</summary>
		/// <param name="action">The action.</param>
		public void Default(Action action) {
			if (!hasBeenHandled)
				action();
		}

		/// <summary>Are they equal.</summary>
		/// <param name="actualValue">The actual value.</param>
		/// <param name="compare">    The compare.</param>
		/// <returns></returns>
		private bool AreEqual(T actualValue, T compare) {
			return Equals(actualValue, compare);
		}
	}
}