#if ASYNC
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    internal static class WriteExpressionCommandExtensionsAsync
    {
        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields, CancellationToken token)
        {
            dbCmd.UpdateOnlySql(model, onlyFields);
            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, object>> onlyFields,
            Expression<Func<T, bool>> where,
            CancellationToken token)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnlyAsync(obj, q, token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T obj,
            string[] onlyFields,
            Expression<Func<T, bool>> where,
            CancellationToken token)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnlyAsync(obj, q, token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q,
            CancellationToken token)
        {
            if (updateFields == null)
                throw new ArgumentNullException(nameof(updateFields));

            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, CachedExpressionCompiler.Evaluate(updateFields));

            q.CopyParamsTo(dbCmd);

            var updateFieldValues = updateFields.AssignedValues();
            dbCmd.GetDialectProvider().PrepareUpdateRowStatement<T>(dbCmd, updateFieldValues, q.WhereExpression);

            return dbCmd.ExecNonQueryAsync(token);
        }

        public static Task<int> UpdateAddAsync<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q)
        {
            if (updateFields == null)
                throw new ArgumentNullException(nameof(updateFields));

            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, CachedExpressionCompiler.Evaluate(updateFields));

            q.CopyParamsTo(dbCmd);

            var updateFieldValues = updateFields.AssignedValues();
            dbCmd.GetDialectProvider().PrepareUpdateRowAddStatement<T>(dbCmd, updateFieldValues, q.WhereExpression);

            return dbCmd.ExecNonQueryAsync();
        }

        internal static Task<int> UpdateNonDefaultsAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> obj, CancellationToken token)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(obj);
            q.PrepareUpdateStatement(dbCmd, item, excludeDefaults: true);
            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression, CancellationToken token)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(expression);
            q.PrepareUpdateStatement(dbCmd, item);
            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where, CancellationToken token)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var whereSql = q.Where(@where).WhereExpression;
            q.CopyParamsTo(dbCmd);
            dbCmd.PrepareUpdateAnonSql<T>(dbCmd.GetDialectProvider(), updateOnly, whereSql);

            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task InsertOnlyAsync<T>(this IDbCommand dbCmd, T obj, string[] onlyFields, CancellationToken token)
        {
            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, obj);

            var sql = dbCmd.GetDialectProvider().ToInsertRowStatement(dbCmd, obj, onlyFields);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        public static Task<int> InsertOnlyAsync<T>(this IDbCommand dbCmd, Expression<Func<T>> insertFields, CancellationToken token)
        {
            if (insertFields == null)
                throw new ArgumentNullException(nameof(insertFields));

            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, CachedExpressionCompiler.Evaluate(insertFields));

            var insertFieldsValues = insertFields.AssignedValues();
            dbCmd.GetDialectProvider().PrepareParameterizedInsertStatement<T>(dbCmd, insertFieldsValues.Keys);

            dbCmd.SetParameters(insertFieldsValues, excludeDefaults: false);

            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where, CancellationToken token)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(where);
            return dbCmd.DeleteAsync(q, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, SqlExpression<T> q, CancellationToken token)
        {
            var sql = q.ToDeleteRowStatement();
            return dbCmd.ExecuteSqlAsync(sql, q.Params, token);
        }
    }
}

#endif