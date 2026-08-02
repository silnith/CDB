using System.Data;
using System.Data.Common;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="Metadata"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="Metadata"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="Metadata.Name"/></term><description><see cref="SQLCDB.MetadataNameParamName"/></description></item>
///   <item><term><see cref="Metadata.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class MetadataTableAccessor : TableAccessor<Metadata>
{
    public MetadataTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromMetadataStatement, sqlCDB.InsertIntoMetadataStatement)
    {
    }

    /// <inheritdoc/>
    internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, sqlCDB.MetadataNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, Metadata obj)
    {
        dbCommand.Parameters[sqlCDB.MetadataNameParamName].Value = obj.Name;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
