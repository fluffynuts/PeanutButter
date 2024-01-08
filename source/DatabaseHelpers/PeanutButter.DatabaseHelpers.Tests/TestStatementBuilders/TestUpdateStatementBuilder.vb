Imports System.Globalization
Imports NSubstitute
Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators.RandomValueGen
Imports PeanutButter.Utils
Imports NExpect
Imports NExpect.Expectations

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestUpdateStatementBuilder
        <Test()>
        Public Sub Create_ShouldReturnNewInstanceOfUpdateStatementBuilder()
            Dim builder = UpdateStatementBuilder.Create()
            Expect(builder) _
                .To.Be.An.Instance.Of(Of UpdateStatementBuilder)
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
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue = GetRandomString(),
                condition = GetRandomString()
            Dim expected = "update [" & tableName & "] set [" & columnName & "]='" & fieldValue & "' where " & condition
            Dim builder = UpdateStatementBuilder.Create()
            With builder
                .WithTable(tableName)
                .WithField(columnName, fieldValue)
                .WithCondition(condition)
            End With
            Dim result = builder.Build()
            Expect(result).To.Equal(expected)
        End Sub
        <Test()>
        Public Sub Build_GivenTableAndOneDecimalFieldAndCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue = GetRandomDecimal(),
                condition = GetRandomString()
            dim dd = new DecimalDecorator(fieldValue)
            Dim expected = "update [" & tableName & "] set [" & columnName & "]=" & dd.ToString() & " where " & condition
            Dim builder = UpdateStatementBuilder.Create()
            With builder
                .WithTable(tableName)
                .WithField(columnName, fieldValue)
                .WithCondition(condition)
            End With
            Dim result = builder.Build()
            Dim nfi = New NumberFormatInfo()
            nfi.NumberDecimalSeparator = "."
            nfi.CurrencyDecimalSeparator = "."
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneNullableDecimalFieldAndCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue as Decimal? = GetRandomDecimal(),
                condition = GetRandomString()
            Dim dd = new DecimalDecorator(fieldValue.Value)
            Dim expected = "update [" & tableName & "] set [" & columnName & "]=" & dd.ToString() & " where " & condition
            Dim builder = UpdateStatementBuilder.Create()
            With builder
                .WithTable(tableName)
                .WithField(columnName, fieldValue)
                .WithCondition(condition)
            End With
            Dim result = builder.Build()
            Dim nfi = New NumberFormatInfo()
            nfi.NumberDecimalSeparator = "."
            nfi.CurrencyDecimalSeparator = "."
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneNullableDecimalFieldAndDecimalCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue as Decimal? = GetRandomDecimal(),
                thisCondition = new Condition("condition_field", Condition.EqualityOperators.Equals, FieldValue.Value)
            Dim builder = UpdateStatementBuilder.Create()
            Dim dd = new DecimalDecorator(FieldValue.Value)
            Dim expected = "update [" & tableName & "] set [" & ColumnName & "]=" & dd.ToString() & " where " & thisCondition.ToString()
            With builder
                .WithTable(tableName)
                .WithField(ColumnName, FieldValue)
                .WithCondition(thisCondition)
            End With
            Dim result = builder.Build()
            Dim nfi = New NumberFormatInfo()
            nfi.NumberDecimalSeparator = "."
            nfi.CurrencyDecimalSeparator = "."
            Assert.That(thisCondition.ToString(), Does.Not.Contain(","))
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneNullableDecimalFieldAndNullableDecimalCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue as Decimal? = GetRandomDecimal(),
                thisCondition = new Condition("condition_field", Condition.EqualityOperators.Equals, FieldValue)
            Dim dd = new DecimalDecorator(FieldValue.Value)
            Dim expected = "update [" & TableName & "] set [" & ColumnName & "]=" & dd.ToString() & " where " & thisCondition.ToString()
            Dim builder = UpdateStatementBuilder.Create()
            With builder
                .WithTable(TableName)
                .WithField(ColumnName, FieldValue)
                .WithCondition(thisCondition)
            End With
            Dim result = builder.Build()
            Dim nfi = New NumberFormatInfo()
            nfi.NumberDecimalSeparator = "."
            nfi.CurrencyDecimalSeparator = "."
            
            Assert.That(thisCondition.ToString(), Does.Not.Contain(","))
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneNullableNullDecimalFieldAndCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue As Decimal? = Nothing,
                condition = GetRandomString()
            Dim expected = "update [" & TableName & "] set [" & ColumnName & "]=NULL where " & Condition
            Dim builder = UpdateStatementBuilder.Create()
            With builder
                .WithTable(TableName)
                .WithField(ColumnName, FieldValue)
                .WithCondition(Condition)
            End With
            Dim result = builder.Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneDateTimeFieldAndCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue = DateTime.Now,
                condition = GetRandomString()
            Dim dfi = New DateTimeFormatInfo()
            Dim valueString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", dfi)
            Dim expected = "update [" + tableName + "] set [" + columnName + "]='" + valueString + "' where " + Condition
            dfi.DateSeparator = "/"
            dfi.TimeSeparator = ":"
            Dim result = UpdateStatementBuilder.Create() _
                    .WithTable(tableName) _
                    .WithField(columnName, FieldValue) _
                    .WithCondition(Condition) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneDateTimeFieldAndConditionParts_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue = DateTime.Now,
                condition = GetRandomString(),
                conditionVal = GetRandomString()
            Dim dfi = New DateTimeFormatInfo()
            dfi.DateSeparator = "/"
            dfi.TimeSeparator = ":"
            Dim valueString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", dfi)
            Dim expected = "update [" + tableName + "] set [" + columnName + "]='" + valueString + "' where [" + Condition + "]='" + ConditionVal + "'"
            Dim result = UpdateStatementBuilder.Create() _
                    .WithTable(tableName) _
                    .WithField(columnName, FieldValue) _
                    .WithCondition(Condition, StatementBuilders.Condition.EqualityOperators.Equals, ConditionVal) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneDateTimeFieldAndConditionPartsWithBooleanCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue = DateTime.Now,
                c1 = GetRandomString(),
                conditionVal = GetRandomBoolean(),
                expectedValue = CInt(IIf(ConditionVal, 1, 0))
            Dim dfi = New DateTimeFormatInfo()
            dfi.DateSeparator = "/"
            dfi.TimeSeparator = ":"
            Dim valueString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", dfi)
            Dim expected = "update [" + tableName + "] set [" + columnName + "]='" + valueString + "' where [" + c1 + "]=" + expectedValue.ToString()

            Dim result = UpdateStatementBuilder.Create() _
                    .WithTable(tableName) _
                    .WithField(columnName, FieldValue) _
                    .WithCondition(c1, Condition.EqualityOperators.Equals, ConditionVal) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneNullableNullDateTimeFieldAndCondition_ReturnsValidUpdateStatement()
            Dim tableName = GetRandomString(),
                columnName = GetRandomString(),
                fieldValue As Date? = Nothing,
                condition = GetRandomString()
            Dim expected = "update [" + tableName + "] set [" + columnName + "]=NULL where " + condition
            Dim result = UpdateStatementBuilder.Create() _
                    .WithTable(tableName) _
                    .WithField(columnName, fieldValue) _
                    .WithCondition(condition) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithFieldCopy_WhenSrcAndDestProvided_BuildReturnsExpectedUpdateStatement()
            Dim tableName = GetRandomString(1),
                srcCol = GetRandomString(1),
                dstCol = GetRandomString(1),
                condition = GetRandomString(1)
            Dim expected = "update [" + tableName + "] set [" + dstCol + "]=[" + srcCol + "] where " + condition
            Dim result = UpdateStatementBuilder.Create() _
                    .WithTable(tableName) _
                    .WithFieldCopy(srcCol, dstCol) _
                    .WithCondition(condition) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithFieldCopyAndFBProvider_WhenSrcAndDestProvided_BuildReturnsExpectedUpdateStatement()
            Dim tableName = GetRandomString(1),
                srcCol = GetRandomString(1),
                dstCol = GetRandomString(1),
                condition = GetRandomString(1)
            Dim expected = "update """ + tableName + """ set """ + dstCol + """=""" + srcCol + """ where " + condition
            Dim result = UpdateStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(tableName) _
                    .WithFieldCopy(srcCol, dstCol) _
                    .WithCondition(condition) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithCondition_GivenCondition_BuildReturnsExpectedUpdateStatement()
            Dim tableName = GetRandomString(),
                col = GetRandomString(),
                val = GetRandomString(),
                condition = Substitute.For(Of ICondition)()
            Dim fakeCondition = GetRandomString()
            condition.ToString().Returns(fakeCondition)
            Dim expected = "update [" + tableName + "] set [" + col + "]='" + val + "' where " + fakeCondition
            Dim result = UpdateStatementBuilder.Create() _
                    .WithTable(tableName) _
                    .WithField(col, val) _
                    .WithCondition(condition) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithAllConditions_GivenConditions_BuildReturnsExpectedUpdateStatement()
            Dim tableName = GetRandomString(),
                col = GetRandomString(),
                val = GetRandomString()
            Dim conditions = New List(Of ICondition)()
            Dim conditionCount = GetRandomInt(2, 5)
            For i = 0 To conditionCount
                Dim c = Substitute.For(Of ICondition)()
                c.ToString().Returns(GetRandomString())
                conditions.Add(c)
            Next
            Dim cstring = String.Join(" and ", conditions.Select(Function(c)
                Return c.ToString()
            End Function))
            Dim expected = "update [" + tableName + "] set [" + col + "]='" + val + "' where (" + cstring + ")"
            Dim result = UpdateStatementBuilder.Create() _
                    .WithTable(tableName) _
                    .WithField(col, val) _
                    .WithAllConditions(conditions.ToArray()) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithAllConditionsAndFirebirdProvider_GivenConditions_BuildReturnsExpectedUpdateStatement()
            Dim tableName = GetRandomString(),
                col = GetRandomString(),
                val = GetRandomString()
            Dim conditions = New List(Of ICondition)()
            Dim conditionCount = GetRandomInt(2, 5)
            For i = 0 To conditionCount
                Dim c = Substitute.For(Of ICondition)()
                c.ToString().Returns(GetRandomString())
                conditions.Add(c)
            Next
            Dim result = UpdateStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(tableName) _
                    .WithField(col, val) _
                    .WithAllConditions(conditions.ToArray()) _
                    .Build()
            Dim cstring = String.Join(" and ", conditions.Select(Function(c)
                Return c.ToString()
                                                                    End Function))
            Dim expected = "update """ + tableName + """ set """ + col + """='" + val + "' where (" + cstring + ")"
            Expect(result).To.Equal(expected)
            for i = 0 to conditionCount
                conditions(i).Received().UseDatabaseProvider(DatabaseProviders.Firebird)
            Next
        End Sub

        <Test()>
        Public SUb WithCondition_WhenCalledTwice_Should_AND_conditionsTogether()
            Dim table = GetRandomString(2),
                cfld1 = GetRandomString(2),
                cfld2 = GetRandomString(3),
                cval1 = GetRandomInt(),
                cval2 = GetRandomString(2),
                target = GetRandomString(2),
                val = GetRandomString(2)
            DIm result = UpdateStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(target, val) _
                    .WithCondition(cfld1, Condition.EqualityOperators.GreaterThan, cval1) _
                    .WithCondition(cfld2, Condition.EqualityOperators.Equals, cval2) _
                    .Build()
            Dim expected = "update [" + table + "] set [" + target  + "]='" + val + "' where ([" + cfld1 + "]>" + cval1.ToString() _
                           + " and [" + cfld2 + "]='" + cval2 + "')"
            Expect(result).To.Equal(expected)
        End SUb


    End Class
End NameSpace