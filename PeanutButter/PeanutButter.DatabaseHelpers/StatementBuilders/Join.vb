Public Class Join
    Public Enum JoinDirection
        Left
        Inner
        Right
        Outer
    End Enum
    Public Property Direction As JoinDirection
    Public Property Condition As Condition
    Public Property LeftField As String
    Public Property LeftTable As String
    Public Property RightTable As String
    Public Property RightField As String
    Public Sub New(_direction As JoinDirection,
                    _leftTable As String,
                    _leftField As String,
                    _op As Condition.EqualityOperators,
                    _rightTable As String,
                    _rightField As String)
        Me.Direction = _direction
        Me.Condition = New Condition("[" + _leftTable + "].[" + _leftField + "]", _op, "[" + _rightTable + "].[" + _rightField + "]", False)
        Me.LeftTable = _leftTable
        Me.LeftField = _leftField
        Me.RightTable = _rightTable
        Me.RightField = _rightField
    End Sub

    Public Overrides Function ToString() As String
        Dim parts As New List(Of String)
        parts.Add(Me.Direction.ToString().ToLower())
        parts.Add(" join [")
        parts.Add(Me.RightTable)
        parts.Add("] on ")
        parts.Add(Me.Condition.ToString())
        Return String.Join("", parts)
    End Function
End Class
