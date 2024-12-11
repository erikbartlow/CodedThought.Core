using CodedThought.Core.Data.Interfaces;

namespace CodedThought.Core.Data
{

    /// <summary>Parameter for comparing a date range</summary>
    public class BetweenParameter : Parameter, IParameter
    {

        /// <summary>Initializes a new instance of the <see cref="BetweenParameter" /> class.</summary>
        public BetweenParameter() : base() { }

        public BetweenParameter(IDataParameter startParam, IDataParameter endParam)
            : base(startParam)
        {
            Parameter2 = endParam;
            ParameterName = startParam.ParameterName;
        }

        #region Properties

        public IDataParameter Parameter2 { get; set; }

        /// <summary>Gets and sets the DbType of the parameter.</summary>
        public new DbType DbType
        {
            get
            {
                return _dbParam.DbType;
            }
            set
            {
                _dbParam.DbType = value;
                Parameter2.DbType = value;
            }
        }

        /// <summary>Gets and sets the direction of the parameter</summary>
        public new ParameterDirection Direction
        {
            get
            {
                return _dbParam.Direction;
            }
            set
            {
                _dbParam.Direction = value;
                Parameter2.Direction = value;
            }
        }

        /// <summary>Gets and sets the parameter Name</summary>
        public new string ParameterName
        {
            get
            {
                return _dbParam.ParameterName;
            }
            set
            {
                _dbParam.ParameterName = value + "_1";
                Parameter2.ParameterName = value + "_2";
            }
        }

        /// <summary>Gets and sets the source column</summary>
        public new string SourceColumn
        {
            get
            {
                return _dbParam.SourceColumn;
            }
            set
            {
                _dbParam.SourceColumn = value;
                Parameter2.SourceColumn = value;
            }
        }

        /// <summary>Gets and sets the Source Version</summary>
        public new DataRowVersion SourceVersion
        {
            get
            {
                return _dbParam.SourceVersion;
            }
            set
            {
                _dbParam.SourceVersion = value;
                Parameter2.SourceVersion = value;
            }
        }

        /// <summary>Gets and sets the second date for the between clause</summary>
        public object Value2
        {
            get { return Parameter2.Value; }
            set { Parameter2.Value = value; }
        }

        #endregion Properties

        /// <summary>Converts to a parameter string.</summary>
        /// <param name="ParameterConnector">The parameter connector.</param>
        /// <returns></returns>
        public override string ToParameterString(string ParameterConnector, bool firstInGroup = true)
        {
            // Add single quotes if necessary.
            switch (_dbParam.DbType)
            {
                case DbType.String:
                    _dbParam.Value = _dbParam.Value;
                    Parameter2.Value = Parameter2.Value;
                    _dbParam.DbType = DbType.String;
                    Parameter2.DbType = DbType.String;
                    break;
            }
            return $" {(!firstInGroup ? _whereType.ToString() : "")} {_dbParam.SourceColumn} BETWEEN {ParameterConnector}{_dbParam.ParameterName} AND {ParameterConnector}{Parameter2.ParameterName}";
        }
    }
}