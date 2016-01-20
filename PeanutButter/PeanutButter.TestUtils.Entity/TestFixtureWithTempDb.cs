using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.FluentMigrator;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.TestUtils.Entity
{
    public class TestFixtureWithTempDb<TDbContext> where TDbContext: DbContext
    {
        private Func<string, IDBMigrationsRunner> CreateMigrationsRunner;
        private bool _databaseShouldHaveASPNetTables = true;
        private SemaphoreSlim _lock = new SemaphoreSlim(1,1);
        private static List<string> _aspNetTables = new List<string>();
        private readonly List<Action<TDbContext>> _toRunBeforeProvidingContext = new List<Action<TDbContext>>();

        protected TestFixtureWithTempDb()
        {
            CreateMigrationsRunner = connectionString =>
            {
                throw new Exception("Please run the Configure protected method before attempting any actual tests.");
            };
        }

        protected void Configure(bool databaseShouldHaveASPNetTables, Func<string, IDBMigrationsRunner> migrationsRunnerFactory)
        {
            _databaseShouldHaveASPNetTables = databaseShouldHaveASPNetTables;
            CreateMigrationsRunner = migrationsRunnerFactory;
        }

        protected void DisableDatabaseRegeneration()
        {
            _lock.Wait();
            _databaseLifetime = TempDatabaseLifetimes.Fixture;
        }

        protected void EnableDatabaseRegeneration()
        {
            if (_databaseLifetime == TempDatabaseLifetimes.Test)
                return;
            ReleaseTempDb();
            _lock.Release();
        }

        protected virtual void RunBeforeFirstGettingContext(Action<TDbContext> action)
        {
            lock (_toRunBeforeProvidingContext)
            {
                _toRunBeforeProvidingContext.Add(action);
            }
        }


        protected virtual TDbContext GetContext(bool logSql = false)
        {
            var connection = _tempDb.CreateConnection();
            lock (_tempDb)
            {
                var context = (TDbContext) Activator.CreateInstance(typeof(TDbContext), connection);
                if (logSql)
                    context.Database.Log = s => System.Diagnostics.Debug.WriteLine((string) s);
                if (_runBeforeFirstGettingContext)
                {
                    _runBeforeFirstGettingContext = false;
                    _toRunBeforeProvidingContext.ForEach(action => action(context));
                }
                return context;
            }
        }

        static TestFixtureWithTempDb()
        {
            _aspNetTables.Add(@"
CREATE TABLE [AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL
)");
            _aspNetTables.Add(@"
CREATE Table [AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](4000) NULL,
	[ClaimValue] [nvarchar](4000) NULL
)");
            _aspNetTables.Add(@"
CREATE TABLE [AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL
)");
            _aspNetTables.Add(@"
CREATE TABLE [AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL
)");
            _aspNetTables.Add(@"
CREATE TABLE [AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](4000) NULL,
	[SecurityStamp] [nvarchar](4000) NULL,
	[PhoneNumber] [nvarchar](4000) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL
)");
        }

        private TempDBLocalDb _tempDbActual;

        private TempDatabaseLifetimes _databaseLifetime;

        protected TempDBLocalDb _tempDb
        {
            get
            {
                if (_tempDbActual != null)
                    return _tempDbActual;
                _tempDbActual = CreateNewTempDb();
                return _tempDbActual;
            }
        }

        private List<Task> _disposeTasks = new List<Task>();

        private bool _runBeforeFirstGettingContext = true;

        private TempDBLocalDb CreateNewTempDb()
        {
            var created = new TempDBLocalDb();
            if (_databaseShouldHaveASPNetTables)
                CreateAspNetTablesOn(created.ConnectionString);
            var migrator = CreateMigrationsRunner(created.ConnectionString);
            migrator.MigrateToLatest();
            return created;
        }

        private void CreateAspNetTablesOn(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                _aspNetTables.ForEach(sql =>
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                });
            }
        }

        [SetUp]
        public void TestFixtureWithTempDbBaseSetup()
        {
            _runBeforeFirstGettingContext = true;
            if (_databaseLifetime == TempDatabaseLifetimes.Test)
            {
                _lock.Wait();
                _tempDbActual = null;
            }
        }

        [TearDown]
        public void TestFixtureWithTempDbBaseTeardown()
        {
            if (_databaseLifetime == TempDatabaseLifetimes.Test)
            {
                DisposeCurrentTempDb();
                _lock.Release();
            }
        }

        [OneTimeTearDown]
        public void TestFixtureWithTempDbBaseOneTimeTearDown()
        {
            DisposeCurrentTempDb();
            Task.WaitAll(_disposeTasks.ToArray());
            if (_databaseLifetime == TempDatabaseLifetimes.Fixture)
                _lock.Release();
        }

        private void DisposeCurrentTempDb()
        {
            var tempDb = _tempDbActual;
            _tempDbActual = null;
            _disposeTasks.Add(Task.Run(() =>
            {
                if (tempDb != null)
                    tempDb.Dispose();
            }));
        }

        private enum TempDatabaseLifetimes
        {
            Test,
            Fixture
        }

        private void ReleaseTempDb()
        {
            if (_tempDbActual != null)
                _tempDbActual.Dispose();
            _tempDbActual = null;
        }

    }
}