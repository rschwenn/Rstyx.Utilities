
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Reflection
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

        ''' <summary>  Gets the value of a property given by (cascaded) name, from a given object. </summary>
         ''' <param name="Subject">      The object to inspect. </param>
         ''' <param name="PropertyPath"> Path to the property name, based on <paramref name="Subject"/>, i.e. "prop1" or "prop1.prop2.prop3" </param>
         ''' <param name="Flags">        Determines, wich properties should be considered. </param>
         ''' <returns> The found property value on success, otherwise <see langword="null"/>. </returns>
         ''' <remarks>
         ''' <para>
         ''' <paramref name="PropertyPath"/>: Path separator is a point. The first part has to be a direct property of <paramref name="Subject"/>. 
         ''' The last part is the property of interest, whose value will be returened.
         ''' </para>
         ''' <para>
         ''' If the returned value is <see langword="null"/>, either this is the property's value or the property hasn't been found.
         ''' </para>
         ''' </remarks>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="Subject"/> is <see langword="null"/>. </exception>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="PropertyPath"/> is <see langword="null"/> or empty or whitespace only. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function GetPropertyValue(Subject As Object, PropertyPath As String, Flags As BindingFlags) As Object
                
            If (Subject Is Nothing) Then Throw New System.ArgumentNullException("Subject")
            If (PropertyPath.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("PropertyPath")

            Dim RetValue        As Object = Nothing
            Dim PropertyNames() As String = PropertyPath.Split("."c)
            
            For i As Integer = 0 To PropertyNames.Count - 1
                
                Dim pi As PropertyInfo = Subject.GetType().GetProperty(PropertyNames(i), Flags)
                
                If (pi Is Nothing) Then Exit For

                Dim PropertyObject As Object = pi.GetValue(Subject)
                If (i < (PropertyNames.Count - 1)) Then
                    PropertyObject = GetPropertyValue(PropertyObject, PropertyNames(i + 1), Flags)
                End If
                RetValue = PropertyObject
            Next

            Return RetValue
        End Function

        ''' <summary>  Tries to set the value of a property given by (cascaded) name, to a given object. </summary>
         ''' <param name="Subject">      The object to set the property value to. </param>
         ''' <param name="PropertyPath"> Path to the property name, based on <paramref name="Subject"/>, i.e. "prop1" or "prop1.prop2.prop3". </param>
         ''' <param name="Value">        The property value to set. </param>
         ''' <param name="Flags">        Determines, wich properties should be considered. </param>
         ''' <remarks>
         ''' <para>
         ''' <paramref name="PropertyPath"/>: Path separator is a point. The first part has to be a direct property of <paramref name="Subject"/>. 
         ''' The last part is the property of interest, whose value will be returened.
         ''' </para>
         ''' <para>
         ''' If the returned value is <see langword="null"/>, either this is the property's value or the property hasn't been found.
         ''' </para>
         ''' </remarks>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="Subject"/> is <see langword="null"/>. </exception>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="PropertyPath"/> is <see langword="null"/> or empty or whitespace only. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Sub SetPropertyValue(Subject As Object, PropertyPath As String, Value As Object, Flags As BindingFlags)
                
            If (Subject Is Nothing) Then Throw New System.ArgumentNullException("Subject")
            If (PropertyPath.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("PropertyPath")

            Dim PropertyNames() As String = PropertyPath.Split("."c)
            
            For i As Integer = 0 To PropertyNames.Count - 1
                
                Dim pi As PropertyInfo = Subject.GetType().GetProperty(PropertyNames(i), Flags)
                
                If (pi Is Nothing) Then Exit For

                Dim PropertyObject As Object = pi.GetValue(Subject)
                If (i < (PropertyNames.Count - 1)) Then
                    SetPropertyValue(PropertyObject, PropertyNames(i + 1), Value, Flags)
                Else
                    ' Statement "pi.SetValue(Subject, Value)" complains about impossible conversion
                    ' from String to Kilometer, though these two line work like a charme:
                    '  Dim Km As New Kilometer()
                    '  Km = "1.2 + 345.678"
                    '
                    ' So we call the TypeConverter explicit:
                    Dim TargetObjet As Object = TypeDescriptor.GetConverter(pi.PropertyType).ConvertFromString(Value)
                    pi.SetValue(Subject, TargetObjet)
                End If
            Next
        End Sub
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
