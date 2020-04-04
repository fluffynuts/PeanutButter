' ReSharper disable MemberCanBePrivate.Global
Namespace StatementBuilders

  Public Interface IDataCopyStatementBuilder
    Function WithSourceTable(table As String) As IDataCopyStatementBuilder
    Function WithTargetTable(table As String) As IDataCopyStatementBuilder
    Function WithFieldMapping(sourceName As String, Optional destName As String = Nothing) As IDataCopyStatementBuilder
    Function WithCriteria(criteria As String) as IDataCopyStatementBuilder
    Function WithDatabaseProvider(provider As DatabaseProviders) As IDataCopyStatementBuilder
    Function Build() As String
  End Interface

  Public Class DataCopyStatementBuilder
    Inherits StatementBuilderDatabaseProviderBase
    Implements IDataCopyStatementBuilder
    Private _sourceTable As String
    Private _targetTable As String
    Private ReadOnly _fieldMappings as List(Of FieldMapping) = New List(Of FieldMapping)
    Private _criteria As String

    public Sub New
      SetDatabaseProvider(DatabaseProviders.Access)
    End Sub

    Private Class FieldMapping
      Public Source As String
      Public Target As String
      Public Sub New(newSource As String, Optional newTarget As String = Nothing)
        Source = newSource
        If newTarget is Nothing Then
          Target = newSource
        Else
          Target = newTarget
        End If
      End Sub
    End Class

    Public Shared Function Create() As IDataCopyStatementBuilder
      Return New DataCopyStatementBuilder()
    End Function

    Public Function Build() As String Implements IDataCopyStatementBuilder.Build
      CheckParameters()
      Dim parts = New List(Of String)
      parts.Add("insert into ")
      parts.Add(_openObjectQuote)
      parts.Add(_targetTable)
      parts.Add(_closeObjectQuote + " ")
      AddFieldsTo(parts)
      parts.Add(" from ")
      parts.Add(_openObjectQuote)
      parts.Add(_sourceTable)
      parts.Add(_closeObjectQuote)
      If Not _criteria Is Nothing Then
        parts.Add(" where ")
        parts.Add(_criteria)
      End If
      Return String.Join("", parts)
    End Function

    Private Sub AddFieldsTo(list As List(Of String))
      Dim pre = New List(Of String),
          post = New List(Of String)
      pre.Add("(")
      post.Add(") select ")

      Dim notFirst = False
      For Each mapping As FieldMapping In _fieldMappings
        If notFirst Then
          pre.Add(",")
          post.Add(",")
        End If
        notFirst = True
        pre.Add(_openObjectQuote + mapping.Target + _closeObjectQuote)
        post.Add(_openObjectQuote + mapping.Source + _closeObjectQuote)
      Next
      list.AddRange(pre)
      list.AddRange(post)
    End Sub

    Private Sub CheckParameters()
      If _sourceTable Is Nothing Then
        Throw New ArgumentException([GetType]().Name + ": source table not set")
      End If
      If _targetTable Is Nothing Then
        Throw New ArgumentException([GetType]().Name + ": target table not set")
      End If
      If _fieldMappings.Count = 0 Then
        Throw New ArgumentException([GetType]().Name + ": no fields set")
      End If
    End Sub

    Public Function WithCriteria(criteria As String) As IDataCopyStatementBuilder Implements IDataCopyStatementBuilder.WithCriteria
      _criteria = criteria
      Return Me
    End Function

    Public Function WithTargetTable(table As String) As IDataCopyStatementBuilder Implements IDataCopyStatementBuilder.WithTargetTable
      _targetTable = table
      Return Me
    End Function

    Public Function WithFieldMapping(sourceName As String, Optional destName As String = Nothing) As IDataCopyStatementBuilder Implements IDataCopyStatementBuilder.WithFieldMapping
      _fieldMappings.Add(New FieldMapping(sourceName, destName))
      Return Me
    End Function

    Public Function WithSourceTable(table As String) As IDataCopyStatementBuilder Implements IDataCopyStatementBuilder.WithSourceTable
      _sourceTable = table
      Return Me
    End Function

    Public Function WithDatabaseProvider(provider As DatabaseProviders) As IDataCopyStatementBuilder Implements IDataCopyStatementBuilder.WithDatabaseProvider
      SetDatabaseProvider(provider)
      Return Me
    End Function
  End Class
End NameSpace