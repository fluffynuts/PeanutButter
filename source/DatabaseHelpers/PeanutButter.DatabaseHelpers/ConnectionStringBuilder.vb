Public Interface IConnectionStringBuilder
    Function Build() As String
    Function WithProvider(provider As String) As IConnectionStringBuilder
    Function WithSource(source As String) As IConnectionStringBuilder
End Interface
Public Class ConnectionStringBuilder
    Implements IConnectionStringBuilder
    Public Shared Function Create() As ConnectionStringBuilder
        Return New ConnectionStringBuilder()
    End Function
    Private _provider As String
    Private _source As String
    Public Function Build() As String Implements IConnectionStringBuilder.Build
        CheckParameters()
        Dim result = New List(Of String)
        result.Add("Provider=")
        result.Add(_provider)
        result.Add(";")
        result.Add("Data Source=")
        result.Add(_source)
        Return String.Join("", result)
    End Function
    Private Sub CheckParameters()
        If _provider Is Nothing Then
            Throw New ArgumentException("ConnectionStringBuilder: provider not set")
        End If
        If _source Is Nothing Then
            Throw New ArgumentException("ConnectionStringBuilder: source not set")
        End If
    End Sub
    Public Overridable Function WithProvider(provider As String) As IConnectionStringBuilder Implements IConnectionStringBuilder.WithProvider
        _provider = provider
        Return Me
    End Function
    Public Function WithSource(source As String) As IConnectionStringBuilder Implements IConnectionStringBuilder.WithSource
        _source = source
        Return Me
    End Function

    Public Function WithJetProvider() As IConnectionStringBuilder
        Return WithProvider("Microsoft.Jet.OLEDB.4.0")
    End Function

End Class
