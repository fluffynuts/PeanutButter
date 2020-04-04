

' ReSharper disable once CheckNamespace
Namespace StatementBuilders
    Public Class ComputedField
        Inherits StatementBuilderBase
        Implements IField

        Public Enum ComputeFunctions
            Max
            Min
            Coalesce
            Count
        End Enum
' ReSharper disable FieldCanBeMadeReadOnly.Global
' ReSharper disable MemberCanBePrivate.Global
        Public FieldName As String
        Public ComputeFunction As ComputeFunctions
        Public FieldAlias As String
' ReSharper restore FieldCanBeMadeReadOnly.Global
' ReSharper disable once UnusedMember.Global
        public Sub New(field as IField, computeFunction as ComputeFunctions)
            Me.New(field, computeFunction, string.Empty)
        End Sub

        public Sub New(field as IField, computeFunction as ComputeFunctions, fieldAlias As String)
            Me.New(field.ToString(), computeFunction, fieldAlias)
        End Sub

        Public Sub New(fieldName As String, computeFunction As ComputeFunctions)
            Me.New(fieldName, computeFunction, string.Empty)
        End Sub
' ReSharper restore MemberCanBePrivate.Global

        Public Sub New(fieldName As String, computeFunction As ComputeFunctions, fieldAlias As String)
            Me.FieldName = fieldName
            Me.ComputeFunction = computeFunction
            If fieldAlias is Nothing Then
                Me.FieldAlias = fieldName
            Else
                Me.FieldAlias = fieldAlias
            End If
        End Sub

        Public Overrides Function ToString() As String Implements IField.ToString
            Dim parts = New List(Of String)
            parts.Add(ComputeFunction.ToString())
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
            SetDatabaseProvider(provider)
        End Sub
    End Class
End NameSpace