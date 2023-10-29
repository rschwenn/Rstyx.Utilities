
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.DataTableExtensions
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Math
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks

'Imports PdfSharp.Pdf
'Imports PdfSharp.Pdf.IO

'Imports org.pdfclown.documents
'Imports org.pdfclown.files

'Imports ExcelToEnumerable
'Imports ExcelToEnumerable.Exceptions

Imports ExcelDataReader

Imports Rstyx.Utilities
Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.Domain.IO
Imports Rstyx.Utilities.Math.MathExtensions
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.PDF.PdfUtils
Imports Rstyx.Utilities.UI.ViewModel
Imports Rstyx.Utilities.StringUtils

Public Class MainViewModel
    Inherits Rstyx.Utilities.UI.ViewModel.ViewModelBase
    
    Private Shared ReadOnly Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.TestWpf.MainViewModel")
    
    
    #Region "Initializing and Finalizing"
        
        ''' <summary> Creates a new instance and sets display names. </summary>
        Public Sub New()
            Logger.LogDebug("New(): Init MainViewModel ...")
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
        Public Property ValidatingProperty1  As Double
        
        Private _BackStoreKilometer  As Kilometer = New Kilometer(1234.5678)
        'Private _ValidatingProperty2 As String
        Public Property ValidatingProperty2 As String
            Get
                Return _BackStoreKilometer.ToString()
            End Get
            Set(value As String)
                _BackStoreKilometer.Parse(value)
                Me.NotifyPropertyChanged("ValidatingProperty2")
            End Set
        End Property
        
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
                        CmdInfo.ExecuteAction       = AddressOf Me.Test_1
                        CmdInfo.CanExecutePredicate = AddressOf Me.CanStartTestTask
                        CmdInfo.Decoration          = Decoration
                        
                        _TestTaskAsyncCommand = New AsyncDelegateUICommand(CmdInfo, CancelCallback:=Nothing, SupportsCancellation:=False, runAsync:=True)
                        '_TestTaskAsyncCommand = New AsyncDelegateUICommand(CmdInfo, CancelCallback:=Nothing, SupportsCancellation:=False, runAsync:=True, ThreadAptState:=Threading.ApartmentState.STA)
                    End If
                Catch ex As System.Exception
                    Logger.LogError(ex, sprintf(Rstyx.Utilities.Resources.Messages.Global_ErrorCreatingCommandIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
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
        
        Public Sub Test()
            
            'Dim Task1 As Task = Task.Factory.StartNew(AddressOf test_1)
            
            Dim TestEnum2 As ArrayUtils.SortType = 2 'ArrayUtils.SortType.Numeric
            Logger.LogInfo(sprintf("Enum Value=%s:  Display=%s", TestEnum2.ToString(), TestEnum2.ToDisplayString()))
            
        End Sub
        
        
        Public Sub TestOrder()
            
            Dim Files As New Collection(Of String)
            Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot - Sortierung absteigend\Profil N94-118.pdf")
            Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot - Sortierung absteigend\Profil N94-127.pdf")
            Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot - Sortierung absteigend\Profil N94-129   N94-128.pdf")
            Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot - Sortierung absteigend\Profil N95-1s   N95-2s.pdf")
            
            Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot_negative_Km\Profil  km 0.1 + 75.78.pdf")
            Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot_negative_Km\Profil  km -0.2 - 22.01.pdf")
            Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot_negative_Km\Profil  km -0.1 - 12.13.pdf")

            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.4 + 82.30, Gleis 5500-re, Krbw Str 5544, Stütze 1.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 27.93, Gleis 5500-li, Krbw Str 5544, Stütze 4.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.4 + 88.55, Gleis 5500-re, Krbw Str 5544, KUK.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.4 + 96.79, Gleis 5500-re, Krbw Str 5544, Stütze 2.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 14.66, Gleis 5500-li, Krbw Str 5544, Stütze 3.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 14.82, Gleis 5501-re, Krbw Str 5544, Stütze 3.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 16.98, Gleis 5501-re, Krbw Str 5544, KUK.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 27.93, Gleis 5501-re, Krbw Str 5544, Stütze 4.pdf")
            
            For Each FilePath As String In Files.OrderBy(Function(ByVal PathName) PathName, New AlphanumericKmComparer(IgnoreCase:=True))
                Logger.LogInfo(FilePath)
            Next
            
            #If DEBUG Then
                Dim th As System.Threading.Thread = System.Threading.Thread.CurrentThread
                System.Diagnostics.Debug.Print(sprintf("TestPDF: Current thread ID = %d,  ApartmentState = %s,  IsThreadPoolThread = %s,  IsBackground = %s", th.ManagedThreadId, th.GetApartmentState().ToString(), th.IsThreadPoolThread, th.IsBackground))
            #End If
            
        End Sub
        
        Public Sub TestPDF()
            
            Dim Files As New Collection(Of String)
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.4 + 82.30, Gleis 5500-re, Krbw Str 5544, Stütze 1.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.4 + 88.55, Gleis 5500-re, Krbw Str 5544, KUK.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.4 + 96.79, Gleis 5500-re, Krbw Str 5544, Stütze 2.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 14.66, Gleis 5500-li, Krbw Str 5544, Stütze 3.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 14.82, Gleis 5501-re, Krbw Str 5544, Stütze 3.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 16.98, Gleis 5501-re, Krbw Str 5544, KUK.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 27.76, Gleis 5500-li, Krbw Str 5544, Stütze 4.pdf")
            'Files.Add("X:\Quellen\DotNet\VisualBasic\Rstyx.Microstation\BatchplotAddin\Test\Batchplot-Bug_6\Lichtraumprofil  Km 5.5 + 27.93, Gleis 5501-re, Krbw Str 5544, Stütze 4.pdf")
            
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.0 - 27.18.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.0 - 67.10.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.0 + 17.60 b.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.0 + 17.60.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.0 + 68.33.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.1 + 22.03.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.1 + 75.78.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.8 + 84.82.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  0.9 + 29.88.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km  1.4 + 76.54.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km -0.1 - 12.13.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km   -0.1 - 67.04.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km -0.2 - 22.01.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km -0.2 - 73.95.pdf")
            Files.Add("D:\daten\Batchplot_negative_Km\Einzelplots_Test\Profil  km a 0.2 + 20.05.pdf")
            
            
            #If DEBUG Then
                Dim th As System.Threading.Thread = System.Threading.Thread.CurrentThread
                System.Diagnostics.Debug.Print(sprintf("TestPDF: Current thread ID = %d,  ApartmentState = %s,  IsThreadPoolThread = %s,  IsBackground = %s", th.ManagedThreadId, th.GetApartmentState().ToString(), th.IsThreadPoolThread, th.IsBackground))
            #End If
            
            Const Filename As String = "T:\TestOutput.pdf"
            
            Call JoinPdfFiles(InputPaths:=Files, OutputPath:=Filename, Order:=True, DeleteInput:=False)
            
            ' ...and start a viewer.
            System.Diagnostics.Process.Start(Filename)
            
        End Sub
        
        Public Sub TestJPEG_1()
            Dim SourceImage As Bitmap = New Bitmap("D:\Daten\LRP\Fotos\02070001.JPG")
            
            Const TargetWidth  As Integer = 512
            Const TargetHeight As Integer = 768
            
            Dim IsRotated As Boolean = False
            
            ' EXIF: all properties.
            'Dim ImageProperties() As PropertyItem = SourceImage.PropertyItems
            
            ' Orientation (see http://sylvana.net/jpegcrop/exif_orientation.html).
            Dim PropIDs As Collection(Of Integer) = New Collection(Of Integer)(SourceImage.PropertyIdList)
            Dim ExifOrientationID As Integer = &H0112  ' decimal: 37510
            If (PropIDs.Contains(ExifOrientationID)) Then
                Dim Orientation As Byte = SourceImage.GetPropertyItem(ExifOrientationID).Value(0)
                
                If (Orientation > 1) Then
                    Dim ImageRotateFlipType As Dictionary(Of Byte, RotateFlipType) = New Dictionary(Of Byte, RotateFlipType)
                    ImageRotateFlipType.Add(0, RotateFlipType.RotateNoneFlipNone)    ' Orientation unknown.
                    ImageRotateFlipType.Add(1, RotateFlipType.RotateNoneFlipNone)    ' Orientation o.k.
                    ImageRotateFlipType.Add(2, RotateFlipType.RotateNoneFlipX)
                    ImageRotateFlipType.Add(3, RotateFlipType.Rotate180FlipNone)
                    ImageRotateFlipType.Add(4, RotateFlipType.RotateNoneFlipY)
                    ImageRotateFlipType.Add(5, RotateFlipType.Rotate90FlipX)
                    ImageRotateFlipType.Add(6, RotateFlipType.Rotate90FlipNone)
                    ImageRotateFlipType.Add(7, RotateFlipType.Rotate270FlipX)
                    ImageRotateFlipType.Add(8, RotateFlipType.Rotate270FlipNone)
                    
                    SourceImage.RotateFlip(ImageRotateFlipType(Orientation))
                    IsRotated = True
                End If
            End If
            
            ' Size.
            Dim DoResize As Boolean = (Not ((TargetWidth = SourceImage.Width) AndAlso (TargetHeight = SourceImage.Height)) )
            'If (DoResize OrElse IsRotated) Then
                Dim TargetImage As New Bitmap(TargetWidth, TargetHeight)
                Dim FlagColor   As Color = Color.FromArgb(255, 0, 0, 255)
                
                Using TargetGraphics As Graphics = Graphics.FromImage(TargetImage)
                    TargetGraphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    TargetGraphics.DrawImage(SourceImage, 0, 0, TargetImage.Width, TargetImage.Height)
                    
                    TargetGraphics.FillRectangle(Brushes.LightGray, New Rectangle(100, 700, TargetWidth - 200, 40))
                    TargetGraphics.FillEllipse(Brushes.Blue,        New Rectangle(115, 715, 10, 10))  ' Flag
                    TargetGraphics.FillEllipse(Brushes.Blue,        New Rectangle(TargetWidth - 125, 715, 10, 10))  ' Flag
                    'TargetGraphics.FillRectangle(Brushes.Blue,      New Rectangle(114, 716, 8, 8))  ' Flag
                    'TargetGraphics.FillRectangle(Brushes.Blue,      New Rectangle(TargetWidth - 122, 716, 8, 8))  ' Flag
                    
                    Dim DirText As String = "Bild in Richtung"
                    'Dim DirText As String = "Bild in Richtung"
                    Dim DirFont As Font   = New Font(FontFamily.GenericSansSerif, 11, FontStyle.Regular, GraphicsUnit.Point)
                    Dim DirSize As SizeF  = TargetGraphics.MeasureString(DirText, DirFont)
                    TargetGraphics.DrawString(DirText, DirFont, Brushes.Blue, TargetWidth / 2 - DirSize.Width / 2, 705)
                End Using
                
                ' http://www.wisotop.de/farbabstand-farben-vergleichen.shtml
                
                Const X1 As Integer = 118
                Const Y1 As Integer = 720
                Dim PixelColor As Color = TargetImage.GetPixel(X1, Y1)
                'Dim ColorDist  As Double = Sqrt( Pow(FlagColor.R - PixelColor.R, 2) + Pow(FlagColor.G - PixelColor.G, 2) + Pow(FlagColor.B - PixelColor.B, 2) )
                'Dim ColorDist  As Double = Sqrt( ((FlagColor.R - PixelColor.R) * (FlagColor.R - PixelColor.R)) +
                '                                 ((FlagColor.G - PixelColor.G) * (FlagColor.G - PixelColor.G)) +
                '                                 ((FlagColor.B - PixelColor.B) * (FlagColor.B - PixelColor.B))
                '                               )
                Dim dR As Double = CInt(FlagColor.R) - CInt(PixelColor.R)
                Dim dG As Double = CInt(FlagColor.G) - CInt(PixelColor.G)
                Dim dB As Double = CInt(FlagColor.B) - CInt(PixelColor.B)
                Dim ColorDist  As Double = Sqrt( (dR * dR) + (dG * dG) + (dG * dG) )
                
                
                Dim IsFlag As Boolean = (Abs(ColorDist) < 20)
                TargetImage.MakeTransparent()
                TargetImage.SetPixel(X1, Y1, FlagColor)
                
                TargetImage.Save("D:\Daten\LRP\Fotos\Test_Ziel.jpg", ImageFormat.Jpeg)
            'End If
            
            
            Dim dummy As String = "d"
        End Sub
        
        ' For TestExcelToEnumerator:
        'Public Class SiteRecord
         '  Public Property deactivated         As String
         '  Public Property active              As String
         '  Public Property Standort_ID         As String
         '  Public Property UserDomain          As String
         '  Public Property Name                As String
         '  Public Property PLZ                 As String
         '  Public Property Ort                 As String
         '  Public Property Strasse             As String
         '  Public Property Tel                 As String
         '  Public Property Fax                 As String
         '  Public Property Mail                As String
         '  Public Property Hinweise            As String
         '  Public Property Fusszeile_Excel_1   As String
         '  Public Property SourceExcelRow      As Integer
        'End Class
        
        'Public Sub TestExcelToEnumerator()
         '  
         '  Dim TableName  = "Standorte"
         '  Dim Workbook   = "X:\Quellen\DotNet\VisualBasic\Rstyx.Utilities\TestWpf\TestData\Standortdaten.xlsx"
         '  Dim Exceptions = New List(Of Exception)()
         '  
         '  Logger.LogInfo("TestExcelToEnumerator:")
         '  
         '  Using oFS As FileStream = File.OpenRead(Workbook)
         '      
         '      Dim Options As ExcelToEnumerableOptionsBuilder(Of SiteRecord) = (New ExcelToEnumerableOptionsBuilder(Of SiteRecord)) _
         '          .UsingSheet(TableName) _
         '          .UsingHeaderNames(True) _
         '          .OutputExceptionsTo(Exceptions) _
         '          .Property(Function(oSite As SiteRecord) oSite.Standort_ID).ShouldBeUnique() _
         '          .Property(Function(oSite As SiteRecord) oSite.Standort_ID).IsRequired() _
         '          .Property(Function(oSite As SiteRecord) oSite.active).UsesColumnNamed("aktiv") _
         '          .Property(Function(oSite As SiteRecord) oSite.active).ShouldBeOneOf("1", "") _
         '          .Property(Function(oSite As SiteRecord) oSite.SourceExcelRow).MapsToRowNumber()
         '      
         '      'Dim Sites As IEnumerable(Of SiteRecord) = oFS.ExcelToEnumerable(Of SiteRecord)(Options)
         '      
         '      For Each Site As SiteRecord In oFS.ExcelToEnumerable(Of SiteRecord)(Options)
         '          Logger.LogInfo(sprintf("Zeile=%2d, aktiv=%s,  ID=%s, PLZ=%s, Ort=%s", Site.SourceExcelRow, Site.active, Site.Standort_ID, Site.PLZ, Site.Ort))
         '      Next
         '      
         '  End Using
         '  
         '  For Each ex As Exception In Exceptions
         '      If (TypeOf ex Is ExcelToEnumerableCellException) Then
         '          Dim ex2 As ExcelToEnumerableCellException = DirectCast(ex, ExcelToEnumerableCellException)
         '          Logger.LogInfo(sprintf("Fehler:  Zeile %2d, Feld=%s:  %s", ex2.RowNumber, ex2.PropertyPath, ex2.Message))
         '      Else 
         '          Logger.LogInfo(sprintf("Fehler:  %s", ex.Message))
         '      End If
         '  Next
         '  
        'End Sub
        
        ''' <summary> Gets a whole Table from a given Excel worksheet (via ExcelDataReader). </summary>
         ''' <param name="XlFilePath"> Full path of Excel worksheet. </param>
         ''' <param name="TableName">  The name of the table to get. </param>
         ''' <returns>                 The DataTable. </returns>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="TableName"/> is <see langword="null"/> or empty. </exception>
         ''' <exception cref="System.IO.FileNotFoundException"> <paramref name="XlFilePath"/> hasn't been found (may be empty or invalid). </exception>
         ''' <exception cref="Rstyx.Utilities.RemarkException"> Wraps any exception with a clear message. </exception>
        Public Shared Function GetExcelSheet(TableName As String, XlFilePath As String) As DataTable
            
            If (TableName.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("TableName")
            If (Not File.Exists(XlFilePath)) Then Throw New System.IO.FileNotFoundException(Rstyx.Utilities.Resources.Messages.DBUtils_ExcelWorkbookNotFound, XlFilePath)
            Try
                Logger.LogDebug(sprintf("getExcelSheet(): Try to get table '%s' from Excel workbook '%s'.", TableName, XlFilePath))
                
                Using oFS As FileStream = File.OpenRead(XlFilePath)
                    
                    Dim DataSetConfig As New ExcelDataSetConfiguration()
                    DataSetConfig.UseColumnDataType  = False
                    DataSetConfig.ConfigureDataTable = Function(Reader As IExcelDataReader) (New ExcelDataTableConfiguration() With {.UseHeaderRow = True})
                    DataSetConfig.FilterSheet        = Function(Reader As IExcelDataReader, SheetIndex As Integer) (Reader.Name = TableName)
                    
                    Dim XlReader As IExcelDataReader = ExcelReaderFactory.CreateOpenXmlReader(oFS)
                    Dim Sheets   As DataSet          = XlReader.AsDataSet(DataSetConfig)
                    
                    If (Not Sheets.Tables.Contains(TableName)) Then
                        Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.DBUtils_TableNotFoundInExcelWorkbook, TableName, XlFilePath))
                    End If
                    
                    Return Sheets.Tables(TableName)
                End Using
                
            Catch ex As System.Exception
                Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.DBUtils_OpenExcelWorksheetFailed, TableName, XlFilePath), ex)
            End Try
        End Function
        
        Public Sub TestExcelDataReader()
            
            Dim TableName  = "Standorte"
            Dim Workbook   = "X:\Quellen\DotNet\VisualBasic\Rstyx.Utilities\TestWpf\TestData\Standortdaten.xlsx"
            
            Logger.LogInfo("TestExcelDataReader:")
            Dim Sheet As DataTable = GetExcelSheet(TableName, Workbook)
            
            For Each Site As DataRow In Sheet.Rows
                Logger.LogInfo(sprintf("aktiv=%s,  ID=%s, PLZ=%s, Ort=%s", Site("aktiv"), Site("Standort_ID"), Site("PLZ"), Site("Ort")))
            Next
            
            'Using oFS As FileStream = File.OpenRead(Workbook)
            '
            '    Dim Sheet As DataTable
            '    
            '    Dim DataSetConfig As New ExcelDataSetConfiguration()
            '    DataSetConfig.UseColumnDataType  = False
            '    DataSetConfig.ConfigureDataTable = Function(Reader As IExcelDataReader) (New ExcelDataTableConfiguration() With {.UseHeaderRow = True})
            '    DataSetConfig.FilterSheet        = Function(Reader As IExcelDataReader, SheetIndex As Integer) (Reader.Name = TableName)
            '    
            '    Dim XlReader As IExcelDataReader = ExcelReaderFactory.CreateOpenXmlReader(oFS)
            '    Dim Sheets   As DataSet = XlReader.AsDataSet(DataSetConfig)
            '    
            '    If (Not Sheets.Tables.Contains(TableName)) Then
            '        Logger.LogInfo("Tabelle xxx existiert nicht!")
            '    Else
            '        Sheet = Sheets.Tables(TableName)
            '        For Each Site As DataRow In Sheet.Rows
            '            Logger.LogInfo(sprintf("aktiv=%s,  ID=%s, PLZ=%s, Ort=%s", Site("aktiv"), Site("Standort_ID"), Site("PLZ"), Site("Ort")))
            '        Next
            '    End If
            'End Using
            
        End Sub
        
        Public Shared Sub StartProcessTest()
            
            Logger.LogInfo("StartProcessTest startet ..")
            'Dim Batch As String = "G:\Bat\Querprf.bat"
            Dim Batch As String = "X:\Quellen\Wscripts\Querprf\Querprf.bat"
            Dim ipkt  As String = "X:\Quellen\Wscripts\Querprf\Bf_Memmingen_QP_5400.ipkt"
            
            ' Start process.
            Dim StartInfo  As New System.Diagnostics.ProcessStartInfo()
            StartInfo.FileName  = Batch
            StartInfo.Arguments = ipkt
            StartInfo.UseShellExecute = False
            StartInfo.WorkingDirectory = "X:\Quellen\Wscripts\Querprf"
            
            'Logger.LogDebug(StringUtils.Sprintf("startEditor(): Auszuführende Datei: '%s'.", StartInfo.FileName))
            'Logger.LogDebug(StringUtils.Sprintf("startEditor(): Argumente: '%s'.", StartInfo.Arguments))
            'Logger.LogDebug(StringUtils.Sprintf("startEditor(): %s wird gestartet.", TargetEditor.ToDisplayString()))

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture
            
            Using Proc As System.Diagnostics.Process = System.Diagnostics.Process.Start(StartInfo)
            End Using
            Logger.LogInfo("StartProcessTest Ende.")
        End Sub
        

        ''' <summary>  Gets the value of a property given by (cascaded) name, from a given object. </summary>
         ''' <param name="Subject">      The object to inspect. </param>
         ''' <param name="PropertyPath"> Path to the property name, based on <paramref name="Subject"/>, i.e. "prop1" or "prop1.prop2.prop3" </param>
         ''' <param name="Flags">        Determines, wich properties should be considered. </param>
         ''' <returns> The found property value on success, otherwise <see langword="null"/>. </returns>
         ''' <remarks>
         ''' <para>
         ''' <paramref name="PropertyPath"/>: Path separator is a point. The first part has to be a direct property of <paramref name="Subject"/>. 
         ''' The last part is the property of interest, whose value will be returened.
         ''' </para>
         ''' <para>
         ''' If the returned value is <see langword="null"/>, either this is the property's value or the property hasn't been found.
         ''' </para>
         ''' </remarks>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="Subject"/> is <see langword="null"/>. </exception>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="PropertyPath"/> is <see langword="null"/> or empty or whitespace only. </exception>
        Private Function Tmp_GetPropertyValue(Subject As Object, PropertyPath As String, Flags As BindingFlags) As Object
                
            If (Subject Is Nothing) Then Throw New System.ArgumentNullException("Subject")
            If (PropertyPath.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("PropertyPath")

            Dim RetValue        As Object = Nothing
            Dim PropertyNames() As String = PropertyPath.Split("."c)
            
            For i As Integer = 0 To PropertyNames.Count - 1
                
                Dim pi As PropertyInfo = Subject.GetType().GetProperty(PropertyNames(i), Flags)
                
                If (pi Is Nothing) Then Exit For

                Dim PropertyValue As Object = pi.GetValue(Subject)
                If (i < (PropertyNames.Count - 1)) Then
                    PropertyValue = Tmp_GetPropertyValue(PropertyValue, PropertyNames(i + 1), Flags)
                End If
                RetValue = PropertyValue
            Next

            Return RetValue
        End Function

        Private Function GetPropertyInfo(Subject As Object, PropertyPath As String) As PropertyInfo
                
            Dim RetValue        As PropertyInfo = Nothing
            Dim PropertyNames() As String  = PropertyPath.Split("."c)
            Dim NamesCount      As Integer = PropertyNames.Count
            Dim DeclaringType   As Type    = Subject.GetType()
            
            For i As Integer = 0 To NamesCount - 1
                
                Dim PropertyName As String = PropertyNames(i)
                Dim pi As PropertyInfo = DeclaringType.GetProperty(PropertyName)
                
                If (pi IsNot Nothing) Then
                    If (pi.DeclaringType.Name = DeclaringType.Name) Then
                        If (i < (NamesCount - 1)) Then
                            pi = GetPropertyInfo(pi.GetValue(Subject), PropertyNames(i + 1))
                        End If
                        RetValue = pi
                    End If
                End If
            Next
            Return RetValue
        End Function

        Public Sub Test_1(CancelToken As System.Threading.CancellationToken)
            
            'Dim TcReader As TcFileReader
            
            'Try
                Logger.LogInfo("")

                Dim d1 As Double = 1 / Double.PositiveInfinity
                Dim d2 As Double = 1 / Double.NegativeInfinity


                Dim Str1 As String = "5"
                'Dim Str2 As String = " "
                'Dim result As Integer = Str1.CompareTo(Str2)
                'Logger.LogInfo(sprintf(" '%s' CompareTo '%s' = %d\n", Str1, Str2, result))
                '
                'Dim Zahl1 As Double = Double.NaN
                'Dim Zahl2 As Double = Double.NaN
                'Logger.LogInfo(sprintf(" '%.3f' = '%.3f' => %s\n", Zahl1, Zahl2, Zahl1.EqualsTolerance(Zahl2, 0.1)))
                '
                'Dim Km3 As New Kilometer()
                'Km3 = "1.2 + 345.678"
                ''Km3 = CType("1.2 + 345.678", Kilometer)
                ''Dim Km2 As Kilometer = CType(TypeDescriptor.GetConverter(GetType(Kilometer)).ConvertFromString("1.2 + 345.678"), Kilometer)
                'Dim Km2 As Kilometer = TypeDescriptor.GetConverter(GetType(Kilometer)).ConvertFromString("1.2 + 345.678")
                'Dim Km1 As New Kilometer()
                'Dim KilometerAttributes As AttributeCollection = TypeDescriptor.GetAttributes(Km1)
                'Dim myAttribute As TypeConverterAttribute = CType(KilometerAttributes(GetType(TypeConverterAttribute)), TypeConverterAttribute)
                'Logger.LogInfo("Type Conveter für 'Kilometer' ist: " & myAttribute.ConverterTypeName)
                'Logger.LogInfo("")
                '
                'Dim Bool1 As Boolean = False
                'Dim Bool2 As Boolean = False
                'Dim Bool3 As Boolean = (Bool1 = Bool2)
                'Logger.LogInfo(sprintf(" '%s' = '%s' => %s\n", Bool1, Bool2, Bool3))
                '
                '
                'Dim InputFile As GeoPointFile = New IpktFile() With {.EditOptions = GeoPointEditOptions.None}
                'InputFile.FilePath = "X:\Quellen\DotNet\VisualBasic\Rstyx.Apps\VEedit\Test\GisPnr2i_Beispiele\Test2_i.ipkt"
                ''Dim Points As New GeoPointList(SourcePointList:=InputFile.PointStream, MetaData:=InputFile, CancelToken:=CancelToken, StatusIndicator:=Me)
                'For Each ip As GeoIPoint In InputFile.PointStream
                '    Dim tcp   As GeoTcPoint  = ip.AsGeoTcPoint
                '    ''Dim vp2  As GeoVEPoint = ip.AsGeoVEPoint
                '    'Logger.LogInfo(ip.ID)
                'Next
                
                
                
                ''Call StartProcessTest()
                ''Call TestExcelDataReader()
                '
                ''Call TestOrder()
                ''Call TestPDF()
                ''Call TestJPEG()
                '
                'Logger.LogInfo(sprintf("  Environment.GetEnvironmentVariable(""PATH"")       => '%s'", Environment.GetEnvironmentVariable("PATH")))
                'Logger.LogInfo(sprintf("  Environment.ExpandEnvironmentVariables(""%%PATH%%"") => '%s'", Environment.ExpandEnvironmentVariables("%PATH%")))
                'Logger.LogInfo("")
                '
                'Logger.LogInfo(sprintf("  GeoPointEditOptions.ParseInfoForActualCant => '%s'", GeoPointEditOptions.ParseInfoForActualCant.ToDisplayString()))
                'Logger.LogInfo(sprintf("  GeoPointEditOptions.ParseInfoForPointKind => '%s'", GeoPointEditOptions.ParseInfoForPointKind.ToDisplayString()))
                'Logger.LogInfo(sprintf("  GeoPointEditOptions.Parse_iTC => '%s'", GeoPointEditOptions.Parse_iTC.ToDisplayString()))
                '
                'Logger.LogInfo(sprintf("  GeoPointOutputOptions.CreateInfoWithActualCant => '%s'", GeoPointOutputOptions.CreateInfoWithActualCant.ToDisplayString()))
                'Logger.LogInfo(sprintf("  GeoPointOutputOptions.CreateInfoWithPointKind => '%s'", GeoPointOutputOptions.CreateInfoWithPointKind.ToDisplayString()))
                'Logger.LogInfo(sprintf("  GeoPointOutputOptions.Create_iTC => '%s'", GeoPointOutputOptions.Create_iTC.ToDisplayString()))
                '
                'Dim TestString As String = "u=5  WA   28"
                'Dim TestString2 As String = Nothing
                'Dim Matches As System.Text.RegularExpressions.MatchCollection = TestString.GetMatches("  ")
                'If (Matches.Count > 0) Then
                '    TestString2 = TestString.Left(Matches(0).Index) & TestString.Substring(Matches(0).Index + 1)
                'End If
                'Logger.LogInfo(sprintf("  '%s' => '%s'", TestString, TestString2))
                
                'Dim SourcePoint As New GeoIPoint ()
                'SourcePoint.ParseTextForKindCodes(Me.Textbox)
                'Logger.LogInfo(sprintf("  Punktart = %s,  VArtAB = %s,  VArt = %s,  u = %3.0f    Info = '%s'", SourcePoint.Kind.ToDisplayString(), SourcePoint.MarkTypeAB, SourcePoint.MarkType, SourcePoint.ActualCant * 1000, SourcePoint.Info))
                'SourcePoint.Info    = Me.Textbox
                'SourcePoint.Comment = " HB 22.33"
                'SourcePoint.ParseInfoForKindHints(TryComment:=False)
                'Logger.LogInfo(sprintf("  Info = '%s'  =>   Punktart = %s   u = %3.0f   Info neu = '%s'    ", Me.Textbox, SourcePoint.Kind.ToDisplayString(), SourcePoint.ActualCant * 1000, SourcePoint.Info))
                
                'Dim Km1 As Kilometer = New Kilometer("-0.1 - 212.13")
                'Dim Km2 As Kilometer = New Kilometer("-0.1 - 12.13")
                'Dim Km3 As Kilometer = New Kilometer("*12.3456")
                'Dim Km4 As Kilometer = New Kilometer("123.4567*")
                'Dim Km5 As Kilometer = New Kilometer("0.1 + 12.13")
                'Logger.LogInfo(sprintf("Km %+18s  TDB = %9.2f", Km1.ToKilometerNotation(2), Km1.TDBValue))
                'Logger.LogInfo(sprintf("Km %+18s  TDB = %9.2f", Km2.ToKilometerNotation(2), Km2.TDBValue))
                'Logger.LogInfo(sprintf("Km %+18s  TDB = %9.2f", Km3.ToKilometerNotation(2), Km3.TDBValue))
                'Logger.LogInfo(sprintf("Km %+18s  TDB = %9.2f", Km4.ToKilometerNotation(2), Km4.TDBValue))
                'Logger.LogInfo(sprintf("Km %+18s  TDB = %9.2f", Km5.ToKilometerNotation(2), Km5.TDBValue))
                
                'Logger.LogInfo(sprintf("CurrentCulture = %s", System.Globalization.CultureInfo.CurrentCulture.Name))
                'Dim d1 As Double = Double.NaN
                'd1.TryParse("+Unendlich")
                'Logger.LogInfo(sprintf("%+5.3f", d1))
                '
                'Dim d2 As Double = Double.NegativeInfinity
                'Dim d3 As Double = Double.PositiveInfinity
                'Logger.LogInfo(d2.ToString())
                'Logger.LogInfo(d3.ToString())
                'Logger.LogInfo(sprintf("%+5.3f", d2))
                ''Logger.LogInfo(sprintf("%+5.3f", d3))
                '
                'Dim NegInf As String = "" & ChrW(&H221E)
                '
                'Logger.LogInfo("")
                '
                'System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture.Clone()
                'System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.PositiveInfinitySymbol = ChrW(&H221E)
                'System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NegativeInfinitySymbol = "-" & ChrW(&H221E)
                '
                ''System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("de")
                'Logger.LogInfo(sprintf("CurrentCulture = %s", System.Globalization.CultureInfo.CurrentCulture.Name))
                '
                'd1 = Double.NaN
                'd1.TryParse("+Unendlich")
                'Logger.LogInfo(sprintf("%+5.3f", d1))
                '
                'd2 = Double.NegativeInfinity
                'd3 = Double.PositiveInfinity
                'Logger.LogInfo(d2.ToString())
                'Logger.LogInfo(d3.ToString())
                'Logger.LogInfo(sprintf("%+5.3f", d2))
                '
                'd1.TryParse(NegInf)
                'Logger.LogInfo(sprintf(" %s => %+5.3f", NegInf, d1))
                
                'Dim int1 As Integer = 1
                'Dim int2 As Nullable(Of Integer) = 2
                'Dim int3 As Nullable(Of Integer) = Nothing
                'Dim TestType1  As Type = int1.GetType()
                'Dim TestType2  As Type = GetType(Nullable(Of Integer))
                'Dim TestType3  As Type = GetType(Nullable(Of))
                'Dim TestType4  As Type = TestType2.GetGenericTypeDefinition()
                'Dim test  As Boolean = (TestType2.IsGenericType AndAlso (TestType4 Is TestType3))
                'Dim test2 As Boolean = (TestType2.IsGenericType AndAlso (TestType2.GetGenericTypeDefinition() Is GetType(Nullable(Of))))
                'Dim test3 As Boolean = (TestType2.Name = "Nullable`1")
                'Dim test4 As Boolean = (TestType2 Is GetType(Nullable(Of Integer)))
                '
                'Dim text As String = sprintf("%5.3f%5.3f", "1", int3)
                ''Dim text As String = CStr(Nothing)
                ''int2 = int3 '.GetValueOrDefault()
                
                'Dim KV As New KvFile(Me.FilePath1)
                'Dim KV As New KvFile("D:\Daten\koo\Test-KV.kv")
                
                ''Dim KF As New KfFile(Me.FilePath1)
                ''Dim iP As New iPktFile(Me.FilePath1)
                'Dim TC As New TcFileReader(Me.FilePath1)
                ''TC.FilePath = "T:\_test\zaun_li_IstGleis.txt"
                ''TC.FilePath = "X:\Quellen\Awk\Bahn\SOLLIST\2018-06-22_Bf_Ungerhausen_MVk_GL3.A0"
                ''Dim TC As New TcFileReader("X:\Quellen\DotNet\VisualBasic\Rstyx.Apps\VEedit\Test\Ulm_LiRa_4700_Endzu.A0")
                ''TC.EditOptions = GeoPointEditOptions.GuessAllKindsFromInfo
                ''KV.CollectParseErrors = True
                ''TC.ShowParseErrorsInJedit = False
                ''iP.Constraints = GeoPointConstraints.UniqueID
                ''TC.CollectParseErrors = True
                ''KV.Constraints = GeoPointConstraints.UniqueID 
                ''Dim pts As GeoPointOpenList = iP.Load(Me.FilePath1)
                ''Dim pts As GeoPointOpenList = KV.Load()
                ''TC.Load("X:\Quellen\DotNet\VisualBasic\Rstyx.Utilities\TestWpf\source\Test1_AKG----D.A0")
                ''Dim pts As GeoPointOpenList = TC.Load("X:\Quellen\DotNet\VisualBasic\Rstyx.Utilities\TestWpf\source\IstGleis_2008_GIC.a0")
                'Try
                '    TC.Load()
                'Catch ex As Exception
                '    Logger.LogError(ex, "Crash...")
                'Finally
                '    'Logger.LogInfo(TC.ToReport(OnlySummary:=False))
                'End Try
                ' 'Dim pts As New GeoPointOpenList(iP.PointStream, iP)
                'Dim pts As New GeoPointOpenList(KV.PointStream, KV)
                ' 
                'Dim iP2 As New iPktFile("D:\Daten\koo\Test-KV_out.ipkt")
                'iP2.Store(pts)
                
                'Dim KV2 As New KvFile("X:\Quellen\DotNet\VisualBasic\Rstyx.Apps\VEedit\Test\Test_out.kv")
                'KV2.Store(pts)
                'KV2.Header = KV.Header
                ' KV2.Store(KV.PointStream, KV)
                'iP.Store(pts, "X:\Quellen\DotNet\VisualBasic\Rstyx.Utilities\TestWpf\source\Test_out.ipkt")
                
                
                'Dim dtUTC As DateTime = DateTime.UtcNow
                'Logger.LogInfo(dtUTC.ToLongDateString())
                'Logger.LogInfo(dtUTC.ToShortDateString())
                'Logger.LogInfo(dtUTC.ToLongTimeString())
                'Logger.LogInfo(dtUTC.ToShortTimeString())
                'Dim dtLocal As DateTime = dtUTC.ToLocalTime()
                'Logger.LogInfo(dtLocal.ToShortTimeString())
                'Logger.LogInfo("")
                '
                ''Dim RefTime As DateTime = DateTime.Parse("01.01.1970 0:00")
                'Dim RefTime As DateTime = DateTime.SpecifyKind(DateTime.Parse("01.01.1970 0:00"), DateTimeKind.Utc)
                'Dim ts As TimeSpan = dtUTC.Subtract(RefTime)
                'Dim sec As Integer = CInt(ts.TotalSeconds)
                'Dim NewTime As DateTime = RefTime.AddSeconds(sec)
                'Logger.LogInfo(RefTime.ToString())
                'Logger.LogInfo(RefTime.ToLocalTime().ToString())
                'Logger.LogInfo("")
                '
                'Logger.LogInfo(ts.TotalSeconds.ToString())
                '
                ''Logger.LogInfo(StringUtils.Sprintf("3 Stellen: '%3s'", "123456789".TrimToMaxLength(3)))
                ''Logger.LogInfo(StringUtils.Sprintf("Nothing: '%6.3f'", Nothing))
                '
                ' 'Dim TestText1 As String = "25082013"
                ' Dim TestDate As DateTime
                ' Dim TestText1 As String = ""
                ' 'Dim TestDate As Nullable(Of DateTime) = Nothing
                ' ''Dim success As Boolean = DateTime.TryParse(TestText1, TestDate)
                ' 'Dim success As Boolean = DateTime.TryParseExact(TestText1, "ddMMyyyy", Nothing, Globalization.DateTimeStyles.None, TestDate)
                ' Dim success As Boolean = DateTime.TryParseExact(TestText1, "s", Nothing, Globalization.DateTimeStyles.None, TestDate)
                ' Logger.LogInfo(TestDate.ToShortDateString())
                ' Logger.LogInfo(TestDate.ToString("s"))
                '
                'Dim TestDouble As Double = 33
                'Double.TryParse(Nothing , TestDouble)
                'Logger.LogInfo(StringUtils.Sprintf("Double = %.3f", TestDouble))
                '
                'Dim p1 As New GeoTcPoint()
                'p1.Q    = -4.000
                'p1.HSOK =  5.000
                'p1.Ueb  =  -0.150
                'p1.Ra   =  -1
                'Dim p2 As New GeoTcPoint()
                'p2.Q    =  4.000
                'p2.HSOK =  5.000
                'p2.Ueb  =  -0.150
                'p2.Ra   =  -1
                'p1.TransformHorizontalToCanted()
                'p2.TransformHorizontalToCanted()
                'Logger.LogInfo(StringUtils.Sprintf("Q=%.3f  HSOK=%.3f  (u=%.3f)  QG=%.3f  HG=%.3f", p1.Q, p1.HSOK, p1.Ueb * sign(p1.Ra), p1.QG, p1.HG))
                'Logger.LogInfo(StringUtils.Sprintf("Q=%.3f  HSOK=%.3f  (u=%.3f)  QG=%.3f  HG=%.3f", p2.Q, p2.HSOK, p2.Ueb * sign(p2.Ra), p2.QG, p2.HG))
                '
                'Dim p3 As New GeoTcPoint()
                'p3.QG   = p1.QG
                'p3.HG   = p1.HG
                'p3.Ueb  = -0.150
                'p3.Ra   = -1
                'Dim p4 As New GeoTcPoint()
                'p4.QG   = p2.QG
                'p4.HG   = p2.HG
                'p4.Ueb  = -0.150
                'p4.Ra   = -1
                'p3.TransformCantedToHorizontal()
                'p4.TransformCantedToHorizontal()
                'Logger.LogInfo(StringUtils.Sprintf("Q=%.3f  HSOK=%.3f  (u=%.3f)  QG=%.3f  HG=%.3f", p3.Q, p3.HSOK, p3.Ueb * sign(p3.Ra), p3.QG, p3.HG))
                'Logger.LogInfo(StringUtils.Sprintf("Q=%.3f  HSOK=%.3f  (u=%.3f)  QG=%.3f  HG=%.3f", p4.Q, p4.HSOK, p4.Ueb * sign(p4.Ra), p4.QG, p4.HG))
                '
                'Dim dou As Double = 24430.0 'Double.NaN
                'Dim i16 As Int16 = 0
                'If (Not Double.IsNaN(dou)) Then i16 = CInt(dou)
                'Logger.LogInfo(i16)
                
                'Dim d1  As Double = 1.2301
                'Dim d2  As Double = 1.23
                'Dim eps As Double = 0.000099999999999
                'Logger.LogInfo(StringUtils.Sprintf("%.4f = %.4f (eps=%.4f): %s", d1, d2, eps, d1.EqualsAlmost(d2, eps)))
                
                'Dim Km As Kilometer = New Kilometer(Me.Textbox)
                'Logger.LogInfo(StringUtils.Sprintf("Km = %8.3f  (Status=%s)  =>  %s", Km.Value, Km.Status.ToDisplayString(), Km.ToKilometerNotation(3)))
                
                'Logger.LogInfo(StringUtils.Sprintf("gültig     = %s", Rstyx.Utilities.IO.FileUtils.IsValidFilePath(Me.Textbox)))
                'Logger.LogInfo(StringUtils.Sprintf("korrigiert = %s", Rstyx.Utilities.IO.FileUtils.ValidateFilePathSpelling(Me.Textbox)))
                'Logger.LogInfo(StringUtils.Sprintf("gültig     = %s", Rstyx.Utilities.IO.FileUtils.IsValidFileName(Me.Textbox)))
                'Logger.LogInfo(StringUtils.Sprintf("korrigiert = %s", Rstyx.Utilities.IO.FileUtils.ValidateFileNameSpelling(Me.Textbox)))
                
                'Dim Path As String = "T:\Debug.log"
                'Dim fdk As New IO.DataTextFileReader(LineStartCommentToken:="*", LineEndCommentToken:="|", SeparateHeader:=True)
                'fdk.Load(Me.FilePath1)
                'Logger.LogInfo(StringUtils.Sprintf("Zeilen gelesen = %d", fdk.TotalLinesCount))
                
                'Dim li  As TrackTitle = TrackTitle.GetDBAGTrackTitle(6265)
                
                'TcReader = New TcFileReader()
                'TcReader.Load(Me.FilePath1)
                'Logger.LogInfo(TcReader.ToReport(OnlySummary:=True))
                'Logger.LogInfo(TcReader.ToString())
                
                'Dim Info As String = Me.Textbox
                'Dim Cant As Double = GeoMath.ParseCant(Info, strict:=False, absolute:=False, editPointInfo:=True)
                'Logger.LogInfo(StringUtils.Sprintf("Überhöhung = %.0f  (Info = '%s')", Cant, Info))
                 
                'Dim li  As GeoMath.DBAGTrackInfo
                'li = GeoMath.GetDBAGTrackTitle(6265)
                'Logger.LogInfo(li.ShortTitle)
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
                '        Logger.LogInfo(StringUtils.Sprintf("Feld '%s' = %.3f", Zahl.Definition.Caption, Zahl.Value))
                '    Else
                '        Errors.Add(Zahl.ParseError)
                '    End If
                '    
                '    If (SplitLine.TryParseField(FieldDefS, Text)) Then
                '        Logger.LogInfo(StringUtils.Sprintf("Feld '%s' = %s", Text.Definition.Caption, Text.Value))
                '    Else
                '        Errors.Add(Text.ParseError)
                '    End If
                '    
                '    Errors.ToLoggingConsole()
                'End If
                
                'Apps.AppUtils.StartEditor(5, "")
                'Logger.LogInfo(Rstyx.Utilities.IO.FileUtils.FilePart.Dir.ToDisplayString())
                'Logger.LogInfo(EnumExtensions.ToDisplayString(Rstyx.Utilities.IO.FileUtils.FilePart.Dir))
                
                'Me.ShowHelpFile()
                
                'Dim success As Boolean = TrySetProperty("Textbo", "teschtttt", New String() {"hh", "teschtttyy"})
                
                'Throw New InvalidDataException("+++++++++++++++++++++++++++")
                
                'Dim fic As IO.FileInfoCollection = IO.FileUtils.FindFile("*.bsh", "G:\Tools\jEdit_51\macros\Aktive_Datei", Nothing, SearchOption.TopDirectoryOnly)
                'Dim fi As FileInfo = IO.FileUtils.FindFile("*.bsh", ";;G:\Tools\jEdit_51\macros\Aktive_Datei;G:\Tools\jEdit_51\macros\Aktive_Datei", ";", Nothing)
                'Logger.LogInfo(fi.FullName)
                'Logger.LogInfo(Me.FilePath1)
                
                'Dim Field = "Symbol_Scale_Width"
                'Dim TableName = "Points$"
                'Dim Workbook = Me.FilePath1
                '
                '' Siehe:  https://www.microsoft.com/de-DE/download/details.aspx?id=13255
                '' Siehe:  https://www.microsoft.com/en-us/download/details.aspx?id=54920&751be11f-ede8-5a0c-058c-2ee190a24fa6=True&e6b34bbe-475b-1abd-2c51-b5034bcdd6d2=True&fa43d42b-25b5-4a42-fe9b-1634f450f5ee=True
                '' ... Hinweise unter "Anweisungen zur Installation"
                '' Siehe: https://www.connectionstrings.com/ace-oledb-12-0/info-and-download/
                '' Provider   = "Microsoft.ACE.OLEDB.12.0",  XLSX => "Excel 12.0 Xml", XLSM =  "Excel 12.0 Macro"
                ''Dim DBconn  As OleDbConnection = Nothing
                ''Dim CSB As OleDbConnectionStringBuilder = New OleDbConnectionStringBuilder()
                ''CSB.DataSource = Workbook
                ''CSB.DataSource = Me.FilePath1
                ''CSB.Provider   = "Microsoft.Jet.OLEDB.4.0"
                ''CSB.Add("Extended Properties", "Excel 8.0;HDR=Yes;IMEX=1;")
                ''CSB.Provider   = "Microsoft.ACE.OLEDB.12.0"
                ''CSB.Add("Extended Properties", "Excel 12.0 Xml;ReadOnly=True")
                ''DBconn = New OleDbConnection(CSB.ConnectionString)
                ''Logger.LogInfo(StringUtils.Sprintf("ConnectionString = '%s'", CSB.ConnectionString))
                ''DBconn.Open()
                ''DBconn.Close()
                '
                'Dim TableName = "Standorte$"
                'Dim Workbook  = "C:\ProgramData\intermetric\sync_Ressourcen\MicroStation\Workspace\standards\I_Tabellen\Standortdaten.xlsx"
                ' Dim Workbook  = "R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xlsx"
                ' 'Using XLconn As System.Data.OleDb.OleDbConnection = DBUtils.ConnectToExcelWorkbook("R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xlsx")
                ' 'Using XLconn As System.Data.OleDb.OleDbConnection = DBUtils.ConnectToExcelWorkbook(Me.FilePath1)
                ' Using Table As System.Data.DataTable = DBUtils.GetExcelSheet(TableName, Workbook)
                ' 'Using Table As System.Data.DataTable = DBconn.GetTable(TableName)
                '     ''Dim Table As DataTable = DBUtils.GetOleDBTable(TableName, XLconn)
                '     'Dim Table As DataTable = XLconn.GetTable(TableName)
                '     'Logger.LogInfo(StringUtils.Sprintf("Feld '%s' existiert = %s", Field, DBconn.TableContainsField(TableName, Field)))
                '     'Logger.LogInfo(StringUtils.Sprintf("Feld '%s' existiert = %s", Field, Table.ContainsField(Field)))
                '     
                '     'Dim SQL = "SELECT * FROM " & TableName
                '     'Dim Table As DataTable = DBUtils.QueryOLEDB(SQL, XLconn)
                '     'Dim Query = From site In Table.AsEnumerable() Where site.Field(Of String)("UserDomain") = "dummy"
                '     
                '     'Dim Table As DataTable = DBUtils.GetExcelSheet(TableName, Workbook)
                '     'Dim yes = Table.ContainsField(Field)
                '     'Logger.LogInfo(StringUtils.Sprintf("Existiert Feld '%s' in Tabelle '%s' = %s", Field, Table.TableName, Table.ContainsField(Field)))
                '     '
                '     For Each row As System.Data.DataRow In Table.AsEnumerable()
                '         Logger.LogInfo(StringUtils.Sprintf("Standort '%s' = UserDomain '%s'", row("Standort_ID"), row("UserDomain")))
                '     Next 
                ' End Using
                
                'UI.ClassEvents.SelectAllOnTextBoxGotFocus = (Not UI.ClassEvents.SelectAllOnTextBoxGotFocus)
                'Logger.LogInfo(StringUtils.Sprintf("gültig     = %s", Rstyx.Utilities.IO.FileUtils.IsValidFilePath(Me.Textbox)))
                'Logger.LogInfo(StringUtils.Sprintf("korrigiert = %s", Rstyx.Utilities.IO.FileUtils.ValidateFilePathSpelling(Me.Textbox)))
                
            'Catch ex As ParseException
            '    'Logger.LogError(ex, StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadFailed, TcReader.FilePath))
            '    'TcReader.ParseErrors.ShowInJEdit()
            '    Logger.LogError(ex, "==> Fehler...")
            '    
            'Catch ex As System.Exception
            '    Logger.LogError(ex, StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
            '    'Throw New RemarkException("test_1(): Unerwarteter Fehler.", ex)
            'End Try
        End Sub
        
        Protected Overrides Sub ShowHelpFile()
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
                Logger.LogError(ex, StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
            End Try
            Return RetValue
        End Function
        
        Public Sub Test_0()
            'Dim s As String
            'Dim sa() As String
            'Dim d As Nullable(Of Double)
            ''Dim bool As Boolean
            '
            Dim TestEnum As Cinch.CustomDialogIcons = Cinch.CustomDialogIcons.Question
            Logger.LogInfo(StringUtils.Sprintf("Enum Value=%s:  Display=%s", TestEnum.ToString(), TestEnum.ToDisplayString()))
            Me.StatusText = TestEnum.ToDisplayString()
            
            System.Threading.Thread.Sleep(3000)
            
            Dim TestEnum2 As ArrayUtils.SortType = ArrayUtils.SortType.Numeric
            Logger.LogInfo(StringUtils.Sprintf("Enum Value=%s:  Display=%s", TestEnum2.ToString(), TestEnum2.ToDisplayString()))
            Me.StatusText = TestEnum2.ToDisplayString()
            
            's = "klop-123.4+8y9u=+987#äö"
            'd = 123456789.789
            'sa = s.Split("\s+")
            
            'Logger.LogInfo(GeoMath.Hex2Dec("2fde"))
            'd = GeoMath.GetKilometer(a)
            
            'GeoMath.GetCantFromPointinfo(s, d, False )
            Dim li  As TrackTitle = TrackTitle.GetDBAGTrackTitle(6265)
            Logger.LogInfo(li.ShortTitle)
            'Logger.LogInfo(Strings.FileSpec2RegExp("X:\Quellen\DotNet\VisualBasic\_Backup\Rstyx.Utilities_2012-??-14.*"))
            
            'Logger.LogInfo(s.Left("y"c))
            'Logger.LogInfo("\sc\rf\\\#\\\".RemoveTrailingBackslashes())
            'Logger.LogInfo("\sc\rf\\\#\\\".TrimEnd("\"))
            'Logger.LogInfo(Strings.GetFilePart("D:\daten\Test_QP\qp_ueb.dgn", Strings.FilePart.Dir_Proj   ))
            'Logger.LogInfo(System.IO.File.Exists("D:\daten\Test_QP\"))
            
            'dim keyName      as string = "HKEY_CURRENT_USER\Software\VB and VBA Program Settings\Microstation\frmBatchPlot\"
            ''dim valueName    as string = "Dialog_Left"
            'dim valueName    as string = "test"
            'dim valueString  as string = "1234"
            'dim valueObj     as Object = "123456"
            'Logger.LogInfo(RegUtils.ValueExists(key))
            'Logger.LogInfo(RegUtils.GetValue(Of String)(keyName & valueName))
            'Logger.LogInfo("Pfad = '" & RegUtils.GetApplicationPath("HKEY_CLASSES_ROOT\i_M5_file\shell\i_zzHilfe\ShellCommand\") & "'")
            
            
            '
            'Dim Folders  As String = Environment.ExpandEnvironmentVariables("%PATH%")
            'Dim Folders  As String = Environment.GetEnvironmentVariable("PROGRAMFILES")
            'Dim Folders  As String = "T:\"
            'Dim File     As String = " *"
            'Dim File     As String = "cedt.exe"
            'Dim File     As String = "uedit*.exe;ultaedit*.exe"
            ''Logger.LogInfo(Folders.IndexOfAny(System.IO.Path.GetInvalidPathChars()))
            ''
            'Dim found  As System.Collections.ObjectModel.Collection(Of System.IO.fileinfo) = IO.FileUtils.FindFiles(File, Folders, ";", IO.SearchOption.AllDirectories)
            '
            'If (found.IsNotNull) Then 
            '    For Each fi In found 
            '        Logger.LogInfo(fi.FullName)
            '    Next
            '    Logger.LogInfo(found.Count)
            'End If
            'Logger.LogInfo(StringUtils.Sprintf(" %d Dateien gefunden", found.Count))
            
            'Logger.LogInfo(Environment.GetEnvironmentVariable("jedit_home"))
            'Logger.LogInfo(AppUtils.AppPathJava)
            
            'AppUtils.StartEditor(AppUtils.SupportedEditors.jEdit , """T:\_test\12 3 Ö .tx t"" +line:44,11")
            'Logger.LogInfo(AppUtils.StartFile("T:\_test\12 3 Ö .txt"))
            'AppUtils.StartEditor(AppUtils.SupportedEditors.UltraEdit  , """T:\_test\12 3 Ö .txt"" +line:44,11")
            
            'AppUtils.CurrentEditor = AppUtils.SupportedEditors.CrimsonEditor   
            'AppUtils.StartEditor(AppUtils.CurrentEditor, """T:\_test\12 3 Ö .txt""")
            
            'Dim IconDictionary  As ResourceDictionary = UI.Resources.UIResources.Icons
            '
            'Logger.LogInfo("Microsoft.VisualBasic.FileIO.FileSystem.CurrentDirectory = " & Microsoft.VisualBasic.FileIO.FileSystem.CurrentDirectory)
            'Logger.LogInfo("System.Environment.CurrentDirectory = " & System.Environment.CurrentDirectory())
            'Logger.LogInfo("System.IO.Directory.GetCurrentDirectory() = " & System.IO.Directory.GetCurrentDirectory())
            '
            'Logger.LogInfo("EditorCombo2.ActualHeight = " & EditorCombo2.ActualHeight)
            '
            'FileChoser1.InputFilePath = "rrrrrrrrrr"
            
            'Dim bytes() As Byte = {111, 112, 113, 00, 00, 00}
            
            ' Statt ByteArray2String() - Nullwerte am Ende des Arrays sind egal...
            'Dim text As String  = System.Text.Encoding.Default.GetString(bytes)
            'Dim bytes() As Byte = System.Text.Encoding.Default.GetBytes("str")
            
            'Dim Field = "UserDomain"
            'Dim TableName = "Standorte$"
            'Dim Workbook = "R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xls"
            'Dim Workbook  = "C:\ProgramData\intermetric\sync_Ressourcen\MicroStation\Workspace\standards\I_Tabellen\Standortdaten.xlsx"
            '
            'Dim XLconn As OleDbConnection = DBUtils.ConnectToExcelWorkbook("R:\Microstation\Workspace\Standards\I_Tabellen\Standortdaten.xls")
            ''Dim Table As DataTable = DBUtils.GetOleDBTable(TableName, XLconn)
            'Dim Table As DataTable = XLconn.GetTable(TableName)
            'Logger.LogInfo(StringUtils.Sprintf("Feld '%s' existiert = %s", Field, XLconn.TableContainsField(TableName, Field)))
            'Logger.LogInfo(StringUtils.Sprintf("Feld '%s' existiert = %s", Field, Table.ContainsField(Field)))
            
            'Dim SQL = "SELECT * FROM " & TableName
            'Dim Table As DataTable = DBUtils.QueryOLEDB(SQL, XLconn)
            'Dim Query = From site In Table.AsEnumerable() Where site.Field(Of String)("UserDomain") = "dummy"
            
            'Dim Table As DataTable = DBUtils.GetExcelSheet(TableName, Workbook)
            'Dim yes = DBExtensions.ContainsField(Table, Field)
            'Dim yes = Table.ContainsField(Field)
            
            'For Each row As DataRow In Table.AsEnumerable()
            '    Logger.LogInfo(StringUtils.Sprintf("Standort '%s' = UserDomain '%s'", row("Standort_ID"), row("UserDomain")))
            'Next 
            
            'TestPanel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity))
            
        End Sub
        
    #End Region

End Class

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
