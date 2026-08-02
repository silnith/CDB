using System.Data;
using System.Data.Common;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="TileArchivedTexture"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="TileArchivedTexture"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="TileArchivedTexture.LatitudeValue"/></term><description><see cref="SQLCDB.LatitudeParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.LongitudeValue"/></term><description><see cref="SQLCDB.LongitudeParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.DatasetValue"/></term><description><see cref="SQLCDB.DatasetParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.ComponentSelector1"/></term><description><see cref="SQLCDB.ComponentSelector1ParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.ComponentSelector2"/></term><description><see cref="SQLCDB.ComponentSelector2ParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.Level"/></term><description><see cref="SQLCDB.LevelOfDetailParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.Up"/></term><description><see cref="SQLCDB.UpParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.Right"/></term><description><see cref="SQLCDB.RightParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.Name"/></term><description><see cref="SQLCDB.TextureNameParamName"/></description></item>
///   <item><term><see cref="TileArchivedTexture.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class TileArchivedTextureTableAccessor : TableAccessor<TileArchivedTexture>
{
    public TileArchivedTextureTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromTileArchivedTextureStatement, sqlCDB.InsertIntoTileArchivedTextureStatement)
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
        CreateAndAttachParameter(dbCommand, sqlCDB.TextureNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, TileArchivedTexture obj)
    {
        dbCommand.Parameters[sqlCDB.LatitudeParamName].Value = obj.LatitudeValue.Value;
        dbCommand.Parameters[sqlCDB.LongitudeParamName].Value = obj.LongitudeValue.Value;
        dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.DatasetValue.Value;
        dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
        dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
        dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.Level.Value;
        dbCommand.Parameters[sqlCDB.UpParamName].Value = obj.Up;
        dbCommand.Parameters[sqlCDB.RightParamName].Value = obj.Right;
        dbCommand.Parameters[sqlCDB.TextureNameParamName].Value = obj.Name;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
