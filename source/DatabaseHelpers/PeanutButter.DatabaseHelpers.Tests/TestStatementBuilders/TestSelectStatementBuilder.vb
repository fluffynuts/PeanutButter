Imports NUnit.Framework
Imports PeanutButter.DatabaseHelpers.StatementBuilders
Imports PeanutButter.RandomGenerators.RandomValueGen
Imports NExpect
Imports NExpect.Expectations

Namespace TestStatementBuilders

    <TestFixture()>
    Public Class TestSelectStatementBuilder
        <Test()>
        Public Sub Create_ShouldReturnNewInstanceOfSelectStatementBuilder()
            Dim builder1 = SelectStatementBuilder.Create()
            Dim builder2 = SelectStatementBuilder.Create()
            Expect(builder1) _
                .Not.To.Equal(builder2)
            Expect(builder1) _
                .To.Be.An.Instance.Of(Of SelectStatementBuilder)
            Expect(builder2) _
                .To.Be.An.Instance.Of(Of SelectStatementBuilder)
        End Sub
        Private Function Create() As ISelectStatementBuilder
            Return SelectStatementBuilder.Create()
        End Function
        <Test()>
        Public Sub WithTable_ShouldReturnBuilderInstance()
            Dim builder = Create()
            Dim result  = builder.WithTable(GetRandomString())
            Expect(result) _
                .To.Equal(builder)
        End Sub

        <Test()>
        Public Sub WithField_ShouldReturnBuilderInstance()
            Dim builder = Create()
            Dim result = builder.WithField(GetRandomString())
            Expect(result) _
                .To.Equal(builder)
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneField_ShouldReturnExpectedSelectStatement()
            Dim table = GetRandomString(1),
                field = GetRandomString(1)
            Dim result = Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .Build()
            Expect(result, "select [" + field + "] from [" + table + "]")
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneField_AndFirebirdProvider_ShouldReturnExpectedSelectStatement()
            Dim table = GetRandomString(1),
                field = GetRandomString(1)
            Dim result = Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table) _
                    .WithField(field) _
                    .Build()
            Expect(result, "select """ + field + """ from """ + table + """")
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndOneSelectField_ShouldReturnExpectedSelectStatement()
            Dim table = GetRandomString(),
                field = GetRandomString()
            Dim result = Create() _
                    .WithTable(table) _
                    .WithField(New SelectField(table, field)) _
                    .Build()
            Expect(result, "select [" + table + "].[" + field + "] from [" + table + "]")
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndTwoFields_ShouldReturnExpectedStatement()
            Dim table = GetRandomString(1),
                f1 = GetRandomString(1),
                f2 = GetRandomString(1)
            Dim result = Create() _
                    .WithTable(table) _
                    .WithField(f1) _
                    .WithField(f2) _
                    .Build()
            Expect("select [" + f1 + "],[" + f2 + "] from [" + table + "]", result)
        End Sub

        <Test()>
        Public Sub WithCondition_ShouldReturnBuilderInstance()
            Dim builder = Create()
            Dim result = builder.WithCondition(GetRandomString(1))
            Expect(result) _
                .To.Equal(builder)
        End Sub
        <Test()>
        Public Sub WithStringCondition_GivenTableAndFieldAndOneWhereClause_ShouldReturnExpectedStatement()
            Dim table = GetRandomString(1),
                field = GetRandomString(1),
                clause = GetRandomString(1)
            Dim result = Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .WithCondition(clause) _
                    .Build()
            Expect(result, "select [" + field + "] from [" + table + "] where " + clause)
        End Sub

        <Test()>
        Public Sub WithInt32Condition_GivenTableAndFieldAndOneWhereClause_ShouldReturnExpectedStatement()
            Dim table = GetRandomString(1),
                field = GetRandomString(1),
                value = Int32.Parse(CStr(GetRandomInt()))
            Dim result = Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .WithCondition(field, Condition.EqualityOperators.Equals, value) _
                    .Build()
            Expect(result, "select [" + field + "] from [" + table + "] where [" + field + "]=" + value.ToString())
        End Sub

        <Test()>
        Public Sub WithInt32ConditionAndFirebirdDB_GivenTableAndFieldAndOneWhereClause_ShouldReturnExpectedStatement()
            Dim table = GetRandomString(1),
                field = GetRandomString(1),
                value = Int32.Parse(CStr(GetRandomInt()))
            Dim result = Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table) _
                    .WithField(field) _
                    .WithCondition(field, Condition.EqualityOperators.Equals, value) _
                    .Build()
            Expect(result, "select """ + field + """ from """ + table + """ where """ + field + """=" + value.ToString())
        End Sub

        <Test()>
        Public Sub WithCondition_WhenCalledTwice_ShouldAddConditionAsAnd()
            Dim table = GetRandomString(1),
                field1 = GetRandomString(1),
                value1 = GetRandomString(1),
                field2 = GetRandomString(1),
                value2 = GetRandomString(1)
            Dim result = Create() _
                    .WithAllFieldsFrom(table) _
                    .WithCondition(field1, Condition.EqualityOperators.Equals, value1) _
                    .WithCondition(field2, Condition.EqualityOperators.Equals, value2) _
                    .Build()
            Expect(result, "select * from [" + table + "] where ([" + field1 + "]='" + value1 + "' and [" + field2 + "]='" + value2 + "')")

        End Sub


        <Test()>
        Public Sub GivenTableAndStarColumn_DoesNotBracketStar()
            Dim table = GetRandomString(1),
                field = "*"
            Dim result = Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .Build()
            Expect(result, "select * from [" + table + "]")
        End Sub

        <Test()>
        Public Sub WithAllFields_SelectsStar()
            Dim table = GetRandomString()
            Dim result = Create() _
                    .WithAllFieldsFrom(table) _
                    .Build()
            Expect(result, "select * from [" + table + "]")
        End Sub

        <Test()>
        Public Sub SelectAllFrom_Returns_WithAllFields_Query()
            Dim table = GetRandomString()
            Dim result = SelectStatementBuilder.SelectAllFrom(table)
            Expect(result, "select * from [" + table + "]")
        End Sub

        <Test()>
        Public Sub Build_GivenTableAndComputedField_ReturnsExpectedSql()
            Dim field = GetRandomString(),
                fieldAlias = GetRandomString(),
                table = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithComputedField(field, ComputedField.ComputeFunctions.Max, fieldAlias) _
                    .Build()
            Expect(result, "select Max([" + field + "]) as [" + fieldAlias + "] from [" + table + "]")
        End Sub

        <Test()>
        Public Sub Build_GivenInnerJoin_ReturnsExpectedSql()
            Dim field1 = "field1",
                field2 = "field2",
                joinField1 = "joinField1",
                joinField2 = "joinField2",
                table1 = "table1",
                table2 = "table2"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, Condition.EqualityOperators.Equals, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
            Dim expected = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenInnerJoinWithRandomParts_ReturnsExpectedSql()
            Dim field1 = GetRandomString(),
                field2 = GetRandomString(),
                joinField1 = GetRandomString(),
                joinField2 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, Condition.EqualityOperators.Equals, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
            Dim expected = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenLeftJoinWithRandomParts_ReturnsExpectedSql()
            Dim field1 = GetRandomString(),
                field2 = GetRandomString(),
                joinField1 = GetRandomString(),
                joinField2 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithLeftJoin(table1, joinField1, Condition.EqualityOperators.Equals, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
            Dim expected = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] left join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenInnerJoinWithTablesAndFieldsOnly_InfersEqualityOperator()
            Dim field1 = GetRandomString(),
                field2 = GetRandomString(),
                joinField1 = GetRandomString(),
                joinField2 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
            Dim expected = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenInnerJoinWithTablesAndFieldsOnlyWithFirebirdProvider_InfersEqualityOperator()
            Dim field1 = GetRandomString(),
                field2 = GetRandomString(),
                joinField1 = GetRandomString(),
                joinField2 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
            Dim expected = "select """ + field1 + """,""" + field2 + """ from """ + table1 + """ inner join """ + table2 + """ on """ + table1 + """.""" + joinField1 + """=""" + table2 + """.""" + joinField2 + """"
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenInnerJoinWithLeftTableLeftFieldAndRightTableOnly_InfersEqualityOperatorAndRightField()
            Dim field1 = GetRandomString(),
                field2 = GetRandomString(),
                joinField1 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithInnerJoin(table1, joinField1, table2) _
                    .WithField(field2) _
                    .Build()
            Dim expected = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] inner join [" + table2 + "] on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField1 + "]"
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(JoinDirections.Inner, "inner")>
        <TestCase(JoinDirections.Outer, "outer")>
        <TestCase(JoinDirections.Left, "left")>
        <TestCase(JoinDirections.Right, "right")>
        Public Sub Build_UsingSimpleComplexJoin_ShouldProduceCorrectSQLFor_(direction as JoinDirections, joinStr As String)
            Dim leftCol = GetRandomString(),
                rightCol = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithAllFieldsFrom(table1) _
                    .WithJoin(table1, table2, direction, New Condition(New SelectField(table1, leftCol), _
                                                                       Condition.EqualityOperators.Equals, _
                                                                       New SelectField(table2, rightCol))) _
                    .Build() 
            Dim expected = "select * from [" + table1 + "] " + joinStr + " join [" + table2 + "] on [" + table1 + "].[" + leftCol + "]=[" + table2 + "].[" + rightCol + "]"
            Expect(result).To.Equal(expected)
        End Sub 

        <TestCase(JoinDirections.Inner, "inner")>
        <TestCase(JoinDirections.Outer, "outer")>
        <TestCase(JoinDirections.Left, "left")>
        <TestCase(JoinDirections.Right, "right")>
        Public Sub Build_UsingLessSimpleComplexJoin_ShouldProduceCorrectSQLFor_(direction as JoinDirections, joinStr As String)
            Dim leftCol = GetRandomString(),
                rightCol = GetRandomString(),
                leftCol2 = GetRandomString(),
                rightCol2 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithAllFieldsFrom(table1) _
                    .WithJoin(table1, table2, direction, 
                              New Condition(New SelectField(table1, leftCol), _
                                            Condition.EqualityOperators.Equals, _
                                            New SelectField(table2, rightCol)),
                              New Condition(New SelectField(table1, leftCol2), _
                                            Condition.EqualityOperators.Equals, _
                                            new SelectField(table2, rightCol2))) _
                    .Build() 
            Dim expected = "select * from [" + table1 + "] " + joinStr + " join [" + table2 + "] on ([" + table1 + "].[" + _ 
                           leftCol + "]=[" + table2 + "].[" + rightCol + "] and [" + table1 + "].[" + leftCol2 + "]=[" + _
                           table2 + "].[" + rightCol2 + "])"
            Expect(result).To.Equal(expected)
        End Sub 

        <Test()>
        Public Sub ComplexJoinSimilarToSpecificUseCase()
            Dim contractId = "nContractID",
                userId = "nUserId",
                leftTable = "Contract",
                rightTable = "ContractUser",
                userIdVal = 93
            Dim result = SelectStatementBuilder.Create() _
                    .WithAllFieldsFrom(leftTable) _
                    .WithJoin(leftTable, rightTable, JoinDirections.Left, _
                              New Condition(New SelectField(leftTable, contractId), _
                                            Condition.EqualityOperators.Equals, _
                                            New SelectField(rightTable, contractId)), _
                              New Condition(New SelectField(leftTable, userId), _
                                            Condition.EqualityOperators.Equals, _
                                            userIdVal)) _
                    .Build()
            Dim expected = "select * from [Contract] left join [ContractUser] on ([Contract].[nContractID]=[ContractUser].[nContractID] and [Contract].[nUserId]=93)"
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithCondition_GivenSelectFieldAndStringValue_ProducesExpectedResult()
            Dim fld = New SelectField(GetRandomString(), GetRandomString())
            Dim val = GetRandomString()
            Dim expected = "select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + "='" + val + "'"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, Condition.EqualityOperators.Equals, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithCondition_GivenTwoSelectFields_ProducesExpectedResult()
            Dim f1 = New SelectField(GetRandomString(), GetRandomString()),
                f2 = New SelectField(GetRandomString(), GetRandomString())
            Dim expected = "select [" + f1.Field + "] from [" + f1.Table + "],[" + f2.Table + "] where " + f1.ToString() + "=" + f2.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(f1.Table) _
                    .WithTable(f2.Table) _
                    .WithField(f1.Field) _
                    .WithCondition(f1, Condition.EqualityOperators.Equals, f2) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithCondition_GivenSelectFieldAndInt16Value_ProducesExpectedResult()
            Dim fld = New SelectField(GetRandomString(), GetRandomString())
            Dim val = Int16.Parse(CStr(GetRandomInt(1, 100)))
            Dim expected = "select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + "=" + val.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, Condition.EqualityOperators.Equals, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(Condition.EqualityOperators.Equals)>
        <TestCase(Condition.EqualityOperators.GreaterThan)>
        <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.LessThan)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.NotEquals)>
        Public Sub WithCondition_GivenSelectFieldAndInt32Value_ProducesExpectedResult(op As Condition.EqualityOperators)
            Dim fld = New SelectField(GetRandomString(), GetRandomString())
            Dim val = Int32.Parse(CStr(GetRandomInt(1, 100)))
            Dim expected = "select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(Condition.EqualityOperators.Equals)>
        <TestCase(Condition.EqualityOperators.GreaterThan)>
        <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.LessThan)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.NotEquals)>
        Public Sub WithCondition_GivenSelectFieldAndInt64Value_ProducesExpectedResult(op As Condition.EqualityOperators)
            Dim fld = New SelectField(GetRandomString(), GetRandomString())
            Dim val = Int64.Parse(CStr(GetRandomInt(1, 100)))
            Dim expected = "select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(Condition.EqualityOperators.Equals)>
        <TestCase(Condition.EqualityOperators.GreaterThan)>
        <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.LessThan)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.NotEquals)>
        Public Sub WithCondition_GivenSelectFieldAndDecimalValue_ProducesExpectedResult(op As Condition.EqualityOperators)
            Dim fld = New SelectField(GetRandomString(), GetRandomString())
            Dim val = Decimal.Parse(CStr(GetRandomInt(1, 100)))
            Dim expected = "select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(Condition.EqualityOperators.Equals)>
        <TestCase(Condition.EqualityOperators.GreaterThan)>
        <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.LessThan)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.NotEquals)>
        Public Sub WithCondition_GivenSelectFieldAndDoubleValue_ProducesExpectedResult(op As Condition.EqualityOperators)
            Dim fld = New SelectField(GetRandomString(), GetRandomString())
            Dim val = Double.Parse(CStr(GetRandomInt(1, 100)))
            Dim expected = "select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + val.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(Condition.EqualityOperators.Equals)>
        <TestCase(Condition.EqualityOperators.GreaterThan)>
        <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.LessThan)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.NotEquals)>
        Public Sub WithCondition_GivenSelectFieldAndDateValue_ProducesExpectedResult(op As Condition.EqualityOperators)
            Dim fld = New SelectField(GetRandomString(), GetRandomString())
            Dim val = GetRandomDate()
            Dim expected = "select [" + fld.Field + "] from [" + fld.Table + "] where " + fld.ToString() + Condition.OperatorResolutions(op) + "'" + val.ToString("yyyy/MM/dd") + "'"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(fld.Table) _
                    .WithField(fld.Field) _
                    .WithCondition(fld, op, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(Condition.EqualityOperators.Equals)>
        <TestCase(Condition.EqualityOperators.GreaterThan)>
        <TestCase(Condition.EqualityOperators.GreaterThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.LessThan)>
        <TestCase(Condition.EqualityOperators.LessThanOrEqualTo)>
        <TestCase(Condition.EqualityOperators.NotEquals)>
        Public Sub WithCondition_CanAcceptAConditionObject(op As Condition.EqualityOperators)
            Dim c = New Condition(GetRandomString(), op, GetRandomString())
            Dim table = GetRandomString()
            Dim fld = GetRandomString()
            Dim expected = "select [" + fld + "] from [" + table + "] where " + c.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithCondition(c) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(OrderBy.Directions.Descending)>
        <TestCase(OrderBy.Directions.Ascending)>
        Public Sub OrderBy_AddsExpectedOrderByClauseToOutput(direction As OrderBy.Directions)
            Dim o = New OrderBy(GetRandomString(), GetRandomString(), direction)
            Dim table = GetRandomString()
            Dim fld = GetRandomString()
            Dim expected = "select [" + fld + "] from [" + table + "] " + o.ToString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .OrderBy(o) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(OrderBy.Directions.Ascending)>
        <TestCase(OrderBy.Directions.Descending)>
        Public Sub OrderBy_GivenFieldAndDirection_AddsExpectedOrderByClauseToOutput(direction As OrderBy.Directions)
            Dim ofld = GetRandomString(),
                table = GetRandomString(),
                fld = GetRandomString()
            Dim expected = "select [" + fld + "] from [" + table + "] order by [" + ofld + "] " + CStr(IIf(direction = OrderBy.Directions.Ascending, "asc", "desc"))
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .OrderBy(ofld, direction) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(OrderBy.Directions.Ascending)>
        <TestCase(OrderBy.Directions.Descending)>
        Public Sub OrderBy_GivenFieldAndTableAndDirection_AddsExpectedOrderByClauseToOutput(direction As OrderBy.Directions)
            Dim ofld = GetRandomString(),
                otable = GetRandomString(),
                table = GetRandomString(),
                fld = GetRandomString()
            Dim expected = "select [" + fld + "] from [" + table + "] order by [" + otable + "].[" + ofld + "] " + CStr(IIf(direction = OrderBy.Directions.Ascending, "asc", "desc"))
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .OrderBy(otable, ofld, direction) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(OrderBy.Directions.Ascending)>
        <TestCase(OrderBy.Directions.Descending)>
        Public Sub OrderBy_GivenFieldAndTableAndDirectionAndFirebirdProvider_AddsExpectedOrderByClauseToOutput(direction As OrderBy.Directions)
            Dim ofld = GetRandomString(),
                otable = GetRandomString(),
                table = GetRandomString(),
                fld = GetRandomString()
            Dim expected = "select """ + fld + """ from """ + table + """ order by """ + otable + """.""" + ofld + """ " + CStr(IIf(direction = OrderBy.Directions.Ascending, "asc", "desc"))
            Dim result = SelectStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table) _
                    .WithField(fld) _
                    .OrderBy(otable, ofld, direction) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_WhenUsingSelectFieldConstructsOnFirebirdDatabaseProvider_ShouldProduceSqlWithoutBrackets()
            ' this test is based on a real-world failure
' ReSharper disable once InconsistentNaming
            Dim getFilterConditions as Func(Of IEnumerable(Of ICondition)) = Function()
                return { new Condition("IS_PROCESSED", Condition.EqualityOperators.Equals, 0) }
                    End Function
            Dim expected = "select distinct ""ZONE"".""CTRL_SLA"" from ""ZONE"" inner join ""TRANSACK"" on ""ZONE"".""CTRL_SLA""=""TRANSACK"".""CTRL_SLA"" where ""IS_PROCESSED""=0"
            Dim table1  = "ZONE"
            Dim result = SelectStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table1) _
                    .Distinct() _
                    .WithField(new SelectField(table1, "CTRL_SLA")) _
                    .WithInnerJoin(table1, "CTRL_SLA", "TRANSACK", _
                                   "CTRL_SLA") _
                    .WithAllConditions(getFilterConditions().ToArray()).Build()
            Expect(result,expected)
        End Sub

        <Test()>
        Public Sub WithAllConditions_GivenSomeConditions_ReturnsExpectedStatement()
            Dim fld = GetRandomString(),
                table = GetRandomString(),
                c1 = New Condition(GetRandomString(), Condition.EqualityOperators.Equals, GetRandomString()),
                c2 = New Condition(GetRandomString(), Condition.EqualityOperators.GreaterThan, GetRandomString()),
                c3 = New Condition(GetRandomString(), Condition.EqualityOperators.LessThan, GetRandomString())
            Dim expected = "select [" + fld + "] from [" + table + "] where (" + c1.ToString() + " and " + c2.ToString() + " and " + c3.ToString() +")"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithAllConditions(c1, c2, c3) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithAllConditions_GivenSomeConditionsAndFirebirdProvider_ReturnsExpectedStatement()
            Dim fld = GetRandomString(),
                table = GetRandomString(),
                c1 = New Condition(GetRandomString(), Condition.EqualityOperators.Equals, GetRandomString()),
                c2 = New Condition(GetRandomString(), Condition.EqualityOperators.GreaterThan, GetRandomString()),
                c3 = New Condition(GetRandomString(), Condition.EqualityOperators.LessThan, GetRandomString())
            Dim result = SelectStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithAllConditions(c1, c2, c3) _
                    .Build()
            Dim expected = "select """ + fld + """ from """ + table + """ where (" + c1.ToString() + " and " + c2.ToString() + " and " + c3.ToString() +")"
            Assert.That(c1.ToString(), Does.Not.Contain("["))
            Assert.That(c1.ToString(), Does.Not.Contain("]"))
            Assert.That(c2.ToString(), Does.Not.Contain("["))
            Assert.That(c2.ToString(), Does.Not.Contain("]"))
            Assert.That(c3.ToString(), Does.Not.Contain("["))
            Assert.That(c3.ToString(), Does.Not.Contain("]"))
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithAllConditions_GivenSomeConditionsAndFirebirdProviderAndJumbledOrder_ReturnsExpectedStatement()
            Dim fld = GetRandomString(),
                table = GetRandomString(),
                c1 = New Condition(GetRandomString(), Condition.EqualityOperators.Equals, GetRandomString()),
                c2 = New Condition(GetRandomString(), Condition.EqualityOperators.GreaterThan, GetRandomString()),
                c3 = New Condition(GetRandomString(), Condition.EqualityOperators.LessThan, GetRandomString())
            Dim expected = "select """ + fld + """ from """ + table + """ where (" + c1.ToString() + " and " + c2.ToString() + " and " + c3.ToString() +")"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithAllConditions(c1, c2, c3) _
                    .WithDatabaseProvider(DatabaseProviders.Firebird) _
                    .Build()
            Assert.That(c1.ToString(), Does.Not.Contain("["))
            Assert.That(c1.ToString(), Does.Not.Contain("]"))
            Assert.That(c2.ToString(), Does.Not.Contain("["))
            Assert.That(c2.ToString(), Does.Not.Contain("]"))
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithAnyConditions_GivenSomeConditions_ReturnsExpectedStatement()
            Dim fld = GetRandomString(),
                table = GetRandomString(),
                c1 = New Condition(GetRandomString(), Condition.EqualityOperators.Equals, GetRandomString()),
                c2 = New Condition(GetRandomString(), Condition.EqualityOperators.GreaterThan, GetRandomString()),
                c3 = New Condition(GetRandomString(), Condition.EqualityOperators.LessThan, GetRandomString())
            Dim expected = "select [" + fld + "] from [" + table + "] where (" + c1.ToString() + " or " + c2.ToString() + " or " + c3.ToString() + ")"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(fld) _
                    .WithAnyCondition(c1, c2, c3) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_WhenDistinctSpecified_ProducesDistinctQuery()
            ' setup
            Dim table = GetRandomString()
            Dim expected = "select distinct * from [" + table + "]"
            ' assert pre-conditions
            ' perform test
            Dim result = SelectStatementBuilder.Create().Distinct().WithAllFieldsFrom(table).Build()
            ' assert test results
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenFieldWithAlias_ProducesQueryWithAliasedField()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                aliasAs = GetRandomString()
            Dim expected = "select [" + field + "] as [" + aliasAs + "] from [" + table + "]"
            Dim result = SelectStatementBuilder.Create().WithTable(table).WithField(field, aliasAs).Build()

            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTop_ProducesQueryWithTopRequirement()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                topVal = GetRandomInt()
            Dim expected = "select top " + topVal.ToString() + " [" + field + "] from [" + table + "]"
            Dim result = SelectStatementBuilder.Create().WithTable(table).WithField(field).WithTop(topVal).Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenTopAndFirebirdDatabaseProvider_ProducesQueryWithTopRequirement()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                topVal = GetRandomInt()
            Dim expected = "select first " + topVal.ToString() + " """ + field + """ from """ + table + """"
            Dim result = SelectStatementBuilder.Create().WithDatabaseProvider(DatabaseProviders.Firebird).WithTable(table).WithField(field).WithTop(topVal).Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenContains_ProducesQueryWithLike()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                val = GetRandomString()
            Dim expected = "select [" + field + "] from [" + table + "] where [" + field + "] like '%" + val + "%'"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .WithCondition(field, Condition.EqualityOperators.Contains, val).Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenContains_AndSelectField_ProducesQueryWithLike()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                val = GetRandomString()
            Dim expected = "select [" + field + "] from [" + table + "] where [" + table + "].[" + field + "] like '%" + val + "%'"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table)  _
                    .WithField(field) _
                    .WithCondition(New SelectField(table, field), Condition.EqualityOperators.Contains, val) _
                    .Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenLikeProducesQueryWithLikeAndNoExtraPercentages()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                val = GetRandomString()
            Dim expected = "select [" + field + "] from [" + table + "] where [" + field + "] like '" + val + "'"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .WithCondition(field, Condition.EqualityOperators.Like, val).Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenStartsWithProducesQueryWithLikeAndOnlyEndPercentage()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                val = GetRandomString()
            Dim expected = "select [" + field + "] from [" + table + "] where [" + field + "] like '" + val + "%'"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .WithCondition(field, Condition.EqualityOperators.StartsWith, val).Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub Build_GivenEndsWithProducesQueryWithLikeAndOnlyStartPercentage()
            Dim table = GetRandomString(),
                field = GetRandomString(),
                val = GetRandomString()
            Dim expected = "select [" + field + "] from [" + table + "] where [" + field + "] like '%" + val + "'"
            Dim result = SelectStatementBuilder.Create() _
                    .WithTable(table) _
                    .WithField(field) _
                    .WithCondition(field, Condition.EqualityOperators.EndsWith, val).Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithFields_AddsAllFieldsInSpecifiedOrder()
            Dim table = GetRandomString(),
                f1 = GetRandomString(),
                f2 = GetRandomString(),
                f3 = GetRandomString()
            Dim expected = "select [" + f1 + "],[" + f2 + "],[" + f3 + "] from [" + table + "]"
            Dim result = SelectStatementBuilder.Create().WithTable(table).WIthFIelds(f1, f2, f3).Build()
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithAllFieldsFrom_MultipleCallsShouldNotCreateBrokenSql()
            Dim table = GetRandomString()
            Dim builder = SelectStatementBuilder.Create().WithAllFieldsFrom(table)
            Dim expected = builder.Build()
            builder.WithAllFieldsFrom(table)
            Dim result = builder.Build()
            Expect(result).To.Equal(expected)
        End Sub

        ' TODO:
        ' Access requires bracketing around join parts (and SQL Server tolerates them), so this should be implemented
        <Test()>
        <Explicit("PLEASE FIX ME")>
        Public Sub InnerJoin_GivenTwoTablesToJoinAcross_ShouldProduceSqlWithBracketedJoinParts()
            Dim t1 = GetRandomString(),
                t2 = GetRandomString(),
                t3 = GetRandomString(),
                keyField = GetRandomString()
            Dim builder = SelectStatementBuilder.Create() _
                    .WithField(new SelectField(t1, keyField)) _
                    .WithTable(t1) _
                    .WithInnerJoin(t1, keyField, t2, keyField) _
                    .WithInnerJoin(t1, keyField, t3, keyField)
            Dim result = builder.Build()
            Dim expected = "select [" + t1 + "].[" + keyField + "] from (([" + t1 + "] inner join [" + t2 + "] on [" + t1 + "].[" + keyField + "]=[" _
                           + t2 + "].[" + keyField + "]) inner join [" + t3 + "].[" + keyField + "] on [" + t1 + "].[" + keyField + "]=[" _
                           + t3 + "].[" + keyField + "]"
                            
            Expect(result).To.Equal(expected)
        End Sub

        <TestCase(DatabaseProviders.Access, "")>
        <TestCase(DatabaseProviders.Firebird, "")>
        <TestCase(DatabaseProviders.SQLServer, " WITH (NOLOCK)")>
        <TestCase(DatabaseProviders.SQLite, "")>
        Public Sub WithNoLock_ShouldInsertRelevantNoLockPart(provider As DatabaseProviders, expectedPart As String)
            Dim table = GetRandomString(),
                f1 = GetRandomString(),
                f2 = GetRandomString(),
                f3 = GetRandomString()
            Dim builder = SelectStatementBuilder.Create().WithDatabaseProvider(provider)
            Dim l = builder.OpenObjectQuote
            Dim r = builder.CloseObjectQuote
            Dim result = builder.WithNoLock().WithTable(table).WIthFIelds(f1, f2, f3).Build()
            Dim expected = "select " + l + f1 + r + "," + l + f2 + r + "," + l + f3 + r + " from " + l + table + r + expectedPart
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        public Sub WithNoLock_WhenProviderIsSQLServer_ShouldAddNoLockHintsToAllTables()
            Dim field1 = GetRandomString(),
                field2 = GetRandomString(),
                joinField1 = GetRandomString(),
                joinField2 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim result = SelectStatementBuilder.Create() _
                    .WithDatabaseProvider(DatabaseProviders.SQLServer) _
                    .WithNoLock() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithLeftJoin(table1, joinField1, Condition.EqualityOperators.Equals, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
            Dim expected = "select [" + field1 + "],[" + field2 + "] from [" + table1 + "] WITH (NOLOCK) left join [" + table2 + "] WITH (NOLOCK) on [" + table1 + "].[" + joinField1 + "]=[" + table2 + "].[" + joinField2 + "]"
            Expect(result).To.Equal(expected)
        End Sub


        <TestCase(DatabaseProviders.Access)>
        <TestCase(DatabaseProviders.Firebird)>
        <TestCase(DatabaseProviders.SQLite)>
        public Sub WithNoLock_WhenProviderIsNotSQLServer_ShouldNotAlterOutput(provider As DatabaseProviders)
            Dim field1 = GetRandomString(),
                field2 = GetRandomString(),
                joinField1 = GetRandomString(),
                joinField2 = GetRandomString(),
                table1 = GetRandomString(),
                table2 = GetRandomString()
            Dim builder = SelectStatementBuilder.Create() _
                    .WithDatabaseProvider(provider)
            Dim result = builder.WithNoLock() _
                    .WithTable(table1) _
                    .WithField(field1) _
                    .WithLeftJoin(table1, joinField1, Condition.EqualityOperators.Equals, table2, joinField2) _
                    .WithField(field2) _
                    .Build()
            Dim l = builder.OpenObjectQuote
            Dim r = builder.CloseObjectQuote
            Dim expected = "select " + l + field1 + r + "," + l + field2 + r + " from " + l + table1 + r + " left join " + l + table2 + r + " on " + l + table1 + r + "." + l + joinField1 + r + "=" + l + table2 + r + "." + l + joinField2 + r
            Expect(result).To.Equal(expected)
        End Sub

        <Test()>
        Public Sub WithSubSelect_GivenISelectStatementBuilderAndAlias_ShouldProduceExpectedSQL()
            dim table = GetRandomString(2),
                subQueryAlias = GetRandomString(2)
            Dim inner = SelectStatementBuilder.Create() _
                    .WithAllFieldsFrom(table)
            Dim result = SelectStatementBuilder.Create() _
                    .WithAllFieldsFrom(inner, subQueryAlias) _
                    .Build()
            dim expected = "select * from (select * from [" + table + "]) as [" + subQueryAlias + "]"
            Expect(result).To.Equal(expected)
        End Sub

    End Class
End NameSpace