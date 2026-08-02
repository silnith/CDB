using System.Data;
using System.Data.Common;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="Tile"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="Tile"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="Tile.LatitudeValue"/></term><description><see cref="SQLCDB.LatitudeParamName"/></description></item>
///   <item><term><see cref="Tile.LongitudeValue"/></term><description><see cref="SQLCDB.LongitudeParamName"/></description></item>
///   <item><term><see cref="Tile.DatasetValue"/></term><description><see cref="SQLCDB.DatasetParamName"/></description></item>
///   <item><term><see cref="Tile.ComponentSelector1"/></term><description><see cref="SQLCDB.ComponentSelector1ParamName"/></description></item>
///   <item><term><see cref="Tile.ComponentSelector2"/></term><description><see cref="SQLCDB.ComponentSelector2ParamName"/></description></item>
///   <item><term><see cref="Tile.Level"/></term><description><see cref="SQLCDB.LevelOfDetailParamName"/></description></item>
///   <item><term><see cref="Tile.Up"/></term><description><see cref="SQLCDB.UpParamName"/></description></item>
///   <item><term><see cref="Tile.Right"/></term><description><see cref="SQLCDB.RightParamName"/></description></item>
///   <item><term><see cref="Tile.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class TileTableAccessor : TableAccessor<Tile>
{
    public TileTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromTileStatement, sqlCDB.InsertIntoTileStatement)
    {
    }

    /// <inheritdoc/>
    internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, sqlCDB.LatitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.LongitudeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.UpParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.RightParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, Tile obj)
    {
        dbCommand.Parameters[sqlCDB.LatitudeParamName].Value = obj.LatitudeValue.Value;
        dbCommand.Parameters[sqlCDB.LongitudeParamName].Value = obj.LongitudeValue.Value;
        dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.DatasetValue.Value;
        dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
        dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
        dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.Level.Value;
        dbCommand.Parameters[sqlCDB.UpParamName].Value = obj.Up;
        dbCommand.Parameters[sqlCDB.RightParamName].Value = obj.Right;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
