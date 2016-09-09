Imports System.Reflection
Imports NUnit.Framework
Imports PeanutButter.RandomGenerators
Imports PeanutButter.Utils

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestInsertStatementBuilder
        <Test()>
        Public Sub Create_ShouldReturnNewInstanceOfInsertStatementBuilder()
            Dim builder1 = InsertStatementBuilder.Create()
            Dim builder2 = InsertStatementBuilder.Create()
            Assert.AreNotEqual(builder1, builder2)
            Assert.IsInstanceOf(Of InsertStatementBuilder)(builder1)
            Assert.IsInstanceOf(Of InsertStatementBuilder)(builder2)
        End Sub

        Private Function Create() As IInsertStatementBuilder
            Return InsertStatementBuilder.Create()
        End Function

        <Test()>
        Public Sub Build_GivenNoTableName_ThrowsException()
            Dim ex = Assert.Throws(Of ArgumentException)(Function() as String
                Return Create().Build()
                                                            End Function)
            StringAssert.Contains("no table specified", ex.Message)
        End Sub
        <Test()>
        Public Sub Build_GivenNoFields_ThrowsException()
            Dim ex = Assert.Throws(Of ArgumentException)(Function() as String
                Return Create() _
                                                            .WithTable(RandomValueGen.GetRandomString(1)) _
                                                            .Build()
                                                            End Function)
            StringAssert.Contains("no fields specified", ex.Message)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleStringField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue = RandomValueGen.GetRandomString(1)
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values ('" + fieldValue + "')", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleStringFieldAndFirebirdProvider_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue = RandomValueGen.GetRandomString(1)
            Dim sql = Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into """ + tableName + """ (""" + fieldName + """) values ('" + fieldValue + "')", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullStringField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue = vbNullString
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)", sql)
        End Sub


        <Test()>
        Public Sub Build_GivenTableNameAndSingleIntegerField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue = RandomValueGen.GetRandomInt()
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values (" + fieldValue.ToString() + ")", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleDecimalField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue As Decimal = RandomValueGen.GetRandomDecimal()
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Dim decimalDecorator  = New DecimalDecorator(fieldValue, "0.00")
            Assert.IsFalse(decimalDecorator.ToString().Contains(","))
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values (" + decimalDecorator.ToString() + ")", sql)
        End Sub
        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullableDecimalField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue As Nullable(Of Decimal) = RandomValueGen.GetRandomDecimal()
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Dim decimalDecorator  = New DecimalDecorator(fieldValue.Value, "0.00")
            Assert.IsFalse(decimalDecorator.ToString().Contains(","))
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values (" + decimalDecorator.ToString() + ")", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullIntegerField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue As Nullable(Of Integer) = Nothing
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullDecimalField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue As Nullable(Of Decimal) = Nothing
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleDateTimeField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue = DateTime.Now
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values ('" + fieldValue.ToString("yyyy-MM-dd HH:mm:ss") + "')", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndSingleNullDateTimeField_ReturnsExpectedString()
            Dim tableName = RandomValueGen.GetRandomString(1),
                fieldName = RandomValueGen.GetRandomString(1),
                fieldValue As Nullable(Of DateTime) = Nothing
            Dim sql = Create() _
                    .WithTable(tableName) _
                    .WithField(fieldName, fieldValue) _
                    .Build()
            Assert.AreEqual("insert into [" + tableName + "] ([" + fieldName + "]) values (NULL)", sql)
        End Sub

        <Test()>
        Public Sub Build_GivenTableNameAndMultipleFields_ReturnsExpectedString()
            Dim t = RandomValueGen.GetRandomString(),
                f1 = RandomValueGen.GetRandomString(),
                v1 = RandomValueGen.GetRandomString(),
                f2 = RandomValueGen.GetRandomString(),
                v2 = RandomValueGen.GetRandomString()
            Dim sql = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithField(f2, v2) _
                    .Build()
            Assert.AreEqual("insert into [" + t + "] ([" + f1 + "],[" + f2 + "]) values ('" + v1 + "','" + v2 + "')", sql)
        End Sub

        <TestCase(Nothing)>
        <TestCase("")>
        <TestCase(vbTab)>
        <TestCase(" ")>
        Public Sub WithNonBlankField_OnlyAddsFieldWhenNotWhitespaceOrNull(val As String)
            ' setup
            Dim t = RandomValueGen.GetRandomString(),
                f1 = RandomValueGen.GetRandomString(),
                v1 = RandomValueGen.GetRandomString(),
                f2 = RandomValueGen.GetRandomString()
            ' assert pre-conditions
            ' perform test
            Dim sql = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithNonBlankField(f2, val) _
                    .Build()
            ' assert test results
            Assert.AreEqual("insert into [" + t + "] ([" + f1 + "]) values ('" + v1 + "')", sql)
        End Sub

        <Test()>
        Public Sub WithConditionalField_WhenConditionalIsTrue_IncludesTrueField()
            Dim t = RandomValueGen.GetRandomString(),
                f1 = RandomValueGen.GetRandomString(),
                v1 = RandomValueGen.GetRandomString(),
                f2 = RandomValueGen.GetRandomString(),
                v2 = RandomValueGen.GetRandomString()
            Dim sql = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(True, f2, v2) _
                    .Build()
            Assert.AreEqual("insert into [" + t + "] ([" + f1 + "],[" + f2 + "]) values ('" + v1 + "','" + v2 + "')", sql)
        End Sub

        <Test()>
        Public Sub WithConditionalField_WhenConditionalIsFalseAndFalseFieldOmitted_SkipsField()
            Dim t = RandomValueGen.GetRandomString(),
                f1 = RandomValueGen.GetRandomString(),
                v1 = RandomValueGen.GetRandomString(),
                f2 = RandomValueGen.GetRandomString(),
                v2 = RandomValueGen.GetRandomString()
            Dim sql = Create() _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(False, f2, v2) _
                    .Build()
            Assert.AreEqual("insert into [" + t + "] ([" + f1 + "]) values ('" + v1 + "')", sql)
        End Sub

        <TestCase(DatabaseProviders.Access)>
        <TestCase(DatabaseProviders.SQLServer)>
        <TestCase(DatabaseProviders.SQLite)>
        Public Sub WithConditionalField_WHenConditionalIsFalseAndFalseFieldIsNotNothing_IncludesFalseField(provider As DatabaseProviders)
            Dim t = RandomValueGen.GetRandomString(),
                f1 = RandomValueGen.GetRandomString(),
                v1 = RandomValueGen.GetRandomString(),
                f2 = RandomValueGen.GetRandomString(),
                v2 = RandomValueGen.GetRandomString(),
                v3 = RandomValueGen.GetRandomString()
            Dim sql = Create().WithDatabaseProvider(provider) _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(False, f2, v2, v3) _
                    .Build()
            Assert.AreEqual("insert into [" + t + "] ([" + f1 + "],[" + f2 + "]) values ('" + v1 + "','" + v3 + "')", sql)
        End Sub
        <Test()>
        Public Sub Firebird_WithConditionalField_WHenConditionalIsFalseAndFalseFieldIsNotNothing_IncludesFalseField()
            Dim t = RandomValueGen.GetRandomString(),
                f1 = RandomValueGen.GetRandomString(),
                v1 = RandomValueGen.GetRandomString(),
                f2 = RandomValueGen.GetRandomString(),
                v2 = RandomValueGen.GetRandomString(),
                v3 = RandomValueGen.GetRandomString()
            Dim sql = Create().WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(t) _
                    .WithField(f1, v1) _
                    .WithConditionalField(False, f2, v2, v3) _
                    .Build()
            Assert.AreEqual("insert into """ + t + """ (""" + f1 + """,""" + f2 + """) values ('" + v1 + "','" + v3 + "')", sql)
        End Sub

        <Test()>
        Public Sub Sub_WithField_GivenStringValueWhichIsNull_ShouldAttemptToInsertNull()
            Dim t = RandomValueGen.GetRandomString(),
                f = RandomValueGen.GetRandomString(),
                v = CType(Nothing, String)
            Dim sql = Create().WithTable(t).WithField(f, v).Build()
            Assert.AreEqual("insert into [" + t + "] ([" + f + "]) values (NULL)", sql)
        End Sub

        <Test()>
        Public Sub Sub_WithField_GivenStringValueWhichIsEmpty_ShouldNOTAttemptToInsertNull()
            Dim t = RandomValueGen.GetRandomString(),
                f = RandomValueGen.GetRandomString(),
                v = ""
            Dim sql = Create().WithTable(t).WithField(f, v).Build()
            Assert.AreEqual("insert into [" + t + "] ([" + f + "]) values ('')", sql)
        End Sub


    End Class
End NameSpace