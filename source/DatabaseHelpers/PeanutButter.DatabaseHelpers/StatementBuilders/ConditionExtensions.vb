Imports System.Runtime.CompilerServices

Namespace StatementBuilders

  Public Module ConditionExtensions
    <Extension()>
    Public Function [And](ByVal first as ICondition, ByVal second as ICondition) as ICondition
      Return new CompoundCondition(first, CompoundCondition.BooleanOperators.OperatorAnd, second)
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, fieldName As String, conditionOperator As Condition.EqualityOperators, fieldValue As String, _
                          Optional quote As Boolean = True, _
                          Optional leftConditionIsField As Boolean = True, _
                          Optional rightConditionIsField As Boolean = False) as ICondition
      return first.And(new Condition(fieldName, conditionOperator, fieldValue, quote, leftConditionIsField, rightConditionIsField))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, fieldName As String, fieldValue As String, Optional quote As Boolean = True) as ICondition
      return first.And(New Condition(fieldName, fieldValue, quote))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, field As IField, fieldValue As String, Optional quote As Boolean = True) as ICondition
      return first.And(new Condition(field, fieldValue, quote))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, field As IField, op As Condition.EqualityOperators, value As String, Optional quote As Boolean = True) as ICondition
      return first.And(new Condition(field, op, value, quote))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, rightField As IField) As ICondition
      return first.And(new Condition(leftField, op, rightField))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, leftField As IField, rightField As IField) as ICondition
      return first.And(new Condition(leftField, rightField))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Int64) as ICondition
      return first.And(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Decimal) as ICondition
      return first.And(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Nullable(Of Decimal)) as ICondition
      return first.And(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Double) as ICondition
      return first.And(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as DateTime) as ICondition
      return first.And(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [And](ByVal first as ICondition, rawString as String) as ICondition
      return first.And(new Condition(rawString))
    End Function

    <Extension()>
    Public Function [Or](ByVal first As ICondition, ByVal second as ICondition) as ICondition
      Return new CompoundCondition(first, CompoundCondition.BooleanOperators.OperatorOr, second)
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, rightField As IField) as ICondition
      return first.Or(new Condition(leftField, rightField))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, op as Condition.EqualityOperators, rightField As IField) as ICondition
      return first.Or(new Condition(leftField, op, rightField))
    End Function


    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, rightField As string) as ICondition
      return first.Or(new Condition(leftField, rightField))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, rightField As string) as ICondition
      return first.Or(new Condition(leftField, op, rightField))
    End Function


    <Extension()>
    Public Function [Or](ByVal first as ICondition, fieldName As String, conditionOperator As Condition.EqualityOperators, fieldValue As String, _
                         Optional quote As Boolean = True, _
                         Optional leftConditionIsField As Boolean = True, _
                         Optional rightConditionIsField As Boolean = False) as ICondition
      return first.Or(new Condition(fieldName, conditionOperator, fieldValue, quote, leftConditionIsField, rightConditionIsField))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, fieldName As String, fieldValue As String, Optional quote As Boolean = True) as ICondition
      return first.Or(New Condition(fieldName, fieldValue, quote))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, field As IField, fieldValue As String, Optional quote As Boolean = True) as ICondition
      return first.Or(new Condition(field, fieldValue, quote))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, field As IField, op As Condition.EqualityOperators, value As String, Optional quote As Boolean = True) as ICondition
      return first.Or(new Condition(field, op, value, quote))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Int64) as ICondition
      return first.Or(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Decimal) as ICondition
      return first.Or(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Nullable(Of Decimal)) as ICondition
      return first.Or(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as Double) as ICondition
      return first.Or(new Condition(leftField, op, val))
    End Function

    <Extension()>
    Public Function [Or](ByVal first as ICondition, leftField As IField, op As Condition.EqualityOperators, val as DateTime) as ICondition
      return first.Or(new Condition(leftField, op, val))
    End Function



    <Extension()>
    public Function IsLikeOperator(ByVal op as Condition.EqualityOperators) As Boolean
      Return op = Condition.EqualityOperators.Like or _
             op = Condition.EqualityOperators.Contains or _
             op = Condition.EqualityOperators.EndsWith or _
             op = Condition.EqualityOperators.StartsWith
    End Function

    <Extension()>
    Public Function LeftWildcard(op as Condition.EqualityOperators) As String
      if op = Condition.EqualityOperators.Contains OrElse  _
         op = Condition.EqualityOperators.EndsWith Then
        return "%"
      End If
      return ""
    End Function

    <Extension()>
    Public Function RightWildcard(op as Condition.EqualityOperators) As String
      if op = Condition.EqualityOperators.Contains OrElse  _
         op = Condition.EqualityOperators.StartsWith Then
        return "%"
      End If
      return ""
    End Function
  End Module
End NameSpace