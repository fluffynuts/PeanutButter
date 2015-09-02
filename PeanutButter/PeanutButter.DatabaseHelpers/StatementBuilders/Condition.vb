Imports PeanutButter.Utils

Public Interface ICondition
    Sub UseDatabaseProvider(provider As DatabaseProviders)
    Function ToString() As String
End Interface

Public Class Condition
    Inherits StatementBuilderBase
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

    Private __fieldName as String
    Public Property FieldName As String
        Get
            return __fieldName
        End Get
        Protected Set(value As String)
            __fieldName = value
        End Set
    End Property
    Public ReadOnly EqualityOperator As EqualityOperators
    Private __value as String
    Public Property Value As String
        Get
            return __value
        End Get
        Set(value As String)
            __value = value
        End Set
    End Property
    Public ReadOnly QuoteValue As Boolean
    Public ReadOnly LeftConditionIsField As Boolean
    Public ReadOnly RightConditionIsField As Boolean
    Private _leftField As SelectField
    Private _rightField As SelectField

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
        Me._leftField = field
        Me.FieldName = field.ToString()
        Me.EqualityOperator = op
        Me.QuoteValue = quote
        Me.Value = value
        Me.LeftConditionIsField = True
        Me.RightConditionIsField = False
    End Sub

    Public Sub New(leftField As SelectField, op As EqualityOperators, rightField As SelectField)
        Me._leftField = leftField
        Me._rightField = rightField
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

        If (FieldName.IndexOf(_openObjectQuote) < 0) And LeftConditionIsField Then
            parts.Add(Me.FieldQuote(FieldName))
        Else
            parts.Add(Me.FieldName)
        End If
        If Me.Value = "NULL" And Not Me.QuoteValue Then
            parts.Add(ResolveIsOperator())
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

    Private Function ResolveIsOperator() As String
        Select Case EqualityOperator
            Case EqualityOperators.Equals
                Return " is "
            Case EqualityOperators.NotEquals
                Return " is NOT "
            Case Else
                Throw new Exception("Invalid equality operator " & EqualityOperator & " for NULL value")
        End Select
    End Function

    Private Function FieldQuote(val As String) As String
        If val.IndexOf(_openObjectQuote) > -1 Then
            Return val
        End If
        Return String.Join("", New String() {_openObjectQuote, val, _closeObjectQuote})
    End Function

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Int32)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Int16)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Decimal)
        Me.New(_fieldName, _conditionOperator, new DecimalDecorator(_fieldValue).ToString(), False, True, False)
    End Sub

    Public Sub New (_fieldName as String, _conditionOperator as EqualityOperators, _fieldValue as Nullable(of Decimal))
        Me.New(_fieldName, _conditionOperator, CStr(IIf(_fieldValue.HasValue, new DecimalDecorator(_fieldValue.Value).ToString(), "NULL")), false)
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

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements ICondition.UseDatabaseProvider
        SetDatabaseProvider(provider)
        ReevaluateSelectFields()
    End Sub

    Private Sub ReevaluateSelectFields()
        if Not _leftField Is Nothing Then
            _leftField.UseDatabaseProvider(_databaseProvider)
            Me.FieldName = _leftField.ToString()
        End If
        if Not _rightField Is Nothing Then
            _rightField.UseDatabaseProvider(_databaseProvider)
            Me.Value = _rightField.ToString()
        End If
    End Sub
End Class
