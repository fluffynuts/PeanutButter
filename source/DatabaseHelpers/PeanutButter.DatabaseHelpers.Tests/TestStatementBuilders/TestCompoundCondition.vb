Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestCompoundCondition
        <TestCase(Condition.EqualityOperators.Equals, Condition.EqualityOperators.Equals, CompoundCondition.BooleanOperators.OperatorAnd)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo, Condition.EqualityOperators.GreaterThanOrEqualTo, CompoundCondition.BooleanOperators.OperatorOr)>
        Public Sub Construct_GivenTwoConditionAndOperator_StoresValuesInProperties(leftOp As Condition.EqualityOperators, _
                                                                                   rightOp As Condition.EqualityOperators, _
                                                                                   boolOp As CompoundCondition.BooleanOperators)

            Dim left = New Condition(RandomValueGen.GetRandomString(), leftOp, RandomValueGen.GetRandomString())
            Dim right = New Condition(RandomValueGen.GetRandomString(), rightOp, RandomValueGen.GetRandomString())
            Dim cc = New CompoundCondition(left, boolOp, right)
            Assert.AreEqual(cc.LeftCondition, left)
            Assert.AreEqual(cc.RightCondition, right)
            Assert.AreEqual(cc.LogicalOperator, boolOp)
        End Sub
        <TestCase(Condition.EqualityOperators.Equals, Condition.EqualityOperators.Equals, CompoundCondition.BooleanOperators.OperatorAnd)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo, Condition.EqualityOperators.GreaterThanOrEqualTo, CompoundCondition.BooleanOperators.OperatorOr)>
        Public Sub Construct_GivenTwoConditionAndOperator_ToStringProducesExpectedValue(leftOp As Condition.EqualityOperators, _
                                                                                        rightOp As Condition.EqualityOperators, _
                                                                                        boolOp As CompoundCondition.BooleanOperators)
            Dim left = New Condition(RandomValueGen.GetRandomString(), leftOp, RandomValueGen.GetRandomString())
            Dim right = New Condition(RandomValueGen.GetRandomString(), rightOp, RandomValueGen.GetRandomString())
            Dim cc = New CompoundCondition(left, boolOp, right)
            Assert.AreEqual("(" + left.ToString() + " " + CompoundCondition.OperatorResolutions(boolOp) + " " + right.ToString() + ")", cc.ToString())
        End Sub

        Private Function RS() As String
            Return RandomValueGen.GetRandomString()
        End Function

        <TestCase(CompoundCondition.BooleanOperators.OperatorAnd)>
        <TestCase(CompoundCondition.BooleanOperators.OperatorOr)>
        Public Sub Construct_GivenTwoCompoundConditions_ToStringProducesExpectedValue(logicalOp As CompoundCondition.BooleanOperators)
            Dim left = New CompoundCondition(New Condition(RS(), Condition.EqualityOperators.Equals, RS()), _
                                             CompoundCondition.BooleanOperators.OperatorAnd, _
                                             New Condition(RS(), Condition.EqualityOperators.GreaterThanOrEqualTo, RS()))
            Dim right = New CompoundCondition(New Condition(RS(), Condition.EqualityOperators.Equals, RS()), _
                                              CompoundCondition.BooleanOperators.OperatorAnd, _
                                              New Condition(RS(), Condition.EqualityOperators.GreaterThanOrEqualTo, RS()))
            Dim cc = New CompoundCondition(left, logicalOp, right)
            Assert.AreEqual("(" + left.ToString() + " " + CompoundCondition.OperatorResolutions(logicalOp) + " " + right.ToString() + ")", cc.ToString())
        End Sub

        <TestCase(CompoundCondition.BooleanOperators.OperatorOr)>
        <TestCase(CompoundCondition.BooleanOperators.OperatorAnd)>
        Public Sub Construct_GivenTwoKnownCompoundConditions_ToStringProducesExpectedValue(op As CompoundCondition.BooleanOperators)
            Dim left = New CompoundCondition(New Condition("col1", Condition.EqualityOperators.Equals, "val1"), _
                                             CompoundCondition.BooleanOperators.OperatorAnd, _
                                             New Condition("col2", Condition.EqualityOperators.Equals, "val2"))
            Dim right = New CompoundCondition(New Condition("col3", Condition.EqualityOperators.Equals, "val3"), _
                                              CompoundCondition.BooleanOperators.OperatorAnd, _
                                              New Condition("col4", Condition.EqualityOperators.Equals, "val4"))
            Dim cc = New CompoundCondition(left, op, right)
            Dim interOp = CStr(IIf(op = CompoundCondition.BooleanOperators.OperatorAnd, " and ", " or "))
            Assert.AreEqual("(([col1]='val1' and [col2]='val2')" + interOp + "([col3]='val3' and [col4]='val4'))", cc.ToString())
        End Sub


    End Class
End NameSpace