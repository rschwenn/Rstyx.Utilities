
Imports System
Imports System.Collections.Generic
'Imports System.Collections.ObjectModel
Imports System.IO

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader and writer for iPkt GeoPoint files - see <see cref="GeoPointFile"/>. </summary>
    Public Class iPktFile
        Inherits GeoPointFile
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.iPktFile")
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Me.LineStartCommentToken = "#"
                
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.iPktFile_Label_DefaultHeader1)
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.iPktFile_Label_DefaultHeader2)
                Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.iPktFile_Label_DefaultHeader3)
                
                Logger.logDebug("New(): iPktFile instantiated")
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Reads the point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to read from. </param>
             ''' <returns> All read points as <see cref="GeoPointOpenList"/>. </returns>
             ''' <remarks>
             ''' If this method fails, <see cref="GeoPointFile.ParseErrors"/> should provide the parse errors occurred."
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Function Load(FilePath As String) As GeoPointOpenList
                
                Dim PointList As New GeoPointOpenList()
                Try 
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadStart, FilePath))
                    
                    Me.Reset(FilePath)
                    
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
                    
                    For Each DataLine As DataTextLine In FileReader.DataCache
                        
                        Dim FieldID  As DataField(Of String) = Nothing
                        
                        If (DataLine.HasData) Then
                            Try
    		                    Dim p As New GeoIPoint()
                                
                                FieldID = DataLine.ParseField(RecDef.PointID)
                                p.ID    = FieldID.Value
                                
                                Dim FieldY     As DataField(Of Double) = DataLine.ParseField(RecDef.Y)
                                Dim FieldX     As DataField(Of Double) = DataLine.ParseField(RecDef.X)
                                Dim FieldZ     As DataField(Of Double) = DataLine.ParseField(RecDef.Z)
                                Dim FieldTime  As DataField(Of String) = DataLine.ParseField(RecDef.TimeStamp)
                                
                                p.Y = FieldY.Value
                                p.X = FieldX.Value
                                p.Z = FieldZ.Value
                                
                                p.CalcCode     = DataLine.ParseField(RecDef.CalcCode   ).Value
                                p.ObjectKey    = DataLine.ParseField(RecDef.ObjectKey  ).Value
                                p.GraficsCode  = DataLine.ParseField(RecDef.GraficsCode).Value
                                p.GraficsDim   = DataLine.ParseField(RecDef.GraficsDim ).Value
                                p.GraficsEcc   = DataLine.ParseField(RecDef.GraficsEcc ).Value
                                p.CoordType    = DataLine.ParseField(RecDef.CoordType  ).Value
                                p.CoordSys     = DataLine.ParseField(RecDef.CoordSys   ).Value
                                p.Flags        = DataLine.ParseField(RecDef.Flags      ).Value
                                p.wp           = DataLine.ParseField(RecDef.wp         ).Value
                                p.wh           = DataLine.ParseField(RecDef.wh         ).Value
                                p.Info         = DataLine.ParseField(RecDef.Info       ).Value
                                p.AttKey1      = DataLine.ParseField(RecDef.AttKey1    ).Value
                                p.AttValue1    = DataLine.ParseField(RecDef.AttValue1  ).Value
                                p.AttKey2      = DataLine.ParseField(RecDef.AttKey2    ).Value
                                p.AttValue2    = DataLine.ParseField(RecDef.AttValue2  ).Value
                                p.Comment      = DataLine.ParseField(RecDef.Comment    ).Value
                                
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
                                
                                If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                p.VerifyConstraints(Me.Constraints, FieldX, FieldY, FieldZ)
                                PointList.Add(p)
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, ex.Message, FilePath))
                                If (Not CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                                
                            Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                Me.ParseErrors.Add(ex.ParseError)
                                If (Not CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                            End Try
                        End If
                    Next
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    ElseIf (PointList.Count = 0) Then
                        Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                    End If
                    
                    Logger.logDebug(PointList.ToString())
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadSuccess, PointList.Count, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadFailed, FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
                
                Return PointList
            End Function
            
            ''' <summary> Writes the points collection to the point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <param name="FilePath">  File to store the points into. </param>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Sub Store(PointList As IEnumerable(Of IGeoPoint), FilePath As String)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreStart, FilePath))
                    
                    Me.Reset(Nothing)
                    
                    Dim PointFmt   As String = " %0.6d|%+2s|%+6s|%+2s|%6.3f|%6.3f|%+20s|%+3s|%14.5f|%14.5f|%14.5f|%19s|%+6s|%+4s|%4.1f|%4.1f|%-25s|%2s|%-25s|%2s|%-25s|%s"
                    Dim PointCount As Integer = 0
                    Dim UniqueID   As Boolean = True  ' iGeo ignores all but the first point with same ID => hence don't write more than once.
                    
                    Using oSW As New StreamWriter(FilePath, append:=False, encoding:=Me.FileEncoding)
                        
                        ' Header.
                        If (TypeOf PointList Is IHeader) Then
                            Dim HeaderLines As String = Me.CreateFileHeader(PointList).ToString()
                            If (HeaderLines.IsNotEmptyOrWhiteSpace()) Then oSW.Write(HeaderLines)
                        End If
                        
                        ' Points.
                        For Each SourcePoint As IGeoPoint In PointList
                            Try
                                ' Convert point: This verifies the ID and provides all fields for writing.
                                Dim p As GeoIPoint = SourcePoint.AsGeoIPoint()
                                
                                ' Check for unique ID.
                                If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                
                                PointCount += 1
                                Dim TimeStamp As String = If(p.TimeStamp.HasValue, p.TimeStamp.Value.ToString("s"), Nothing)
                                
                                ' Write line.
                                oSW.WriteLine(sprintf(PointFmt, PointCount,
                                                      p.CalcCode.TrimToMaxLength(2),
                                                      p.ObjectKey.TrimToMaxLength(6),
                                                      p.GraficsCode.TrimToMaxLength(2),
                                                      p.GraficsDim,
                                                      p.GraficsEcc,
                                                      p.ID.TrimToMaxLength(20),
                                                      p.CoordType.TrimToMaxLength(3),
                                                      p.Y, p.X, p.Z,
                                                      sprintf("%19s", TimeStamp),
                                                      p.CoordSys.TrimToMaxLength(6),
                                                      p.Flags.TrimToMaxLength(4),
                                                      p.wp, p.wh,
                                                      p.Info.TrimToMaxLength(25),
                                                      p.AttKey1.TrimToMaxLength(2),
                                                      p.AttValue1.TrimToMaxLength(25),
                                                      p.AttKey2.TrimToMaxLength(2),
                                                      p.AttValue2.TrimToMaxLength(25),
                                                      p.Comment
                                                     ))
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(New ParseError(ParseErrorLevel.[Error], SourcePoint.SourceLineNo, 0, 0, ex.Message, SourcePoint.SourcePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                                
                            'Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                            '    Me.ParseErrors.Add(ex.ParseError)
                            '    If (Not Me.CollectParseErrors) Then
                            '        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                            '    End If
                            End Try
                        Next
                    End Using
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    End If
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreSuccess, PointCount, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreFailed, FilePath), ex)
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
                    Me.Flags        = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Flags      , DataFieldPositionType.ColumnAndLength, 132,  4, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.wp           = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_wp         , DataFieldPositionType.ColumnAndLength, 137,  4, DataFieldOptions.NotRequired)
                    Me.wh           = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_wh         , DataFieldPositionType.ColumnAndLength, 142,  4, DataFieldOptions.NotRequired)
                    Me.Info         = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Info       , DataFieldPositionType.ColumnAndLength, 147, 25, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
                    Me.AttKey1      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttKey1    , DataFieldPositionType.ColumnAndLength, 173,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttValue1    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttValue1  , DataFieldPositionType.ColumnAndLength, 176, 25, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttKey2      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttKey2    , DataFieldPositionType.ColumnAndLength, 202,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttValue2    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttValue2  , DataFieldPositionType.ColumnAndLength, 205, 25, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.Comment      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Comment    , DataFieldPositionType.ColumnAndLength, 231, Integer.MaxValue, DataFieldOptions.NotRequired + DataFieldOptions.TrimEnd)
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
                    Public Info         As DataFieldDefinition(Of String)
                    Public AttKey1      As DataFieldDefinition(Of String)
                    Public AttValue1    As DataFieldDefinition(Of String)
                    Public AttKey2      As DataFieldDefinition(Of String)
                    Public AttValue2    As DataFieldDefinition(Of String)
                    Public Comment      As DataFieldDefinition(Of String)
                #End Region
            End Class
        
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
