
Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader and writer for iPkt GeoPoint files - see <see cref="GeoPointFile"/>. </summary>
    Public Class iPktFile
        Inherits GeoPointFile
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Domain.IO.iPktFile")
            
            Private Shared ReadOnly CooPrecisionDefault As Integer =  5
            Private Shared ReadOnly CooPrecisionBLh     As Integer = 10
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Me.LineStartCommentToken = "#"
                
                ' By default the ipkt text field should be parsed and written as iGeo "iTrassen-Codierung".
                Me.EditOptions   = GeoPointEditOptions.Parse_iTC
                Me.OutputOptions = GeoPointOutputOptions.Create_iTC
                
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.iPktFile_Label_DefaultHeader1)
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.iPktFile_Label_DefaultHeader2)
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.iPktFile_Label_DefaultHeader3)
                
                Me.HeaderDiscardLines.Add(" @Kommentar=")

                Me.FileFormatProperties.Add("ActualCant"      , 0)   
                Me.FileFormatProperties.Add("ActualCantAbs"   , 0)
                Me.FileFormatProperties.Add("ActualTrackGauge", 0)
                Me.FileFormatProperties.Add("AttKey1"         , 0)
                Me.FileFormatProperties.Add("AttKey2"         , 0)
                Me.FileFormatProperties.Add("AttValue1"       , 0)
                Me.FileFormatProperties.Add("AttValue2"       , 0)
                Me.FileFormatProperties.Add("CalcCode"        , 0)
                Me.FileFormatProperties.Add("Comment"         , 0)
                Me.FileFormatProperties.Add("CoordSys"        , 0)
                Me.FileFormatProperties.Add("CoordType"       , 0)
                Me.FileFormatProperties.Add("Flags"           , 0)
                Me.FileFormatProperties.Add("GraficsCode"     , 0)
                Me.FileFormatProperties.Add("GraficsDim"      , 0)
                Me.FileFormatProperties.Add("GraficsEcc"      , 0)
                Me.FileFormatProperties.Add("ID"              , 0)
                Me.FileFormatProperties.Add("Info"            , 0)
                Me.FileFormatProperties.Add("Kind"            , 0)
                Me.FileFormatProperties.Add("MarkType"        , 0)
                Me.FileFormatProperties.Add("MarkTypeAB"      , 0)
                Me.FileFormatProperties.Add("ObjectKey"       , 0)
                Me.FileFormatProperties.Add("StatusHints"     , 0)
                Me.FileFormatProperties.Add("TimeStamp"       , 0)
                Me.FileFormatProperties.Add("wh"              , 0)
                Me.FileFormatProperties.Add("wp"              , 0)
                Me.FileFormatProperties.Add("X"               , 0)
                Me.FileFormatProperties.Add("Y"               , 0)
                Me.FileFormatProperties.Add("Z"               , 0)
                
                Logger.LogDebug("New(): iPktFile instantiated")
            End Sub
            
            ''' <summary> Creates a new instance with a given file path. </summary>
             ''' <param name="FilePath"> The file path of the file to be read or write. May be <see langword="null"/>. </param>
            Public Sub New(FilePath As String)
                Me.New()
                Me.FilePath = FilePath
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Reads the Point file and provides the points as lazy established enumerable. </summary>
             ''' <returns> All points of <see cref="DataFile.FilePath"/> as lazy established enumerable. </returns>
             ''' <remarks>
             ''' <para>
             ''' If a file header is recognized it will be stored in <see cref="GeoPointFile.Header"/> property before yielding the first Point.
             ''' </para>
             ''' <para>
             ''' If this method fails, <see cref="GeoPointFile.ParseErrors"/> should provide the parse errors occurred."
             ''' </para>
             ''' <para>
             ''' The following Point attributes will be converted to properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Attribute Name</b> </term>  <description> <b>Property Name</b> </description></listheader>
             ''' <item> <term>  SysH  </term>  <description>  HeightSys  </description></item>
             ''' <item> <term>  PArt  </term>  <description>  KindText   </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public ReadOnly Overrides Iterator Property PointStream() As IEnumerable(Of IGeoPoint)
                Get
                    Dim PointCount  As Integer = 0
                    Try
                        Logger.LogInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadStart, Me.FilePath))
                        Logger.LogInfo(Me.GetPointEditOptionsLogText())

                        For Each p As IGeoPoint In Me.DataLineStream.AsParallel().Select(AddressOf ParseTextLine) _
                                                                                 .Where(Function(Point) ((Point IsNot Nothing) AndAlso Point.ID.IsNotEmptyOrWhiteSpace()))
                            PointCount += 1
                            Yield p
                        Next
                        
                        ' Throw exception if parsing errors has been collected.
                        If (Me.ParseErrors.HasErrors) Then
                            Throw New ParseException(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                        ElseIf (PointCount = 0) Then
                            Logger.LogWarning(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, Me.FilePath))
                        End If
                        
                        'Logger.LogDebug(PointList.ToString())
                        Logger.LogInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadSuccess, PointCount, Me.FilePath))
                        

                    Catch ex As AggregateException
                        ' Any exception from parallel processing (Me.DataLineStream.AsParallel...) is wrapped in an AggregateException.
                        If (TypeOf ex.InnerException Is ParseException) Then
                            Throw
                        Else
                            Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadFailed, Me.FilePath), ex)
                        End If
                    Catch ex As ParseException
                        ' Signals errors that has been collected, because CollectParseErrors = True.
                        Throw
                    Catch ex as System.Exception
                        Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadFailed, Me.FilePath), ex)
                    Finally
                        Me.ParseErrors.ToLoggingConsole()
                        If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                    End Try
                End Get
            End Property
            
            ''' <summary> Parses one single <see cref="DataTextLine"/>. </summary>
             ''' <param name="DataLine"> The Line to parse. </param>
             ''' <returns> The newly creatd Point. May be <see langword="null"/>. </returns>
             ''' <exception cref="ArgumentNullException"> <paramref name="DataLine"/> is <see langword="null"/>. </exception>
            Protected Function ParseTextLine(DataLine As DataTextLine) As IGeoPoint
                
                If (DataLine Is Nothing) Then Throw New System.ArgumentNullException("DataLine")
                
	            Dim p           As GeoIPoint = Nothing
                
                Dim RecDef      As New RecordDefinition()  '** Nicht für jeden Punkt einzeln
                Dim UniqueID    As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
                Dim ParseResult As GeoPoint.ParseInfoTextResult
                Dim FieldID     As DataField(Of String) = Nothing
                
                If (DataLine.HasData) Then
                    Try
    		            p = New GeoIPoint()
                        
                        ' Keep some data fields for later issue tracking.
                        FieldID                                = DataLine.ParseField(RecDef.PointID)
                        Dim FieldY     As DataField(Of Double) = DataLine.ParseField(RecDef.Y)
                        Dim FieldX     As DataField(Of Double) = DataLine.ParseField(RecDef.X)
                        Dim FieldZ     As DataField(Of Double) = DataLine.ParseField(RecDef.Z)
                        Dim FieldTime  As DataField(Of String) = DataLine.ParseField(RecDef.TimeStamp)
                        
                        p.ID = FieldID.Value
                        p.Y  = FieldY.Value
                        p.X  = FieldX.Value
                        p.Z  = FieldZ.Value
                        
                        ' Object key: Remove leading zero's if integer.
                        Dim KeyText    As String = DataLine.ParseField(RecDef.ObjectKey).Value
                        Dim KeyInt     As Integer
                        If (Integer.TryParse(KeyText, KeyInt)) Then KeyText = KeyInt.ToString()
                        If (KeyText = "0") Then KeyText = String.Empty
                        p.ObjectKey    = KeyText
                        
                        p.CalcCode     = DataLine.ParseField(RecDef.CalcCode   ).Value
                        p.GraficsCode  = DataLine.ParseField(RecDef.GraficsCode).Value
                        p.GraficsDim   = DataLine.ParseField(RecDef.GraficsDim ).Value
                        p.GraficsEcc   = DataLine.ParseField(RecDef.GraficsEcc ).Value
                        p.CoordType    = DataLine.ParseField(RecDef.CoordType  ).Value
                        p.Flags        = DataLine.ParseField(RecDef.Flags      ).Value
                        p.wp           = DataLine.ParseField(RecDef.wp         ).Value
                        p.wh           = DataLine.ParseField(RecDef.wh         ).Value
                        p.Info         = DataLine.ParseField(RecDef.Text       ).Value
                        p.AttKey1      = DataLine.ParseField(RecDef.AttKey1    ).Value
                        p.AttValue1    = DataLine.ParseField(RecDef.AttValue1  ).Value
                        p.AttKey2      = DataLine.ParseField(RecDef.AttKey2    ).Value
                        p.AttValue2    = DataLine.ParseField(RecDef.AttValue2  ).Value
                        
                        p.SourcePath   = FilePath
                        p.SourceLineNo = DataLine.SourceLineNo
                        
                        ' Parse time stamp if given (DataLine.ParseField is unable to do it).
                        If (FieldTime.Value.IsNotEmptyOrWhiteSpace()) Then
                            Dim TimeStamp As DateTime
                            Dim success   As Boolean = DateTime.TryParseExact(FieldTime.Value, "s", Nothing, Globalization.DateTimeStyles.None, TimeStamp)
                            If (success) Then
                                p.TimeStamp = TimeStamp
                            Else
                                Throw New ParseException(ParseError.Create(ParseErrorLevel.[Error],
                                                                           DataLine.SourceLineNo,
                                                                           FieldTime,
                                                                           sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_InvalidFieldNotTimeStamp, FieldTime.Definition.Caption, FieldTime.Value),
                                                                           sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_HintValidTimeStampFormat, "2012-04-11T15:23:01"),
                                                                           FilePath))
                            End If
                        End If
                        
                        ' Attributes and comment from free data.
                        Dim FieldFreeData As DataField(Of String) = DataLine.ParseField(RecDef.FreeData)
                        p.ParseFreeData(FieldFreeData.Value, FieldFreeData.Source)
                        
                        ' Convert attributes into matching properties.
                        p.ConvertPropertyAttributes()
                        p.SetKindFromKindText()
                        
                        ' Info and Point kinds (maybe with related data: MarkTypeAB, MarkType, ActualCant) from Text and/or iTrassen-Codierung.
                        ParseResult = p.ParseInfoTextInput(Me.EditOptions)
                        If (ParseResult.HasConflict) Then
                            Me.ParseErrors.Add(New ParseError(ParseErrorLevel.Warning, DataLine.SourceLineNo, 0, 0, ParseResult.Message, ParseResult.Hints, FilePath))
                        End If

                        ' Coord and height system.
                        Dim MixedSys As String = DataLine.ParseField(RecDef.CoordSys).Value
                        If (MixedSys.IsNotEmptyOrWhiteSpace()) Then
                            ' There may be conflicts systems from/in attributes and ipkt systems field.
                            ' But ipkt systems field has priority over attribute.
                            p.CoordSys = MixedSys
                        End If
                        If (p.CoordSys.IsNotEmptyOrWhiteSpace()) Then
                            ' Check for pattern of combined AVANI system notations.
                            Dim oMatch As Match = Regex.Match(p.CoordSys, "^([A-Z][A-Z][0-9])([A-Z][0-9][0-9])$")
                            If (oMatch.Success) Then
                                Dim CoordSys  As String = oMatch.Groups(1).Value
                                Dim HeightSys As String = oMatch.Groups(2).Value
                                ' Height system may have been read from attribute.
                                If (p.HeightSys.IsEmptyOrWhiteSpace()) Then
                                    p.CoordSys  = CoordSys
                                    p.HeightSys = HeightSys
                                ElseIf (p.HeightSys = HeightSys) Then
                                    p.CoordSys  = CoordSys
                                Else
                                    ' Conflict: different height systems in attribute and ipkt systems field.
                                    ' Maybe add warning to Me.ParseErrors.
                                    ' Attribute height system has priority over ipkt combined systems field.
                                End If
                            End If
                        End If
                        
                        ' Verifying.
                        If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                        p.VerifyConstraints(Me.Constraints, FieldX, FieldY, FieldZ)
                        
                    Catch ex As InvalidIDException
                        Dim oError As ParseError = ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, ex.Message, Nothing, FilePath)
                        If (CollectParseErrors) Then
                            Me.ParseErrors.Add(oError)
                            p = Nothing
                        Else
                            If (Me.ParseErrors.AddIfNoError(oError)) Then
                                Throw New ParseException(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                            End If
                        End If
                        
                    Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                        If (CollectParseErrors) Then
                            Me.ParseErrors.Add(ex.ParseError)
                            p = Nothing
                        Else
                            If (Me.ParseErrors.AddIfNoError(ex.ParseError)) Then
                                Throw New ParseException(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                            End If
                        End If
                    End Try
                End If
                
                Return p
            End Function
            
            ''' <summary> Writes the given Point list to the Point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <param name="MetaData">  An object providing the header for <paramref name="PointList"/>. May be <see langword="null"/>. </param>
             ''' <exception cref="System.InvalidOperationException"> <see cref="DataFile.FilePath"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
             ''' <remarks>
             ''' <para>
             ''' The following properties will be converted to attributes:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property Name</b> </term>  <description> <b>Attribute Name</b> </description></listheader>
             ''' <item> <term>  HeightSys  </term>  <description>  SysH  </description></item>
             ''' <item> <term>  KindText   </term>  <description>  PArt  </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' If <see cref="GeoPoint.StatusHints"/> isn't <b>None</b>, an asterisk will be written at line start.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ArgumentNullException"> <paramref name="PointList"/> is <see langword="null"/>. </exception>
            Public Overrides Sub Store(PointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
                
                If (PointList Is Nothing) Then Throw New System.ArgumentNullException("PointList")

                Dim PointCount As Integer = 0
                Try
                    Logger.LogInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreStart, Me.FilePath))
                    Logger.LogInfo(Me.GetPointOutputOptionsLogText)
                    If (Me.FilePath.IsEmptyOrWhiteSpace()) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.DataFile_MissingFilePath)
                    
                    Dim PointFmt    As String  = "%1s%0.6d|%+2s|%+6s|%+2s|%6.3f|%6.3f|%+20s|%+3s|%+14s|%+14s|%+14s|%19s|%-6s|%+4s|%4.1f|%4.1f|%-25s|%2s|%-25s|%2s|%-25s|%s"
                    Dim CoordFmt    As String  = "%14.5f"
                    Dim UniqueID    As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
                    Dim HeaderDone  As Boolean = False
                    Dim Header      As Collection(Of String) = Nothing
                    Dim SyncHandle  As New Object()
                    
                    ' Reset this GeoPointFile, but save the header if needed.
                    If (MetaData Is Me) Then
                        Header = Me.Header?.Clone()
                    End If
                    Me.Reset(Nothing)  ' This clears Me.Header, too.
                    Me.Header = Header
                    
                    
                    Using oSW As New StreamWriter(Me.FilePath, append:=Me.FileAppend, encoding:=Me.FileEncoding)
                        
                        ' Process every point (all in parallel) to a text output line.
                        For Each TextLine As String In PointList.AsParallel().AsOrdered().Select(
                            Function(SourcePoint As IGeoPoint) As String
                                Dim FileTextLine As String = Nothing
                                Try
                                    ' Convert Point: This verifies the ID and provides all fields for writing.
                                    Dim p As GeoIPoint = SourcePoint.AsGeoIPoint()
                                    
                                    ' Check for unique ID.
                                    If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                    
                                    Dim TimeStamp As String = If(p.TimeStamp.HasValue, p.TimeStamp.Value.ToString("s"), Nothing)
                                        
                                    ' Object key: Add leading zero's if integer.
                                    Dim KeyText As String = p.ObjectKey
                                    Dim KeyInt  As Integer
                                    If (Integer.TryParse(KeyText, KeyInt)) Then KeyText = sprintf("%6.6d", KeyInt)
                                    
                                    ' Format for coordinates.
                                    If (p.CoordType.Trim() = "BLh") Then
                                        CoordFmt = "%14." & CooPrecisionBLh & "f"
                                    Else
                                        CoordFmt = "%14." & CooPrecisionDefault & "f"
                                    End If
                                
                                    ' Combine AVANI coord and height system into ipkt systems field.
                                    If (p.CoordSys?.IsMatchingTo("^[A-Z][A-Z][0-9]$") AndAlso p.HeightSys?.IsMatchingTo("^[A-Z][0-9][0-9]$")) Then
                                        p.CoordSys &= p.HeightSys
                                        p.HeightSys = Nothing
                                        SourcePoint.HeightSys = Nothing  ' Avoid the SysH attribute to be created in next statement.
                                    End If
                                
                                    ' Convert properties to attributes in order to be written to file.
                                    p.AddPropertyAttributes(SourcePoint, Me.FileFormatProperties, BindingFlags.Public Or BindingFlags.Instance)
                                
                                    ' Status hints.
                                    Dim StatusHints As Char = If(p.StatusHints = GeoPointStatusHints.None, " "c, "*"c)
                                    
                                    ' Create line.
                                    FileTextLine = sprintf(PointFmt,
                                                           StatusHints,
                                                           PointCount + 1,
                                                           p.CalcCode.TrimToMaxLength(2),
                                                           KeyText.TrimToMaxLength(6),
                                                           p.GraficsCode.TrimToMaxLength(2),
                                                           p.GraficsDim,
                                                           p.GraficsEcc,
                                                           p.ID,
                                                           p.CoordType.TrimToMaxLength(3),
                                                           sprintf(CoordFmt, p.Y),
                                                           sprintf(CoordFmt, p.X),
                                                           sprintf(CoordFmt, p.Z),
                                                           sprintf("%19s", TimeStamp),
                                                           p.CoordSys.TrimToMaxLength(6),
                                                           p.Flags.TrimToMaxLength(4),
                                                           p.wp, p.wh,
                                                           p.CreateInfoTextOutput(Me.OutputOptions),
                                                           p.AttKey1.TrimToMaxLength(2),
                                                           p.AttValue1.TrimToMaxLength(25),
                                                           p.AttKey2.TrimToMaxLength(2),
                                                           p.AttValue2.TrimToMaxLength(25),
                                                           p.CreateFreeDataText()
                                                          )
                                    Interlocked.Increment(PointCount)
                                    
                                Catch ex As InvalidIDException
                                    Dim oError As New ParseError(ParseErrorLevel.[Error], SourcePoint.SourceLineNo, 0, 0, ex.Message, SourcePoint.SourcePath)
                                    If (CollectParseErrors) Then
                                        Me.ParseErrors.Add(oError)
                                    Else
                                        If (Me.ParseErrors.AddIfNoError(oError)) Then
                                            Throw New ParseException(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                                        End If
                                    End If
                                End Try
                                Return FileTextLine 
                            End Function
                            ).Where(Function(Text As String) (Text IsNot Nothing))

                            ' Write Header.
                            If (Not HeaderDone) Then
                                SyncLock (SyncHandle)
                                    If ((Not HeaderDone) AndAlso (Not Me.FileAppend)) Then
                                        ' If MetaData is a GeoPointFile and PointList is the same GeoPointFile's PointStream,
                                        ' then only at this Point, the header has been read and coud be written.
                                        Dim HeaderLines As String = Me.CreateFileHeader(PointList, MetaData).ToString()
                                        If (HeaderLines.IsNotEmptyOrWhiteSpace()) Then oSW.Write(HeaderLines)
                                    End If
                                    HeaderDone = True
                                End SyncLock
                            End If

                            ' Write line.
                            oSW.WriteLine(TextLine)
                        Next
                    End Using
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                    End If
                    
                    Logger.LogInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreSuccess, PointCount, Me.FilePath))
                    
                Catch ex As AggregateException
                    ' Any exception from parallel processing (PointList.AsParallel...) is wrapped in an AggregateException.
                    If (TypeOf ex.InnerException Is ParseException) Then
                        Throw
                    Else
                        Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreFailed, Me.FilePath), ex)
                    End If
                Catch ex As ParseException
                    ' Signals errors that has been collected, because CollectParseErrors = True.
                    Throw
                Catch ex as Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreFailed, Me.FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
        #End Region
        
        #Region "Record Definitions"
            
            ''' <summary> Definition of a source record. </summary>
            Protected Class RecordDefinition
                
                ''' <summary> Initializes the field definition. </summary>
                Public Sub New()
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    Me.CalcCode     = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_CalcCode   , DataFieldPositionType.ColumnAndLength,   8,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.ObjectKey    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_ObjectKey  , DataFieldPositionType.ColumnAndLength,  11,  6, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.GraficsCode  = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_GraficsCode, DataFieldPositionType.ColumnAndLength,  18,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.GraficsDim   = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_GraficsDim , DataFieldPositionType.ColumnAndLength,  21,  6, DataFieldOptions.NotRequired)
                    Me.GraficsEcc   = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_GraficsEcc , DataFieldPositionType.ColumnAndLength,  28,  6, DataFieldOptions.NotRequired)
                    Me.PointID      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_PointID    , DataFieldPositionType.ColumnAndLength,  35, 20, DataFieldOptions.Trim)
                    Me.CoordType    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_CoordType  , DataFieldPositionType.ColumnAndLength,  56,  3, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.Y            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Y          , DataFieldPositionType.ColumnAndLength,  60, 14, DataFieldOptions.NotRequired)
                    Me.X            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_X          , DataFieldPositionType.ColumnAndLength,  75, 14, DataFieldOptions.NotRequired)
                    Me.Z            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Z          , DataFieldPositionType.ColumnAndLength,  90, 14, DataFieldOptions.NotRequired)
                    Me.TimeStamp    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_TimeStamp  , DataFieldPositionType.ColumnAndLength, 105, 19, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.CoordSys     = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_CoordSys   , DataFieldPositionType.ColumnAndLength, 125,  6, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.Flags        = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Flags      , DataFieldPositionType.ColumnAndLength, 132,  4, DataFieldOptions.NotRequired)
                    Me.wp           = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_wp         , DataFieldPositionType.ColumnAndLength, 137,  4, DataFieldOptions.NotRequired)
                    Me.wh           = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_wh         , DataFieldPositionType.ColumnAndLength, 142,  4, DataFieldOptions.NotRequired)
                    Me.Text         = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Text       , DataFieldPositionType.ColumnAndLength, 147, 25, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                    Me.AttKey1      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttKey1    , DataFieldPositionType.ColumnAndLength, 173,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttValue1    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttValue1  , DataFieldPositionType.ColumnAndLength, 176, 25, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttKey2      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttKey2    , DataFieldPositionType.ColumnAndLength, 202,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttValue2    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttValue2  , DataFieldPositionType.ColumnAndLength, 205, 25, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.FreeData     = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_FreeData   , DataFieldPositionType.ColumnAndLength, 231, Integer.MaxValue, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                End Sub
                
                #Region "Public Fields"
                    Public CalcCode     As DataFieldDefinition(Of String)
                    Public ObjectKey    As DataFieldDefinition(Of String)
                    Public GraficsCode  As DataFieldDefinition(Of String)
                    Public GraficsDim   As DataFieldDefinition(Of Double)
                    Public GraficsEcc   As DataFieldDefinition(Of Double)
                    Public PointID      As DataFieldDefinition(Of String)
                    Public CoordType    As DataFieldDefinition(Of String)
                    Public Y            As DataFieldDefinition(Of Double)
                    Public X            As DataFieldDefinition(Of Double)
                    Public Z            As DataFieldDefinition(Of Double)
                    Public TimeStamp    As DataFieldDefinition(Of String)
                    Public CoordSys     As DataFieldDefinition(Of String)
                    Public Flags        As DataFieldDefinition(Of String)
                    Public wp           As DataFieldDefinition(Of Double)
                    Public wh           As DataFieldDefinition(Of Double)
                    Public Text         As DataFieldDefinition(Of String)
                    Public AttKey1      As DataFieldDefinition(Of String)
                    Public AttValue1    As DataFieldDefinition(Of String)
                    Public AttKey2      As DataFieldDefinition(Of String)
                    Public AttValue2    As DataFieldDefinition(Of String)
                    Public FreeData     As DataFieldDefinition(Of String)
                #End Region
            End Class
        
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
