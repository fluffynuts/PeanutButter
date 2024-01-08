Imports NUnit.Framework
Imports PeanutButter.RandomGenerators
Imports NExpect
Imports NExpect.Expectations

<TestFixture()>
Public Class TestConnectionStringBuilder
    <Test()>
    Public Sub Create_ShouldReturnNewBuilder()
        Dim result = ConnectionStringBuilder.Create()
        Expect(result) _
            .To.Be.An.Instance.Of (Of ConnectionStringBuilder)
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
        Expect(result) _
            .To.Equal(expected)
    End Sub
End Class
