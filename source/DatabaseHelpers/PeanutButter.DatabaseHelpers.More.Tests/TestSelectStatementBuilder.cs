using System;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable InconsistentNaming

namespace PeanutButter.DatabaseHelpers.More.Tests
{
    [TestFixture]
    public class TestSelectStatementBuilder : AssertionHelper
    {
        private SelectField FieldFor(string tableName, string columnName, string alias = null)
        {
            var result = new SelectField(tableName, columnName);
            if (alias != null)
                result.SetAlias(alias);
            return result;
        }

        [Test]
        [Ignore("This is example code, no assertions")]
        public void ExampleOf_BuildingQueryWithDynamicColumnName()
        {
            //--------------- Arrange -------------------
            const string CONT = "Contract";
            const string LUTCC = "tblLUTCellCaptive";
            const string POL = "policy";
            const string POLT = "tblLUTPolicyType";
            const string PR = "tblPolicyRole";
            const string CL = "tblClient";
            const string C = "Claim";
            const string TPOL = "tblPolicy";
            const string TA = "tblAsset";
            const string TD = "tblDealer";
            const string PT = "ProductType";

            var sqlQuery = @"SELECT CONT.fCertificate as PolicyNumber,"
                                      + "CONT.fAccName as AccountName,"
                                      + "CL.fFirstName+''+CL.fSurname as CellCaptive,"
                                      + "PT.Description as ProductName,"
                                      + "CONT.fTransactionNo as TransactionNumber,"
                                      + "TD.fName as Dealer,"
                                      + "TPOL.fWebPolicyID as WebPolicyId "
                                      + " FROM Contract (NOLOCK) CONT "
                                      + " JOIN tblLUTCellCaptive(NOLOCK)LUTCC ON LUTCC.fCellCaptiveID = CONT.fCellCaptiveID"
                                      + " JOIN policy (NOLOCK) POL ON POL.ContractIntID = CONT.ContractIntID"
                                      + " JOIN tblLUTPolicyType POLT (NOLOCK) ON POLT.fPolicyTypeID = POL.PolicyTypeID"
                                      + " JOIN tblPolicyRole PR(NOLOCK) ON PR.fPolicyID = POL.PolicyIntID "
                                      + " JOIN tblClient CL(NOLOCK) ON CL.fClientID = PR.fClientID"
                                      + " JOIN Claim C(NOLOCK) ON C.PolicyID = POL.PolicyID"
                                      + " JOIN tblPolicy TPOL(NOLOCK) ON TPOL.fPolicyID = PR.fPolicyID"
                                      + " JOIN tblAsset TA(NOLOCK) on TA.fAssetID = TPOL.fAssetID"
                                      + " JOIN tblDealer(NOLOCK) TD ON TD.fDealerID = TPOL.fDealerID "
                                      + " JOIN ProductType (NOLOCK)  PT on PT.ProductTypeID = POLT.ProductTypeID "
                                      + " WHERE @option like '%'+ @searchTerm +'%' ";
            var searchOption = new SelectField(CONT, "fCertificate");

            var searchTerm = "MKWM000819";
            var replaced = sqlQuery.Replace("@option", searchOption.ToString()).Replace("@searchTerm", searchTerm);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sql = SelectStatementBuilder.Create()
                            .WithDatabaseProvider(DatabaseProviders.SQLServer)
                            .WithNoLock()
                            .WithField(FieldFor(CONT, "fAccName", "AccountName"))
                            .WithField(new ConcatenatedField(
                                            "CellCaptive",
                                            FieldFor(CL, "fFirstName"),
                                            FieldFor(CL, "fSurname")))
                            .WithField(FieldFor(PT, "Description", "ProductName"))
                            .WithField(FieldFor(CONT, "fTransactionNo", "TransactionNumber"))
                            .WithField(FieldFor(TD, "fName", "Dealer"))
                            .WithField(FieldFor(TPOL, "fWebPolicyID", "WebPolicyId"))
                            .WithTable(CONT)
                            .WithInnerJoin(CONT, "fCellCaptiveID", LUTCC)
                            .WithInnerJoin(CONT, "ContractIntID", POL)
                            .WithInnerJoin(POL, "PolicyTypeID", POLT, "fPolicyTypeID")
                            .WithInnerJoin(POL, "PolicyIntID", PR, "fPolicyID")
                            .WithInnerJoin(PR, "fClientID", CL)
                            .WithInnerJoin(POL, "PolicyID", C)
                            .WithInnerJoin(PR, "fPolicyID", TPOL)
                            .WithInnerJoin(TPOL, "fAssetID", TA)
                            .WithInnerJoin(TPOL, "fDealerID", TD)
                            .WithInnerJoin(POLT, "ProductTypeID", PT)
                            .WithCondition(searchOption, Condition.EqualityOperators.Contains, searchTerm)
                            .Build();


            //--------------- Assert -----------------------
            Console.WriteLine(replaced);
            Console.WriteLine(@"-----");
            Console.WriteLine(sql);
        }

        [Test]
        public void ShouldBeAbleToAliasTableNames()
        {
            //--------------- Arrange -------------------
            var tableName = GetRandomAlphaString();
            var tableAlias = GetRandomAlphaString();
            var fieldName = GetRandomAlphaString();
            var fieldAlias = GetRandomAlphaString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sql = SelectStatementBuilder.Create()
                        .WithTable(tableName, tableAlias)
                        .WithField(fieldName, fieldAlias)
                        .Build();


            //--------------- Assert -----------------------
            Expect(sql, Is.EqualTo($"select [{fieldName}] as [{fieldAlias}] from [{tableName}] as [{tableAlias}]"));
        }

        [Test]
        public void ShouldBeAbleToAliasJoinParts()
        {
            //--------------- Arrange -------------------
            var table1 = GetRandomAlphaString();
            var alias1 = GetRandomAlphaString();
            var field = GetRandomAlphaString();
            var table2 = GetRandomAlphaString();
            var alias2 = GetRandomAlphaString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = SelectStatementBuilder.Create()
                            .WithTable(table1, alias1)
                            .WithField(field)
                            .WithInnerJoin(alias1, field, Condition.EqualityOperators.Equals, table2, alias2, field)
                            .Build();

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(
                $"select [{field}] from [{table1}] as [{alias1}] inner join [{table2}] as [{alias2}] on [{alias1}].[{field}]=[{alias2}].[{field}]"
            ));

        }

        [Test]
        public void ShouldBeAbleToAliasJoinPartsWithNoLock()
        {
            //--------------- Arrange -------------------
            var table1 = GetRandomAlphaString();
            var alias1 = GetRandomAlphaString();
            var field = GetRandomAlphaString();
            var table2 = GetRandomAlphaString();
            var alias2 = GetRandomAlphaString();
            var expected = $"select [{field}] from [{table1}] WITH (NOLOCK) as [{alias1}] inner join [{table2}] WITH (NOLOCK) as [{alias2}] on [{alias1}].[{field}]=[{alias2}].[{field}]";

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = SelectStatementBuilder.Create()
                            .WithDatabaseProvider(DatabaseProviders.SQLServer)
                            .WithNoLock()
                            .WithTable(table1, alias1)
                            .WithField(field)
                            .WithInnerJoin(alias1, field, Condition.EqualityOperators.Equals, table2, alias2, field)
                            .Build();

            //--------------- Assert -----------------------
            Console.WriteLine(result);
            Console.WriteLine(expected);
            Expect(result, Is.EqualTo(
                expected
            ));

        }



    }
}
