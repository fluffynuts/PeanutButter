Namespace StatementBuilders
' ReSharper disable MemberCanBePrivate.Global

  Public Class OrderBy
    Inherits StatementBuilderBase
    Implements IOrderBy
    Public Enum Directions
      Ascending
      Descending
    End Enum

    Public Property Direction As Directions
    Public Property Field As String
    Public Property Table As String
    Public Property PartialOrder As Boolean

    Public Sub New(fieldName As String, direction As Directions)
      Field = fieldName
      Table = Nothing
      Me.Direction = direction
    End Sub

    Public Sub New(tableName As String, fieldName As String, direction As Directions)
      Table = tableName
      Field = fieldName
      Me.Direction = direction
    End Sub

    Public Overrides Function ToString() As String Implements IOrderBy.ToString
      Dim parts = New List(Of String)()
      If Not PartialOrder Then parts.Add("order by ")
      If Table Is Nothing Then
        parts.Add(FieldQuote(Field))
      Else
        parts.Add(FieldQuote(Table))
        parts.Add(".")
        parts.Add(FieldQuote(Field))
      End If
      parts.Add(" ")
      If Direction = Directions.Ascending Then
        parts.Add("asc")
      Else
        parts.Add("desc")
      End If
      Return String.Join("", parts)
    End Function

    Private Function FieldQuote(str As String) as String
      If str.IndexOf("[", StringComparison.Ordinal) > -1 Then Return str
      Return String.Join("", New String() {_openObjectQuote, str, _closeObjectQuote})
    End Function

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements IOrderBy.UseDatabaseProvider
      SetDatabaseProvider(provider)
    End Sub

  End Class
End NameSpace