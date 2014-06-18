Imports NUnit.Framework
Imports PeanutButter.RandomGenerators

Public Class TestCondition
    <Test()>
    Public Sub Constructor_GivenFieldAndValue_ProducesExpectedCondition()
        Dim fld = RandomValueGen.GetRandomString()
        Dim value = RandomValueGen.GetRandomString()
        Dim c = New Condition(New SelectField(fld), Condition.EqualityOperators.Equals, value)
        Assert.AreEqual("[" + fld + "]='" + value + "'", c.ToString())
    End Sub

    <Test()>
    Public Sub Constructor_GivenFIeldAndNonQuotedValue_ProducesExpectedCondition()
        Dim fld = RandomValueGen.GetRandomString()
        Dim val = RandomValueGen.GetRandomInt()
        Dim c = New Condition(New SelectField(fld), Condition.EqualityOperators.Equals, val.ToString(), False)
        Assert.AreEqual("[" + fld + "]=" + val.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub Constructor_GivenTwoBasicFields_ProducesExpectedCondition()
        Dim t1 = RandomValueGen.GetRandomString(),
            f1 = RandomValueGen.GetRandomString()
        Dim t2 = RandomValueGen.GetRandomString(),
            f2 = RandomValueGen.GetRandomString()
        Dim leftField As SelectField = New SelectField(t1, f1)
        Dim rightField As SelectField = New SelectField(t2, f2)
        Dim c = New Condition(leftField, Condition.EqualityOperators.Equals, rightField)
        Assert.AreEqual(leftField.ToString() + "=" + rightField.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub Constructor_GivenTwoBasicFieldsOnly_InfersEquality()
        Dim t1 = RandomValueGen.GetRandomString(),
            f1 = RandomValueGen.GetRandomString()
        Dim t2 = RandomValueGen.GetRandomString(),
            f2 = RandomValueGen.GetRandomString()
        Dim leftField As SelectField = New SelectField(t1, f1)
        Dim rightField As SelectField = New SelectField(t2, f2)
        Dim c = New Condition(leftField, rightField)
        Assert.AreEqual(leftField.ToString() + "=" + rightField.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub Constructor_GivenFieldNameAndValueOnly_InfersEqualityOperator()
        Dim fld = RandomValueGen.GetRandomString()
        Dim value = RandomValueGen.GetRandomString()
        Dim c = New Condition(fld, value)
        Assert.AreEqual("[" + fld + "]='" + value + "'", c.ToString())
    End Sub

    <Test()>
    Public Sub Constructor_GivenFieldAndValueOnly_InfersEqualityOperator()
        Dim t1 = RandomValueGen.GetRandomString(),
            f1 = RandomValueGen.GetRandomString(),
            val = RandomValueGen.GetRandomString()
        Dim leftField As SelectField = New SelectField(t1, f1)
        Dim c = New Condition(leftField, val)
        Assert.AreEqual(leftField.ToString() + "='" + val + "'", c.ToString())
    End Sub

    <Test()>
    Public Sub Int16ValueConstructor()
        Dim f = RandomValueGen.GetRandomString(),
            v = Int16.Parse(CStr(RandomValueGen.GetRandomInt(1, 100))),
            c = New Condition(f, Condition.EqualityOperators.Equals, v)
        Assert.AreEqual("[" + f + "]=" + v.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub Int32ValueConstructor()
        Dim f = RandomValueGen.GetRandomString(),
            v = Int32.Parse(CStr(RandomValueGen.GetRandomInt(1, 100))),
            c = New Condition(f, Condition.EqualityOperators.Equals, v)
        Assert.AreEqual("[" + f + "]=" + v.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub Int64ValueConstructor()
        Dim f = RandomValueGen.GetRandomString(),
            v = Int64.Parse(CStr(RandomValueGen.GetRandomInt(1, 100))),
            c = New Condition(f, Condition.EqualityOperators.Equals, v)
        Assert.AreEqual("[" + f + "]=" + v.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub DecimalValueConstructor()
        Dim f = RandomValueGen.GetRandomString(),
            v = Decimal.Parse(CStr(RandomValueGen.GetRandomInt(1, 100))),
            c = New Condition(f, Condition.EqualityOperators.Equals, v)
        Assert.AreEqual("[" + f + "]=" + v.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub DoubleValueConstructor()
        Dim f = RandomValueGen.GetRandomString(),
            v = Double.Parse(CStr(RandomValueGen.GetRandomInt(1, 100))),
            c = New Condition(f, Condition.EqualityOperators.Equals, v)
        Assert.AreEqual("[" + f + "]=" + v.ToString(), c.ToString())
    End Sub

    <Test()>
    Public Sub DateTimeValueConstructor()
        Dim f = RandomValueGen.GetRandomString(),
            v = RandomValueGen.GetRandomDate(),
            c = New Condition(f, Condition.EqualityOperators.Equals, v)
        Assert.AreEqual("[" + f + "]='" + v.ToString("yyyy/MM/dd HH:mm:ss") + "'", c.ToString())
    End Sub

    <Test()>
    Public Sub NullString()
        Dim f = RandomValueGen.GetRandomString(),
            v = DirectCast(Nothing, String)
        Dim c = New Condition(f, Condition.EqualityOperators.Equals, v)
        Assert.AreEqual("[" + f + "] is NULL", c.ToString())
    End Sub

    <Test()>
    Public Sub ConditionWithOneSelectField_RespectsDatabaseProvider
        Dim f = RandomValueGen.GetRandomString(),
            v = RandomValueGen.GetRandomString()
        Dim c = new Condition(new SelectField(f), v)
        Assert.AreEqual("[" + f + "]='" + v + "'", c.ToString())
        c.UseDatabaseProvider(DatabaseProviders.Firebird)
        Assert.AreEqual("""" + f + """='" + v + "'", c.ToString())
    End Sub

    <Test()>
    Public Sub ConditionWithSelectFields_RespectsDatabaseProvider
        Dim f1 = RandomValueGen.GetRandomString(),
            f2 = RandomValueGen.GetRandomString()
        Dim c = new Condition(new SelectField(f1), new SelectField(f2))
        Assert.AreEqual("[" + f1 + "]=[" + f2 + "]", c.ToString())
        c.UseDatabaseProvider(DatabaseProviders.Firebird)
        Assert.AreEqual("""" + f1 + """=""" + f2 + """", c.ToString())
    End Sub
End Class
