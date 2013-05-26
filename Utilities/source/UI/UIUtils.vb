
Namespace UI
    
    ''' <summary> Static utility methods for dealing with the GUI. </summary>
    Public NotInheritable Class UIUtils
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UIUtils")
            
            Private Shared ReadOnly SyncHandle As New Object()
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
	        Shared Sub New()
	        End Sub
            
        #End Region
        
        #Region "Public Static Methods"
            
            ''' <summary> Creates a BitmapImage from a file path </summary>
             ''' <param name="path"> File path, i.e. for a project resource: "/ProjectName;component/Resources/save.png"</param>
             ''' <returns> The BitmapImage generated from the given file. </returns>
            Public Shared Function getImageFromPath(path As String) As System.Windows.Media.Imaging.BitmapImage
                SyncLock (SyncHandle)
                    Dim bi As New System.Windows.Media.Imaging.BitmapImage()
                    bi.BeginInit()
                    bi.UriSource = New System.Uri(path, System.UriKind.RelativeOrAbsolute)
                    bi.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.None
                    bi.EndInit()
                    Return bi
                End SyncLock
            End Function
            
            ''' <summary> Returns the current WFP application's main window. Returns "Nothing", if it's not a standalone WPF application. </summary>
             ''' <returns> The current WFP application's main window, or Null. </returns>
            Public Shared Function getMainWindow() As System.Windows.Window
                SyncLock (SyncHandle)
                    Dim AppMainWindow  As System.Windows.Window = Nothing
                    Dim AppType  As String = String.Empty
                    
                    If (System.Windows.Application.Current isNot Nothing) then
                        ' WPF Application (standalone or XPAB)
                        If (System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName = "PresentationHost.exe") then
                            ' WPF Browser Application
                            AppType = "XPAB"
                        Else
                            ' WPF standalone Application
                            AppType = "WPF"
                            AppMainWindow = System.Windows.Application.Current.MainWindow
                            If (Not AppMainWindow.IsInitialized) then
                                AppMainWindow = Nothing
                            End If
                        End If
                    'ElseIf (System.Windows.Forms.Application.OpenForms.Count > 0) then
                        ''Windows Forms Application" (hopefully since there is at least one Windows Form)
                        'AppType = "WinForm"
                    End If
                    Return AppMainWindow
                End SyncLock
            End Function
            
        #End Region
        
    End Class
    
    #Region "Enums"
        
        ''' <summary> Determines the way a UI control is colored if it should warn the user. </summary>
        Public Enum WarningColorMode As Byte
            
            ''' <summary> No colorizing is done. </summary>
            None        = 0
            
            ''' <summary> Foreground color is changed. </summary>
            Foreground  = 1
            
            ''' <summary> Background color is changed. </summary>
            Background  = 2
        End Enum
        
    #End Region
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
