#If NETSTANDARD Then
#Else
' ReSharper disable UnusedMember.Global
Imports System.Data.Common
Imports System.Data.OleDb
Imports PeanutButter.DatabaseHelpers.StatementBuilders

Namespace Executors

  Public Interface IDataReaderBuilder
    Inherits IDisposable
    Function Build() As DbDataReader
    Function WithSql(sql As String) As IDataReaderBuilder
    Function WithSql(statementBuilder As ISelectStatementBuilder) As IDataReaderBuilder
    Function WithConnectionFactory(connectionResolver as Func(Of IDbConnection)) as IDataReaderBuilder
  End Interface

  Public Class DataReaderBuilder
    Implements IDataReaderBuilder
    Implements IDisposable

    Public Shared Function Create() As DataReaderBuilder
      Return New DataReaderBuilder()
    End Function

    Private _sql As String
    Private _selectBuilder As ISelectStatementBuilder
    Private ReadOnly _readers As New List(Of DbDataReader)
    Private _connectionResolver As Func(Of IDbConnection)

    Public Function Build() As DbDataReader Implements IDataReaderBuilder.Build
      Dim sql = GetSQLString()
      If sql Is Nothing Then
        Throw New ArgumentException(Me.GetType().Name & " :: sql not set and no builder provided", "sql")
      End If

      If _connectionResolver Is Nothing Then
        Throw New ArgumentException("No connection resolver defined for DbDataReader.Build", "_connectionResolver")
      End If
      Dim connection = _connectionResolver()

      Dim command = New OleDbCommand()
      command.Connection = TryCast(connection, OleDbConnection)
      command.CommandText = sql
      Return command.ExecuteReader
    End Function

#Disable Warning InconsistentNaming
    Private Function GetSQLString() As String
#Enable Warning InconsistentNaming
      If _sql Is Nothing Then
        If _selectBuilder Is Nothing Then
          Return Nothing
        End If
        Return _selectBuilder.Build()
      End If
      Return _sql
    End Function

    Public Function WithSql(sql As String) As IDataReaderBuilder Implements IDataReaderBuilder.WithSql
      _sql = sql
      _selectBuilder = Nothing
      Return Me
    End Function

    Public Function WithSql(statementBuilder As ISelectStatementBuilder) As IDataReaderBuilder Implements IDataReaderBuilder.WithSql
      _sql = Nothing
      _selectBuilder = statementBuilder
      Return Me
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
      SyncLock Me
        For Each reader In _readers
          If Not reader.IsClosed Then
            reader.Close()
          End If
        Next
      End SyncLock
    End Sub

    Public Function WithConnectionFactory(connectionResolver As Func(Of IDbConnection)) As IDataReaderBuilder Implements IDataReaderBuilder.WithConnectionFactory
      _connectionResolver = connectionResolver
      Return Me
    End Function
  End Class
End NameSpace
#End If