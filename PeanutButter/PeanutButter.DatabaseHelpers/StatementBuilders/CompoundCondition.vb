Public Class CompoundCondition
    Inherits StatementBuilderBase
    Implements ICondition


    Public Enum BooleanOperators
        OperatorAnd
        OperatorOr
    End Enum

    Public ReadOnly LeftCondition As ICondition
    Public ReadOnly RightCondition As ICondition
    Public ReadOnly LogicalOperator As BooleanOperators

    Public Shared ReadOnly Property OperatorResolutions As IDictionary(Of BooleanOperators, String)
        Get
            Return _operatorResolutions
        End Get
    End Property

    Private Shared _operatorResolutions As IDictionary(Of BooleanOperators, String)
    Shared Sub New()
        Dim opRes = New Dictionary(Of BooleanOperators, String)
        opRes(BooleanOperators.OperatorAnd) = "and"
        opRes(BooleanOperators.OperatorOr) = "or"
        _operatorResolutions = New ReadOnlyDictionary(Of BooleanOperators, String)(opRes)
    End Sub

    Public Sub New(leftCondition As ICondition, logicalOp As BooleanOperators, rightCondition As ICondition)
        Me.LeftCondition = leftCondition
        Me.RightCondition = rightCondition
        Me.LogicalOperator = logicalOp
    End Sub

    Public Overrides Function ToString() As String Implements ICondition.ToString
        LeftCondition.UseDatabaseProvider(_databaseProvider)
        RightCondition.UseDatabaseProvider(_databaseProvider)
        Return String.Join("", New String() { _
           "(", _
           Me.LeftCondition.ToString(), _
           " ", _
           _operatorResolutions(Me.LogicalOperator), _
           " ", _
           Me.RightCondition.ToString(), _
           ")" _
        })
    End Function

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements ICondition.UseDatabaseProvider
        MyBase.SetDatabaseProvider(provider)
    End Sub
End Class
