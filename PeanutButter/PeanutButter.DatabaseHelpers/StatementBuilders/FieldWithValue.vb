Public Class FieldWithValue
    Public Property Name As String
    Public Property Value As String
    Public Property QuoteMe As Boolean
    Public Overrides Function ToString() As String
        Return String.Join("", New String() {Name, " = ", Value, " (FieldWithValue)"})
    End Function
    Public Sub New(fieldName As String, fieldValue As String, quote As Boolean)
        Me.Name = fieldName
        If fieldValue = vbNullString Or fieldValue Is Nothing Then
            Me.Value = "NULL"
            Me.QuoteMe = False
        Else
            Dim parts = fieldValue.Split(New Char() {Chr(0)})
            Me.Value = parts(0)
            Me.QuoteMe = quote
        End If
    End Sub
End Class