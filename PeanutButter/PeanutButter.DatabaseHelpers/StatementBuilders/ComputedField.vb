Public Class ComputedField
    Public Enum ComputeFunctions
        Max
        Min
        Coalesce
        Count
    End Enum
    Public FieldName As String
    Public ComputeFunction As ComputeFunctions
    Public FieldAlias As String
    Public Sub New(_fieldName As String, _computeFunction As ComputeFunctions, Optional _alias As String = Nothing)
        FieldName = _fieldName
        ComputeFunction = _computeFunction
        FieldAlias = CStr(IIf(_alias Is Nothing, _fieldName, _alias))
    End Sub
    Public Overrides Function ToString() As String
        Dim parts = New List(Of String)
        parts.Add(Me.ComputeFunction.ToString())
        parts.Add("([")
        parts.Add(FieldName)
        parts.Add("])")
        parts.Add(" as [")
        parts.Add(FieldAlias)
        parts.Add("]")
        Return String.Join("", parts)
    End Function
End Class
