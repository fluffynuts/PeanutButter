Imports NSubstitute
Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators

<TestFixture()>
Public Class TestComputedField
    <Test()>
    Public Sub Construct_GivenFieldAndFunction_ShouldConstructWithEmptyAlias()
        Dim sut = new ComputedField(Substitute.For(Of IField), ComputedField.ComputeFunctions.Coalesce)
        Assert.AreEqual("", sut.FieldAlias)
    End Sub

    <Test()>
    Public Sub Construct_GivenFieldNameAndFunction_ShouldConstructWithEmptyAlias()
        Dim fieldName = RandomValueGen.GetRandomString(4)
        Dim computedFunction = RandomValueGen.GetRandomEnum(Of ComputedField.ComputeFunctions)()
        Dim sut = new ComputedField(fieldName, computedFunction)
        Assert.AreEqual("", sut.FieldAlias)
        Assert.AreEqual(sut.FieldName, fieldName)
        Assert.AreEqual(sut.ComputeFunction, computedFunction)
    End Sub

    <Test()>
    Public Sub Construct_Given_FieldAndComputeFunctionAndAlias_ShouldConstructWithFieldNameInAlias()
        Dim fieldName = RandomValueGen.GetRandomString(4)
        Dim fieldAlias = RandomValueGen.GetRandomString(4)
        Dim computeFunction = RandomValueGen.GetRandomEnum(Of ComputedField.ComputeFunctions)()
        Dim field = Substitute.For(Of IField)()
        field.ToString.Returns(fieldName)

        Dim sut = new ComputedField(field, computeFunction, fieldAlias)

        Assert.AreEqual(sut.FieldAlias, fieldAlias)
        Assert.AreEqual(sut.ComputeFunction, computeFunction)
        Assert.AreEqual(sut.FieldName, fieldName)

    End Sub

End Class
