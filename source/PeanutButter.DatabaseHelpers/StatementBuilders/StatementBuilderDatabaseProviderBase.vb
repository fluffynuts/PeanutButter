Public interface IStatementBuilder
    ReadOnly Property OpenObjectQuote() As String
    ReadOnly Property CloseObjectQuote() As String
    ReadOnly Property DatabaseProvider As DatabaseProviders
End Interface
Public MustInherit Class StatementBuilderDatabaseProviderBase
    Implements IStatementBuilder

    Public ReadOnly Property OpenObjectQuote() As String Implements IStatementBuilder.OpenObjectQuote
        Get
            Return _openObjectQuote
        End Get
    End Property
    Public ReadOnly Property CloseObjectQuote As String Implements IStatementBuilder.CloseObjectQuote
        Get
            return _closeObjectQuote
        End Get
    End Property

    Public ReadOnly Property DatabaseProvider() As DatabaseProviders Implements IStatementBuilder.DatabaseProvider
        Get
            Return _databaseProvider
        End Get
    End Property

    Protected _openObjectQuote As String
    Protected _closeObjectQuote As String
    Protected _databaseProvider As DatabaseProviders

    Protected Sub SetDatabaseProvider(provider As DatabaseProviders)
        _databaseProvider = provider
        Select Case provider
            Case DatabaseProviders.Firebird
                _openObjectQuote = """"
                _closeObjectQuote = """"
            Case DatabaseProviders.Unknown
                Throw New Exception("DatabaseProvider configured as Unknown")
            Case Else
                _openObjectQuote = "["
                _closeObjectQuote = "]"
        End Select
    End Sub
End Class
