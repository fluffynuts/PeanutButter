using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace PeanutButter.Utils.Entity
{
    public static class DbContextExtensions
    {
        public static int SaveChangesWithErrorReporting(this DbContext ctx)
        {
            try
            {
                return ctx.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                ThrowEntityValidationErrorFor(ex);
            }
            catch (DbUpdateException ex)
            {
                ThrowUpdateMessageFor(ex);
            }
            catch (Exception ex)
            {
                ThrowDefaultExceptionFor(ex);
            }
            throw new Exception("Unhandled logic path in SaveChangesWithErrorReporting");
        }

//        public static async Task<int> SaveChangesWithErrorReportingAsync(this DbContext ctx)
//        {
//            try
//            {
//                return ctx.SaveChangesAsync().Result;   // no GetAwaiter in net40
//            }
//            catch (DbEntityValidationException ex)
//            {
//                ThrowEntityValidationErrorFor(ex);
//            }
//            catch (DbUpdateException ex)
//            {
//                ThrowUpdateMessageFor(ex);
//            }
//            catch (Exception ex)
//            {
//                ThrowDefaultExceptionFor(ex);
//            }
//            throw new Exception("Unhandled logic path in SaveChangesWithErrorReporting");
//        }

        private static void ThrowDefaultExceptionFor(Exception ex)
        {
            throw new Exception("Some exception thrown during save: " + ex.Message, ex);
        }

        private static void ThrowUpdateMessageFor(DbUpdateException ex)
        {
            throw new Exception("DBUpdate Error: " + GetInnermostExceptionMessageFor(ex), ex);
        }

        private static void ThrowEntityValidationErrorFor(DbEntityValidationException ex)
        {
            var errors = string.Join(
                "\n", 
                ex.EntityValidationErrors.Select(GetErrorString));
            throw new Exception("Error whilst trying to persist to the database:\n" + errors, ex);
        }

        private static string GetErrorString(DbEntityValidationResult e)
        {
            return string.Join(" :: ",
                e.Entry.GetType().Name,
                string.Join("\n",  e.ValidationErrors.Select(GetErrorMessage)));
        }

        private static string GetErrorMessage(DbValidationError ee)
        {
            return string.Join("\n", ee.ErrorMessage);
        }

        private static string GetInnermostExceptionMessageFor(Exception ex)
        {
            if (ex.InnerException != null)
                return GetInnermostExceptionMessageFor(ex.InnerException);
            return ex.Message;
        }
    }
}