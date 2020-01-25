
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.Runtime.InteropServices

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
         ''' <returns> <see cref="Rstyx.Utilities.Resources.Messages.Global_Label_BooleanTrue"/> or <see cref="Rstyx.Utilities.Resources.Messages.Global_Label_BooleanFalse"/>. </returns>
         ''' <param name="Value"> The boolean value to convert. </param>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ToDisplayString(Value As Boolean) As String
            Return If(Value, Rstyx.Utilities.Resources.Messages.Global_Label_BooleanTrue, Rstyx.Utilities.Resources.Messages.Global_Label_BooleanFalse)
        End Function
            
            ''' <summary> Replacement for <c>Boolean.TryParse</c> which converts strings localized by resources. </summary>
             ''' <param name="Result"> The parsing result. </param>
             ''' <param name="Value">  String to parse. </param>
             ''' <returns> <see langword="true"/> if <paramref name="Value"/> has been parsed successfull, otherwise <see langword="false"/>. </returns>
             ''' <remarks>
             ''' If <c>Boolean.TryParse</c> fails, then special parsing will be done for 
             ''' <see cref="Rstyx.Utilities.Resources.Messages.Global_Label_BooleanTrue"/> and
             ''' <see cref="Rstyx.Utilities.Resources.Messages.Global_Label_BooleanFalse"/> (not case sensitive).
             ''' If <paramref name="Value"/> contains is only one character, it will be successfuly parsed 
             ''' if it matches the first character of one of the resorce strings.
             ''' </remarks>
            <System.Runtime.CompilerServices.Extension()> 
            Public Function TryParse(<out> ByRef Result As Boolean, Value As String) As Boolean
                Dim success As Boolean = False
                
                If (Value.IsNotEmptyOrWhiteSpace()) Then
                    
                    success = Boolean.TryParse(Value, Result)
                    
                    If (Not success) Then
                        Select Case Value.ToLowerInvariant()
                            Case Rstyx.Utilities.Resources.Messages.Global_Label_BooleanTrue.ToLower()  :  Result = True  : success = True
                            Case Rstyx.Utilities.Resources.Messages.Global_Label_BooleanFalse.ToLower() :  Result = False : success = True
                        End Select
                    End If
                    
                    If (Not success) Then
                        If (Value.Length = 1) Then
                            Select Case Value.Left(1).ToLowerInvariant()
                                Case Rstyx.Utilities.Resources.Messages.Global_Label_BooleanTrue.Left(1).ToLower()  :  Result = True  : success = True
                                Case Rstyx.Utilities.Resources.Messages.Global_Label_BooleanFalse.Left(1).ToLower() :  Result = False : success = True
                            End Select
                        End If
                    End If
                End If
                
                Return success
            End Function
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
