Imports System.Globalization
Imports PeanutButter.DatabaseHelpers
Imports PeanutButter.Utils

Public Interface IUpdateStatementBuilder
    Inherits IStatementBuilder
    Function WithDatabaseProvider(provider As DatabaseProviders) As IUpdateStatementBuilder
    Function Build() As String
    Function WithField(col As String, val As String, Optional quote As Boolean = True) As IUpdateStatementBuilder
    Function WithField(col As String, val As Decimal, Optional format As String = Nothing) As IUpdateStatementBuilder
    Function WithField(col As String, val As Int64) As IUpdateStatementBuilder
    Function WithField(col As String, val As Nullable(Of Decimal), Optional format As String = Nothing) As IUpdateStatementBuilder
    Function WithField(col As String, val As DateTime, Optional format As String = Nothing) As IUpdateStatementBuilder
    Function WithField(col As String, val As Nullable(Of DateTime), Optional format As String = Nothing) As IUpdateStatementBuilder
    Function WithField(col as String, val as Boolean) as IUpdateStatementBuilder
    Function WithNullField(col As String) As IUpdateStatementBuilder
    Function WithFieldCopy(srcCol As String, dstCol As String) As IUpdateStatementBuilder
    Function WithTable(table As String) As IUpdateStatementBuilder
    Function WithCondition(condition As String) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As String) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int64) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int32) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int16) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Decimal) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Double) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As DateTime) As IUpdateStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Boolean) As IUpdateStatementBuilder
    Function WithCondition(condition As ICondition) As IUpdateStatementBuilder
    Function WithAllConditions(ParamArray conditions() As ICondition) As IUpdateStatementBuilder
    Function ForAllRows() As IUpdateStatementBuilder
End Interface
Public Class UpdateStatementBuilder
    Inherits StatementBuilderBase
    Implements IUpdateStatementBuilder

    Private _table As String
    Private _fields As List(Of FieldWithValue)
    private _condition as ICondition
    Private _forAllRows As Boolean

    Public Shared Function Create() As UpdateStatementBuilder
        Return New UpdateStatementBuilder()
    End Function
    Public Sub New()
        Me._fields = New List(Of FieldWithValue)
    End Sub
    Public Function Build() As String Implements IUpdateStatementBuilder.Build
        CheckBuildParameters()
        Dim sql As New List(Of String)
        sql.Add("update ")
        sql.Add(_openObjectQuote)
        sql.Add(Me._table)
        sql.Add(_closeObjectQuote)
        sql.Add(" set ")
        Me.AddFieldsTo(sql)
        Me.AddConditionTo(sql)
        Return String.Join("", sql)
    End Function

    Private Sub AddFieldsTo(sql As List(Of String))
        Dim fld As FieldWithValue
        For i = 0 To Me._fields.Count - 1
            If i > 0 Then
                sql.Add(", ")
            End If
            fld = Me._fields(i)
            sql.Add(_openObjectQuote)
            sql.Add(fld.Name)
            sql.Add(_closeObjectQuote)
            sql.Add("=")
            If fld.QuoteMe Then
                sql.Add("'")
                sql.Add(fld.Value.Replace("'", "''"))
                sql.Add("'")
            Else
                sql.Add(fld.Value)
            End If
        Next
    End Sub

    Private Sub AddConditionTo(sql As List(Of String))
        If Not _forAllRows Then
            sql.Add(" where ")
            sql.Add(_condition.ToString())
        End If
    End Sub

    Public Function WithFieldCopy(srcField As String, dstField As String) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithFieldCopy
        Me._fields.Add(New FieldWithValue(dstField, _openObjectQuote + srcField + _closeObjectQuote, False))
        Return Me
    End Function
    Public Function WithField(column As String, value As String, Optional quote As Boolean = True) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithField
        Me._fields.Add(New FieldWithValue(column, value, quote))
        Return Me
    End Function
    Public Function WithField(column As String, value As Decimal, Optional format As String = Nothing) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithField
        Me._fields.Add(New FieldWithValue(column, CStr(IIf(format Is Nothing, New DecimalDecorator(value).ToString(), value.ToString(format))), False))
        Return Me
    End Function
    Public Function WithField(column As String, value As Nullable(Of Decimal), Optional format As String = Nothing) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithField
        If (value.HasValue) Then
            Return WithField(column, value.Value, format)
        Else
            Return WithField(column, DirectCast(Nothing, String))
        End If
    End Function
    Public Function WithField(column As String, value As DateTime, Optional format As String = Nothing) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithField
        Me._fields.Add(New FieldWithValue(column, value.ToString(CStr(IIf(format Is Nothing, "yyyy/MM/dd HH:mm:ss", format)), GetSQLDateTimeStringFormatInfo()), True))
        Return Me
    End Function
    Private Function GetSQLDateTimeStringFormatInfo() As DateTimeFormatInfo
        Dim dfi = New DateTimeFormatInfo()
        dfi.DateSeparator = "/"
        dfi.TimeSeparator = ":"
        Return dfi
    End Function
    Public Function WithField(column As String, value As Nullable(Of DateTime), Optional format As String = Nothing) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithField
        If (value.HasValue) Then
            Return WithField(column, value.Value, format)
        Else
            Return WithField(column, DirectCast(Nothing, String))
        End If
    End Function

    Public Function WithTable(name As String) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithTable
        Me._table = name
        Return Me
    End Function
    Public Function WithCondition(condition As String) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        SetOrAnd(new Condition(condition))
        Return Me
    End Function

    Private Function SetOrAnd(condition As ICondition) As IUpdateStatementBuilder
        if _condition Is Nothing Then
            _condition = condition
        Else 
            _condition = _condition.And(condition)
        End If
        return Me
    End Function

    Private Sub CheckBuildParameters()
        If String.IsNullOrEmpty(Me._table) Then
            Throw New ArgumentException(Me.MakeMessage("Table not set"))
        End If
        If Me._fields.Count = 0 Then
            Throw New ArgumentException(Me.MakeMessage("No fields specified"))
        End If
        If _condition Is Nothing And Not _forAllRows Then
            Throw New ArgumentException(Me.MakeMessage("No condition specified"))
        End If
    End Sub
    Private Function MakeMessage(str As String) As String
        Return "UpdateStatementBuilder: " & str
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Date) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        Return Me.WithCondition(New Condition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Decimal) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        Return Me.WithCondition(New Condition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Double) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        Return Me.WithCondition(New Condition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Integer) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        Return Me.WithCondition(New Condition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Long) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        Return Me.WithCondition(New Condition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Short) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        Return Me.WithCondition(New Condition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As String) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        Return Me.WithCondition(New Condition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(condition As ICondition) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        condition.UseDatabaseProvider(_databaseProvider)
        Return SetOrAnd(condition)
    End Function

    Public Function WithAllConditions(ParamArray conditions() As ICondition) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithAllConditions
        Dim conditionChain = New ConditionChain(CompoundCondition.BooleanOperators.OperatorAnd, conditions)
        conditionChain.UseDatabaseProvider(_databaseProvider)
        Return Me.WithCondition(conditionChain)
    End Function

    Public Function WithField(fieldName As String, val As Long) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithField
        Me._fields.Add(New FieldWithValue(fieldName, val.ToString(), False))
        Return Me
    End Function

    Public Function WithNullField(col As String) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithNullField
        Me._fields.Add(New FieldWithValue(col, "NULL", False))
        Return Me
    End Function

    Public Function ForAllRows() As IUpdateStatementBuilder Implements IUpdateStatementBuilder.ForAllRows
        Me._forAllRows = True
        Return Me
    End Function

    Public Function WithField(col As String, val As Boolean) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithField
        Me._fields.Add(new FieldWithValue(col, CInt(IIF(val, 1, 0)).ToString(), false))
        return Me
    End Function

    Public Function WithDatabaseProvider(provider As DatabaseProviders) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithDatabaseProvider
        SetDatabaseProvider(provider)
        return Me
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Boolean) As IUpdateStatementBuilder Implements IUpdateStatementBuilder.WithCondition
        SetOrAnd(new Condition(fieldName, op, fieldValue))
        return Me
    End Function
End Class
