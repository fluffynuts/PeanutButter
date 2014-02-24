Public Interface ICondition
    Function ToString() As String
End Interface

Public Class Condition
    Implements ICondition
    Public Enum EqualityOperators
        Equals
        NotEquals
        GreaterThan
        GreaterThanOrEqualTo
        LessThan
        LessThanOrEqualTo
        Like_
    End Enum
    Public Shared ReadOnly Property OperatorResolutions As IDictionary(Of EqualityOperators, String)
        Get
            Return _operatorResolutions
        End Get
    End Property
    Protected Shared _operatorResolutions As ReadOnlyDictionary(Of EqualityOperators, String) = New ReadOnlyDictionary(Of EqualityOperators, String)
    Shared Sub New()
        Dim operators As New Dictionary(Of EqualityOperators, String)
        operators(EqualityOperators.Equals) = "="
        operators(EqualityOperators.NotEquals) = "<>"
        operators(EqualityOperators.GreaterThan) = ">"
        operators(EqualityOperators.GreaterThanOrEqualTo) = ">="
        operators(EqualityOperators.LessThan) = "<"
        operators(EqualityOperators.LessThanOrEqualTo) = "<="
        operators(EqualityOperators.Like_) = " like "
        _operatorResolutions = New ReadOnlyDictionary(Of EqualityOperators, String)(operators)
    End Sub

    Public ReadOnly FieldName As String
    Public ReadOnly EqualityOperator As EqualityOperators
    Public ReadOnly Value As String
    Public ReadOnly QuoteValue As Boolean
    Public ReadOnly LeftConditionIsField As Boolean
    Public ReadOnly RightConditionIsField As Boolean

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As String, Optional quote As Boolean = True, Optional _leftConditionIsField As Boolean = True, Optional _rightConditionIsField As Boolean = False)
        Me.FieldName = _fieldName
        Me.EqualityOperator = _conditionOperator
        If _fieldValue Is Nothing Then
            Me.Value = "NULL"
            Me.QuoteValue = False
        ElseIf _conditionOperator = EqualityOperators.Like_ Then
            Me.Value = String.Join("", {"%", _fieldValue, "%"})
            Me.QuoteValue = True
        Else
            Me.Value = _fieldValue
            Me.QuoteValue = CBool(IIf((Not quote) Or _fieldValue = "?", False, True))
        End If
        Me.LeftConditionIsField = _leftConditionIsField
        Me.RightConditionIsField = _rightConditionIsField
    End Sub

    Public Sub New(_fieldName As String, _fieldValue As String, Optional quote As Boolean = True)
        Me.New(_fieldName, EqualityOperators.Equals, _fieldValue, quote)
    End Sub

    Public Sub New(_fieldName as String, op as EqualityOperators, _fieldValue as Boolean)
        Me.New(_fieldName, op, CInt(IIf(_fieldValue, 1, 0)).ToString(), false)
    End Sub

    Public Sub New(field As SelectField, _fieldValue As String, Optional quote As Boolean = True)
        Me.New(field, EqualityOperators.Equals, _fieldValue, quote)
    End Sub

    Public Sub New(field As SelectField, op As EqualityOperators, value As String, Optional quote As Boolean = True)
        Me.FieldName = field.ToString()
        Me.EqualityOperator = op
        Me.QuoteValue = quote
        Me.Value = value
        Me.LeftConditionIsField = True
        Me.RightConditionIsField = False
    End Sub

    Public Sub New(leftField As SelectField, op As EqualityOperators, rightField As SelectField)
        Me.FieldName = leftField.ToString()
        Me.LeftConditionIsField = True
        Me.EqualityOperator = op
        Me.Value = rightField.ToString()
        Me.QuoteValue = False
        Me.RightConditionIsField = True
    End Sub

    Public Sub New(leftField As SelectField, rightField As SelectField)
        Me.New(leftField, EqualityOperators.Equals, rightField)
    End Sub


    Public Overrides Function ToString() As String Implements ICondition.ToString
        Dim parts = New List(Of String)

        If (FieldName.IndexOf("[") < 0) And LeftConditionIsField Then
            parts.Add(Me.FieldQuote(FieldName))
        Else
            parts.Add(Me.FieldName)
        End If
        If Me.Value = "NULL" And Not Me.QuoteValue Then
            parts.Add(" is ")
        Else
            parts.Add(_operatorResolutions(Me.EqualityOperator))
        End If
        If (Me.QuoteValue) Then
            parts.AddRange(New String() {"'", Me.Value.Replace("'", "''"), "'"})
        ElseIf RightConditionIsField Then
            parts.Add(FieldQuote(Me.Value))
        Else
            parts.Add(Me.Value)
        End If
        Return String.Join("", parts)
    End Function

    Private Function FieldQuote(val As String) As String
        If val.IndexOf("[") > -1 Then
            Return val
        End If
        Return String.Join("", New String() {"[", val, "]"})
    End Function

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Int32)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Int16)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Decimal)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Int64)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Double)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As DateTime)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString("yyyy/MM/dd HH:mm:ss"), True, True, False)
    End Sub

End Class
