Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

<TestFixture()>
Public Class TestDataCopyStatementBuilder
    <Test()>
    Public Sub Create_ShouldReturnNewInstanceOfBuilder()
        Dim builder1 = DataCopyStatementBuilder.Create(),
            builder2 = DataCopyStatementBuilder.Create()
        Assert.AreNotEqual(builder1, builder2)
        Assert.IsInstanceOf(Of IDataCopyStatementBuilder)(builder1)
        Assert.IsInstanceOf(Of IDataCopyStatementBuilder)(builder2)
    End Sub

    Private Function Create() as IDataCopyStatementBuilder
        Return DataCopyStatementBuilder.Create()
    End Function

    <Test()>
    Public Sub Build_GivenNoSourceTable_ShouldThrow()
        Dim ex = Assert.Throws(Of ArgumentException)(Function() as String
                                                         Return Create().Build()
                                                     End Function)
        StringAssert.Contains("source table not set", ex.Message)
    End Sub

    <Test()>
    Public Sub Build_GivenSourceTableAndNoTargetTable_ShouldThrow()
        Dim ex = Assert.Throws(Of ArgumentException)(Function() as String
                                                         Return Create() _
                                                                .WithSourceTable(RandomValueGen.GetRandomString(1)) _
                                                                .Build()
                                                     End Function)
        StringAssert.Contains("target table not set", ex.Message)
    End Sub

    <Test()>
    Public Sub Build_GivenSourceAndTargetTableButNoFieldMappings_ShouldThrow()
        Dim ex = Assert.Throws(Of ArgumentException)(Function() As String
                                                         Return Create() _
                                                             .WithSourceTable(RandomValueGen.GetRandomString(1)) _
                                                             .WithTargetTable(RandomValueGen.GetRandomString(1)) _
                                                             .Build()
                                                     End Function)
        StringAssert.Contains("no fields set", ex.Message)
    End Sub

    <Test()>
    Public Sub Build_GivenSourceTargetAndOneFieldMapping_ShouldReturnExpectedString()
        Dim src = RandomValueGen.GetRandomString(1, 5),
            target = RandomValueGen.GetRandomString(1, 5),
            srcField = RandomValueGen.GetRandomString(1, 5),
            targetField = RandomValueGen.GetRandomString(1, 5)
        Dim sql = Create() _
                  .WithSourceTable(src) _
                  .WithTargetTable(target) _
                  .WithFieldMapping(srcField, targetField) _
                  .Build()
        Assert.AreEqual(String.Join("", New String() { _
            "insert into [", target, "] ([", targetField, "]) select [", srcField, "] from [", src, "]" _
             }), sql)
    End Sub

    <Test()>
    Public Sub Build_GivenSOurceTargetAndTwoFieldMappings_ShouldReturnExpectedString()
        Dim src = RandomValueGen.GetRandomString(1, 5),
            target = RandomValueGen.GetRandomString(1, 5),
            srcField1 = RandomValueGen.GetRandomString(1, 5),
            targetField1 = RandomValueGen.GetRandomString(1, 5),
            srcField2 = RandomValueGen.GetRandomString(1, 5),
            targetField2 = RandomValueGen.GetRandomString(1, 5)

        Dim sql = Create() _
                  .WithSourceTable(src) _
                  .WithTargetTable(target) _
                  .WithFieldMapping(srcField1, targetField1) _
                  .WithFieldMapping(srcField2, targetField2) _
                  .Build()
        Assert.AreEqual(String.Join("", New String() { _
            "insert into [", target, "] ([", targetField1, "],[", targetField2, "]) select [", srcField1, "],[", srcField2, "] from [", src, "]" _
             }), sql)
    End Sub

    <TestCase(DatabaseProviders.Access)>
    <TestCase(DatabaseProviders.SQLServer)>
    <TestCase(DatabaseProviders.SQLite)>
    Public Sub Build_GivenSourceTargetAndTwoFieldMappingsAndCriteria_ShouldReturnExpectedString(provider As DatabaseProviders)
        Dim src = RandomValueGen.GetRandomString(1, 5),
            target = RandomValueGen.GetRandomString(1, 5),
            srcField1 = RandomValueGen.GetRandomString(1, 5),
            targetField1 = RandomValueGen.GetRandomString(1, 5),
            srcField2 = RandomValueGen.GetRandomString(1, 5),
            targetField2 = RandomValueGen.GetRandomString(1, 5),
            criteria = RandomValueGen.GetRandomString(1, 5)

        Dim sql = Create() _
                  .WithDatabaseProvider(provider) _
                  .WithSourceTable(src) _
                  .WithTargetTable(target) _
                  .WithFieldMapping(srcField1, targetField1) _
                  .WithFieldMapping(srcField2, targetField2) _
                  .WithCriteria(criteria) _
                  .Build()
        Assert.AreEqual(String.Join("", New String() { _
            "insert into [", target, "] ([", targetField1, "],[", targetField2, _
            "]) select [", srcField1, "],[", srcField2, "] from [", src, "]", _
            " where ", criteria
             }), sql)
    End Sub

    
    <Test()>
    Public Sub Build_GivenFirebirdProvider_ShouldBuildAppropriateSQLString()
        Dim src = RandomValueGen.GetRandomString(1, 5),
            target = RandomValueGen.GetRandomString(1, 5),
            srcField1 = RandomValueGen.GetRandomString(1, 5),
            targetField1 = RandomValueGen.GetRandomString(1, 5),
            srcField2 = RandomValueGen.GetRandomString(1, 5),
            targetField2 = RandomValueGen.GetRandomString(1, 5),
            criteria = RandomValueGen.GetRandomString(1, 5)

        Dim sql = Create() _
                  .WithDatabaseProvider(DatabaseProviders.Firebird) _
                  .WithSourceTable(src) _
                  .WithTargetTable(target) _
                  .WithFieldMapping(srcField1, targetField1) _
                  .WithFieldMapping(srcField2, targetField2) _
                  .WithCriteria(criteria) _
                  .Build()
        Assert.AreEqual(String.Join("", New String() { _
            "insert into """, target, """ (""", targetField1, """,""", targetField2, _
            """) select """, srcField1, """,""", srcField2, """ from """, src, """", _
            " where ", criteria
             }), sql)
    End Sub

End Class
