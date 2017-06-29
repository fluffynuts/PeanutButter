Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

<TestFixture()>
Public Class TestConnectionStringBuilder
    <Test()>
    Public Sub Create_ShouldReturnNewBuilder()
        Dim result = ConnectionStringBuilder.Create()
        Assert.IsInstanceOf(Of ConnectionStringBuilder)(result)
    End Sub

    <Test()>
    Public Sub Build_ShouldProduceExpectedConnectionString()
        Dim provider = RandomValueGen.GetRandomString(4)
        Dim source = RandomValueGen.GetRandomString(4)
        Dim expected = "Provider=" & provider & ";Data Source=" & source

        Dim result = ConnectionStringBuilder.Create() _
                        .WithProvider(provider) _
                        .WithSource(source) _
                        .Build()
        Assert.AreEqual(result, expected)
    End Sub
End Class
