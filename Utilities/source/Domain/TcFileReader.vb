
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Math
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions

Imports Rstyx.Utilities
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.Validation

Namespace Domain
    
    ''' <summary>  Represents a reader that can read and cache points with track geometry coordinates (TC) of a text file using <see cref="DataTextFileReader"/>. </summary>
     ''' <remarks>
     ''' <list type="bullet">
     ''' <listheader> <description> <b>General Features:</b> </description></listheader>
     ''' <item> The <see cref="M:Load"/> method clears all results before loading the given file. </item>
     ''' <item> File header lines will be provided by the <see cref="P:Header"/> property. </item>
     ''' <item> Found TC blocks will be provided by the <see cref="P:Blocks"/> property. </item>
     ''' <item> Parsing errors will be provided by the <see cref="P:ParseErrors"/> property. </item>
     ''' </list>
     ''' <para>
     ''' The track geometry coordinates will be expected as output blocks of following programs and types:
     ''' </para>
     ''' <list type="table">
     ''' <listheader> <term> <b>Program</b> </term>  <description> Output Type </description></listheader>
     ''' <item> <term> Verm.esn (Version 6.22) </term>  <description> "Umformung", from THW or D3 module (one-line or two-line records) </description></item>
     ''' <item> <term> Verm.esn (Version 8.40) </term>  <description> "Umformung", from THW or D3 module (one-line or two-line records) </description></item>
     ''' <item> <term> iTrassePC (Version 2.0) </term>  <description> "A1", "A5" </description></item>
     ''' <item> <term> iGeo (Version 1.1)      </term>  <description> "A1", "A5" </description></item>
     ''' </list>
     ''' <list type="bullet">
     ''' <listheader> <description> <b>Restrictions to the input file</b> </description></listheader>
     ''' <item> All supported block types (and only such!) may appear mixed in one file in arbitrary order. </item>
     ''' <item> All comment lines, immediately before the programs original output, will be associated to this block. </item>
     ''' <item> Comment lines at file start, that has not been associated to a block, will be treated as file header. </item>
     ''' <item> Other comment lines or empty lines will be ignored. </item>
     ''' <item> Every block ends one line before the next block starts. The last block ends at file end. </item>
     ''' </list>
     ''' </remarks>
    Public Class TcFileReader
        
        #Region "Private Fields"
            
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.TCFileReader")
            
            Private SourceBlocks            As New Collection(Of TcSourceBlockInfo)
            Private VermEsnRecordDefinitions As New Dictionary(Of String, TcRecordDefinitionVermEsn)  ' Key = getKeyForRecordDefinition(TcBlockType)
            
            Private _Blocks                 As New Collection(Of TcBlock)
            Private _Header                 As New Collection(Of String)
            Private _FilePath               As String
            Private _ParseErrors            As New ParseErrorCollection()
            
            Private _CommentLinesCount      As Long
            Private _DataLinesCount         As Long
            Private _EmptyLinesCount        As Long
            Private _TotalLinesCount        As Long
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new instance of TCFileReader with default settings. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new instance of TCFileReader with specified settings. </summary>
             ''' <param name="CantBase">           The base length for cant. Must be greater than zero. </param>
             ''' <param name="StationAsKilometer"> If <see langword="true"/> the station value may be used also as kilometer. </param>
             ''' <exception cref="System.ArgumentException"> <paramref name="CantBase"/> isn't > 0. </exception>
            Public Sub New(CantBase As Double, StationAsKilometer As Boolean)
                
                If (Not (CantBase > 0)) Then  Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.Global_ValueNotGreaterThanZero, "CantBase")
                
                Me.CantBase = CantBase
                Me.StationAsKilometer = StationAsKilometer
                
                setRecordDefinitionsVermEsn()
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
                Public ReadOnly Property Blocks() As Collection(Of TcBlock)
                    Get
                        Return _Blocks
                    End Get
                End Property
                
                ''' <summary> Returns the path to the last read file. </summary>
                Public ReadOnly Property FilePath() As String
                    Get
                        Return _FilePath
                    End Get
                End Property
                
                ''' <summary> Returns the Header lines of the text file. </summary>
                 ''' <remarks> These are all leading comment lines that don't semm to belong to an output block of iTrassePC. </remarks>
                Public ReadOnly Property Header() As Collection(Of String)
                    Get
                        Return _Header
                    End Get
                End Property
                
                ''' <summary> Returns the errors occurred while parsing the input file. </summary>
                Public ReadOnly Property ParseErrors() As ParseErrorCollection
                    Get
                        Return _ParseErrors
                    End Get
                End Property
                
            #End Region
            
            #Region "Statistics"
                
                ''' <summary> Returns the count of comment lines. </summary>
                Public ReadOnly Property CommentLinesCount() As Long
                    Get
                        Return _CommentLinesCount
                    End Get
                End Property
                
                ''' <summary> Returns the count of lines containing data. </summary>
                Public ReadOnly Property DataLinesCount() As Long
                    Get
                        Return _DataLinesCount
                    End Get
                End Property
                
                ''' <summary> Returns the count of empty lines. </summary>
                Public ReadOnly Property EmptyLinesCount() As Long
                    Get
                        Return _EmptyLinesCount
                    End Get
                End Property
                
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
            
            ' Wrapped exceptions:
             ' <exception cref="System.ArgumentException">             <paramref name="Path"/> is empty. </exception>
             ' <exception cref="System.ArgumentNullException">         <paramref name="Path"/> or <paramref name="Encoding"/> is <see langword="null"/>. </exception>
             ' <exception cref="System.IO.FileNotFoundException">      The file cannot be found. </exception>
             ' <exception cref="System.IO.DirectoryNotFoundException"> The specified path is invalid, such as being on an unmapped drive. </exception>
             ' <exception cref="System.NotSupportedException">         <paramref name="Path"/> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
             ' <exception cref="System.ArgumentOutOfRangeException">   <paramref name="BufferSize"/> is less than or equal to zero. </exception>
             ' <exception cref="System.OutOfMemoryException">          There is insufficient memory to allocate a buffer for the returned string. </exception>
             ' <exception cref="System.IO.IOException">                An I/O error occurs. </exception>
            ''' <summary> Loads the file using default settings for <see cref="StreamReader"/>. </summary>
             ''' <param name="Path"> The complete path of to the TC file to be read (for <see cref="StreamReader"/>). </param>
             ''' <remarks>
             ''' <para>
             ''' The default settings for <see cref="StreamReader"/> are: UTF-8, not detect encoding, buffersize 1024.
             ''' </para>
             ''' <para>
             ''' The loaded data will be provided by the <see cref="P:TcFileReader.Blocks"/> and <see cref="P:TcFileReader.Header"/> properties,
             ''' which are cleared before.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="RemarkException"> Wraps any exception. </exception>
            Public Sub Load(Path As String)
                Me.Load(Path, Encoding.UTF8, DetectEncodingFromByteOrderMarks:=False, BufferSize:=1024)
            End Sub
            
            ' Wrapped exceptions:
             ' <exception cref="System.ArgumentException">             <paramref name="Path"/> is empty. </exception>
             ' <exception cref="System.ArgumentNullException">         <paramref name="Path"/> or <paramref name="Encoding"/> is <see langword="null"/>. </exception>
             ' <exception cref="System.IO.FileNotFoundException">      The file cannot be found. </exception>
             ' <exception cref="System.IO.DirectoryNotFoundException"> The specified path is invalid, such as being on an unmapped drive. </exception>
             ' <exception cref="System.NotSupportedException">         <paramref name="Path"/> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
             ' <exception cref="System.ArgumentOutOfRangeException">   <paramref name="BufferSize"/> is less than or equal to zero. </exception>
             ' <exception cref="System.OutOfMemoryException">          There is insufficient memory to allocate a buffer for the returned string. </exception>
             ' <exception cref="System.IO.IOException">                An I/O error occurs. </exception>
            ''' <summary> Loads the file using specified settings for the used <see cref="StreamReader"/>. </summary>
             ''' <param name="Path">                             The complete path to the data text file to be read. </param>
             ''' <param name="Encoding">                         The character encoding to use. </param>
             ''' <param name="DetectEncodingFromByteOrderMarks"> Indicates whether to look for byte order marks at the beginning of the file. </param>
             ''' <param name="BufferSize">                       The minimum buffer size, in number of 16-bit characters. </param>
             ''' <remarks>
             ''' The loaded data will be provided by the <see cref="P:TcFileReader.Blocks"/> and <see cref="P:TcFileReader.Header"/> properties,
             ''' which are cleared before. Same for  <see cref="P:TcFileReader.ParseErrors"/>.
             ''' </remarks>
             ''' <exception cref="RemarkException"> Wraps any exception. </exception>
            Public Sub Load(Path As String,
                            Encoding As Encoding,
                            DetectEncodingFromByteOrderMarks As Boolean,
                            BufferSize As Integer
                           )
                Try
                    Dim SplittedLine    As PreSplittedTextLine
                    Dim kvp             As KeyValuePair(Of String, String)
                    Dim SourceBlock     As TcSourceBlockInfo
                    Dim TcBlock         As TcBlock
                    Dim BlockCount      As Integer = 0
                    
                    ' Read the file and cache pre-split lines. Don't recognize a header because the general logic doesn't match to TC file.
                    Logger.logDebug(StringUtils.sprintf("Load TC file '%s'", Path))
                    Dim FileReader As New DataTextFileReader()
                    FileReader.SeparateHeader = False
                    FileReader.Load(Path, Encoding, DetectEncodingFromByteOrderMarks, BufferSize)
                    
                    ' Reset results and settings.
                    Me.Reset(Path)
                    
                    Logger.logDebug(" Looking for block beginnings...")
                    ' Find and store source block beginnings (still without EndIndex, Version and Format).
                    For i As Integer = 0 To FileReader.DataCache.Count - 1
                        
                        SplittedLine = FileReader.DataCache(i)
                        
                        If (SplittedLine.IsCommentLine) Then
                            ' Comment line: Look for output header of iGeo and iTrassePC.
                            If (SplittedLine.HasComment) Then
                                kvp = splitHeaderLineIGeo(SplittedLine.Comment)
                                If (kvp.Key IsNot Nothing) Then
                                    If ((kvp.Key = "Programm") AndAlso ((kvp.Value = "iTrassePC") Or (kvp.Value = "iGeo"))) Then
                                        ' Create and store source block info.
                                        SourceBlock = New TcSourceBlockInfo()
                                        SourceBlock.BlockType.Version = TcBlockVersion.Current
                                        SourceBlock.BlockType.Program = If((kvp.Value = "iGeo"), TcBlockProgram.iGeo, TcBlockProgram.iTrassePC)
                                        SourceBlock.StartIndex = findStartOfBlock(FileReader.DataCache, i)
                                        SourceBlocks.Add(SourceBlock)
                                        BlockCount += 1
                                        Logger.logDebug(StringUtils.sprintf("  Found %d. block: from %s, start index=%d, indicated at index=%d", BlockCount, SourceBlock.BlockType.Program, SourceBlock.StartIndex, i))
                                    End If
                                End If
                            End If
                        ElseIf (SplittedLine.HasData) Then
                            ' Line contains data and maybe line end comment.
                            If (SplittedLine.Data.IsMatchingTo("^(Trassenumformung|Umformung")) Then
                                SourceBlock = New TcSourceBlockInfo()
                                SourceBlock.BlockType.Program = TcBlockProgram.VermEsn
                                SourceBlock.StartIndex = findStartOfBlock(FileReader.DataCache, i)
                                SourceBlocks.Add(SourceBlock)
                                BlockCount += 1
                                Logger.logDebug(StringUtils.sprintf("  Found %d. block: from %s, start index=%d, indicated at index=%d", BlockCount, SourceBlock.BlockType.Program, SourceBlock.StartIndex, i))
                            End If
                        End If
                    Next
                    
                    ' Store block end lines (the line before the next block start).
                    For i As Integer = 0 To SourceBlocks.Count - 1
                        If (i = (SourceBlocks.Count - 1)) Then
                            SourceBlocks(i).EndIndex = FileReader.DataCache.Count - 1
                        Else
                            SourceBlocks(i).EndIndex = SourceBlocks(i + 1).StartIndex - 1
                        End If
                    Next
                    
                    Logger.logDebug(" Looking for file header...")
                    ' Find and store file header lines (all comment lines from file start until the first non comment line or block start).
                    Dim MaxIndexToLook As Integer = If((SourceBlocks.Count > 0), SourceBlocks(0).StartIndex - 1, FileReader.DataCache.Count - 1)
                    For i As Integer = 0 To MaxIndexToLook
                        If (Not FileReader.DataCache(i).IsCommentLine) Then
                            Exit For
                        Else
                            Me.Header.Add(FileReader.DataCache(i).Comment)
                        End If
                    Next
                    
                    Logger.logDebug(" Read blocks...")
                    ' Read every block with all it's data and, if successfull, store it to Me.Blocks.
                    For i As Integer = 0 To SourceBlocks.Count - 1
                        
                        Select Case SourceBlocks(i).BlockType.Program
                            
                            Case TcBlockProgram.VermEsn
                                TcBlock = readBlockVermEsn(FileReader.DataCache, SourceBlocks(i))
                                If (TcBlock.IsValid) Then Me.Blocks.Add(TcBlock)
                                
                            'Case TcBlockProgram.iGeo, TcBlockProgram.iTrassePC 
                            '    Me.Blocks.Add(ReadBlockIGeo(SourceBlocks(i)))
                        End Select
                    Next
                    
                    ' Log parsing errors and warnings.
                    If (Me.ParseErrors.HasErrors OrElse Me.ParseErrors.HasWarnings) Then Me.ParseErrors.ToLoggingConsole()
                    
                    ' Throw exception if parsing errors has occurred.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New RemarkException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                    ElseIf (Me.TotalPointCount = 0) Then
                        Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_NoPoints, Me.FilePath))
                    End If
                    
                Catch ex As System.Exception
                    Throw New RemarkException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_LoadFailed, Me.FilePath), ex)
                End Try
                    
            End Sub
            
            ''' <summary> Resets this <see cref="TcFileReader"/>. </summary>
            Public Sub Reset()
                Me.Reset(Nothing)
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <inheritdoc/>
            Public Overrides Function ToString() As String
                Dim List As New StringBuilder()
                
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToString_File  , Me.FilePath))
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToString_Errors, Me.ParseErrors.Count))
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_ToString_Blocks, Me.Blocks.Count))
                
                For i As Integer = 0 To Me.Blocks.Count - 1
                    List.AppendLine()
                    List.AppendLine(Me.Blocks(i).ToString())
                Next
                
                Return List.ToString()
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
                
                ''' <summary> Output format "A1" of iGeo or iTrassePC. </summary>
                A1  = 1
                
                ''' <summary> Output format "A5" of iGeo or iTrassePC. </summary>
                A5  = 2
                
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
                
            End Enum
            
            ''' <summary> The type description of a block of points with track geometry coordinates. </summary>
            Public Class TcBlockType
                Inherits Cinch.ValidatingObject
                
                #Region "Private Fields"
                    
                    'Private Shared Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Microstation.AddIn.MetaData.TextFieldDefinition")
                    
                    Private Shared MissingProgramRule   As Cinch.SimpleRule
                    Private Shared MissingVersionRule   As Cinch.SimpleRule
                    Private Shared MissingFormatRule    As Cinch.SimpleRule
                    Private Shared MissingSubFormatRule As Cinch.SimpleRule
                    Private Shared FormatMismatchRule   As DelegateRule
                    
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
                        MissingFormatRule = New Cinch.SimpleRule("Format",
                                                                 Rstyx.Utilities.Resources.Messages.TcBlockType_MissingFormat,
                                                                 Function (oValidatingObject As Object) (DirectCast(oValidatingObject, TcBlockType).Format = TcBlockFormat.None))
                        '
                        MissingSubFormatRule = New Cinch.SimpleRule("SubFormat",
                                                                    Rstyx.Utilities.Resources.Messages.TcBlockType_MissingSubFormat,
                                                                    Function (oValidatingObject As Object) As Boolean
                                                                        Dim IsValidRule  As Boolean = True
                                                                        Dim BlockType  As TcBlockType = DirectCast(oValidatingObject, TcBlockType)
                                                                        
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
                                                                  
                                                                  Select Case BlockType.Program
                                                                      
                                                                      Case TcBlockProgram.VermEsn
                                                                          Select Case BlockType.Format
                                                                              Case TcBlockFormat.D3, TcBlockFormat.THW
                                                                                  'o.k.
                                                                              Case Else
                                                                                  IsValidRule = False
                                                                                  BrokenMessage = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcBlockType_FormatMismatch, BlockType.Format, BlockType.Program)
                                                                          End Select
                                                                          
                                                                      Case TcBlockProgram.iGeo, TcBlockProgram.iTrassePC
                                                                          Select Case BlockType.Format
                                                                              Case TcBlockFormat.A1, TcBlockFormat.A5
                                                                                  'o.k.
                                                                              Case Else
                                                                                  IsValidRule = False
                                                                                  BrokenMessage = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcBlockType_FormatMismatch, BlockType.Format, BlockType.Program)
                                                                          End Select
                                                                  End Select
                                                                  
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
                        ElseIf(Me.Format = TcBlockFormat.A5) Then
                            RetValue &= " (Format A5)"
                        Else
                            RetValue &= " (Format ??)"
                        End If
                        
                    ElseIf (Me.Program = TcBlockProgram.iGeo) Then
                        RetValue = "iGeo"
                        
                        If (Me.Version = TcBlockVersion.Current) Then
                            RetValue &= " 1.1"
                        Else
                            RetValue &= " v??" 
                        End If
                        
                        If (Me.Format = TcBlockFormat.A1) Then
                            RetValue &= " (Format A1)"
                        ElseIf(Me.Format = TcBlockFormat.A5) Then
                            RetValue &= " (Format A5)"
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
                            RetValue &= " (Format D3 / "
                        ElseIf (Me.Format = TcBlockFormat.THW) Then
                            RetValue &= " (Format THW / "
                        Else
                            RetValue &= " (Format ?? / "
                        End If
                        
                        If (Me.Format = TcBlockSubFormat.OneLine) Then
                            RetValue &= " einzeilig)"
                        ElseIf (Me.Format = TcBlockSubFormat.TwoLine) Then
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
                    
                    ''' <summary> A description of this block. </summary>
                    Public Property Description()       As String = String.Empty
                    
                    ''' <summary> The point list of this block. </summary>
                    Public Property Points()            As New IDCollection(Of GeoTcPoint)
                    
                    ''' <summary> Determines the reference frame of the track geometry. </summary>
                    Public Property TrackRef()          As New TrackGeometryInfo()
                    
                    ''' <summary> Determines the file source. </summary>
                    Public Property Source()            As New DataBlockFileSourceInfo()
                    
                #End Region
                
                #Region "Overrides"
                    
                    ''' <inheritdoc/>
                    Public Overrides Function ToString() As String
                        Dim List As New StringBuilder()
                        
                        List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToString_Description, Me.Description))
                        List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToString_Type       , Me.BlockType.ToString()))
                        List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToString_Source     , Me.Source.ToString()))
                        List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToString_TrackRef   , Me.TrackRef.ToString()))
                        List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcBlock_ToString_Points     , Me.Points.Count))
                        
                        List.AppendLine(Rstyx.Utilities.Resources.Messages.TcBlock_TableHeader.ToHeadLine("-", Padding:=False))
                        For i As Integer = 0 To Me.Points.Count - 1
                            List.AppendLine(StringUtils.sprintf("%5d %s", i + 1, Me.Points(i).ToString()))
                        Next
                        
                        Return List.ToString()
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
                    Return StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.DataBlockFileSourceInfo_ToString, Me.FilePath, Me.StartLineNo, Me, EndLineNo)
                End Function
                
            End Class
            
            
            ''' <summary> Info regarding source of every found TC block. </summary>
            Private Class TcSourceBlockInfo
                
                ''' <summary> Type of TC block. Only Program is determined yet! </summary>
                Public BlockType        As New TcBlockType()
                
                ''' <summary> Index into <see cref="P:DataTextFileReader.DataCache"/> representing the first line of this block. </summary>
                Public StartIndex       As Integer = -1
                
                ''' <summary> Index into <see cref="P:DataTextFileReader.DataCache"/> representing the first DATA line of this block. </summary>
                Public DataStartIndex   As Integer = -1
                
                ''' <summary> Index into <see cref="P:DataTextFileReader.DataCache"/> representing the last line of this block. </summary>
                Public EndIndex         As Integer = -1
                
            End Class
            
            ''' <summary> Definition of a Verm.esn TC source record. </summary>
            Private Class TcRecordDefinitionVermEsn
                
                Public ID_Factor   As Integer
                
                Public ID   As DataFieldDefinition(Of Double)
                Public Y    As DataFieldDefinition(Of Double)
                Public X    As DataFieldDefinition(Of Double)
                Public Z    As DataFieldDefinition(Of Double)
                Public Com  As DataFieldDefinition(Of String)
                Public Com2 As DataFieldDefinition(Of String)
                Public St   As DataFieldDefinition(Of Double)
                Public Km   As DataFieldDefinition(Of Double)
                Public Q    As DataFieldDefinition(Of Double)
                Public QKm  As DataFieldDefinition(Of Double)
                Public HSOK As DataFieldDefinition(Of Double)
                Public Ra   As DataFieldDefinition(Of Double)
                Public Ri   As DataFieldDefinition(Of Double)
                Public Ueb  As DataFieldDefinition(Of Double)
                Public ZSOK As DataFieldDefinition(Of Double)
                
            End Class
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Restets this <see cref="TcFileReader"/> and re-initializes it whith a new file path. </summary>
             ''' <param name="Path"> The complete path to the data text file to be read. </param>
            Private Sub Reset(Path As String)
                ' Clear results.
                SourceBlocks.Clear()
                Me.Header.Clear()
                Me.Blocks.Clear()
                Me.ParseErrors.Clear()
                
                ' Set file path.
                Me.ParseErrors.FilePath = Path
                _FilePath = Path
                
                ' Clear statistics.
                
            End Sub
            
            ''' <summary> Finds the start line of a given block. </summary>
             ''' <param name="SplitLines">        The data cache holding the block of interest. </param>
             ''' <param name="CurrentStartIndex"> Index into <paramref name="SplitLines"/> determining the line containing "Programm:..." or "[Trassen]umformung". </param>
             ''' <returns> Index into <paramref name="SplitLines"/> determining the line that should be the first line of the given block. </returns>
             ''' <remarks> 
             ''' Search backwards for block start.
             ''' All (and only) comment lines before the current line will belong to the block.
             ''' BUT: Avoid including a preceeding empty block of iGeo and iTrassePC by looking for matching tokens.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SplitLines"/> is <see langword="null"/>. </exception>
            Private Function findStartOfBlock(SplitLines As Collection(Of PreSplittedTextLine), CurrentStartIndex As Integer) As Integer
                
                If (SplitLines Is Nothing) Then Throw New System.ArgumentNullException("SplitLines")
                
                Dim RetValue            As Integer = 0
                Dim FoundStart          As Boolean = False
                Dim k                   As Integer = CurrentStartIndex - 1
                Dim kvp                 As KeyValuePair(Of String, String)
                Dim PrevSplitLine       As PreSplittedTextLine
                Dim SearchTerminators() As String = {"Überhöhungsband", "Gradiente", "Km-Linie", "Achse", "Checksumme"}
                
                Do While ((k > -1) AndAlso (Not FoundStart))
                    
                    PrevSplitLine = SplitLines(k)
                    
                    If (Not PrevSplitLine.IsCommentLine) Then
                        ' No comment line.
                        FoundStart = True
                        RetValue = k + 1
                    ElseIf (PrevSplitLine.HasComment) Then
                        ' Comment line which isn't empty.
                        If (PrevSplitLine.Comment.Trim().IsMatchingTo("^-+|PktNr")) Then
                            FoundStart = True
                            RetValue = k + 1
                        End If
                        
                        If (Not FoundStart) Then
                            kvp = splitHeaderLineIGeo(PrevSplitLine.Comment)
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
             ''' <returns> The found key/value pair. If not found, key and value are <see langword="null"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Comment"/> is <see langword="null"/>. </exception>
            Private Function splitHeaderLineIGeo(Comment As String) As KeyValuePair(Of String, String)
                
                If (Comment Is Nothing) Then Throw New System.ArgumentNullException("Comment")
                
                Dim RetValue As KeyValuePair(Of String, String) = Nothing
                
                Dim KeyValue() As String = Comment.Trim().Split(":"c)
                
                If (KeyValue.Length = 2) Then
                    RetValue = New KeyValuePair(Of String, String)(KeyValue(0).Trim(), KeyValue(1).Trim())
                'Else
                '    RetValue = New KeyValuePair(Of String, String)(Nothing, Nothing)
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Returns a key into the record definition dictionary. </summary>
             ''' <param name="BlockType"> The block type to get the key for. </param>
            Private Function getKeyForRecordDefinition(BlockType As TcBlockType) As String
                Return BlockType.Program.ToString & "_" & BlockType.Version.ToString & "_" & BlockType.Format.ToString
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
             ''' <param name="SplitLines">  The data cache holding the block of interest. </param>
             ''' <param name="SourceBlock"> Determines the block type and it's position in <paramref name="SplitLines"/>. </param>
             ''' <returns> The complete <see cref="TcBlock"/> read from <paramref name="SplitLines"/>. </returns>
             ''' <remarks>
             ''' <list type="bullet">
             ''' <listheader> <description> <b>Hints:</b> </description></listheader>
             ''' <item> The returned block <b>may be invalid!</b> (check for .IsValid!). </item>
             ''' <item> All parsing errors will be added to <see cref="P:ParseErrors"/> property. </item>
             ''' <item> The Points collection (<see cref="P:Points"/>) may be empty. </item>
             ''' </list>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SplitLines"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceBlock"/> is <see langword="null"/>. </exception>
            Private Function readBlockVermEsn(SplitLines As Collection(Of PreSplittedTextLine), SourceBlock As TcSourceBlockInfo) As TcBlock
                
                If (SplitLines Is Nothing) Then Throw New System.ArgumentNullException("SplitLines")
                If (SourceBlock Is Nothing) Then Throw New System.ArgumentNullException("SourceBlock")
                
                Logger.logDebug(StringUtils.sprintf(" Reading Verm.esn TC output block (start index = %d, end index = %d)", SourceBlock.StartIndex, SourceBlock.EndIndex))
                
                ' Create the block.
                Dim Block As New TcBlock()
                
                ' Parse header.
                findBlockMetaDataVermEsn(SplitLines, SourceBlock, Block)
                
                ' Read points.
                If (Not Block.IsValid) Then
                    Me.ParseErrors.AddError(SourceBlock.StartIndex, 0, 0, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_InvalidTcBlock, SourceBlock.StartIndex, SourceBlock.EndIndex, Block.Error))
                Else
                    Dim RecDef As TcRecordDefinitionVermEsn = VermEsnRecordDefinitions.Item(getKeyForRecordDefinition(Block.BlockType))
                    Dim i      As Integer = SourceBlock.DataStartIndex
                    
                    Do While (i <= SourceBlock.EndIndex)
                        Try
                            Dim SplitLine As PreSplittedTextLine = SplitLines(i)
                            
                            If (SplitLine.HasData) Then
                                
                                Dim Com2 As String = Nothing
                                Dim ID2  As String = Nothing
                                
                                Dim p    As New GeoTcPoint()
                                
                                ' Cartesian coordinates line.
                                If (Block.BlockType.SubFormat = TcBlockSubFormat.TwoLine) Then
                                    ID2  = StringUtils.sprintf("%d", SplitLine.ParseField(RecDef.ID).Value * RecDef.ID_Factor)
                                    p.Y  = SplitLine.ParseField(RecDef.Y).Value
                                    p.X  = SplitLine.ParseField(RecDef.X).Value
                                    p.Z  = SplitLine.ParseField(RecDef.Z).Value
                                    Com2 = SplitLine.ParseField(RecDef.Com2).Value
                                    If (RecDef.St IsNot Nothing) Then p.St = SplitLine.ParseField(RecDef.St).Value
                                    
                                    ' Switch to second line of this record.
                                    i += 1
                                    SplitLine = SplitLines(i)
                                    If (Not SplitLine.HasData) Then Throw New Rstyx.Utilities.IO.ParseException(New ParseError(ParseErrorLevel.Error, SplitLine.SourceLineNo, 0, 0, Rstyx.Utilities.Resources.Messages.TcFileReader_MissingSecondLine, Nothing))
                                End If
                                
                                ' Track coordinates line.
                                ' Point-ID.
                                p.ID = StringUtils.sprintf("%d", SplitLine.ParseField(RecDef.ID).Value * RecDef.ID_Factor)
                                If ((Block.BlockType.SubFormat = TcBlockSubFormat.TwoLine) AndAlso (Not (p.ID = ID2))) Then
                                    Throw New Rstyx.Utilities.IO.ParseException(New ParseError(ParseErrorLevel.Error, SplitLine.SourceLineNo, 0, 0, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_IDMismatch, ID2, p.ID), Nothing))
                                ElseIf (Block.Points.Contains(p.ID)) Then
                                    Throw New Rstyx.Utilities.IO.ParseException(New ParseError(ParseErrorLevel.Error, SplitLine.SourceLineNo, 0, 0, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_IDdoubled, p.ID), Nothing))
                                End If
                                
                                ' Track values.
                                p.Km   = SplitLine.ParseField(RecDef.Km).Value
                                p.Q    = SplitLine.ParseField(RecDef.Q).Value
                                p.Ra   = SplitLine.ParseField(RecDef.Ra).Value
                                p.Ri   = SplitLine.ParseField(RecDef.Ri).Value
                                p.Ueb  = SplitLine.ParseField(RecDef.Ueb).Value
                                p.ZSOK = SplitLine.ParseField(RecDef.ZSOK).Value
                                p.HSOK = SplitLine.ParseField(RecDef.HSOK).Value
                                p.QKm  = SplitLine.ParseField(RecDef.QKm).Value
                                
                                ' Point info and comment.
                                p.Info = SplitLine.ParseField(RecDef.Com).Value
                                If (p.Info.IsEmptyOrWhiteSpace()) Then p.Info = Com2
                                If (SplitLine.HasComment) Then p.Comment = SplitLine.Comment
                                
                                ' Other info.
                                p.ActualCant   = GeoMath.parseCant(p.Info)
                                p.CantBase     = Me.CantBase
                                p.SourceLineNo = SplitLine.SourceLineNo
                                p.TrackRef     = Block.TrackRef
                                
                                ' Calculate values not having read.
                                p.transformHorizontalToCanted()
                                
                                ' Add Point to the block.
                                Block.Points.Add(p)
                                
                                ' Switch to next record.
                                i += 1
                            End If
                        Catch ex As ParseException
                            Me.ParseErrors.Add(ex.ParseError)
                        End Try
                    Loop
                    
                    ' Warning for empty block.
                    If (Block.Points.Count = 0) Then
                        Me.ParseErrors.AddWarning(SourceBlock.StartIndex, 0, 0, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TcFileReader_EmptyTcBlock, SourceBlock.StartIndex, SourceBlock.EndIndex))
                    End If
                End If
                
                Return Block
            End Function
            
            ''' <summary> Finds meta data of a block of Verm.esn output ("Umformung"), especially block type and geometry reference info and first data line. </summary>
             ''' <param name="SplitLines">     [In]  The data cache holding the block of interest. </param>
             ''' <param name="SourceBlock">    [In/Out] Info about block source in <paramref name="SplitLines"/>. </param>
             ''' <param name="Block">          [Out] The TcBlock to fill with info. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SplitLines"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceBlock"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Block"/> is <see langword="null"/>. </exception>
            Private Sub findBlockMetaDataVermEsn(SplitLines As Collection(Of PreSplittedTextLine), ByRef SourceBlock As TcSourceBlockInfo, ByRef Block As TcBlock)
                
                If (SplitLines Is Nothing) Then Throw New System.ArgumentNullException("SplitLines")
                If (SourceBlock Is Nothing) Then Throw New System.ArgumentNullException("SourceBlock")
                If (Block Is Nothing) Then Throw New System.ArgumentNullException("Block")
                
                Dim SplittedLine    As PreSplittedTextLine
                Dim FullLine        As String
                Dim Pattern         As String
                Dim PathName        As String
                Dim oMatch          As Match
                Dim DataFound       As Boolean = False
                Dim FormatFound     As Boolean = False
                Dim i               As Integer = SourceBlock.StartIndex
                
                Block.BlockType.Program  = SourceBlock.BlockType.Program
                
                Block.Source.FilePath    = Me.FilePath
                Block.Source.StartLineNo = SplitLines(SourceBlock.StartIndex).SourceLineNo
                Block.Source.EndLineNo   = SplitLines(SourceBlock.EndIndex).SourceLineNo
                
                Logger.logDebug(" Find meta data of Verm.esn TC output block:")
                
                Do
                    SplittedLine = SplitLines(i)
                    FullLine = SplittedLine.getFullLine()
                    
                    If (Not FullLine.IsMatchingTo("^[0-9]")) Then
                        ' Header line.
                        
                        ' Check for: Format, Version and Alignement name.
                        If (Block.BlockType.Format = TcBlockFormat.None) Then
                            ' Try: THW, current version (alignment full path)
                            Pattern = "^\s*Trassenumformung\s+(.+?\.tra)\s+\("
                            oMatch = Regex.Match(FullLine, Pattern)
                            
                            If (oMatch.Success) Then
                                FormatFound = True
                                Block.BlockType.Version = TcBlockVersion.Current
                                Block.BlockType.Format  = TcBlockFormat.THW
                                PathName = oMatch.Groups(1).Value
                                Block.TrackRef.NameOfAlignment = getNameFromMatch(PathName, False)
                            End If
                        End If
                        If (Block.BlockType.Format = TcBlockFormat.None) Then
                            ' Try: THW, outdated version (DOS, alignment name without path and without spaces!)
                            Pattern = "^\s*Trassenumformung\s+(.+?)\s+\("
                            oMatch = Regex.Match(FullLine, Pattern)
                            
                            If (oMatch.Success) Then
                                FormatFound = True
                                Block.BlockType.Version = TcBlockVersion.Outdated
                                Block.BlockType.Format  = TcBlockFormat.THW
                                Block.TrackRef.NameOfAlignment = oMatch.Groups(1).Value
                            End If
                        End If
                        If (Block.BlockType.Format = TcBlockFormat.None) Then
                            ' Try: D3, both versions.
                            Pattern = "^\s*Umformung\s+(.+)\s+\("
                            oMatch = Regex.Match(FullLine, Pattern)
                            
                            If (oMatch.Success) Then
                                FormatFound = True
                                Block.BlockType.Format = TcBlockFormat.D3
                                PathName = oMatch.Groups(1).Value
                                
                                ' Version.
                                If (PathName.Contains(Path.PathSeparator)) Then
                                    ' Current version (D3 project full path).
                                    Block.BlockType.Version = TcBlockVersion.Current
                                Else
                                    ' Outdated version (No D3 project name/path, only internal line name).
                                    Block.BlockType.Version = TcBlockVersion.Outdated
                                End If
                                
                                ' Alignment name.
                                Block.TrackRef.NameOfAlignment = getNameFromMatch(PathName, True)
                                
                            End If
                        End If
                        
                        ' Check for: Gradient and Km name.
                        If (Block.BlockType.Format = TcBlockFormat.THW) Then
                            
                            Pattern = "^\s*(.*?)\s*:\s*(\S*.*\S+)\s*"
                            oMatch = Regex.Match(FullLine, Pattern)
                            
                            If (oMatch.Success) Then
                                Dim Key   As String  = oMatch.Groups(1).Value
                                Dim Value As String  = oMatch.Groups(2).Value
                                Select Case Key
                                    Case "Gradiente":            Block.TrackRef.NameOfGradientLine = getNameFromMatch(oMatch.Groups(1).Value, False)
                                    Case "Reduktion auf Trasse": Block.TrackRef.NameOfKmAlignment  = getNameFromMatch(oMatch.Groups(1).Value, False)
                                End Select
                            End If
                        End If
                        
                    Else
                        ' First Data Line => Check for sub format.
                        DataFound = True
                        SourceBlock.DataStartIndex = i
                        
                        ' Determine sub format.
                        Block.BlockType.SubFormat = TcBlockSubFormat.OneLine
                        If ((i < SourceBlock.EndIndex) AndAlso (SplitLines(i + 1).HasData)) Then
                            Dim IDLength As Integer = VermEsnRecordDefinitions(getKeyForRecordDefinition(Block.BlockType)).ID.Length
                            Dim ID1      As String  = SplittedLine.Data.Left(IDLength)
                            Dim ID2      As String  = SplitLines(i + 1).Data.Left(IDLength)
                            If (ID1 = ID2) Then
                                Block.BlockType.SubFormat = TcBlockSubFormat.TwoLine
                            End If
                        End If
                    End If
                    
                    i += 1
                Loop Until (DataFound OrElse (i > SourceBlock.EndIndex))
                
                Logger.logDebug(StringUtils.sprintf("  Name of Alignment    : %s", Block.TrackRef.NameOfAlignment))
                Logger.logDebug(StringUtils.sprintf("  Name of Km-Alignment : %s", Block.TrackRef.NameOfKmAlignment))
                Logger.logDebug(StringUtils.sprintf("  Name of Gradient Line: %s", Block.TrackRef.NameOfGradientLine))
                Logger.logDebug(StringUtils.sprintf("  First Data Line      : %s", SplitLines(SourceBlock.DataStartIndex).SourceLineNo))
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
                    .ID_Factor = 10000,
                    .ID   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
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
                    .Km   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                               DataFieldPositionType.ColumnAndLength, 7, 12),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 19, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 72, 7,
                                                               DataFieldOptions.NotRequired),
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
                                                               DataFieldPositionType.ColumnAndLength, 49, 6),
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
                    .ID_Factor  = 100000,
                    .ID   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
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
                    .Km   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                               DataFieldPositionType.ColumnAndLength, 8, 12),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 20, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 74, 7,
                                                               DataFieldOptions.NotRequired),
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
                    .ID_Factor  = 10000,
                    .ID   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
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
                    .St   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_St,
                                                               DataFieldPositionType.ColumnAndLength, 33, 16,
                                                               DataFieldOptions.AllowKilometerNotation),
                    _
                    .Km   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                               DataFieldPositionType.ColumnAndLength, 7, 17),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 22, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 74, 6,
                                                               DataFieldOptions.NotRequired),
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
                    .ID_Factor  = 100000,
                    .ID   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID,
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
                    .St   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_St,
                                                               DataFieldPositionType.ColumnAndLength, 36, 16,
                                                               DataFieldOptions.AllowKilometerNotation),
                    _
                    .Km   = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km,
                                                               DataFieldPositionType.ColumnAndLength, 8, 15),
                    _
                    .Q    = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Q,
                                                               DataFieldPositionType.ColumnAndLength, 23, 9,
                                                               DataFieldOptions.IgnoreLeadingAsterisks),
                    _
                    .QKm  = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_QKm,
                                                               DataFieldPositionType.ColumnAndLength, 78, 7,
                                                               DataFieldOptions.NotRequired),
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
            
            ' ''' <summary> Calls <paramref name="SplitLine"/>.<c>TryParseField()</c> and does related actions. </summary>
             '  ''' <typeparam name="TFieldValue"> The type to parse the field into. </typeparam>
             '  ''' <param name="SplitLine">       The <see cref="PreSplittedTextLine"/> to parse the field from. </param>
             '  ''' <param name="FieldDef">        The parsing instructions. </param>
             '  ''' <param name="Result">          The resulting DataField object. </param>
             '  ''' <returns>                      <see langword="true"/> on success, otherwise <see langword="false"/>. </returns>
             ' ''' <remarks> If a parsing error has occurred it will be added to <c>Me.ParseErrors</c>. </remarks>
             ' Private Function TryParseField(Of TFieldValue As IConvertible)(SplitLine As PreSplittedTextLine,
             '                                                                FieldDef As DataFieldDefinition(Of TFieldValue),
             '                                                                <Out> ByRef Result As DataField(Of TFieldValue)
             '                                                               ) As Boolean
             '     Dim success As Boolean = SplitLine.TryParseField(FieldDef, Result)
             '     If (Not success) Then
             '         Me.ParseErrors.Add(Result.ParseError)
             '     End If
             '     Return success
            ' End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
