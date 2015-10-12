Imports System.Windows

Class Application
    
    ' Ereignisse auf Anwendungsebene wie Startup, Exit und DispatcherUnhandledException
    ' können in dieser Datei verarbeitet werden.
    
    Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.TestWpf.Application")
    
    ''' <summary> Saves the application settings (MainWindow geometry) </summary>
    ''' <param name="sender"> ignored </param>
    ''' <param name="e">      ignored </param>
    Private Sub Application_Exit(sender As Object, e As System.Windows.ExitEventArgs) Handles Me.Exit
        Try
            MySettings.Default.Save()
        Catch ex As System.Exception
            System.Diagnostics.Debug.Fail("Application_Exit() failed!")
        End Try
    End Sub
    
    ''' <summary> Process unhandled exception of WPF UI thread (prevent default unhandled exception processing). </summary>
    ''' <param name="sender"> Event source </param>
    ''' <param name="e">      Event arguments </param>
    Private Sub Application_DispatcherUnhandledException(ByVal sender As Object, ByVal e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        Try
            Logger.logError(e.Exception, Rstyx.Utilities.Resources.Messages.Global_DispatcherUnhandledException)
            e.Handled = True
        Catch ex As System.Exception
            System.Diagnostics.Debug.Fail("Application_DispatcherUnhandledException() failed!")
        End Try
    End Sub
    
    ''' <summary> Ensures that default application resources are available. </summary>
    ''' <param name="sender"> Event source </param>
    ''' <param name="e">      Event arguments </param>
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        Rstyx.Utilities.UI.Resources.UIResources.EnsureThemePatchesApplied()
    End Sub
End Class

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
