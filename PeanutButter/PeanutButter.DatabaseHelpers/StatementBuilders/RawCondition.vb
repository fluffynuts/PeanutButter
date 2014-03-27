Public Class RawCondition
    Inherits StatementBuilderBase
    Implements ICondition

    Private _condition As String

    public Sub New(condition as String)
        _condition = condition
    End Sub
    Public Overrides Function ToString() As String Implements ICondition.ToString
        return _condition
    End Function

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements ICondition.UseDatabaseProvider
        MyBase.SetDatabaseProvider(provider)
    End Sub
End Class
