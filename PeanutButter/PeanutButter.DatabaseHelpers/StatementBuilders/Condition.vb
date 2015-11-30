Imports PeanutButter.Utils

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
        <Obsolete("Like_ is deprecated in favour of Contains with a raw Like, StartsWith and Endswith to also help")>
        Like_
        [Like]
        Contains
        StartsWith
        EndsWith
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

#Disable Warning BC40000 ' Type or member is obsolete
        operators(EqualityOperators.Like_) = " like "
#Enable Warning BC40000 ' Type or member is obsolete
        operators(EqualityOperators.Contains) = " like "
        operators(EqualityOperators.StartsWith) = " like "
        operators(EqualityOperators.EndsWith) = " like "
        operators(EqualityOperators.Like) = " like "
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
        Set(_value As String)
            __value = _value
        End Set
    End Property
    Public ReadOnly QuoteValue As Boolean
    Public ReadOnly LeftConditionIsField As Boolean
    Public ReadOnly RightConditionIsField As Boolean
    Private _leftField As IField
    Private _rightField As IField
    Private _rawString As String

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As String, Optional quote As Boolean = True, Optional _leftConditionIsField As Boolean = True, Optional _rightConditionIsField As Boolean = False)
        Me.FieldName = _fieldName
        Me.EqualityOperator = _conditionOperator
        If _fieldValue Is Nothing Then
            Me.Value = "NULL"
            Me.QuoteValue = False
        ElseIf _conditionOperator.IsLikeOperator() Then
            Me.Value = _fieldValue
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

    Public Sub New(field As IField, _fieldValue As String, Optional quote As Boolean = True)
        Me.New(field, EqualityOperators.Equals, _fieldValue, quote)
    End Sub

    Public Sub New(field As IField, op As EqualityOperators, value As String, Optional quote As Boolean = True)
        Me._leftField = field
        Me.FieldName = field.ToString()
        Me.EqualityOperator = op
        Me.QuoteValue = quote
        Me.Value = value
        Me.LeftConditionIsField = True
        Me.RightConditionIsField = False
    End Sub

    Public Sub New(leftField As IField, op As EqualityOperators, rightField As IField)
        Me._leftField = leftField
        Me._rightField = rightField
        Me.FieldName = leftField.ToString()
        Me.LeftConditionIsField = True
        Me.EqualityOperator = op
        Me.Value = rightField.ToString()
        Me.QuoteValue = False
        Me.RightConditionIsField = True
    End Sub

    Public Sub New(leftField As IField, rightField As IField)
        Me.New(leftField, EqualityOperators.Equals, rightField)
    End Sub

    Public Sub New (rawString as String)
        _rawString = rawString
    End Sub

    Public Overrides Function ToString() As String Implements ICondition.ToString
        if _rawString IsNot Nothing
            return _rawString
        End If
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
            parts.AddRange(New String() {"'", EqualityOperator.LeftWildcard, Me.Value.Replace("'", "''"), EqualityOperator.RightWildcard, "'"})
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

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Int64)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(field as IField, _conditionOperator As EqualityOperators, _fieldValue As Int64)
        Me.New(field, _conditionOperator, _fieldValue.ToString(), False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Decimal)
        Me.New(_fieldName, _conditionOperator, new DecimalDecorator(_fieldValue).ToString(), False, True, False)
    End Sub

    Public Sub New(field as IField, _conditionOperator As EqualityOperators, _fieldValue As Decimal)
        Me.New(field, _conditionOperator, new DecimalDecorator(_fieldValue).ToString(), False)
    End Sub

    Public Sub New (_fieldName as String, _conditionOperator as EqualityOperators, _fieldValue as Nullable(of Decimal))
        Me.New(_fieldName, _conditionOperator, CStr(IIf(_fieldValue.HasValue, new DecimalDecorator(_fieldValue.Value).ToString(), "NULL")), false)
    End Sub

    Public Sub New (field as IField, _conditionOperator as EqualityOperators, _fieldValue as Nullable(of Decimal))
        Me.New(field, _conditionOperator, CStr(IIf(_fieldValue.HasValue, new DecimalDecorator(_fieldValue.Value).ToString(), "NULL")), false)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As Double)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(field as IField, _conditionOperator As EqualityOperators, _fieldValue As Double)
        Me.New(field, _conditionOperator, _fieldValue.ToString(), False)
    End Sub

    Public Sub New(_fieldName As String, _conditionOperator As EqualityOperators, _fieldValue As DateTime)
        Me.New(_fieldName, _conditionOperator, _fieldValue.ToString("yyyy/MM/dd HH:mm:ss"), True, True, False)
    End Sub

    Public Sub New(field as IField, _conditionOperator As EqualityOperators, _fieldValue As DateTime)
        Me.New(field, _conditionOperator, _fieldValue.ToString("yyyy/MM/dd HH:mm:ss"), True)
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
