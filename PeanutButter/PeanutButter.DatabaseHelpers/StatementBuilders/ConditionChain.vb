Public Class ConditionChain
    Inherits StatementBuilderBase
    Implements ICondition

    Private _conditions As New List(Of ICondition)
    Private _operator As CompoundCondition.BooleanOperators

    Public Overrides Function ToString() As String Implements ICondition.ToString
        Dim parts = New List(Of String)
        Dim opString = CStr(IIf(Me._operator = CompoundCondition.BooleanOperators.OperatorAnd, " and ", " or "))
        For Each c In Me._conditions
            c.UseDatabaseProvider(_databaseProvider)
            If parts.Count > 0 Then
                parts.Add(opString)
            End If
            parts.Add(c.ToString())
        Next
        Return "(" + String.Join("", parts) + ")"
    End Function

    Public Sub New(op As CompoundCondition.BooleanOperators, ParamArray conditions As ICondition())
        Me._operator = op
        For Each c In conditions.Where(Function(condition)
                                           Return Not condition Is Nothing
                                       End Function)
            Me._conditions.Add(c)
        Next
    End Sub

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements ICondition.UseDatabaseProvider
        SetDatabaseProvider(provider)
        for each c In _conditions
            c.UseDatabaseProvider(provider)
        Next
    End Sub
End Class
