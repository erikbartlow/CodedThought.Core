namespace CodedThought.Core {

	/// <summary>
	/// Provides a generic list suitable for binding to DataGrids when column sorting capability is desired. Inherits from IBindingList<>. SortingList supports 2 types of sorting:
	/// 1) A traditional, on-demmand sort (provided by quicksort)
	/// 2) A proactive, binary sort on all properties as the list is constructed (NOT recommended for large lists) A property must implement IComparable if one wants to sort on it. To enable or
	/// prevent a Property from being indexed by FastSort, use <see cref="IndexAttribute" /> on the property accessor. SortingList also Supports the <see
	/// cref="INotifyPropertyChanged" /> Interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class SortingList<T> : BindingList<T> {
		private PropertyInfo? pi;
		private bool bSupportsFastSorting = false;
		private bool bSupportsPropertyChanged = false;
		private string _fastSortProperty = null;
		private ListSortDirection? _fastSortDirection = null;
		private Dictionary<string, List<SortIndex>> _dictIndexes = null;

		public SortingList() : base() {
			_dictIndexes = new Dictionary<string, List<SortIndex>>();

			if (typeof(T).GetInterface("INotifyPropertyChanged") != null) {
				bSupportsPropertyChanged = true;
			}
		}

		/// <summary>Initializes a new instance of the <see cref="SortingList&lt;T&gt;" /> class.</summary>
		/// <param name="enableFastSort">if set to <c>true</c> enables the FastSort indexing.</param>
		public SortingList(bool enableFastSort)
			: this() {
			if (enableFastSort) {
				bSupportsFastSorting = true;

				PropertyInfo[] properties = typeof(T).GetProperties();
				foreach (PropertyInfo p in properties) {
					if (p.PropertyType.GetInterface("IComparable") != null) {
						Attribute attr = Attribute.GetCustomAttribute(p, typeof(IndexAttribute), false);
						if (attr != null) {
							IndexAttribute ia = (IndexAttribute)attr;
							if (ia.Index)
								_dictIndexes.Add(p.Name, new List<SortIndex>());
						}
					}
				}
			}
		}

		public new T this[int index] {
			get {
				if (bSupportsFastSorting && !string.IsNullOrEmpty(_fastSortProperty) && _fastSortDirection != null) {
					if (_dictIndexes.ContainsKey(_fastSortProperty)) {
						if (_fastSortDirection == ListSortDirection.Descending) {
							return base[_dictIndexes[_fastSortProperty][index].Index];
						} else {
							List<SortIndex> list = _dictIndexes[_fastSortProperty];
							return base[list[list.Count - 1 - index].Index];
						}
					}
				}

				return base[index];
			}
			set {
				if (bSupportsPropertyChanged) {
					INotifyPropertyChanged iNPC = (INotifyPropertyChanged)value;
					iNPC.PropertyChanged += new PropertyChangedEventHandler(iNPC_PropertyChanged);
				}

				if (bSupportsFastSorting) {
					RemoveAt(index);
					this[index] = value;
					IndexItem(value);
				} else {
					base[index] = value;
				}
			}
		}

		#region Core

		protected override bool SupportsSortingCore {
			get {
				return true;
			}
		}

		protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
			SortCore(prop.Name, direction);
		}

		protected void SortCore(string name, ListSortDirection direction) {
			pi = typeof(T).GetProperty(name);
			if (pi.PropertyType.GetInterface("IComparable") == null)
				return;
			QuickSort(0, this.Count - 1, direction);
		}

		/// <summary>Sorts the list using QuickSort on the specified property name.</summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="direction">   The sort direction.</param>
		public void Sort(string propertyName, ListSortDirection direction) {
			try {
				SortCore(propertyName, direction);
			} catch { }
		}

		#endregion Core

		#region FastSort

		#region SortIndex

		private class SortIndex : IComparable {
			private int _index = -1;
			private Object _item = null;

			public SortIndex() {
			}

			public SortIndex(int index, Object item) {
				_index = index;
				_item = item;
			}

			public int Index {
				get { return _index; }
				set { _index = value; }
			}

			public Object Item {
				get { return _item; }
				set { _item = value; }
			}

			public int CompareTo(object obj) {
				SortIndex si = (SortIndex)obj;
				return ((IComparable)this._item).CompareTo(si.Item);
			}

			public override string ToString() {
				return _item.ToString();
			}
		}

		#endregion SortIndex

		/// <summary>Gets a value indicating whether this list supports fast sorting.</summary>
		/// <value><c>true</c> if [supports fast sorting]; otherwise, <c>false</c>.</value>
		public bool SupportsFastSorting {
			get { return bSupportsFastSorting; }
		}

		/// <summary>Gets a value indicating whether &lt;T&gt; Supports the <see cref="INotifyPropertyChanged" /> Interface.</summary>
		/// <value><c>true</c> if [supports notify property changed]; otherwise, <c>false</c>.</value>
		public bool SupportsNotifyPropertyChanged {
			get { return bSupportsPropertyChanged; }
		}

		/// <summary>Gets or sets the T.Property on which to FastSort. FastSort is only a availible if the SupportsFastSearching Property is set to true;</summary>
		/// <value>The fast sort property.</value>
		public string FastSortProperty {
			get { return _fastSortProperty; }
			set { _fastSortProperty = value; }
		}

		/// <summary>Gets or sets the direction on which to FastSort. FastSort is only a availible if the SupportsFastSearching Property is set to true.</summary>
		/// <value>The fast sort direction.</value>
		public ListSortDirection? FastSortDirection {
			get { return _fastSortDirection; }
			set { _fastSortDirection = value; }
		}

		public new void Add(T item) {
			base.Add(item);
			if (bSupportsFastSorting) {
				IndexItem(item);
			}
			if (bSupportsPropertyChanged) {
				INotifyPropertyChanged iNPC = (INotifyPropertyChanged)item;
				iNPC.PropertyChanged += new PropertyChangedEventHandler(iNPC_PropertyChanged);
			}
		}

		public new void Insert(int index, T item) {
			base.Insert(index, item);
			if (bSupportsFastSorting) {
				IndexItem(item);
			}
			if (bSupportsPropertyChanged) {
				INotifyPropertyChanged iNPC = (INotifyPropertyChanged)item;
				iNPC.PropertyChanged += new PropertyChangedEventHandler(iNPC_PropertyChanged);
			}
		}

		public new bool Remove(T item) {
			bool bRet = base.Remove(item);
			if (bSupportsFastSorting && bRet) {
				UnindexItem(item);
			}
			if (bSupportsPropertyChanged && bRet) {
				INotifyPropertyChanged iNPC = (INotifyPropertyChanged)item;
				iNPC.PropertyChanged -= new PropertyChangedEventHandler(iNPC_PropertyChanged);
			}
			return bRet;
		}

		public new void RemoveAt(int index) {
			if (bSupportsFastSorting) {
				UnindexItem(this[index]);
			}
			if (bSupportsPropertyChanged) {
				INotifyPropertyChanged iNPC = (INotifyPropertyChanged)this[index];
				iNPC.PropertyChanged -= new PropertyChangedEventHandler(iNPC_PropertyChanged);
			}
			base.RemoveAt(index);
		}

		public new void Clear() {
			if (bSupportsPropertyChanged) {
				foreach (T item in this) {
					INotifyPropertyChanged iNPC = (INotifyPropertyChanged)item;
					iNPC.PropertyChanged -= new PropertyChangedEventHandler(iNPC_PropertyChanged);
				}
			}
			base.Clear();
			foreach (string key in _dictIndexes.Keys) {
				_dictIndexes[key] = new List<SortIndex>();
			}
		}

		/// <summary>
		/// If FastSort is enabled, this searches on the indexed property and returns the first match found. If a match is not found, or FastSort is disabled, or the Property is not indexed, returns -1
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="target">      The target.</param>
		/// <returns></returns>
		public int SearchProperty(string propertyName, object target) {
			if (bSupportsFastSorting && _dictIndexes.ContainsKey(propertyName)) {
				List<SortIndex> list = _dictIndexes[propertyName];
				int index = BinarySearchObjectIndex(list, target, 0, (list.Count - 1) / 2, (list.Count - 1 < 0 ? 0 : list.Count - 1));

				if (index > -1) {
					//return index;
					if (_fastSortDirection == ListSortDirection.Descending) {
						return _dictIndexes[propertyName][index].Index;
					} else {
						List<SortIndex> templist = _dictIndexes[propertyName];
						return templist[templist.Count - 1 - index].Index;
					}
				}
			}
			return -1;
		}

		private void IndexItem(T item) {
			PropertyInfo[] properties = typeof(T).GetProperties();
			int itemIndex = this.IndexOf(item);
			foreach (PropertyInfo p in properties) {
				if (_dictIndexes.ContainsKey(p.Name)) {
					IndexProperty(p.Name, p.GetValue(item, null), itemIndex);
				}
			}
		}

		private void IndexProperty(string propertyName, object value, int objIndex) {
			if (_dictIndexes.ContainsKey(propertyName)) {
				List<SortIndex> listIndex = _dictIndexes[propertyName];
				SortIndex si = new(objIndex, value);

				if (listIndex.Count == 0) //doing this check here rather than in the BinaryInsert() is more efficient
				{
					listIndex.Add(si);
				} else {
					BinaryInsert(listIndex, si, 0, (listIndex.Count - 1) / 2, listIndex.Count - 1);
				}
			}
		}

		private void UnindexItem(T item) {
			PropertyInfo[] properties = typeof(T).GetProperties();
			int itemIndex = this.IndexOf(item);
			foreach (PropertyInfo p in properties) {
				if (_dictIndexes.ContainsKey(p.Name)) {
					object value = typeof(T).GetProperty(p.Name).GetValue(item, null);
					List<SortIndex> listIndex = _dictIndexes[p.Name];
					int index = BinarySearchObjectIndex(listIndex, item, 0, listIndex.Count / 2, (listIndex.Count - 1 < 0 ? 0 : listIndex.Count - 1));
					listIndex.RemoveAt(index);
				}
			}
			base.Remove(item);
		}

		private void iNPC_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (_dictIndexes.ContainsKey(e.PropertyName)) {
				object value = typeof(T).GetProperty(e.PropertyName).GetValue(sender, null);
				List<SortIndex> listIndex = _dictIndexes[e.PropertyName];
				int index = BinarySearchObjectIndex(listIndex, sender, 0, listIndex.Count / 2, (listIndex.Count - 1 < 0 ? 0 : listIndex.Count - 1));
				SortIndex si = listIndex[index];
				listIndex.RemoveAt(index);
				IndexProperty(e.PropertyName, value, si.Index);
			}
		}

		private void BinaryInsert(List<SortIndex> list, SortIndex si, int startIndex, int currentindex, int endIndex) {
			int comparison = si.CompareTo(list[currentindex]);

			//check bounds
			if (currentindex == startIndex || currentindex == endIndex) {
				if (comparison <= 0) //item goes before the current position
				{
					list.Insert(currentindex, si);
				} else //item goes after the current position
				  {
					if (currentindex >= list.Count - 1) //if current position is end of list, just add it to the end of the list
					{
						list.Add(si);
					} else //otherwise add it after the current item
					  {
						list.Insert(currentindex + 1, si);
					}
				}
				return;
			}

			//newIndex should go to the current index
			if (comparison == 0) {
				list.Insert(currentindex, si);
				return;
			}

			//newIndex comes before the current object
			if (comparison < 0) {
				int next = currentindex - (currentindex - startIndex) / 2;
				if (next == currentindex) {
					next--;
				}
				BinaryInsert(list, si, startIndex, next, currentindex);
				return;
			}

			//newIndex comes after the current object
			if (comparison > 0) {
				int next = (endIndex - currentindex) / 2 + currentindex;
				if (next == currentindex) {
					next++;
				}
				BinaryInsert(list, si, currentindex, next, endIndex);
				return;
			}
		}

		private int BinarySearchObjectIndex(List<SortIndex> list, Object item, int startIndex, int currentindex, int endIndex) {
			int comparison = ((IComparable)item).CompareTo(list[currentindex].Item);

			if (comparison == 0) {
				return list[currentindex].Index;
				//return currentindex;
			}

			//check bounds
			if (currentindex <= startIndex || currentindex >= endIndex) {
				return -1;
			}

			//newIndex comes before the current object
			if (comparison < 0) {
				int next = currentindex - (currentindex - startIndex) / 2;
				if (next == currentindex) {
					next--;
				}
				return BinarySearchObjectIndex(list, item, startIndex, next, currentindex);
			}

			//newIndex comes after the current object
			if (comparison > 0) {
				int next = (endIndex - currentindex) / 2 + currentindex;
				if (next == currentindex) {
					next++;
				}
				return BinarySearchObjectIndex(list, item, currentindex, next, endIndex);
			}

			return -1;
		}

		#endregion FastSort

		#region QuickSort

		private IComparable Target(int index) {
			return (IComparable)pi.GetValue(this[index], null);
		}

		private void Swap(int index1, int index2) {
			T temp = this[index1];
			base[index1] = base[index2];
			base[index2] = temp;
		}

		private int Partition(int left, int right, int pivotIndex, ListSortDirection dir) {
			IComparable pivotCompare = Target(pivotIndex);
			Swap(pivotIndex, right);
			int storeIndex = left;
			if (dir == ListSortDirection.Ascending) {
				for (int i = left; i <= right - 1; i++) {
					if (Target(i).CompareTo(pivotCompare) <= 0) {
						Swap(storeIndex, i);
						storeIndex++;
					}
				}
			} else {
				for (int i = left; i <= right - 1; i++) {
					if (Target(i).CompareTo(pivotCompare) >= 0) {
						Swap(storeIndex, i);
						storeIndex++;
					}
				}
			}
			Swap(right, storeIndex);
			return storeIndex;
		}

		private void QuickSort(int left, int right, ListSortDirection dir) {
			if (this.Count <= 1)
				return;
			if (right > left) {
				Random r = new();
				int pivotIndex = r.Next(left, right);
				int pivotNewIndex = Partition(left, right, pivotIndex, dir);
				QuickSort(left, pivotNewIndex - 1, dir);
				QuickSort(pivotNewIndex + 1, right, dir);
			}
		}

		#endregion QuickSort
	}
}