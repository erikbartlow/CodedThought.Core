namespace CodedThought.Core.Extensions {

	public static class FileHelpers {

		/// <summary>Gets the files by extensions.</summary>
		/// <param name="dir">         The dir.</param>
		/// <param name="searchOption">The search option.</param>
		/// <param name="extensions">  The extensions.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">An array of file extensions is required.</exception>
		public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, List<string> exclude, SearchOption searchOption = SearchOption.TopDirectoryOnly, params string[] extensions) {
			if (extensions == null)
				throw new ArgumentNullException("extensions", "An array of file extensions is required.");

			List<FileInfo> files = new();
			foreach (string ext in extensions) {
				files.AddRange(dir.EnumerateFiles("*." + ext)
					.Where(f => (f.Extension == "." + ext) && !exclude.Any(x => f.Name.Contains(x))));
			}

			return files;
		}
	}
}