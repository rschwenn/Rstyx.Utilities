
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.DataTableExtensions
'Imports System.Linq
Imports System.IO
Imports System.Threading.Tasks

Imports Rstyx.Utilities
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.UI.ViewModel

Public Class MainViewModel
    Inherits Rstyx.Utilities.UI.ViewModel.ViewModelBase
    
    Private Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.TestWpf.MainViewModel")
    
    
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
        Public Property Textbox   As String
        
        Private _TextField  As String
        Public ReadOnly Property TextField  As String
            Get
                Return _TextField
            End Get
        End Property
        
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
                        CmdInfo.CanExecutePredicate = AddressOf Me.CanStartTestTask
                        CmdInfo.Decoration          = Decoration
                        
                        _TestTaskAsyncCommand = New AsyncDelegateUICommand(CmdInfo, CancelCallback:=Nothing, SupportsCancellation:=False, runAsync:=True)
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_ErrorCreatingCommandIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
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
            
            'Dim TcReader As New TcFileReader()
            
            Try
                Dim Km As Kilometer = New Kilometer(Me.Textbox)
                Logger.logInfo(StringUtils.sprintf("Km = %8.3f  (Status=%s)  =>  %s", Km.Value, Km.Status.ToDisplayString(), Km.ToKilometerNotation(3)))
                
                'Logger.logInfo(StringUtils.sprintf("gültig     = %s", Rstyx.Utilities.IO.FileUtils.isValidFilePath(Me.Textbox)))
                'Logger.logInfo(StringUtils.sprintf("korrigiert = %s", Rstyx.Utilities.IO.FileUtils.validateFilePathSpelling(Me.Textbox)))
                'Logger.logInfo(StringUtils.sprintf("gültig     = %s", Rstyx.Utilities.IO.FileUtils.isValidFileName(Me.Textbox)))
                'Logger.logInfo(StringUtils.sprintf("korrigiert = %s", Rstyx.Utilities.IO.FileUtils.validateFileNameSpelling(Me.Textbox)))
                
                'Dim Path As String = "T:\Debug.log"
                'Dim fdk As New IO.DataTextFileReader(LineStartCommentToken:="*", LineEndCommentToken:="|", SeparateHeader:=True)
                'fdk.Load(Me.FilePath1)
                'Logger.logInfo(StringUtils.sprintf("Zeilen gelesen = %d", fdk.TotalLinesCount))
                
                'TcReader = New TcFileReader()
                'TcReader.Load(Me.FilePath1)
                'Logger.logInfo(TcReader.ToReport(OnlySummary:=True))
                'Logger.logInfo(TcReader.ToString())
                
                'Dim Info As String = Me.Textbox
                'Dim Cant As Double = GeoMath.parseCant(Info, strict:=False, absolute:=False, editPointInfo:=True)
                'Logger.logInfo(StringUtils.sprintf("Überhöhung = %.0f  (Info = '%s')", Cant, Info))
                 
                'Dim li  As GeoMath.DBAGTrackInfo
                'li = GeoMath.getDBAGTrackTitle(6265)
                'Logger.logInfo(li.ShortTitle)
                'Exit Sub
                
                'Dim Zahl        As DataField(Of Double) = Nothing
                'Dim Text        As DataField(Of String) = Nothing
                'Dim PError      As ParseError = Nothing
                'Dim Errors      As New ParseErrorCollection()
                'Dim SplitLine   As New PreSplittedTextLine(Me.Textbox, Nothing, Nothing)
                '
                'Dim FieldDefS As New DataFieldDefinition(Of String)("texxxt",
                '                                                    DataFieldPositionType.WordNumber,
                '                                                    2, -1,
                '                                                    DataFieldOptions.NotRequired
                '                                                   )
                'Dim FieldDefD As New DataFieldDefinition(Of Double)("Zahl",
                '                                                    DataFieldPositionType.ColumnAndLength,
                '                                                    4, 15,
                '                                                    DataFieldOptions.AllowKilometerNotation Or DataFieldOptions.NotRequired 
                '                                                   )
                ''
                'If (SplitLine.HasData) Then
                '    If (SplitLine.TryParseField(FieldDefD, Zahl)) Then
                '        Logger.logInfo(StringUtils.sprintf("Feld '%s' = %.3f", Zahl.Definition.Caption, Zahl.Value))
                '    Else
                '        Errors.Add(Zahl.ParseError)
                '    End If
                '    
                '    If (SplitLine.TryParseField(FieldDefS, Text)) Then
                '        Logger.logInfo(StringUtils.sprintf("Feld '%s' = %s", Text.Definition.Caption, Text.Value))
                '    Else
                '        Errors.Add(Text.ParseError)
                '    End If
                '    
                '    Errors.ToLoggingConsole()
                'End If
                
                'Apps.AppUtils.startEditor(5, "")
                'Logger.logInfo(Rstyx.Utilities.IO.FileUtils.FilePart.Dir.ToDisplayString())
                'Logger.logInfo(EnumExtensions.ToDisplayString(Rstyx.Utilities.IO.FileUtils.FilePart.Dir))
                
                'Me.showHelpFile()
                
                'Dim success As Boolean = TrySetProperty("Textbo", "teschtttt", New String() {"hh", "teschtttyy"})
                
                'Throw New InvalidDataException("+++++++++++++++++++++++++++")
                
                'Dim fic As IO.FileInfoCollection = IO.FileUtils.findFile("*.bsh", "G:\Tools\jEdit_50\macros\Aktive_Datei", Nothing, SearchOption.TopDirectoryOnly)
                'Dim fi As FileInfo = IO.FileUtils.findFile("*.bsh", "G:\Tools\jEdit_50\macros\Aktive_Datei", Nothing, Nothing)
                'Logger.logInfo(fi.FullName)
                
                'Dim Field = "UserDomaiN"
                'Dim TableName = "Standorte$y"
                'Dim Workbook = "R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xls"
                
                'Dim DBconn  As OleDbConnection = Nothing
                'Dim CSB As OleDbConnectionStringBuilder = New OleDbConnectionStringBuilder()
                'CSB.DataSource = Workbook
                ''CSB.Provider   = "Microsoft.Jet.OLEDB.4.0"
                ''CSB.Add("Extended Properties", "Excel 8.0;HDR=Yes;IMEX=1;")
                'CSB.Provider   = "yyMicrosoft.ACE.OLEDB.12.0"
                'CSB.Add("Extended Properties", "Excel 8.0;HDR=Yes;IMEX=1;")
                'DBconn = New OleDbConnection(CSB.ConnectionString)
                'DBconn.Open()
                'DBconn.Close()
                
                'Using XLconn As System.Data.OleDb.OleDbConnection = DBUtils.connectToExcelWorkbook("R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xls")
                'Using Table As System.Data.DataTable = DBUtils.getExcelSheet(TableName, Workbook)
                '    ''Dim Table As DataTable = DBUtils.getOleDBTable(TableName, XLconn)
                '    'Dim Table As DataTable = XLconn.getTable(TableName)
                '    'Logger.logInfo(StringUtils.sprintf("Feld '%s' existiert = %s", Field, XLconn.TableContainsField(TableName, Field)))
                '    'Logger.logInfo(StringUtils.sprintf("Feld '%s' existiert = %s", Field, Table.containsField(Field)))
                '    
                '    'Dim SQL = "SELECT * FROM " & TableName
                '    'Dim Table As DataTable = DBUtils.queryOLEDB(SQL, XLconn)
                '    'Dim Query = From site In Table.AsEnumerable() Where site.Field(Of String)("UserDomain") = "dummy"
                '    
                '    'Dim Table As DataTable = DBUtils.getExcelSheet(TableName, Workbook)
                '    'Dim yes = Table.containsField(Field)
                '    Logger.logInfo(StringUtils.sprintf("Existiert Feld '%s' in Tabelle '%s' = %s", Field, Table.TableName, Table.containsField(Field)))
                '    
                '    For Each row As System.Data.DataRow In Table.AsEnumerable()
                '        Logger.logInfo(StringUtils.sprintf("Standort '%s' = UserDomain '%s'", row("Standort_ID"), row("UserDomain")))
                '    Next 
                'End Using
                
                'UI.ClassEvents.SelectAllOnTextBoxGotFocus = (Not UI.ClassEvents.SelectAllOnTextBoxGotFocus)
                'Logger.logInfo(StringUtils.sprintf("gültig     = %s", Rstyx.Utilities.IO.FileUtils.isValidFilePath(Me.Textbox)))
                'Logger.logInfo(StringUtils.sprintf("korrigiert = %s", Rstyx.Utilities.IO.FileUtils.validateFilePathSpelling(Me.Textbox)))
                
            Catch ex As ParseException
                'Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadFailed, TcReader.FilePath))
                'TcReader.ParseErrors.ShowInJEdit()
                
            Catch ex As System.Exception
                Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                'Throw New RemarkException("test_1(): Unerwarteter Fehler.", ex)
            End Try
        End Sub
        
        Protected Overrides Sub showHelpFile()
            Throw New InvalidDataException("showHelpFile()  +++++++++++++++++++++++++++")
        End Sub
        
        ''' <summary> Checks if test_1 could be started. </summary>
         ''' <returns> Boolean </returns>
        Private Function CanStartTestTask() As Boolean
            Dim RetValue  As Boolean = False
            Try
                'Throw New RemarkException("AAAAAAAAAAA")
                RetValue = True
            Catch ex As System.Exception
                Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
            End Try
            Return RetValue
        End Function
        
        Public Sub test_0()
            'Dim s As String
            'Dim sa() As String
            'Dim d As Nullable(Of Double)
            Dim li  As GeoMath.DBAGTrackInfo
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
            li = GeoMath.getDBAGTrackTitle(6265)
            Logger.logInfo(li.ShortTitle)
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
            'Dim found  As System.Collections.ObjectModel.Collection(Of System.IO.fileinfo) = IO.FileUtils.findFiles(File, Folders, ";", IO.SearchOption.AllDirectories)
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
