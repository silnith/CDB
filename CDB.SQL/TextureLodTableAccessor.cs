using System.Data;
using System.Data.Common;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="TextureLod"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="TextureLod"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="TextureLod.Dataset"/></term><description><see cref="SQLCDB.DatasetParamName"/></description></item>
///   <item><term><see cref="TextureLod.ComponentSelector1"/></term><description><see cref="SQLCDB.ComponentSelector1ParamName"/></description></item>
///   <item><term><see cref="TextureLod.ComponentSelector2"/></term><description><see cref="SQLCDB.ComponentSelector2ParamName"/></description></item>
///   <item><term><see cref="TextureLod.LevelOfDetail"/></term><description><see cref="SQLCDB.LevelOfDetailParamName"/></description></item>
///   <item><term><see cref="TextureLod.Name"/></term><description><see cref="SQLCDB.TextureNameParamName"/></description></item>
///   <item><term><see cref="TextureLod.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class TextureLodTableAccessor : TableAccessor<TextureLod>
{
    public TextureLodTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromTextureLodStatement, sqlCDB.InsertIntoTextureLodStatement)
    {
    }

    /// <inheritdoc/>
    internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.TextureNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, TextureLod obj)
    {
        dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
        dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
        dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
        dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.LevelOfDetail.Value;
        dbCommand.Parameters[sqlCDB.TextureNameParamName].Value = obj.Name;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
