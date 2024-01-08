Imports NSubstitute
Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators
Imports NExpect
Imports NExpect.Expectations

<TestFixture()>
Public Class TestComputedField
    <Test()>
    Public Sub Construct_GivenFieldAndFunction_ShouldConstructWithEmptyAlias()
        Dim sut = new ComputedField(Substitute.For (Of IField), ComputedField.ComputeFunctions.Coalesce)
        Expect(sut.FieldAlias) _
            .To.Be.Empty()
    End Sub

    <Test()>
    Public Sub Construct_GivenFieldNameAndFunction_ShouldConstructWithEmptyAlias()
        Dim fieldName = RandomValueGen.GetRandomString(4)
        Dim computedFunction = RandomValueGen.GetRandomEnum (Of ComputedField.ComputeFunctions)()
        Dim sut = new ComputedField(fieldName, computedFunction)
        Expect(sut.FieldAlias) _
            .To.Be.Empty()
        Expect(sut.FieldName) _
            .To.Equal(fieldName)
        Expect(sut.ComputeFunction) _
            .To.Equal(computedFunction)
    End Sub

    <Test()>
    Public Sub Construct_Given_FieldAndComputeFunctionAndAlias_ShouldConstructWithFieldNameInAlias()
        Dim fieldName = RandomValueGen.GetRandomString(4)
        Dim fieldAlias = RandomValueGen.GetRandomString(4)
        Dim computeFunction = RandomValueGen.GetRandomEnum (Of ComputedField.ComputeFunctions)()
        Dim field = Substitute.For (Of IField)()
        field.ToString.Returns(fieldName)

        Dim sut = new ComputedField(field, computeFunction, fieldAlias)

        Expect(sut.FieldAlias) _
            .To.Equal(fieldAlias)
        Expect(sut.ComputeFunction) _
            .To.Equal(computeFunction)
        Expect(sut.FieldName) _
            .To.Equal(fieldName)
    End Sub
End Class
