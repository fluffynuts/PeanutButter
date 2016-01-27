
Public Interface ISelectStatementBuilder
    Inherits IStatementBuilder
    Function WithDatabaseProvider(provider As DatabaseProviders) As ISelectStatementBuilder
    Function WithTable(ByVal name As String) As ISelectStatementBuilder
    Function WithField(ByVal name As String, Optional aliasAs As String = Nothing) As ISelectStatementBuilder
    Function WithField(ByVal field As IField) As ISelectStatementBuilder
    Function WithFields(ParamArray fields() As String) As ISelectStatementBuilder
    Function WithAllFieldsFrom(table As String) As ISelectStatementBuilder
    Function WithAllFieldsFrom(subQueryBuilder as ISelectStatementBuilder, subQueryAlias as String) as ISelectStatementBuilder
    Function WithCondition(condition As ICondition) As ISelectStatementBuilder
    Function WithCondition(condition As String) As ISelectStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As String) As ISelectStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int64) As ISelectStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int32) As ISelectStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Int16) As ISelectStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Decimal) As ISelectStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Double) As ISelectStatementBuilder
    Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As DateTime) As ISelectStatementBuilder
    Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As String) As ISelectStatementBuilder
    Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Int64) As ISelectStatementBuilder
    Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Int32) As ISelectStatementBuilder
    Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Int16) As ISelectStatementBuilder
    Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Decimal) As ISelectStatementBuilder
    Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Double) As ISelectStatementBuilder
    Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As DateTime) As ISelectStatementBuilder
    Function WithCondition(field As String, op As Condition.EqualityOperators, fieldValue As Boolean) As ISelectStatementBuilder
    Function WithCondition(leftField As IField, op As Condition.EqualityOperators, rightField As IField) As ISelectStatementBuilder
    Function WithAllConditions(ParamArray conditions As ICondition()) As ISelectStatementBuilder
    Function WithAnyCondition(ParamArray conditions As ICondition()) As ISelectStatementBuilder
    Function WithComputedField(fieldName As String, functionName As ComputedField.ComputeFunctions, Optional fieldAlias As String = Nothing) As ISelectStatementBuilder
    Function Build() As String
    Function WithInnerJoin(table1 As String, field1 As String, eq As Condition.EqualityOperators, table2 As String, field2 As String) As ISelectStatementBuilder
    Function WithInnerJoin(table1 As String, field1 As String, table2 As String, Optional field2 As String = Nothing) As ISelectStatementBuilder
    Function WithLeftJoin(table1 As String, field1 As String, eq As Condition.EqualityOperators, table2 As String, field2 As String) As ISelectStatementBuilder
    Function WithLeftJoin(table1 As String, field1 As String, table2 As String, Optional field2 As String = Nothing) As ISelectStatementBuilder
    Function WithJoin(table1 as String, table2 as String, direction as JoinDirections, ParamArray joinConditions() as ICondition) as ISelectStatementBuilder
    Function OrderBy(orderByObj As OrderBy) As ISelectStatementBuilder
    Function OrderBy(fieldName As String, direction As OrderBy.Directions) As ISelectStatementBuilder
    Function OrderBy(tableName As String, fieldName As String, direction As OrderBy.Directions) As ISelectStatementBuilder
    Function OrderBy(multi As MultiOrderBy) As ISelectStatementBuilder
    Function Distinct() As ISelectStatementBuilder
    Function WithTop(rows As Integer) As ISelectStatementBuilder
    Function WithNoLock() As ISelectStatementBuilder
End Interface
Public Class SelectStatementBuilder
    Inherits StatementBuilderBase
    Implements ISelectStatementBuilder

    Dim _distinct As Boolean
    Dim _top As Integer?

    Public Shared Function Create() As ISelectStatementBuilder
        Return New SelectStatementBuilder()
    End Function

    Public Overrides Function ToString() As String
        Return Me.Build()
    End Function

    Private Class RenderedCondition
        Public Property WasRaw as Boolean
        Public Property Value as String
        public Sub New(value as String, raw as Boolean)
            Me.Value = value
            Me.WasRaw = raw
        End Sub
    End Class

    Private Class SubQuery
        Public ReadOnly SubQueryStatement as ISelectStatementBuilder
        Public ReadOnly SubQueryAlias as String
        public Sub New (stmt as ISelectStatementBuilder, sAlias as String)
            SubQueryStatement = stmt
            SubQueryAlias = sAlias
        End Sub
    End Class

    Private _subQueries as new List(Of SubQuery)
    Private _tableNames As New List(Of String)
    Private _fields As New List(Of IField)
    Private _joins As New List(Of Join)
    Private _orderBy As IOrderBy
    Private _iCondition As ICondition
    Private _noLock As Boolean

    Public Function WithTable(name As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithTable
        If _tableNames.Any(Function(tn)
                               return tn.ToLower() = name.ToLower()
                           End Function) Then Return Me
        _tableNames.Add(name)
        Return Me
    End Function
    Public Function WithField(name As String, Optional aliasAs As String = Nothing) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithField
        If name = "*" And _fields.Any(Function(fn)
                                              return fn.ToString().ToLower() = name.ToLower()
                                          End Function) Then Return Me
        Dim selectField  = new SelectField(name)
        selectField.SetAlias(aliasAs)
        _fields.Add(selectField)
        Return Me
    End Function

    Public Function WithField(field As IField) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithField
        field.UseDatabaseProvider(_databaseProvider)
        _fields.Add(field)
        Return Me
    End Function

    Public Function WithCondition(clause As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        return SetOrAnd(new Condition(clause))
    End Function

    Private Function SetOrAnd(condition as ICondition) as ISelectStatementBuilder
        if _iCondition Is Nothing Then
            _iCondition = condition
        Else 
            _iCondition = _ICondition.And(condition)
        End If
        return Me
    End Function

    Public Function WithCondition(condition As ICondition) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        condition.UseDatabaseProvider(_databaseProvider)
        Return SetOrAnd(condition)
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Date) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Decimal) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Double) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Integer) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Long) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As Short) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(fieldName As String, op As Condition.EqualityOperators, fieldValue As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(fieldName, op, fieldValue))
    End Function

    Public Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(new Condition(field, op, fieldValue))
    End Function

    Public Function WithCondition(leftField As IField, op As Condition.EqualityOperators, rightField As IField) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Dim condition = CreateCondition(leftField, op, rightField)
        Return Me.WithCondition(condition)
    End Function

    Public Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Int64) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Dim condition As Condition = CreateCondition(field, op, fieldValue.ToString())
        Return Me.WithCondition(condition)
    End Function

    Public Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Int32) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(field, op, fieldValue.ToString()))
    End Function
    Public Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Int16) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(field, op, fieldValue.ToString()))
    End Function
    Public Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Decimal) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(field, op, fieldValue.ToString()))
    End Function
    Public Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As Double) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(field, op, fieldValue.ToString()))
    End Function
    Public Function WithCondition(field As IField, op As Condition.EqualityOperators, fieldValue As DateTime) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(CreateCondition(field, op, fieldValue.ToString("yyyy/MM/dd"), True))
    End Function

    Public Function WithAllConditions(ParamArray conditions As ICondition()) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithAllConditions
        Return Me.WithCondition(New ConditionChain(CompoundCondition.BooleanOperators.OperatorAnd, conditions))
    End Function

    Public Function WithAnyCondition(ParamArray conditions As ICondition()) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithAnyCondition
        Return Me.WithCondition(New ConditionChain(CompoundCondition.BooleanOperators.OperatorOr, conditions))
    End Function

    Public Function WithCondition(field As String, op As Condition.EqualityOperators, fieldValue As Boolean) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithCondition
        Return Me.WithCondition(new Condition(field, op, CInt(IIf(fieldValue, 1, 0))))
    End Function

    Public Function Build() As String Implements ISelectStatementBuilder.Build
        CheckParameters()
        Dim sql = New List(Of String)
        sql.Add("select ")
        If Me._distinct Then sql.Add("distinct ")
        AddLeadingRowLimiterIfRequired(sql)
        Me.AddFieldsTo(sql)
        sql.Add(" from ")
        sql.Add(GetInitialTables())
        Me.AddJoinsTo(sql)
        Me.AddConditionsTo(sql)
        Me.AddOrdersTo(sql)
        Return String.Join("", sql)
    End Function

    Private Function GetInitialTables() As String
        _subQueries.ForEach(Function(s) s.SubQueryStatement.WithDatabaseProvider(_databaseProvider))
        Dim quotedTableNames = _tableNames.Select(Function(tn)
            Return String.Join("", { _openObjectQuote, tn, _closeObjectQuote })
                                                     End Function) _
            .Union(_subQueries.Select(Function(s) "(" + s.SubQueryStatement.ToString() + ") as " + _openObjectQuote + s.SubQueryAlias + _closeObjectQuote))
        Dim joinWith = ","
        AddNoLockHintAsRequiredTo(joinWith)
        Dim result  = String.Join(joinWith, quotedTableNames)
        AddNoLockHintAsRequiredTo(result)
        Return result
    End Function

    Private Function ShouldAddNoLockHint() As Boolean
        Return _noLock And _databaseProvider = DatabaseProviders.SQLServer
    End Function

    Private Sub AddLeadingRowLimiterIfRequired(ByVal sql As List(Of String))
        If Not Me._top.HasValue Then Return
        If _databaseProvider = DatabaseProviders.Firebird Then
            sql.Add("first " + Me._top.Value.ToString() + " ")
        else
            sql.Add("top " + Me._top.Value.ToString() + " ")
        End If
    End Sub

    Private Sub AddOrdersTo(sql As List(Of String))
        If (Me._orderBy Is Nothing) Then Exit Sub
        sql.Add(" ")
        _orderBy.UseDatabaseProvider(_databaseProvider)
        sql.Add(Me._orderBy.ToString())
    End Sub

    Private Sub AddJoinsTo(sql As List(Of String))
        _joins.ForEach(Sub(join)
                           sql.Add(" ")
                           join.UseDatabaseProvider(_databaseProvider)
                           join.SetNoLock(_noLock)
                           sql.Add(join.ToString())
                       End Sub)
    End Sub

    Private Sub AddNoLockHintAsRequiredTo(ByRef str As String)
        str += GetNoLockHintString()
    End Sub

    Private Function GetNoLockHintString() As String
        If Not ShouldAddNoLockHint() Then Return ""
        Select Case _databaseProvider
            Case DatabaseProviders.SQLServer
                return " WITH (NOLOCK)"
            Case Else
                return ""
        End Select
    End Function


    Private Sub CheckParameters()
        If _tableNames.Count = 0 AndAlso _subQueries.Count = 0 Then
            Throw New ArgumentException(Me.GetType().Name() + ": must specify at least one table or subquery before building")
        End If
    End Sub

    Private Sub AddConditionsTo(ByVal sql As List(Of String))
        if _iCondition Is Nothing Then
            Return
        End If
        sql.Add(" where ")
        sql.Add(_iCondition.ToString())
    End Sub

    Private Sub AddFieldsTo(ByVal sql As List(Of String))
        Dim addedFields = 0
        _fields.ForEach(Sub(field)
                                field.UseDatabaseProvider(_databaseProvider)
                                Dim fieldName = field.ToString()
                                sql.Add(CStr(IIf(addedFields = 0, "", ",")))
                                addedFields += 1
                                sql.Add(fieldName)
                            End Sub)
        If addedFields = 0 Then
            Throw New ArgumentException(Me.GetType().Name() + ": no fields specified for query")
        End If
    End Sub

    Function WithAllFieldsFrom(table As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithAllFieldsFrom
        Return Me.WithTable(table) _
                    .WithField("*")
    End Function

    Function WithAllFieldsFrom(subQuery as ISelectStatementBuilder, subQueryAlias as String) as ISelectStatementBuilder Implements ISelectStatementBuilder.WithAllFieldsFrom
        _subQueries.Add(new SubQuery(subQuery, subQueryAlias))
        return WithField(new SelectField("*"))
    End Function

    Shared Function SelectAllFrom(table As String) As String
        Return Create().WithAllFieldsFrom(table).Build()
    End Function

    Public Function WithComputedField(fieldName As String, func As ComputedField.ComputeFunctions, Optional fieldAlias As String = Nothing) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithComputedField
        _fields.Add(New ComputedField(fieldName, func, fieldAlias))
        Return Me
    End Function

    Public Function OrderBy(orderByObj As OrderBy) As ISelectStatementBuilder Implements ISelectStatementBuilder.OrderBy
        Me._orderBy = orderByObj
        Return Me
    End Function

    Public Function OrderBy(fieldName As String, direction As OrderBy.Directions) As ISelectStatementBuilder Implements ISelectStatementBuilder.OrderBy
        Return Me.OrderBy(New OrderBy(fieldName, direction))
    End Function

    Public Function OrderBy(tableName As String, fieldName As String, direction As OrderBy.Directions) As ISelectStatementBuilder Implements ISelectStatementBuilder.OrderBy
        Return Me.OrderBy(New OrderBy(tableName, fieldName, direction))
    End Function

    Public Function Distinct() As ISelectStatementBuilder Implements ISelectStatementBuilder.Distinct
        Me._distinct = True
        Return Me
    End Function

    Public Function OrderBy(multi As MultiOrderBy) As ISelectStatementBuilder Implements ISelectStatementBuilder.OrderBy
        Me._orderBy = multi
        Return Me
    End Function

    Public Function WithTop(rows As Integer) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithTop
        Me._top = rows
        Return Me
    End Function

    Public Function WithFields(ParamArray fields() As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithFields
        For Each fld As String In fields
            Me.WithField(fld)
        Next
        Return Me
    End Function

    Public Function WithDatabaseProvider(provider As DatabaseProviders) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithDatabaseProvider
        SetDatabaseProvider(provider)
        If _iCondition IsNot Nothing Then
            _iCondition.UseDatabaseProvider(provider)
        End If
        _subQueries.ForEach(Function(s) s.SubQueryStatement.WithDatabaseProvider(provider))
        return Me
    End Function

    Public Function WithJoin(table1 As String, table2 As String, direction As JoinDirections, 
                             ParamArray joinConditions As ICondition()) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithJoin
        Dim joinObject = new Join(direction, table1, table2, joinConditions)
        _joins.Add(joinObject)
        Return Me
    End Function

    Public Function WithInnerJoin(leftTable As String, leftField As String, eq As Condition.EqualityOperators, rightTable As String, rightField As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithInnerJoin
        Dim joinObject As Join = CreateJoinObjectFor(JoinDirections.Inner, leftTable, leftField, eq, rightTable, rightField)
        Me._joins.Add(joinObject)
        Return Me
    End Function

    Private Function CreateJoinObjectFor(ByVal direction As JoinDirections, ByVal leftTable As String, ByVal leftField As String, ByVal eq As Condition.EqualityOperators, ByVal rightTable As String, ByVal rightField As String) As Join
        Dim joinObject  = New Join(direction, leftTable, leftField, eq, rightTable, rightField)
        joinObject.UseDatabaseProvider(_databaseProvider)
        Return joinObject
    End Function

    Public Function WithInnerJoin(leftTable As String, leftField As String, rightTable As String, Optional rightField As String = Nothing) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithInnerJoin
        If String.IsNullOrEmpty(rightField) Then
            rightField = leftField
        End If
        Return Me.WithInnerJoin(leftTable, leftField, Condition.EqualityOperators.Equals, rightTable, rightField)
    End Function

    Public Function WithLeftJoin(table1 As String, field1 As String, eq As Condition.EqualityOperators, table2 As String, field2 As String) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithLeftJoin
        Dim joinObject = CreateJoinObjectFor(JoinDirections.Left, table1, field1, eq, table2, field2)
        Me._joins.Add(joinObject)
        return Me
    End Function

    Public Function WithLeftJoin(table1 As String, field1 As String, table2 As String, Optional field2 As String = Nothing) As ISelectStatementBuilder Implements ISelectStatementBuilder.WithLeftJoin
        if String.IsNullOrEmpty(field2) Then
            field2 = field1
        End If
        return WithLeftJoin(table1, field1, Condition.EqualityOperators.Equals, table2, field2)
    End Function

    Public Function WithNoLock() As ISelectStatementBuilder Implements ISelectStatementBuilder.WithNoLock
        _noLock = True
        return Me
    End Function
End Class
