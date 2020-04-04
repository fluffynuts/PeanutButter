Imports System.Collections

Namespace StatementBuilders

  Public Class ReadOnlyDictionary(Of TKey, TValue)
    Implements IDictionary(Of TKey, TValue)

    Private ReadOnly _dictionary As IDictionary(Of TKey, TValue)
    Public Sub New()
      _dictionary = New Dictionary(Of TKey, TValue)()
    End Sub

    Public Sub New(dict As IDictionary(Of TKey, TValue))
      _dictionary = New Dictionary(Of TKey, TValue)
      For Each kvp As KeyValuePair(Of TKey, TValue) In dict
        _dictionary(kvp.Key) = kvp.Value
      Next
    End Sub

    Public Sub Add(theItem As KeyValuePair(Of TKey, TValue)) Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Add
      ThrowReadOnly()
    End Sub

    Public Sub Add(key As TKey, value As TValue) Implements IDictionary(Of TKey, TValue).Add
      ThrowReadOnly()
    End Sub


    Public Function ContainsKey(k As TKey) As Boolean Implements IDictionary(Of TKey, TValue).ContainsKey
      Return _dictionary.ContainsKey(k)
    End Function

    Public ReadOnly Property Keys As ICollection(Of TKey) Implements IDictionary(Of TKey, TValue).Keys
      Get
        Return _dictionary.Keys
      End Get
    End Property

    Private Function ThrowReadOnly() as Boolean
      Throw New InvalidOperationException("ReadOnlyDictionary is ReadOnly. How rare.")
    End Function

    Public Sub Clear() Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Clear
      ThrowReadOnly()
    End Sub

    Public Function Contains(theItem As KeyValuePair(Of TKey, TValue)) As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Contains
      Return _dictionary.Contains(theItem)
    End Function

    Public Sub CopyTo(array() As KeyValuePair(Of TKey, TValue), arrayIndex As Integer) Implements ICollection(Of KeyValuePair(Of TKey, TValue)).CopyTo
      _dictionary.CopyTo(array, arrayIndex)
    End Sub

    Public ReadOnly Property Count As Integer Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Count
      Get
        Return _dictionary.Count
      End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).IsReadOnly
      Get
        Return True
      End Get
    End Property

    Public Function Remove(theItem As KeyValuePair(Of TKey, TValue)) As Boolean Implements ICollection(Of KeyValuePair(Of TKey, TValue)).Remove
      Return ThrowReadOnly()
    End Function

    Default Public Property Item(key As TKey) As TValue Implements IDictionary(Of TKey, TValue).Item
      Get
        Return _dictionary(key)
      End Get
      Set(value As TValue)
        ThrowReadOnly()
      End Set
    End Property

    Public Function Remove(key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).Remove
      Return ThrowReadOnly()
    End Function

    Public Function TryGetValue(key As TKey, ByRef value As TValue) As Boolean Implements IDictionary(Of TKey, TValue).TryGetValue
      Return _dictionary.TryGetValue(key, value)
    End Function

    Public ReadOnly Property Values As ICollection(Of TValue) Implements IDictionary(Of TKey, TValue).Values
      Get
        Return _dictionary.Values
      End Get
    End Property

    Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of TKey, TValue)) Implements IEnumerable(Of KeyValuePair(Of TKey, TValue)).GetEnumerator
      Return _dictionary.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
      Return _dictionary.GetEnumerator()
    End Function
  End Class
End NameSpace