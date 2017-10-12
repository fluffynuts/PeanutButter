Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestConditionChain
        <TestCase(CompoundCondition.BooleanOperators.OperatorAnd)>
        Public Sub CreateConditionChain_ProducesExpectedChainString(op As CompoundCondition.BooleanOperators)
            Dim leftLeft = New Condition("col1", Condition.EqualityOperators.Equals, "val1")
            Dim leftRight = New Condition("col2", Condition.EqualityOperators.Equals, "val2")
            Dim rightLeft = New Condition("col3", Condition.EqualityOperators.Equals, "val3")
            Dim rightRight = New Condition("col4", Condition.EqualityOperators.Equals, "val4")
            Dim cc = New ConditionChain(op, leftLeft, leftRight, rightLeft, rightRight)
            Dim opString = CStr(IIf(op = CompoundCondition.BooleanOperators.OperatorAnd, " and ", " or "))
            Assert.AreEqual("([col1]='val1'" + opString + "[col2]='val2'" + opString + "[col3]='val3'" + opString + "[col4]='val4')", cc.ToString())
        End Sub


    End Class
End NameSpace