using System.Collections;

namespace CodedThought.Core.Data {

	/// <summary>ParameterCollection provides a type safe collection of IDataParameter objects. It is not specif to any database.</summary>
	public class ParameterCollection : CollectionBase {
		private DatabaseObject _dbo;

		#region Constructors

		/// <summary>Initializes a new instance of the <see cref="ParameterCollection" /> class.</summary>
		public ParameterCollection() : base() {
		}

		/// <summary>Initializes a new instance of the <see cref="ParameterCollection" /> class.</summary>
		/// <param name="databaseToUse">The database to use.</param>
		public ParameterCollection(IDatabaseObject databaseToUse) : base() {
			_subList = new ParameterCollection();
			_dbo = (DatabaseObject)databaseToUse;
		}

		#endregion Constructors

		#region Add Methods

		/// <summary>add a parameter to the collection</summary>
		/// <param name="parameter">parameter to be added</param>
		public void Add(IDataParameter parameter) {
			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
		}

		/// <summary>Adds the specified parameter.</summary>
		/// <param name="parameter">The parameter.</param>
		public void Add(Parameter parameter) {
			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
		}

		/// <summary>Adds an output parameter to the collection based on the database supported.</summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="returnType">   Type of the return.</param>
		/// <returns></returns>
		public IDataParameter AddOutputParameter(string parameterName, DbTypeSupported returnType) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateOutputParameter(parameterName, returnType);
			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds a return parameter to the collection based on the database supported.</summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="returnType">   Type of the return.</param>
		/// <returns></returns>
		public IDataParameter AddReturnParameter(string parameterName, DbTypeSupported returnType) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateReturnParameter(parameterName, returnType);
			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds a string parameter to the collection based on the database supported</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns>A reference to the parameter it creates and adds</returns>
		public IDataParameter AddStringParameter(string srcTableColumnName, string parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateStringParameter(srcTableColumnName, parameterValue);

			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}

			return parameter;
		}

		/// <summary>Adds a int32 parameter to the collection based on the database supported</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns>A reference to the parameter it creates and adds</returns>
		public IDataParameter AddInt32Parameter(string srcTableColumnName, int parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateInt32Parameter(srcTableColumnName, parameterValue);

			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds the GUID parameter to the collection based on the database supported.</summary>
		/// <param name="srcTableColumnName">Name of the SRC table column.</param>
		/// <param name="parameterValue">    The parameter value.</param>
		/// <returns></returns>
		public IDataParameter AddGuidParameter(string srcTableColumnName, Guid parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateGuidParameter(srcTableColumnName, parameterValue);
			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds a Double parameter to the collection based on the database supported</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns>A reference to the parameter it creates and adds</returns>
		public IDataParameter AddDoubleParameter(string srcTableColumnName, double parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateDoubleParameter(srcTableColumnName, parameterValue);

			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds a DateTime parameter to the collection based on the database supported</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <returns>A reference to the parameter it creates and adds</returns>
		public IDataParameter AddDateTimeParameter(string srcTableColumnName, DateTime parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateDateTimeParameter(srcTableColumnName, parameterValue);

			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds a Char parameter to the collection based on the database supported</summary>
		/// <param name="srcTableColumnName"></param>
		/// <param name="parameterValue">    </param>
		/// <param name="size">              </param>
		/// <returns>A reference to the parameter it creates and adds</returns>
		public IDataParameter AddCharParameter(string srcTableColumnName, string parameterValue, int size) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateCharParameter(srcTableColumnName, parameterValue, size);

			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds the XML parameter based on the database supported.</summary>
		/// <param name="srcTableColumnName">Name of the SRC table column.</param>
		/// <param name="parameterValue">    The parameter value.</param>
		/// <returns></returns>
		public IDataParameter AddXMLParameter(string srcTableColumnName, string parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateXMLParameter(srcTableColumnName, parameterValue);

			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds the boolean parameter.</summary>
		/// <param name="srcTableColumnName">Name of the source table column.</param>
		/// <param name="parameterValue">    if set to <c>true</c> [parameter value].</param>
		/// <returns></returns>
		public IDataParameter AddBooleanParameter(string srcTableColumnName, bool parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateBooleanParameter(srcTableColumnName, parameterValue);
			if (!List.Contains(parameter)) {
				List.Add(parameter);
			}
			return parameter;
		}

		/// <summary>Adds the sub group parameter list.</summary>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public void AddSubGroupParameterList(ParameterCollection list) {
			_subList = list;
		}

		/// <summary>Adds a parameter for the REST Api call.</summary>
		/// <param name="actionName">    </param>
		/// <param name="parameterValue"></param>
		/// <returns><see cref="IDataParameter" /></returns>
		/// <remarks>Since a REST Api does not communicate with a database directly the Api Parameter is primarily holding the action and parameter of a Url request. For example, https://localhost/[controller]/[action]?parameter=value</remarks>
		public void AddApiParameter(string parameterName, string parameterValue) {
			IDataParameter parameter = this.DerivedDatabaseObject.CreateApiParameter(parameterName, parameterValue);
			if (!List.Contains(parameter)) { List.Add(parameter); }
		}

		#endregion Add Methods

		#region Properties

		private WhereType _subListWhereType = WhereType.AND;
		private ParameterCollection _subList;

		/// <summary>Gets or sets the type of the sub parameter group where clause.</summary>
		/// <value>The type of the sub parameter group where.</value>
		public WhereType SubParameterGroupWhereType {
			get { return _subListWhereType; }
			set { _subListWhereType = value; }
		}

		/// <summary>Gets the derived database object.</summary>
		/// <value>The derived database object.</value>
		internal DatabaseObject DerivedDatabaseObject {
			get { return this._dbo; }
			set { this._dbo = value; }
		}

		#endregion Properties

		/// <summary>remove a parameter from the collection</summary>
		/// <param name="index">zero bound index of the parameter in the collection to remove</param>
		public void Remove(int index) {
			List.RemoveAt(index);
		}

		/// <summary>Add the ParameterCollection to this collection</summary>
		/// <param name="parameters">the parameter collection to add</param>
		public void Add(ParameterCollection parameters) {
			foreach (IDataParameter parameter in parameters) {
				List.Add(parameter);
			}
		}

		/// <summary>Add the IDataParameterCollection to this collection</summary>
		/// <param name="parameters">the parameter collection to add</param>
		public void Add(IDataParameterCollection parameters) {
			foreach (IDataParameter parameter in parameters) {
				List.Add(parameter);
			}
		}

		/// <summary>return a single parameter object, based on the index within the collection.</summary>
		public IDataParameter this[int index] {
			get { return (IDataParameter)List[index]; }
		}

		/// <summary>return a single parameter object, based on the name within the collection.</summary>
		public IDataParameter this[string name] {
			get {
				IDataParameter thisParam = null;
				foreach (IDataParameter param in List) {
					if (param.ParameterName == name) {
						thisParam = param;
						continue;
					}
				}
				return thisParam;
			}
		}

		public string GenerateWhereClauseFromParams(string ParameterConnector) {
			StringBuilder sql = new();
			int count = 0;

			foreach (IDataParameter param in this) {
				Parameter thisParam = ConvertToParameter(param);
				if (param is Parameter) {
					if (param is BetweenParameter) {
						sql.Append(((BetweenParameter)param).ToParameterString(ParameterConnector, (count == 0)));
					} else if (param is LikeParameter) {
						((LikeParameter)param).WildcardCharacter = DerivedDatabaseObject.WildCardCharacter;
						sql.Append(((LikeParameter)param).ToParameterString(ParameterConnector, (count == 0)));
					} else if (param is InParameter) {
						sql.Append(((InParameter)param).ToParameterString(ParameterConnector, (count == 0)));
					} else if (param is ComparisonParameter) {
						sql.Append(((ComparisonParameter)param).ToParameterString(ParameterConnector, (count == 0)));
					} else {
						sql.Append(((Parameter)param).ToParameterString(ParameterConnector, (count == 0)));
					}
				} else {
					sql.Append(thisParam.ToParameterString(ParameterConnector, (count == 0)));
				}
				count++;
			}

			// Process any sub groupings
			if (_subList?.Count > 0) {
				sql.Append($" {_subList.SubParameterGroupWhereType.ToString()} ({_subList.GenerateWhereClauseFromParams(ParameterConnector)})");
			}
			return sql.ToString();
		}

		public void AddParametersToCommand(IDbCommand cmd, string strParameterConnector) {
			cmd.Parameters.Clear();
			foreach (IDataParameter param in this) {
				param.ParameterName = strParameterConnector + param.ParameterName;
				if (param is Parameter) {
					if (param is BetweenParameter) {
						cmd.Parameters.Add(((BetweenParameter)param).BaseParameter);
						cmd.Parameters.Add(((BetweenParameter)param).Parameter2);
					} else if (param is InParameter) {
						foreach (IDataParameter pn in ((InParameter)param).InParameters) {
							cmd.Parameters.Add(pn);
						}
					} else {
						cmd.Parameters.Add(((Parameter)param).BaseParameter);
					}
				} else {
					cmd.Parameters.Add(param);
				}
			}
		}

		public void ExtractAndReloadParameterCollection(IDbCommand cmd, string strParameterConnector) {
			foreach (IDataParameter cmdParam in cmd.Parameters) {
				if (cmdParam.Direction != ParameterDirection.Input) {
					cmdParam.ParameterName = cmdParam.ParameterName.Replace(strParameterConnector, string.Empty);
					foreach (IDataParameter collectionParam in this) {
						if (collectionParam.ParameterName == cmdParam.ParameterName) {
							collectionParam.Value = collectionParam.Value;
						}
					}
				}
			}
			cmd.Parameters.Clear();
		}

		public Parameter ConvertToParameter(IDataParameter parameter) {
			Parameter param = new Parameter(parameter);
			return param;
		}
	}
}