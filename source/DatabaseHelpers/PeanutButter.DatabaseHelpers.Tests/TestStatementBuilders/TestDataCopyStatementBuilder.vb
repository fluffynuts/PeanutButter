Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators
Imports NExpect
Imports NExpect.Expectations
Imports NSubstitute.ExceptionExtensions
Imports PeanutButter.RandomGenerators.RandomValueGen

Namespace TestStatementBuilders
    <TestFixture()>
    Public Class TestDataCopyStatementBuilder
        <Test()>
        Public Sub Create_ShouldReturnNewInstanceOfBuilder()
            Dim builder1 = DataCopyStatementBuilder.Create(),
                builder2 = DataCopyStatementBuilder.Create()
            Expect(builder1) _
                .Not.To.Equal(builder2)
            Expect(builder1) _
                .To.Be.An.Instance.Of (Of IDataCopyStatementBuilder)
            Expect(builder2) _
                .To.Be.An.Instance.Of (Of IDataCopyStatementBuilder)
        End Sub

        Private Function Create() as IDataCopyStatementBuilder
            Return DataCopyStatementBuilder.Create()
        End Function

        <Test()>
        Public Sub Build_GivenNoSourceTable_ShouldThrow()
            Assert.That(
                (Function() as String
                    Return Create().Build()
                End Function),
                NUnit.Framework.Throws.Exception.InstanceOf(Of ArgumentException).With.Message.Contains("source table not set")
                )
        End Sub

        <Test()>
        Public Sub Build_GivenSourceTableAndNoTargetTable_ShouldThrow()
            Assert.That(
                (Function() as string
                    return Create() _
                           .WithSourceTable(GetRandomString(1)) _
                           .Build()
                End Function),
                NUnit.Framework.Throws.Exception.InstanceOf (Of ArgumentException).With.Message.Contains("target table not set")
                )
        End Sub

        <Test()>
        Public Sub Build_GivenSourceAndTargetTableButNoFieldMappings_ShouldThrow()
            Assert.That(
                (Function() as String
                    return Create() _
                           .WithSourceTable(GetRandomString(1)) _
                           .WithTargetTable(GetRandomString(1)) _
                           .Build()
                End Function),
                NUnit.Framework.Throws.Exception.InstanceOf (Of ArgumentException).With.Message.Contains("no fields set")
                )
        End Sub

        <Test()>
        Public Sub Build_GivenSourceTargetAndOneFieldMapping_ShouldReturnExpectedString()
            Dim src = GetRandomString(1, 5),
                target = GetRandomString(1, 5),
                srcField = GetRandomString(1, 5),
                targetField = GetRandomString(1, 5)
            Dim result = Create() _
                    .WithSourceTable(src) _
                    .WithTargetTable(target) _
                    .WithFieldMapping(srcField, targetField) _
                    .Build()
            Dim expected = String.Join("", New String() { _
                                                            "insert into [", target, "] ([", targetField, "]) select [",
                                                            srcField, "] from [", src, "]"
                                                        })
            Expect(result) _
                .To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenSOurceTargetAndTwoFieldMappings_ShouldReturnExpectedString()
            Dim src = GetRandomString(1, 5),
                target = GetRandomString(1, 5),
                srcField1 = GetRandomString(1, 5),
                targetField1 = GetRandomString(1, 5),
                srcField2 = GetRandomString(1, 5),
                targetField2 = GetRandomString(1, 5)

            Dim result = Create() _
                    .WithSourceTable(src) _
                    .WithTargetTable(target) _
                    .WithFieldMapping(srcField1, targetField1) _
                    .WithFieldMapping(srcField2, targetField2) _
                    .Build()
            Dim expected = String.Join("", New String() { _
                                                            "insert into [", target, "] ([", targetField1, "],[",
                                                            targetField2, "]) select [", srcField1, "],[", srcField2,
                                                            "] from [", src, "]"
                                                        })
            Expect(result) _
                .To.Equal(expected)
        End Sub

        <TestCase(DatabaseProviders.Access)>
        <TestCase(DatabaseProviders.SQLServer)>
        <TestCase(DatabaseProviders.SQLite)>
        Public Sub Build_GivenSourceTargetAndTwoFieldMappingsAndCriteria_ShouldReturnExpectedString(
                                                                                                    provider As _
                                                                                                       DatabaseProviders)
            Dim src = GetRandomString(1, 5),
                target = GetRandomString(1, 5),
                srcField1 = GetRandomString(1, 5),
                targetField1 = GetRandomString(1, 5),
                srcField2 = GetRandomString(1, 5),
                targetField2 = GetRandomString(1, 5),
                criteria = GetRandomString(1, 5)

            Dim result = Create() _
                    .WithDatabaseProvider(provider) _
                    .WithSourceTable(src) _
                    .WithTargetTable(target) _
                    .WithFieldMapping(srcField1, targetField1) _
                    .WithFieldMapping(srcField2, targetField2) _
                    .WithCriteria(criteria) _
                    .Build()
            Dim expected = String.Join("", New String() { _
                                                            "insert into [", target, "] ([", targetField1, "],[",
                                                            targetField2,
                                                            "]) select [", srcField1, "],[", srcField2, "] from [", src,
                                                            "]",
                                                            " where ", criteria
                                                        })
            Expect(result) _
                .To.Equal(expected)
        End Sub


        <Test()>
        Public Sub Build_GivenFirebirdProvider_ShouldBuildAppropriateSQLString()
            Dim src = GetRandomString(1, 5),
                target = GetRandomString(1, 5),
                srcField1 = GetRandomString(1, 5),
                targetField1 = GetRandomString(1, 5),
                srcField2 = GetRandomString(1, 5),
                targetField2 = GetRandomString(1, 5),
                criteria = GetRandomString(1, 5)

            Dim result = Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithSourceTable(src) _
                    .WithTargetTable(target) _
                    .WithFieldMapping(srcField1, targetField1) _
                    .WithFieldMapping(srcField2, targetField2) _
                    .WithCriteria(criteria) _
                    .Build()
            Dim expected = String.Join("", New String() { _
                                                            "insert into """, target, """ (""", targetField1, """,""",
                                                            targetField2,
                                                            """) select """, srcField1, """,""", srcField2,
                                                            """ from """, src, """",
                                                            " where ", criteria
                                                        })
            Expect(result) _
                .To.Equal(expected)
        End Sub
    End Class
End NameSpace