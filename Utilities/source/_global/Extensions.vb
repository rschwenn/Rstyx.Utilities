
Imports System
Imports System.Linq
Imports System.Collections.Generic

'Namespace Extensions
    
    ''' <summary> Extension methods for various types. </summary>
    Public Module Extensions
        
        ''' <summary> Checks wether or not the given type isn't abstract and implements a certain interface. </summary>
         ''' <param name="Value">        The input type. </param>
         ''' <param name="TheInterface"> The interface to check for. </param>
         ''' <returns>                   <see langword="true"/> if value isn't an abstrcat class and implements the Interface. </returns>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="Value"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function IsImplementing(Value As Type, TheInterface As Type) As Boolean
            
            If (Value Is Nothing) Then Throw New System.ArgumentNullException("Value")
            
            Dim RetValue  As Boolean = False
            
            If (Not (Value.IsInterface Or Value.IsAbstract)) Then
                For Each CurrentInterface As Type In Value.GetInterfaces()
                    If (CurrentInterface Is TheInterface) Then
                        RetValue = True
                        Exit For
                    End If
                Next
            End If
            Return RetValue
        End Function
        
        ''' <summary> Converts <paramref name="SourceItems"/> into a dictionary. </summary>
         ''' <typeparam name="TKey">    Type of dictionary Keys. </typeparam>
         ''' <typeparam name="TValue">  Type of dictionary Values. </typeparam>
         ''' <param name="SourceItems"> The source list to convert. May be <see langword="null"/> </param>
         ''' <returns>                  A dictionary containing the source items. May be empty. </returns>
         ''' <remarks></remarks>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ToDictionary(Of TKey, TValue)(SourceItems As IEnumerable(Of KeyValuePair(Of  TKey, TValue))) As Dictionary(Of TKey, TValue)
            
            Dim RetDict As New Dictionary(Of TKey, TValue)
            
            If (SourceItems IsNot Nothing) Then
                For Each SourceItem As KeyValuePair(Of  TKey, TValue) In SourceItems
                    RetDict.Add(SourceItem.Key, SourceItem.Value)
                Next
            End If
            
            Return RetDict
        End Function
        
        ''' <summary> Converts this Boolean value to a string localized by resources. </summary>
         ''' <param name="Value"> The boolean value to convert. </param>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ToDisplayString(Value As Boolean) As String
            Return If(Value, Rstyx.Utilities.Resources.Messages.Global_Label_BooleanTrue, Rstyx.Utilities.Resources.Messages.Global_Label_BooleanFalse)
        End Function
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
