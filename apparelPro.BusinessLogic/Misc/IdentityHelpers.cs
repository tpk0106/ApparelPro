using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace apparelPro.BusinessLogic.Misc
{
    public static class IdentityHelpers
    {
        public static async Task EnableIdentityInsertAsync<T>(this DbContext context) => 
            await SetIdentityInsertAsync<T>(context,enable:true);
        public static async Task DisableIdentityInsertAsync<T>(this DbContext context) => 
            await SetIdentityInsertAsync<T>(context, enable: false);

        private static async Task SetIdentityInsertAsync<T>(DbContext context, bool enable)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var et = context.Model.FindEntityType(typeof(T));
            var tablenameWithSchema = GetName(et);
            
            //var entityType = context.Model.FindEntityType(typeof(T));
            //var tn = entityType.GetTableName();
            //var sc = entityType.GetSchema();
            var value = enable ? "ON" : "OFF";
         
            await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {tablenameWithSchema} {value}");            
        }

        public static async Task SaveChangesWithIdentityInsertAsync<T>(this DbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var model = context.Model;
          //  await context.EnableIdentityInsertAsync<T>();
           
            using (var transaction = context.Database.BeginTransaction())
            {
               // await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Countries ON");
                await context.EnableIdentityInsertAsync<T>();              
                await context.SaveChangesAsync();
                //await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Countries OFF");
                await context.DisableIdentityInsertAsync<T>();
                await transaction.CommitAsync();
            }
            
            //await context.DisableIdentityInsertAsync<T>();
        }

        // https://www.codeproject.com/Articles/5339402/Delete-All-Rows-in-Entity-Framework-Core-6
        // https://zeffron.wordpress.com/2016/06/03/using-sql-identity-insert-with-entity-framework-making-it-work/
        private static string GetName(IEntityType? entityType,
                                      string? defaultSchemaName = "dbo")
        {
            /*3.0.1 these were working*/
            //var schemaName = entityType.GetSchema();
            //var tableName = entityType.GetTableName();

            /*5 and 6 these are working*/
            var schema = entityType?.FindAnnotation("Relational:Schema")?.Value;
            string tableName = entityType?.GetAnnotation("Relational:TableName")?.Value.ToString();
            string schemaName = schema == null ? defaultSchemaName : schema.ToString();
            /*table full name*/
            string name = string.Format("[{0}].[{1}]", schemaName, tableName);
            //string name = string.Format("{0}", tableName);
            return name;
        }

        public static string TableName<T>(DbContext dbContext) where T : class
        {
            var entityType = dbContext.Model.FindEntityType(typeof(T));
            return GetName(entityType);
        }

        public static string TableName<T>(DbSet<T> dbSet) where T : class
        {
            var entityType = dbSet.EntityType;
            return GetName(entityType);
        }
    }
}
