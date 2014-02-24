Public Interface IOrderBy
    Overloads Function ToString() As String
End Interface

Public Class MultiOrderBy
    Implements IOrderBy
    Private _parts As New List(Of OrderBy)
    Public Sub New(direction As OrderBy.Directions, ParamArray fields As String())
        Dim partialO = False
        For Each field In fields
            Dim orderBy = New OrderBy(field, direction)
            orderBy.PartialOrder = partialO
            _parts.Add(orderBy)
            partialO = True
        Next
    End Sub

    Public Overloads Function ToString() As String Implements IOrderBy.ToString
        Dim result = New List(Of String)
        For Each part In _parts
            result.Add(part.ToString())
        Next
        Return String.Join(", ", result)
    End Function

End Class
