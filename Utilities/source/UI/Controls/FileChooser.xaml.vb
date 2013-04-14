
Imports System
Imports System.IO
Imports System.Windows

Imports Rstyx.Utilities.IO

Namespace UI.Controls
    
    Public Class FileChooser
        Inherits UserControlBase
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UI.Controls.FileChooser")
            
            ' Default dependency property values.
            Private Shared ChangesWorkingDirDefault     As Boolean = False
            Private Shared DefaultDirectoryDefault      As String  = String.Empty
            Private Shared EditButtonIsEnabledDefault   As Boolean = True
            Private Shared EditButtonVisibilityDefault  As System.Windows.Visibility = System.Windows.Visibility.Visible
            Private Shared FileDialogTitleDefault       As String  = String.Empty    ' Default Title
            Private Shared FileFilterDefault            As String  = "Alle Dateien (*.*)|*.*"
            Private Shared FileFilterIndexDefault       As Integer = 1  ' First filter
            Private Shared FileModeDefault              As System.IO.FileMode = System.IO.FileMode.Open
            Private Shared FilePathDefault              As String  = String.Empty
            Private Shared InputFilePathDefault         As String  = String.Empty
            Private Shared IsExistingFileDefault        As Boolean = False
            Private Shared IsValidFilePathDefault       As Boolean = False
            Private Shared TextBoxToolTipDefault        As String  = String.Empty
            Private Shared WarningColorModeDefault      As Rstyx.Utilities.UI.WarningColorMode = UI.WarningColorMode.Foreground
            Private Shared WatermarkDefault             As Object = Nothing
            
            Private FileWatcher                         As FileSystemWatcher
            
        #End Region
        
        #Region "Constructors and Finalizers"
            
            Public Sub New
                InitializeComponent()
                'SetValue(TextBoxToolTipPropertyKey, getTextBoxToolTip(Me))
            End Sub
            
            ''' <summary> Unregister event handlers. </summary>
            Private Sub UserControlBase_Unloaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Unloaded
                If (FileWatcher IsNot Nothing) Then
                    FileWatcher.EnableRaisingEvents = False
                    RemoveHandler FileWatcher.Created, AddressOf OnFileCreatedOrDeleted
                    RemoveHandler FileWatcher.Deleted, AddressOf OnFileCreatedOrDeleted
                    RemoveHandler FileWatcher.Renamed, AddressOf OnFileRenamed
                    FileWatcher.Dispose()
                    FileWatcher = Nothing
                End If
            End Sub
            
        #End Region
        
        #Region "ReadOnly Dependency Properties"
            
            #Region "FilePath"
                
                Private Shared ReadOnly FilePathPropertyKey As DependencyPropertyKey =
                    DependencyProperty.RegisterReadOnly("FilePath",
                                                        GetType(String),
                                                        GetType(FileChooser),
                                                        New FrameworkPropertyMetadata(FilePathDefault, AddressOf OnFilePathChanged)
                                                        )
                '
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FilePath Dependency Property. </summary>
                Public Shared ReadOnly FilePathProperty As DependencyProperty = FilePathPropertyKey.DependencyProperty
                
                Private Shared Sub OnFilePathChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                    Dim FileChooserInstance As FileChooser = CType(d, FileChooser)
                    FileChooserInstance.RaiseFilePathChanged()
                    'Logger.logInfo(StringUtils.sprintf("OnFilePathChanged(): FilePath changed to: '%s'", e.NewValue))
                End Sub
                
                ''' <summary> Provides the resulting valid file path, otherwise String.Empty. If <see cref="FileMode"/> is <c>Open</c> then the file does exist (ReadOnly). </summary>
                 ''' <remarks> 
                 ''' <para>
                 ''' If this property changes, the <see cref="FilePathChanged"/> Event is raised.
                 ''' </para>
                 ''' <para>
                 ''' If the content of the file path text field is calculated to a valid file path then this is returned, otherwise String.Empty. 
                 ''' </para>
                 ''' </remarks>
                Public ReadOnly Property FilePath() As String
                    Get
                        Return CStr(GetValue(FilePathProperty))
                    End Get
                End Property
                
            #End Region
            
            #Region "IsExistingFile"
                
                Private Shared ReadOnly IsExistingFilePropertyKey As DependencyPropertyKey =
                    DependencyProperty.RegisterReadOnly("IsExistingFile",
                                                        GetType(Boolean),
                                                        GetType(FileChooser),
                                                        New FrameworkPropertyMetadata(IsExistingFileDefault, AddressOf OnIsExistingFileChanged)
                                                        )
                '
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.IsExistingFile Dependency Property. </summary>
                Public Shared ReadOnly IsExistingFileProperty As DependencyProperty = IsExistingFilePropertyKey.DependencyProperty
                
                Private Shared Sub OnIsExistingFileChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                    'Logger.logInfo(BooleanUtils.sprintf("OnIsExistingFileChanged(): IsExistingFile changed to: '%s'", e.NewValue))
                End Sub
                
                ''' <summary> Indicates whether or not the actual file path corresponds to an existing file (ReadOnly). </summary>
                Public ReadOnly Property IsExistingFile() As Boolean
                    Get
                        Return CBool(GetValue(IsExistingFileProperty))
                    End Get
                End Property
                
            #End Region
            
            #Region "IsValidFilePath"
                
                Private Shared ReadOnly IsValidFilePathPropertyKey As DependencyPropertyKey =
                    DependencyProperty.RegisterReadOnly("IsValidFilePath",
                                                        GetType(Boolean),
                                                        GetType(FileChooser),
                                                        New FrameworkPropertyMetadata(IsValidFilePathDefault, AddressOf OnIsValidFilePathChanged)
                                                        )
                '
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.IsValidFilePath Dependency Property. </summary>
                Public Shared ReadOnly IsValidFilePathProperty As DependencyProperty = IsValidFilePathPropertyKey.DependencyProperty
                
                Private Shared Sub OnIsValidFilePathChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                    'Logger.logInfo(BooleanUtils.sprintf("OnIsValidFilePathChanged(): IsValidFilePath changed to: '%s'", e.NewValue))
                End Sub
                
                ''' <summary> Indicates whether or not the actual input file path is valid spelled (ReadOnly). </summary>
                Public ReadOnly Property IsValidFilePath() As Boolean
                    Get
                        Return CBool(GetValue(IsValidFilePathProperty))
                    End Get
                End Property
                
            #End Region
            
            #Region "TextBoxToolTip"
                
                Private Shared ReadOnly TextBoxToolTipPropertyKey As DependencyPropertyKey =
                    DependencyProperty.RegisterReadOnly("TextBoxToolTip",
                                                        GetType(String),
                                                        GetType(FileChooser),
                                                        New FrameworkPropertyMetadata(TextBoxToolTipDefault)
                                                        )
                '
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.TextBoxToolTip Dependency Property. </summary>
                Public Shared ReadOnly TextBoxToolTipProperty As DependencyProperty = TextBoxToolTipPropertyKey.DependencyProperty
                
                ''' <summary> Gets the tool tip for the file path text field (ReadOnly). </summary>
                Public ReadOnly Property TextBoxToolTip() As String
                    Get
                        Return CStr(GetValue(TextBoxToolTipProperty))
                    End Get
                End Property
                
            #End Region
            
        #End Region
        
        #Region "Dependency Properties"
            
            #Region "ChangesWorkingDir"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.ChangesWorkingDir Dependency Property. </summary>
                Public Shared ReadOnly ChangesWorkingDirProperty As DependencyProperty =
                    DependencyProperty.Register("ChangesWorkingDir",
                                                GetType(Boolean),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(ChangesWorkingDirDefault)
                                                )
                '
                ''' <summary> Determines if the working directory should be changed to the parent directory of the actual resulting file path. Defaults to <c>False</c>. </summary>
                Public Property ChangesWorkingDir() As Boolean
                    Get
                        Return CBool(GetValue(ChangesWorkingDirProperty))
                    End Get
                    Set(ByVal value As Boolean)
                        SetValue(ChangesWorkingDirProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "DefaultDirectory"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.DefaultDirectory Dependency Property. </summary>
                Public Shared ReadOnly DefaultDirectoryProperty As DependencyProperty =
                    DependencyProperty.Register("DefaultDirectory",
                                                GetType(String),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(DefaultDirectoryDefault)
                                                )
                                                'New FrameworkPropertyMetadata(DefaultDirectoryDefault, AddressOf OnDefaultDirectoryChanged)
                '
                'Private Shared Sub OnDefaultDirectoryChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                '    'OnClrPropertyChanged("DefaultDirectory")
                'End Sub
                
                ''' <summary> If not String.Empty, it's the directory which is used instead of the current directory if the file text box is empty. </summary>
                 ''' <remarks> Used as the file dialog's initial directory and to "root" a relative file name. </remarks>
                Public Property DefaultDirectory() As String
                    Get
                        Return CStr(GetValue(DefaultDirectoryProperty))
                    End Get
                    Set(ByVal value As String)
                        SetValue(DefaultDirectoryProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "EditButtonIsEnabled"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.EditButtonIsEnabled Dependency Property. </summary>
                Public Shared ReadOnly EditButtonIsEnabledProperty As DependencyProperty =
                    DependencyProperty.Register("EditButtonIsEnabled",
                                                GetType(Boolean),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(EditButtonIsEnabledDefault)
                                                )
                '
                ''' <summary> Determines the <c>IsEnabled</c> property of the edit button. Defaults to <c>True</c>. </summary>
                Public Property EditButtonIsEnabled() As Boolean
                    Get
                        Return CType(GetValue(EditButtonIsEnabledProperty), Boolean)
                    End Get
                    Set(ByVal value As Boolean)
                        SetValue(EditButtonIsEnabledProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "EditButtonVisibility"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.EditButtonVisibility Dependency Property. </summary>
                Public Shared ReadOnly EditButtonVisibilityProperty As DependencyProperty =
                    DependencyProperty.Register("EditButtonVisibility",
                                                GetType(System.Windows.Visibility),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(EditButtonVisibilityDefault)
                                                )
                '
                ''' <summary> Determines the visibility (Visible, Hidden, Collapsed) of the edit buton. Defaults to <c>Visible</c>. </summary>
                Public Property EditButtonVisibility() As System.Windows.Visibility
                    Get
                        Return CType(GetValue(EditButtonVisibilityProperty), System.Windows.Visibility)
                    End Get
                    Set(ByVal value As System.Windows.Visibility)
                        SetValue(EditButtonVisibilityProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "FileDialogTitle"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.FileDialogTitle Dependency Property. </summary>
                Public Shared ReadOnly FileDialogTitleProperty As DependencyProperty =
                    DependencyProperty.Register("FileDialogTitle",
                                                GetType(String),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(FileDialogTitleDefault)
                                                )
                '
                ''' <summary> Sets or gets the title for the file dialog. </summary>
                Public Property FileDialogTitle() As String
                    Get
                        Return CStr(GetValue(FileDialogTitleProperty))
                    End Get
                    Set(ByVal value As String)
                        SetValue(FileDialogTitleProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "FileFilter"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.FileFilter Dependency Property. </summary>
                Public Shared ReadOnly FileFilterProperty As DependencyProperty =
                    DependencyProperty.Register("FileFilter",
                                                GetType(String),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(FileFilterDefault)
                                                )
                                                'New FrameworkPropertyMetadata(FileFilterDefault, AddressOf OnFileFilterChanged)
                '
                'Private Shared Sub OnFileFilterChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                '    'OnClrPropertyChanged("FileFilter")
                'End Sub
                
                ''' <summary> Sets or gets the file filter for the file dialog. </summary>
                 ''' <remarks> Example: "Text files (*.txt)|*.txt|Comma separated (*.csv)|*.csv|Office files (*.doc, *.xls, *.ppt)|(*.doc;*.xls;*.ppt)" </remarks>
                Public Property FileFilter() As String
                    Get
                        Return CStr(GetValue(FileFilterProperty))
                    End Get
                    Set(ByVal value As String)
                        SetValue(FileFilterProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "FileFilterIndex"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.FileFilterIndex Dependency Property. </summary>
                Public Shared ReadOnly FileFilterIndexProperty As DependencyProperty =
                    DependencyProperty.Register("FileFilterIndex",
                                                GetType(Integer),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(FileFilterIndexDefault, AddressOf OnFileFilterIndexChanged)
                                                )
                '
                Private Shared Sub OnFileFilterIndexChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                    'OnClrPropertyChanged("FileFilterIndex")
                End Sub
                
                ''' <summary> Sets or gets the current <see cref="FileFilter"/> index for the file dialog. </summary>
                Public Property FileFilterIndex() As Integer
                    Get
                        Return Cint(GetValue(FileFilterIndexProperty))
                    End Get
                    Set(ByVal value As Integer)
                        SetValue(FileFilterIndexProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "FileMode"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.FileMode Dependency Property. </summary>
                Public Shared ReadOnly FileModeProperty As DependencyProperty =
                    DependencyProperty.Register("FileMode",
                                                GetType(System.IO.FileMode),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(FileModeDefault, AddressOf OnFileModeChanged)
                                                )
                '
                Private Shared Sub OnFileModeChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                    Dim FileChooserInstance As FileChooser = CType(d, FileChooser)
                    FileChooserInstance.SetValue(TextBoxToolTipPropertyKey, getTextBoxToolTip(FileChooserInstance))
                End Sub
                
                ''' <summary> Determines if the chosen file is intended to be opened or saved. Defaults to <c>Open</c>. </summary>
                 ''' <remarks> All values but <c>Open</c> are recognized as "Save". </remarks>
                Public Property FileMode() As System.IO.FileMode
                    Get
                        Return CType(GetValue(FileModeProperty), System.IO.FileMode)
                    End Get
                    Set(ByVal value As System.IO.FileMode)
                        SetValue(FileModeProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "InputFilePath"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.InputFilePath Dependency Property. </summary>
                Public Shared ReadOnly InputFilePathProperty As DependencyProperty =
                    DependencyProperty.Register("InputFilePath",
                                                GetType(String),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(InputFilePathDefault, AddressOf OnInputFilePathPropertyChanged)
                                                )
                '
                ''' <summary> Forwardes InputFilePath to the FilePathTextBox. </summary>
                Private Shared Sub OnInputFilePathPropertyChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                    Dim FileChooserInstance As FileChooser = CType(d, FileChooser)
                    FileChooserInstance.FilePathTextBox.Text = CType(e.NewValue, String)
                End Sub
                ''' <summary> Sets or gets the content of the file path text field. </summary>
                 ''' <remarks> This can be used to require a "desired" file path, wich is validated by the FileChooser like interactive input. </remarks>
                Public Property InputFilePath() As String
                    Get
                        Return CStr(GetValue(InputFilePathProperty))
                    End Get
                    Set(ByVal value As String)
                        SetValue(InputFilePathProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "WarningColorMode"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.WarningColorMode Dependency Property. </summary>
                Public Shared ReadOnly WarningColorModeProperty As DependencyProperty =
                    DependencyProperty.Register("WarningColorMode",
                                                GetType(Rstyx.Utilities.UI.WarningColorMode),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(WarningColorModeDefault)
                                                )
                '
                ''' <summary> Determines the way the file text box is colored if the FileChooser doesn't actually has a resulting file path. Defaults to <c>Background</c>. </summary>
                Public Property WarningColorMode() As Rstyx.Utilities.UI.WarningColorMode
                    Get
                        Return CType(GetValue(WarningColorModeProperty), Rstyx.Utilities.UI.WarningColorMode)
                    End Get
                    Set(ByVal value As Rstyx.Utilities.UI.WarningColorMode)
                        SetValue(WarningColorModeProperty, value)
                    End Set
                End Property
                
            #End Region
            
            #Region "Watermark"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.FileChooser.Watermark Dependency Property. </summary>
                Public Shared ReadOnly WatermarkProperty As DependencyProperty =
                    DependencyProperty.Register("Watermark",
                                                GetType(Object),
                                                GetType(FileChooser),
                                                New FrameworkPropertyMetadata(WatermarkDefault)
                                                )
                '
                ''' <summary> Determines the watermark for the file text box, which is an object (usually a String) to represent Null or missing text. Defaults to <c>Null</c>. </summary>
                Public Property Watermark() As Object
                    Get
                        Return CType(GetValue(WatermarkProperty), Object)
                    End Get
                    Set(ByVal value As Object)
                        SetValue(WatermarkProperty, value)
                    End Set
                End Property
                
            #End Region
            
        #End Region
        
        #Region "Events"
            
            Private ReadOnly FilePathChangedWeakEvent As New Cinch.WeakEvent(Of EventHandler(Of EventArgs))
            
            ''' <summary> Raises when the resulting FilePath has changed. (Internaly managed in a weakly way). </summary>
            Public Custom Event FilePathChanged As EventHandler(Of EventArgs)
                
                AddHandler(ByVal value As EventHandler(Of EventArgs))
                    FilePathChangedWeakEvent.Add(value)
                End AddHandler
                
                RemoveHandler(ByVal value As EventHandler(Of EventArgs))
                    FilePathChangedWeakEvent.Remove(value)
                End RemoveHandler
                
                RaiseEvent(ByVal sender As Object, ByVal e As EventArgs)
                    FilePathChangedWeakEvent.Raise(sender, e)
                End RaiseEvent
                
            End Event
            
            ''' <summary> Raises the FilePathChanged event. </summary>
            Private Sub RaiseFilePathChanged()
                RaiseEvent FilePathChanged(Me, EventArgs.Empty)
            End Sub
            
        #End Region
        
        #Region "Event Handlers"
            
            ''' <summary> Evaluates the file path text field and triggers all dependencies. It's the FileChooser's main logic. </summary>
            Private Sub OnFilePathTextBoxChanged(sender As System.Object , e As System.Windows.Controls.TextChangedEventArgs) Handles FilePathTextBox.TextChanged
                OnInputFilePathChanged()
            End Sub
            
            ''' <summary> Shows a file dialog and writes the choosen file into the file path text box. Current directory won't be changed. </summary>
            Private Sub getFilePathFromDialog() Handles FileDialogButton.Click
                
                Dim Aborted            As Boolean = True
                Dim ChoosenFile        As String  = String.Empty
                Dim InitialFileName    As String  = String.Empty
                Dim OpenFile           As Boolean = (Me.FileMode = System.IO.FileMode.Open)
                
                ' Initial file name (must exist)
                If (File.Exists(Me.FilePath)) Then
                    InitialFileName = Me.FilePath
                End If
                
                ' Init and show dialog.
                If (OpenFile) Then
                    Logger.logDebug("getFilePathFromDialog(): Initialize OpenFileDialog.")
                    Dim Dialog As Microsoft.Win32.OpenFileDialog = New Microsoft.Win32.OpenFileDialog()
                    
                    Dialog.Title            = Me.FileDialogTitle
                    Dialog.FileName         = InitialFileName
                    Dialog.InitialDirectory = getInitialDirectory(Me, ForDialog:=True)
                    Dialog.Filter           = Me.FileFilter
                    Dialog.FilterIndex      = Me.FileFilterIndex
                    Dialog.RestoreDirectory = True
                    Dialog.ValidateNames    = True
                    
                    If (Dialog.ShowDialog(UIUtils.getMainWindow())) Then
                        Aborted = False
                        ChoosenFile = Dialog.FileName
                        SetCurrentValue(InputFilePathProperty, Dialog.FileName)
                        SetCurrentValue(FileFilterIndexProperty, Dialog.FilterIndex)
                    End if
                Else
                    Logger.logDebug("getFilePathFromDialog(): Initialize SaveFileDialog.")
                    Dim Dialog As Microsoft.Win32.SaveFileDialog = New Microsoft.Win32.SaveFileDialog()
                    
                    Dialog.Title            = Me.FileDialogTitle
                    Dialog.FileName         = InitialFileName
                    Dialog.InitialDirectory = getInitialDirectory(Me, ForDialog:=True)
                    Dialog.Filter           = Me.FileFilter
                    Dialog.FilterIndex      = Me.FileFilterIndex
                    Dialog.RestoreDirectory = True
                    Dialog.ValidateNames    = True
                    Dialog.OverwritePrompt  = True
                    
                    If (Dialog.ShowDialog(UIUtils.getMainWindow())) Then
                        Aborted = False
                        ChoosenFile = Dialog.FileName
                        SetCurrentValue(InputFilePathProperty, Dialog.FileName)
                        SetCurrentValue(FileFilterIndexProperty, Dialog.FilterIndex)
                    End if
                End If
                
                ' Log result.
                If (Aborted) Then
                    Logger.logDebug("getFilePathFromDialog(): FileDialog aborted.")
                Else 
                    Logger.logDebug(StringUtils.sprintf("getFilePathFromDialog(): Choosen file: '%s'", ChoosenFile))
                End If
            End Sub
            
            ''' <summary> Loads Me.FilePath into the current editor. </summary>
            Private Sub EditFile(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles EditFileButton.Click
                Apps.AppUtils.startEditor(Apps.AppUtils.CurrentEditor, """" & Me.FilePath & """")
            End Sub
            
            ''' <summary> Forward the focus from the user control into the text field. </summary>
            Private Sub OnUcGotFocus(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles Me.GotFocus
                If (Not (FilePathTextBox.IsFocused Or FileDialogButton.IsFocused Or EditFileButton.IsFocused)) Then
                    FilePathTextBox.Focus()
                End If
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Gets the actual initial directory for this FileChooser depending on current status. </summary>
            Private Shared Function getInitialDirectory(FileChooserInstance As FileChooser, ForDialog As Boolean) As String
                
                Dim isRelativeDefault   As Boolean = False
                Dim InitialDirectory    As String  = String.Empty
                Dim OpenFile            As Boolean = (FileChooserInstance.FileMode = System.IO.FileMode.Open)
                Dim DirectoryMustExist  As Boolean = ForDialog Or OpenFile
                
                ' If text box isn't empty, try to get a directory.
                If (FileChooserInstance.InputFilePath.IsNotEmptyOrWhiteSpace()) Then
                    InitialDirectory = FileUtils.getFilePart(FileChooserInstance.InputFilePath, FileUtils.FilePart.Dir, False)
                    
                    If (InitialDirectory.IsNotEmptyOrWhiteSpace() AndAlso DirectoryMustExist) Then
                        If (Not Directory.Exists(InitialDirectory)) Then
                            InitialDirectory = String.Empty
                        End If
                    End If
                End If
                
                ' If no success and FileChooserInstance.DefaultDirectory is set, try this (if relative, then relative to current directory!).
                If (InitialDirectory.IsEmpty()) Then
                    If (FileChooserInstance.DefaultDirectory.IsNotEmptyOrWhiteSpace()) Then
                        If (FileUtils.IsValidFilePath(FileChooserInstance.DefaultDirectory)) Then
                            If (Path.IsPathRooted(FileChooserInstance.DefaultDirectory)) Then
                                Try 
                                    InitialDirectory = Path.GetFullPath(FileChooserInstance.DefaultDirectory)
                                    
                                    If (InitialDirectory.IsNotEmptyOrWhiteSpace() AndAlso DirectoryMustExist) Then
                                        If (Not Directory.Exists(InitialDirectory)) Then
                                            InitialDirectory = String.Empty
                                        End If
                                    End If
                                Catch ex As System.Exception 
                                    ' Silently catch wrong path syntax and whatever else...
                                End Try 
                            Else
                                isRelativeDefault = True
                            End If
                        End If
                    End If
                End If
                
                ' If no success take current working directory.
                If (InitialDirectory.IsEmpty()) Then 
                    If (isRelativeDefault) Then
                        InitialDirectory = System.Environment.CurrentDirectory & Path.DirectorySeparatorChar & FileChooserInstance.DefaultDirectory
                        
                        If (DirectoryMustExist AndAlso (Not Directory.Exists(InitialDirectory))) Then
                            InitialDirectory = System.Environment.CurrentDirectory
                        End If
                    Else 
                        InitialDirectory = System.Environment.CurrentDirectory
                    End If
                End If
                
                Return InitialDirectory
            End Function
            
            ''' <summary> Returns the ToolTip for the text box depending on current status. </summary>
            Private Shared Function getTextBoxToolTip(FileChooserInstance As FileChooser) As String
                
                Dim ToolTip             As String  = String.Empty
                Dim InputFileAbsolute   As String  = String.Empty
                Dim OpenFile            As Boolean = (FileChooserInstance.FileMode = System.IO.FileMode.Open)
                
                If (OpenFile) Then
                    ' If file name isn't rooted yet, do it now.
                    If (FileUtils.IsValidFilePath(FileChooserInstance.InputFilePath)) Then
                        If (Path.IsPathRooted(FileChooserInstance.InputFilePath)) Then
                            InputFileAbsolute = FileChooserInstance.InputFilePath
                        Else
                            InputFileAbsolute = Path.GetFullPath(Path.Combine(getInitialDirectory(FileChooserInstance, ForDialog:=False), FileChooserInstance.InputFilePath))
                        End If
                    End If
                    
                    ToolTip = "Zu lesende Datei"
                    If (FileChooserInstance.FilePath.IsNotEmpty()) Then
                        ToolTip = ToolTip & ": " & FileChooserInstance.FilePath
                    ElseIf (FileChooserInstance.InputFilePath.IsNotEmpty())
                        If (Not FileChooserInstance.IsValidFilePath) Then
                            ToolTip = ToolTip & " ist kein gültiger Dateiname"
                        Else
                            ToolTip = ToolTip & " existiert nicht: " & InputFileAbsolute
                        End If
                    End If
                Else
                    ToolTip = "Zu schreibende Datei"
                    If (FileChooserInstance.FilePath.IsNotEmpty()) Then
                        If (Not FileChooserInstance.IsExistingFile) Then
                            ToolTip = ToolTip & " existiert (noch) nicht"
                        End If
                        ToolTip = ToolTip & ": " & FileChooserInstance.FilePath
                    ElseIf (Not FileChooserInstance.IsValidFilePath) Then
                        If (FileChooserInstance.InputFilePath.IsNotEmpty()) Then
                            ToolTip = ToolTip & " ist kein gültiger Dateiname"
                        End If
                    End If
                End If
                
                Return ToolTip.Trim()
            End Function
            
            
            ''' <summary> Evaluates the file path text field and triggers all dependencies. It's the FileChooser's main logic. </summary>
            Private Sub OnInputFilePathChanged()
                
                Dim InputFilePathTrimmed    As String  = FilePathTextBox.Text.Trim() 'Me.InputFilePath.Trim()
                Dim FileName                As String  = String.Empty
                Dim InputFileAbsolute       As String  = String.Empty
                Dim isValidFileName         As Boolean = False
                Dim FileExists              As Boolean = False
                Dim OpenFile                As Boolean = (Me.FileMode = System.IO.FileMode.Open)
                
                ' Evaluate file path.
                If (InputFilePathTrimmed.IsEmpty()) Then
                    SetCurrentValue(InputFilePathProperty, String.Empty)
                Else
                    ' If file name isn't rooted yet, do it now.
                    If (FileUtils.IsValidFilePath(InputFilePathTrimmed)) Then
                        If (Path.IsPathRooted(InputFilePathTrimmed)) Then
                            InputFileAbsolute = InputFilePathTrimmed
                        Else
                            InputFileAbsolute = Path.GetFullPath(Path.Combine(getInitialDirectory(Me, ForDialog:=False), InputFilePathTrimmed))
                        End If
                    End If
                    
                    FileExists = System.IO.File.Exists(InputFileAbsolute)
                    
                    If (OpenFile) then
                        ' File must exist to be valid => no spelling check and so on.
                        If (InputFileAbsolute.IsEmpty()) Then
                            isValidFileName = False
                        Else
                            isValidFileName = True
                            
                            ' Write back absolute path to text field.
                            SetCurrentValue(InputFilePathProperty, InputFileAbsolute)
                            
                            ' A file to open is only valid, if it already exists.
                            If (FileExists) Then
                                FileName = InputFileAbsolute
                            End If
                        End If
                    Else
                        ' File doesn't have to exist, that's why it has to be a valid path...
                        If (InputFileAbsolute.IsEmpty()) Then
                            ' File is somehow invalid: Remove invalid characters.
                            InputFileAbsolute = FileUtils.getFilePart(FileUtils.validateFilePathSpelling(InputFilePathTrimmed, String.Empty), FileUtils.FilePart.Dir_Base_Ext)
                        End If
                        If (InputFileAbsolute.IsEmpty()) Then
                            ' Still invalid (i.e. a ":" at the wrong position.
                            isValidFileName = False
                        Else
                            isValidFileName = True
                            
                            ' Write back absolute path to text field
                            SetCurrentValue(InputFilePathProperty, InputFileAbsolute)
                            
                            FileName = InputFileAbsolute
                        End if
                    End If
                End If
                
                ' Update resulting (readonly) properties.
                SetValue(IsValidFilePathPropertyKey, isValidFileName)
                SetValue(IsExistingFilePropertyKey, FileExists)
                SetValue(FilePathPropertyKey, FileName)
                SetValue(TextBoxToolTipPropertyKey, getTextBoxToolTip(Me))
                
                ' Update the file system watcher.
                updateFileWatcher()
                
                ' Current directory
                If (Me.ChangesWorkingDir AndAlso isValidFileName) Then
                    Dim NewCurrentDirectory As String = System.IO.Path.GetDirectoryName(InputFileAbsolute)
                    If (Directory.Exists(NewCurrentDirectory)) Then
                        If (Not (System.Environment.CurrentDirectory = NewCurrentDirectory)) Then 
                            System.Environment.CurrentDirectory = NewCurrentDirectory
                            Logger.logDebug(StringUtils.sprintf("OnInputFilePathChanged(): CurrentDirectory changed to: '%s'", System.Environment.CurrentDirectory))
                        End If
                    End If
                End If
                
            End Sub
            
            ''' <summary> Initializes and/or updates the file watcher to watch the actual file. </summary>
            Private Sub updateFileWatcher()
                Try
                    ' Create the watcher.
                    If (FileWatcher Is Nothing) Then
                        
                        FileWatcher = New FileSystemWatcher()
                        FileWatcher.NotifyFilter = NotifyFilters.FileName
                        
                        AddHandler FileWatcher.Created, AddressOf OnFileCreatedOrDeleted
                        AddHandler FileWatcher.Deleted, AddressOf OnFileCreatedOrDeleted
                        AddHandler FileWatcher.Renamed, AddressOf OnFileRenamed
                    End If
                    
                    ' Update the watcher.
                    FileWatcher.EnableRaisingEvents = False
                    
                    If (Me.IsValidFilePath) Then
                        Try
                            Dim FileToWatch As String = Me.InputFilePath
                            FileWatcher.Path = System.IO.Path.GetDirectoryName(FileToWatch)
                            FileWatcher.Filter = System.IO.Path.GetFileName(FileToWatch)
                            FileWatcher.EnableRaisingEvents = True
                        Catch ex As System.Exception
                            ' Silently catch wrong path names.
                            'Logger.logError(ex, "updateFileWatcher(): unerwarteter Fehler")
                        End Try
                    End If
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "updateFileWatcher(): unerwarteter Fehler")
                End Try
            End Sub
            
            ''' <summary> Response to creating or deleting the actual file in the file system.. </summary>
            Private Sub OnFileCreatedOrDeleted(source As Object, e As FileSystemEventArgs)
                ' Call back to the UI thread (because this method is called by the FileSystemWatcher's thread!)
                Me.Dispatcher.BeginInvoke(New System.Action(AddressOf OnFileExistsChanged))
            End Sub
            
            ''' <summary> Response to renaming the actual file in the file system.. </summary>
            Private Sub OnFileRenamed(source As Object, e As RenamedEventArgs)
                ' Call back to the UI thread (because this method is called by the FileSystemWatcher's thread!)
                Me.Dispatcher.BeginInvoke(New System.Action(AddressOf OnFileExistsChanged))
            End Sub
            
            ''' <summary> Response to changing the existence of the actual file in the file system (deleted, renamed, created). </summary>
            Private Sub OnFileExistsChanged()
                Try
                    'SetValue(IsExistingFilePropertyKey, System.IO.File.Exists(Me.FilePath))
                    OnInputFilePathChanged()
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "onFileExistenceChanged(): unerwarteter Fehler")
                End Try
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
