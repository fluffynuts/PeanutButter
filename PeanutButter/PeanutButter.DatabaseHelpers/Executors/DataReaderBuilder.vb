Imports System.Data.OleDb
Imports System.Data.Common

Public Interface IDataReaderBuilder
    Inherits IDisposable
    Function Build() As DbDataReader
    Function WithSql(sql As String) As IDataReaderBuilder
    Function WithSql(statementBuilder As ISelectStatementBuilder) As IDataReaderBuilder
    Function WithConnection(conn As IDbConnection) As IDataReaderBuilder
End Interface

Public Class DataReaderBuilder
    Implements IDataReaderBuilder
    Implements IDisposable

    Public Shared Function Create() As DataReaderBuilder
        Return New DataReaderBuilder()
    End Function

    Private _conn As IDbConnection
    Private _sql As String
    Private _selectBuilder As ISelectStatementBuilder
    Dim _readers As New List(Of DbDataReader)

    Public Function Build() As DbDataReader Implements IDataReaderBuilder.Build
        Dim sql = Me.GetSQLString()
        If sql Is Nothing Then
            Throw New ArgumentException(Me.GetType().Name + " :: sql not set and no builder provided")
        End If
        If _conn Is Nothing Then
            Throw new Exception("No connection defined for DbDataReader.Build")
        End If

        Dim command = New OleDbCommand()
        command.Connection = TryCast(_conn, OleDbConnection)
        command.CommandText = sql
        Return command.ExecuteReader
    End Function

    Private Function GetSQLString() As String
        If _sql Is Nothing Then
            If _selectBuilder Is Nothing Then
                Return Nothing
            End If
            Return _selectBuilder.Build()
        End If
        Return _sql
    End Function

    Public Function WithConnection(conn As IDbConnection) As IDataReaderBuilder Implements IDataReaderBuilder.WithConnection
        _conn = conn
        Return Me
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

End Class
