' ReSharper disable UnusedMember.Global
' ReSharper disable UnusedMemberInSuper.Global
Namespace StatementBuilders

  Public Interface IDeleteStatementBuilder
    Inherits IStatementBuilder
    Function WithDatabaseProvider(provider As DatabaseProviders) As IDeleteStatementBuilder
    Function Build() As String
    Function WithTable(table As String) As IDeleteStatementBuilder
    Function WithCondition(condition As String) As IDeleteStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As String) As IDeleteStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int64) As IDeleteStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int32) As IDeleteStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int16) As IDeleteStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Decimal) As IDeleteStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Double) As IDeleteStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As DateTime) As IDeleteStatementBuilder
    Function WithCondition(fieldName as String, op as Condition.EqualityOperators, fieldValue as Boolean) as IDeleteStatementBuilder
    Function WithCondition(condition As ICondition) As IDeleteStatementBuilder
    Function WithAllConditions(ParamArray conditions As ICondition()) As IDeleteStatementBuilder
    Function WithAllRows() As IDeleteStatementBuilder
  End Interface
  Public Class DeleteStatementBuilder
    Inherits StatementBuilderBase
    Implements IDeleteStatementBuilder

    Public Shared Function Create() As DeleteStatementBuilder
      Return New DeleteStatementBuilder()
    End Function

    Private _table As String
    Private _allRows As Boolean

    Private ReadOnly _conditions as List(Of String) = New List(Of String)

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Date) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition As Condition = CreateCondition(fieldName, op, fieldValue)
      Return WithCondition(condition.ToString())
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Decimal) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition As Condition = CreateCondition(fieldName, op, fieldValue)
      Return WithCondition(condition.ToString())
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Double) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition As Condition = CreateCondition(fieldName, op, fieldValue)
      Return WithCondition(condition.ToString())
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Integer) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition As Condition = CreateCondition(fieldName, op, fieldValue)
      Return WithCondition(condition.ToString())
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Long) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition As Condition = CreateCondition(fieldName, op, fieldValue)
      Return WithCondition(condition.ToString())
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Short) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition As Condition = CreateCondition(fieldName, op, fieldValue)
      Return WithCondition(condition.ToString())
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As String) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition As Condition = CreateCondition(fieldName, op, fieldValue)
      Return WithCondition(condition.ToString())
    End Function

    Public Function WithDatabaseProvider(ByVal provider As DatabaseProviders) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithDatabaseProvider
      SetDatabaseProvider(provider)
      return Me
    End Function

    Public Function Build() As String Implements IDeleteStatementBuilder.Build
      CheckParameters()
      Dim parts = New List(Of String)
      parts.Add("delete from ")
      parts.Add(_openObjectQuote)
      parts.Add(_table)
      parts.Add(_closeObjectQuote)
      AddConditionsTo(parts)
      Return String.Join("", parts)
    End Function

    Private Sub AddConditionsTo(parts as List(Of String))
      if _allRows Then Return
      parts.Add(" where ")
      Dim firstCondition = True
      For Each cond As String In _conditions
        If Not firstCondition Then
          parts.Add(" and ")
        End If
        firstCondition = False
        parts.Add(cond)
      Next
    End Sub

    Private Sub CheckParameters()
      If _table Is Nothing Then
        Throw New ArgumentException([GetType]().Name + ": no table specified")
      End If
      If Not _allRows And _conditions.Count = 0 Then
        Throw New ArgumentException([GetType]().Name + ": no condition(s) specified")
      End If
    End Sub
    Public Function WithCondition(condition As String) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      _conditions.Add(condition)
      Return Me
    End Function

    Public Function WithCondition(condition As ICondition) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      condition.UseDatabaseProvider(_databaseProvider)
      _conditions.Add(condition.ToString())
      Return Me
    End Function

    Public Function WithAllConditions(ParamArray conditions As ICondition()) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithAllConditions
      Dim conditionChain  = New ConditionChain(CompoundCondition.BooleanOperators.OperatorAnd, conditions)
      conditionChain.UseDatabaseProvider(_databaseProvider)
      Return WithCondition(conditionChain)
    End Function

    Public Function WithTable(table As String) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithTable
      _table = table
      Return Me
    End Function

    Public Function WithAllRows() As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithAllRows
      _allRows = True
      Return Me
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Boolean) As IDeleteStatementBuilder Implements IDeleteStatementBuilder.WithCondition
      Dim condition  = new Condition(fieldName, op, fieldValue)
      condition.UseDatabaseProvider(_databaseProvider)
      _conditions.Add(condition.ToString())
      Return Me
    End Function
  End Class
End NameSpace