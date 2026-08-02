using System.Data;
using System.Data.Common;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="TileArchivedFeature"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="TileArchivedFeature"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="TileArchivedFeature.LatitudeValue"/></term><description><see cref="SQLCDB.LatitudeParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.LongitudeValue"/></term><description><see cref="SQLCDB.LongitudeParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.DatasetValue"/></term><description><see cref="SQLCDB.DatasetParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.ComponentSelector1"/></term><description><see cref="SQLCDB.ComponentSelector1ParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.ComponentSelector2"/></term><description><see cref="SQLCDB.ComponentSelector2ParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.Level"/></term><description><see cref="SQLCDB.LevelOfDetailParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.Up"/></term><description><see cref="SQLCDB.UpParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.Right"/></term><description><see cref="SQLCDB.RightParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.FeatureCode"/> <see cref="FeatureCode.Category"/></term><description><see cref="SQLCDB.FeatureCategoryParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.FeatureCode"/> <see cref="FeatureCode.Subcategory"/></term><description><see cref="SQLCDB.FeatureSubcategoryParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.FeatureCode"/> <see cref="FeatureCode.Type"/></term><description><see cref="SQLCDB.FeatureTypeParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.FeatureSubcode"/></term><description><see cref="SQLCDB.FeatureSubcodeParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.Name"/></term><description><see cref="SQLCDB.ModelNameParamName"/></description></item>
///   <item><term><see cref="TileArchivedFeature.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class TileArchivedFeatureTableAccessor : TableAccessor<TileArchivedFeature>
{
    public TileArchivedFeatureTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromTileArchivedFeatureStatement, sqlCDB.InsertIntoTileArchivedFeatureStatement)
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
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureCategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureTypeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcodeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ModelNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, TileArchivedFeature obj)
    {
        dbCommand.Parameters[sqlCDB.LatitudeParamName].Value = obj.LatitudeValue.Value;
        dbCommand.Parameters[sqlCDB.LongitudeParamName].Value = obj.LongitudeValue.Value;
        dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.DatasetValue.Value;
        dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
        dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
        dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.Level.Value;
        dbCommand.Parameters[sqlCDB.UpParamName].Value = obj.Up;
        dbCommand.Parameters[sqlCDB.RightParamName].Value = obj.Right;
        dbCommand.Parameters[sqlCDB.FeatureCategoryParamName].Value = obj.FeatureCode.Category;
        dbCommand.Parameters[sqlCDB.FeatureSubcategoryParamName].Value = obj.FeatureCode.Subcategory;
        dbCommand.Parameters[sqlCDB.FeatureTypeParamName].Value = obj.FeatureCode.Type;
        dbCommand.Parameters[sqlCDB.FeatureSubcodeParamName].Value = obj.FeatureSubcode;
        dbCommand.Parameters[sqlCDB.ModelNameParamName].Value = obj.Name;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
