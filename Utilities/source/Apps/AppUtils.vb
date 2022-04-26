
Imports System
Imports System.IO
Imports System.Linq
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Threading

Namespace Apps
    
    ''' <summary> Static properties and utility methods for dealing with (external) Applications. </summary>
    Public NotInheritable Class AppUtils
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger              As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Apps.AppUtils")
            
            Private Shared ReadOnly SyncHandle          As New Object()
            Private Shared _Instance                    As AppUtils = Nothing
            
            Private Shared _AppPathUltraEdit            As String = Nothing
            Private Shared _AppPathCrimsonEditor        As String = Nothing
            Private Shared _AppPathJava                 As String = Nothing
            Private Shared _AppPathJEdit                As String = Nothing
            Private Shared _JAVA_HOME                   As String = Nothing
            Private Shared _JEDIT_HOME                  As String = Nothing
            Private Shared _JEDIT_SETTINGS              As String = Nothing
            
            Private Shared _AvailableEditors            As Dictionary(Of SupportedEditors, EditorInfo) = Nothing
            Private Shared _CurrentEditor               As SupportedEditors
            
            Private Shared ReadOnly ClassInitialized    As Boolean = InitializeStaticVariables()
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New()
                Logger.logDebug("New(): Instantiation starts.")
            End Sub
            
            Private Shared Function InitializeStaticVariables() As Boolean
                Logger.logDebug("initializeStaticVariables(): Initialization of static variables starts ...")
                
                _CurrentEditor = InitCurrentEditor()
                
                Return True
            End Function
            
        #End Region
        
        #Region "Enums"
            
            ''' <summary> Editors supported by <see cref="AppUtils" /> class. </summary>
            Public Enum SupportedEditors As Integer
                
                ''' <summary> No editor determined or available. </summary>
                None = 0
                
                ''' <summary> UltraEdit </summary>
                UltraEdit = 1
                
                ''' <summary> jEdit </summary>
                jEdit = 2
                
                ''' <summary> Crimson Editor </summary>
                CrimsonEditor = 3
            End Enum
            
        #End Region
        
        #Region "ReadOnly Properties"
            
            ''' <summary>  Returns the one and only AppUtils instance to allow WPF two way binding. </summary>
            Public Shared ReadOnly Property Instance() As AppUtils
                Get
                    SyncLock (SyncHandle)
                        If (_Instance Is Nothing) Then
                            _Instance = New AppUtils
                            Logger.logDebug("Instance [Get]: AppUtils instantiated.")
                        End If
                        Return _Instance
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns the path of the existing Crimson Editor application file. </summary>
             ''' <remarks>
             ''' <para>
             ''' Returns an empty String if the application file hasn't been found.
             ''' </para>
             ''' <para>
             ''' Cedt.exe is searched for in the following places:
             ''' </para>
             ''' <list type="number">
             '''     <item>
             '''         <description> in HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Crimson Editor 3.72\DisplayIcon\ (version 3.72) </description>
             '''     </item>
             '''     <item>
             '''         <description> in G:\Tools\Crimson Editor </description>
             '''     </item>
             '''     <item>
             '''         <description> in "%PROGRAMFILES%\Crimson Editor" </description>
             '''     </item>
             '''     <item>
             '''         <description> in "%PROGRAMFILES%\Tools\Crimson Editor" </description>
             '''     </item>
             '''     <item>
             '''         <description> in %PATH% without subdirectories </description>
             '''     </item>
             ''' </list>
             ''' </remarks>
            Public Shared ReadOnly Property AppPathCrimsonEditor() As String
                Get
                    SyncLock (SyncHandle)
                        If (_AppPathCrimsonEditor Is Nothing) Then
                            _AppPathCrimsonEditor = GetAppPathCrimsonEditor()
                        End If
                        Return _AppPathCrimsonEditor
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns the path of the existing UltraEdit application file. </summary>
             ''' <remarks>
             ''' <para>
             ''' Returns an empty String if the application file hasn't been found.
             ''' </para>
             ''' <para>
             ''' UEdit*.exe and UltaEdit*.exe are searched for in the following places:
             ''' </para>
             ''' <list type="number">
             '''     <item>
             '''         <description> %PROGRAMFILES%\IDM Computer Solutions\UltraEdit\ </description>
             '''     </item>
             '''     <item>
             '''         <description> %PROGRAMFILES(X86)%\IDM Computer Solutions\UltraEdit\ </description>
             '''     </item>
             '''     <item>
             '''         <description> %PROGRAMW6432%\IDM Computer Solutions\UltraEdit\ </description>
             '''     </item>
             '''     <item>
             '''         <description> in HKEY_CLASSES_ROOT\UltraEdit.txt\shell\open\command\ (version 11) </description>
             '''     </item>
             '''     <item>
             '''         <description> in HKEY_CLASSES_ROOT\Applications\UEdit32.exe\shell\edit\command\ (version 6..10) </description>
             '''     </item>
             '''     <item>
             '''         <description> in HKEY_CLASSES_ROOT\UltraEdit-32 Document\shell\open\command\ (version 6..10) </description>
             '''     </item>
             '''     <item>
             '''         <description> in %PATH% without subdirectories </description>
             '''     </item>
             ''' </list>
             ''' </remarks>
            Public Shared ReadOnly Property AppPathUltraEdit() As String
                Get
                    SyncLock (SyncHandle)
                        If (_AppPathUltraEdit Is Nothing) Then
                            _AppPathUltraEdit = GetAppPathUltraEdit()
                        End If
                        Return _AppPathUltraEdit
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns the path of the existing Java application file. </summary>
             ''' <remarks>
             ''' <para>
             ''' Returns an empty String if the application file hasn't been found.
             ''' </para>
             ''' <para>
             ''' Javaw.exe is searched for in the following places:
             ''' </para>
             ''' <list type="number">
             '''     <item>
             '''         <description> in %JAVA_HOME%\bin </description>
             '''     </item>
             '''     <item>
             '''         <description> in %JEDIT_HOME%\jre\bin (Java bundled with jEdit) </description>
             '''     </item>
             '''     <item>
             '''         <description> in HKEY_CLASSES_ROOT\jarfile\shell\open\command\ </description>
             '''     </item>
             '''     <item>
             '''         <description> in %PATH% </description>
             '''     </item>
             ''' </list>
             ''' </remarks>
            Public Shared ReadOnly Property AppPathJava() As String
                Get
                    SyncLock (SyncHandle)
                        If (_AppPathJava Is Nothing) Then
                            GetJavaEnvironment()
                        End If
                        Return _AppPathJava
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns the path of the existing JEdit.jar application file. </summary>
             ''' <remarks>
             ''' <para>
             ''' Returns an empty String if the application file hasn't been found.
             ''' </para>
             ''' <para>
             ''' jEdit.jar is searched for in the following places:
             ''' </para>
             ''' <list type="number">
             '''     <item>
             '''         <description> in %JEDIT_HOME% </description>
             '''     </item>
             '''     <item>
             '''         <description> in "%PROGRAMFILES%\jEdit" </description>
             '''     </item>
             '''     <item>
             '''         <description> in "%PROGRAMFILES(X86)%\jEdit" </description>
             '''     </item>
             '''     <item>
             '''         <description> in "%PROGRAMW6432%\jEdit" </description>
             '''     </item>
             '''     <item>
             '''         <description> in Application Setting "AppUtils_jEdit_FallbackPath" </description>
             '''     </item>
             '''     <item>
             '''         <description> in %PATH% without subdirectories </description>
             '''     </item>
             ''' </list>
             ''' </remarks>
            Public Shared ReadOnly Property AppPathJEdit() As String
                Get
                    SyncLock (SyncHandle)
                        If (_AppPathJEdit Is Nothing) Then
                            GetJEditEnvironment()
                        End If
                        Return _AppPathJEdit
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns a list of all available editors. </summary>
            Public Shared ReadOnly Property AvailableEditors() As Dictionary(Of SupportedEditors, EditorInfo)
                Get
                    SyncLock (SyncHandle)
                        If (_AvailableEditors Is Nothing) Then
                            _AvailableEditors = GetAvailableEditors()
                        End If
                        Return _AvailableEditors
                    End SyncLock
                End Get
            End Property
            
            
            ''' <summary> Returns Java's program directory if <see cref="AppPathJava" /> has been found, otherwise String.Empty. </summary>
            Public Shared ReadOnly Property JAVA_HOME() As String
                Get
                    SyncLock (SyncHandle)
                        If (_JAVA_HOME Is Nothing) Then
                            GetJavaEnvironment()
                        End If
                        Return _JAVA_HOME
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns jEdit's program directory if <see cref="AppPathJEdit" /> has been found, otherwise String.Empty. </summary>
            Public Shared ReadOnly Property JEDIT_HOME() As String
                Get
                    SyncLock (SyncHandle)
                        If (_JEDIT_HOME Is Nothing) Then
                            GetJEditEnvironment()
                        End If
                        Return _JEDIT_HOME
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns expanded environment variable %JEDIT_SETTINGS%, which is the settings directory jEdit should use (maybe String.Empty). </summary>
            Public Shared ReadOnly Property JEDIT_SETTINGS() As String
                Get
                    SyncLock (SyncHandle)
                        If (_JEDIT_SETTINGS Is Nothing) Then
                            GetJEditEnvironment()
                        End If
                        Return _JEDIT_SETTINGS
                    End SyncLock
                End Get
            End Property
            
        #End Region
        
        #Region "Settings"
            
            ''' <summary>  Determines the editor to use for editing tasks when no other editor is stated explicitly. </summary>
             ''' <value>   One of the <see cref="AvailableEditors"/> keys. "SupportedEditors.None" and all other values are ignored. </value>
             ''' <returns> The editor to use as current. If "SupportedEditors.None", then there is no editor available. </returns>
             ''' <remarks> This is an application setting. </remarks>
            Public Shared Property CurrentEditor() As SupportedEditors
                Get
                    SyncLock (SyncHandle)
                        ' _CurrentEditor is initialized by initCurrentEditor().
                        Return _CurrentEditor
                    End SyncLock
                End Get
                Set(value As SupportedEditors)
                    SyncLock (SyncHandle)
                        If ((value <> _CurrentEditor) AndAlso (value <> SupportedEditors.None) AndAlso ([Enum].IsDefined(GetType(SupportedEditors), value))) Then
                            Logger.logDebug(StringUtils.sprintf("CurrentEditor [Set]: Aktueller Editor wird geändert zu '%s'.", value.ToDisplayString()))
                            _CurrentEditor = value
                            My.Settings.AppUtils_CurrentEditor = _CurrentEditor
                        Else
                            Logger.logDebug(StringUtils.sprintf("CurrentEditor [Set]: Änderung des aktuellen Editors zu '%s' abgewiesen oder unnötig.", value.ToDisplayString()))
                        End If
                    End SyncLock
                End Set
            End Property
            
        #End Region
        
        #Region "Public Static Methods"
            
            ''' <summary> The current culture will be patched to use'.' as decimal separator, ' ' as number grouping character and ',' as list separator. </summary>
            Public Shared Sub PatchCurrentCultureSeparators()
                
                Logger.logDebug("PatchCurrentCultureSeparators():  Set '.' as decimal separator, ' ' as number grouping character and ',' as list separator.")
            
                Dim ci As CultureInfo                  = Thread.CurrentThread.CurrentCulture.Clone()
                ci.NumberFormat.NumberDecimalSeparator = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator
                ci.NumberFormat.NumberGroupSeparator   = " "
                ci.TextInfo.ListSeparator              = CultureInfo.InvariantCulture.TextInfo.ListSeparator
                Thread.CurrentThread.CurrentCulture    = ci
            End Sub
            
            ''' <summary> The given editor will be started if possible. </summary>
             ''' <param name="TargetEditor"> Determines one of the supported editors. </param>
             ''' <param name="Arguments">    Command line arguments for the editor. May be <see langword="null"/>. </param>
             ''' <exception cref="System.ArgumentException"> <paramref name="TargetEditor"/> is not supported or <see cref="SupportedEditors"/><c>.None</c> or currently not available. </exception>
             ''' <remarks> 
             ''' <para>
             ''' Notes on jEdit's command line:
             ''' </para>
             ''' <para>
             ''' The Java command line is: "-ms128m -mx1024m -Dawt.useSystemAAFontSettings=on -Dsun.java2d.noddraw=true -jar &lt;path of jedit.jar&gt;.
             ''' </para>
             ''' <para>
             ''' The options "-reuseview -background" are automatically specified.
             ''' </para>
             ''' <para>
             ''' If %JEDIT_SETTINGS% is defined, the argument "-settings=%JEDIT_SETTINGS%" is automatically specified.
             ''' </para>
             ''' </remarks>
            Public Shared Sub StartEditor(ByVal TargetEditor As SupportedEditors, ByVal Arguments As String)
                
                Logger.logDebug(StringUtils.sprintf("startEditor(): Anforderungen: Editor = '%s', Argumente = '%s'.", TargetEditor.ToDisplayString(), Arguments))
                
                ' Validate Arguments.
                Dim ErrMessage  As String = Nothing
                If (TargetEditor = SupportedEditors.None) Then
                    If (AvailableEditors.Count = 0) Then
                        ErrMessage = Rstyx.Utilities.Resources.Messages.AppUtils_NoEditorAvailable
                    Else 
                        ErrMessage = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AppUtils_InvalidEditor, TargetEditor.ToDisplayString())
                    End If
                ElseIf (Not AvailableEditors.ContainsKey(TargetEditor)) Then
                    If ([Enum].IsDefined(GetType(SupportedEditors), TargetEditor)) Then
                        ErrMessage = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AppUtils_EditorNotAvailable, TargetEditor.ToDisplayString())
                    Else 
                        ErrMessage = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AppUtils_NotSupportedEditor, TargetEditor.ToDisplayString())
                    End If
                End If
                If (ErrMessage IsNot Nothing) Then Throw New System.ArgumentException(ErrMessage, "TargetEditor")
                If (Arguments Is Nothing) Then Arguments = String.Empty
                
                ' Start process.
                Dim StartInfo  As New System.Diagnostics.ProcessStartInfo()
                StartInfo.FileName  = AvailableEditors.Item(TargetEditor).AppPath
                StartInfo.Arguments = AvailableEditors.Item(TargetEditor).Arguments & " " & Arguments
                
                Logger.logDebug(StringUtils.sprintf("startEditor(): Auszuführende Datei: '%s'.", StartInfo.FileName))
                Logger.logDebug(StringUtils.sprintf("startEditor(): Argumente: '%s'.", StartInfo.Arguments))
                Logger.logDebug(StringUtils.sprintf("startEditor(): %s wird gestartet.", TargetEditor.ToDisplayString()))
                
                Using EditorProcess As System.Diagnostics.Process = System.Diagnostics.Process.Start(StartInfo)
                End Using
            End Sub
            
            ''' <summary> The given File will be opened in it's associated application if possible. </summary>
             ''' <param name="AbsoluteFilePath"> The absolute path of the file to start. </param>
             ''' <exception cref="System.IO.FileNotFoundException"> <paramref name="AbsoluteFilePath"/> hasn't been found. </exception>
            Public Shared Sub StartFile(ByVal AbsoluteFilePath As String)
                
                Logger.logDebug(StringUtils.sprintf("startFile(): zu startende Datei = '%s'.", AbsoluteFilePath))
                
                Using FileProcess As System.Diagnostics.Process = System.Diagnostics.Process.Start(AbsoluteFilePath)
                End Using
            End Sub
            
            ''' <summary> Activates Excel and invokes the CSV import of GeoTools VBA Add-In. </summary>
             ''' <param name="FilePath"> The csv file to import in Excel. </param>
             ''' <exception cref="System.IO.FileNotFoundException"> <paramref name="FilePath"/> hasn't been found. </exception>
             ''' <exception cref="System.IO.FileNotFoundException"> xlm.vbs hasn't been found after extracting from resources. </exception>
             ''' <exception cref="Rstyx.Utilities.RemarkException"> Wraps any exception occurred while invoking the system command with a clear message. </exception>
             ''' <remarks>
             ''' <para>
             ''' Excel will be started up if no running instance is found.
             ''' </para>
             ''' <para>
             ''' The GeoTools VBA Add-In for Excel has to be installed (www.rstyx.de). 
             ''' </para>
             ''' <para>
             ''' This method extracts a VBscript file from dll resource to a temporary file, wich in turn is invoked via wscript.exe.
             ''' The VBscript does the work.
             ''' </para>
             ''' </remarks>
            Public Shared Sub StartExcelGeoToolsCSVImport(byVal FilePath As String)
                SyncLock (SyncHandle)
                    Logger.logInfo(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AppUtils_InvokeGeoToolsCsvImport, FilePath))
                    If (Not File.Exists(FilePath)) Then Throw New System.IO.FileNotFoundException(Rstyx.Utilities.Resources.Messages.AppUtils_GeoToolsCsvNotFound, FilePath)
                    
                    ' Exctract resource VBscript to temporary file.
                    Dim XlmVbsFile  As String = System.IO.Path.GetTempFileName()
                    Logger.logDebug(StringUtils.sprintf("startExcelGeoToolsCSVImport(): Exctract shell vb script form dll to: '%s'", XlmVbsFile))
                    System.IO.File.WriteAllText(XlmVbsFile, My.Resources.StartExcelAndXLMacro)
                    If (Not File.Exists(XlmVbsFile)) Then Throw New System.IO.FileNotFoundException(Rstyx.Utilities.Resources.Messages.AppUtils_XlmVBScriptNotFound, XlmVbsFile)
                    
                    Try
                        ' Invoke temporary file via wscript.exe.
                        Dim ShellCommand  As String = StringUtils.sprintf("%%comspec%% /c wscript /E:vbscript ""%s"" /M:Import_CSV ""/D:%s"" /silent:true", XlmVbsFile, FilePath)
                        Logger.logDebug(StringUtils.sprintf("startExcelGeoToolsCSVImport(): Invoke shell command = '%s'", ShellCommand))
                        Microsoft.VisualBasic.Interaction.Shell(System.Environment.ExpandEnvironmentVariables(ShellCommand), AppWinStyle.Hide, Wait:=False)
                    Catch ex as System.Exception
                        Throw New RemarkException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AppUtils_ErrorInvokingXlGeoToolsImport, FilePath), ex)
                    End Try
                End SyncLock
            End Sub
            
        #End Region
        
        #Region "Data Structures, Nested Classes"
            
            ''' <summary> Editor info record </summary>
            Public Class EditorInfo
                
                ''' <summary> Creates a new, empty EditorInfo. </summary>
                Public Sub New
                End Sub
                
                ''' <summary> Creates a new EditorInfo and initializes all properties. </summary>
                 ''' <param name="DisplayName"> Editor name for displaying </param>
                 ''' <param name="AppPath">     Full path of the application file </param>
                 ''' <param name="Arguments">   Fixed arguments that will be provided at the command line </param>
                Public Sub New(DisplayName As String, AppPath As String, Arguments As String)
                    Me.DisplayName = DisplayName
                    Me.AppPath     = AppPath
                    Me.Arguments   = Arguments
                End Sub
                
                ''' <summary> Editor name for displaying. </summary>
                Public Property DisplayName  As String  = String.Empty
                
                ''' <summary> Full path of the application file. </summary>
                Public Property AppPath      As String  = String.Empty
                
                ''' <summary> Immutual arguments that will be provided at the command line before other arguments. </summary>
                Public Property Arguments    As String  = String.Empty
            End Class
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Searches for the Crimson Editor application file in some places. </summary>
             ''' <returns> The found application file path or String.Empty. </returns>
            Private Shared  Function GetAppPathCrimsonEditor() As String
                
                Dim CEditExe      As String = String.Empty
                Dim Key_CEditExe  As String
                Dim CEditDir      As String
                Dim fi            As FileInfo
                Dim Success       As Boolean = false
                const AppName     As String = "cedt.exe"
                Logger.logDebug("getAppPathCrimsonEditor(): Programmdatei von Crimson Editor ermitteln.")
                
                ' If it could be uninstalled, maybe it could be found ...
                Key_CEditExe = "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Crimson Editor 3.72\DisplayIcon"
                CEditExe = RegUtils.getApplicationPath(Key_CEditExe)
                if (CEditExe.IsEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getAppPathCrimsonEditor(): Crimson Editor nicht gefunden in Registry => suche nach Portable Version.")
                    Success = False
                end if
                
                ' Search for portable version in 3 places.
                if (not Success) Then
                    Logger.logDebug("getAppPathCrimsonEditor(): Crimson Editor nicht gefunden in Registry => suche nach Portable Version.")
                    CEditDir = "G:\Tools\Crimson Editor"
                    CEditExe  = CEditDir & "\" & AppName
                    if (File.Exists(CEditExe)) Then
                      Success = true
                    else
                      Logger.logDebug("getAppPathCrimsonEditor(): cedt.exe nicht gefunden im (hart kodierten) Verzeichnis '" & CEditDir & "'")
                    end if
                end if
                
                if (not Success) Then
                    CEditDir = Environment.GetEnvironmentVariable("PROGRAMFILES") & "\Crimson Editor"
                    CEditExe  = CEditDir & "\" & AppName
                    if (File.Exists(CEditExe)) Then
                      Success = true
                    else
                      Logger.logDebug("getAppPathCrimsonEditor(): cedt.exe nicht gefunden im (hart kodierten) Verzeichnis '" & CEditDir & "'")
                    end if
                end if
                
                if (not Success) Then
                    CEditDir = Environment.GetEnvironmentVariable("PROGRAMFILES") & "\Tools\Crimson Editor"
                    CEditExe  = CEditDir & "\" & AppName
                    if (File.Exists(CEditExe)) Then
                      Success = true
                    else
                      Logger.logDebug("getAppPathCrimsonEditor(): cedt.exe nicht gefunden im (hart kodierten) Verzeichnis '" & CEditDir & "'")
                    end if
                end if
                
                ' Search in %PATH%
                if (not Success) Then
                    CEditExe = String.Empty
                    Logger.logDebug("getAppPathCrimsonEditor(): Programmdatei von Crimson Editor im Dateisystem suchen im %PATH%.")
                    fi = IO.FileUtils.findFile(AppName, Environment.ExpandEnvironmentVariables("%PATH%"), ";", SearchOption.TopDirectoryOnly)
                    If (fi IsNot Nothing) Then CEditExe = fi.FullName
                end if
                
                ' Result
                if (CEditExe.IsEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getAppPathCrimsonEditor(): Programmdatei von Crimson Editor nicht gefunden.")
                Else 
                    Logger.logDebug(StringUtils.sprintf("getAppPathCrimsonEditor(): Programmdatei von Crimson Editor gefunden: '%s'.", CEditExe))
                end if
                
                Return CEditExe
            End Function
            
            ''' <summary> Searches for the UltraEdit application file in some places. </summary>
             ''' <returns> The found application file path or String.Empty. </returns>
            Private Shared Function GetAppPathUltraEdit() As String
                
                Dim UEditExe      As String = String.Empty
                Dim Key_UEditExe  As String
                Dim fi            As FileInfo
                Const AppNames    As String = "UEdit*.exe;UltaEdit*.exe"
                Logger.logDebug("getAppPathUltraEdit(): Programmdatei von UltraEdit ermitteln.")
    
                ' Check default paths of UltraEdit.
                Dim DefaultFolder     As String = "\IDM Computer Solutions\UltraEdit"
                Dim DefaultFolders(2) As String
                DefaultFolders(0) = "%PROGRAMFILES%" & DefaultFolder
                DefaultFolders(1) = "%PROGRAMFILES(X86)%" & DefaultFolder
                DefaultFolders(2) = "%PROGRAMW6432%" & DefaultFolder
                Logger.logDebug("getAppPathUltraEdit(): Programmdatei von UltraEdit im Dateisystem suchen in Standardpfaden.")
                fi = IO.FileUtils.findFile(AppNames, DefaultFolders.Join(";"c), ";", SearchOption.TopDirectoryOnly)
                If (fi IsNot Nothing) Then UEditExe = fi.FullName
                
                ' Version 11
                If (UEditExe.IsEmptyOrWhiteSpace()) Then
                    ' This only works if UltraEdit has made a file association for ".txt"!!!
                    Key_UEditExe = "HKEY_CLASSES_ROOT\UltraEdit.txt\shell\open\command\"
                    UEditExe = RegUtils.getApplicationPath(Key_UEditExe)
                End if
                
                ' Version 6 ... 10:
                If (UEditExe.IsEmptyOrWhiteSpace()) Then
                    Key_UEditExe = "HKEY_CLASSES_ROOT\Applications\UEdit32.exe\shell\edit\command\"
                    UEditExe = RegUtils.getApplicationPath(Key_UEditExe)
                    If (UEditExe.IsEmptyOrWhiteSpace()) Then
                        Key_UEditExe = "HKEY_CLASSES_ROOT\UltraEdit-32 Document\shell\open\command\"
                        UEditExe = RegUtils.getApplicationPath(Key_UEditExe)
                    End if
                End if
                
                ' Search in %PATH%
                If (UEditExe.IsEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getAppPathUltraEdit(): Programmdatei von UltraEdit im Dateisystem suchen im %PATH%.")
                    fi = IO.FileUtils.findFile(AppNames, Environment.ExpandEnvironmentVariables("%PATH%"), ";", SearchOption.TopDirectoryOnly)
                    If (fi IsNot Nothing) Then UEditExe = fi.FullName
                End if
                
                ' Result
                If (UEditExe.IsEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getAppPathUltraEdit(): Programmdatei von UltraEdit nicht gefunden.")
                Else 
                    Logger.logDebug(StringUtils.sprintf("getAppPathUltraEdit(): Programmdatei von UltraEdit gefunden: '%s'.", UEditExe))
                End if
                
                Return UEditExe
            End Function
            
            ''' <summary> Searches for the Java application file in some places and gets it's environment. </summary>
             ''' <remarks> Determines the Java application file path and %JAVA_HOME%. </remarks>
            Private Shared Sub GetJavaEnvironment()
                
                Dim JavaExe       As String = String.Empty
                Dim fi            As FileInfo
                Const AppNames    As String = "Javaw.exe"
                Const Key_JavaExe As String = "HKEY_CLASSES_ROOT\jarfile\shell\open\command\"
                Logger.logDebug("getJavaEnvironment(): Programmdatei von Java ermitteln.")
                
                ' Search in %JAVA_HOME%
                Logger.logDebug("getJavaEnvironment(): Programmdatei von Java im Dateisystem suchen in %JAVA_HOME%.")
                _JAVA_HOME = Environment.GetEnvironmentVariable("JAVA_HOME")
                If (_JAVA_HOME Is Nothing) Then
                    _JAVA_HOME = String.Empty
                Else
                    fi = IO.FileUtils.findFile(AppNames, _JAVA_HOME & "\bin", ";", SearchOption.TopDirectoryOnly)
                    If (fi IsNot Nothing) Then JavaExe = fi.FullName
                End If
                
                ' Search Java in %JEDIT_HOME%\jre
                If (JavaExe.IsEmptyOrWhiteSpace() AndAlso JEDIT_HOME.IsNotEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getJavaEnvironment(): Programmdatei von Java im Dateisystem suchen unter %JEDIT_HOME%\jre")
                    _JAVA_HOME = JEDIT_HOME & "\jre"
                    fi = IO.FileUtils.findFile(AppNames, _JAVA_HOME & "\bin", ";", SearchOption.TopDirectoryOnly)
                    If (fi IsNot Nothing) Then JavaExe = fi.FullName
                End if
                
                ' Default app for .jar files from registry
                If (JavaExe.IsEmptyOrWhiteSpace()) Then
                    JavaExe = RegUtils.getApplicationPath(Key_JavaExe)
                End if
                
                ' Search dafault Java in %PATH%
                If (JavaExe.IsEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getJavaEnvironment(): Programmdatei der Java-Standardinstallation im Dateisystem suchen im %PATH%.")
                    fi = IO.FileUtils.findFile(AppNames, Environment.GetEnvironmentVariable("PATH"), ";", SearchOption.TopDirectoryOnly)
                    If (fi IsNot Nothing) Then JavaExe = fi.FullName
                End if
                
                '' Search in %PROGRAMFILES%
                'if (JavaExe.IsEmptyOrWhiteSpace()) Then
                '    Logger.logDebug("getJavaEnvironment(): Programmdatei von Java im Dateisystem suchen unter %PROGRAMFILES%.")
                '    fi = IO.FileUtils.findFile(AppNames, Environment.GetEnvironmentVariable("PROGRAMFILES"), ";", SearchOption.AllDirectories)
                '    If (fi IsNot Nothing) Then JavaExe = fi.FullName
                'end if
                
                ' Result
                If (JavaExe.IsEmptyOrWhiteSpace()) Then
                    _JAVA_HOME = String.Empty
                    Logger.logDebug("getJavaEnvironment(): Programmdatei von Java nicht gefunden.")
                Else
                    _JAVA_HOME = IO.FileUtils.getFilePart(JavaExe, IO.FileUtils.FilePart.Dir)
                    If (_JAVA_HOME.EndsWith("\bin", ignoreCase:=True, culture:= CultureInfo.InvariantCulture)) Then
                        _JAVA_HOME = _JAVA_HOME.Left(_JAVA_HOME.Length - 4)
                    ElseIf (_JAVA_HOME.EndsWith("\bin\", ignoreCase:=True, culture:= CultureInfo.InvariantCulture)) Then
                        _JAVA_HOME = _JAVA_HOME.Left(_JAVA_HOME.Length - 5)
                    End If
                    Logger.logDebug(StringUtils.sprintf("getJavaEnvironment(): Programmdatei von Java gefunden: '%s'.", JavaExe))
                    Logger.logDebug(StringUtils.sprintf("getJavaEnvironment(): _JAVA_HOME davon abgeleitet: '%s'.", _JAVA_HOME))
                End if
                
                _AppPathJava = JavaExe
            End Sub
            
            ''' <summary> Searches for the jEdit application file in some places and gets it's environment. </summary>
             ''' <remarks> Determines jedit.jar path, %JEDIT_HOME% and %JEDIT_SETTINGS%. </remarks>
            Private Shared Sub GetJEditEnvironment()
                
                Dim jEditJar      As String = String.Empty
                Dim jEditHome     As String = String.Empty
                Dim fi            As FileInfo
                Const JarName     As String = "jEdit.jar"
                Dim Success       As Boolean = False
                Logger.logDebug("getJEditEnvironment(): Pfad\Dateiname von " & JarName & " ermitteln.")
                
                ' Search in %JEDIT_HOME%
                _JEDIT_HOME = Environment.GetEnvironmentVariable("JEDIT_HOME")
                If (_JEDIT_HOME Is Nothing) Then
                    _JEDIT_HOME = String.Empty
                    Logger.logDebug("getJEditEnvironment(): Umgebungsvariable %JEDIT_HOME% existiert nicht. => Suche weiter in Konfiguration")
                Else
                    _JEDIT_HOME = _JEDIT_HOME.ReplaceWith("\\+$", "")
                    jEditJar = _JEDIT_HOME & "\" & JarName
                    If (File.Exists(jEditJar)) Then
                      Success = True
                      Logger.logDebug("getJEditEnvironment(): jEdit.jar gefunden im Verzeichnis der Umgebungsvariable %JEDIT_HOME%='" & _JEDIT_HOME & "'")
                    Else
                      Logger.logDebug("getJEditEnvironment(): jEdit.jar nicht gefunden im Verzeichnis der Umgebungsvariable %JEDIT_HOME%='" & _JEDIT_HOME & "'")
                    End If
                End if
                
                ' Search in %PROGRAMFILES%
                If (Not Success) Then
                    jEditHome = Environment.GetEnvironmentVariable("PROGRAMFILES") & "\jEdit"
                    jEditJar = jEditHome & "\" & JarName
                    If (File.Exists(jEditJar)) Then
                        Success = True
                    Else
                        Logger.logDebug("getJEditEnvironment(): jEdit.jar nicht gefunden im Verzeichnis der Umgebungsvariable %PROGRAMFILES%='" & jEditHome & "'")
                    End If
                End if
                
                ' Search in %PROGRAMFILES(X86)%
                If (Not Success) Then
                    jEditHome = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") & "\jEdit"
                    jEditJar = jEditHome & "\" & JarName
                    If (File.Exists(jEditJar)) Then
                        Success = True
                    Else
                        Logger.logDebug("getJEditEnvironment(): jEdit.jar nicht gefunden im Verzeichnis der Umgebungsvariable %PROGRAMFILES(X86)%='" & jEditHome & "'")
                    End If
                End if
                
                ' Search in %PROGRAMW6432%
                If (Not Success) Then
                    jEditHome = Environment.GetEnvironmentVariable("PROGRAMW6432") & "\jEdit"
                    jEditJar = jEditHome & "\" & JarName
                    If (File.Exists(jEditJar)) Then
                        Success = True
                    Else
                        Logger.logDebug("getJEditEnvironment(): jEdit.jar nicht gefunden im Verzeichnis der Umgebungsvariable %PROGRAMW6432%='" & jEditHome & "'")
                    End If
                End if
                
                ' Search in Application Setting "AppUtils_jEdit_FallbackPath"
                If (Not Success) Then
                    jEditHome = My.Settings.AppUtils_jEdit_FallbackPath
                    jEditJar = jEditHome & "\" & JarName
                    If (File.Exists(jEditJar)) Then
                        Success = True
                    Else
                        Logger.logDebug("getJEditEnvironment(): jEdit.jar nicht gefunden im (hart kodierten) Verzeichnis '" & jEditHome & "'")
                    End If
                End if
                
                ' Search in %PATH%
                If (Not Success) Then
                    Logger.logDebug("getJEditEnvironment(): jEdit.jar im Dateisystem suchen im %PATH%.")
                    fi = IO.FileUtils.findFile(JarName, Environment.ExpandEnvironmentVariables("%PATH%"), ";", SearchOption.TopDirectoryOnly)
                    If (fi IsNot Nothing) Then jEditJar = fi.FullName
                End if
                
                
                ' jEdit settings directory
                _JEDIT_SETTINGS = Environment.GetEnvironmentVariable("JEDIT_SETTINGS")
                If (_JEDIT_SETTINGS Is Nothing) Then
                    _JEDIT_SETTINGS = String.Empty
                Else
                    _JEDIT_SETTINGS = _JEDIT_SETTINGS.ReplaceWith("\\+$", "")
                End If
                Logger.logDebug("getJEditEnvironment(): Umgebungsvariable %JEDIT_SETTINGS%='" & _JEDIT_SETTINGS & "'")
                
                ' Result
                If (Not Success) Then
                    _AppPathJEdit = String.Empty
                    _JEDIT_HOME   = String.Empty
                    Logger.logDebug("getJEditEnvironment(): Programmdatei von jEdit nicht gefunden.")
                Else
                    _AppPathJEdit = jEditJar
                    _JEDIT_HOME   = IO.FileUtils.getFilePart(jEditJar, IO.FileUtils.FilePart.Dir)
                    Logger.logDebug(StringUtils.sprintf("getJEditEnvironment(): Programmdatei von jEdit gefunden: '%s'.", jEditJar))
                End if
            End Sub
            
            ''' <summary> Looks for availability of supported editors. </summary>
             ''' <returns> A dictionary of all available editors. If no editor has been found, the dictionary has no entry. </returns>
            Private Shared Function GetAvailableEditors() As Dictionary(Of SupportedEditors, EditorInfo)
                Dim Arguments  As String
                Dim Editors    As New Dictionary(Of SupportedEditors, EditorInfo)
                Logger.logDebug("getAvailableEditors(): Suche alle unterstützten Editoren...")
                
                ' UltraEdit.
                If (AppPathUltraEdit.IsNotEmptyOrWhiteSpace()) Then
                    Editors.Add(SupportedEditors.UltraEdit, New EditorInfo(SupportedEditors.UltraEdit.ToDisplayString(), AppPathUltraEdit, String.Empty))
                End If
                
                ' Crimson Editor.
                If (AppPathCrimsonEditor.IsNotEmptyOrWhiteSpace()) Then
                    Editors.Add(SupportedEditors.CrimsonEditor, New EditorInfo(SupportedEditors.CrimsonEditor.ToDisplayString(), AppPathCrimsonEditor, String.Empty))
                End If
                
                ' jEdit.
                If (AppPathJava.IsEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getAvailableEditors(): jEdit ist nicht verfügbar, weil Java nicht verfügbar ist.")
                ElseIf (AppPathJEdit.IsEmptyOrWhiteSpace()) Then
                    Logger.logDebug("getAvailableEditors(): jEdit ist nicht verfügbar, weil jEdit.jar nicht gefunden wurde.")
                Else
                    ' Arguments: all for Java and basic for jEdit.
                    'Arguments = "-ms128m -mx1024m -Dawt.useSystemAAFontSettings=on -Dsun.java2d.noddraw=true -jar """ & AppPathJEdit & """ -reuseview -background "
                    Arguments = "-jar """ & AppPathJEdit & """ -reuseview -background "
                            
                    ' Arguments: jEdit settings directory if needed.
                    if (JEDIT_SETTINGS.IsNotEmptyOrWhiteSpace()) Then
                        Arguments &= """-settings=" & JEDIT_SETTINGS & """ "
                        Logger.logDebug(StringUtils.sprintf("getAvailableEditors(): Als jEdit-Einstellungsverzeichnis wird verwendet: '%s'.", JEDIT_SETTINGS))
                    End If
                    
                    Editors.Add(SupportedEditors.jEdit, New EditorInfo(SupportedEditors.jEdit.ToDisplayString(), AppPathJava, Arguments))
                End If
                
                'Result
                If (Editors.Count = 0) Then
                    Logger.logWarning(Rstyx.Utilities.Resources.Messages.AppUtils_NoEditorAvailable)
                Else
                    Logger.logDebug("getAvailableEditors(): Gefundene Editoren:")
                    For Each Editor As KeyValuePair(Of SupportedEditors, EditorInfo) In Editors
                        Logger.logDebug(StringUtils.sprintf("getAvailableEditors(): - '%s'.", Editor.Value.DisplayName))
                    Next
                End If
                
                Return Editors
            End Function
            
            ''' <summary> Initializes the current editor.. </summary>
             ''' <returns> The initial current editor. May be "SupportedEditors.None". </returns>
             ''' <remarks> 
             ''' <para>
             ''' First, the current editor is restored from application settings "AppUtils_CurrentEditor". 
             ''' </para>
             ''' <para>
             ''' If this restored editor is not available, then the first available is used if there is one, otherwise "SupportedEditors.None",
             ''' and the application setting is updated.
             ''' </para>
             ''' </remarks>
            Private Shared Function InitCurrentEditor() As SupportedEditors
                Logger.logDebug("initCurrentEditor(): Aktuellen Editor ermitteln.")
                
                Dim initialEditor As SupportedEditors = SupportedEditors.None
                If ([Enum].IsDefined(GetType(SupportedEditors), My.Settings.AppUtils_CurrentEditor)) Then
                    initialEditor = CType(My.Settings.AppUtils_CurrentEditor, SupportedEditors)
                End If
                Logger.logDebug(StringUtils.sprintf("initCurrentEditor(): Aktueller Editor war zuletzt '%s'.", initialEditor.ToDisplayString()))
                
                ' If editor restored from settings is not available, then use the first available if there is one.
                If (Not AvailableEditors.ContainsKey(initialEditor)) Then
                    Logger.logDebug(StringUtils.sprintf("initCurrentEditor(): '%s' ist nicht verfügbar => verwende den ersten verfügbaren Editor.", initialEditor.ToDisplayString()))
                    If (AvailableEditors.Count > 0) Then
                        initialEditor = AvailableEditors.Keys.ElementAt(0)
                    Else
                        initialEditor = SupportedEditors.None
                    End If
                End If
                
                ' Store the new current editor in settings.
                If (My.Settings.AppUtils_CurrentEditor <> initialEditor) Then
                    My.Settings.AppUtils_CurrentEditor = initialEditor
                End If
                
                Logger.logDebug(StringUtils.sprintf("initCurrentEditor(): Aktueller Editor ist '%s'.", initialEditor.ToDisplayString()))
                Return initialEditor
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
