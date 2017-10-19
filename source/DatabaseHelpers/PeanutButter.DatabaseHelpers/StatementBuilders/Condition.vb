Imports PeanutButter.Utils

Namespace StatementBuilders

' ReSharper disable MemberCanBePrivate.Global
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
' ReSharper disable InconsistentNaming
    Protected Shared ReadOnly _operatorResolutions As ReadOnlyDictionary(Of EqualityOperators, String) = New ReadOnlyDictionary(Of EqualityOperators, String)
' ReSharper restore InconsistentNaming
    Shared Sub New()
      Dim operators As New Dictionary(Of EqualityOperators, String)
      operators(EqualityOperators.Equals) = "="
      operators(EqualityOperators.NotEquals) = "<>"
      operators(EqualityOperators.GreaterThan) = ">"
      operators(EqualityOperators.GreaterThanOrEqualTo) = ">="
      operators(EqualityOperators.LessThan) = "<"
      operators(EqualityOperators.LessThanOrEqualTo) = "<="
      operators(EqualityOperators.Contains) = " like "
      operators(EqualityOperators.StartsWith) = " like "
      operators(EqualityOperators.EndsWith) = " like "
      operators(EqualityOperators.Like) = " like "
      _operatorResolutions = New ReadOnlyDictionary(Of EqualityOperators, String)(operators)
    End Sub

    Public Property FieldName As String
    Public ReadOnly EqualityOperator As EqualityOperators
    Public Property Value As String
    Public ReadOnly QuoteValue As Boolean
    Public ReadOnly LeftConditionIsField As Boolean
    Public ReadOnly RightConditionIsField As Boolean
    Private ReadOnly _leftField As IField
    Private ReadOnly _rightField As IField
    Private ReadOnly _rawString As String

    Public Sub New(fieldName As String, conditionOperator As EqualityOperators, fieldValue As String, Optional quote As Boolean = True, Optional leftConditionIsField As Boolean = True, Optional rightConditionIsField As Boolean = False)
      Me.FieldName = fieldName
      EqualityOperator = conditionOperator
      If fieldValue Is Nothing Then
        Value = "NULL"
        QuoteValue = False
      ElseIf conditionOperator.IsLikeOperator() Then
        Value = fieldValue
        QuoteValue = True
      Else
        Value = fieldValue
        QuoteValue = CBool(IIf((Not quote) Or fieldValue = "?", False, True))
      End If
      Me.LeftConditionIsField = leftConditionIsField
      Me.RightConditionIsField = rightConditionIsField
    End Sub

    Public Sub New(fieldName As String, fieldValue As String, Optional quote As Boolean = True)
      Me.New(fieldName, EqualityOperators.Equals, fieldValue, quote)
    End Sub

    Public Sub New(fieldName as String, op as EqualityOperators, fieldValue as Boolean)
      Me.New(fieldName, op, CInt(IIf(fieldValue, 1, 0)).ToString(), false)
    End Sub

    Public Sub New(field As IField, fieldValue As String, Optional quote As Boolean = True)
      Me.New(field, EqualityOperators.Equals, fieldValue, quote)
    End Sub

    Public Sub New(field As IField, op As EqualityOperators, value As String, Optional quote As Boolean = True)
      _leftField = field
      FieldName = field.ToString()
      EqualityOperator = op
      QuoteValue = quote
      Me.Value = value
      LeftConditionIsField = True
      RightConditionIsField = False
    End Sub

    Public Sub New(leftField As IField, op As EqualityOperators, rightField As IField)
      _leftField = leftField
      _rightField = rightField
      FieldName = leftField.ToString()
      LeftConditionIsField = True
      EqualityOperator = op
      Value = rightField.ToString()
      QuoteValue = False
      RightConditionIsField = True
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

      If (FieldName.IndexOf(_openObjectQuote, StringComparison.Ordinal) < 0) And LeftConditionIsField Then
        parts.Add(FieldQuote(FieldName))
      Else
        parts.Add(FieldName)
      End If
      If Value = "NULL" And Not QuoteValue Then
        parts.Add(ResolveIsOperator())
      Else
        parts.Add(_operatorResolutions(EqualityOperator))
      End If
      If (QuoteValue) Then
        parts.AddRange(New String() {"'", EqualityOperator.LeftWildcard, Value.Replace("'", "''"), EqualityOperator.RightWildcard, "'"})
      ElseIf RightConditionIsField Then
        parts.Add(FieldQuote(Value))
      Else
        parts.Add(Value)
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
      If val.IndexOf(_openObjectQuote, StringComparison.Ordinal) > -1 Then
        Return val
      End If
      Return String.Join("", New String() {_openObjectQuote, val, _closeObjectQuote})
    End Function

    Public Sub New(fieldName As String, conditionOperator As EqualityOperators, fieldValue As Int64)
      Me.New(fieldName, conditionOperator, fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(field as IField, conditionOperator As EqualityOperators, fieldValue As Int64)
      Me.New(field, conditionOperator, fieldValue.ToString(), False)
    End Sub

    Public Sub New(fieldName As String, conditionOperator As EqualityOperators, fieldValue As Decimal)
      Me.New(fieldName, conditionOperator, new DecimalDecorator(fieldValue).ToString(), False, True, False)
    End Sub

    Public Sub New(field as IField, conditionOperator As EqualityOperators, fieldValue As Decimal)
      Me.New(field, conditionOperator, new DecimalDecorator(fieldValue).ToString(), False)
    End Sub

    Public Sub New (fieldName as String, conditionOperator as EqualityOperators, fieldValue as Nullable(of Decimal))
      Me.New(fieldName, conditionOperator, CStr(IIf(fieldValue.HasValue, new DecimalDecorator(fieldValue.Value).ToString(), "NULL")), false)
    End Sub

    Public Sub New (field as IField, conditionOperator as EqualityOperators, fieldValue as Nullable(of Decimal))
      Me.New(field, conditionOperator, CStr(IIf(fieldValue.HasValue, new DecimalDecorator(fieldValue.Value).ToString(), "NULL")), false)
    End Sub

    Public Sub New(fieldName As String, conditionOperator As EqualityOperators, fieldValue As Double)
      Me.New(fieldName, conditionOperator, fieldValue.ToString(), False, True, False)
    End Sub

    Public Sub New(field as IField, conditionOperator As EqualityOperators, fieldValue As Double)
      Me.New(field, conditionOperator, fieldValue.ToString(), False)
    End Sub

    Public Sub New(fieldName As String, conditionOperator As EqualityOperators, fieldValue As DateTime)
      Me.New(fieldName, conditionOperator, fieldValue.ToString("yyyy/MM/dd HH:mm:ss"), True, True, False)
    End Sub

    Public Sub New(field as IField, conditionOperator As EqualityOperators, fieldValue As DateTime)
      Me.New(field, conditionOperator, fieldValue.ToString("yyyy/MM/dd HH:mm:ss"), True)
    End Sub

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements ICondition.UseDatabaseProvider
      SetDatabaseProvider(provider)
      ReevaluateSelectFields()
    End Sub

    Private Sub ReevaluateSelectFields()
      if Not _leftField Is Nothing Then
        _leftField.UseDatabaseProvider(_databaseProvider)
        FieldName = _leftField.ToString()
      End If
      if Not _rightField Is Nothing Then
        _rightField.UseDatabaseProvider(_databaseProvider)
        Value = _rightField.ToString()
      End If
    End Sub

  End Class
End NameSpace