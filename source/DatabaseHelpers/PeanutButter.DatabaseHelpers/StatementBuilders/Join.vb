' ReSharper disable MemberCanBePrivate.Global
Namespace StatementBuilders
  Public Class Join
    Inherits StatementBuilderBase
    Private _noLock As Boolean
    Private _hasSingleCondition As Boolean

    Public Sub UseDatabaseProvider(provider As DatabaseProviders)
      SetDatabaseProvider(provider)
      SetConditionDatabaseProviders()
    End Sub
    Public Property Direction As JoinDirections
    Public Property Conditions As List(Of ICondition) = new List(Of ICondition)
    Public Property LeftField As String
    Public Property LeftTable As String
    Public Property RightTable As String
    Public Property RightField As String
    public Property RightTableAlias as String
    Public Property EqualityOperator As Condition.EqualityOperators

    Public Sub New(direction As JoinDirections,
                   leftTable As String,
                   leftField As String,
                   op As Condition.EqualityOperators,
                   rightTable As String,
                   rightField As String)
      Me.Direction = direction
      Me.LeftTable = leftTable
      Me.LeftField = leftField
      Me.RightTable = rightTable
      Me.RightField = rightField
      EqualityOperator = op
      _hasSingleCondition = True
      SetupSingleCondition()
    End Sub

    Public Sub New(direction as JoinDirections,
                   table1 as String,
                   table2 as String,
                   ParamArray conditions() as ICondition)
      Me.Direction = direction
      LeftTable = table1
      RightTable = table2
      Me.Conditions.AddRange(conditions)
    End Sub

    Private Sub SetupSingleCondition()
      Conditions.Add(New Condition(New SelectField(LeftTable, LeftField), _
                                   EqualityOperator, _
                                   New SelectField(RightAliasOrName(), RightField)))
    End Sub

    Private Function RightAliasOrName() As String
      If RightTableAlias Is Nothing Then Return RightTable
      return RightTableAlias
    End Function

    Private Sub RecalculateSingleCondition()
      if Not _hasSingleCondition Then Return
      Conditions.Clear()
      SetupSingleCondition()
    End Sub

    Public Overrides Function ToString() As String
      Dim parts As New List(Of String)
      SetConditionDatabaseProviders()
      parts.Add(Direction.ToString().ToLower())
      parts.Add(" join ")
      parts.Add(_openObjectQuote)
      parts.Add(RightTable)
      parts.Add(_closeObjectQuote)
      If _noLock AndAlso _databaseProvider = DatabaseProviders.SQLServer Then
        parts.Add(" WITH (NOLOCK)")
      End If
      if RightTableAlias IsNot Nothing Then
        parts.Add(" as ")
        parts.Add(_openObjectQuote)
        parts.Add(RightTableAlias)
        parts.Add(_closeObjectQuote)
        RecalculateSingleCondition
      End If
      parts.Add(" on ")
      Dim compound = new ConditionChain(CompoundCondition.BooleanOperators.OperatorAnd, Conditions.ToArray())
      compound.UseDatabaseProvider(_databaseProvider)
      parts.Add(compound.ToString())
      Return String.Join("", parts)
    End Function

    Private Sub SetConditionDatabaseProviders()

      Conditions.ForEach(Sub(c) c.UseDatabaseProvider(_databaseProvider))
    End Sub

    Public Sub SetNoLock(ByVal noLock As Boolean)
      _noLock = noLock
    End Sub
  End Class
End NameSpace