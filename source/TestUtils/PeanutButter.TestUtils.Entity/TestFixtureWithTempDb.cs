using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using PeanutButter.FluentMigrator;
using PeanutButter.TempDb;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Entity.Attributes;


namespace PeanutButter.TestUtils.Entity
{
    public class TestFixtureWithTempDb<TDbContext>
        where TDbContext : DbContext
    {
        private Func<string, IDBMigrationsRunner> _createMigrationsRunner;
#pragma warning disable S2743
        private bool _databaseShouldHaveAspNetTables = true;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly List<string> AspNetTables = new List<string>();
#pragma warning restore S2743
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly List<Action<TDbContext>> _toRunBeforeProvidingContext = new List<Action<TDbContext>>();
        private readonly List<Task> _disposeTasks = new List<Task>();
        private bool _runBeforeFirstGettingContext = true;

        protected TestFixtureWithTempDb()
        {
            EnableTestIsolationInTransactions();
            _createMigrationsRunner =
                connectionString => { throw new Exception("Please run the Configure protected method before attempting any actual tests."); };
        }

        protected void Configure(
            bool databaseShouldHaveAspNetTables,
            Func<string, IDBMigrationsRunner> migrationsRunnerFactory
        )
        {
            _databaseShouldHaveAspNetTables = databaseShouldHaveAspNetTables;
            _createMigrationsRunner = migrationsRunnerFactory;
        }

        protected void DisableDatabaseRegeneration()
        {
            if (_databaseLifetime == TempDatabaseLifetimes.Fixture)
                return;
            _lock.Wait();
            _databaseLifetime = TempDatabaseLifetimes.Fixture;
        }

        // ReSharper disable once UnusedMember.Global
        protected void EnableDatabaseRegeneration()
        {
            if (_databaseLifetime == TempDatabaseLifetimes.Test)
                return;
            ReleaseTempDb();
            _lock.Release();
        }

        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void RunBeforeFirstGettingContext(Action<TDbContext> action)
        {
            lock (_toRunBeforeProvidingContext)
            {
                _toRunBeforeProvidingContext.Add(action);
            }
        }

        private void IsolateIfNecessary()
        {
            if (!_testIsolationEnabled || !_testIsolationRequired)
                return;
            _testIsolationRequired = false;
            _transactionScope = CreateTransactionScopeCapableOfSurvivingAsyncAwait();
        }

        private static TransactionScope CreateTransactionScopeCapableOfSurvivingAsyncAwait()
        {
            return new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }


        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual TDbContext GetContext(bool logSql = false)
        {
            var connection = _tempDb.CreateConnection();
            lock (_tempDb)
            {
                var context = (TDbContext) Activator.CreateInstance(typeof(TDbContext), connection);
                if (logSql)
                    context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                RunFirstTimeActionsOn(context);
                IsolateIfNecessary();
                return context;
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected void RunFirstTimeActionsOn(TDbContext context)
        {
            if (_runBeforeFirstGettingContext)
            {
                _runBeforeFirstGettingContext = false;
                lock (_toRunBeforeProvidingContext)
                {
                    _toRunBeforeProvidingContext.ForEach(action => action(context));
                }
            }
        }

        static TestFixtureWithTempDb()
        {
            AspNetTables.Add(@"
CREATE TABLE [AspNetRoles](
    [Id] [nvarchar](128) NOT NULL,
    [Name] [nvarchar](256) NOT NULL
)");
            AspNetTables.Add(@"
CREATE Table [AspNetUserClaims](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [nvarchar](128) NOT NULL,
    [ClaimType] [nvarchar](4000) NULL,
    [ClaimValue] [nvarchar](4000) NULL
)");
            AspNetTables.Add(@"
CREATE TABLE [AspNetUserLogins](
    [LoginProvider] [nvarchar](128) NOT NULL,
    [ProviderKey] [nvarchar](128) NOT NULL,
    [UserId] [nvarchar](128) NOT NULL
)");
            AspNetTables.Add(@"
CREATE TABLE [AspNetUserRoles](
    [UserId] [nvarchar](128) NOT NULL,
    [RoleId] [nvarchar](128) NOT NULL
)");
            AspNetTables.Add(@"
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

        private ITempDB _tempDbActual;

        private TempDatabaseLifetimes _databaseLifetime;
        private bool _testIsolationEnabled;
        private bool _testIsolationRequired;
        private TransactionScope _transactionScope;

        // ReSharper disable once InconsistentNaming
#pragma warning disable S100
        // ReSharper disable once InconsistentNaming
        protected ITempDB _tempDb
        {
#pragma warning restore S100
            get
            {
                if (_tempDbActual != null)
                    return _tempDbActual;
                _tempDbActual = CreateNewTempDb();
                return _tempDbActual;
            }
        }

        protected ITempDB CreateNewTempDb()
        {
            var shared = FindSharedTempDb();
            if (shared != null)
                return shared;
            var created = new TempDBLocalDb();
            if (_databaseShouldHaveAspNetTables)
                CreateAspNetTablesOn(created.ConnectionString);
            var migrator = _createMigrationsRunner(created.ConnectionString);
            migrator.MigrateToLatest();
            RegisterSharedDb(created);
            return created;
        }

        private ITempDB FindSharedTempDb()
        {
            var sharedDbName = GetSharedDbNameForThisFixture();
            if (sharedDbName == null)
                return null;
            ValidateCanShareTempDb();
            return SharedDatabaseLocator.Find(sharedDbName);
        }

        private void ValidateCanShareTempDb()
        {
            var myType = GetType();
            var requiredAttrib = myType.Assembly
                .GetCustomAttributes(true)
                .OfType<AllowSharedTempDbInstancesAttribute>()
                .FirstOrDefault();
            if (requiredAttrib == null)
                throw new SharedTempDbFeatureRequiresAssemblyAttributeException(myType);
        }

        private void RegisterSharedDb(ITempDB db)
        {
            var sharedDbName = GetSharedDbNameForThisFixture();
            if (sharedDbName == null)
                return;
            SharedDatabaseLocator.Register(sharedDbName, db);
        }

        private string GetSharedDbNameForThisFixture()
        {
            return GetType()
                .GetCustomAttributes(true)
                .OfType<UseSharedTempDbAttribute>()
                .FirstOrDefault()
                ?.Name;
        }

        private void CreateAspNetTablesOn(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                AspNetTables.ForEach(sql =>
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
            _testIsolationRequired = true;
            if (_databaseLifetime == TempDatabaseLifetimes.Test)
            {
                _lock.Wait();
                _tempDbActual = null;
            }
        }

        [TearDown]
        public void TestFixtureWithTempDbBaseTeardown()
        {
            if (_testIsolationEnabled)
            {
                RollBackIsolationTransaction();
                _testIsolationRequired = true;
            }

            if (_databaseLifetime == TempDatabaseLifetimes.Test)
            {
                DisposeCurrentTempDb();
                _runBeforeFirstGettingContext = true;
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
            if (GetSharedDbNameForThisFixture() != null)
            {
                _tempDbActual = null;
                return; // never destroy a shared db
            }

            var tempDb = _tempDbActual;
            _tempDbActual = null;
            _disposeTasks.Add(Task.Run(() => { tempDb?.Dispose(); }));
        }

        private enum TempDatabaseLifetimes
        {
            Test,
            Fixture
        }

        private void ReleaseTempDb()
        {
            _tempDbActual?.Dispose();
            _tempDbActual = null;
        }

        protected void EnableTestIsolationInTransactions()
        {
            _testIsolationEnabled = true;
            DisableDatabaseRegeneration();
        }

        protected void DisableTestIsolationInTransactions()
        {
            _testIsolationEnabled = false;
            CommitIsolationTransaction();
        }

        protected void CommitIsolationTransaction()
        {
            _transactionScope?.Complete();
            _transactionScope = null;
        }

        protected void RollBackIsolationTransaction()
        {
            _transactionScope?.Dispose();
            _transactionScope = null;
        }
    }
}