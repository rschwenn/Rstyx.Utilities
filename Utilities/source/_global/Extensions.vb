
Imports System

'Namespace Extensions
    
    ''' <summary> Extension methods for various types. </summary>
    Public Module Extensions
        
        ''' <summary> Checks wether or not the given type isn't abstrcat and implements a certain interface. </summary>
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
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
