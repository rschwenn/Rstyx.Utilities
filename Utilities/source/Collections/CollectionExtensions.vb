Imports System.Collections.Generic
Imports System.Linq


Namespace Collections
    
    ''' <summary> Extension methods for collections. </summary>
    Public Module CollectionExtensions
        
        ''' <summary> Finds the first occurence of a given value in a "System.Collections.Generic.IDictionary(Of TKey, TValue)" and gets it's key. </summary>
         ''' <typeparam name="TKey">   Type of keys. </typeparam>
         ''' <typeparam name="TValue"> Type of values. </typeparam>
         ''' <param name="dictionary"> The dictionary. </param>
         ''' <param name="Value">      The value to search for. </param>
         ''' <param name="FoundKey">   [Out] Will contain the key of the found value, otherwise it isn't changed. </param>
         ''' <returns>                 True, if the value has been found, otherwise False. </returns>
         ''' <remarks>                 For value types of TKey, the result is ambigious if the default value of TKey is returned (May be a real occurence or not)! </remarks>
         ''' <exception cref="T:System.ArgumentNullException"> <paramref name="dictionary"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function findKeyByValue(Of TKey, TValue)(ByVal dictionary As IDictionary(Of TKey, TValue), Value As TValue, ByRef FoundKey As TKey) As Boolean
            FoundKey = getKeyByValue(dictionary, Value)
            Return (FoundKey IsNot Nothing)
        End Function
        
        ''' <summary> Finds the first occurence of a given value in a "System.Collections.Generic.IDictionary(Of TKey, TValue)" and returns it's key. </summary>
         ''' <typeparam name="TKey">   Type of keys. </typeparam>
         ''' <typeparam name="TValue"> Type of values. </typeparam>
         ''' <param name="dictionary"> The dictionary. </param>
         ''' <param name="Value">      The value to search for. </param>
         ''' <returns>                 The key of the found value, or <see langword="null"/>. </returns>
         ''' <remarks>                 For value types of TKey, the result is ambigious if the default value of TKey is returned (May be a real occurence or not)! </remarks>
         ''' <exception cref="T:System.ArgumentNullException"> <paramref name="dictionary"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function getKeyByValue(Of TKey, TValue)(ByVal dictionary As IDictionary(Of TKey, TValue), Value As TValue) As TKey
            Dim RetKey As TKey = Nothing
            If (dictionary is Nothing) Then Throw New System.ArgumentNullException("dictionary")
            
            Dim DefaultKvp As KeyValuePair(Of TKey, TValue) = Nothing
            Dim FirstKvp As KeyValuePair(Of TKey, TValue) = dictionary.FirstOrDefault( Function(kvp) ( kvp.Value.Equals(Value)))
            If (Not FirstKvp.Equals(DefaultKvp)) Then
                RetKey = FirstKvp.Key
            End If
            
            Return RetKey
        End Function
        
    End Module
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
