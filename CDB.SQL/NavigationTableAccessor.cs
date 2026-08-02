using System.Data;
using System.Data.Common;
using static Silnith.CDB.SQL.SQLCDB;

namespace Silnith.CDB.SQL;

/// <summary>
/// A table accessor for type <see cref="Navigation"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <listheader><term><see cref="Navigation"/> Property</term><description>SQL Statement Parameter</description></listheader>
///   <item><term><see cref="Navigation.Dataset"/></term><description><see cref="SQLCDB.DatasetParamName"/></description></item>
///   <item><term><see cref="Navigation.ComponentSelector1"/></term><description><see cref="SQLCDB.ComponentSelector1ParamName"/></description></item>
///   <item><term><see cref="Navigation.ComponentSelector2"/></term><description><see cref="SQLCDB.ComponentSelector2ParamName"/></description></item>
///   <item><term><see cref="Navigation.FileType"/></term><description><see cref="SQLCDB.FileTypeParamName"/></description></item>
/// </list>
/// </remarks>
public class NavigationTableAccessor : TableAccessor<Navigation>
{
    public NavigationTableAccessor(SQLCDB sqlCDB)
        : base(sqlCDB, sqlCDB.SelectFromNavigationStatement, sqlCDB.InsertIntoNavigationStatement)
    {
    }

    /// <inheritdoc/>
    internal override void CreateAndAttachObjectParameter(DbCommand dbCommand)
    {
        CreateAndAttachParameter(dbCommand, sqlCDB.DatasetParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector1ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.ComponentSelector2ParamName, DbType.Int32);
        CreateAndAttachParameter(dbCommand, sqlCDB.FileTypeParamName, DbType.String);
    }

    /// <inheritdoc/>
    internal override void SetObjectParameters(DbCommand dbCommand, Navigation obj)
    {
        dbCommand.Parameters[sqlCDB.DatasetParamName].Value = obj.Dataset.Value;
        dbCommand.Parameters[sqlCDB.ComponentSelector1ParamName].Value = obj.ComponentSelector1;
        dbCommand.Parameters[sqlCDB.ComponentSelector2ParamName].Value = obj.ComponentSelector2;
        dbCommand.Parameters[sqlCDB.FileTypeParamName].Value = obj.FileType;
    }
}
