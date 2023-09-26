Imports System.Data.Common
Imports NSubstitute
Imports PeanutButter.DatabaseHelpers.Executors
Imports PeanutButter.DatabaseHelpers.StatementBuilders

' ReSharper disable UnusedMember.Global
' ReSharper disable MemberCanBePrivate.Global
Public Class BuilderFaker
    Public Shared Function CreateFakeSelectStatementBuilder() as ISelectStatementBuilder
        return CreateFakeSelectStatementBuilder(Nothing)
    End Function
    Public Shared Function CreateFakeSelectStatementBuilder(withBuildSql As String) As ISelectStatementBuilder
        Dim builder = Substitute.For(Of ISelectStatementBuilder)()
        builder.Distinct().Returns(builder)
        builder.WithTable(Arg.Any(Of String)).Returns(builder)
        builder.WithField(Arg.Any(Of String), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of SelectField)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of DateTime)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Int16)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Int32)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Int64)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Long)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of String)).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Decimal)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Int16)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Int32)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Int64)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Long)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Decimal)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Double)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of DateTime)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of SelectField)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of SelectField)()).Returns(builder)
        builder.WithInnerJoin(Arg.Any(Of String)(), Arg.Any(Of String)(), Arg.Any(Of String)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithComputedField(Arg.Any(Of String)(), Arg.Any(Of ComputedField.ComputeFunctions)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithAllFieldsFrom(Arg.Any(Of String)()).Returns(builder)
        builder.WithAllConditions(Arg.Any(Of Condition)()).Returns(builder)
        builder.WithAllConditions(Arg.Any(Of Condition)(), Arg.Any(Of Condition)()).Returns(builder)
        If withBuildSql IsNot Nothing Then
            builder.Build.Returns(withBuildSql)
        End If
        Return builder
    End Function

    Public Shared Function CreateFakeUpdateStatementBuilder() as IUpdateStatementBuilder
        Return CreateFakeUpdateStatementBuilder(Nothing)
    End Function

    Public Shared Function CreateFakeUpdateStatementBuilder(withBuildSql As String) As IUpdateStatementBuilder
        Dim builder = Substitute.For(Of IUpdateStatementBuilder)()
        builder.WithTable(Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Decimal)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Nullable(Of Decimal))(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of DateTime)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Nullable(Of DateTime))(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Int64)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Int32)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Int16)()).Returns(builder)
        builder.WithNullField(Arg.Any(Of String)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)(), Arg.Any(Of Condition.EqualityOperators)(), Arg.Any(Of Int32)()).Returns(builder)
        builder.WithCondition(Arg.Any(Of String)).Returns(builder)
        builder.WithAllConditions(Arg.Any(Of ICondition), Arg.Any(Of ICondition)()).Returns(builder)
        If withBuildSql IsNot Nothing Then
            builder.Build.Returns(withBuildSql)
        End If
        Return builder
    End Function

    Public Shared Function CreateFakeInsertStatementBuilder() As IInsertStatementBuilder
        return CreateFakeInsertStatementBuilder(Nothing)
    End Function

    Public Shared Function CreateFakeInsertStatementBuilder(withBuildSql As String) As IInsertStatementBuilder
        Dim builder = Substitute.For(Of IInsertStatementBuilder)()
        builder.WithTable(Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Decimal)(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Nullable(Of Decimal))(), Arg.Any(Of String)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of DateTime)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Nullable(Of DateTime))()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Int64)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Int32)()).Returns(builder)
        builder.WithField(Arg.Any(Of String)(), Arg.Any(Of Int16)()).Returns(builder)
        If withBuildSql IsNot Nothing Then
            builder.Build.Returns(withBuildSql)
        End If
        Return builder
    End Function

    Public Shared Function CreateFakeDataReaderBuilder() As IDataReaderBuilder
        Return CreateFakeDataReaderBuilder(DirectCast(Nothing, DbDataReader))
    End Function

    Public Shared Function CreateFakeDataReaderBuilder(withBuildDataReader As DbDataReader) As IDataReaderBuilder
        Dim builder = Substitute.For(Of IDataReaderBuilder)()
        builder.WithSql(Arg.Any(Of String)()).Returns(builder)
        builder.WithSql(Arg.Any(Of ISelectStatementBuilder)()).Returns(builder)
        builder.WithConnectionFactory(Arg.Any(Of Func(Of IDbConnection))()).Returns(builder)
        If withBuildDataReader Is Nothing Then Return builder
        builder.Build().Returns(withBuildDataReader)
        Return builder
    End Function

    Public Shared Function CreateFakeDataReaderBuilder(ParamArray withReaderReadResult() As Boolean) As IDataReaderBuilder
        Dim reader = Substitute.For(Of DbDataReader)()
        Dim results = New Queue(Of Boolean)
        For Each r In withReaderReadResult
            results.Enqueue(r)
        Next
        Dim alwaysReturn As Boolean? = Nothing
        If results.Count() = 1 Then
            alwaysReturn = results.Dequeue()
        End If
        reader.Read().ReturnsForAnyArgs(Function(args)
                                            If results.Count() = 0 Then
                                                If alwaysReturn.HasValue Then
                                                    Return alwaysReturn.Value
                                                End If
                                                Return False
                                            End If
                                            Return results.Dequeue()
                                        End Function)
        Return CreateFakeDataReaderBuilder(reader)
    End Function

    Public Shared Function CreateFakeScalarExecutorBuilder() As IScalarExecutorBuilder
        Dim builder = Substitute.For(Of IScalarExecutorBuilder)()
        builder.WithConnectionFactory(Arg.Any(Of Func(Of IDbConnection))()).Returns(builder)
        builder.WithSql(Arg.Any(Of String)()).Returns(builder)
        builder.WithSql(Arg.Any(Of IInsertStatementBuilder)()).Returns(builder)
        builder.WithSql(Arg.Any(Of IUpdateStatementBuilder)()).Returns(builder)
        ' TODO: mock out returning something from Execute & ExecuteInsert
        Return builder
    End Function

End Class
' ReSharper restore UnusedMember.Global
' ReSharper restore MemberCanBePrivate.Global
