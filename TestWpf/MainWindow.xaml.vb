
Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Diagnostics
Imports System.Windows
Imports Rstyx.Utilities

Class MainWindow 
    
    Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.TestWpf")

    Private ViewModel       As MainViewModel = Nothing
    
    
    Private Sub MainWindow_Initialized(sender As Object, e As System.EventArgs) Handles Me.Initialized
        '
        ViewModel = New MainViewModel()
        Me.DataContext = ViewModel
        '
        'Logger.logInfo("MainWindow_Activated")
        'FileChoser1.InputFilePath = "debug.lo"
        'FileChoser1.ChangesWorkingDir = True 
        '
        'FileChoser2.FileFilter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
        'FileChoser2.DefaultDirectory = "deh"
        'FileChoser2.InputFilePath = "dd"
    End Sub
    
    Private Sub Button1_Click(sender As System.Object , e As System.Windows.RoutedEventArgs) Handles Button1.Click
        'ViewModel.test()
        
        'TestToggle.IsChecked = (Not TestToggle.IsChecked)
        
        'FileChoser2.InputFilePath = "per code festgelegt"
    End Sub
    
End Class

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
