
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader for KV GeoPoint files (VE Ascii). </summary>
     ''' <remarks>
     ''' .
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description>  </description></item>
     ''' <item><description> The read points will be returned by the Load method as <see cref="GeoPointList"/>. </description></item>
     ''' <item><description>  </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class KvReader
        Inherits GeoPointFileReader
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.KvReader")
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Me.LineStartCommentToken = "#"
                Logger.logDebug("New(): KvReader instantiated")
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Reads the point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to read from. </param>
             ''' <returns> All read points as <see cref="GeoPointList"/>. </returns>
             ''' <remarks>
             ''' If this method fails, <see cref="GeoPointFileReader.ParseErrors"/> should provide the parse errors occurred."
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFileReader.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Function Load(FilePath As String) As GeoPointList
                
                Dim PointList As New GeoPointList()
                Try 
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KvReader_LoadStart, FilePath))
                    
                    PointList.Clear()
                    Me.ParseErrors.FilePath = FilePath
                    Dim RecDef As New RecDef()
                    
                    Dim FileReader As New DataTextFileReader()
                    FileReader.LineStartCommentToken = Me.LineStartCommentToken
                    FileReader.LineEndCommentToken   = Me.LineEndCommentToken
                    FileReader.SeparateHeader        = Me.SeparateHeader
                    FileReader.Load(FilePath, Encoding:=Me.FileEncoding, DetectEncodingFromByteOrderMarks:=False, BufferSize:=1024)
                    
                    PointList.Header = FileReader.Header
                    PointList.Constraints = Me.Constraints
                    
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
                                
                                p.Y            = FieldY.Value
                                p.X            = FieldX.Value
                                p.Z            = FieldZ.Value
                                
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
                                p.SourceLineNo       = DataLine.SourceLineNo
                                
                                PointList.VerifyConstraints(p, FieldID, FieldX, FieldY, FieldZ)
                                PointList.Add(p)
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, ex.Message))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                                
                            Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                Me.ParseErrors.Add(ex.ParseError)
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                            End Try
                        End If
                    Next
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KvReader_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    ElseIf (PointList.Count = 0) Then
                        Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                    End If
                    
                    Logger.logDebug(PointList.ToString())
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KvReader_LoadSuccess, PointList.Count, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.KvReader_LoadFailed, FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
                
                Return PointList
            End Function
            
        #End Region
        
        #Region "Record Definitions"
            
            ''' <summary> Definition of a source record. </summary>
            Protected Class RecDef
                
                ''' <summary> Initializes the field definition. </summary>
                Public Sub New()
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    Me.PointID      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_PointID   , DataFieldPositionType.ColumnAndLength,   0,  7)
                    Me.Y            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Y         , DataFieldPositionType.ColumnAndLength,   9, 14, DataFieldOptions.ZeroAsNaN)
                    Me.X            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_X         , DataFieldPositionType.ColumnAndLength,  24, 14, DataFieldOptions.ZeroAsNaN)
                    Me.Z            = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Z         , DataFieldPositionType.ColumnAndLength,  39,  9, DataFieldOptions.ZeroAsNaN)
                    Me.Km           = New DataFieldDefinition(Of Kilometer)(Rstyx.Utilities.Resources.Messages.Domain_Label_Km        , DataFieldPositionType.ColumnAndLength,  49, 12, DataFieldOptions.NotRequired)
                    Me.PositionInfo = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Info      , DataFieldPositionType.ColumnAndLength,  63, 13, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.HeightInfo   = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_HeightInfo, DataFieldPositionType.ColumnAndLength,  77, 13, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
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
                    Me.ObjectKey    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_ObjectKey , DataFieldPositionType.ColumnAndLength, 141,  7, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.Comment      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Comment   , DataFieldPositionType.ColumnAndLength, 149,  Integer.MaxValue, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
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
