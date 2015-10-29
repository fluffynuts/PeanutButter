Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

<TestFixture()>
Public class TestConditionExtensions
    <Test()>
    Public Sub And_ShouldAddConditionsTogetherWithAnd()
        Dim field1 = RandomValueGen.GetRandomString(2),
            value1 = RandomValueGen.GetRandomString(2),
            field2 = RandomValueGen.GetRandomString(2),
            value2 = RandomValueGen.GetRandomString(2)
        Dim left = New Condition(field1, Condition.EqualityOperators.Equals, value1)
        Dim right = New Condition(field2, Condition.EqualityOperators.Equals, value2)
        Dim result = left.And(right)
        Assert.AreEqual("([" + field1 + "]='" + value1 + "' and [" + field2 + "]='" + value2 + "')", result.ToString())
    End Sub

    <Test()>
    Public Sub Or_ShouldAddConditionsTogetherWithOr()

        Dim field1 = RandomValueGen.GetRandomString(2),
            value1 = RandomValueGen.GetRandomString(2),
            field2 = RandomValueGen.GetRandomString(2),
            value2 = RandomValueGen.GetRandomString(2)
        Dim left = New Condition(field1, Condition.EqualityOperators.Equals, value1)
        Dim right = New Condition(field2, Condition.EqualityOperators.Equals, value2)
        Dim result = left.Or(right)
        Assert.AreEqual("([" + field1 + "]='" + value1 + "' or [" + field2 + "]='" + value2 + "')", result.ToString())
    End Sub

    <Test()>
    Public Sub LotsOfAndsAndAnOr()
        Dim field1 = RandomValueGen.GetRandomString(2),
            value1 = RandomValueGen.GetRandomString(2),
            field2 = RandomValueGen.GetRandomString(2),
            value2 = RandomValueGen.GetRandomString(2)
        Dim left = New Condition(field1, Condition.EqualityOperators.Equals, value1)
        Dim right = New Condition(field2, Condition.EqualityOperators.Equals, value2)
        Dim result = left.And(right).Or(right.And(left))
        Dim expected = "(([" + field1 + "]='" + value1 + "' and [" + field2 + "]='" + value2 + "') or ([" + field2 + "]='" + value2 + "' and [" + field1 + "]='" + value1 + "'))"
        Assert.AreEqual(expected, result.ToString())
    End Sub

    <Test()>
    Public Sub And_ShouldSupport_ConditionConstructorParameters()
        Dim field1 = RandomValueGen.GetRandomString(2),
            value1 = RandomValueGen.GetRandomString(2),
            field2 = RandomValueGen.GetRandomString(2),
            value2 = RandomValueGen.GetRandomString(2)

        Dim left = New Condition(field1, Condition.EqualityOperators.Equals, value1)
        Dim right = new Condition(field2, Condition.EqualityOperators.Equals, value2)
        Dim expected = left.And(right).ToString()

        Dim t1 = left.And(field2, Condition.EqualityOperators.Equals, value2, true, true, true)
        Dim t2 = left.And(field2, value2, true)
        Dim rightSelectField  = new SelectField(field2)
        Dim t3 = left.And(rightSelectField, value2, true)
        Dim t4 = left.And(rightSelectField, Condition.EqualityOperators.Equals, value2, true)

        Assert.AreEqual(expected, t1.ToString())
        Assert.AreEqual(expected, t2.ToString())
        Assert.AreEqual(expected, t3.ToString())
        Assert.AreEqual(expected, t4.ToString())

        Dim twoFields = new Condition(new SelectField(field1), Condition.EqualityOperators.Equals, rightSelectField)
        Dim expected2 = left.And(twoFields)
        Assert.AreEqual(expected2.ToString(), 
                        left.And(new SelectField(field1), Condition.EqualityOperators.Equals, rightSelectField).ToString())
        dim t5 = left.And(New SelectField(field1), New SelectField(field2))
        Assert.AreEqual(expected2.ToString(), t5.ToString())
    End Sub

    <Test()>
    Public Sub And_ShouldSupportExtendedSelectFieldConstructorParameters()
        Dim intVal = RandomValueGen.GetRandomInt(),
            decimalVal = RandomValueGen.GetRandomDecimal(),
            nullableDecimalVal As Nullable(Of Decimal) = RandomValueGen.GetRandomDecimal(),
            doubleVal = RandomValueGen.GetRandomDouble(),
            dateVal = RandomValueGen.GetRandomDate()
        Dim field1 = RandomValueGen.GetRandomString(2),
            value1 = RandomValueGen.GetRandomString(2),
            field2 = RandomValueGen.GetRandomString(2),
            value2 = RandomValueGen.GetRandomString(2)

        Dim f2 = new SelectField(field2)
        Dim left = New Condition(field1, Condition.EqualityOperators.Equals, value1)
        Dim right = new Condition(f2, Condition.EqualityOperators.Equals, intVal)
        Dim expected = left.And(right).ToString()
        Dim result = left.And(f2, Condition.EqualityOperators.Equals, intVal)
        Assert.AreEqual(expected, result.ToString())
        
        right = New Condition(f2, Condition.EqualityOperators.Equals, decimalVal)
        expected = left.And(right).ToString()
        result = left.And(f2, Condition.EqualityOperators.Equals, decimalVal)
        Assert.AreEqual(expected, result.ToString())

        right = New Condition(f2, Condition.EqualityOperators.Equals, nullableDecimalVal)
        expected = left.And(right).ToString()
        result = left.And(f2, Condition.EqualityOperators.Equals, nullableDecimalVal)
        Assert.AreEqual(expected, result.ToString())

        right = New Condition(f2, Condition.EqualityOperators.Equals, doubleVal)
        expected = left.And(right).ToString()
        result = left.And(f2, Condition.EqualityOperators.Equals, doubleVal)
        Assert.AreEqual(expected, result.ToString())

        right = New Condition(f2, Condition.EqualityOperators.Equals, dateVal)
        expected = left.And(right).ToString()
        result = left.And(f2, Condition.EqualityOperators.Equals, dateVal)
        Assert.AreEqual(expected, result.ToString())


    End Sub
    <Test()>
    Public Sub Or_ShouldSupportExtendedSelectFieldConstructorParameters()
        Dim intVal = RandomValueGen.GetRandomInt(),
            decimalVal = RandomValueGen.GetRandomDecimal(),
            nullableDecimalVal As Nullable(Of Decimal) = RandomValueGen.GetRandomDecimal(),
            doubleVal = RandomValueGen.GetRandomDouble(),
            dateVal = RandomValueGen.GetRandomDate()
        Dim field1 = RandomValueGen.GetRandomString(2),
            value1 = RandomValueGen.GetRandomString(2),
            field2 = RandomValueGen.GetRandomString(2),
            value2 = RandomValueGen.GetRandomString(2)

        Dim f2 = new SelectField(field2)
        Dim left = New Condition(field1, Condition.EqualityOperators.Equals, value1)
        Dim right = new Condition(f2, Condition.EqualityOperators.Equals, intVal)
        Dim expected = left.Or(right).ToString()
        Dim result = left.Or(f2, Condition.EqualityOperators.Equals, intVal)
        Assert.AreEqual(expected, result.ToString())
        
        right = New Condition(f2, Condition.EqualityOperators.Equals, decimalVal)
        expected = left.Or(right).ToString()
        result = left.Or(f2, Condition.EqualityOperators.Equals, decimalVal)
        Assert.AreEqual(expected, result.ToString())

        right = New Condition(f2, Condition.EqualityOperators.Equals, nullableDecimalVal)
        expected = left.Or(right).ToString()
        result = left.Or(f2, Condition.EqualityOperators.Equals, nullableDecimalVal)
        Assert.AreEqual(expected, result.ToString())

        right = New Condition(f2, Condition.EqualityOperators.Equals, doubleVal)
        expected = left.Or(right).ToString()
        result = left.Or(f2, Condition.EqualityOperators.Equals, doubleVal)
        Assert.AreEqual(expected, result.ToString())

        right = New Condition(f2, Condition.EqualityOperators.Equals, dateVal)
        expected = left.Or(right).ToString()
        result = left.Or(f2, Condition.EqualityOperators.Equals, dateVal)
        Assert.AreEqual(expected, result.ToString())


    End Sub

    <Test()>
    Public Sub Or_ShouldSupport_ConditionConstructorParameters()
        Dim field1 = RandomValueGen.GetRandomString(2),
            value1 = RandomValueGen.GetRandomString(2),
            field2 = RandomValueGen.GetRandomString(2),
            value2 = RandomValueGen.GetRandomString(2)
        Dim left = new Condition(field1, Condition.EqualityOperators.Equals, value1)
        Dim right = new Condition(field2, Condition.EqualityOperators.Equals, value2)
        Dim expected = left.Or(right).ToString()

        Dim t1 = left.Or(field2, Condition.EqualityOperators.Equals, value2, true, true, true)
        Dim t2 = left.Or(field2, value2, true)
        Dim t3 = left.Or(new SelectField(field2), value2, true)
        Dim t4 = left.Or(new SelectField(field2), Condition.EqualityOperators.Equals, value2, true)

        Assert.AreEqual(expected, t1.ToString())
        Assert.AreEqual(expected, t2.ToString())
        Assert.AreEqual(expected, t3.ToString())
        Assert.AreEqual(expected, t4.ToString())

        Dim twoFields = new Condition(new SelectField(field1), Condition.EqualityOperators.Equals, new SelectField(field2))
        Dim expected2 = left.Or(twoFields)
        Dim actual = left.Or(new SelectField(field1), Condition.EqualityOperators.Equals, new SelectField(field2)).ToString()
        Assert.AreEqual(expected2.ToString(), 
                        actual)
        dim t5 = left.Or(New SelectField(field1), New SelectField(field2))
        Assert.AreEqual(expected2.ToString(), t5.ToString())
    End Sub
End Class