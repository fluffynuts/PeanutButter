#If NETSTANDARD Then
#Else
' ReSharper disable UnusedMember.Global
' ReSharper disable UnusedMemberInSuper.Global
Imports System.Data.OleDb
Imports PeanutButter.DatabaseHelpers.StatementBuilders

Namespace Executors

  Public Class InsertFailsException
    Inherits Exception
    Public Sub New(sql As String, originalException As Exception)
      MyBase.New(String.Join("", {
                                   "Unable to perform insert with the following sql:",
                                   vbCrLf,
                                   vbCrLf,
                                   sql,
                                   vbCrLf,
                                   vbCrLf,
                                   "Original exception is:",
                                   vbCrLf,
                                   originalException.Message
                                 }))
    End Sub
  End Class
  Public Interface IScalarExecutorBuilder
    Function WithSql(sql As String) As IScalarExecutorBuilder
    Function WithSql(builder As IUpdateStatementBuilder) As IScalarExecutorBuilder
    Function WithSql(builder As IInsertStatementBuilder) As IScalarExecutorBuilder
    Function WithSql(builder As IDeleteStatementBuilder) As IScalarExecutorBuilder
    Function WithConnectionFactory(connectionFactory As Func(Of IDbConnection)) As IScalarExecutorBuilder
    Function Execute() As Object
    Function ExecuteInsert() As Object
  End Interface
  Public Class ScalarExecutorBuilder
    Implements IScalarExecutorBuilder

    Public Shared Function Create() As ScalarExecutorBuilder
      Return New ScalarExecutorBuilder()
    End Function
    Private _updateBuilder As IUpdateStatementBuilder
    Private _insertBuilder As IInsertStatementBuilder
    Private _deleteBuilder As IDeleteStatementBuilder
    Private _sql As String
    Private _connFactory As Func(Of IDbConnection)

    Public Overridable Function Execute() As Object Implements IScalarExecutorBuilder.Execute
      Dim sqlStatement = "(not set)"
      Try
        If _connFactory Is Nothing Then
          throw new Exception("No connection factory defined for ScalarExecutorBuilder.Execute")
        End If
        sqlStatement = GetSqlString()
        If sqlStatement Is Nothing Then Throw New ArgumentException([GetType]().Name, $":: sql not set")
        Dim cmd = New OleDbCommand
        Dim conn = _connFactory()
        Dim transaction = conn.BeginTransaction
        cmd.Connection = CType(conn, OleDbConnection)
        cmd.CommandText = sqlStatement
        cmd.Transaction = CType(transaction, OleDbTransaction)
        Dim result = cmd.ExecuteScalar()
        cmd.Transaction.Commit()
        Return result
      Catch ex As Exception
        Throw New InsertFailsException(sqlStatement, ex)
      End Try
    End Function

    Private Function GetSqlString() As String
      If _sql IsNot Nothing Then Return _sql
      If _updateBuilder IsNot Nothing Then Return _updateBuilder.Build()
      If _insertBuilder IsNot Nothing Then Return _insertBuilder.Build()
      If _deleteBuilder IsNot Nothing Then Return _deleteBuilder.Build()
      Return Nothing
    End Function

    Public Overridable Function ExecuteInsert() As Object Implements IScalarExecutorBuilder.ExecuteInsert
      Execute()
      _sql = "select @@IDENTITY"
      Return Execute()
    End Function

    Public Overridable Function WithConnectionFactory(connFactory As Func(Of IDbConnection)) As IScalarExecutorBuilder Implements IScalarExecutorBuilder.WithConnectionFactory
      _connFactory = connFactory
      Return Me
    End Function

    Public Overridable Function WithSql(sql As String) As IScalarExecutorBuilder Implements IScalarExecutorBuilder.WithSql
      SetSQLSource(sql, Nothing, Nothing, Nothing)
      Return Me
    End Function

    ' TESTME
    Public Overridable Function WithSql(builder As IUpdateStatementBuilder) As IScalarExecutorBuilder Implements IScalarExecutorBuilder.WithSql
      SetSQLSource(Nothing, builder, Nothing, Nothing)
      Return Me
    End Function

    ' TESTME
    Public Overridable Function WithSql(builder As IInsertStatementBuilder) As IScalarExecutorBuilder Implements IScalarExecutorBuilder.WithSql
      SetSQLSource(Nothing, Nothing, builder, Nothing)
      Return Me
    End Function

    ' TESTME
    Public Overridable Function WithSql(builder As IDeleteStatementBuilder) As IScalarExecutorBuilder Implements IScalarExecutorBuilder.WithSql
      SetSQLSource(Nothing, Nothing, Nothing, builder)
      Return Me
    End Function

' ReSharper disable InconsistentNaming
    Private Sub SetSQLSource(str As String, up As IUpdateStatementBuilder, ins As IInsertStatementBuilder, del As IDeleteStatementBuilder)
' ReSharper restore InconsistentNaming
      _sql = str
      _updateBuilder = up
      _insertBuilder = ins
      _deleteBuilder = del
    End Sub
  End Class

' ReSharper restore UnusedMember.Global
' ReSharper restore UnusedMemberInSuper.Global
End NameSpace
#End If