Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

<TestFixture()>
Public Class TestDeleteStatementBuilder
    <Test()>
    Public Sub Create_ReturnsNewBuilderInstance()
        Dim b1 = DeleteStatementBuilder.Create(),
            b2 = DeleteStatementBuilder.Create()
        Assert.AreNotEqual(b1, b2)
        Assert.IsInstanceOf(Of DeleteStatementBuilder)(b1)
        Assert.IsInstanceOf(Of DeleteStatementBuilder)(b2)
    End Sub
    <Test()>
    Public Sub Build_WhenNoTableSpecified_Throws()
        Dim ex = Assert.Throws(Of ArgumentException)(Function()
                                                         Return DeleteStatementBuilder.Create() _
                                                                .Build()
                                                     End Function)
        StringAssert.Contains("no table specified", ex.Message)
    End Sub
    <Test()>
    Public Sub Build_WhenTableSpecifiedAndNoConditionSpecified_Throws()
        ' if someone really wants to delete everything, they should add condition 1=1
        '   this at least makes the dev think about the code and helps to prevent accidents
        Dim ex = Assert.Throws(Of ArgumentException)(Function()
                                                         Return DeleteStatementBuilder.Create() _
                                                                .WithTable(RandomValueGen.GetRandomString()) _
                                                                .Build()
                                                     End Function)
        StringAssert.Contains("no condition(s) specified", ex.Message)
    End Sub

    <Test()>
    Public Sub Build_GivenTableNameAndCondition_ReturnsExpectedSQLString()
        Dim table = RandomValueGen.GetRandomString(),
            conditionField = RandomValueGen.GetRandomString(),
            conditionValue = RandomValueGen.GetRandomString()
        Dim sql = DeleteStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithCondition(conditionField, Condition.EqualityOperators.Equals, conditionValue) _
                    .Build()
        Assert.AreEqual("delete from [" + table + "] where [" + conditionField + "]='" + conditionValue + "'", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenTableNameAndConditionForFirebird_ReturnsExpectedSQLString()
        Dim table = RandomValueGen.GetRandomString(),
            conditionField = RandomValueGen.GetRandomString(),
            conditionValue = RandomValueGen.GetRandomString()
        Dim sql = DeleteStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table) _
                    .WithCondition(conditionField, Condition.EqualityOperators.Equals, conditionValue) _
                    .Build()
        Assert.AreEqual("delete from " + table + " where " + conditionField + "='" + conditionValue + "'", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenTableNameAnd2Conditions_ReturnsExpectedSQLString()
        Dim table = RandomValueGen.GetRandomString(),
            conditionField1 = RandomValueGen.GetRandomString(),
            conditionValue1 = RandomValueGen.GetRandomString(),
            conditionField2 = RandomValueGen.GetRandomString(),
        conditionValue2 = RandomValueGen.GetRandomString()
        Dim sql = DeleteStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithCondition(conditionField1, Condition.EqualityOperators.Equals, conditionValue1) _
                    .WithCondition(conditionField2, Condition.EqualityOperators.Equals, conditionValue2) _
                    .Build()
        Assert.AreEqual("delete from [" + table + "] where [" + conditionField1 + "]='" + conditionValue1 + "' and [" + conditionField2 + "]='" + conditionValue2 + "'", sql)
    End Sub
End Class
