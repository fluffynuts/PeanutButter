Imports System.Globalization
Imports PeanutButter.Utils

Public Interface IInsertStatementBuilder
    Inherits IStatementBuilder
    Function WithDatabaseProvider(provider As DatabaseProviders) As IInsertStatementBuilder
    Function WithTable(tableName As String) As IInsertStatementBuilder
    Function WithField(col As String, val As String) As IInsertStatementBuilder
    Function WithParameter(col As String, val As String) As IInsertStatementBuilder
    Function WithField(col As String, val As String, quote As Boolean) As IInsertStatementBuilder
    Function WithField(col As String, val As Decimal, Optional format As String = "0.00") As IInsertStatementBuilder
    Function WithField(col As String, val As Nullable(Of Decimal), Optional format As String = "0.00") As IInsertStatementBuilder
    Function WithField(col As String, val As Long) As IInsertStatementBuilder
    Function WithField(col As String, val As Nullable(Of Long)) As IInsertStatementBuilder
    Function WithField(col As String, val As DateTime) As IInsertStatementBuilder
    Function WithField(col As String, val As Nullable(Of DateTime)) As IInsertStatementBuilder
    Function WithField(col as String, val as Boolean) as IInsertStatementBuilder
    Function WithField(field As FieldWithValue) As IInsertStatementBuilder
    Function WithNonBlankField(col As String, val As String) As IInsertStatementBuilder
    Function WithConditionalField(condition As Boolean, col As String, trueVal As String, Optional falseVal As String = Nothing) As IInsertStatementBuilder
    Function WithConditionalField(condition As Boolean, col As String, trueVal As Long?, Optional falseVal As Long? = Nothing) As IInsertStatementBuilder
    Function WithConditionalField(condition As Boolean, col As String, trueVal As Decimal?, Optional falseVal As Decimal? = Nothing) As IInsertStatementBuilder
    Function Build() As String
End Interface

Public Class InsertStatementBuilder
    Inherits StatementBuilderBase
    Implements IInsertStatementBuilder

    Private _table As String
    Private ReadOnly _fields As New List(Of FieldWithValue)

    Public Shared Function Create() As IInsertStatementBuilder
        Return New InsertStatementBuilder()
    End Function

    Public Function WithTable(tableName As String) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithTable
        Me._table = tableName
        Return Me
    End Function
    Public Function Build() As String Implements IInsertStatementBuilder.Build
        CheckParameters()
        Dim sql As New List(Of String)
        sql.Add(String.Format("insert into {0}", _openObjectQuote))
        sql.Add(Me._table)
        sql.Add(String.Format("{0} ", _closeObjectQuote))
        Me.AddFieldsTo(sql)
        Return String.Join("", sql)
    End Function

    Private Sub AddFieldsTo(sql As List(Of String))
        Dim NotFirst = False
        sql.Add("(")
        For Each fld In Me._fields
            If NotFirst Then
                sql.Add(",")
            End If
            NotFirst = True
            sql.Add(_openObjectQuote)
            sql.Add(fld.Name)
            sql.Add(_closeObjectQuote)
        Next
        sql.Add(") values (")
        NotFirst = False
        For Each fld In Me._fields
            If NotFirst Then
                sql.Add(",")
            End If
            NotFirst = True
            If fld.QuoteMe Then
                sql.Add("'")
                sql.Add(fld.Value.Replace("'", "''"))
                sql.Add("'")
            Else
                sql.Add(fld.Value)
            End If
        Next
        sql.Add(")")
    End Sub

    Public Function WithField(column As String, value As String) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        Me._fields.Add(New FieldWithValue(column, value, True))
        Return Me
    End Function
    Public Function WithField(column As String, value As Decimal, Optional format As String = "0.00") As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        Dim stringVal = New DecimalDecorator(value, format).ToString()
        Me._fields.Add(New FieldWithValue(column, stringVal, False))
        Return Me
    End Function
    Public Function WithField(column As String, value As Nullable(Of Decimal), Optional format As String = "0.00") As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        If (value.HasValue) Then
            _fields.Add(New FieldWithValue(column, new DecimalDecorator(value.Value, format).ToString(), False))
        Else
            _fields.Add(New FieldWithValue(column, "NULL", False))
        End If
        Return Me
    End Function
    Public Function WithField(column As String, value As DateTime) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        Dim fmt = New DateTimeFormatInfo()
        fmt.DateSeparator = "/"
        fmt.TimeSeparator = ":"
        Me._fields.Add(New FieldWithValue(column, value.ToString("yyyy-MM-dd HH:mm:ss", fmt), True))
        Return Me
    End Function
    Public Function WithField(column As String, value As Nullable(Of DateTime)) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        If (value.HasValue) Then
            _fields.Add(New FieldWithValue(column, value.Value.ToString(), True))
        Else
            _fields.Add(New FieldWithValue(column, "NULL", False))
        End If
        Return Me
    End Function

    Public Function WithField(column As String, value As Long) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        Me._fields.Add(New FieldWithValue(column, value.ToString(), False))
        Return Me
    End Function
    Public Function WithField(column As String, value As Nullable(Of Long)) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        If value.HasValue Then
            Me._fields.Add(New FieldWithValue(column, value.Value.ToString(), False))
        Else
            Me._fields.Add(New FieldWithValue(column, "NULL", False))
        End If
        Return Me
    End Function

    Private Sub CheckParameters()
        If String.IsNullOrEmpty(Me._table) Then
            Throw New ArgumentException("InsertStatementBuilder: no table specified")
        End If
        If Me._fields.Count = 0 Then
            Throw New ArgumentException("InsertStatementBuilder: no fields specified")
        End If
    End Sub

    Public Function WithNonBlankField(col As String, val As String) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithNonBlankField
        If String.IsNullOrWhiteSpace(val) Then Return Me
        Return Me.WithField(col, Trim(val))
    End Function

    Public Function WithConditionalField(condition As Boolean, col As String, trueVal As String, Optional falseVal As String = Nothing) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithConditionalField
        If condition And Not trueVal Is Nothing Then Return Me.WithField(col, trueVal)
        If Not condition And Not falseVal Is Nothing Then Return Me.WithField(col, falseVal)
        Return Me
    End Function

    Public Function WithConditionalField(condition As Boolean, col As String, trueVal As Long?, Optional falseVal As Long? = Nothing) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithConditionalField
        If condition And Not trueVal Is Nothing Then Return Me.WithField(col, trueVal)
        If Not condition And Not falseVal Is Nothing Then Return Me.WithField(col, falseVal)
        Return Me
    End Function

    Public Function WithConditionalField2(condition As Boolean, col As String, trueVal As Decimal?, Optional falseVal As Decimal? = Nothing) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithConditionalField
        If condition And Not trueVal Is Nothing Then Return Me.WithField(col, trueVal)
        If Not condition And Not falseVal Is Nothing Then Return Me.WithField(col, falseVal)
        Return Me
    End Function

    Public Function WithField(col As String, val As Boolean) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        Me._fields.Add(new FieldWithValue(col, CInt(IIf(val, 1, 0)).ToString(), false))
        return Me
    End Function

    Public Function WithField(col As String, val As String, quote As Boolean) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        Me._fields.Add(New FieldWithValue(col, val, quote))
        return Me
    End Function

    Public Function WithParameter(col As String, val As String) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithParameter
        return Me.WithField(col, val, false)
    End Function

    Public Overloads Function WithDatabaseProvider(provider As DatabaseProviders) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithDatabaseProvider
        SetDatabaseProvider(provider)
        return Me
    End Function

    Public Function WithField(field As FieldWithValue) As IInsertStatementBuilder Implements IInsertStatementBuilder.WithField
        Me._fields.Add(field)
        return Me
    End Function
End Class
