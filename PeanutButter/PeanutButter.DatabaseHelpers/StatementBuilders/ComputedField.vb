

Public Class ComputedField
    Inherits StatementBuilderBase
    Implements IField
    Public Enum ComputeFunctions
        Max
        Min
        Coalesce
        Count
    End Enum
    Public FieldName As String
    Public ComputeFunction As ComputeFunctions
    Public FieldAlias As String
    public Sub New(field as IField, _computeFunction as ComputeFunctions, Optional _alias As String = Nothing)
        Me.New(field.ToString(), _computeFunction, _alias)
    End Sub

    Public Sub New(_fieldName As String, _computeFunction As ComputeFunctions, Optional _alias As String = Nothing)
        FieldName = _fieldName
        ComputeFunction = _computeFunction
        FieldAlias = CStr(IIf(_alias Is Nothing, _fieldName, _alias))
    End Sub

    Public Overrides Function ToString() As String Implements IField.ToString
        Dim parts = New List(Of String)
        parts.Add(Me.ComputeFunction.ToString())
        parts.Add("(")
        parts.Add(_openObjectQuote)
        parts.Add(FieldName)
        parts.Add(_closeObjectQuote)
        parts.Add(")")
        parts.Add(" as ")
        parts.Add(_openObjectQuote)
        parts.Add(FieldAlias)
        parts.Add(_closeObjectQuote)
        Return String.Join("", parts)
    End Function

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements IField.UseDatabaseProvider
        MyBase.SetDatabaseProvider(provider)
    End Sub
End Class