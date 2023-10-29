
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Resources
Imports System.Runtime.InteropServices

Imports PGK.Extensions

Imports Rstyx.Utilities.StringUtils


'Namespace Enums
    
    '' <summary> Static utility methods for dealing with Enums. </summary>
    'Public NotInheritable Class EnumUtils
        
        '#Region "Private Fields"
        '    
        '    'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger(MyClass.GetType.FullName)
        '    Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.EnumUtils")
        '    
        '#End Region
        '
        '#Region "Constructor"
        '    
        '    Private Sub New
        '        'Hides the Constructor
        '    End Sub
        '    
        '#End Region
        '
        '#Region "General Methods"
        '    
        '    ''' <summary> String formatting like in C or awk (does not support %e, %E, %g, %G). </summary>
        '     ''' <param name="FormatString"> A String like "test: %s  %11.3f" </param>
        '     ''' <param name="Parms"> Parameter list or Array (Nested 1d-Arrays as single parameters are supported). </param>
        '     ''' <returns> The FormatString with expanded variables. </returns>
        '    Public Shared Function Sprintf(ByVal FormatString As String, ParamArray Parms() As Object) As String
        '        Return sprintf(False, FormatString, Parms)
        '    End Function
        '    
        '#End Region
        '
        '#Region "Private Members"
        '    
        '    ''' <summary> Returns the first character from a buffer string and removes it from the buffer. </summary>
        '     ''' <param name="Buffer"> The working string. </param>
        '    Private Shared Function NextChar(ByRef Buffer As String) As String
        '        Dim FirstChar = Buffer.Substring(0, 1)
        '        Buffer = Buffer.Substring(1)
        '        Return FirstChar
        '    End Function
        '    
        '#End Region
        
    'End Class
    
    ''' <summary> Extension methods for enums or other types dealing with enums. </summary>
    Public Module EnumExtensions
        
        ''' <summary> For every (already in this session used) UI culture there is one StringDictionary cache. </summary>
        Private ReadOnly DisplayStringCache     As New Dictionary(Of String, System.Collections.Specialized.StringDictionary)
        
        Private ThisAssemblyResourceProviders   As IEnumerable(Of Type) = Nothing
        
        ''' <summary> Returns a localized display string for the given Enum value from resources. </summary>
         ''' <param name="Value"> The Enum value to display. </param>
         ''' <returns>            Localized string if available in resources, otherwise Enum.ToString(). </returns>
         ''' <remarks>
         ''' The display string is searched this way:
         ''' <para>
         ''' Assemblies that are searched:
         ''' <list type="bullet">
         ''' <item><description> The assembly where the given enum is defined. </description></item>
         ''' <item><description> This assembly (Rstyx.Utilities.dll). </description></item>
         ''' </list>
         ''' </para>
         ''' <para>
         ''' Classes that are searched:
         ''' <list type="bullet">
         ''' <item><description> Full name ends with ".My.Resources.Resources". </description></item>
         ''' <item><description> Full name ends with ".EnumDisplayStrings". </description></item>
         ''' </list>
         ''' </para>
         ''' <para>
         ''' The class has to provide a property whose name matches the enum value:
         ''' <list type="bullet">
         ''' <item><description> <b>{Enum_Full_Name}</b> (where "." and "+" are replaced by "_") </description></item>
         ''' <item><description> If the class name ends with ".My.Resources.Resources", the property name has to be <b>Display_Enum_</b>{Enum_Full_Name} </description></item>
         ''' </list>
         ''' </para>
         ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ToDisplayString(Value As System.Enum) As String
            Dim RetValue As String = "{Invalid Enum value}"
            Try
                ' Fallback.
                RetValue = Value.ToString()
                
                Dim EnumType            As System.Type = Value.GetType()
                Dim ShortResourceKey    As String = EnumType.FullName.Replace("."c, "_"c).Replace("+"c, "_"c) & "_" & Value.ToString()
                Dim LongResourceKey     As String = "Display_Enum_" & ShortResourceKey
                Dim CultureName         As String = System.Threading.Thread.CurrentThread.CurrentUICulture.Name
                
                ' Create cache for the current UI culture.
                If (Not DisplayStringCache.ContainsKey(CultureName)) Then
                    DisplayStringCache.Add(CultureName, New System.Collections.Specialized.StringDictionary())
                End If
                
                ' Retrieve the display name from resource and add it to the cache.
                If (Not DisplayStringCache.Item(CultureName).ContainsKey(ShortResourceKey)) Then
                    
                    ' Add default string (enum's name) to avoid repeated exceptions.
                    DisplayStringCache.Item(CultureName).Add(ShortResourceKey, Value.ToString())
                    
                    ' Assembly that defines the Enum: Find Classes (Modules) whose full name is ending to ".My.Resources.Resources" or ".EnumDisplayStrings".
                    Dim EnumAssemblyResourceProviders As IEnumerable(Of Type) = From ty As System.Type In EnumType.Assembly.GetTypes() 
                                                                                Where ty.FullName.IsMatchingTo("\.My\.Resources\.Resources$|\.EnumDisplayStrings$")
                    '
                    ' This Assembly: Find Classes (Modules) whose full name is ending to ".My.Resources.Resources" or ".EnumDisplayStrings".
                    If (ThisAssemblyResourceProviders Is Nothing) Then
                        ThisAssemblyResourceProviders = From ty As System.Type In Assembly.GetExecutingAssembly().GetTypes() 
                                                        Where ty.FullName.IsMatchingTo("\.My\.Resources\.Resources$|\.EnumDisplayStrings$")
                    End If
                    
                    ' Collect all found classes from the two assemblies.
                    Dim ResourceProviders  As List(Of Type) = EnumAssemblyResourceProviders.ToList()
                    ResourceProviders.AddRange(ThisAssemblyResourceProviders)
                    '
                    ' Get property whose name is the matching resource key (see above).
                    For Each ResourceProvider As Type In ResourceProviders
                        
                        ' Choose ResourceKey.
                        Dim ResourceKey As String = LongResourceKey
                        If (ResourceProvider.FullName.IsMatchingTo("\.EnumDisplayStrings$")) Then ResourceKey = ShortResourceKey
                        
                        Dim EnumResourceKeyProperty As PropertyInfo = ResourceProvider.GetProperty(ResourceKey, BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Static)
                        
                        If (EnumResourceKeyProperty IsNot Nothing) Then
                            Dim DisplayString  As String = CStr(EnumResourceKeyProperty.GetValue(Nothing, Nothing))
                            If (DisplayString.IsNotEmptyOrWhiteSpace()) Then
                                DisplayStringCache.Item(CultureName).Item(ShortResourceKey) = DisplayString
                                Exit For
                            End If
                        End If
                    Next
                End If
                
                ' This enum value should be in the cache now...
                RetValue = DisplayStringCache.Item(CultureName).Item(ShortResourceKey)
                
            Catch ex as System.Exception
                'Logger.LogError(ex, StringUtils.Sprintf("ToDisplayString(): Fehler bei Suche nach DisplayString .", FolderName))
                System.Diagnostics.Trace.WriteLine(ex)
            End Try
            Return RetValue
        End Function
        
        ''' <summary> Returns a localized display string for the given <c>Nullable(Of Enum)</c> value from resources. </summary>
         ''' <typeparam name="TEnum">         The Enum type wrapped by the <c>Nullable</c>. </typeparam>
         ''' <param name="NullableStructure"> The <c>Nullable(Of Enum)</c> value to display. </param>
         ''' <returns>                        If <c>T</c> is an <c>Enum</c> then the localized string available in resources, otherwise <c>T.ToString()</c>. </returns>
         ''' <exception cref="System.ArgumentException"> Type parameter is not an Enum type. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ToDisplayString(Of TEnum As Structure)(NullableStructure As Nullable(Of TEnum)) As String
            
            Dim TargetType As Type = GetType(TEnum)
            If (Not (TargetType.BaseType.Name = GetType(System.Enum).Name)) Then
                Throw New System.ArgumentException(sprintf(Rstyx.Utilities.Resources.Messages.EnumUtils_InvalidTargetType, TargetType.Name), "TEnum")
            End If
            Dim RetValue  As String = String.Empty
            
            If (NullableStructure.HasValue) Then
                
                If (NullableStructure.GetType().IsEnum) Then
                    RetValue = ObjectEnumToDisplayString(NullableStructure.Value)
                Else
                    RetValue = NullableStructure.Value.ToString()
                End If
                
            End If
            
            Return RetValue
        End Function
        
        ''' <summary> Converts a localized display string for the given Enum type to the underlying Enum object. </summary>
         ''' <typeparam name="TEnum">         The target Enum type. </typeparam>
         ''' <param name="Result">            [Out] The resulting Enum object. It's type will be treated as target Enum type. </param>
         ''' <param name="EnumDisplayString"> The localized display string of a Enum of type . </param>
         ''' <returns> <see langword="true"/> on success, otherwise <see langword="false"/>. </returns>
         ''' <remarks>
         ''' <para>
         ''' <paramref name="Result"/> won't be changed, if parsing isn't successfull.
         ''' </para>
         ''' <para>
         ''' See <see cref="ToDisplayString"/> for more information.
         ''' </para>
         ''' </remarks>
         ''' <exception cref="System.ArgumentException"> <paramref name="Result"/> is not an Enum type. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function TryParseDisplayString(Of TEnum As Structure)(<Out> ByRef Result As TEnum, EnumDisplayString As String) As Boolean
            
            Dim TargetType As Type = GetType(TEnum)
            If (Not (TargetType.BaseType.Name = GetType(System.Enum).Name)) Then
                Throw New System.ArgumentException(sprintf(Rstyx.Utilities.Resources.Messages.EnumUtils_InvalidTargetType, TargetType.Name), "TEnum")
            End If
            Dim RetValue  As Boolean = False
            Try
                Dim EnumValue As TEnum = Nothing
                
                For Each i As Integer In System.Enum.GetValues(GetType(TEnum))
                    
                    If ([Enum].TryParse(i, EnumValue)) Then
                    
                        If (ToDisplayString(EnumValue) = EnumDisplayString) Then
                            Result   = EnumValue
                            RetValue = True
                        End If
                    End If
                Next
            Catch ex as System.Exception
                System.Diagnostics.Trace.WriteLine(ex)
            End Try
            Return RetValue
        End Function
        
        ''' <summary> Helps <c>ToDisplayString(Of T As Structure)</c> to resolve the real method. </summary>
        ''' <param name="Value"> Enum as object. </param>
        ''' <returns>            The display string or String.Empty. </returns>
        Private Function ObjectEnumToDisplayString(Value As Object) As String
            Dim RetValue  As String = String.Empty
            If (Value IsNot Nothing) Then
                If (Value.GetType().IsEnum) Then
                    RetValue = ToDisplayString(Value)
                End If
            End If
            Return RetValue
        End Function
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
