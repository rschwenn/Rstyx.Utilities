Imports System.Reflection

Namespace Apps
    
    ''' <summary> Provides easy access to info about an application. </summary>
    Public Class AppInfo
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Apps.AppInfo")
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new empty AppInfo. </summary>
            Public Sub New()
                'Me.New(System.Reflection.TargetAssembly.GetCallingAssembly())
            End Sub
            
            ''' <summary> Creates a new AppInfo for a given assembly. </summary>
             ''' <param name="TargetAssembly"> The assembly of interest. May be <see langword="null"/>. </param>
            Public Sub New(TargetAssembly As Assembly)
                TryParseAssembly(TargetAssembly)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary>The assembly's title. </summary>
            Public Property Title() As String = Nothing
            
            ''' <summary> The assembly's version object. </summary>
            Public Property Version() As New System.Version
            
        #End Region
        
        #Region "Members"
            
            ''' <summary> Parses information from an assembly into this AppInfo. </summary>
             ''' <param name="TargetAssembly"> The assembly of interest. May be <see langword="null"/>. </param>
            Public Sub TryParseAssembly(TargetAssembly As Assembly)
                'Try
                If (TargetAssembly IsNot Nothing ) Then
                    
                    Dim TargetAssemblyName As AssemblyName = New AssemblyName(TargetAssembly.FullName)
                    
                    ' TargetAssembly title
                    Title = TargetAssemblyName.Name
                    If (Title.IsEmptyOrWhiteSpace()) Then
                        Dim Attributes As Object() = TargetAssembly.GetCustomAttributes(GetType(AssemblyTitleAttribute), False)
                        If (Attributes.Length > 0) Then
                            Title = CType(Attributes(0), AssemblyTitleAttribute).Title
                        End If
                    End If
                    If (Title.IsEmptyOrWhiteSpace()) Then
                        Title = System.IO.Path.GetFileNameWithoutExtension(TargetAssembly.Location)
                    End If
                    
                    ' TargetAssembly version
                    Version = TargetAssemblyName.Version
                End If
                    
                'Catch ex As System.Exception
                '    Logger.LogError(ex, "initAppInfo(): Fehler beim Bestimmen der Anwendungsinformationen.")
                'End Try
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns title and version as concatenated string. </summary>
            Public Overrides Function ToString() As String
                Return Me.Title & " " & Me.Version.ToString()
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
