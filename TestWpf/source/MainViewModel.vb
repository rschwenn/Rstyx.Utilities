
Imports System.IO
Imports System.Threading.Tasks

Imports Rstyx.Utilities.Files
Imports Rstyx.Utilities.UI.ViewModel

Public Class MainViewModel
    Inherits Rstyx.Utilities.UI.ViewModel.ViewModelBase
    
    Private Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.TestWpf.MainViewModel")
    
    
    #Region "Initializing and Finalizing"
        
        ''' <summary> Creates a new instance and sets display names. </summary>
        Public Sub New()
            Logger.logDebug("New(): Init MainViewModel ...")
            
        End Sub
        
        '' <summary> Disposes WpfPanel. </summary>
        'Protected Overrides Sub OnDispose()
        '    If (_SheetDefinitionLevelList IsNot Nothing) Then _SheetDefinitionLevelList.Dispose()
        '    BatchPlotViewPanel = Nothing
        'End Sub
        
    #End Region
    
    #Region "Properties"
        
        Private _TestTaskAsyncCommand   As AsyncDelegateUICommand = Nothing
        
        Public Property FilePath1 As String
        
        ''' <summary> Determines the current ShellCommand for the Test button. </summary>
        Public Property TestTaskAsyncCommand() As AsyncDelegateUICommand
            Get
                Try
                    If (_TestTaskAsyncCommand Is Nothing) Then
                        
                        Dim Decoration As New UICommandDecoration()
                        Decoration.Caption     = "Test" 
                        Decoration.Description = "Start des Testes"
                        Decoration.IconBrush   = UI.Resources.UIResources.IconBrush("Handmade_Start1")
                        
                        Dim CmdInfo As New DelegateUICommandInfo
                        CmdInfo.ExecuteAction       = AddressOf Me.test_1
                        'CmdInfo.CanExecutePredicate = AddressOf Me.CanStartTestTask
                        CmdInfo.Decoration          = Decoration
                        
                        _TestTaskAsyncCommand = New AsyncDelegateUICommand(CmdInfo, False)
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, "TestTaskAsyncCommand[Get]: Fehler beim Erzeugen des Plot-Befehls.")
                End Try
                
                Return _TestTaskAsyncCommand
            End Get
            Set(ByVal value As AsyncDelegateUICommand)
                _TestTaskAsyncCommand = value
                Me.NotifyPropertyChanged("TestTaskAsyncCommand")
            End Set
        End Property
        
    #End Region
    
    
    #Region "Methods"
        
        Public Sub test()
            
            'Dim Task1 As Task = Task.Factory.StartNew(AddressOf test_1)
            
            Dim TestEnum2 As ArrayUtils.SortType = 2 'ArrayUtils.SortType.Numeric
            Logger.logInfo(StringUtils.sprintf("Enum Value=%s:  Display=%s", TestEnum2.ToString(), TestEnum2.ToDisplayString()))
            
        End Sub
        
        Public Sub test_1(CancelToken As System.Threading.CancellationToken)
            
            'Dim TestEnum As Cinch.CustomDialogIcons = Cinch.CustomDialogIcons.Question
            'Logger.logInfo(StringUtils.sprintf("Enum Value=%s:  Display=%s", TestEnum.ToString(), TestEnum.ToDisplayString()))
            'Me.StatusText = TestEnum.ToDisplayString()
            '
            'System.Threading.Thread.Sleep(3000)
            '
            'For i = 1 To 10000
            '    Dim TestEnum2 As ArrayUtils.SortType = ArrayUtils.SortType.Numeric
            '    Logger.logInfo(StringUtils.sprintf("Enum Value=%s:  Display=%s", TestEnum2.ToString(), TestEnum2.ToDisplayString()))
            '    'Me.StatusText = TestEnum2.ToDisplayString()
            '    Cinch.ApplicationHelper.DoEvents()
            '    If (CancelToken.IsCancellationRequested) Then Exit For
            'Next
            'System.Threading.Thread.Sleep(3000)
            
            'Dim success As Boolean = Rstyx.Utilities.Apps.AppUtils.startExcelGeoToolsCSVImport(Me.FilePath1)
            
            UI.ClassEvents.SelectAllOnTextBoxGotFocus = (Not UI.ClassEvents.SelectAllOnTextBoxGotFocus)
            
        End Sub
        
        Public Sub test_0()
            'Dim s As String
            'Dim sa() As String
            'Dim d As Nullable(Of Double)
            ''Dim li  As GeoMath.DBAGLineInfo
            ''Dim bool As Boolean
            '
            Dim TestEnum As Cinch.CustomDialogIcons = Cinch.CustomDialogIcons.Question
            Logger.logInfo(StringUtils.sprintf("Enum Value=%s:  Display=%s", TestEnum.ToString(), TestEnum.ToDisplayString()))
            Me.StatusText = TestEnum.ToDisplayString()
            
            System.Threading.Thread.Sleep(3000)
            
            Dim TestEnum2 As ArrayUtils.SortType = ArrayUtils.SortType.Numeric
            Logger.logInfo(StringUtils.sprintf("Enum Value=%s:  Display=%s", TestEnum2.ToString(), TestEnum2.ToDisplayString()))
            Me.StatusText = TestEnum2.ToDisplayString()
            
            's = "klop-123.4+8y9u=+987#äö"
            'd = 123456789.789
            'sa = s.Split("\s+")
            
            'Logger.logInfo(GeoMath.Hex2Dec("2fde"))
            'd = GeoMath.getKilometer(a)
            
            'GeoMath.getCantFromPointinfo(s, d, False )
            'Dim li = GeoMath.getDBAGLineTitle(6265)
            'Logger.logInfo(li.ShortTitle)
            'Logger.logInfo(Strings.FileSpec2RegExp("X:\Quellen\DotNet\VisualBasic\_Backup\Rstyx.Utilities_2012-??-14.*"))
            
            'Logger.logInfo(s.left("y"c))
            'Logger.logInfo("\sc\rf\\\#\\\".removeTrailingBackslashes())
            'Logger.logInfo("\sc\rf\\\#\\\".TrimEnd("\"))
            'Logger.logInfo(Strings.getFilePart("D:\daten\Test_QP\qp_ueb.dgn", Strings.FilePart.Dir_Proj   ))
            'Logger.logInfo(System.IO.File.Exists("D:\daten\Test_QP\"))
            
            'dim keyName      as string = "HKEY_CURRENT_USER\Software\VB and VBA Program Settings\Microstation\frmBatchPlot\"
            ''dim valueName    as string = "Dialog_Left"
            'dim valueName    as string = "test"
            'dim valueString  as string = "1234"
            'dim valueObj     as Object = "123456"
            'Logger.logInfo(RegUtils.ValueExists(key))
            'Logger.logInfo(RegUtils.getValue(Of String)(keyName & valueName))
            'Logger.logInfo("Pfad = '" & RegUtils.getApplicationPath("HKEY_CLASSES_ROOT\i_M5_file\shell\i_zzHilfe\ShellCommand\") & "'")
            
            
            '
            'Dim Folders  As String = Environment.ExpandEnvironmentVariables("%PATH%")
            'Dim Folders  As String = Environment.GetEnvironmentVariable("PROGRAMFILES")
            'Dim Folders  As String = "T:\"
            'Dim File     As String = " *"
            'Dim File     As String = "cedt.exe"
            'Dim File     As String = "uedit*.exe;ultaedit*.exe"
            ''Logger.logInfo(Folders.IndexOfAny(System.IO.Path.GetInvalidPathChars()))
            ''
            'Dim found  As System.Collections.ObjectModel.Collection(Of System.IO.fileinfo) = Files.FileUtils.findFiles(File, Folders, ";", IO.SearchOption.AllDirectories)
            '
            'If (found.IsNotNull) Then 
            '    For Each fi In found 
            '        Logger.logInfo(fi.FullName)
            '    Next
            '    Logger.logInfo(found.Count)
            'End If
            'Logger.logInfo(StringUtils.sprintf(" %d Dateien gefunden", found.Count))
            
            'Logger.logInfo(Environment.GetEnvironmentVariable("jedit_home"))
            'Logger.logInfo(AppUtils.AppPathJava)
            
            'AppUtils.startEditor(AppUtils.SupportedEditors.jEdit , """T:\_test\12 3 Ö .tx t"" +line:44,11")
            'Logger.logInfo(AppUtils.startFile("T:\_test\12 3 Ö .txt"))
            'AppUtils.startEditor(AppUtils.SupportedEditors.UltraEdit  , """T:\_test\12 3 Ö .txt"" +line:44,11")
            
            'AppUtils.CurrentEditor = AppUtils.SupportedEditors.CrimsonEditor   
            'AppUtils.startEditor(AppUtils.CurrentEditor, """T:\_test\12 3 Ö .txt""")
            
            'Dim IconDictionary  As ResourceDictionary = UI.Resources.UIResources.Icons
            '
            'Logger.logInfo("Microsoft.VisualBasic.FileIO.FileSystem.CurrentDirectory = " & Microsoft.VisualBasic.FileIO.FileSystem.CurrentDirectory)
            'Logger.logInfo("System.Environment.CurrentDirectory = " & System.Environment.CurrentDirectory())
            'Logger.logInfo("System.IO.Directory.GetCurrentDirectory() = " & System.IO.Directory.GetCurrentDirectory())
            '
            'Logger.logInfo("EditorCombo2.ActualHeight = " & EditorCombo2.ActualHeight)
            '
            'FileChoser1.InputFilePath = "rrrrrrrrrr"
            
            'Dim bytes() As Byte = {111, 112, 113, 00, 00, 00}
            
            ' Statt ByteArray2String() - Nullwerte am Ende des Arrays sind egal...
            'Dim text As String  = System.Text.Encoding.Default.GetString(bytes)
            'Dim bytes() As Byte = System.Text.Encoding.Default.GetBytes("str")
            
            'Dim Field = "UserDomain"
            'Dim TableName = "Standorte$"
            'Dim Workbook = "R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xls"
            '
            'Dim XLconn As OleDbConnection = DBUtils.connectToExcelWorkbook("R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xls")
            ''Dim Table As DataTable = DBUtils.getOleDBTable(TableName, XLconn)
            'Dim Table As DataTable = XLconn.getTable(TableName)
            'Logger.logInfo(StringUtils.sprintf("Feld '%s' existiert = %s", Field, XLconn.TableContainsField(TableName, Field)))
            'Logger.logInfo(StringUtils.sprintf("Feld '%s' existiert = %s", Field, Table.containsField(Field)))
            
            'Dim SQL = "SELECT * FROM " & TableName
            'Dim Table As DataTable = DBUtils.queryOLEDB(SQL, XLconn)
            'Dim Query = From site In Table.AsEnumerable() Where site.Field(Of String)("UserDomain") = "dummy"
            
            'Dim Table As DataTable = DBUtils.getExcelSheet(TableName, Workbook)
            'Dim yes = DBExtensions.containsField(Table, Field)
            'Dim yes = Table.containsField(Field)
            
            'For Each row As DataRow In Table.AsEnumerable()
            '    Logger.logInfo(StringUtils.sprintf("Standort '%s' = UserDomain '%s'", row("Standort_ID"), row("UserDomain")))
            'Next 
            
            'TestPanel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity))
            
        End Sub
        
    #End Region

End Class

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
