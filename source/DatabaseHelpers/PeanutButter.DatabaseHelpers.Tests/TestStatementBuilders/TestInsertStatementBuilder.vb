Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.Utils
Imports PeanutButter.RandomGenerators.RandomValueGen
Imports NExpect
Imports NExpect.Expectations

Namespace TestStatementBuilders
    <TestFixture()>
    Public Class TestInsertStatementBuilder
        <Test()>
        Public Sub Create_ShouldReturnNewInstanceOfInsertStatementBuilder()
            Dim builder1 = InsertStatementBuilder.Create()
            Dim builder2 = InsertStatementBuilder.Create()
            Expect(builder1) _
                .Not.To.Equal(builder2)
            Expect(builder1) _
                .To.Be.An.Instance.Of (Of InsertStatementBuilder)
            Expect(builder2) _
                .To.Be.An.Instance.Of (Of InsertStatementBuilder)
        End Sub

        Private Function Create() As IInsertStatementBuilder
            Return InsertStatementBuilder.Create()
        End Function

        <Test()>
        Public Sub Build_GivenNoTableName_ThrowsException()
            Assert.That(
                (Function() as String
                    return Create().Build()
                End Function),
                Throws.Exception.InstanceOf (Of ArgumentException).With.Message.Contains("no table specified")
                )
        End Sub

        <Test()>
        Public Sub Build_GivenNoFields_ThrowsException()
            Assert.That(
                (Function() as String
                    return Create() _
                           .WithTable(GetRandomString(1)) _
                           .Build()
                End Function),
                Throws.Exception.InstanceOf (Of ArgumentException).With.Message.Contains("no fields specified")
                )
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleStringField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue = GetRandomString(1)
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + tableName + "] ([" + fieldName + "]) values ('" + fieldValue + "')")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleStringFieldAndFirebirdProvider_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue = GetRandomString(1)
            Dim result = Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into """ + tableName + """ (""" + fieldName + """) values ('" + fieldValue + "')")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullStringField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue = vbNullString
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)")
        End Sub


        <Test()>
        Public Sub Build_GivenTableNameAndSingleIntegerField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue = GetRandomInt()
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + tableName + "] ([" + fieldName + "]) values (" + fieldValue.ToString() + ")")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleDecimalField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue As Decimal = GetRandomDecimal()
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Dim decimalDecorator = New DecimalDecorator(fieldValue, "0.00")
            Assert.That(decimalDecorator.ToString(), Does.Not.Contain(","))
            Expect(result) _
                .To.Equal(
                    "insert into [" + tableName + "] ([" + fieldName + "]) values (" + decimalDecorator.ToString() + ")")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullableDecimalField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue As Nullable(Of Decimal) = GetRandomDecimal()
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Dim decimalDecorator = New DecimalDecorator(fieldValue.Value, "0.00")
            Assert.That(decimalDecorator.ToString(), Does.Not.Contain(","))
            Expect(result) _
                .To.Equal(
                    "insert into [" + tableName + "] ([" + fieldName + "]) values (" + decimalDecorator.ToString() + ")")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullIntegerField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue As Nullable(Of Integer) = Nothing
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullDecimalField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue As Nullable(Of Decimal) = Nothing
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleDateTimeField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue = DateTime.Now
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal(
                    "insert into [" + tableName + "] ([" + fieldName + "]) values ('" +
                    fieldValue.ToString("yyyy-MM-dd HH:mm:ss") + "')")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullDateTimeField_ReturnsExpectedString()
            Dim tableName = GetRandomString(1),
                fieldName = GetRandomString(1),
                fieldValue As Nullable(Of DateTime) = Nothing
            Dim result = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)")
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndMultipleFields_ReturnsExpectedString()
            Dim t = GetRandomString(),
                f1 = GetRandomString(),
                v1 = GetRandomString(),
                f2 = GetRandomString(),
                v2 = GetRandomString()
            Dim result = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithField(f2, v2) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + t + "] ([" + f1 + "],[" + f2 + "]) values ('" + v1 + "','" + v2 + "')")
        End Sub

        <TestCase(Nothing)>
        <TestCase("")>
        <TestCase(vbTab)>
        <TestCase(" ")>
        Public Sub WithNonBlankField_OnlyAddsFieldWhenNotWhitespaceOrNull(val As String)
            ' setup
            Dim t = GetRandomString(),
                f1 = GetRandomString(),
                v1 = GetRandomString(),
                f2 = GetRandomString()
            ' assert pre-conditions
            ' perform test
            Dim result = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithNonBlankField(f2, val) _
                    .Build()
            ' assert test results
            Expect(result) _
                .To.Equal("insert into [" + t + "] ([" + f1 + "]) values ('" + v1 + "')")
        End Sub

        <Test()>
        Public Sub WithConditionalField_WhenConditionalIsTrue_IncludesTrueField()
            Dim t = GetRandomString(),
                f1 = GetRandomString(),
                v1 = GetRandomString(),
                f2 = GetRandomString(),
                v2 = GetRandomString()
            Dim result = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(True, f2, v2) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + t + "] ([" + f1 + "],[" + f2 + "]) values ('" + v1 + "','" + v2 + "')")
        End Sub

        <Test()>
        Public Sub WithConditionalField_WhenConditionalIsFalseAndFalseFieldOmitted_SkipsField()
            Dim t = GetRandomString(),
                f1 = GetRandomString(),
                v1 = GetRandomString(),
                f2 = GetRandomString(),
                v2 = GetRandomString()
            Dim result = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(False, f2, v2) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + t + "] ([" + f1 + "]) values ('" + v1 + "')")
        End Sub

        <TestCase(DatabaseProviders.Access)>
        <TestCase(DatabaseProviders.SQLServer)>
        <TestCase(DatabaseProviders.SQLite)>
        Public Sub WithConditionalField_WHenConditionalIsFalseAndFalseFieldIsNotNothing_IncludesFalseField(
                                                                                                           provider As _
                                                                                                              DatabaseProviders)
            Dim t = GetRandomString(),
                f1 = GetRandomString(),
                v1 = GetRandomString(),
                f2 = GetRandomString(),
                v2 = GetRandomString(),
                v3 = GetRandomString()
            Dim result = Create().WithDatabaseProvider(provider) _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(False, f2, v2, v3) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into [" + t + "] ([" + f1 + "],[" + f2 + "]) values ('" + v1 + "','" + v3 + "')")
        End Sub

        <Test()>
        Public Sub Firebird_WithConditionalField_WHenConditionalIsFalseAndFalseFieldIsNotNothing_IncludesFalseField()
            Dim t = GetRandomString(),
                f1 = GetRandomString(),
                v1 = GetRandomString(),
                f2 = GetRandomString(),
                v2 = GetRandomString(),
                v3 = GetRandomString()
            Dim result = Create().WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(False, f2, v2, v3) _
                    .Build()
            Expect(result) _
                .To.Equal("insert into """ + t + """ (""" + f1 + """,""" + f2 + """) values ('" + v1 + "','" + v3 + "')")
        End Sub

        <Test()>
        Public Sub Sub_WithField_GivenStringValueWhichIsNull_ShouldAttemptToInsertNull()
            Dim t = GetRandomString(),
                f = GetRandomString(),
                v = CType(Nothing, String)
            Dim result = Create().WithTable(t).WithField(f, v).Build()
            Expect(result) _
                .To.Equal("insert into [" + t + "] ([" + f + "]) values (NULL)")
        End Sub

        <Test()>
        Public Sub Sub_WithField_GivenStringValueWhichIsEmpty_ShouldNOTAttemptToInsertNull()
            Dim t = GetRandomString(),
                f = GetRandomString(),
                v = ""
            Dim result = Create().WithTable(t).WithField(f, v).Build()
            Expect(result) _
                .To.Equal("insert into [" + t + "] ([" + f + "]) values ('')")
        End Sub
    End Class
End NameSpace