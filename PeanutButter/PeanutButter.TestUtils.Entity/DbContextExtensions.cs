using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;

namespace PeanutButter.TestUtils.Entity
{
    public static class DbContextExtensions
    {
        public static void SaveChangesWithErrorReporting(this DbContext ctx)
        {
            try
            {
                ctx.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errors = string.Join((string) "\n", (IEnumerable<string>) ex.EntityValidationErrors.Select(e =>
                    e.Entry.GetType().Name + " :: " + string.Join((string) "\n", (IEnumerable<string>) e.ValidationErrors.Select(ee => string.Join("\n", ee.ErrorMessage)))));
                throw new Exception("Error whilst trying to persist to the database:\n" + errors);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("DBUpdate Error: " + GetInnermostExceptionMessageFor(ex));
            }
            catch (Exception ex)
            {
                throw new Exception("Some exception thrown during save: " + ex.Message);
            }
        }

        private static string GetInnermostExceptionMessageFor(Exception ex)
        {
            if (ex.InnerException != null)
                return GetInnermostExceptionMessageFor(ex.InnerException);
            return ex.Message;
        }
    }
}