Namespace StatementBuilders
  Public Class FieldWithValue
    Public Property Name As String
    Public Property Value As String
    Public Property QuoteMe As Boolean
    Public Overrides Function ToString() As String
      Return String.Join("", New String() {Name, " = ", Value, " (FieldWithValue)"})
    End Function
    Public Sub New(fieldName As String, fieldValue As String, quote As Boolean)
      Name = fieldName
      If fieldValue Is Nothing Then
        Value = "NULL"
        QuoteMe = False
      Else
        Dim parts = fieldValue.Split(New Char() {Chr(0)})
        Value = parts(0)
        QuoteMe = quote
      End If
    End Sub
  End Class
End NameSpace