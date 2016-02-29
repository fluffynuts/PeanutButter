Imports PeanutButter.Utils

Public class ConcatenatedField
    Inherits StatementBuilderBase
    Implements IField

    Private _fields As IField()
    Private _alias As IField

    Public Sub New(_fieldAlias as String, ParamArray otherFields() as String)
        if otherFields.IsEmpty() Then
            Throw new ArgumentException("At least one filed must be provided for a Concatenated field")
        End If
        _alias = new SelectField(_fieldAlias)
        _fields = otherFields.Select(Function(f) New SelectField(f)).ToArray()
    End Sub

    Public Sub New(_fieldAlias as String, ParamArray otherFields() as IField)
        if otherFields.IsEmpty() Then
            Throw new ArgumentException("At least one filed must be provided for a Concatenated field")
        End If
        _alias = new SelectField(_fieldAlias)
        _fields = otherFields
    End Sub

    Public Overrides Function ToString() As String Implements IField.ToString
        Dim parts = new List(Of String)
        For Each field in _fields
            field.UseDatabaseProvider(_databaseProvider)
            parts.Add(field.ToString())
        Next
        Return string.Join("+", parts) + " as " + _alias.ToString()
    End Function

    Public Sub UseDatabaseProvider(provider As DatabaseProviders) Implements IField.UseDatabaseProvider
        SetDatabaseProvider(provider)
    End Sub
End Class