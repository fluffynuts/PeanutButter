Public Class Join
    Inherits StatementBuilderBase
    Private _noLock As Boolean

    Public Enum JoinDirection
        Left
        Inner
        Right
        Outer
    End Enum
    Public Sub UseDatabaseProvider(provider As DatabaseProviders)
        SetDatabaseProvider(provider)
        Me.SetupCondition()
    End Sub
    Public Property Direction As JoinDirection
    Public Property Condition As Condition
    Public Property LeftField As String
    Public Property LeftTable As String
    Public Property RightTable As String
    Public Property RightField As String
    Public Property EqualityOperator As Condition.EqualityOperators

    Public Sub New(_direction As JoinDirection,
                    _leftTable As String,
                    _leftField As String,
                    _op As Condition.EqualityOperators,
                    _rightTable As String,
                    _rightField As String)
        Me.Direction = _direction
        Me.LeftTable = _leftTable
        Me.LeftField = _leftField
        Me.RightTable = _rightTable
        Me.RightField = _rightField
        Me.EqualityOperator = _op
        SetupCondition()
    End Sub

    Private Sub SetupCondition()
        Dim localLeftField  = _openObjectQuote + LeftTable + _closeObjectQuote + "." + _openObjectQuote + LeftField + _closeObjectQuote
        Dim localRightField  = _openObjectQuote + RightTable + _closeObjectQuote + "." + _openObjectQuote + RightField + _closeObjectQuote
        Me.Condition = New Condition(localLeftField, EqualityOperator, localRightField, False)
        Me.Condition.UseDatabaseProvider(_databaseProvider)
    End Sub

    Public Overrides Function ToString() As String
        Dim parts As New List(Of String)
        Me.SetupCondition()
        parts.Add(Me.Direction.ToString().ToLower())
        parts.Add(" join ")
        parts.Add(_openObjectQuote)
        parts.Add(Me.RightTable)
        parts.Add(_closeObjectQuote)
        If _noLock AndAlso _databaseProvider = DatabaseProviders.SQLServer Then
            parts.Add(" WITH (NOLOCK)")
        End If
        parts.Add(" on ")
        parts.Add(Me.Condition.ToString())
        Return String.Join("", parts)
    End Function

    Public Sub SetNoLock(ByVal noLock As Boolean)
        _noLock = noLock
    End Sub
End Class
