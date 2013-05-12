
'Namespace System
   ' 
   ' ''' <summary> Static utility methods for dealing with the System (file system, registry, processes ...). </summary>
   ' Public NotInheritable Class SysUtils
   '     
   '     #Region "Private Fields"
   '         
   '         'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(MyClass.GetType.FullName)
   '         Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.SysUtils")
   '         
   '     #End Region
   '     
   '     #Region "Constructor"
   '         
   '         Private Sub New
   '             'Hides the Constructor
   '         End Sub
   '         
   '     #End Region
   '     
   '     #Region "Enums"
   '         
   '         '' <summary> Sorting types supported by sorting methods of this class. </summary>
   '         'Public Enum SortType As Integer
   '         '    
   '         '    ''' <summary> Numeric if possible, otherwise alphanumeric </summary>
   '         '    Numeric      = 0
   '         '    
   '         '    ''' <summary> Alphanumeric </summary>
   '         '    Alphanumeric = 1
   '         'End Enum
   '         
   '     #End Region
   '     
   '     #Region "Public Static Methods"
   '         
   '         ' ''' <summary> Tests wether a given registry key exists. </summary>
   '         ' ''' <param name="KeyName"> A registry key name, that begins with a valid registry root name and may or may not end with a backslash. </param>
   '         ' ''' <returns> True, if the key exists AND can be accessed with actual rights, otherwise False. </returns>
   '         ' ''' <remarks>Example for a registry key name: "HKEY_CURRENT_USER\MyTestKey\Key2\Key3". </remarks>
   '         'Public Shared Function RegKeyExists(ByVal KeyName As String)
   '         '    Dim TestValue As Object
   '         '    Dim success   As Boolean = false
   '         '    
   '         '    Try
   '         '        TestValue = Microsoft.Win32.Registry.GetValue(KeyName, "", "default")
   '         '        success   = (TestValue isNot Nothing)
   '         '        
   '         '    Catch ex as System.ArgumentException
   '         '        Logger.logError("RegValueExists(): ungültiger Stammschlüsselname in ValuePathName " & KeyName)
   '         '        
   '         '    Catch ex as System.IO.IOException
   '         '        Logger.logError("RegValueExists(): Der RegistryKey " & KeyName & " wurde zum Löschen markiert.")
   '         '        
   '         '    Catch ex as System.Security.SecurityException
   '         '        Logger.logDebug("RegValueExists(): Es fehlt die Berechtigung, um aus dem Registrierungsschlüssel " & KeyName & " zu lesen.")
   '         '        
   '         '    Catch ex as System.Exception
   '         '        Logger.logError("RegValueExists(): unbekannter Fehler")
   '         '    End Try
   '         '    
   '         '    Return success
   '         'End Function
   '         '
   '         ' ''' <summary> Tests wether a given registry value name exists. </summary>
   '         ' ''' <param name="ValuePathName"> A registry value name with full path, that begins with a valid registry root name. </param>
   '         ' ''' <returns> True, if the value exists AND can be accessed with actual rights, otherwise False. </returns>
   '         ' ''' <remarks> 
   '         ' ''' Example for a registry key name: "HKEY_CURRENT_USER\MyTestKey\Key2\Key3". 
   '         ' ''' If it ends with a backslash, the default key is tested.
   '         ' ''' </remarks>
   '         'Public Shared Function RegValueExists(ByVal ValuePathName As String)
   '         '    Const UniqValue As String = "@mns4u6e8le9k6fe3rkjui548@"
   '         '    Dim TestValue As Object
   '         '    Dim success   As Boolean = false
   '         '    
   '         '    Dim ValueName As String = ValuePathName.Right("\")
   '         '    Dim KeyName   As String = ValuePathName.Left(ValuePathName.Length - ValueName.Length)
   '         '    
   '         '    Try
   '         '        TestValue = Microsoft.Win32.Registry.GetValue(KeyName, ValueName, UniqValue)
   '         '        success   = ((TestValue isNot Nothing) and (TestValue <> UniqValue))
   '         '        
   '         '    Catch ex as System.ArgumentException
   '         '        Logger.logError("RegValueExists(): ungültiger Stammschlüsselname in ValuePathName " & ValuePathName)
   '         '        
   '         '    Catch ex as System.IO.IOException
   '         '        Logger.logError("RegValueExists(): Der RegistryKey " & KeyName & " wurde zum Löschen markiert.")
   '         '        
   '         '    Catch ex as System.Security.SecurityException
   '         '        Logger.logDebug("RegValueExists(): Es fehlt die Berechtigung, um aus dem Registrierungsschlüssel " & KeyName & " zu lesen.")
   '         '        
   '         '    Catch ex as System.Exception
   '         '        Logger.logError("RegValueExists(): unbekannter Fehler")
   '         '    End Try
   '         '    
   '         '    Return success
   '         'End Function
   '         
   '         
   '     #End Region
   '     
   '     #Region "Private Members"
   '         
   '         
   '     #End Region
   '     
   ' End Class
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
