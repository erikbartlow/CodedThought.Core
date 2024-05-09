namespace CodedThought.Core.Data.Interfaces
{
    public interface ITableColumn
    {
        string CorrespondingPropertyName { get; set; }
        bool IsPrimary {  get; set; }
        bool IsDescending { get; set; }
        bool IsIdentity { get; set; }
        bool IsInsertable { get; set; }
        bool IsNullable { get; set; }
        bool IsSortColumn { get; set; }
        bool IsUpdateable { get; set; }
        int MaxLength { get; set; }
        string Name { get; set; }
        int OrdinalPosition { get; set; }
        int Size { get; set; }
        Type SystemType { get; set; }
        DbTypeSupported Type { get; set; }
        DbType DbType { get; set; }
    }
}