Class Application

    ' Ereignisse auf Anwendungsebene wie Startup, Exit und DispatcherUnhandledException
    ' können in dieser Datei verarbeitet werden.
    
    ''' <summary>
    ''' Saves the application settings (MainWindow geometry)
    ''' </summary>
    ''' <param name="sender"> ignored </param>
    ''' <param name="e"> ignored </param>
    Private Sub Application_Exit(sender As Object, e As System.Windows.ExitEventArgs) Handles Me.Exit
        MySettings.Default.Save()
    End Sub

End Class
