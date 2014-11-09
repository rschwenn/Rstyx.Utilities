
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader and writer for KV GeoPoint files (VE Ascii) - see <see cref="GeoPointFile"/>. </summary>
    Public Class KvFile
        Inherits GeoPointFile
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.KvFile")
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Me.LineStartCommentToken = "#"
                
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.KvFile_Label_DefaultHeader1)
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.KvFile_Label_DefaultHeader2)
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.KvFile_Label_DefaultHeader3)
                Me.DefaultHeader.Add("-----------------------------------------------------------------------------------------------------------------------------------------------------")
                
                Logger.logDebug("New(): KvFile instantiated")
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Reads the point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to load the points from. </param>
             ''' <returns> All read points as <see cref="GeoPointOpenList"/>. </returns>
             ''' <remarks>
             ''' If this method fails, <see cref="GeoPointFile.ParseErrors"/> should provide the parse errors occurred."
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Function Load(FilePath As String) As GeoPointOpenList
                
                Dim PointList As New GeoPointOpenList()
                Try 
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KvFile_LoadStart, FilePath))
                    
                    Me.IDCheckList.Clear()
                    Me.ParseErrors.Clear()
                    Me.ParseErrors.FilePath = FilePath
                    Dim UniqueID As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
                    Dim RecDef   As New RecordDefinition()
                    
                    Dim FileReader As New DataTextFileReader()
                    FileReader.LineStartCommentToken = Me.LineStartCommentToken
                    FileReader.LineEndCommentToken   = Me.LineEndCommentToken
                    FileReader.SeparateHeader        = Me.SeparateHeader
                    FileReader.Load(FilePath, Encoding:=Me.FileEncoding, DetectEncodingFromByteOrderMarks:=False, BufferSize:=1024)
                    
                    ' Store read header lines (only if they differ from the default ones).
                    For Each HeadLine As String In FileReader.Header
                        If (Not Me.DefaultHeader.Contains(HeadLine)) Then
                            PointList.Header.Add(HeadLine)
                        End If
                    Next
                    
                    For i As Integer = 0 To FileReader.DataCache.Count - 1
                        
                        Dim DataLine As DataTextLine = FileReader.DataCache(i)
                        Dim FieldID  As DataField(Of String) = Nothing
                        
                        If (DataLine.HasData) Then
                            Try
    		                    Dim p As New GeoVEPoint()
                                
                                FieldID = DataLine.ParseField(RecDef.PointID)
                                p.ID    = FieldID.Value
                                
                                Dim FieldY     As DataField(Of Double) = DataLine.ParseField(RecDef.Y)
                                Dim FieldX     As DataField(Of Double) = DataLine.ParseField(RecDef.X)
                                Dim FieldZ     As DataField(Of Double) = DataLine.ParseField(RecDef.Z)
                                
                                p.Y = FieldY.Value
                                p.X = FieldX.Value
                                p.Z = FieldZ.Value
                                
                                p.TrackPos.Kilometer = DataLine.ParseField(RecDef.Km).Value
                                p.Info               = DataLine.ParseField(RecDef.PositionInfo).Value
                                p.HeightInfo         = DataLine.ParseField(RecDef.HeightInfo).Value
                                p.Kind               = DataLine.ParseField(RecDef.PointKind).Value
                                p.TrackPos.TrackNo   = DataLine.ParseField(RecDef.TrackNo).Value
                                p.TrackPos.RailsCode = DataLine.ParseField(RecDef.RailsCode).Value
                                p.HeightSys          = DataLine.ParseField(RecDef.HeightSys).Value
                                p.mp                 = DataLine.ParseField(RecDef.mp).Value
                                p.mh                 = DataLine.ParseField(RecDef.mh).Value
                                p.MarkHints          = DataLine.ParseField(RecDef.MarkHints).Value  ' Stability Code
                                p.MarkType           = DataLine.ParseField(RecDef.MarkType).Value
                                p.sp                 = DataLine.ParseField(RecDef.sp).Value
                                p.sh                 = DataLine.ParseField(RecDef.sh).Value
                                p.Job                = DataLine.ParseField(RecDef.Job).Value
                                p.ObjectKey          = DataLine.ParseField(RecDef.ObjectKey).Value
                                p.Comment            = DataLine.ParseField(RecDef.Comment).Value
                                
                                p.SourcePath         = FilePath
                                p.SourceLineNo       = DataLine.SourceLineNo
                                
                                If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                p.VerifyConstraints(Me.Constraints, FieldX, FieldY, FieldZ)
                                PointList.Add(p)
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, ex.Message, Nothing, FilePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                                
                            Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                Me.ParseErrors.Add(ex.ParseError)
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                            End Try
                        End If
                    Next
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    ElseIf (PointList.Count = 0) Then
                        Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                    End If
                    
                    Logger.logDebug(PointList.ToString())
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KvFile_LoadSuccess, PointList.Count, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.KvFile_LoadFailed, FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
                
                Return PointList
            End Function
            
            ''' <summary> Writes the points collection to the point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <param name="FilePath">  File to store the points into. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="PointList"/> is a <see cref="GeoPointList"/> then
             ''' it's ensured that the point ID's written to the file are unique.
             ''' Otherwise point ID's may be not unique.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Sub Store(PointList As IEnumerable(Of IGeoPoint), FilePath As String)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KvFile_StoreStart, FilePath))
                    
                    Me.ParseErrors.Clear()
                    Me.IDCheckList.Clear()
                    
                    Dim PointFmt   As String = "%+7s %15.5f%15.5f%10.4f %12.4f  %-13s %-13s %-4s %4d %1s  %3s %5.0f %5.0f  %1s %+3s  %1s%1s  %-8s %7s %-s"
                    Dim PointCount As Integer = 0
                    Dim UniqueID   As Boolean = (TypeOf PointList Is GeoPointList)
                    
                    Using oSW As New StreamWriter(FilePath, append:=False, encoding:=Me.FileEncoding)
                        
                        ' Header.
                        If (TypeOf PointList Is IHeader) Then
                            Dim HeaderLines As String = Me.CreateFileHeader(PointList).ToString()
                            If (HeaderLines.IsNotEmptyOrWhiteSpace()) Then oSW.Write(HeaderLines)
                        End If
                        
                        For Each SourcePoint As IGeoPoint In PointList
                            Try
                                ' Convert point: This verifies the ID and provides all fields for writing.
                                Dim p As GeoVEPoint = SourcePoint.AsGeoVEPoint()
                                
                                ' Check for unique ID, if PointList is unique (since Point ID may have changed while converting to VE point).
                                If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                
                                ' Write line.
                                oSW.WriteLine(sprintf(PointFmt,
                                                      P.ID,
                                                      If(Double.IsNaN(P.Y), 0, P.Y),
                                                      If(Double.IsNaN(P.X), 0, P.X),
                                                      If(Double.IsNaN(P.Z), 0, P.Z),
                                                      p.TrackPos.Kilometer.Value,
                                                      P.Info.TrimToMaxLength(13),
                                                      P.HeightInfo.TrimToMaxLength(13),
                                                      P.Kind.TrimToMaxLength(4),
                                                      p.TrackPos.TrackNo,
                                                      p.TrackPos.RailsCode.TrimToMaxLength(1),
                                                      P.HeightSys.TrimToMaxLength(3),
                                                      P.mp,
                                                      P.mh, 
                                                      P.MarkHints.TrimToMaxLength(1),
                                                      P.MarkType.TrimToMaxLength(3),
                                                      P.sp.TrimToMaxLength(1),
                                                      P.sh.TrimToMaxLength(1),
                                                      P.Job.TrimToMaxLength(8),
                                                      P.ObjectKey.TrimToMaxLength(7),
                                                      p.Comment
                                                     ))
                                PointCount += 1
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(New ParseError(ParseErrorLevel.[Error], SourcePoint.SourceLineNo, 0, 0, ex.Message, SourcePoint.SourcePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                                
                            'Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                            '    Me.ParseErrors.Add(ex.ParseError)
                            '    If (Not Me.CollectParseErrors) Then
                            '        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                            '    End If
                            End Try
                        Next
                    End Using
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    End If
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KvFile_StoreSuccess, PointCount, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.KvFile_StoreFailed, FilePath), ex)
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
                    Me.PointID      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_PointID   , DataFieldPositionType.ColumnAndLength,   0,  7)
                    Me.Y            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Y         , DataFieldPositionType.ColumnAndLength,   9, 14, DataFieldOptions.ZeroAsNaN)
                    Me.X            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_X         , DataFieldPositionType.ColumnAndLength,  24, 14, DataFieldOptions.ZeroAsNaN)
                    Me.Z            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Z         , DataFieldPositionType.ColumnAndLength,  39,  9, DataFieldOptions.ZeroAsNaN)
                    Me.Km           = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km        , DataFieldPositionType.ColumnAndLength,  49, 12, DataFieldOptions.NotRequired)
                    Me.PositionInfo = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Info      , DataFieldPositionType.ColumnAndLength,  63, 13, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                    Me.HeightInfo   = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_HeightInfo, DataFieldPositionType.ColumnAndLength,  77, 13, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                    Me.PointKind    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_PointKind , DataFieldPositionType.ColumnAndLength,  91,  4, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.TrackNo      = New DataFieldDefinition(Of Integer)  (Rstyx.Utilities.Resources.Messages.Domain_Label_TrackNo   , DataFieldPositionType.ColumnAndLength,  96,  4, DataFieldOptions.NotRequired)
                    Me.RailsCode    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_RailsCode , DataFieldPositionType.ColumnAndLength, 101,  1, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.HeightSys    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_HeightSys , DataFieldPositionType.ColumnAndLength, 104,  3, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.mp           = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_mp        , DataFieldPositionType.ColumnAndLength, 108,  5, DataFieldOptions.NotRequired)
                    Me.mh           = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_mh        , DataFieldPositionType.ColumnAndLength, 114,  5, DataFieldOptions.NotRequired)
                    Me.MarkHints    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Stability , DataFieldPositionType.ColumnAndLength, 121,  1, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.MarkType     = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_MarkType  , DataFieldPositionType.ColumnAndLength, 123,  3, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.sp           = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_sp        , DataFieldPositionType.ColumnAndLength, 128,  1, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.sh           = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_sh        , DataFieldPositionType.ColumnAndLength, 129,  1, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.Job          = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Job       , DataFieldPositionType.ColumnAndLength, 132,  8, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.ObjectKey    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_ObjectKey , DataFieldPositionType.ColumnAndLength, 141,  7, DataFieldOptions.NotRequired + DataFieldOptions.Trim + DataFieldOptions.ZeroAsNaN)
                    Me.Comment      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Comment   , DataFieldPositionType.ColumnAndLength, 149,  Integer.MaxValue, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                End Sub
                
                #Region "Public Fields"
                    Public PointID      As DataFieldDefinition(Of String)
                    Public Y            As DataFieldDefinition(Of Double)
                    Public X            As DataFieldDefinition(Of Double)
                    Public Z            As DataFieldDefinition(Of Double)
                    Public Km           As DataFieldDefinition(Of Kilometer)
                    Public PositionInfo As DataFieldDefinition(Of String)
                    Public HeightInfo   As DataFieldDefinition(Of String)
                    Public PointKind    As DataFieldDefinition(Of String)
                    Public TrackNo      As DataFieldDefinition(Of Integer)
                    Public RailsCode    As DataFieldDefinition(Of String)
                    Public HeightSys    As DataFieldDefinition(Of String)
                    Public mp           As DataFieldDefinition(Of Double)
                    Public mh           As DataFieldDefinition(Of Double)
                    Public MarkHints    As DataFieldDefinition(Of String)
                    Public MarkType     As DataFieldDefinition(Of String)
                    Public sp           As DataFieldDefinition(Of String)
                    Public sh           As DataFieldDefinition(Of String)
                    Public Job          As DataFieldDefinition(Of String)
                    Public ObjectKey    As DataFieldDefinition(Of String)
                    Public Comment      As DataFieldDefinition(Of String)
                #End Region
            End Class
        
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
