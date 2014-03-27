Imports PeanutButter.RandomGenerators
Imports NUnit.Framework

<TestFixture()>
Public Class TestSelectStatementBuilder
    <Test()>
    Public Sub Create_ShouldReturnNewInstanceOfSelectStatementBuilder()
        Dim builder = SelectStatementBuilder.Create()
    End Sub
    Private Function Create() As SelectStatementBuilder
        Return SelectStatementBuilder.Create()
    End Function
    <Test()>
    Public Sub WithTable_ShouldReturnBuilderInstance()
        Dim builder = Create()
        Assert.AreEqual(builder, builder.WithTable(RandomValueGen.GetRandomString()))
    End Sub

    <Test()>
    Public Sub WithField_ShouldReturnBuilderInstance()
        Dim builder = Create()
        Assert.AreEqual(builder, builder.WithField(RandomValueGen.GetRandomString()))
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneField_ShouldReturnExpectedSelectStatement()
        Dim table = RandomValueGen.GetRandomString(1),
            field = RandomValueGen.GetRandomString(1)
        Dim sql = Create() _
                  .WithTable(table) _
                  .WithField(field) _
                  .Build()
        Assert.AreEqual("select [" + field + "] from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneField_AndFirebirdProvider_ShouldReturnExpectedSelectStatement()
        Dim table = RandomValueGen.GetRandomString(1),
            field = RandomValueGen.GetRandomString(1)
        Dim sql = Create() _
                  .WithDatabaseProvider(DatabaseProviders.Firebird) _
                  .WithTable(table) _
                  .WithField(field) _
                  .Build()
        Assert.AreEqual("select " + field + " from " + table, sql)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneSelectField_ShouldReturnExpectedSelectStatement()
        Dim table = RandomValueGen.GetRandomString(),
            field = RandomValueGen.GetRandomString()
        Dim sql = Create() _
                  .WithTable(table) _
                  .WithField(New SelectField(table, field)) _
                  .Build()
        Assert.AreEqual("select [" + table + "].[" + field + "] from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndTwoFields_ShouldReturnExpectedStatement()
        Dim table = RandomValueGen.GetRandomString(1),
            f1 = RandomValueGen.GetRandomString(1),
            f2 = RandomValueGen.GetRandomString(1)
        Dim sql = Create() _
                  .WithTable(table) _
                  .WithField(f1) _
                  .WithField(f2) _
                  .Build()
        Assert.AreEqual("select [" + f1 + "],[" + f2 + "] from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub WithCondition_ShouldReturnBuilderInstance()
        Dim builder = Create()
        Assert.AreEqual(builder, builder.WithCondition(RandomValueGen.GetRandomString(1)))
    End Sub
    <Test()>
    Public Sub WithStringCondition_GivenTableAndFieldAndOneWhereClause_ShouldReturnExpectedStatement()
        Dim table = RandomValueGen.GetRandomString(1),
            field = RandomValueGen.GetRandomString(1),
            clause = RandomValueGen.GetRandomString(1)
        Dim sql = Create() _
                  .WithTable(table) _
                  .WithField(field) _
                  .WithCondition(clause) _
                  .Build()
        Assert.AreEqual("select [" + field + "] from [" + table + "] where " + clause, sql)
    End Sub

    <Test()>
    Public Sub WithInt32Condition_GivenTableAndFieldAndOneWhereClause_ShouldReturnExpectedStatement()
        Dim table = RandomValueGen.GetRandomString(1),
            field = RandomValueGen.GetRandomString(1),
            value = Int32.Parse(CStr(RandomValueGen.GetRandomInt()))
        Dim sql = Create() _
                  .WithTable(table) _
                  .WithField(field) _
                  .WithCondition(field, Condition.EqualityOperators.Equals, value) _
                  .Build()
        Assert.AreEqual("select [" + field + "] from [" + table + "] where [" + field + "]=" + value.ToString(), sql)
    End Sub

    <Test()>
    Public Sub WithInt32ConditionAndFirebirdDB_GivenTableAndFieldAndOneWhereClause_ShouldReturnExpectedStatement()
        Dim table = RandomValueGen.GetRandomString(1),
            field = RandomValueGen.GetRandomString(1),
            value = Int32.Parse(CStr(RandomValueGen.GetRandomInt()))
        Dim sql = Create() _
                  .WithDatabaseProvider(DatabaseProviders.Firebird) _
                  .WithTable(table) _
                  .WithField(field) _
                  .WithCondition(field, Condition.EqualityOperators.Equals, value) _
                  .Build()
        Assert.AreEqual("select " + field + " from " + table + " where " + field + "=" + value.ToString(), sql)
    End Sub


    <Test()>
    Public Sub GivenTableAndStarColumn_DoesNotBracketStar()
        Dim table = RandomValueGen.GetRandomString(1),
            field = "*"
        Dim sql = Create() _
                  .WithTable(table) _
                  .WithField(field) _
                  .Build()
        Assert.AreEqual("select * from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub WithAllFields_SelectsStar()
        Dim table = RandomValueGen.GetRandomString()
        Dim sql = Create() _
                    .WithAllFieldsFrom(table) _
                    .Build()
        Assert.AreEqual("select * from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub SelectAllFrom_Returns_WithAllFields_Query()
        Dim table = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.SelectAllFrom(table)
        Assert.AreEqual("select * from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndComputeField_ReturnsExpectedSql()
        Dim field = RandomValueGen.GetRandomString(),
            fieldAlias = RandomValueGen.GetRandomString(),
            table = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithComputedField(field, ComputedField.ComputeFunctions.Max, fieldAlias) _
                    .Build()
        Assert.AreEqual("select Max([" + field + "]) as [" + fieldAlias + "] from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenJoin_ReturnsExpectedSql()
        Dim field1 = "field1",
            field2 = "field2",
            joinField1 = "joinField1",
            joinField2 = "joinField2",
            table1 = "table1",
            table2 = "table2"
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, Condition.EqualityOperators.Equals, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
        Dim expectedSql = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
        Assert.AreEqual(expectedSql, sql)
    End Sub

    <Test()>
    Public Sub Build_GivenJoinWithRandomParts_ReturnsExpectedSql()
        Dim field1 = RandomValueGen.GetRandomString(),
            field2 = RandomValueGen.GetRandomString(),
            joinField1 = RandomValueGen.GetRandomString(),
            joinField2 = RandomValueGen.GetRandomString(),
            table1 = RandomValueGen.GetRandomString(),
            table2 = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, Condition.EqualityOperators.Equals, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
        Dim expectedSql = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
        Assert.AreEqual(expectedSql, sql)
    End Sub

    <Test()>
    Public Sub Build_GivenInnerJoinWithTablesAndFieldsOnly_InfersEqualityOperator()
        Dim field1 = RandomValueGen.GetRandomString(),
            field2 = RandomValueGen.GetRandomString(),
            joinField1 = RandomValueGen.GetRandomString(),
            joinField2 = RandomValueGen.GetRandomString(),
            table1 = RandomValueGen.GetRandomString(),
            table2 = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
        Dim expectedSql = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
        Assert.AreEqual(expectedSql, sql)
    End Sub

    <Test()>
    Public Sub Build_GivenInnerJoinWithTablesAndFieldsOnlyWithFirebirdProvider_InfersEqualityOperator()
        Dim field1 = RandomValueGen.GetRandomString(),
            field2 = RandomValueGen.GetRandomString(),
            joinField1 = RandomValueGen.GetRandomString(),
            joinField2 = RandomValueGen.GetRandomString(),
            table1 = RandomValueGen.GetRandomString(),
            table2 = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
        Dim expectedSql = "select " + field1 + "," + field2 + " from " + table1 + " inner join " + table2 + " on " + table1 + "." + joinField1 + "=" + table2 + "." + joinField2
        Assert.AreEqual(expectedSql, sql)
    End Sub

    <Test()>
    Public Sub Build_GivenInnerJoinWithLeftTableLeftFieldAndRightTableOnly_InfersEqualityOperatorAndRightField()
        Dim field1 = RandomValueGen.GetRandomString(),
            field2 = RandomValueGen.GetRandomString(),
            joinField1 = RandomValueGen.GetRandomString(),
            table1 = RandomValueGen.GetRandomString(),
            table2 = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, table2) _
                    .WithField(field2) _
                    .Build()
        Dim expectedSql = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField1 + "]"
        Assert.AreEqual(expectedSql, sql)
    End Sub

    <Test()>
    Public Sub WithCondition_GivenSelectFieldAndStringValue_ProducesExpectedResult()
        Dim fld = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim val = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, Condition.EqualityOperators.Equals, val) _
                    .Build()
        Assert.AreEqual("select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + "='" + val + "'", sql)
    End Sub

    <Test()>
    Public Sub WithCondition_GivenTwoSelectFields_ProducesExpectedResult()
        Dim f1 = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString()),
            f2 = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim sql = SelectStatementBuilder.Create() _
                  .WithTable(f1.Table) _
                  .WithTable(f2.Table) _
                  .WithField(f1.Field) _
                  .WithCondition(f1, Condition.EqualityOperators.Equals, f2) _
                  .Build()
        Assert.AreEqual("select [" + f1.Field + "] from [" + f1.Table + "],[" + f2.Table + "] where " + f1.ToString() + "=" + f2.ToString(), sql)
    End Sub

    <Test()>
    Public Sub WithCondition_GivenSelectFieldAndInt16Value_ProducesExpectedResult()
        Dim fld = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim val = Int16.Parse(CStr(RandomValueGen.GetRandomInt(1, 100)))
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, Condition.EqualityOperators.Equals, val) _
                    .Build()
        Assert.AreEqual("select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + "=" + val.ToString(), sql)
    End Sub

    <TestCase(Condition.EqualityOperators.Equals)>
    <TestCase(Condition.EqualityOperators.GreaterThan)>
    <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.LessThan)>
    <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.NotEquals)>
    Public Sub WithCondition_GivenSelectFieldAndInt32Value_ProducesExpectedResult(op As Condition.EqualityOperators)
        Dim fld = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim val = Int32.Parse(CStr(RandomValueGen.GetRandomInt(1, 100)))
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
        Assert.AreEqual("select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString(), sql)
    End Sub

    <TestCase(Condition.EqualityOperators.Equals)>
    <TestCase(Condition.EqualityOperators.GreaterThan)>
    <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.LessThan)>
    <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.NotEquals)>
    Public Sub WithCondition_GivenSelectFieldAndInt64Value_ProducesExpectedResult(op As Condition.EqualityOperators)
        Dim fld = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim val = Int64.Parse(CStr(RandomValueGen.GetRandomInt(1, 100)))
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
        Assert.AreEqual("select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString(), sql)
    End Sub

    <TestCase(Condition.EqualityOperators.Equals)>
    <TestCase(Condition.EqualityOperators.GreaterThan)>
    <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.LessThan)>
    <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.NotEquals)>
    Public Sub WithCondition_GivenSelectFieldAndDecimalValue_ProducesExpectedResult(op As Condition.EqualityOperators)
        Dim fld = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim val = Decimal.Parse(CStr(RandomValueGen.GetRandomInt(1, 100)))
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
        Assert.AreEqual("select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString(), sql)
    End Sub

    <TestCase(Condition.EqualityOperators.Equals)>
    <TestCase(Condition.EqualityOperators.GreaterThan)>
    <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.LessThan)>
    <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.NotEquals)>
    Public Sub WithCondition_GivenSelectFieldAndDoubleValue_ProducesExpectedResult(op As Condition.EqualityOperators)
        Dim fld = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim val = Double.Parse(CStr(RandomValueGen.GetRandomInt(1, 100)))
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
        Assert.AreEqual("select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString(), sql)
    End Sub

    <TestCase(Condition.EqualityOperators.Equals)>
    <TestCase(Condition.EqualityOperators.GreaterThan)>
    <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.LessThan)>
    <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.NotEquals)>
    Public Sub WithCondition_GivenSelectFieldAndDateValue_ProducesExpectedResult(op As Condition.EqualityOperators)
        Dim fld = New SelectField(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString())
        Dim val = RandomValueGen.GetRandomDate()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
        Assert.AreEqual("select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + "'" + val.ToString("yyyy/MM/dd") + "'", sql)
    End Sub

    <TestCase(Condition.EqualityOperators.Equals)>
    <TestCase(Condition.EqualityOperators.GreaterThan)>
    <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.LessThan)>
    <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
    <TestCase(Condition.EqualityOperators.NotEquals)>
    Public Sub WithCondition_CanAcceptAConditionObject(op As Condition.EqualityOperators)
        Dim c = New Condition(RandomValueGen.GetRandomString(), op, RandomValueGen.GetRandomString())
        Dim table = RandomValueGen.GetRandomString()
        Dim fld = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithCondition(c) _
                    .Build()
        Assert.AreEqual("select [" + fld + "] from [" + table + "] where " + c.ToString(), sql)
    End Sub

    <TestCase(OrderBy.Directions.Descending)>
    <TestCase(OrderBy.Directions.Ascending)>
    Public Sub OrderBy_AddsExpectedOrderByClauseToOutput(direction As OrderBy.Directions)
        Dim o = New OrderBy(RandomValueGen.GetRandomString(), RandomValueGen.GetRandomString(), direction)
        Dim table = RandomValueGen.GetRandomString()
        Dim fld = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .OrderBy(o) _
                    .Build()
        Assert.AreEqual("select [" + fld + "] from [" + table + "] " + o.ToString(), sql)
    End Sub

    <TestCase(OrderBy.Directions.Ascending)>
    <TestCase(OrderBy.Directions.Descending)>
    Public Sub OrderBy_GivenFieldAndDirection_AddsExpectedOrderByClauseToOutput(direction As OrderBy.Directions)
        Dim ofld = RandomValueGen.GetRandomString(),
            table = RandomValueGen.GetRandomString(),
            fld = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .OrderBy(ofld, direction) _
                    .Build()
        Assert.AreEqual("select [" + fld + "] from [" + table + "] order by [" + ofld + "] " + CStr(IIf(direction = OrderBy.Directions.Ascending, "asc", "desc")), sql)
    End Sub

    <TestCase(OrderBy.Directions.Ascending)>
    <TestCase(OrderBy.Directions.Descending)>
    Public Sub OrderBy_GivenFieldAndTableAndDirection_AddsExpectedOrderByClauseToOutput(direction As OrderBy.Directions)
        Dim ofld = RandomValueGen.GetRandomString(),
            otable = RandomValueGen.GetRandomString(),
            table = RandomValueGen.GetRandomString(),
            fld = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .OrderBy(otable, ofld, direction) _
                    .Build()
        Assert.AreEqual("select [" + fld + "] from [" + table + "] order by [" + otable + "].[" + ofld + "] " + CStr(IIf(direction = OrderBy.Directions.Ascending, "asc", "desc")), sql)
    End Sub

    <Test()>
    Public Sub WithAllConditions_GivenSomeConditions_ReturnsExpectedStatement()
        Dim fld = RandomValueGen.GetRandomString(),
            table = RandomValueGen.GetRandomString(),
            c1 = New Condition(RandomValueGen.GetRandomString(), Condition.EqualityOperators.Equals, RandomValueGen.GetRandomString()),
            c2 = New Condition(RandomValueGen.GetRandomString(), Condition.EqualityOperators.GreaterThan, RandomValueGen.GetRandomString()),
            c3 = New Condition(RandomValueGen.GetRandomString(), Condition.EqualityOperators.LessThan, RandomValueGen.GetRandomString())
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithAllConditions(c1, c2, c3) _
                    .Build()
        Assert.AreEqual("select [" + fld + "] from [" + table + "] where (" + c1.ToString() + " and " + c2.ToString() + " and " + c3.ToString() +")", sql)
    End Sub

    <Test()>
    Public Sub WithAnyConditions_GivenSomeConditions_ReturnsExpectedStatement()
        Dim fld = RandomValueGen.GetRandomString(),
            table = RandomValueGen.GetRandomString(),
            c1 = New Condition(RandomValueGen.GetRandomString(), Condition.EqualityOperators.Equals, RandomValueGen.GetRandomString()),
            c2 = New Condition(RandomValueGen.GetRandomString(), Condition.EqualityOperators.GreaterThan, RandomValueGen.GetRandomString()),
            c3 = New Condition(RandomValueGen.GetRandomString(), Condition.EqualityOperators.LessThan, RandomValueGen.GetRandomString())
        Dim sql = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithAnyCondition(c1, c2, c3) _
                    .Build()
        Assert.AreEqual("select [" + fld + "] from [" + table + "] where (" + c1.ToString() + " or " + c2.ToString() + " or " + c3.ToString() + ")", sql)
    End Sub

    <Test()>
    Public Sub Build_WhenDistinctSpecified_ProducesDistinctQuery()
        ' setup
        Dim table = RandomValueGen.GetRandomString()
        ' assert pre-conditions
        ' perform test
        Dim sql = SelectStatementBuilder.Create().Distinct().WithAllFieldsFrom(table).Build()
        ' assert test results
        Assert.AreEqual("select distinct * from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenFieldWithAlias_ProducesQueryWithAliasedField()
        Dim table = RandomValueGen.GetRandomString(),
            field = RandomValueGen.GetRandomString(),
            aliasAs = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create().WithTable(table).WithField(field, aliasAs).Build()

        Assert.AreEqual("select [" + field + "] as [" + aliasAs + "] from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenTop_ProducesQueryWithTopRequirement()
        Dim table = RandomValueGen.GetRandomString(),
            field = RandomValueGen.GetRandomString(),
            topVal = RandomValueGen.GetRandomInt()
        Dim sql = SelectStatementBuilder.Create().WithTable(table).WithField(field).WithTop(topVal).Build()
        Assert.AreEqual("select top " + topVal.ToString() + " [" + field + "] from [" + table + "]", sql)
    End Sub

    <Test()>
    Public Sub Build_GivenLike_ProducesQueryWithLike()
        Dim table = RandomValueGen.GetRandomString(),
            field = RandomValueGen.GetRandomString(),
            val = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create().WithTable(table).WithField(field).WithCondition(field, Condition.EqualityOperators.Like_, val).Build()
        Assert.AreEqual("select [" + field + "] from [" + table + "] where [" + field + "] like '%" + val + "%'", sql)
    End Sub

    <Test()>
    Public Sub WithFields_AddsAllFieldsInSpecifiedOrder()
        Dim table = RandomValueGen.GetRandomString(),
            f1 = RandomValueGen.GetRandomString(),
            f2 = RandomValueGen.GetRandomString(),
            f3 = RandomValueGen.GetRandomString()
        Dim sql = SelectStatementBuilder.Create().WithTable(table).WIthFIelds(f1, f2, f3).Build()
        Assert.AreEqual("select [" + f1 + "],[" + f2 + "],[" + f3 + "] from [" + table + "]", sql)
    End Sub

End Class
