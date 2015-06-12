Public Class StatementBuilderBase
    Inherits StatementBuilderDatabaseProviderBase
    
    Public Sub New
        SetDatabaseProvider(DatabaseProviders.Access)
    End Sub

    Protected Function CreateCondition(ByVal fieldName As String, ByVal op As Condition.EqualityOperators, ByVal fieldValue As Date) As Condition
        Dim condition  = New Condition(fieldName, op, fieldValue)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal fieldName As String, ByVal op As Condition.EqualityOperators, ByVal fieldValue As Decimal) As Condition
        Dim condition  = New Condition(fieldName, op, fieldValue)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal fieldName As String, ByVal op As Condition.EqualityOperators, ByVal fieldValue As Double) As Condition
        Dim condition  = New Condition(fieldName, op, fieldValue)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal fieldName As String, ByVal op As Condition.EqualityOperators, ByVal fieldValue As Integer) As Condition
        Dim condition  = New Condition(fieldName, op, fieldValue)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal fieldName As String, ByVal op As Condition.EqualityOperators, ByVal fieldValue As Long) As Condition
        Dim condition  = New Condition(fieldName, op, fieldValue)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal fieldName As String, ByVal op As Condition.EqualityOperators, ByVal fieldValue As Short) As Condition
        Dim condition  = New Condition(fieldName, op, fieldValue)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal fieldName As String, ByVal op As Condition.EqualityOperators, ByVal fieldValue As String) As Condition
        Dim condition  = New Condition(fieldName, op, fieldValue)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal leftField As SelectField, ByVal op As Condition.EqualityOperators, ByVal rightField As SelectField) As Condition
        leftField.UseDatabaseProvider(_databaseProvider)
        Dim condition  = New Condition(leftField, op, rightField)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function

    Protected Function CreateCondition(ByVal field As SelectField, ByVal op As Condition.EqualityOperators, ByVal fieldValue As String) As Condition
        return CreateCondition(field, op, fieldValue, False)
    End Function

    Protected Function CreateCondition(ByVal field As SelectField, ByVal op As Condition.EqualityOperators, ByVal fieldValue As String, quote As boolean) As Condition
        field.UseDatabaseProvider(_databaseProvider)
        Dim condition  = New Condition(field, op, fieldValue, quote)
        condition.UseDatabaseProvider(_databaseProvider)
        Return condition
    End Function


End Class