Public Class SelectField
    Inherits StatementBuilderBase
    Implements IField
    Public ReadOnly Table As String
    Public ReadOnly Field As String
    Private _alias As String

    Public Sub New(fieldName As String)
        Me.Table = Nothing
        Me.Field = fieldName
    End Sub

    Public Sub New(tableName As String, fieldName As String)
        Me.Table = tableName
        Me.Field = fieldName
    End Sub

    Public Overrides Function ToString() As String Implements IField.ToString
        Dim withoutAlias as String
        If Me.Table Is Nothing Then
            withoutAlias = String.Join("", _fieldQuoteOpen, Field, _fieldQuoteClose)
        Else
            withoutAlias = String.Join("", _openObjectQuote, Table, _closeObjectQuote, ".", _fieldQuoteOpen, Me.Field, _fieldQuoteClose)
        End If
        if _alias is Nothing Then
            return withoutAlias
        End If
        return String.Join("", withoutAlias, " as ", _openObjectQuote, _alias, _closeObjectQuote)
    End Function

    Private ReadOnly Property _fieldQuoteOpen as String
        Get
            Return CStr(IIf(Field = "*", "", _openObjectQuote))
        End Get
    End Property

    Private ReadOnly Property _fieldQuoteClose as String
        Get
            Return CStr(IIf(Field = "*", "", _closeObjectQuote))
        End Get
    End Property

    Public Overrides Function Equals(obj As Object) As Boolean
        Dim other = TryCast(obj, SelectField)
        If other Is Nothing Then
            Return False
        End If
        Return Me.ToString() = other.ToString()
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return Me.ToString().GetHashCode()
    End Function

    Public Sub UseDatabaseProvider(ByVal provider As DatabaseProviders) Implements IField.UseDatabaseProvider
        SetDatabaseProvider(provider)
    End Sub

    Public Sub SetAlias(s As String)
        _alias = s
    End Sub
End Class
