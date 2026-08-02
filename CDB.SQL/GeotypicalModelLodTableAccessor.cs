using System.Data;
using System.Data.Common;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="GeotypicalModelLod"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="GeotypicalModelLod"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="GeotypicalModelLod.Dataset"/></term><description><see cref="SQLCDB.DatasetParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.ComponentSelector1"/></term><description><see cref="SQLCDB.ComponentSelector1ParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.ComponentSelector2"/></term><description><see cref="SQLCDB.ComponentSelector2ParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.LevelOfDetail"/></term><description><see cref="SQLCDB.LevelOfDetailParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.FeatureCode"/> <see cref="FeatureCode.Category"/></term><description><see cref="SQLCDB.FeatureCategoryParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.FeatureCode"/> <see cref="FeatureCode.Subcategory"/></term><description><see cref="SQLCDB.FeatureSubcategoryParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.FeatureCode"/> <see cref="FeatureCode.Type"/></term><description><see cref="SQLCDB.FeatureTypeParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.FeatureSubcode"/></term><description><see cref="SQLCDB.FeatureSubcodeParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.Name"/></term><description><see cref="SQLCDB.ModelNameParamName"/></description></item>
///   <item><term><see cref="GeotypicalModelLod.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class GeotypicalModelLodTableAccessor : TableAccessor<GeotypicalModelLod>
{
    public GeotypicalModelLodTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromGeotypicalModelLodStatement, sqlCDB.InsertIntoGeotypicalModelLodStatement)
    {
    }

    /// <inheritdoc/>
    internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureCategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcategoryParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureTypeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.FeatureSubcodeParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ModelNameParamName, DbType.String);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, GeotypicalModelLod obj)
    {
        dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
        dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
        dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
        dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.LevelOfDetail.Value;
        dbCommand.Parameters[sqlCDB.FeatureCategoryParamName].Value = obj.FeatureCode.Category;
        dbCommand.Parameters[sqlCDB.FeatureSubcategoryParamName].Value = obj.FeatureCode.Subcategory;
        dbCommand.Parameters[sqlCDB.FeatureTypeParamName].Value = obj.FeatureCode.Type;
        dbCommand.Parameters[sqlCDB.FeatureSubcodeParamName].Value = obj.FeatureSubcode;
        dbCommand.Parameters[sqlCDB.ModelNameParamName].Value = obj.Name;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
