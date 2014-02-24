Public Class SelectField
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

    Public Overrides Function ToString() As String
        If Me.Table Is Nothing Then
            Return String.Join("", New String() {"[", Me.Field, "]"})
        End If
        Return String.Join("", New String() {"[", Me.Table, "].[", Me.Field, "]"})
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

End Class
