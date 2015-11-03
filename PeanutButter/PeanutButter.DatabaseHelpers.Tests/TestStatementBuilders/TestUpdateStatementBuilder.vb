Imports NSubstitute
Imports NUnit.Framework
Imports System.Globalization
Imports PeanutButter.RandomGenerators
Imports PeanutButter.Utils
Imports PeanutButter.DatabaseHelpers

<TestFixture()>
Public Class TestUpdateStatementBuilder
    <Test()>
    Public Sub Create_ShouldReturnNewInstanceOfUpdateStatementBuilder()
        Dim builder = UpdateStatementBuilder.Create()
        Assert.IsInstanceOf(Of UpdateStatementBuilder)(builder)
    End Sub
    Private Function Create() As UpdateStatementBuilder
        Return UpdateStatementBuilder.Create()
    End Function
    <Test()>
    Public Sub Build_WhenNoUpdateTableOrFieldsDefined_ThrowsArgumentException()
        Assert.Throws(Of ArgumentException)(Sub() Create().Build())
    End Sub
    <Test()>
    Public Sub Build_GivenTableAndOneStringFieldAndCondition_ReturnsValidUpdateStatement()
        Dim TableName = RandomValueGen.GetRandomString(),
            ColumnName = RandomValueGen.GetRandomString(),
            FieldValue = RandomValueGen.GetRandomString(),
            Condition = RandomValueGen.GetRandomString()
        Dim builder = UpdateStatementBuilder.Create()
        With builder
            .WithTable(TableName)
            .WithField(ColumnName, FieldValue)
            .WithCondition(Condition)
        End With
        Dim statement = builder.Build()
        Assert.AreEqual("update [" & TableName & "] set [" & ColumnName & "]='" & FieldValue & "' where " & Condition, statement)
    End Sub
    <Test()>
    Public Sub Build_GivenTableAndOneDecimalFieldAndCondition_ReturnsValidUpdateStatement()
        Dim TableName = RandomValueGen.GetRandomString(),
            ColumnName = RandomValueGen.GetRandomString(),
            FieldValue = RandomValueGen.GetRandomDecimal(),
            Condition = RandomValueGen.GetRandomString()
        Dim builder = UpdateStatementBuilder.Create()
        With builder
            .WithTable(TableName)
            .WithField(ColumnName, FieldValue)
            .WithCondition(Condition)
        End With
        Dim statement = builder.Build()
        Dim nfi = New NumberFormatInfo()
        nfi.NumberDecimalSeparator = "."
        nfi.CurrencyDecimalSeparator = "."
        dim dd = new DecimalDecorator(FieldValue)
        Assert.AreEqual("update [" & TableName & "] set [" & ColumnName & "]=" & dd.ToString() & " where " & Condition, statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneNullableDecimalFieldAndCondition_ReturnsValidUpdateStatement()
        Dim TableName = RandomValueGen.GetRandomString(),
            ColumnName = RandomValueGen.GetRandomString(),
            FieldValue as Nullable(Of Decimal)= RandomValueGen.GetRandomDecimal(),
            Condition = RandomValueGen.GetRandomString()
        Dim builder = UpdateStatementBuilder.Create()
        With builder
            .WithTable(TableName)
            .WithField(ColumnName, FieldValue)
            .WithCondition(Condition)
        End With
        Dim statement = builder.Build()
        Dim nfi = New NumberFormatInfo()
        nfi.NumberDecimalSeparator = "."
        nfi.CurrencyDecimalSeparator = "."
        Dim dd = new DecimalDecorator(FieldValue.Value)
        Assert.AreEqual("update [" & TableName & "] set [" & ColumnName & "]=" & dd.ToString() & " where " & Condition, statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneNullableDecimalFieldAndDecimalCondition_ReturnsValidUpdateStatement()
        Dim TableName = RandomValueGen.GetRandomString(),
            ColumnName = RandomValueGen.GetRandomString(),
            FieldValue as Nullable(Of Decimal)= RandomValueGen.GetRandomDecimal(),
            thisCondition = new Condition("condition_field", Condition.EqualityOperators.Equals, FieldValue.Value)
        Dim builder = UpdateStatementBuilder.Create()
        With builder
            .WithTable(TableName)
            .WithField(ColumnName, FieldValue)
            .WithCondition(thisCondition)
        End With
        Dim statement = builder.Build()
        Dim nfi = New NumberFormatInfo()
        nfi.NumberDecimalSeparator = "."
        nfi.CurrencyDecimalSeparator = "."
        Dim dd = new DecimalDecorator(FieldValue.Value)
        Assert.IsFalse(thisCondition.ToString().Contains(","))
        Assert.AreEqual("update [" & TableName & "] set [" & ColumnName & "]=" & dd.ToString() & " where " & thisCondition.ToString(), statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneNullableDecimalFieldAndNullableDecimalCondition_ReturnsValidUpdateStatement()
        Dim TableName = RandomValueGen.GetRandomString(),
            ColumnName = RandomValueGen.GetRandomString(),
            FieldValue as Nullable(Of Decimal)= RandomValueGen.GetRandomDecimal(),
            thisCondition = new Condition("condition_field", Condition.EqualityOperators.Equals, FieldValue)
        Dim builder = UpdateStatementBuilder.Create()
        With builder
            .WithTable(TableName)
            .WithField(ColumnName, FieldValue)
            .WithCondition(thisCondition)
        End With
        Dim statement = builder.Build()
        Dim nfi = New NumberFormatInfo()
        nfi.NumberDecimalSeparator = "."
        nfi.CurrencyDecimalSeparator = "."
        Dim dd = new DecimalDecorator(FieldValue.Value)
        Assert.IsFalse(thisCondition.ToString().Contains(","))
        Assert.AreEqual("update [" & TableName & "] set [" & ColumnName & "]=" & dd.ToString() & " where " & thisCondition.ToString(), statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneNullableNullDecimalFieldAndCondition_ReturnsValidUpdateStatement()
        Dim TableName = RandomValueGen.GetRandomString(),
            ColumnName = RandomValueGen.GetRandomString(),
            FieldValue As Nullable(Of Decimal) = Nothing,
            Condition = RandomValueGen.GetRandomString()
        Dim builder = UpdateStatementBuilder.Create()
        With builder
            .WithTable(TableName)
            .WithField(ColumnName, FieldValue)
            .WithCondition(Condition)
        End With
        Dim statement = builder.Build()
        Assert.AreEqual("update [" & TableName & "] set [" & ColumnName & "]=NULL where " & Condition, statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneDateTimeFieldAndCondition_ReturnsValidUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(),
            columnName = RandomValueGen.GetRandomString(),
            FieldValue = DateTime.Now,
            Condition = RandomValueGen.GetRandomString()
        Dim dfi = New DateTimeFormatInfo()
        dfi.DateSeparator = "/"
        dfi.TimeSeparator = ":"
        Dim valueString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", dfi)
        Dim statement = UpdateStatementBuilder.Create() _
                        .WithTable(tableName) _
                        .WithField(columnName, FieldValue) _
                        .WithCondition(Condition) _
                        .Build()
        Assert.AreEqual("update [" + tableName + "] set [" + columnName + "]='" + valueString + "' where " + Condition, statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneDateTimeFieldAndConditionParts_ReturnsValidUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(),
            columnName = RandomValueGen.GetRandomString(),
            FieldValue = DateTime.Now,
            Condition = RandomValueGen.GetRandomString(),
            ConditionVal = RandomValueGen.GetRandomString()

        Dim dfi = New DateTimeFormatInfo()
        dfi.DateSeparator = "/"
        dfi.TimeSeparator = ":"
        Dim valueString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", dfi)
        Dim statement = UpdateStatementBuilder.Create() _
                        .WithTable(tableName) _
                        .WithField(columnName, FieldValue) _
                        .WithCondition(Condition, DatabaseHelpers.Condition.EqualityOperators.Equals, ConditionVal) _
                        .Build()
        Assert.AreEqual("update [" + tableName + "] set [" + columnName + "]='" + valueString + "' where [" + Condition + "]='" + ConditionVal + "'", statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneDateTimeFieldAndConditionPartsWithBooleanCondition_ReturnsValidUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(),
            columnName = RandomValueGen.GetRandomString(),
            FieldValue = DateTime.Now,
            c1 = RandomValueGen.GetRandomString(),
            ConditionVal = RandomValueGen.GetRandomBoolean(),
            expected = CInt(IIf(ConditionVal, 1, 0))

        Dim dfi = New DateTimeFormatInfo()
        dfi.DateSeparator = "/"
        dfi.TimeSeparator = ":"
        Dim valueString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", dfi)
        Dim statement = UpdateStatementBuilder.Create() _
                        .WithTable(tableName) _
                        .WithField(columnName, FieldValue) _
                        .WithCondition(c1, Condition.EqualityOperators.Equals, ConditionVal) _
                        .Build()
        Assert.AreEqual("update [" + tableName + "] set [" + columnName + "]='" + valueString + "' where [" + c1 + "]=" + expected.ToString(), statement)
    End Sub

    <Test()>
    Public Sub Build_GivenTableAndOneNullableNullDateTimeFieldAndCondition_ReturnsValidUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(),
            columnName = RandomValueGen.GetRandomString(),
            FieldValue As Nullable(Of DateTime) = Nothing,
            Condition = RandomValueGen.GetRandomString()
        Dim valueString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        Dim statement = UpdateStatementBuilder.Create() _
                        .WithTable(tableName) _
                        .WithField(columnName, FieldValue) _
                        .WithCondition(Condition) _
                        .Build()
        Assert.AreEqual("update [" + tableName + "] set [" + columnName + "]=NULL where " + Condition, statement)
    End Sub

    <Test()>
    Public Sub WithFieldCopy_WhenSrcAndDestProvided_BuildReturnsExpectedUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(1),
            srcCol = RandomValueGen.GetRandomString(1),
            dstCol = RandomValueGen.GetRandomString(1),
            condition = RandomValueGen.GetRandomString(1)
        Dim statement = UpdateStatementBuilder.Create() _
                        .WithTable(tableName) _
                        .WithFieldCopy(srcCol, dstCol) _
                        .WithCondition(condition) _
                        .Build()
        Assert.AreEqual("update [" + tableName + "] set [" + dstCol + "]=[" + srcCol + "] where " + condition, statement)
    End Sub

    <Test()>
    Public Sub WithFieldCopyAndFBProvider_WhenSrcAndDestProvided_BuildReturnsExpectedUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(1),
            srcCol = RandomValueGen.GetRandomString(1),
            dstCol = RandomValueGen.GetRandomString(1),
            condition = RandomValueGen.GetRandomString(1)
        Dim statement = UpdateStatementBuilder.Create() _
                        .WithDatabaseProvider(DatabaseProviders.Firebird) _
                        .WithTable(tableName) _
                        .WithFieldCopy(srcCol, dstCol) _
                        .WithCondition(condition) _
                        .Build()
        Assert.AreEqual("update """ + tableName + """ set """ + dstCol + """=""" + srcCol + """ where " + condition, statement)
    End Sub

    <Test()>
    Public Sub WithCondition_GivenCondition_BuildReturnsExpectedUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(),
            col = RandomValueGen.GetRandomString(),
            val = RandomValueGen.GetRandomString(),
            condition = Substitute.For(Of ICondition)()
        Dim fakeCondition = RandomValueGen.GetRandomString()
        condition.ToString().Returns(fakeCondition)
        Dim statement = UpdateStatementBuilder.Create() _
                        .WithTable(tableName) _
                        .WithField(col, val) _
                        .WithCondition(condition) _
                        .Build()
        Assert.AreEqual("update [" + tableName + "] set [" + col + "]='" + val + "' where " + fakeCondition, statement)
    End Sub

    <Test()>
    Public Sub WithAllConditions_GivenConditions_BuildReturnsExpectedUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(),
            col = RandomValueGen.GetRandomString(),
            val = RandomValueGen.GetRandomString()
        Dim conditions = New List(Of ICondition)()
        Dim conditionCount = RandomValueGen.GetRandomInt(2, 5)
        For i = 0 To conditionCount
            Dim c = Substitute.For(Of ICondition)()
            c.ToString().Returns(RandomValueGen.GetRandomString())
            conditions.Add(c)
        Next
        Dim statement = UpdateStatementBuilder.Create() _
                            .WithTable(tableName) _
                            .WithField(col, val) _
                            .WithAllConditions(conditions.ToArray()) _
                            .Build()
        Dim cstring = String.Join(" and ", conditions.Select(Function(c)
                                                                 Return c.ToString()
                                                             End Function))
        Assert.AreEqual("update [" + tableName + "] set [" + col + "]='" + val + "' where (" + cstring + ")", statement)
    End Sub

    <Test()>
    Public Sub WithAllConditionsAndFirebirdProvider_GivenConditions_BuildReturnsExpectedUpdateStatement()
        Dim tableName = RandomValueGen.GetRandomString(),
            col = RandomValueGen.GetRandomString(),
            val = RandomValueGen.GetRandomString()
        Dim conditions = New List(Of ICondition)()
        Dim conditionCount = RandomValueGen.GetRandomInt(2, 5)
        For i = 0 To conditionCount
            Dim c = Substitute.For(Of ICondition)()
            c.ToString().Returns(RandomValueGen.GetRandomString())
            conditions.Add(c)
        Next
        Dim statement = UpdateStatementBuilder.Create() _
                            .WithDatabaseProvider(DatabaseProviders.Firebird) _
                            .WithTable(tableName) _
                            .WithField(col, val) _
                            .WithAllConditions(conditions.ToArray()) _
                            .Build()
        Dim cstring = String.Join(" and ", conditions.Select(Function(c)
                                                                 Return c.ToString()
                                                             End Function))
        Assert.AreEqual("update """ + tableName + """ set """ + col + """='" + val + "' where (" + cstring + ")", statement)
        for i = 0 to conditionCount
            conditions(i).Received().UseDatabaseProvider(DatabaseProviders.Firebird)
        Next
    End Sub

    <Test()>
    Public SUb WithCondition_WhenCalledTwice_Should_AND_conditionsTogether()
        Dim table = RandomValueGen.GetRandomString(2),
            cfld1 = RandomValueGen.GetRandomString(2),
            cfld2 = RandomValueGen.GetRandomString(3),
            cval1 = RandomValueGen.GetRandomInt(),
            cval2 = RandomValueGen.GetRandomString(2),
            target = RandomValueGen.GetRandomString(2),
            val = RandomValueGen.GetRandomString(2)
        DIm sql = UpdateStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(target, val) _
                    .WithCondition(cfld1, Condition.EqualityOperators.GreaterThan, cval1) _
                    .WithCondition(cfld2, Condition.EqualityOperators.Equals, cval2) _
                    .Build()
        Dim expected = "update [" + table + "] set [" + target  + "]='" + val + "' where ([" + cfld1 + "]>" + cval1.ToString() _
                        + " and [" + cfld2 + "]='" + cval2 + "')"
        Assert.AreEqual(expected, sql)
    End SUb


End Class
