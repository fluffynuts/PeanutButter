Namespace StatementBuilders

  Public Class SelectField
    Inherits StatementBuilderBase
    Implements IField
    Public ReadOnly Table As String
    Public ReadOnly Field As String
    Private _alias As String

    Public Sub New(fieldName As String)
      Table = Nothing
      Field = fieldName
    End Sub

    Public Sub New(tableName As String, fieldName As String)
      Table = tableName
      Field = fieldName
    End Sub

    Public Overrides Function ToString() As String Implements IField.ToString
      Dim withoutAlias as String
      If Table Is Nothing Then
        withoutAlias = String.Join("", FieldQuoteOpen, Field, FieldQuoteClose)
      Else
        withoutAlias = String.Join("", _openObjectQuote, Table, _closeObjectQuote, ".", FieldQuoteOpen, Field, FieldQuoteClose)
      End If
      if _alias is Nothing Then
        return withoutAlias
      End If
      return String.Join("", withoutAlias, " as ", _openObjectQuote, _alias, _closeObjectQuote)
    End Function

    Private ReadOnly Property FieldQuoteOpen as String
      Get
        Return CStr(IIf(Field = "*", "", _openObjectQuote))
      End Get
    End Property

    Private ReadOnly Property FieldQuoteClose as String
      Get
        Return CStr(IIf(Field = "*", "", _closeObjectQuote))
      End Get
    End Property

    Public Overrides Function Equals(obj As Object) As Boolean
      Dim other = TryCast(obj, SelectField)
      If other Is Nothing Then
        Return False
      End If
      Return ToString() = other.ToString()
    End Function

    Public Overrides Function GetHashCode() As Integer
      Return ToString().GetHashCode()
    End Function

    Public Sub UseDatabaseProvider(ByVal provider As DatabaseProviders) Implements IField.UseDatabaseProvider
      SetDatabaseProvider(provider)
    End Sub

    Public Sub SetAlias(s As String)
      _alias = s
    End Sub
  End Class
End NameSpace