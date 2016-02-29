Imports System.Runtime.CompilerServices
Imports NSubstitute

Module ISelectStatementBuilderExtensions
    <Extension()>
    Public Function HasAllConditions(ByVal builder As ISelectStatementBuilder, ParamArray expectedConditions As ICondition())
        If Not TryCast(builder, SelectStatementBuilder) Is Nothing Then Throw New Exception("HasAllConditions extension method is not meant for concrete implementation of SelectStatementBuilder")
        Dim conditionCall = builder.ReceivedCalls().FirstOrDefault(Function(theCall)
                                                                       Dim args = theCall.GetArguments()
                                                                       If args.Length = 0 Then Return False
                                                                       Dim c = TryCast(args(0), ICondition())
                                                                       Return Not c Is Nothing
                                                                   End Function)
        If conditionCall Is Nothing Then Return False
        Dim conditions = TryCast(conditionCall.GetArguments()(0), ICondition())
        If conditions Is Nothing Then Return False
        Return expectedConditions.All(Function(thisExpectedCondition)
                                          Dim stringCondition = thisExpectedCondition.ToString()
                                          Return conditions.Any(Function(thisActualCondition)
                                                                    Return thisActualCondition.ToString() = stringCondition
                                                                End Function)
                                      End Function)

    End Function

    <Extension()>
    Public Function HasCondition(ByVal builder As ISelectStatementBuilder, expectedfieldName As String, expectedOperator As Condition.EqualityOperators, expectedValue As String)
        Dim conditionCall = builder.ReceivedCalls().FirstOrDefault(Function(theCall)
                                                                       Dim args = theCall.GetArguments()
                                                                       If args.Length <> 3 Then Return False
                                                                       Try
                                                                           If expectedfieldName <> DirectCast(args(0), String) Then Return False
                                                                           If expectedOperator <> DirectCast(args(1), Condition.EqualityOperators) Then Return False
                                                                           Return expectedValue = DirectCast(args(2), String)
                                                                       Catch ex As Exception
                                                                           Return False
                                                                       End Try
                                                                   End Function)
        Return Not conditionCall Is Nothing
    End Function

    <Extension()>
    Public Function HasCondition(ByVal builder As ISelectStatementBuilder, expectedFieldName As String, expectedOperator As Condition.EqualityOperators, expectedValue As Long)
        Dim valueMatcher As Func(Of Object, Boolean) = Function(obj)
                                                           Try
                                                               Dim intVal = DirectCast(obj, Integer)
                                                               Return intVal = expectedValue
                                                           Catch ex As Exception
                                                               Return False
                                                           End Try
                                                       End Function
        Return HasCondition(builder, expectedFieldName, expectedOperator, valueMatcher)
    End Function

    Public Function HasCondition(ByVal builder As ISelectStatementBuilder, expectedFieldName As String, expectedOperator As Condition.EqualityOperators, valueComparer As Func(Of Object, Boolean))
        Dim conditionCall = builder.ReceivedCalls().FirstOrDefault(Function(theCall)
                                                                       Dim args = theCall.GetArguments()
                                                                       If args.Length <> 3 Then Return False
                                                                       Try
                                                                           If expectedFieldName <> DirectCast(args(0), String) Then Return False
                                                                           If expectedOperator <> DirectCast(args(1), Condition.EqualityOperators) Then Return False
                                                                           Return valueComparer(args(2))
                                                                       Catch ex As Exception
                                                                           Return False
                                                                       End Try
                                                                   End Function)
        Return Not conditionCall Is Nothing
    End Function
End Module
