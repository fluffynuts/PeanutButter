Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators
Imports NExpect
Imports NExpect.Expectations
Imports PeanutButter.RandomGenerators.RandomValueGen

Namespace TestStatementBuilders
    <TestFixture()>
    Public Class TestDeleteStatementBuilder
        <Test()>
        Public Sub Create_ReturnsNewBuilderInstance()
            Dim b1 = DeleteStatementBuilder.Create(),
                b2 = DeleteStatementBuilder.Create()
            Expect(b1) _
                .Not.To.Equal(b2)
            Expect(b1) _
                .To.Be.An.Instance.Of (Of DeleteStatementBuilder)
            Expect(b2) _
                .To.Be.An.Instance.Of (Of DeleteStatementBuilder)
        End Sub

        <Test()>
        Public Sub Build_WhenNoTableSpecified_Throws()
            Assert.That(
                (Function() as String
                    return DeleteStatementBuilder.Create() _
                           .Build()
                End Function),
                Throws.Exception.InstanceOf (Of ArgumentException).With.Message.Contains("no table specified")
                )
        End Sub

        <Test()>
        Public Sub Build_WhenTableSpecifiedAndNoConditionSpecified_Throws()
            ' if someone really wants to delete everything, they should add condition 1=1
            '   this at least makes the dev think about the code and helps to prevent accidents
            Assert.That(
                (Function() as String
                    Return DeleteStatementBuilder.Create() _
                           .WithTable(GetRandomString(1)) _
                           .Build()
                End Function),
                Throws.Exception.InstanceOf (Of ArgumentException).With.Message.Contains("no condition(s) specified")
                )
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndCondition_ReturnsExpectedSQLString()
            Dim table = GetRandomString(),
                conditionField = GetRandomString(),
                conditionValue = GetRandomString()
            Dim result = DeleteStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithCondition(conditionField, Condition.EqualityOperators.Equals, conditionValue) _
                    .Build()
            Expect(result) _
                .To.Equal("delete from [" + table + "] where [" + conditionField + "]='" + conditionValue + "'")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndConditionForFirebird_ReturnsExpectedSQLString()
            Dim table = GetRandomString(),
                conditionField = GetRandomString(),
                conditionValue = GetRandomString()
            Dim result = DeleteStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table) _
                    .WithCondition(conditionField, Condition.EqualityOperators.Equals, conditionValue) _
                    .Build()
            Expect(result) _
                .To.Equal("delete from """ + table + """ where """ + conditionField + """='" + conditionValue + "'")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAnd2Conditions_ReturnsExpectedSQLString()
            Dim table = GetRandomString(),
                conditionField1 = GetRandomString(),
                conditionValue1 = GetRandomString(),
                conditionField2 = GetRandomString(),
                conditionValue2 = GetRandomString()
            Dim result = DeleteStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithCondition(conditionField1, Condition.EqualityOperators.Equals, conditionValue1) _
                    .WithCondition(conditionField2, Condition.EqualityOperators.Equals, conditionValue2) _
                    .Build()
            Expect(result) _
                .To.Equal(
                    "delete from [" + table + "] where [" + conditionField1 + "]='" + conditionValue1 + "' and [" +
                    conditionField2 + "]='" + conditionValue2 + "'")
        End Sub

        <TestCase(Condition.EqualityOperators.Equals)>
        <TestCase(Condition.EqualityOperators.GreaterThan)>
        <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.LessThan)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.NotEquals)>
        Public Sub WithCondition_GivenFielName_AndBooleanValue_ProducesExpectedResult(op As Condition.EqualityOperators)
            Dim table = GetRandomString(),
                fld = GetRandomString(),
                value = GetRandomBoolean(),
                expected = new Condition(fld, op, CInt(IIF(value, 1, 0)))

            Dim result = DeleteStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithCondition(fld, op, value) _
                    .Build()
            Expect(result) _
                .To.Equal("delete from [" + table + "] where " + expected.ToString())
        End Sub
    End Class
End NameSpace