
Imports System.Collections.Generic
Imports System.Linq

Namespace Collections
    
    ''' <summary> Extension methods for collections. </summary>
    Public Module CollectionExtensions
        
        ''' <summary> Finds the first occurence of a given value in a <see cref="System.Collections.Generic.IDictionary(Of TKey, TValue)"/> and gets it's key. </summary>
         ''' <typeparam name="TKey">   Type of keys. </typeparam>
         ''' <typeparam name="TValue"> Type of values. </typeparam>
         ''' <param name="dictionary"> The dictionary. </param>
         ''' <param name="Value">      The value to search for. </param>
         ''' <param name="FoundKey">   [Out] Will contain the key of the found value, otherwise it isn't changed. </param>
         ''' <returns>                 <see langword="true"/>, if the value has been found, otherwise <see langword="false"/>. </returns>
         ''' <remarks>                 For value types of TKey, the result is ambigious if the default value of TKey is returned (May be a real occurence or not)! </remarks>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="dictionary"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function findKeyByValue(Of TKey, TValue)(ByVal dictionary As IDictionary(Of TKey, TValue), Value As TValue, ByRef FoundKey As TKey) As Boolean
            FoundKey = getKeyByValue(dictionary, Value)
            Dim DefaultKey As TKey = Nothing
            Return (Not Object.Equals(FoundKey, DefaultKey))
        End Function
        
        ''' <summary> Finds the first occurence of a given value in a <see cref="System.Collections.Generic.IDictionary(Of TKey, TValue)"/> and returns it's key. </summary>
         ''' <typeparam name="TKey">   Type of keys. </typeparam>
         ''' <typeparam name="TValue"> Type of values. </typeparam>
         ''' <param name="dictionary"> The dictionary. </param>
         ''' <param name="Value">      The value to search for. </param>
         ''' <returns>                 The key of the found value, or default value of type parameter <c>TKey</c>. </returns>
         ''' <remarks>                 For value types of TKey, the result is ambigious if the default value of TKey is returned (May be a real occurence or not)! </remarks>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="dictionary"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function getKeyByValue(Of TKey, TValue)(ByVal dictionary As IDictionary(Of TKey, TValue), Value As TValue) As TKey
            
            If (dictionary Is Nothing) Then Throw New System.ArgumentNullException("dictionary")
            
            Dim RetKey      As TKey = Nothing
            Dim DefaultKvp  As KeyValuePair(Of TKey, TValue) = Nothing
            Dim FirstKvp    As KeyValuePair(Of TKey, TValue) = dictionary.FirstOrDefault( Function(kvp) ( kvp.Value.Equals(Value)))
            
            If (Not FirstKvp.Equals(DefaultKvp)) Then
                RetKey = FirstKvp.Key
            End If
            
            Return RetKey
        End Function
        
        
        ''' <summary> Finds and returns the first Item in a <see cref="ICollection(Of TItem)"/> which string representation equals a given string. </summary>
         ''' <typeparam name="TItem">    Type of Items. </typeparam>
         ''' <param name="collection">   The collection. </param>
         ''' <param name="ItemToString"> The string representation to search for. </param>
         ''' <param name="FoundItem">    [Out] Will contain the found Item, otherwise it isn't changed. </param>
         ''' <returns>                   <see langword="true"/>, if the Item has been found, otherwise <see langword="false"/>. </returns>
         ''' <remarks>                   For value types the result is ambigious if the default value is returned (May be a real occurence or not)! </remarks>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="collection"/> is <see langword="null"/>. </exception>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="ItemToString"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function findItemByString(Of TItem)(ByVal collection As ICollection(Of TItem), ItemToString As String, ByRef FoundItem As TItem) As Boolean
            FoundItem = getItemByString(collection, ItemToString)
            Dim DefaultItem As TItem = Nothing
            Return (Not Object.Equals(FoundItem, DefaultItem))
        End Function
        
        ''' <summary> Finds and returns the first Item in a <see cref="ICollection(Of TItem)"/> which string representation equals a given string. </summary>
         ''' <typeparam name="TItem">    Type of Items. </typeparam>
         ''' <param name="collection">   The collection. </param>
         ''' <param name="ItemToString"> The string representation to search for. </param>
         ''' <returns>                   The found Item, or default value of type parameter <c>TKey</c>. </returns>
         ''' <remarks>                   For value types the result is ambigious if the default value is returned (May be a real occurence or not)! </remarks>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="collection"/> is <see langword="null"/>. </exception>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="ItemToString"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function getItemByString(Of TItem)(ByVal collection As ICollection(Of TItem), ItemToString As String) As TItem
            
            If (collection Is Nothing) Then Throw New System.ArgumentNullException("collection")
            If (ItemToString Is Nothing) Then Throw New System.ArgumentNullException("ItemToString")
            
            Dim RetItem     As TItem = Nothing
            Dim DefaultItem As TItem = Nothing
            Dim FirstItem   As TItem = collection.FirstOrDefault( Function(oItem) ( oItem.ToString().ToLower() = ItemToString.ToLower()))
            
            If ((FirstItem ISNot Nothing) AndAlso (Not FirstItem.Equals(DefaultItem))) Then
                RetItem = FirstItem
            End If
            
            Return RetItem
        End Function
        
    End Module
    
End Namespace

' for jEdi  :collapseFolds=2::tabSize=4::indentSize=4:
