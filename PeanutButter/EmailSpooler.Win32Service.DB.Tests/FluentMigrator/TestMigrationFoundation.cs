using System;
using EmailSpooler.Win32Service.DB.FluentMigrator;
using FluentMigrator;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace EmailSpooler.Win32Service.DB.Tests.FluentMigrator
{
    [TestFixture]
    public class TestMigrationFoundation
    {
        [Test]
        public void Type_ShouldInheritFrom_FluentMigrato_Migration()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (MigrationFoundation);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Migration>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void CreateLastModifiedTriggerFor_GivenTableNameAndIdCol_ShouldProduceExpectedSql()
        {
            //---------------Set up test pack-------------------
            var sut = new MigrationFoundation_EXPOSES_Innards();
            var tableName = RandomValueGen.GetRandomAlphaString(5, 10);
            var idCol = RandomValueGen.GetRandomAlphaString(5, 10);
            var expected = @"create trigger [dbo].[trLastUpdated_" + tableName + @"]
on [dbo].[" + tableName + @"]
for update
as
begin
set NOCOUNT ON;
update [dbo].[" + tableName + @"] set LastModified = CURRENT_TIMESTAMP where [" + idCol + @"] in (select [" + idCol + @"] from inserted);
end".Replace("\r", "");
            var expectedLines = expected.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.BaseCreateLastModifiedTriggerFor(tableName, idCol);

            //---------------Test Result -----------------------
            var resultLines = result.Split(new[] {"\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            CollectionAssert.AreEqual(expectedLines, resultLines);
        }


        private class MigrationFoundation_EXPOSES_Innards: MigrationFoundation
        {
            public override void Up()
            {
                throw new System.NotImplementedException();
            }

            public override void Down()
            {
                throw new System.NotImplementedException();
            }

            public string BaseCreateLastModifiedTriggerFor(string tableName, string idCol)
            {
                return base.CreateLastModifiedTriggerSqlFor(tableName, idCol);
            }
        }

    }
}
