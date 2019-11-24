
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Math
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions

Imports Rstyx.Utilities
Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.Math.MathExtensions
Imports Rstyx.Utilities.StringUtils
Imports Rstyx.Utilities.Validation

Namespace Domain.IO
    
    ''' <summary>  Represents a reader that can read and cache points with track geometry coordinates (TC) from a text file. </summary>
     ''' <remarks>
     ''' <list type="bullet">
     ''' <listheader> <b>General Features :</b> </listheader>
     ''' <item> The <see cref="TcFileReader.Load"/> method clears all results before loading the given file. </item>
     ''' <item> File header lines will be provided by the <see cref="TcFileReader.Header"/> property. </item>
     ''' <item> Found TC blocks will be provided by the <see cref="TcFileReader.Blocks"/> property. It contains all points that has been read without errors. </item>
     ''' <item> Parsing errors will be catched and provided by the <see cref="TcFileReader.ParseErrors"/> property. </item>
     ''' </list>
     ''' <para>
     ''' The track geometry coordinates will be expected as output blocks of following programs and types:
     ''' </para>
     ''' <list type="table">
     ''' <listheader> <term> <b>Program</b> </term>  <description> Output Type </description></listheader>
     ''' <item> <term> Verm.esn (Version 6.22) </term>  <description> "Umformung", from THW or D3 module (one-line or two-line records) </description></item>
     ''' <item> <term> Verm.esn (Version 8.40) </term>  <description> "Umformung", from THW or D3 module (one-line or two-line records) </description></item>
     ''' <item> <term> iTrassePC (Version 2.0) </term>  <description> "A1" </description></item>
     ''' <item> <term> iGeo (Version 1.3)      </term>  <description> "A0", "A1" </description></item>
     ''' </list>
     ''' <list type="bullet">
     ''' <listheader> <b>Restrictions to the input file :</b> </listheader>
     ''' <item> All supported block types (and only such!) may appear mixed in one single file in arbitrary order. </item>
     ''' <item> All comment lines, immediately before the programs original output, will be treated as block comment/info. </item>
     ''' <item> Comment lines at file start, that has not been associated to a block, will be treated as file header. </item>
     ''' <item> Other comment lines or empty lines will be ignored. </item>
     ''' <item> Every block ends one line before the next block starts. The last block ends at file end. </item>
     ''' </list>
     ''' </remarks>
    Public Class TcFileReader
        Inherits GeoPointFile
        
        #Region "Private Fields"
            
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.TCFileReader")
            
            Private SourceBlocks                As New Collection(Of TcSourceBlockInfo)
            Private VermEsnRecordDefinitions    As New Dictionary(Of String, TcRecordDefinitionVermEsn)  ' Key = getKeyForRecordDefinition(TcBlockType)
            Private iGeoRecordDefinitions       As New Dictionary(Of String, TcRecordDefinitionIGeo)     ' Key = getKeyForRecordDefinition(TcBlockType)
            
            Private Const RHO                   As Double = 200 / System.Math.PI
            'Private Const DoublePipeMask        As String = "D1o2u3b4l5e6P7i8p9e0"
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new instance of TCFileReader with default settings. </summary>
            Public Sub New()
                ' Don't recognize a header because the general logic doesn't match to TC file.
                Me.SeparateHeader = False
                
                ' Commentlines are o.k. but line end comments are not.
                Me.LineStartCommentToken = "#"
                
                ' By default recognize rails points and parse actual cant.
                Me.EditOptions = GeoPointEditOptions.ParseCantFromInfo
                
                setRecordDefinitionsVermEsn()
                setRecordDefinitionsIGeo()
            End Sub
            
            ''' <summary> Creates a new instance of TCFileReader with a given file path. </summary>
             ''' <param name="FilePath"> The file path of the TC file to be read. May be <see langword="null"/>. </param>
            Public Sub New(FilePath As String)
                Me.New()
                Me.FilePath = FilePath
            End Sub
            
            ''' <summary> Creates a new instance of TCFileReader with specified settings. </summary>
             ''' <param name="FilePath">           The file path of the TC file to be read. May be <see langword="null"/>. </param>
             ''' <param name="CantBase">           The base length for cant. Must be greater than zero. </param>
             ''' <param name="StationAsKilometer"> If <see langword="true"/> the station value may be used also as kilometer. </param>
             ''' <exception cref="System.ArgumentException"> <paramref name="CantBase"/> isn't > 0. </exception>
            Public Sub New(FilePath As String, CantBase As Double, StationAsKilometer As Boolean)
                Me.New(FilePath)
                If (Not (CantBase > 0)) Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.Global_ValueNotGreaterThanZero, "CantBase")
                
                Me.CantBase = CantBase
                Me.StationAsKilometer = StationAsKilometer
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            #Region "Settings"
                
                ''' <summary> The distance between rails for determining cant (used for calculations). Defaults to 1.500. </summary>
                Public Property CantBase() As Double = 1.500
                
                ''' <summary> If <see langword="true"/> and no kilometer value is found, the station value will be used also as kilometer. Defaults to <see langword="true"/>. </summary>
                Public Property StationAsKilometer() As Boolean = True
                
            #End Region
            
            #Region "Results"
                
                ''' <summary> Returns the list of found TC blocks with points. </summary>
                 ''' <remarks>
                 ''' This property never returns <see langword="null"/>.
                 ''' This collection is empty until <see cref="TcFileReader.Load"/> has been invoked successfully
                 ''' and it will be cleared by <see cref="TcFileReader.Reset"/>.
                 ''' </remarks>
                Public ReadOnly Property Blocks() As New Collection(Of TcBlock)
                
            #End Region
            
            #Region "Statistics"
                
                ''' <summary> Returns the total count of read points (in all blocks). </summary>
                Public ReadOnly Property TotalPointCount() As Long
                    Get
                        Dim Total As Long
                        For Each Block As TcBlock In Me.Blocks
                            Total += Block.Points.Count
                        Next
                        Return Total
                    End Get
                End Property
                
            #End Region
            
        #End Region
        
        #Region "Public Members"
            
            ''' <summary> Loads the whole file into <see cref="TcFileReader.Blocks"/>. </summary>
             ''' <remarks>
             ''' The loaded data will be provided structured by the <see cref="TcFileReader.Blocks"/> and <see cref="TcFileReader.Header"/>
             ''' properties, which are cleared before. Same for <see cref="TcFileReader.ParseErrors"/>.
             ''' </remarks>
             ''' <exception cref="ParseException"> At least one error occurred while parsing, hence <see cref="TcFileReader.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Sub Load()
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadStart, Me.FilePath))
                    Logger.logInfo(Me.GetPointEditOptionsLogText())
                    
                    Dim DataLine        As DataTextLine
                    Dim kvp             As KeyValuePair(Of String, String)
                    Dim SourceBlock     As TcSourceBlockInfo
                    Dim TcBlock         As TcBlock
                    Dim BlockCount      As Integer = 0
                    
                    MyBase.Load()
                    
                    Logger.logDebug(" Looking for block beginnings...")
                    ' Read file. Find and store source block beginnings (still without EndIndex, Version and Format).
                    For i As Integer = 0 To Me.DataLineBuffer.Count - 1
                        
                        DataLine = Me.DataLineBuffer(i)
                        
                        If (DataLine.IsCommentLine) Then
                            ' Comment line: Look for output header of iGeo and iTrassePC.
                            If (DataLine.HasComment) Then
                                kvp = splitHeaderLineIGeo(DataLine.Comment)
                                If (kvp.Key IsNot Nothing) Then
                                    If ((kvp.Key = "Programm") AndAlso ((kvp.Value = "iTrassePC") Or (kvp.Value = "iGeo"))) Then
                                        ' Create and store source block info.
                                        SourceBlock = New TcSourceBlockInfo()
                                        SourceBlock.BlockType.Version = TcBlockVersion.Current
                                        SourceBlock.BlockType.Program = If((kvp.Value = "iGeo"), TcBlockProgram.iGeo, TcBlockProgram.iTrassePC)
                                        SourceBlock.StartIndex = findStartOfBlock(Me.DataLineBuffer, i)
                                        SourceBlocks.Add(SourceBlock)
                                        BlockCount += 1
                                        Logger.logDebug(sprintf("  Found %d. block: from %s, start index=%d, indicated at index=%d", BlockCount, SourceBlock.BlockType.Program.ToDisplayString(), SourceBlock.StartIndex, i))
                                    End If
                                End If
                            End If
                        ElseIf (DataLine.HasData) Then
                            ' Line contains data and maybe line end comment.
                            If (DataLine.Data.IsMatchingTo("^Trassenumformung\s+|^Umformung\s+")) Then
                                SourceBlock = New TcSourceBlockInfo()
                                SourceBlock.BlockType.Program = TcBlockProgram.VermEsn
                                SourceBlock.StartIndex = findStartOfBlock(Me.DataLineBuffer, i)
                                SourceBlocks.Add(SourceBlock)
                                BlockCount += 1
                                Logger.logDebug(sprintf("  Found %d. block: from %s, start index=%d, indicated at index=%d", BlockCount, SourceBlock.BlockType.Program.ToDisplayString(), SourceBlock.StartIndex, i))
                            End If
                        End If
                    Next
                    
                    ' Store block end lines (the line before the next block start).
                    For i As Integer = 0 To SourceBlocks.Count - 1
                        If (i = (SourceBlocks.Count - 1)) Then
                            SourceBlocks(i).EndIndex = Me.DataLineBuffer.Count - 1
                        Else
                            SourceBlocks(i).EndIndex = SourceBlocks(i + 1).StartIndex - 1
                        End If
                    Next
                    
                    Logger.logDebug(" Looking for file header...")
                    ' Find and store file header lines (all comment lines from file start until the first non comment line or block start).
                    Dim MaxIndexToLook As Integer = If((SourceBlocks.Count > 0), SourceBlocks(0).StartIndex - 1, Me.DataLineBuffer.Count - 1)
                    For i As Integer = 0 To MaxIndexToLook
                        If (Not Me.DataLineBuffer(i).IsCommentLine) Then
                            Exit For
                        Else
                            Me.Header.Add(Me.DataLineBuffer(i).Comment)
                        End If
                    Next
                    
                    Logger.logDebug(" Read blocks...")
                    ' Read every block with all it's data and, if successfull, store it to Me.Blocks.
                    For i As Integer = 0 To SourceBlocks.Count - 1
                        
                        Select Case SourceBlocks(i).BlockType.Program
                            
                            Case TcBlockProgram.VermEsn
                                TcBlock = readBlockVermEsn(Me.DataLineBuffer, SourceBlocks(i))
                                If (TcBlock.IsValid) Then Me.Blocks.Add(TcBlock)
                                
                            Case TcBlockProgram.iGeo, TcBlockProgram.iTrassePC
                                TcBlock = readBlockIGeo(Me.DataLineBuffer, SourceBlocks(i))
                                If (TcBlock.IsValid) Then Me.Blocks.Add(TcBlock)
                        End Select
                    Next
                    
                    Logger.logDebug(Me.ToReport(OnlySummary:=False))
                    
                    ' Throw exception if parsing errors has occurred.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                    ElseIf (Me.TotalPointCount = 0) Then
                        Logger.logWarning(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_NoPoints, Me.FilePath))
                    End If
                    
                    'Logger.logDebug(Me.ToReport(OnlySummary:=False))
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadSuccess, Me.TotalPointCount, Me.FilePath))
                    
                    Me.DataLineBuffer.Clear()
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadFailed, Me.FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
            ''' <summary> Resets this <see cref="TcFileReader"/>. </summary>
            Public Overloads Sub Reset()
                Me.Reset(Nothing)
            End Sub
            
            ''' <summary> Creates a report of all blocks of this TcFileReader.</summary>
             ''' <param name="OnlySummary"> If <see langword="true"/>, the point lists won't be reported. </param>
             ''' <returns> The report of all blocks of this TcFileReader.</returns>
            Public Function ToReport(OnlySummary As Boolean) As String
                Dim List As New StringBuilder()
                
                List.AppendLine()
                List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToReport_File  , Me.FilePath))
                List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToReport_Blocks, Me.Blocks.Count))
                List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToReport_Points, Me.TotalPointCount))
                List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToReport_Errors, Me.ParseErrors.Count))
                
                For i As Integer = 0 To Me.Blocks.Count - 1
                    List.AppendLine()
                    List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToReport_BlockNo, i + 1))
                    List.AppendLine(Me.Blocks(i).ToReport(OnlySummary))
                Next
                
                Return List.ToString()
            End Function
            
        #End Region
        
        #Region "GeoPointFile Overrides Stub"
            
            ''' <summary> Reads the point file and provides the points as (not lazy established) enumerable. </summary>
             ''' <returns> All read points of all <see cref="TcFileReader.Blocks"/> as flat list. See <b>remarks</b> for details. </returns>
             ''' <remarks>
             ''' This enumeration will be empty until <see cref="TcFileReader.Load"/> has been invoked successfully.
             ''' <para>
             ''' This method is for convenience only - see <see cref="TcFileReader.Load"/>.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public ReadOnly Overrides Iterator Property PointStream() As IEnumerable(Of IGeoPoint)
                Get
                    For Each Block As TcBlock In Me.Blocks
                        For Each p As GeoTcPoint In Block.Points
                            Yield p
                        Next
                    Next
                End Get
            End Property
            
            ''' <summary> NOT Implemented: Would write the points collection to a file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <param name="MetaData">  An object providing the header for <paramref name="PointList"/>. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="PointList"/> is a <see cref="GeoPointList"/> then
             ''' it's ensured that the point ID's written to the file are unique.
             ''' Otherwise point ID's may be not unique.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any exception. </exception>
            Public Overrides Sub Store(PointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
                Throw New NotImplementedException()
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Creates a summary of file contents. </summary>
             ''' <returns> Summary of all blocks of this TcFileReader.</returns>
            Public Overrides Function ToString() As String
                Return sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToString, Me.FilePath, Me.Blocks.Count, Me.TotalPointCount, Me.ParseErrors.Count)
            End Function
            
        #End Region
        
        #Region "Nested Data Structures"
            
            ''' <summary> Program that is the origin of a track geometry coordinates block. </summary>
            Public Enum TcBlockProgram As Integer
                
                ''' <summary> Not defined. </summary>
                None = 0
                
                ''' <summary> Output of Verm.esn. </summary>
                VermEsn = 1
                
                ''' <summary> Output of iTrassePC. </summary>
                iTrassePC = 2
                
                ''' <summary> Output of iGeo. </summary>
                iGeo = 3
                
            End Enum
            
            ''' <summary> Version of the Program that is the origin of a track geometry coordinates block. </summary>
            Public Enum TcBlockVersion As Integer
                
                ''' <summary> Not defined. </summary>
                None = 0
                
                ''' <summary> The currently supported program version. </summary>
                Current = 1
                
                ''' <summary> The last outdated supported program version. </summary>
                Outdated = 2
                
            End Enum
            
            ''' <summary> Format of a track geometry coordinates block. </summary>
            Public Enum TcBlockFormat As Integer
                
                ''' <summary> Not defined. </summary>
                None = 0
                
                ''' <summary> Output format "A0" of iGeo. </summary>
                A0  = 1
                
                ''' <summary> Output format "A1" of iGeo or iTrassePC. </summary>
                A1  = 2
                
                ''' <summary> Output of Verm.esn module "THW". </summary>
                THW = 3
                
                ''' <summary> Output of Verm.esn module "D3". </summary>
                D3  = 4
                
            End Enum
            
            ''' <summary> Sub format of a track geometry coordinates block. </summary>
            Public Enum TcBlockSubFormat As Integer
                
                ''' <summary> Not defined. </summary>
                None = 0
                
                ''' <summary> One line record (Verm.esn). </summary>
                OneLine = 1
                
                ''' <summary> Two line record (Verm.esn). </summary>
                TwoLine = 2
                
                ''' <summary> Variable fields, character separated (iGeo A0). </summary>
                CSV = 3
                
            End Enum
            
            ''' <summary> The type description of a block of points with track geometry coordinates. </summary>
            Public Class TcBlockType
                Inherits Cinch.ValidatingObject
                
                #Region "Private Fields"
                    
                    'Private Shared Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Microstation.AddIn.MetaData.TextFieldDefinition")
                    
                    Private Shared MissingProgramRule       As Cinch.SimpleRule
                    Private Shared MissingVersionRule       As Cinch.SimpleRule
                    Private Shared MissingFieldNamesRule    As Cinch.SimpleRule
                    Private Shared MissingFormatRule        As DelegateRule
                    Private Shared MissingSubFormatRule     As Cinch.SimpleRule
                    Private Shared FormatMismatchRule       As DelegateRule
                    
                #End Region
                
                #Region "Constuctors"
                    
                    ''' <summary> Static initializations. </summary>
                    Shared Sub New()
                        ' Create Validation Rules.
                        
                        MissingProgramRule = New Cinch.SimpleRule("Program",
                                                                  Rstyx.Utilities.Resources.Messages.TcBlockType_MissingProgram,
                                                                  Function (oValidatingObject As Object) (DirectCast(oValidatingObject, TcBlockType).Program = TcBlockProgram.None))
                        '
                        MissingVersionRule = New Cinch.SimpleRule("Version",
                                                                  Rstyx.Utilities.Resources.Messages.TcBlockType_MissingVersion,
                                                                  Function (oValidatingObject As Object) As Boolean
                                                                      Dim IsValidRule  As Boolean = True
                                                                      Dim BlockType    As TcBlockType = DirectCast(oValidatingObject, TcBlockType)
                                                                      
                                                                      If (BlockType.Program = TcBlockProgram.VermEsn) Then
                                                                          If (BlockType.Version = TcBlockVersion.None) Then IsValidRule = False
                                                                      End If
                                                                      
                                                                      Return Not IsValidRule
                                                                  End Function
                                                                 )
                        
                        '
                        MissingFieldNamesRule = New Cinch.SimpleRule("FieldNames",
                                                                  Rstyx.Utilities.Resources.Messages.TcBlockType_MissingFieldNames,
                                                                  Function (oValidatingObject As Object) As Boolean
                                                                      Dim IsValidRule  As Boolean = True
                                                                      Dim BlockType    As TcBlockType = DirectCast(oValidatingObject, TcBlockType)
                                                                      
                                                                      If (BlockType.SubFormat = TcBlockSubFormat.CSV) Then
                                                                          If (BlockType.FieldNames.IsEmptyOrWhiteSpace()) Then IsValidRule = False
                                                                      End If
                                                                      
                                                                      Return Not IsValidRule
                                                                  End Function
                                                                 )
                        
                        '
                        MissingFormatRule = New DelegateRule("Format",
                                                             Rstyx.Utilities.Resources.Messages.TcBlockType_MissingFormat,
                                                             Function (oValidatingObject As Object, ByRef BrokenMessage As String) As Boolean
                                                                 Dim IsValidRule As Boolean = True
                                                                 Dim BlockType   As TcBlockType = DirectCast(oValidatingObject, TcBlockType)
                                                                 
                                                                 If (BlockType.Format = TcBlockFormat.None) Then
                                                                     IsValidRule = False
                                                                      Select Case BlockType.Program
                                                                          
                                                                          Case TcBlockProgram.VermEsn
                                                                              BrokenMessage = sprintf(Rstyx.Utilities.Resources.Messages.TcBlockType_MissingFormat, BlockType.Program.ToDisplayString(), TcBlockFormat.THW.ToDisplayString(), TcBlockFormat.D3.ToDisplayString())
                                                                              
                                                                          Case TcBlockProgram.iGeo, TcBlockProgram.iTrassePC
                                                                              BrokenMessage = sprintf(Rstyx.Utilities.Resources.Messages.TcBlockType_MissingFormat, BlockType.Program.ToDisplayString(), TcBlockFormat.A0.ToDisplayString(), TcBlockFormat.A1.ToDisplayString())
                                                                      End Select
                                                                 End If
                                                                 
                                                                 Return Not IsValidRule
                                                             End Function
                                                            )
                        '
                        MissingSubFormatRule = New Cinch.SimpleRule("SubFormat",
                                                                    Rstyx.Utilities.Resources.Messages.TcBlockType_MissingSubFormat,
                                                                    Function (oValidatingObject As Object) As Boolean
                                                                        Dim IsValidRule As Boolean = True
                                                                        Dim BlockType   As TcBlockType = DirectCast(oValidatingObject, TcBlockType)
                                                                        
                                                                        If (BlockType.Program = TcBlockProgram.VermEsn) Then
                                                                            If (BlockType.SubFormat = TcBlockSubFormat.None) Then IsValidRule = False
                                                                        End If
                                                                        
                                                                        Return Not IsValidRule
                                                                    End Function
                                                                   )
                        '
                        FormatMismatchRule = New DelegateRule("Format",
                                                              Rstyx.Utilities.Resources.Messages.TcBlockType_FormatMismatch,
                                                              Function (oValidatingObject As Object, ByRef BrokenMessage As String) As Boolean
                                                                  Dim IsValidRule As Boolean = True
                                                                  Dim BlockType   As TcBlockType = DirectCast(oValidatingObject, TcBlockType)
                                                                  
                                                                  If (Not (BlockType.Format = TcBlockFormat.None)) Then
                                                                      
                                                                      Select Case BlockType.Program
                                                                          
                                                                          Case TcBlockProgram.VermEsn
                                                                              Select Case BlockType.Format
                                                                                  Case TcBlockFormat.D3, TcBlockFormat.THW
                                                                                      'o.k.
                                                                                  Case Else
                                                                                      IsValidRule = False
                                                                                      BrokenMessage = sprintf(Rstyx.Utilities.Resources.Messages.TcBlockType_FormatMismatch, BlockType.Format.ToDisplayString(), BlockType.Program.ToDisplayString())
                                                                              End Select
                                                                              
                                                                          Case TcBlockProgram.iGeo, TcBlockProgram.iTrassePC
                                                                              Select Case BlockType.Format
                                                                                  Case TcBlockFormat.A0, TcBlockFormat.A1
                                                                                      'o.k.
                                                                                  Case Else
                                                                                      IsValidRule = False
                                                                                      BrokenMessage = sprintf(Rstyx.Utilities.Resources.Messages.TcBlockType_FormatMismatch, BlockType.Format.ToDisplayString(), BlockType.Program.ToDisplayString())
                                                                              End Select
                                                                      End Select
                                                                  End If
                                                                  
                                                                  Return Not IsValidRule
                                                              End Function
                                                             )
                                                              
                        '
                    End Sub
                    
                    ''' <summary> Creates a new, empty TcBlockType instance. </summary>
                    Public Sub New()
                        Me.AddRule(MissingProgramRule)
                        Me.AddRule(MissingVersionRule)
                        Me.AddRule(MissingFormatRule)
                        Me.AddRule(MissingSubFormatRule)
                        Me.AddRule(FormatMismatchRule)
                    End Sub
                    
                #End Region
                
                ''' <inheritdoc cref="TcBlockProgram" />
                Public Property Program()   As TcBlockProgram = TcBlockProgram.None
                
                ''' <inheritdoc cref="TcBlockVersion" />
                Public Property Version()   As TcBlockVersion = TcBlockVersion.None
                
                ''' <inheritdoc cref="TcBlockFormat" />
                Public Property Format()    As TcBlockFormat = TcBlockFormat.None
                
                ''' <inheritdoc cref="TcBlockSubFormat" />
                Public Property SubFormat() As TcBlockSubFormat = TcBlockSubFormat.None
                
                '''<summary> If <see cref="TcFileReader.TcBlockType.SubFormat"/> is <c>CSV</c>: The ordered list of field names. </summary>
                Public Property FieldNames() As String = Nothing
                
                ''' <inheritdoc/>
                Public Overrides Function ToString() As String
                    Dim RetValue As String = String.Empty
                    
                    If (Me.Program = TcBlockProgram.iTrassePC) Then
                        RetValue = "iTrassePC"
                        
                        If (Me.Version = TcBlockVersion.Current) Then
                            RetValue &= " 2.20"
                        Else
                            RetValue &= " v??" 
                        End If
                        
                        If (Me.Format = TcBlockFormat.A1) Then
                            RetValue &= " (Format A1)"
                        Else
                            RetValue &= " (Format ??)"
                        End If
                        
                    ElseIf (Me.Program = TcBlockProgram.iGeo) Then
                        RetValue = "iGeo"
                        
                        If (Me.Version = TcBlockVersion.Current) Then
                            RetValue &= " 1.3"
                        Else
                            RetValue &= " v??" 
                        End If
                        
                        If (Me.Format = TcBlockFormat.A0) Then
                            RetValue &= " (Format A0)"
                        ElseIf(Me.Format = TcBlockFormat.A1) Then
                            RetValue &= " (Format A1)"
                        Else
                            RetValue &= " (Format ??)"
                        End If
                        
                    ElseIf (Me.Program = TcBlockProgram.VermEsn) Then
                        RetValue = "Verm.esn"
                        
                        If (Me.Version = TcBlockVersion.Current) Then
                            RetValue &= " 8.40"
                        ElseIf(Me.Version = TcBlockVersion.Outdated) Then
                            RetValue &= " 6.22"
                        Else
                            RetValue &= " v??" 
                        End If
                        
                        If (Me.Format = TcBlockFormat.D3) Then
                            RetValue &= " (Format D3 /"
                        ElseIf (Me.Format = TcBlockFormat.THW) Then
                            RetValue &= " (Format THW /"
                        Else
                            RetValue &= " (Format ?? /"
                        End If
                        
                        If (Me.SubFormat = TcBlockSubFormat.OneLine) Then
                            RetValue &= " einzeilig)"
                        ElseIf (Me.SubFormat = TcBlockSubFormat.TwoLine) Then
                            RetValue &= " zweizeilig)"
                        Else
                            RetValue &= " ??-zeilig)"
                        End If
                    Else
                        RetValue = "unbekannt"
                    End If
                    
                    Return RetValue
                End Function
                
            End Class
            
            ''' <summary> A block of points with track geometry coordinates. </summary>
            Public Class TcBlock
                Inherits Cinch.ValidatingObject
                
                #Region "Private Fields"
                    
                    Private Shared InvalidBlockTypeRule As DelegateRule
                    Private Shared InvalidTrackRefRule  As DelegateRule
                    
                #End Region
                
                #Region "Constuctors"
                    
                    ''' <summary> Static initializations. </summary>
                    Shared Sub New()
                        ' Create Validation Rules.
                        
                        InvalidBlockTypeRule = New DelegateRule("BlockType",
                                                                Rstyx.Utilities.Resources.Messages.TcBlock_InvalidBlockType_Fallback,
                                                                Function (oValidatingObject As Object, ByRef BrokenMessage As String) As Boolean
                                                                    Dim IsBroken  As Boolean = False
                                                                    Dim BlockType As TcBlockType = DirectCast(oValidatingObject, TcBlock).BlockType
                                                                    
                                                                    If (Not BlockType.IsValid()) Then
                                                                        IsBroken = True
                                                                        BrokenMessage = BlockType.Error
                                                                    End If
                                                                    
                                                                    Return IsBroken
                                                                End Function
                                                               )
                        '
                        InvalidTrackRefRule = New DelegateRule("TrackRef",
                                                               Rstyx.Utilities.Resources.Messages.TcBlock_InvalidTrackRef_Fallback,
                                                               Function (oValidatingObject As Object, ByRef BrokenMessage As String) As Boolean
                                                                   Dim IsBroken As Boolean = False
                                                                   Dim TrackRef As TrackGeometryInfo = DirectCast(oValidatingObject, TcBlock).TrackRef
                                                                   
                                                                   If (Not TrackRef.IsValid()) Then
                                                                       IsBroken = True
                                                                       BrokenMessage = TrackRef.Error
                                                                   End If
                                                                   
                                                                   Return IsBroken
                                                               End Function
                                                              )
                        '
                    End Sub
                    
                    ''' <summary> Creates a new, empty TcBlock instance. </summary>
                    Public Sub New()
                        Me.AddRule(InvalidBlockTypeRule)
                        Me.AddRule(InvalidTrackRefRule)
                    End Sub
                    
                #End Region
                
                #Region "Properties"
                    
                    ''' <summary> Type of TC block determining it's origin and format. </summary>
                    Public Property BlockType()         As New TcBlockType()
                    
                    ''' <summary> A comment for this block. </summary>
                    Public Property Comment()           As String = String.Empty
                    
                    ''' <summary> The point list of this block. </summary>
                    Public Property Points()            As New GeoPointOpenList
                    
                    ''' <summary> Determines the reference frame of the track geometry. </summary>
                    Public Property TrackRef()          As New TrackGeometryInfo()
                    
                    ''' <summary> Determines the file source. </summary>
                    Public Property Source()            As New DataBlockFileSourceInfo()
                    
                #End Region
                
                #Region "Public Methods"
                    
                    ''' <summary> Creates a report of this TcBlock. </summary>
                     ''' <param name="OnlySummary"> If <see langword="true"/>, the point list won't be reported. </param>
                     ''' <returns> The report of this TcBlock. </returns>
                    Public Function ToReport(OnlySummary As Boolean) As String
                        Dim List           As New StringBuilder()
                        Dim IndentComment  As Integer = Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_Comment.IndexOf("%s")
                        Dim IndentTrackRef As Integer = Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_TrackRef.IndexOf("%s")
                        
                        If (Me.Comment.IsNotEmptyOrWhiteSpace()) Then
                            List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_Comment, Me.Comment.indent(IndentComment, IncludeFirstline:=False, PrependNewlineIfMultiline:=False)))
                        End If
                        List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_Type, Me.BlockType.ToString()))
                        List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_Source, Me.Source.ToString()))
                        If (Not OnlySummary) Then
                            List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_TrackRef, Me.TrackRef.ToString().indent(IndentTrackRef, IncludeFirstline:=True, PrependNewlineIfMultiline:=True)))
                        Else
                            List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_TrackRef, Me.TrackRef.ToString().indent(IndentTrackRef, IncludeFirstline:=False, PrependNewlineIfMultiline:=False)))
                        End If
                        List.AppendLine(sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToReport_Points, Me.Points.Count))
                        
                        If (Not OnlySummary) Then
                            List.AppendLine(Rstyx.Utilities.Resources.Messages.TcBlock_TableHeader.ToHeadLine("-", Padding:=False))
                            For i As Integer = 0 To Me.Points.Count - 1
                                List.AppendLine(sprintf("%5d %s", i + 1, Me.Points(i).ToString()))
                            Next
                        End If
                        
                        Return List.ToString()
                    End Function
                    
                #End Region
                
                #Region "Overrides"
                    
                    ''' <inheritdoc/>
                    Public Overrides Function ToString() As String
                        Return sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToString, Me.TrackRef.NameOfAlignment, Me.Points.Count, Me.BlockType.ToString())
                    End Function
                    
                #End Region
                
            End Class
            
            ''' <summary> Info regarding file source of a data block. </summary>
            Public Class DataBlockFileSourceInfo
                
                ''' <summary> Determines the source file path. </summary>
                ''' <remarks> May be <see langword="null"/> (unknown). </remarks>
                Public Property FilePath()      As String = String.Empty
                
                ''' <summary> Line number in source file, representing the first line of the block. </summary>
                 ''' <remarks> May be <b>-1</b> if unknown. </remarks>
                Public Property StartLineNo()   As Integer
                
                ''' <summary> Line number in source file, representing the last line of the block. </summary>
                 ''' <remarks> May be <b>-1</b> if unknown. </remarks>
                Public Property EndLineNo()     As Integer
                    
                ''' <inheritdoc/>
                Public Overrides Function ToString() As String
                    Return sprintf(Rstyx.Utilities.Resources.Messages.DataBlockFileSourceInfo_ToString, Me.FilePath, Me.StartLineNo, Me.EndLineNo)
                End Function
                
            End Class
            
            
            ''' <summary> Info regarding source of every found TC block inside the internal <see cref="DataFile.DataLineBuffer"/>. </summary>
            Protected Class TcSourceBlockInfo
                
                ''' <summary> Type of TC block. Only Program is determined yet! </summary>
                Public BlockType        As New TcBlockType()
                
                ''' <summary> Index into <see cref="DataFile.DataLineBuffer"/> representing the first line of this block. </summary>
                Public StartIndex       As Integer = -1
                
                ''' <summary> Index into <see cref="DataFile.DataLineBuffer"/> representing the first DATA line of this block. </summary>
                Public DataStartIndex   As Integer = -1
                
                ''' <summary> Index into <see cref="DataFile.DataLineBuffer"/> representing the last line of this block. </summary>
                Public EndIndex         As Integer = -1
                
                ''' <summary> Tells if the first DATA line of this block has been found. </summary>
                Public HasData          As Boolean = False
                
                ''' <summary> The TcRecordDefinition to use for parsing each record of this block. </summary>
                Public RecordDefinition As TcRecordDefinition
                
            End Class
            
            ''' <summary> Base Definition of a TC source record. </summary>
            Protected MustInherit Class TcRecordDefinition
                
                Public ID   As DataFieldDefinition(Of String)
                Public Y    As DataFieldDefinition(Of Double)
                Public X    As DataFieldDefinition(Of Double)
                Public Z    As DataFieldDefinition(Of Double)
                Public St   As DataFieldDefinition(Of Kilometer)
                Public Km   As DataFieldDefinition(Of Kilometer)
                Public Q    As DataFieldDefinition(Of Double)
                Public HSOK As DataFieldDefinition(Of Double)
                Public Ra   As DataFieldDefinition(Of Double)
                Public Ri   As DataFieldDefinition(Of Double)
                Public Ueb  As DataFieldDefinition(Of Double)
                Public ZSOK As DataFieldDefinition(Of Double)
            End Class
            
            ''' <summary> Definition of a iGeo TC source record. </summary>
            Protected Class TcRecordDefinitionIGeo
                Inherits TcRecordDefinition
                
                ''' <summary> Creates a new instance of <c>TcRecordDefinitionIGeo</c>. </summary>
                Public Sub New()
                End Sub
                
                ''' <summary> Creates a new instance of <c>TcRecordDefinitionIGeo</c>. </summary>
                 ''' <param name="InitIgnore"> If <see langword="true"/>, all fields will be initialized. </param>
                 ''' <remarks>
                 ''' If <paramref name="InitIgnore"/> is <see langword="true"/>, all fields will be initialized with a valid field definition,
                 ''' that cause the field to be ignored.
                 ''' This is usefull for parsing CSV records.
                 ''' </remarks>
                Public Sub New(InitIgnore As Boolean)
                    If (InitIgnore) Then
                        Me.ID       = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,  DataFieldPositionType.Ignore, 99, 99)
                        Me.Y        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y,        DataFieldPositionType.Ignore, 99, 99)
                        Me.X        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X,        DataFieldPositionType.Ignore, 99, 99)
                        Me.Z        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z,        DataFieldPositionType.Ignore, 99, 99)
                        Me.St       = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_St,    DataFieldPositionType.Ignore, 99, 99)
                        Me.Km       = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,    DataFieldPositionType.Ignore, 99, 99)
                        Me.Q        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,        DataFieldPositionType.Ignore, 99, 99)
                        Me.H        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_H,        DataFieldPositionType.Ignore, 99, 99)
                        Me.HSOK     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK,     DataFieldPositionType.Ignore, 99, 99)
                        Me.QG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QG,       DataFieldPositionType.Ignore, 99, 99)
                        Me.HG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HG,       DataFieldPositionType.Ignore, 99, 99)
                        Me.UebL     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_UebL,     DataFieldPositionType.Ignore, 99, 99)
                        Me.UebR     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_UebR,     DataFieldPositionType.Ignore, 99, 99)
                        Me.Ueb      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb,      DataFieldPositionType.Ignore, 99, 99)
                        Me.Heb      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Heb,      DataFieldPositionType.Ignore, 99, 99)
                        Me.G        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_G,        DataFieldPositionType.Ignore, 99, 99)
                        Me.Ri       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ri,       DataFieldPositionType.Ignore, 99, 99)
                        Me.Ra       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ra,       DataFieldPositionType.Ignore, 99, 99)
                        Me.V        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_V,        DataFieldPositionType.Ignore, 99, 99)
                        Me.R        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_R,        DataFieldPositionType.Ignore, 99, 99)
                        Me.L        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_L,        DataFieldPositionType.Ignore, 99, 99)
                        Me.HDGM     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HDGM,     DataFieldPositionType.Ignore, 99, 99)
                        Me.ZDGM     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZDGM,     DataFieldPositionType.Ignore, 99, 99)
                        Me.Tm       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Tm,       DataFieldPositionType.Ignore, 99, 99)
                        Me.QT       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QT,       DataFieldPositionType.Ignore, 99, 99)
                        Me.ZSOK     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZSOK,     DataFieldPositionType.Ignore, 99, 99)
                        Me.ZLGS     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZLGS,     DataFieldPositionType.Ignore, 99, 99)
                        Me.RaLGS    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_RaLGS,    DataFieldPositionType.Ignore, 99, 99)
                        Me.AbLGS    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_AbLGS,    DataFieldPositionType.Ignore, 99, 99)
                        Me.RG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_RG,       DataFieldPositionType.Ignore, 99, 99)
                        Me.LG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_LG,       DataFieldPositionType.Ignore, 99, 99)
                        Me.QGT      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QGT,      DataFieldPositionType.Ignore, 99, 99)
                        Me.HGT      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HGT,      DataFieldPositionType.Ignore, 99, 99)
                        Me.QGS      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QGS,      DataFieldPositionType.Ignore, 99, 99)
                        Me.HGS      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HGS,      DataFieldPositionType.Ignore, 99, 99)
                        Me.KmStatus = New DataFieldDefinition(Of KilometerStatus)(Rstyx.Utilities.Resources.Messages.Domain_Label_KmStatus, DataFieldPositionType.Ignore, 99, 99)
                        Me.KmText   = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_KmText,   DataFieldPositionType.Ignore, 99, 99)
                        Me.Text     = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Text,     DataFieldPositionType.Ignore, 99, 99)
                        Me.FreeData = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Comment,  DataFieldPositionType.Ignore, 99, 99)
                    End If
                End Sub
                
                Public Delimiter As Char = " "c
                Public LineEndCommentToken As String = Nothing
                
                Public KmStatus As DataFieldDefinition(Of KilometerStatus)
                Public KmText   As DataFieldDefinition(Of String)
                Public H        As DataFieldDefinition(Of Double)
                Public QG       As DataFieldDefinition(Of Double)
                Public HG       As DataFieldDefinition(Of Double)
                Public UebL     As DataFieldDefinition(Of Double)
                Public UebR     As DataFieldDefinition(Of Double)
                Public Heb      As DataFieldDefinition(Of Double)
                Public G        As DataFieldDefinition(Of Double)
                Public V        As DataFieldDefinition(Of Double)
                Public R        As DataFieldDefinition(Of Double)
                Public L        As DataFieldDefinition(Of Double)
                Public HDGM     As DataFieldDefinition(Of Double)
                Public ZDGM     As DataFieldDefinition(Of Double)
                Public Tm       As DataFieldDefinition(Of Double)
                Public QT       As DataFieldDefinition(Of Double)
                Public LG       As DataFieldDefinition(Of Double)
                Public RG       As DataFieldDefinition(Of Double)
                Public QGT      As DataFieldDefinition(Of Double)
                Public HGT      As DataFieldDefinition(Of Double)
                Public QGS      As DataFieldDefinition(Of Double)
                Public HGS      As DataFieldDefinition(Of Double)
                Public ZLGS     As DataFieldDefinition(Of Double)
                Public RaLGS    As DataFieldDefinition(Of Double)
                Public AbLGS    As DataFieldDefinition(Of Double)
                Public Text     As DataFieldDefinition(Of String)
                Public FreeData As DataFieldDefinition(Of String)
            End Class
            
            ''' <summary> Definition of a Verm.esn TC source record. </summary>
            Protected Class TcRecordDefinitionVermEsn
                Inherits TcRecordDefinition
                
                Public ID_Length As Integer
                
                Public Com       As DataFieldDefinition(Of String)
                Public Com2      As DataFieldDefinition(Of String)
                Public QKm       As DataFieldDefinition(Of Double)
            End Class
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Restets this <see cref="TcFileReader"/> and re-initializes it with a new file path. </summary>
             ''' <param name="Path"> The complete path to the data text file to be read. </param>
            Protected Overrides Sub Reset(Path As String)
                MyBase.Reset(Path)
                SourceBlocks.Clear()
                Me.Blocks.Clear()
            End Sub
            
            ''' <summary> Finds the start line of a given block. </summary>
             ''' <param name="DataLines">        The data cache holding the block of interest. </param>
             ''' <param name="CurrentStartIndex"> Index into <paramref name="DataLines"/> determining the line containing "Programm:..." or "[Trassen]umformung". </param>
             ''' <returns> Index into <paramref name="DataLines"/> determining the line that should be the first line of the given block. </returns>
             ''' <remarks> 
             ''' Search backwards for block start.
             ''' All (and only) comment lines before the current line will belong to the block.
             ''' BUT: Avoid including a preceeding empty block of iGeo and iTrassePC by looking for matching tokens.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="DataLines"/> is <see langword="null"/>. </exception>
            Private Function findStartOfBlock(DataLines As Collection(Of DataTextLine), CurrentStartIndex As Integer) As Integer
                
                If (DataLines Is Nothing) Then Throw New System.ArgumentNullException("DataLines")
                
                Dim RetValue            As Integer = 0
                Dim FoundStart          As Boolean = False
                Dim k                   As Integer = CurrentStartIndex - 1
                Dim kvp                 As KeyValuePair(Of String, String)
                Dim PrevDataLine        As DataTextLine
                Dim SearchTerminators() As String = {"Überhöhungsband", "Gradiente", "Km-Linie", "Achse", "Checksumme"}
                
                Do While ((k > -1) AndAlso (Not FoundStart))
                    
                    PrevDataLine = DataLines(k)
                    
                    If (Not PrevDataLine.IsCommentLine) Then
                        ' No comment line.
                        FoundStart = True
                        RetValue = k + 1
                    ElseIf (PrevDataLine.HasComment) Then
                        ' Comment line which isn't empty.
                        If (PrevDataLine.Comment.Trim().IsMatchingTo("^-+|^PktNr")) Then
                            FoundStart = True
                            RetValue = k + 1
                        End If
                        
                        If (Not FoundStart) Then
                            kvp = splitHeaderLineIGeo(PrevDataLine.Comment)
                            If (kvp.Key IsNot Nothing) Then
                                If (kvp.Key.ContainsAny(SearchTerminators)) Then
                                    FoundStart = True
                                    RetValue = k + 1
                                End If
                            End If
                        End If
                    End If
                    
                    k -= 1
                Loop
                
                Return RetValue
            End Function
            
            ''' <summary> Tries to treat a string as a key/value header line of iGeo or iTrassePC output. </summary>
             ''' <param name="Comment"> The string without leading comment token. </param>
             ''' <returns> The found key/value pair, both strings trimmed. If not found, key and value are <see langword="null"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Comment"/> is <see langword="null"/>. </exception>
            Private Function splitHeaderLineIGeo(Comment As String) As KeyValuePair(Of String, String)
                
                If (Comment Is Nothing) Then Throw New System.ArgumentNullException("Comment")
                
                Dim   RetValue  As KeyValuePair(Of String, String) = Nothing
                Const Delimiter As String = ":"
                
                If (Comment.Contains(Delimiter)) Then
                    Dim Key   As String = Comment.Left(Delimiter).Trim()
                    Dim Value As String = Comment.Right(Delimiter, IncludeDelimiter:=False, GetMaximumMatch:=True).Trim()
                    RetValue = New KeyValuePair(Of String, String)(Key, Value)
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Returns a key into the record definition dictionary. </summary>
             ''' <param name="BlockType"> The block type to get the key for. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="BlockType"/> is <see langword="null"/>. </exception>
            Private Function getKeyForRecordDefinition(BlockType As TcBlockType) As String
                
                If (BlockType Is Nothing) Then Throw New System.ArgumentNullException("BlockType")
                
                Return BlockType.Program.ToString & "_" & BlockType.Version.ToString & "_" & BlockType.Format.ToString
            End Function
            
            ''' <summary> Returns the record definition for the given block type. </summary>
             ''' <param name="BlockType"> The block type to get the record definition for. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="BlockType"/> is <see langword="null"/>. </exception>
            Private Function getIGeoRecordDefinition(BlockType As TcBlockType) As TcRecordDefinition
                
                If (BlockType Is Nothing) Then Throw New System.ArgumentNullException("BlockType")
                
                Dim RetValue As TcRecordDefinition = Nothing
                
                If (BlockType.IsValid) Then
                    If (Not (BlockType.SubFormat = TcBlockSubFormat.CSV)) Then
                        ' Predefined (fixed) format.
                        RetValue = IGeoRecordDefinitions.Item(getKeyForRecordDefinition(BlockType))
                    Else
                        ' CSV format depending on actual block.
                        Dim RecordDef As New TcRecordDefinitionIGeo(InitIgnore:=True)
                        
                        RecordDef.Delimiter = "|"c
                        RecordDef.LineEndCommentToken = Nothing
                        Dim FieldNames() As String = BlockType.FieldNames.Split(RecordDef.Delimiter)
                        
                        For i As Integer = 1 To FieldNames.Length
                            
                            Dim FieldName As String = Trim(FieldNames(i - 1))
                            
                            ' A0: PktNr | Y | X | Z | St | Km | Q | H | HSOK | QG | HG | UebLi | UebRe | Ueb | Heb | G | Ri | Ra | V | R | L | HDGM | ZDGM | Tm | QT | ZSOK | ZLGS | RG | LG | QGT | HGT | QGS | HGS | KmStatus | KmText | RaLGS | AbLGS | Text | Kommentar
                            
                            Select Case FieldName
                                Case "PktNr"     : RecordDef.ID       = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,  DataFieldPositionType.WordNumber, i, 0)
                                Case "Y"         : RecordDef.Y        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y,        DataFieldPositionType.WordNumber, i, 0)
                                Case "X"         : RecordDef.X        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X,        DataFieldPositionType.WordNumber, i, 0)
                                Case "Z"         : RecordDef.Z        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z,        DataFieldPositionType.WordNumber, i, 0)
                                Case "St"        : RecordDef.St       = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_St,    DataFieldPositionType.WordNumber, i, 0)
                                Case "Km"        : RecordDef.Km       = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,    DataFieldPositionType.WordNumber, i, 0)
                                Case "Q"         : RecordDef.Q        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,        DataFieldPositionType.WordNumber, i, 0)
                                Case "H"         : RecordDef.H        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_H,        DataFieldPositionType.WordNumber, i, 0)
                                Case "HSOK"      : RecordDef.HSOK     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK,     DataFieldPositionType.WordNumber, i, 0)
                                Case "QG"        : RecordDef.QG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QG,       DataFieldPositionType.WordNumber, i, 0)
                                Case "HG"        : RecordDef.HG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HG,       DataFieldPositionType.WordNumber, i, 0)
                                Case "UebLi"     : RecordDef.UebL     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_UebL,     DataFieldPositionType.WordNumber, i, 0)
                                Case "UebRe"     : RecordDef.UebR     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_UebR,     DataFieldPositionType.WordNumber, i, 0)
                                Case "Ueb"       : RecordDef.Ueb      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb,      DataFieldPositionType.WordNumber, i, 0)
                                Case "Heb"       : RecordDef.Heb      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Heb,      DataFieldPositionType.WordNumber, i, 0)
                                Case "G"         : RecordDef.G        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_G,        DataFieldPositionType.WordNumber, i, 0)
                                Case "Ri"        : RecordDef.Ri       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ri,       DataFieldPositionType.WordNumber, i, 0)
                                Case "Ra"        : RecordDef.Ra       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ra,       DataFieldPositionType.WordNumber, i, 0)
                                Case "V"         : RecordDef.V        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_V,        DataFieldPositionType.WordNumber, i, 0)
                                Case "R"         : RecordDef.R        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_R,        DataFieldPositionType.WordNumber, i, 0)
                                Case "L"         : RecordDef.L        = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_L,        DataFieldPositionType.WordNumber, i, 0)
                                Case "HDGM"      : RecordDef.HDGM     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HDGM,     DataFieldPositionType.WordNumber, i, 0)
                                Case "ZDGM"      : RecordDef.ZDGM     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZDGM,     DataFieldPositionType.WordNumber, i, 0)
                                Case "Tm"        : RecordDef.Tm       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Tm,       DataFieldPositionType.WordNumber, i, 0)
                                Case "QT"        : RecordDef.QT       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QT,       DataFieldPositionType.WordNumber, i, 0)
                                Case "ZSOK"      : RecordDef.ZSOK     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZSOK,     DataFieldPositionType.WordNumber, i, 0)
                                Case "ZLGS"      : RecordDef.ZLGS     = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZLGS,     DataFieldPositionType.WordNumber, i, 0)
                                Case "RaLGS"     : RecordDef.RaLGS    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_RaLGS,    DataFieldPositionType.WordNumber, i, 0)
                                Case "AbLGS"     : RecordDef.AbLGS    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_AbLGS,    DataFieldPositionType.WordNumber, i, 0)
                                Case "RG"        : RecordDef.RG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_RG,       DataFieldPositionType.WordNumber, i, 0)
                                Case "LG"        : RecordDef.LG       = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_LG,       DataFieldPositionType.WordNumber, i, 0)
                                Case "QGT"       : RecordDef.QGT      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QGT,      DataFieldPositionType.WordNumber, i, 0)
                                Case "HGT"       : RecordDef.HGT      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HGT,      DataFieldPositionType.WordNumber, i, 0)
                                Case "QGS"       : RecordDef.QGS      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QGS,      DataFieldPositionType.WordNumber, i, 0)
                                Case "HGS"       : RecordDef.HGS      = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HGS,      DataFieldPositionType.WordNumber, i, 0)
                                Case "KmStatus"  : RecordDef.KmStatus = New DataFieldDefinition(Of KilometerStatus)(Rstyx.Utilities.Resources.Messages.Domain_Label_KmStatus, DataFieldPositionType.WordNumber, i, 0)
                                Case "KmText"    : RecordDef.KmText   = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_KmText,   DataFieldPositionType.WordNumber, i, 0, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                                Case "Text"      : RecordDef.Text     = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Text,     DataFieldPositionType.WordNumber, i, 0, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                                Case "Kommentar" : RecordDef.FreeData = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_FreeData, DataFieldPositionType.WordNumber, i, 0, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                            End Select
                        Next
                        
                        RetValue = RecordDef
                    End If
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Returns the plain (file) name from a matched name. If <paramref name="LineNameMatch"/> isn't a valid path name, it will be returned without changes. </summary>
             ''' <param name="LineNameMatch"> The match from a RegEx that may contain full path or not. </param>
             ''' <param name="WithExtension"> If <see langword="true"/>, the extension isn't removed. </param>
            Private Function getNameFromMatch(LineNameMatch As String, WithExtension As Boolean) As String
                Dim RetValue As String = LineNameMatch
                
                If ((FileUtils.IsValidFilePath(LineNameMatch)) AndAlso Path.IsPathRooted(LineNameMatch)) Then
                    If (WithExtension) Then
                        RetValue = Path.GetFileName(LineNameMatch)
                    Else
                        RetValue = Path.GetFileNameWithoutExtension(LineNameMatch)
                    End If
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Reads a block of Verm.esn output ("Umformung"). </summary>
             ''' <param name="DataLines">  The data cache holding the block of interest. </param>
             ''' <param name="SourceBlock"> Determines the block type and it's position in <paramref name="DataLines"/>. </param>
             ''' <returns> The complete <see cref="TcBlock"/> read from <paramref name="DataLines"/>. </returns>
             ''' <remarks>
             ''' <list type="bullet">
             ''' <listheader> <description> <b>Hints:</b> </description></listheader>
             ''' <item> The returned block <b>may be invalid!</b> (check for .IsValid!). </item>
             ''' <item> All parsing errors will be added to <see cref="TcFileReader.ParseErrors"/> property. </item>
             ''' <item> The Points collection (<see cref="TcFileReader.Blocks"/>.<see cref="TcFileReader.TcBlock.Points"/>) may be empty. </item>
             ''' </list>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="DataLines"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceBlock"/> is <see langword="null"/>. </exception>
            Private Function readBlockVermEsn(DataLines As Collection(Of DataTextLine), SourceBlock As TcSourceBlockInfo) As TcBlock
                
                If (DataLines Is Nothing) Then Throw New System.ArgumentNullException("DataLines")
                If (SourceBlock Is Nothing) Then Throw New System.ArgumentNullException("SourceBlock")
                
                Logger.logDebug(sprintf(" Reading Verm.esn TC output block (start index = %d, end index = %d)", SourceBlock.StartIndex, SourceBlock.EndIndex))
                
                Dim Block            As New TcBlock()
                Dim UniqueID         As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
                Dim UniqueIDPerBlock As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock) AndAlso (Not Constraints.HasFlag(GeoPointConstraints.UniqueID)))
                
                If (UniqueIDPerBlock) Then Me.IDCheckList.Clear()
                
                ' Parse header.
                findBlockMetaDataVermEsn(DataLines, SourceBlock, Block)
                
                ' Read points.
                If (Not Block.IsValid) Then
                    Me.ParseErrors.AddError(Block.Source.StartLineNo, 0, 0, sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_InvalidTcBlock, Block.Source.StartLineNo, Block.Source.EndLineNo, Block.Error))
                Else
                    If (SourceBlock.HasData) Then
                        
                        Dim RecDef        As TcRecordDefinitionVermEsn = DirectCast(SourceBlock.RecordDefinition, TcRecordDefinitionVermEsn)
                        Dim RecLen        As Integer = If(Block.BlockType.SubFormat = TcBlockSubFormat.TwoLine, 2, 1)
                        Dim RecIdx        As Integer = SourceBlock.DataStartIndex - RecLen
                        Dim FieldID       As DataField(Of String) = Nothing
                        Dim LastRecEmpty  As Boolean = False
                        Dim DataLine      As DataTextLine = Nothing
                        Dim VEPoint       As New GeoVEPoint(RecDef.ID_Length)
                        
                        Do While (RecIdx <= SourceBlock.EndIndex - RecLen)
                            Try
                                ' Switch to next record.
                                If (LastRecEmpty) Then
                                    RecIdx += 1
                                Else
                                    RecIdx += RecLen
                                End If
                                DataLine = DataLines(RecIdx)
                                
                                If (Not DataLine.HasData) Then
                                    LastRecEmpty = True
                                Else
                                    LastRecEmpty = False
                                    
                                    Dim Com2 As String = Nothing
                                    Dim ID2  As String = Nothing
                                    
                                    Dim p    As New GeoTcPoint()
                                    
                                    ' Cartesian coordinates line.
                                    If (Block.BlockType.SubFormat = TcBlockSubFormat.TwoLine) Then
                                        FieldID    = DataLine.ParseField(RecDef.ID)
                                        VEPoint.ID = FieldID.Value  ' Verify ID format.
                                        ID2        = VEPoint.ID
                                        p.Y        = DataLine.ParseField(RecDef.Y).Value
                                        p.X        = DataLine.ParseField(RecDef.X).Value
                                        p.Z        = DataLine.ParseField(RecDef.Z).Value
                                        p.CoordSys = Block.TrackRef.CoordSys
                                        Com2       = DataLine.ParseField(RecDef.Com2).Value.Trim()
                                        If (RecDef.St IsNot Nothing) Then p.St = DataLine.ParseField(RecDef.St).Value
                                        
                                        ' Get second line of this record.
                                        DataLine = DataLines(RecIdx + 1)
                                        If (Not DataLine.HasData) Then Throw New ParseException(New ParseError(ParseErrorLevel.Error, DataLine.SourceLineNo, 0, 0, Rstyx.Utilities.Resources.Messages.TcFileReader_MissingSecondLine, Nothing))
                                    End If
                                    
                                    ' Track coordinates line.
                                    ' Point-ID.
                                    FieldID    = DataLine.ParseField(RecDef.ID)
                                    VEPoint.ID = FieldID.Value  ' Verify ID format.
                                    p.ID       = VEPoint.ID
                                    If ((Block.BlockType.SubFormat = TcBlockSubFormat.TwoLine) AndAlso (Not (p.ID = ID2))) Then
                                        Throw New ParseException(ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_IDMismatch, p.ID, ID2), Nothing, Me.FilePath))
                                    End If
                                    
                                    ' Track values.
                                    p.Q    = DataLine.ParseField(RecDef.Q).Value
                                    p.Ra   = DataLine.ParseField(RecDef.Ra).Value
                                    p.Ri   = DataLine.ParseField(RecDef.Ri).Value / RHO
                                    p.Ueb  = DataLine.ParseField(RecDef.Ueb).Value / 1000
                                    p.ZSOK = DataLine.ParseField(RecDef.ZSOK).Value
                                    p.HSOK = DataLine.ParseField(RecDef.HSOK).Value * (-1)
                                    p.QKm  = DataLine.ParseField(RecDef.QKm).Value
                                    
                                    If (Not Double.IsNaN(p.QKm)) Then
                                        p.Km = DataLine.ParseField(RecDef.Km).Value
                                    Else
                                        p.St = DataLine.ParseField(RecDef.Km).Value
                                    End If
                                    
                                    ' Point info.
                                    p.Info = DataLine.ParseField(RecDef.Com).Value.Trim()
                                    If (p.Info.IsEmptyOrWhiteSpace()) Then p.Info = Com2
                                    
                                    ' Other info.
                                    p.CantBase     = Me.CantBase
                                    p.SourcePath   = Block.Source.FilePath
                                    p.SourceLineNo = DataLine.SourceLineNo
                                    p.TrackRef     = Block.TrackRef
                                    
                                    ' Editing.
                                    If (p.Kind = GeoPointKind.None) Then
                                        If (Me.EditOptions.HasFlag(GeoPointEditOptions.GuessAllKindsFromInfo)) Then
                                            p.ParseInfoForKindHints()
                                        ElseIf (Me.EditOptions.HasFlag(GeoPointEditOptions.ParseCantFromInfo)) Then
                                            p.ParseInfoForActualCant()
                                        End If
                                    End If
                                    p.SetKindTextFromKind(Override:=False)
                                    
                                    ' Calculate values not have been read.
                                    p.transformHorizontalToCanted()
                                    If ((Not p.Km.HasValue) AndAlso Me.StationAsKilometer) Then p.Km = p.St
                                    If (Double.IsNaN(p.Z)) Then p.Z = p.ZSOK + p.HSOK
                                    If (Not Double.IsNaN(p.Z)) Then p.HeightSys = Block.TrackRef.HeightSys
                                    
                                    ' Check for required constraints.
                                    If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                    p.VerifyConstraints(Me.Constraints)
                                    
                                    Block.Points.Add(p)
                                End If
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, ex.Message, Nothing, Me.FilePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, Block.Source.FilePath))
                                End If
                                
                            Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                Me.ParseErrors.Add(ex.ParseError)
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, Block.Source.FilePath))
                                End If
                            End Try
                        Loop
                    End If
                    
                    ' Warning for empty block.
                    If (Block.Points.Count = 0) Then
                        Me.ParseErrors.AddWarning(Block.Source.StartLineNo, 0, 0, sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_EmptyTcBlock, Block.Source.StartLineNo, Block.Source.EndLineNo))
                    End If
                End If
                
                Return Block
            End Function
            
            ''' <summary> Reads a block of iGeo/iTrassePC output (A0, A1). </summary>
             ''' <param name="DataLines">  The data cache holding the block of interest. </param>
             ''' <param name="SourceBlock"> Determines the block type and it's position in <paramref name="DataLines"/>. </param>
             ''' <returns> The complete <see cref="TcBlock"/> read from <paramref name="DataLines"/>. </returns>
             ''' <remarks>
             ''' <list type="bullet">
             ''' <listheader> <description> <b>Hints:</b> </description></listheader>
             ''' <item> The returned block <b>may be invalid!</b> (check for .IsValid!). </item>
             ''' <item> All parsing errors will be added to <see cref="TcFileReader.ParseErrors"/> property. </item>
             ''' <item> The Block's Points collection (<see cref="TcFileReader.Blocks"/>.<see cref="TcFileReader.TcBlock.Points"/>) may be empty. </item>
             ''' </list>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="DataLines"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceBlock"/> is <see langword="null"/>. </exception>
            Private Function readBlockIGeo(DataLines As Collection(Of DataTextLine), SourceBlock As TcSourceBlockInfo) As TcBlock
                
                If (DataLines Is Nothing) Then Throw New System.ArgumentNullException("DataLines")
                If (SourceBlock Is Nothing) Then Throw New System.ArgumentNullException("SourceBlock")
                
                Logger.logDebug(sprintf(" Reading iGeo/iTrassePC TC output block (start index = %d, end index = %d)", SourceBlock.StartIndex, SourceBlock.EndIndex))
                
                Dim Block            As New TcBlock()
                Dim UniqueID         As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
                Dim UniqueIDPerBlock As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock) AndAlso (Not Constraints.HasFlag(GeoPointConstraints.UniqueID)))
                
                If (UniqueIDPerBlock) Then Me.IDCheckList.Clear()
                
                ' Parse header.
                findBlockMetaDataIGeo(DataLines, SourceBlock, Block)
                
                ' Read points.
                If (Not Block.IsValid) Then
                    Me.ParseErrors.AddError(Block.Source.StartLineNo, 0, 0, sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_InvalidTcBlock, Block.Source.StartLineNo, Block.Source.EndLineNo, Block.Error))
                Else
                    If (SourceBlock.HasData) Then
                        Dim RecDef    As TcRecordDefinitionIGeo = DirectCast(SourceBlock.RecordDefinition, TcRecordDefinitionIGeo)
                        Dim RecIdx    As Integer = SourceBlock.DataStartIndex - 1
                        Dim DataLine  As DataTextLine = Nothing
                        Dim FieldID   As DataField(Of String) = Nothing
                        Dim IpktAux   As New GeoIPoint()
                        
                        Do While (RecIdx <= SourceBlock.EndIndex - 1)
                            Try
                                ' Switch to next record.
                                RecIdx += 1
                                
                                If (DataLines(RecIdx).HasData) Then
                                    
                                    DataLine = New DataTextLine(DataLines(RecIdx).Data, LineStartCommentToken:="#", LineEndCommentToken:=RecDef.LineEndCommentToken)
                                    DataLine.SourceLineNo = DataLines(RecIdx).SourceLineNo
                                    DataLine.FieldDelimiter = RecDef.Delimiter
                                    
                                    Dim UebL As Double = Double.NaN
                                    Dim UebR As Double = Double.NaN
                                    
                                    Dim p    As New GeoTcPoint()
                                    
                                    ' ID
                                    FieldID = DataLine.ParseField(RecDef.ID) 
                                    p.ID    = FieldID.Value
                                    
                                    ' Cartesian coordinates.
                                    Dim FieldY As DataField(Of Double) = DataLine.ParseField(RecDef.Y)
                                    Dim FieldX As DataField(Of Double) = DataLine.ParseField(RecDef.X)
                                    Dim FieldZ As DataField(Of Double) = DataLine.ParseField(RecDef.Z)
                                    
                                    p.Y     = FieldY.Value
                                    p.X     = FieldX.Value
                                    p.Z     = FieldZ.Value
                                    
                                    ' Track values.
                                    p.St    = DataLine.ParseField(RecDef.St).Value
                                    p.Km    = DataLine.ParseField(RecDef.Km).Value
                                    p.Q     = DataLine.ParseField(RecDef.Q).Value
                                    p.H     = DataLine.ParseField(RecDef.H).Value
                                    p.HSOK  = DataLine.ParseField(RecDef.HSOK).Value
                                    
                                    p.QG    = DataLine.ParseField(RecDef.QG).Value
                                    p.HG    = DataLine.ParseField(RecDef.HG).Value
                                    
                                    UebL    = DataLine.ParseField(RecDef.UebL).Value
                                    UebR    = DataLine.ParseField(RecDef.UebR).Value
                                    p.Ueb   = DataLine.ParseField(RecDef.Ueb).Value
                                    p.Heb   = DataLine.ParseField(RecDef.Heb).Value
                                    
                                    p.G     = DataLine.ParseField(RecDef.G).Value
                                    p.Ri    = DataLine.ParseField(RecDef.Ri).Value
                                    p.Ra    = DataLine.ParseField(RecDef.Ra).Value
                                    
                                    p.V     = DataLine.ParseField(RecDef.V).Value
                                    p.R     = DataLine.ParseField(RecDef.R).Value
                                    p.L     = DataLine.ParseField(RecDef.L).Value
                                    
                                    p.HDGM  = DataLine.ParseField(RecDef.HDGM).Value
                                    p.ZDGM  = DataLine.ParseField(RecDef.ZDGM).Value
                                    
                                    p.Tm    = DataLine.ParseField(RecDef.Tm).Value
                                    p.QT    = DataLine.ParseField(RecDef.QT).Value

                                    p.ZSOK  = DataLine.ParseField(RecDef.ZSOK).Value
                                    p.ZLGS  = DataLine.ParseField(RecDef.ZLGS).Value
                                    p.RaLGS = DataLine.ParseField(RecDef.RaLGS).Value
                                    p.AbLGS = DataLine.ParseField(RecDef.AbLGS).Value

                                    p.RG    = DataLine.ParseField(RecDef.RG).Value
                                    p.LG    = DataLine.ParseField(RecDef.LG).Value
                                    p.QGT   = DataLine.ParseField(RecDef.QGT).Value
                                    p.HGT   = DataLine.ParseField(RecDef.HGT).Value
                                    p.QGS   = DataLine.ParseField(RecDef.QGS).Value
                                    p.HGS   = DataLine.ParseField(RecDef.HGS).Value
                                    
                                    ' Kilometer: Text and Status.
                                    Dim KmStat As KilometerStatus = DataLine.ParseField(RecDef.KmStatus).Value
                                    Dim KmText As String          = DataLine.ParseField(RecDef.KmText).Value
                                    If (Double.IsNaN(p.Km.Value)) Then
                                        ' Not Km value yet => Parse KmText and set Status if available.
                                        If (KmText.IsNotEmptyOrWhiteSpace()) Then
                                            If (KmStat = KilometerStatus.Unknown) Then
                                                p.Km = New Kilometer(KmText)
                                            Else
                                                p.Km = New Kilometer(KmText, KmStat)
                                            End If
                                        End If
                                    Else
                                        ' Km value already there => Set KmText and Status if available.
                                        If (Not KmStat = KilometerStatus.Unknown) Then
                                            p.Km = New Kilometer(p.Km.TDBValue, KmStat)
                                        End If
                                        If (KmText.IsNotEmptyOrWhiteSpace()) Then
                                            p.Km.Text = KmText
                                        End If
                                    End If
                                    
                                    ' Attributes and comment.
                                    Dim FreeDataText As String
                                    If (Block.BlockType.Format = TcBlockFormat.A0) Then
                                        ' A0:
                                        FreeDataText = DataLine.ParseField(RecDef.FreeData).Value
                                        FreeDataText.Replace(RecDef.Delimiter & RecDef.Delimiter, RecDef.Delimiter)  ' Demask masked attribute delimiters.
                                    Else
                                        ' A1:
                                        FreeDataText = DataLine.Comment
                                    End If
                                    IpktAux.ParseFreeData(FreeDataText)
                                    p.Attributes = IpktAux.Attributes
                                    p.Comment    = IpktAux.Comment
                                    
                                    ' Info and point kinds (maybe with related data: MarkTypeAB, MarkType, ActualCant).
                                    IpktAux.ParseTextForKindCodes(DataLine.ParseField(RecDef.Text).Value)
                                    p.ActualCant = IpktAux.ActualCant
                                    p.Info       = IpktAux.Info
                                    p.Kind       = IpktAux.Kind
                                    p.MarkType   = IpktAux.MarkType
                    
                                    ' Convert selected attributes to properties, which don't belong to .A0 file.
                                    Dim PropertyName   As String
                                    Dim AttStringValue As String
                                    PropertyName = "HeightSys"
                                    AttStringValue = p.GetAttValueByPropertyName(PropertyName)
                                    If (AttStringValue IsNot Nothing) Then
                                        P.HeightSys = AttStringValue.Trim()
                                        p.Attributes.Remove(GeoPoint.AttributeNames(PropertyName))
                                    End If
                                    PropertyName = "KindText"
                                    AttStringValue = p.GetAttValueByPropertyName(PropertyName)
                                    If (AttStringValue IsNot Nothing) Then
                                        P.KindText = AttStringValue.Trim()
                                        p.Attributes.Remove(GeoPoint.AttributeNames(PropertyName))
                                    End If
                                    
                                    ' Editing.
                                    If (p.Kind = GeoPointKind.None) Then
                                        If (Me.EditOptions.HasFlag(GeoPointEditOptions.GuessAllKindsFromInfo)) Then
                                            p.ParseInfoForKindHints(TryComment:=True)
                                        ElseIf (Me.EditOptions.HasFlag(GeoPointEditOptions.ParseCantFromInfo)) Then
                                            p.ParseInfoForActualCant(TryComment:=True)
                                        End If
                                    End If
                                    p.SetKindTextFromKind(Override:=False)
                                    
                                    ' Other info.
                                    p.CantBase     = Me.CantBase
                                    p.SourcePath   = Block.Source.FilePath
                                    p.SourceLineNo = DataLine.SourceLineNo
                                    p.TrackRef     = Block.TrackRef
                                    
                                    ' Justify Cant and Resolve Ambiguities.
                                    ' TODO: Check tolerances and infinities.
                                    Dim NoRadius As Boolean = (Double.IsNaN(p.Ra) OrElse p.Ra.EqualsTolerance(0, 0.001))
                                    If (p.Ueb.EqualsTolerance(0, 0.001) AndAlso NoRadius) Then
                                        ' Ignore minimal cant if radius is unknown or zero.
                                        p.Ueb = 0.0
                                    End If
                                    If (Not (Double.IsNaN(p.Ueb) OrElse p.Ueb.EqualsTolerance(0, 0.001))) Then
                                        If (NoRadius) Then
                                            ' Ensure that sign of cant is determinable by setting a special radius.
                                            Dim CantSign As Double = UebL - UebR
                                            If (Not Double.IsNaN(CantSign)) Then
                                                p.Ra = If(CantSign < 0, Double.NegativeInfinity, Double.PositiveInfinity)
                                            End If
                                        Else
                                            ' Cant sign: Calculate from UebL, UebR and radius.
                                            Dim CantSign As Double = Sign(UebL - UebR) * Sign(p.Ra)
                                            If (Not Double.IsNaN(CantSign)) Then
                                                p.Ueb = CantSign * Abs(p.Ueb)
                                            End If
                                        End If
                                    End If
                                    If (Block.BlockType.Format = TcBlockFormat.A1) Then
                                        If (Block.TrackRef.NameOfCantLine.IsEmptyOrWhiteSpace()) Then
                                            p.H    = p.HSOK
                                            p.HSOK = Double.NaN
                                        End If
                                    End If
                                    
                                    ' Correct Zero to NaN.
                                    If (Block.BlockType.Program = TcBlockProgram.iTrassePC) Then
                                        If (Block.TrackRef.NameOfCantLine.IsEmptyOrWhiteSpace()) Then
                                            p.HSOK = Double.NaN
                                            p.ZSOK = Double.NaN
                                            p.QG   = Double.NaN
                                            p.HG   = Double.NaN
                                            p.Heb  = Double.NaN
                                            p.Ueb  = Double.NaN
                                            UebL   = Double.NaN
                                            UebR   = Double.NaN
                                        End If
                                        If (Block.TrackRef.NameOfGradientLine.IsEmptyOrWhiteSpace()) Then
                                            p.ZLGS = Double.NaN
                                            p.H    = Double.NaN
                                            p.HSOK = Double.NaN
                                            p.G    = Double.NaN
                                        End If
                                        If (Block.TrackRef.NameOfGradientLine.IsEmptyOrWhiteSpace() OrElse Block.TrackRef.NameOfRoadSections.IsEmptyOrWhiteSpace()) Then
                                            p.V    = Double.NaN
                                        End If
                                        If (Block.TrackRef.NameOfGradientLine.IsEmptyOrWhiteSpace() OrElse Block.TrackRef.NameOfTunnelSections.IsEmptyOrWhiteSpace()) Then
                                            p.R    = Double.NaN
                                            p.L    = Double.NaN
                                        End If
                                        If (Block.TrackRef.NameOfDTM.IsEmptyOrWhiteSpace()) Then
                                            p.HDGM = Double.NaN
                                            p.ZDGM = Double.NaN
                                        End If
                                    End If
                                    
                                    ' Calculate values not have been read.
                                    p.transformHorizontalToCanted()
                                    If ((Not p.Km.HasValue) AndAlso Me.StationAsKilometer) Then p.Km = p.St
                                    If (Double.IsNaN(p.Z))    Then p.Z    = p.ZSOK + p.HSOK
                                    If (Double.IsNaN(p.ZSOK)) Then p.ZSOK = p.Z - p.HSOK
                                    
                                    ' Check for required constraints.
                                    If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                    p.VerifyConstraints(Me.Constraints, FieldX, FieldY, FieldZ)
                                    
                                    Block.Points.Add(p)
                                End If
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, ex.Message, Nothing, Block.Source.FilePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, Block.Source.FilePath))
                                End If
                                
                            Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                Me.ParseErrors.Add(ex.ParseError)
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, Block.Source.FilePath))
                                End If
                            End Try
                        Loop
                    End If
                    
                    ' Warning for empty block.
                    If (Block.Points.Count = 0) Then
                        Me.ParseErrors.AddWarning(Block.Source.StartLineNo, 0, 0, sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_EmptyTcBlock, Block.Source.StartLineNo, Block.Source.EndLineNo))
                    End If
                End If
                
                Return Block
            End Function
            
            ''' <summary> Finds meta data of a block of Verm.esn output ("Umformung"), especially block type and geometry reference info and first data line. </summary>
             ''' <param name="DataLines">     [In]  The data cache holding the block of interest. </param>
             ''' <param name="SourceBlock">    [In/Out] Info about block source in <paramref name="DataLines"/>. </param>
             ''' <param name="Block">          [Out] The TcBlock to fill with info. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="DataLines"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceBlock"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Block"/> is <see langword="null"/>. </exception>
            Private Sub findBlockMetaDataVermEsn(ByRef DataLines As Collection(Of DataTextLine), ByRef SourceBlock As TcSourceBlockInfo, ByRef Block As TcBlock)
                
                If (DataLines Is Nothing)   Then Throw New System.ArgumentNullException("DataLines")
                If (SourceBlock Is Nothing) Then Throw New System.ArgumentNullException("SourceBlock")
                If (Block Is Nothing)       Then Throw New System.ArgumentNullException("Block")
                
                Dim DataLine        As DataTextLine
                Dim FullLine        As String
                Dim Pattern         As String
                Dim PathName        As String
                Dim Comment         As New StringBuilder()
                Dim oMatch          As Match
                Dim FormatFound     As Boolean = False
                Dim i               As Integer = SourceBlock.StartIndex
                
                Block.BlockType.Program  = SourceBlock.BlockType.Program
                
                Block.Source.FilePath    = Me.FilePath
                Block.Source.StartLineNo = DataLines(SourceBlock.StartIndex).SourceLineNo
                Block.Source.EndLineNo   = DataLines(SourceBlock.EndIndex).SourceLineNo
                
                Logger.logDebug(" Find meta data of Verm.esn TC output block:")
                
                Do
                    DataLine = DataLines(i)
                    
                    If (DataLine.IsCommentLine) Then
                        If (DataLine.HasComment) Then
                            If (Comment.Length > 0) Then Comment.AppendLine()
                            Comment.Append(DataLine.Comment)
                        End If
                    Else
                        
                        FullLine = DataLine.getFullLine()
                        
                        If (Not FullLine.IsMatchingTo("^ ?[0-9]")) Then
                            ' Header line.
                            If (FullLine.IsNotEmptyOrWhiteSpace()) Then
                                ' Check for: Format, Version and Alignement name.
                                If (Block.BlockType.Format = TcBlockFormat.None) Then
                                    ' Try: THW, current version (alignment full path)
                                    Pattern = "^Trassenumformung\s+(.+?\.tra)\s+\("
                                    oMatch = Regex.Match(FullLine, Pattern, RegexOptions.IgnoreCase)
                                    
                                    If (oMatch.Success) Then
                                        FormatFound = True
                                        Block.BlockType.Version = TcBlockVersion.Current
                                        Block.BlockType.Format  = TcBlockFormat.THW
                                        PathName = oMatch.Groups(1).Value
                                        Block.TrackRef.NameOfAlignment = getNameFromMatch(PathName, False)
                                        SourceBlock.RecordDefinition = VermEsnRecordDefinitions.Item(getKeyForRecordDefinition(Block.BlockType))
                                    End If
                                End If
                                If (Block.BlockType.Format = TcBlockFormat.None) Then
                                    ' Try: THW, outdated version (DOS, alignment name without path and without spaces!)
                                    Pattern = "^Trassenumformung\s+(.+?)\s+\("
                                    oMatch = Regex.Match(FullLine, Pattern, RegexOptions.IgnoreCase)
                                    
                                    If (oMatch.Success) Then
                                        FormatFound = True
                                        Block.BlockType.Version = TcBlockVersion.Outdated
                                        Block.BlockType.Format  = TcBlockFormat.THW
                                        Block.TrackRef.NameOfAlignment = oMatch.Groups(1).Value
                                        SourceBlock.RecordDefinition = VermEsnRecordDefinitions.Item(getKeyForRecordDefinition(Block.BlockType))
                                    End If
                                End If
                                If (Block.BlockType.Format = TcBlockFormat.None) Then
                                    ' Try: D3, both versions.
                                    Pattern = "^Umformung\s+(.+)\s+\("
                                    oMatch = Regex.Match(FullLine, Pattern, RegexOptions.IgnoreCase)
                                    
                                    If (oMatch.Success) Then
                                        FormatFound = True
                                        Block.BlockType.Format = TcBlockFormat.D3
                                        PathName = oMatch.Groups(1).Value
                                        
                                        ' Version.
                                        If (PathName.Contains(Path.DirectorySeparatorChar)) Then
                                            ' Current version (D3 project full path).
                                            Block.BlockType.Version = TcBlockVersion.Current
                                        Else
                                            ' Outdated version (No D3 project name/path, only internal line name).
                                            Block.BlockType.Version = TcBlockVersion.Outdated
                                        End If
                                        
                                        ' Alignment name.
                                        Block.TrackRef.NameOfAlignment = getNameFromMatch(PathName, True)
                                        
                                        ' Record definition.
                                        SourceBlock.RecordDefinition = VermEsnRecordDefinitions.Item(getKeyForRecordDefinition(Block.BlockType))
                                    End If
                                End If
                                
                                ' Check for: Gradient and Km name, and also for system names.
                                Pattern = "^\s*(.*?)\s*:\s*(\S*.*\S+)\s*"
                                oMatch = Regex.Match(FullLine, Pattern)
                                If (oMatch.Success) Then
                                    Dim Key   As String  = oMatch.Groups(1).Value
                                    Dim Value As String  = oMatch.Groups(2).Value
                                    Select Case Key
                                        Case "Gradiente":            Block.TrackRef.NameOfGradientLine = getNameFromMatch(Value, False)
                                        Case "Reduktion auf Trasse": Block.TrackRef.NameOfKmAlignment  = getNameFromMatch(Value, False)
                                        Case "Lagesystem":           Block.TrackRef.CoordSys           = Value.Trim()
                                        Case "Höhensystem":          Block.TrackRef.HeightSys          = Value.Trim()
                                    End Select
                                End If
                            End If
                            
                        Else
                            ' First Data Line => Check for sub format.
                            SourceBlock.HasData = True
                            SourceBlock.DataStartIndex = i
                            
                            ' Determine sub format.
                            If (FormatFound) Then
                                Block.BlockType.SubFormat = TcBlockSubFormat.OneLine
                                If ((i < SourceBlock.EndIndex) AndAlso (DataLines(i + 1).HasData)) Then
                                    Dim IDLength As Integer = DirectCast(SourceBlock.RecordDefinition, TcRecordDefinitionVermEsn).ID.Length
                                    Dim ID1      As String  = DataLine.Data.Left(IDLength)
                                    Dim ID2      As String  = DataLines(i + 1).Data.Left(IDLength)
                                    If (ID1 = ID2) Then
                                        Block.BlockType.SubFormat = TcBlockSubFormat.TwoLine
                                    End If
                                End If
                            End If
                        End If
                    End If
                    
                    i += 1
                Loop Until (SourceBlock.HasData OrElse (i > SourceBlock.EndIndex))
                
                Block.Comment = Comment.ToString()
                Block.Points.Header.Add(Block.Comment)
                
                Logger.logDebug(sprintf("  Name of Alignment    : %s", Block.TrackRef.NameOfAlignment))
                Logger.logDebug(sprintf("  Name of Km-Alignment : %s", Block.TrackRef.NameOfKmAlignment))
                Logger.logDebug(sprintf("  Name of Gradient Line: %s", Block.TrackRef.NameOfGradientLine))
                If (SourceBlock.HasData) Then Logger.logDebug(sprintf("  First Data Line      : %d", DataLines(SourceBlock.DataStartIndex).SourceLineNo))
            End Sub
            
            ''' <summary> Finds meta data of a block of iGeo/iTrassePC output (A0, A1), especially block type and geometry reference info and first data line. </summary>
             ''' <param name="DataLines">   [In]  The data cache holding the block of interest. </param>
             ''' <param name="SourceBlock"> [In/Out] Info about block source in <paramref name="DataLines"/>. </param>
             ''' <param name="Block">       [Out] The TcBlock to fill with info. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="DataLines"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceBlock"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Block"/> is <see langword="null"/>. </exception>
            Private Sub findBlockMetaDataIGeo(ByRef DataLines As Collection(Of DataTextLine), ByRef SourceBlock As TcSourceBlockInfo, ByRef Block As TcBlock)
                
                If (DataLines Is Nothing)   Then Throw New System.ArgumentNullException("DataLines")
                If (SourceBlock Is Nothing) Then Throw New System.ArgumentNullException("SourceBlock")
                If (Block Is Nothing)       Then Throw New System.ArgumentNullException("Block")
                
                Dim DataLine     As DataTextLine
                Dim Comment      As New StringBuilder()
                Dim CommentEnd   As Boolean = False
                Dim kvp          As KeyValuePair(Of String, String)
                Dim i            As Integer = SourceBlock.StartIndex
                
                ' Supported Format strings.
                Const Fmt_A0 = "A0 : Alles EDV"
                Const Fmt_A1 = "A1 : PktNr  Y  X  Z  St/Km"  ' kleinster gemeinsamer Nenner von iTrassePC und iGeo.
                
                Block.BlockType.Program  = SourceBlock.BlockType.Program
                Block.BlockType.Version  = TcBlockVersion.Current
                
                Block.Source.FilePath    = Me.FilePath
                Block.Source.StartLineNo = DataLines(SourceBlock.StartIndex).SourceLineNo
                Block.Source.EndLineNo   = DataLines(SourceBlock.EndIndex).SourceLineNo
                
                Logger.logDebug(" Find meta data of iGeo TC output block:")
                
                Do
                    DataLine = DataLines(i)
                    
                    If (DataLine.IsCommentLine) Then
                        ' Comment line: Look for Comment and output header of iGeo and iTrassePC.
                        If (DataLine.HasComment) Then
                            kvp = splitHeaderLineIGeo(DataLine.Comment)
                            If (kvp.Key IsNot Nothing) Then
                                Select Case kvp.Key
                                    
                                    Case "Format"
                                        If (kvp.Value = Fmt_A0) Then
                                            
                                            Block.BlockType.Format = TcBlockFormat.A0
                                            Block.BlockType.SubFormat = TcBlockSubFormat.CSV
                                            
                                        ElseIf (kvp.Value.IsMatchingTo(Fmt_A1)) Then
                                            
                                            Block.BlockType.Format = TcBlockFormat.A1
                                        End If
                                    
                                    Case "Datei":               CommentEnd = True
                                    
                                    Case "Achse":               Block.TrackRef.NameOfAlignment      = getNameFromMatch(kvp.Value, False) : CommentEnd = True
                                    Case "Stationierungsachse": Block.TrackRef.NameOfKmAlignment    = getNameFromMatch(kvp.Value, False) : CommentEnd = True  ' iTrassePC
                                    Case "Km-Linie":            Block.TrackRef.NameOfKmAlignment    = getNameFromMatch(kvp.Value, False) : CommentEnd = True  ' iGeo
                                    Case "Ueberhöhungsband":    Block.TrackRef.NameOfCantLine       = getNameFromMatch(kvp.Value, False) : CommentEnd = True  ' iTrassePC
                                    Case "Überhöhungsband":     Block.TrackRef.NameOfCantLine       = getNameFromMatch(kvp.Value, False) : CommentEnd = True  ' iGeo
                                    Case "Gradiente":           Block.TrackRef.NameOfGradientLine   = getNameFromMatch(kvp.Value, False) : CommentEnd = True
                                    Case "Regelprofilbereich":  Block.TrackRef.NameOfRoadSections   = getNameFromMatch(kvp.Value, False) : CommentEnd = True
                                    Case "Tunnelprofilbereich": Block.TrackRef.NameOfTunnelSections = getNameFromMatch(kvp.Value, False) : CommentEnd = True
                                    Case "Profilpunktbereich":  Block.TrackRef.NameOfSectionPoints  = getNameFromMatch(kvp.Value, False) : CommentEnd = True
                                    Case "Gleisprofilbereich":  Block.TrackRef.NameOfRailSections   = getNameFromMatch(kvp.Value, False) : CommentEnd = True
                                    Case "DGM":                 Block.TrackRef.NameOfDTM            = getNameFromMatch(kvp.Value, False) : CommentEnd = True
                                    
                                    Case "Feldnamen":           Block.BlockType.FieldNames          = kvp.Value                          : CommentEnd = True  ' Format "A0"
                                    
                                End Select
                            End If
                            
                            If (Not CommentEnd) Then
                                If (Comment.Length > 0) Then Comment.AppendLine()
                                Comment.Append(DataLine.Comment)
                            End If
                        End If
                    Else
                        ' First Data Line.
                        SourceBlock.HasData = True
                        SourceBlock.DataStartIndex = i
                    End If
                    
                    i += 1
                Loop Until (SourceBlock.HasData OrElse (i > SourceBlock.EndIndex))
                
                Block.Comment = Comment.ToString()
                Block.Points.Header.Add(Block.Comment)
                
                ' Set record definition. Only now, because for A0 the format and field names are required.
                SourceBlock.RecordDefinition = getIGeoRecordDefinition(Block.BlockType)
                
                Logger.logDebug(sprintf("  Name of Alignment    : %s", Block.TrackRef.NameOfAlignment))
                Logger.logDebug(sprintf("  Name of Km-Alignment : %s", Block.TrackRef.NameOfKmAlignment))
                Logger.logDebug(sprintf("  Name of Gradient Line: %s", Block.TrackRef.NameOfGradientLine))
                If (SourceBlock.HasData) Then Logger.logDebug(sprintf("  First Data Line      : %d", DataLines(SourceBlock.DataStartIndex).SourceLineNo))
            End Sub
            
            ''' <summary> Sets the definitions for Verm.esn source records. </summary>
            Private Sub setRecordDefinitionsVermEsn()
                Dim BlockType As New TcBlockType()
                BlockType.Program = TcBlockProgram.VermEsn
                
                ' Column definitions are zero-ased!
                
                ' THW, outdated
                BlockType.Format  = TcBlockFormat.THW
                BlockType.Version = TcBlockVersion.Outdated
                VermEsnRecordDefinitions.Add(getKeyForRecordDefinition(BlockType), New TcRecordDefinitionVermEsn With {
                    .ID_Length = 6,
                    .ID   = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
                                                               DataFieldPositionType.ColumnAndLength, 0, 7),
                    _
                    .Y    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y,
                                                               DataFieldPositionType.ColumnAndLength, 7, 13,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .X    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X,
                                                               DataFieldPositionType.ColumnAndLength, 20, 13,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .Z    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z,
                                                               DataFieldPositionType.ColumnAndLength, 48, 12,
                                                               DataFieldOptions.ZeroAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .Com  = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com,
                                                               DataFieldPositionType.ColumnAndLength, 80, 15,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Com2 = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com2,
                                                               DataFieldPositionType.ColumnAndLength, 60, 15,
                                                               DataFieldOptions.NotRequired),
                    _
                    .St   = Nothing,
                    _
                    .Km   = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                                  DataFieldPositionType.ColumnAndLength, 7, 12),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 19, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 72, 7,
                                                               DataFieldOptions.NonNumericAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .HSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK,
                                                               DataFieldPositionType.ColumnAndLength, 63, 8,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Ra   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ra,
                                                               DataFieldPositionType.ColumnAndLength, 38, 11),
                    _
                    .Ri   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ri,
                                                               DataFieldPositionType.ColumnAndLength, 28, 10),
                    _
                    .Ueb  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb,
                                                               DataFieldPositionType.ColumnAndLength, 49, 6,
                                                               DataFieldOptions.NotRequired Or DataFieldOptions.MissingAsZero),
                    _
                    .ZSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZSOK,
                                                               DataFieldPositionType.ColumnAndLength, 55, 8,
                                                               DataFieldOptions.NotRequired)
                    })
                '
                ' THW, current
                BlockType.Format  = TcBlockFormat.THW
                BlockType.Version = TcBlockVersion.Current
                VermEsnRecordDefinitions.Add(getKeyForRecordDefinition(BlockType), New TcRecordDefinitionVermEsn With {
                    .ID_Length  = 7,
                    .ID   = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
                                                               DataFieldPositionType.ColumnAndLength, 0, 8),
                    _
                    .Y    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y,
                                                               DataFieldPositionType.ColumnAndLength, 8, 14,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .X    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X,
                                                               DataFieldPositionType.ColumnAndLength, 22, 14,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .Z    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z,
                                                               DataFieldPositionType.ColumnAndLength, 51, 12,
                                                               DataFieldOptions.ZeroAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .Com  = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com,
                                                               DataFieldPositionType.ColumnAndLength, 82, 16,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Com2 = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com2,
                                                               DataFieldPositionType.ColumnAndLength, 63, 15,
                                                               DataFieldOptions.NotRequired),
                    _
                    .St   = Nothing,
                    _
                    .Km   = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                                  DataFieldPositionType.ColumnAndLength, 8, 12),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 20, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 74, 7,
                                                               DataFieldOptions.NonNumericAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .HSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK,
                                                               DataFieldPositionType.ColumnAndLength, 65, 8,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Ra   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ra,
                                                               DataFieldPositionType.ColumnAndLength, 40, 11),
                    _
                    .Ri   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ri,
                                                               DataFieldPositionType.ColumnAndLength, 29, 10),
                    _
                    .Ueb  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb,
                                                               DataFieldPositionType.ColumnAndLength, 51, 6),
                    _
                    .ZSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZSOK,
                                                               DataFieldPositionType.ColumnAndLength, 57, 8,
                                                               DataFieldOptions.NotRequired)
                    })
                '
                ' D3, outdated
                BlockType.Format  = TcBlockFormat.D3
                BlockType.Version = TcBlockVersion.Outdated
                VermEsnRecordDefinitions.Add(getKeyForRecordDefinition(BlockType), New TcRecordDefinitionVermEsn With {
                    .ID_Length  = 6,
                    .ID   = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
                                                               DataFieldPositionType.ColumnAndLength, 0, 7),
                    _
                    .Y    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y,
                                                               DataFieldPositionType.ColumnAndLength, 7, 13,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .X    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X,
                                                               DataFieldPositionType.ColumnAndLength, 20, 13,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .Z    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z,
                                                               DataFieldPositionType.ColumnAndLength, 49, 11,
                                                               DataFieldOptions.ZeroAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .Com  = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com,
                                                               DataFieldPositionType.ColumnAndLength, 80, 15,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Com2 = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com2,
                                                               DataFieldPositionType.ColumnAndLength, 60, 15,
                                                               DataFieldOptions.NotRequired),
                    _
                    .St   = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_St,
                                                                  DataFieldPositionType.ColumnAndLength, 33, 16),
                    _
                    .Km   = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                                  DataFieldPositionType.ColumnAndLength, 7, 15),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 22, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 74, 6,
                                                               DataFieldOptions.NonNumericAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .HSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK,
                                                               DataFieldPositionType.ColumnAndLength, 65, 8,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Ra   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ra,
                                                               DataFieldPositionType.ColumnAndLength, 40, 11),
                    _
                    .Ri   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ri,
                                                               DataFieldPositionType.ColumnAndLength, 31, 9),
                    _
                    .Ueb  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb,
                                                               DataFieldPositionType.ColumnAndLength, 51, 6),
                    _
                    .ZSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZSOK,
                                                               DataFieldPositionType.ColumnAndLength, 57, 8,
                                                               DataFieldOptions.NotRequired)
                    })
                '
                ' D3, current
                BlockType.Format  = TcBlockFormat.D3
                BlockType.Version = TcBlockVersion.Current
                VermEsnRecordDefinitions.Add(getKeyForRecordDefinition(BlockType), New TcRecordDefinitionVermEsn With {
                    .ID_Length  = 7,
                    .ID   = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
                                                               DataFieldPositionType.ColumnAndLength, 0, 8),
                    _
                    .Y    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y,
                                                               DataFieldPositionType.ColumnAndLength, 8, 14,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .X    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X,
                                                               DataFieldPositionType.ColumnAndLength, 22, 14,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .Z    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z,
                                                               DataFieldPositionType.ColumnAndLength, 52, 11,
                                                               DataFieldOptions.ZeroAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .Com  = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com,
                                                               DataFieldPositionType.ColumnAndLength, 86, 16,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Com2 = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Com2,
                                                               DataFieldPositionType.ColumnAndLength, 63, 15,
                                                               DataFieldOptions.NotRequired),
                    _
                    .St   = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_St,
                                                                 DataFieldPositionType.ColumnAndLength, 36, 16),
                    _
                    .Km   = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                                  DataFieldPositionType.ColumnAndLength, 8, 15),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 23, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 78, 7,
                                                               DataFieldOptions.NonNumericAsNaN Or DataFieldOptions.NotRequired),
                    _
                    .HSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK,
                                                               DataFieldPositionType.ColumnAndLength, 69, 8,
                                                               DataFieldOptions.NotRequired),
                    _
                    .Ra   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ra,
                                                               DataFieldPositionType.ColumnAndLength, 43, 11),
                    _
                    .Ri   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ri,
                                                               DataFieldPositionType.ColumnAndLength, 32, 11),
                    _
                    .Ueb  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb,
                                                               DataFieldPositionType.ColumnAndLength, 54, 7),
                    _
                    .ZSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_ZSOK,
                                                               DataFieldPositionType.ColumnAndLength, 61, 8,
                                                               DataFieldOptions.NotRequired)
                    })
            End Sub
            
            ''' <summary> Sets the definitions for iGeo/iTrassePC source records. </summary>
            Private Sub setRecordDefinitionsIGeo()
                Dim BlockType As New TcBlockType()
                
                ' Column definitions are zero-ased!
                
                ' iGeo A0: will be created uniqely for every A0 block (see getIGeoRecordDefinition() )
                
                ' iGeo + iTrassePC, A1
                BlockType.Format  = TcBlockFormat.A1
                BlockType.Version = TcBlockVersion.Current
                Dim A1RecordDef As New TcRecordDefinitionIGeo(InitIgnore:=True) With {
                    _
                    .LineEndCommentToken = "#",
                    _
                    .ID   = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
                                                               DataFieldPositionType.WordNumber, 1, 0),
                    _
                    .Y    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y,
                                                               DataFieldPositionType.WordNumber, 2, 0),
                    _
                    .X    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X,
                                                               DataFieldPositionType.WordNumber, 3, 0),
                    _
                    .Z    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z,
                                                               DataFieldPositionType.WordNumber, 4, 0,
                                                               DataFieldOptions.ZeroAsNaN),
                    _
                    .St   = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_St,
                                                               DataFieldPositionType.WordNumber, 5, 0),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.WordNumber, 6, 0),
                    _
                    .HSOK = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK,
                                                               DataFieldPositionType.WordNumber, 7, 0),
                    _
                    .Text = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Text,
                                                               DataFieldPositionType.WordNumber, 8, 0,
                                                               DataFieldOptions.NotRequired)
                    }
                BlockType.Program = TcBlockProgram.iGeo
                iGeoRecordDefinitions.Add(getKeyForRecordDefinition(BlockType), A1RecordDef)
                BlockType.Program = TcBlockProgram.iTrassePC
                iGeoRecordDefinitions.Add(getKeyForRecordDefinition(BlockType), A1RecordDef)
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
