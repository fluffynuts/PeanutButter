Public Class SelectField
    Inherits StatementBuilderBase
    Implements IField
    Public ReadOnly Table As String
    Public ReadOnly Field As String
    Public Sub New(fieldName As String)
        Me.Table = Nothing
        Me.Field = fieldName
    End Sub

    Public Sub New(tableName As String, fieldName As String)
        Me.Table = tableName
        Me.Field = fieldName
    End Sub

    Public Overrides Function ToString() As String Implements IField.ToString
        If Me.Table Is Nothing Then
            Return String.Join("", New String() {_openObjectQuote, Me.Field, _closeObjectQuote})
        End If
        Return String.Join("", New String() {_openObjectQuote, Me.Table, _closeObjectQuote, ".", _openObjectQuote, Me.Field, _closeObjectQuote})
    End Function

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
End Class
