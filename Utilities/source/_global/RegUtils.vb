
'Namespace System
    
    ''' <summary> Static utility methods for dealing with the Windows registry. </summary>
    Public NotInheritable Class RegUtils
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger(MyClass.GetType.FullName)
            Private Shared ReadOnly Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.RegUtils")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Static Methods"
            
            ''' <summary> Tests wether a given registry key exists. </summary>
             ''' <param name="KeyPathName"> A registry key name, that begins with a valid registry root name and may or may not end with a backslash. </param>
             ''' <returns> True, if the key exists AND can be accessed with actual permissions, otherwise False. </returns>
             ''' <remarks> Example for a registry key name: "HKEY_CURRENT_USER\MyTestKey\Key2\Key3". </remarks>
            Public Shared Function KeyExists(ByVal KeyPathName As String) As Boolean
                
                Dim success As Boolean = False
                
                Try
                    Dim TestValue As Object = Microsoft.Win32.Registry.GetValue(KeyPathName, "", "default")
                    success = (TestValue IsNot Nothing)
                    
                Catch ex as System.ArgumentException
                    Logger.LogError(ex, "KeyExists(): ungültiger Stammschlüsselname in ValuePathName " & KeyPathName)
                    
                Catch ex as System.IO.IOException
                    Logger.LogError(ex, "KeyExists(): Der RegistryKey " & KeyPathName & " wurde zum Löschen markiert.")
                    
                Catch ex as System.Security.SecurityException
                    Logger.LogError("KeyExists(): Es fehlt die Berechtigung, um aus dem Registrierungsschlüssel " & KeyPathName & " zu lesen.")
                    
                Catch ex as System.Exception
                    Logger.LogError(ex, "KeyExists(): unbekannter Fehler")
                End Try
                
                Return success
            End Function
            
            ''' <summary> Tests wether a given registry value name exists. </summary>
             ''' <param name="ValuePathName"> A registry value name with full path, that begins with a valid registry root name. </param>
             ''' <returns> True, if the value exists AND can be accessed with actual permissions, otherwise False. </returns>
             ''' <remarks> 
             ''' Example for a registry key name: "HKEY_CURRENT_USER\MyTestKey\Key2\Key3". 
             ''' If it ends with a backslash, the default value is tested.
             ''' </remarks>
            Public Shared Function ValueExists(ByVal ValuePathName As String) As Boolean
                
                Const UniqValue As String = "@mns4u6e8le9k6fe3rkjui548@"
                
                Dim success     As Boolean = false
                Dim ValueName   As String = getValueName(ValuePathName)
                Dim KeyPathName As String = getKeyPathName(ValuePathName)
                
                Try
                    Dim TestValue As Object = Microsoft.Win32.Registry.GetValue(KeyPathName, ValueName, UniqValue)
                    success = ((TestValue isNot Nothing) AndAlso (not TestValue.ToString().Equals(UniqValue)))
                    
                Catch ex as System.ArgumentException
                    Logger.LogError(ex, "ValueExists(): ungültiger Stammschlüsselname in ValuePathName " & ValuePathName)
                    
                Catch ex as System.IO.IOException
                    Logger.LogError(ex, "ValueExists(): Der RegistryKey " & KeyPathName & " wurde zum Löschen markiert.")
                    
                Catch ex as System.Security.SecurityException
                    Logger.LogError("ValueExists(): Es fehlt die Berechtigung, um aus dem Registrierungsschlüssel " & KeyPathName & " zu lesen.")
                    
                Catch ex as System.Exception
                    Logger.LogError(ex, "ValueExists(): unbekannter Fehler")
                End Try
                
                Return success
            End Function
            
            ''' <summary> Failure tolerant reading: Returns the value of the given ValuePathName if possible, otherwise Nothing.  </summary>
             ''' <param name="ValuePathName"> A registry value name with full path. If it ends with a backslash, the default value is meant. </param>
             ''' <returns> The value of the given ValuePathName if possible, otherwise Nothing. </returns>
            Public Shared Function GetValue(ByVal ValuePathName As String) As Object
                Dim Value  As Object = Nothing
                If (ValueExists(ValuePathName)) Then
                    Value = Microsoft.Win32.Registry.GetValue(getKeyPathName(ValuePathName), getValueName(ValuePathName), Nothing)
                End If
                Return Value
            End Function
            
            ''' <summary> Failure tolerant reading: Returns the value of the given ValuePathName as string if possible, otherwise Nothing.  </summary>
             ''' <param name="ValuePathName"> A registry value name with full path. If it ends with a backslash, the default value is meant. </param>
             ''' <returns> The value of the given ValuePathName if possible, otherwise Nothing. </returns>
             ''' <remarks> This is only a short hand for using "getValue(Of String)(ValuePathName)". </remarks>
            Public Shared Function GetStringValue(ByVal ValuePathName As String) As String
                Dim Value  As String = Nothing
                Try
                    Value = CStr(getValue(ValuePathName))
                Catch ex As System.Exception
                    Logger.LogDebug("getStringValue(): Der Wert " & ValuePathName & " konnte nicht in einen String konvertiert werden.")
                End Try 
                Return Value
            End Function
            
            ''' <summary> Failure tolerant reading: Returns the value of the given ValuePathName as the given Type if possible, otherwise the initial value of this type. </summary>
             ''' <typeparam name="T">         Type of the return value. </typeparam>
             ''' <param name="ValuePathName"> A registry value name with full path. If it ends with a backslash, the default value is meant. </param>
             ''' <returns>                    The value of the given ValuePathName as the given Type if possible, otherwise the default value of this type. </returns>
             ''' <remarks> 
             ''' If any error occurs while retrieving or converting the vaue, then the initial value of the given Type is returned. 
             ''' CAUTION: I.e. in case of basic numeric types a return value of "0" could mean both that this value has been read successfully or an error has been occured. 
             ''' </remarks>
            Public Shared Function GetValue(Of T)(ByVal ValuePathName As String) As T
                Dim ValueO  As Object
                Dim ValueT  As T = Nothing
                Try
                    ValueO = GetValue(ValuePathName)
                    ValueT = CType(ValueO, T)
                Catch ex As System.Exception
                    Logger.LogDebug("getValue(): Der Wert " & ValuePathName & " konnte nicht in den Typ " & GetType(T).ToString() & " konvertiert werden.")
                End Try 
                Return ValueT
            End Function
            
            ''' <summary> Failure tolerant writing: Sets the value of the given ValuePathName if possible.  </summary>
             ''' <param name="ValuePathName"> A registry value name with full path. If it ends with a backslash, the default value is meant. </param>
             ''' <param name="Value"> The value to write into the registry. </param>
             ''' <param name="ValueKind"> The kind of value to set. </param>
             ''' <returns> True, if the value has been set successfully, otherwise false. </returns>
            Public Shared Function SetValue(ByVal ValuePathName As String, byVal Value As Object, ValueKind As Microsoft.Win32.RegistryValueKind) As Boolean
                Dim success  As Boolean = false
                Try
                    Microsoft.Win32.Registry.SetValue(getKeyPathName(ValuePathName), getValueName(ValuePathName), Value, ValueKind)
                    success = True
                Catch ex As System.Exception
                    Logger.LogError(ex, "setValue(): Fehler beim Schreiben des Wertes " & ValuePathName & ".")
                End Try 
                Return success
            End Function
            
            ''' <summary> Failure tolerant writing: Sets the string value of the given ValuePathName if possible.  </summary>
             ''' <param name="ValuePathName"> A registry value name with full path. If it ends with a backslash, the default value is meant. </param>
             ''' <param name="Value"> The string value to write into the registry. </param>
             ''' <returns> True, if the value has been set successfully, otherwise false. </returns>
            Public Shared Function SetValue(ByVal ValuePathName As String, byVal Value As String) As Boolean
                Return setValue(ValuePathName, Value,Microsoft.Win32.RegistryValueKind.String)
            End Function
            
            ''' <summary> Gets the value name part from a full ValuePathName. </summary>
             ''' <param name="ValuePathName"> A registry value name with full path, that begins with a valid registry root name. </param>
             ''' <returns> The name of the value, which is the part beyond the last backslash. If empty, then it's the default value. </returns>
             ''' <remarks> If ValuePathName ends with a backslash, then an empty string is returned, which indicates the default value. </remarks>
            Public Shared Function GetValueName(ByVal ValuePathName As String) As String
                Return ValuePathName.Right("\")
            End function
            
            ''' <summary> Gets the key name part from a full ValuePathName. </summary>
             ''' <param name="ValuePathName"> A registry value name with full path, that begins with a valid registry root name. </param>
             ''' <returns> The path name of the key, which is the part until the last backslash (including backslash). </returns>
             ''' <remarks> If ValuePathName ends with a backslash, then the whole ValuePathName is returned. </remarks>
            Public Shared Function GetKeyPathName(ByVal ValuePathName As String) As String
                Return ValuePathName.Left(ValuePathName.Length - getValueName(ValuePathName).Length)
            End function
            
            ''' <summary> Extracts a path of an existing application from a registry string value. </summary>
             ''' <param name="ValuePathName"> A registry value name with full path. If it ends with a backslash, the default value is meant. </param>
             ''' <returns> The application path of an existing file, otherwise String.Empty. </returns>
             ''' <remarks>
             ''' <para>
             '''  If read value starts with a double quote, the string between quotes is recognized as path.
             ''' </para>
             ''' <para>
             '''  If read value doesn't starts with a double quote, the string until the first space is recognized as path.
             ''' </para>
             '''  </remarks>
            Public Shared Function GetApplicationPath(ByVal ValuePathName As String) As String
                Dim AppPath  As String = Nothing
                Logger.LogDebug("getApplicationPath(): Pfad\Name einer Programmdatei ermitteln aus RegistryValue '" & ValuePathName & "'.")
                
                if (ValueExists(ValuePathName)) then
                    AppPath = getStringValue(ValuePathName)
                    If (AppPath.IsNotNull) Then
                        Logger.LogDebug(StringUtils.Sprintf("getApplicationPath(): Inhalt des RegistryValue: '%s'", AppPath))
                        AppPath = AppPath.Trim()
                        ' extract Path:
                        if (AppPath.Left(1) = """") then
                            ' Path is quoted => Path is string between quotes.
                            AppPath = AppPath.Substring("""", """", false)
                        else
                            ' Path isn't quoted => Path is string from first character until first space or end.
                            AppPath = AppPath.Left(" ", false)
                        end if
                    end if
                end if
                Logger.LogDebug("getApplicationPath(): Ermittelter Pfad\Name lautet: '" & AppPath & "'")
                
                if (System.IO.File.Exists(AppPath)) then
                    Logger.LogDebug("getApplicationPath(): OK - '" & AppPath & "' existiert.")
                else
                    Logger.LogDebug("getApplicationPath(): Datei '" & AppPath & "' existiert nicht!")
                    AppPath = String.Empty
                end if
                
                Return AppPath
            End Function
            
        #End Region
        
    End Class
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
