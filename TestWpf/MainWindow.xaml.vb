﻿
Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Diagnostics
Imports System.Windows
Imports Rstyx.Utilities

Class MainWindow 
    
    Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.TestWpf")

    Private ViewModel       As MainViewModel = Nothing
    
    
    Private Sub MainWindow_Initialized(sender As Object, e As System.EventArgs) Handles Me.Initialized
        Try
            ViewModel = New MainViewModel()
            Me.DataContext = ViewModel
            '
            'Logger.LogInfo("MainWindow_Activated")
            'FileChoser1.InputFilePath = "debug.lo"
            'FileChoser1.ChangesWorkingDir = True 
            '
            'FileChoser2.FileFilter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            'FileChoser2.DefaultDirectory = "deh"
            'FileChoser2.InputFilePath = "dd"
        Catch ex As System.Exception
            System.Diagnostics.Debug.Fail("MainWindow_Initialized() failed!")
        End Try
    End Sub
    
    Private Sub Button1_Click(sender As System.Object , e As System.Windows.RoutedEventArgs) Handles Button1.Click
        Try
            'ViewModel.Test()
            
            'TestToggle.IsChecked = (Not TestToggle.IsChecked)
            
            'FileChoser2.InputFilePath = "per code festgelegt"
        Catch ex As System.Exception
            Logger.LogError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromInsideEventHandler)
        End Try
    End Sub
    
End Class

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
