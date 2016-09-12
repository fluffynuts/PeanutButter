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
        Me.Field = fieldName
        Me.Table = Nothing
        Me.Direction = direction
    End Sub

    Public Sub New(tableName As String, fieldName As String, direction As Directions)
        Me.Table = tableName
        Me.Field = fieldName
        Me.Direction = direction
    End Sub

    Public Overrides Function ToString() As String Implements IOrderBy.ToString
        Dim parts = New List(Of String)()
        If Not Me.PartialOrder Then parts.Add("order by ")
        If Me.Table Is Nothing Then
            parts.Add(Me.FieldQuote(Me.Field))
        Else
            parts.Add(Me.FieldQuote(Me.Table))
            parts.Add(".")
            parts.Add(Me.FieldQuote(Me.Field))
        End If
        parts.Add(" ")
        parts.Add(CStr(IIf(Me.Direction = Directions.Ascending, "asc", "desc")))
        Return String.Join("", parts)
    End Function

    Private Function FieldQuote(str As String) as String
        If str.IndexOf("[") > -1 Then Return str
        Return String.Join("", New String() {_openObjectQuote, str, _closeObjectQuote})
    End Function

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements IOrderBy.UseDatabaseProvider
        MyBase.SetDatabaseProvider(provider)
    End Sub

End Class
