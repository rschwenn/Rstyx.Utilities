
Namespace Apps
    
    ''' <summary> Provides easy access to info about the current or a given application. </summary>
    Public Class AppInfo
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Apps.AppInfo")
            
            Private _Assembly           As System.Reflection.Assembly = Nothing
            Private _AssemblyName       As System.Reflection.AssemblyName = Nothing
            
            Private _Title              As String = Nothing
            Private _Version            As System.Version = Nothing
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new AppInfo for the calling assembly. </summary>
            Public Sub New()
                Me.New(System.Reflection.Assembly.GetCallingAssembly())
            End Sub
            
            ''' <summary> Creates a new AppInfo for a given assembly. </summary>
             ''' <param name="Assembly"> The assembly of interest. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Assembly"/> is <see langword="null"/>. </exception>
            Public Sub New(Assembly As System.Reflection.Assembly)
                
                If (Assembly Is Nothing) Then Throw New System.ArgumentNullException("Assembly")
                
                _Assembly = Assembly
                
                initAppInfo()
            End Sub
            
        #End Region
        
        #Region "ReadOnly Properties"
            
            ''' <summary> Returns the assembly title if set, otherwise the assembly name. </summary>
            Public ReadOnly Property Title() As String
                Get
                    Return _Title
                End Get
            End Property
            
            ''' <summary> Returns the assembly's version object. </summary>
            Public ReadOnly Property Version() As System.Version
                Get
                    Return _Version
                End Get
            End Property
            
        #End Region
        
        #Region "Private members"
            
            ''' <summary> Collects all information. </summary>
            Private Sub initAppInfo()
                'Try
                    ' Preliminaries
                    _AssemblyName = New System.Reflection.AssemblyName(_Assembly.FullName)
                    
                    ' Assembly title
                    Dim Attributes As Object() = _Assembly.GetCustomAttributes(GetType(System.Reflection.AssemblyTitleAttribute), False)
                    If (Attributes.Length > 0) Then
                        _Title = CType(Attributes(0), System.Reflection.AssemblyTitleAttribute).Title
                    End If
                    If (String.IsNullOrWhiteSpace(_Title)) Then
                        _Title = System.IO.Path.GetFileNameWithoutExtension(_Assembly.Location)
                    End If
                    
                    ' Assembly version
                    _Version = _AssemblyName.Version
                    
                'Catch ex As System.Exception
                '    Logger.logError(ex, "initAppInfo(): Fehler beim Bestimmen der Anwendungsinformationen.")
                'End Try
            End Sub
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
