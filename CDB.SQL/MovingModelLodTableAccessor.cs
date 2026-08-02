using System.Data;
using System.Data.Common;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="MovingModelLod"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="MovingModelLod"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="MovingModelLod.Dataset"/></term><description><see cref="SQLCDB.DatasetParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.ComponentSelector1"/></term><description><see cref="SQLCDB.ComponentSelector1ParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.ComponentSelector2"/></term><description><see cref="SQLCDB.ComponentSelector2ParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.LevelOfDetail"/></term><description><see cref="SQLCDB.LevelOfDetailParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Kind"/></term><description><see cref="SQLCDB.DISKindParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Domain"/></term><description><see cref="SQLCDB.DISDomainParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Country"/></term><description><see cref="SQLCDB.DISCountryParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Category"/></term><description><see cref="SQLCDB.DISCategoryParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Subcategory"/></term><description><see cref="SQLCDB.DISSubcategoryParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Specific"/></term><description><see cref="SQLCDB.DISSpecificParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.MMDC"/> <see cref="DISEntity.Extra"/></term><description><see cref="SQLCDB.DISExtraParamName"/></description></item>
///   <item><term><see cref="MovingModelLod.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class MovingModelLodTableAccessor : TableAccessor<MovingModelLod>
{
    public MovingModelLodTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromMovingModelLodStatement, sqlCDB.InsertIntoMovingModelLodStatement)
    {
    }

    /// <inheritdoc/>
    internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.LevelOfDetailParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DISKindParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DISDomainParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DISCountryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DISCategoryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DISSubcategoryParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DISSpecificParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.DISExtraParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, MovingModelLod obj)
    {
        dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
        dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
        dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
        dbCommand.Parameters[sqlCDB.LevelOfDetailParamName].Value = obj.LevelOfDetail.Value;
        dbCommand.Parameters[sqlCDB.DISKindParamName].Value = obj.MMDC.Kind;
        dbCommand.Parameters[sqlCDB.DISDomainParamName].Value = obj.MMDC.Domain;
        dbCommand.Parameters[sqlCDB.DISCountryParamName].Value = obj.MMDC.Country;
        dbCommand.Parameters[sqlCDB.DISCategoryParamName].Value = obj.MMDC.Category;
        dbCommand.Parameters[sqlCDB.DISSubcategoryParamName].Value = obj.MMDC.Subcategory;
        dbCommand.Parameters[sqlCDB.DISSpecificParamName].Value = obj.MMDC.Specific;
        dbCommand.Parameters[sqlCDB.DISExtraParamName].Value = obj.MMDC.Extra;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
