
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Imports PGK.Extensions
Imports Rstyx.Utilities
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> The GeoVEPointList is a List of GeoVEPoint's and contains all related data and manipulation methods. </summary>
     ''' <remarks>
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> Property for accessing the Header. </description></item>
     ''' <item><description> Methods for reading and writing the binary Verm.esn file. </description></item>
     ''' <item><description> Methods for reading and writing the ASCII "KV" file. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class GeoVEPointList
        Inherits GeoPointListBase(Of Double, GeoVEPoint)
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoVEPointList")
            
            Private VEencoding      As Encoding = Encoding.Default
            Private PointNoFactor   As Integer = 100000
            Private RecordLength    As Integer = 102
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                LineStartCommentToken = "#"
                Logger.logDebug("New(): GeoVEPointList instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _HeaderKF  As GeoVEPoint
            
            ''' <summary> Gets or sets the Header of the binary point file. It returns the default header, if not changed before (i.e. read from binary file). </summary>
            Public Property HeaderKF() As GeoVEPoint
                Get
                    If _HeaderKF Is Nothing Then
                        _HeaderKF = getDefaultKFHeader()
                    End If
                    Return _HeaderKF
                End Get
                Set(value As GeoVEPoint)
                    _HeaderKF = value
                End Set
            End Property
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Reads the binary Verm.esn point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to read from. </param>
             ''' <remarks>
             ''' The first point represent the file header. It will be read as each other point and stored in <see cref="GeoVEPointList.HeaderKF"/>.
             ''' </remarks>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub readFromBinaryFile(FilePath As String)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadBinaryStart, FilePath))
                    
                    Me.Clear()
                    
                    ' Open file
                    Using oBR As New BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read), VEencoding)
    		            
                        oBR.BaseStream.Seek(0, SeekOrigin.Begin)
                        
    		            ' Points count.
    		            Dim PointCount As Integer = CInt(oBR.BaseStream.Length / RecordLength) - 1
                        
    		            ' Read header and points.
    		            For i As Integer = 0 To PointCount
    		            	
    		                Dim p As New GeoVEPoint()
    		                
                            p.ID                 = getDoubleFromVEDouble(oBR.ReadDouble())
                            p.Y                  = getDoubleFromVEDouble(oBR.ReadDouble())
                            p.X                  = getDoubleFromVEDouble(oBR.ReadDouble())
                            p.Z                  = getDoubleFromVEDouble(oBR.ReadDouble())
                            p.TrackPos.Kilometer = New Kilometer(getDoubleFromVEDouble(oBR.ReadDouble()))
                            
                            p.PositionPreInfo    = CChar(VEencoding.GetString(oBR.ReadBytes(1)))
                            p.Info               = Trim(VEencoding.GetString(oBR.ReadBytes(13)))
                            p.PositionPostInfo   = CChar(VEencoding.GetString(oBR.ReadBytes(1)))
                            
                            p.HeightPreInfo      = CChar(VEencoding.GetString(oBR.ReadBytes(1)))
                            p.HeightInfo         = Trim(VEencoding.GetString(oBR.ReadBytes(13)))
                            p.HeightPostInfo     = CChar(VEencoding.GetString(oBR.ReadBytes(1)))
                            
                            p.Kind               = Trim(VEencoding.GetString(oBR.ReadBytes(4)))
                            p.MarkType           = CStr(oBR.ReadByte)
                            p.mp                 = CDbl(oBR.ReadInt16())
                            p.mh                 = CDbl(oBR.ReadInt16())
                            p.ObjectKey          = oBR.ReadInt32()
                            p.MarkHints          = Trim(VEencoding.GetString(oBR.ReadBytes(1)))  ' Stability Code
                            p.HeightSys          = Trim(VEencoding.GetString(oBR.ReadBytes(3)))
                            p.Job                = Trim(VEencoding.GetString(oBR.ReadBytes(8)))
                            p.sp                 = Trim(VEencoding.GetString(oBR.ReadBytes(1)))
                            p.sh                 = Trim(VEencoding.GetString(oBR.ReadBytes(1)))
                            
                            Dim TrackNo As String = Trim(VEencoding.GetString(oBR.ReadBytes(4)))
                            Integer.TryParse(TrackNo, p.TrackPos.TrackNo)
                            
                            p.TrackPos.RailsCode = VEencoding.GetString(oBR.ReadBytes(1))
                            
                            If (i = 0) Then
                                _HeaderKF = p
                            Else
                                Try
                                    Me.VerifyConstraints(p)
                                    Me.Add(p)
                                Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                    Me.ParseErrors.Add(ex.ParseError)
                                    If (Not CollectParseErrors) Then
                                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadBinaryParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                    End If
                                End Try
                            End If
    		            Next
                    End Using
                    
                    ' Throw exception if parsing errors (constraints) has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadBinaryParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    ElseIf (Me.Count = 0) Then
                        Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                    End If
                    
                    Logger.logDebug(Me.ToString())
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadBinarySuccess, Me.Count, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadBinaryFailed, FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    'If (ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
            ''' <summary> Reads the ascii "KV" point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to read from. </param>
             ''' <remarks>
             ''' If this method fails, the points collection won't be cleared, so parse errors stay available."
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoVEPointList.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub readFromKvFile(FilePath As String)
                Try 
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvStart, FilePath))
                    
                    Me.Clear()
                    Me.ParseErrors.FilePath = FilePath
                    Dim RecDef As New RecDefKV()
                    
                    Dim FileReader As New DataTextFileReader()
                    FileReader.LineStartCommentToken = LineStartCommentToken
                    FileReader.Load(FilePath, Encoding:=VEencoding, DetectEncodingFromByteOrderMarks:=False, BufferSize:=1024)
                    Me.Header = FileReader.Header
                    
                    For i As Integer = 0 To FileReader.DataCache.Count - 1
                        
                        Dim DataLine As DataTextLine = FileReader.DataCache(i)
                        
                        If (DataLine.HasData) Then
                            
                            Try
    		                    Dim p As New GeoVEPoint()
                                
                                Dim FieldID As DataField(Of Double) = DataLine.ParseField(RecDef.PointNo)
                                Dim FieldY  As DataField(Of Double) = DataLine.ParseField(RecDef.Y)
                                Dim FieldX  As DataField(Of Double) = DataLine.ParseField(RecDef.X)
                                Dim FieldZ  As DataField(Of Double) = DataLine.ParseField(RecDef.Z)
    		                    
                                p.ID                 = getIDSmart(FieldID.Value)
                                p.Y                  = FieldY.Value
                                p.X                  = FieldX.Value
                                p.Z                  = FieldZ.Value
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
                                p.SourceLineNo       = DataLine.SourceLineNo
                                
                                Me.VerifyConstraints(p, FieldID, FieldX, FieldY, FieldZ)
                                Me.Add(p)
                                
                            Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                Me.ParseErrors.Add(ex.ParseError)
                                If (Not CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                            End Try
                        End If
                    Next
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    ElseIf (Me.Count = 0) Then
                        Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                    End If
                    
                    Logger.logDebug(Me.ToString())
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvSuccess, Me.Count, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvFailed, FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
            ''' <summary> Writes the binary Verm.esn point file from the points collection. </summary>
             ''' <param name="FilePath"> File to write. </param>
             ''' <remarks>
             ''' The first point represents the file header (<see cref="GeoVEPointList.HeaderKF"/>).
             ''' </remarks>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub writeToBinaryFile(FilePath As String)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteBinaryStart, FilePath))
                    
                    Using oBW As New BinaryWriter(File.Open(FilePath, FileMode.Create, FileAccess.Write), VEencoding)
    		            
                        oBW.BaseStream.Seek(0, SeekOrigin.Begin)
    		            
                        ' Write header and points.
                        For i As Integer = -1 To Me.Count - 1
                            
                            Dim p As GeoVEPoint
                            If (i = -1) Then
                                p = Me.HeaderKF
                            Else
                                p = Me.Item(i)
                            End If
                            
                            ' Conversions.
                            Dim MarkType    As Byte = 0
                            Dim mp          As Int16 = 0
                            Dim mh          As Int16 = 0
                            Dim ObjectKey   As Int32 = 0
                            
                            Byte.TryParse(p.MarkType, MarkType)
                            If (Not Double.IsNaN(p.mp)) Then mp = CInt(p.mp)
                            If (Not Double.IsNaN(p.mh)) Then mh = CInt(p.mh)
                            Int32.TryParse(p.ObjectKey, ObjectKey)
                            
                            oBW.Write(getVEDoubleFromDouble(p.ID))
                            oBW.Write(getVEDoubleFromDouble(p.Y))
                            oBW.Write(getVEDoubleFromDouble(p.X))
                            oBW.Write(getVEDoubleFromDouble(p.Z))
                            oBW.Write(getVEDoubleFromDouble(p.TrackPos.Kilometer.Value))
                            
                            oBW.Write(CByte(Asc(p.PositionPreInfo)))
                            oBW.Write(GetByteArray(VEencoding, p.Info, 13, " "c))
                            oBW.Write(CByte(Asc(p.PositionPostInfo)))
                            
                            oBW.Write(CByte(Asc(p.HeightPreInfo)))
                            oBW.Write(GetByteArray(VEencoding, p.HeightInfo, 13, " "c))
                            oBW.Write(CByte(Asc(p.HeightPostInfo)))
                            
                            oBW.Write(GetByteArray(VEencoding, p.Kind, 4, " "c))
                            oBW.Write(MarkType)
                            oBW.Write(mp)
                            oBW.Write(mh)
                            oBW.Write(ObjectKey)
                            oBW.Write(GetByteArray(VEencoding, p.MarkHints, 1, " "c))
                            oBW.Write(GetByteArray(VEencoding, p.HeightSys, 3, " "c))
                            oBW.Write(GetByteArray(VEencoding, p.Job, 8, " "c))
                            oBW.Write(IIf(p.sp.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.sp))))
                            oBW.Write(IIf(p.sh.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.sh))))
                            oBW.Write(GetByteArray(VEencoding, CStr(p.TrackPos.TrackNo), 4, " "c, AdjustAtRight:=True))
                            oBW.Write(IIf(p.TrackPos.RailsCode.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.TrackPos.RailsCode))))
                        Next
                    End Using
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteBinarySuccess, Me.Count, FilePath))
                    
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteBinaryFailed, FilePath), ex)
                End Try
            End Sub
            
            ''' <summary> Writes the ascii "KV" point file from the points collection. </summary>
             ''' <param name="FilePath"> File to write. </param>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub writeToKvFile(FilePath As String)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteKvStart, FilePath))
                    
                    Using oSW As New StreamWriter(FilePath, append:=False, encoding:=VEencoding)
                        oSW.Write(Me.ToKV())
                    End Using
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteKvSuccess, Me.Count, FilePath))
                    
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteKvFailed, FilePath), ex)
                End Try
            End Sub
            
            ' ''' <summary> Changes the point numbers according to a point change table. </summary>
            '  ''' <param name="PointChangeTab"> Table with Point pairs (source => target). </param>
            '  ''' <remarks></remarks>
            ' Public Sub changePointNumbers(PointChangeTab As Dictionary(Of Double, Double))
            '     Dim dblPointNo  As Double
            '     Dim ChangeCount As Long = 0
            '     
            '     If (PointChangeTab.Count < 1) then
            '         Logger.logInfo(Rstyx.Utilities.Resources.Messages.GeoVEPointList_EmptyPointChangeTab)
            '     Else
            '         For Each Point As GeoVEPoint In Me
            '             dblPointNo = Math.Round(Point.ID, 5)
            '             If (PointChangeTab.ContainsKey(dblPointNo)) Then
            '                 Point.ID = PointChangeTab(dblPointNo)
            '                 ChangeCount += 1
            '             End If
            '         Next
            '         
            '         Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ChangePointNumbersSuccess, ChangeCount))
            '     End If
            ' End Sub
            
            ''' <summary> Returns a list of all points in one KV formatted string. </summary>
            Public Function ToKV() As String
                
                Dim KvFmt As String = "%7.0f %15.5f%15.5f%10.4f %12.4f  %-13s %-13s %-4s %4d %1s  %3s %5.0f %5.0f  %1s %+3s  %1s%1s  %-8s %7s"
                Dim PointList As New System.Text.StringBuilder()
                
                ' Header lines.
                If (Me.Header.Count > 0) Then
                    For Each HeaderLine As String In Me.Header
                        PointList.Append(LineStartCommentToken)
                        PointList.AppendLine(HeaderLine)
                    Next
                Else
                    ' Default Header.
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoVEPointList_Label_KvDefaultHeader1)
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoVEPointList_Label_KvDefaultHeader2)
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoVEPointList_Label_KvDefaultHeader3)
                    PointList.AppendLine("#-----------------------------------------------------------------------------------------------------------------------------------------------------")
                End If
                
                ' Points.
                For Each p As GeoVEPoint In Me
                    
                    PointList.AppendLine(sprintf(KvFmt, P.ID * PointNoFactor, IIf(Double.IsNaN(P.Y), 0, P.Y), IIf(Double.IsNaN(P.X), 0, P.X), IIf(Double.IsNaN(P.Z), 0, P.Z),
                                P.TrackPos.Kilometer.Value, P.Info.TrimToMaxLength(13), P.HeightInfo.TrimToMaxLength(13),
                                P.Kind.TrimToMaxLength(4), P.TrackPos.TrackNo, P.TrackPos.RailsCode.TrimToMaxLength(1), P.HeightSys.TrimToMaxLength(3), P.mp, P.mh, 
                                P.MarkHints.TrimToMaxLength(1), P.MarkType.TrimToMaxLength(3), P.sp.TrimToMaxLength(1), P.sh.TrimToMaxLength(1),
                                P.Job.TrimToMaxLength(8), P.ObjectKey.TrimToMaxLength(7)))
                Next
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns a list of all points in one string. </summary>
            Public Overrides Function ToString() As String
                
                Dim PointList As New System.Text.StringBuilder()
                
                ' Header lines.
                If (Me.Header.Count > 0) Then
                    For Each HeaderLine As String In Me.Header
                        PointList.Append(LineStartCommentToken)
                        PointList.AppendLine(HeaderLine)
                    Next
                Else
                    ' Default Header.
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoVEPointList_Label_KvDefaultHeader1)
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoVEPointList_Label_KvDefaultHeader2)
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoVEPointList_Label_KvDefaultHeader3)
                    PointList.AppendLine("#-----------------------------------------------------------------------------------------------------------------------------------------------------")
                End If
                
                ' Points.
                For Each p As GeoVEPoint In Me
                    PointList.AppendLine(p.ToString())
                Next
                PointList.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------")
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Record Definitions"
            
            ''' <summary> Definition of a KV source record. </summary>
            Protected Class RecDefKV
                
                ''' <summary> Initializes the field definition. </summary>
                Public Sub New()
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    Me.PointNo      = New DataFieldDefinition(Of Double)   (Rstyx.Utilities.Resources.Messages.Domain_Label_PointID   , DataFieldPositionType.ColumnAndLength,   0,  7)
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
                    Public PointNo      As DataFieldDefinition(Of Double)
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
        
        #Region "Private Members"
            
            ''' <summary> Gets a normalized Double from a VE flavoured storage. </summary>
             ''' <param name="VEDouble"> Double from VE file. </param>
             ''' <returns> Unchanged input value or <c>Double.NaN</c> if <paramref name="VEDouble"/> = 1.0E+40. </returns>
            Private Function getDoubleFromVEDouble(VEDouble As Double) As Double
                Return IIf(VEDouble = 1.0E+40, Double.NaN, VEDouble)
            End Function
            
            ''' <summary> Gets a normalized Double from a VE flavoured storage. </summary>
             ''' <param name="NormDouble"> Double from VE file. </param>
             ''' <returns> Unchanged input value or 1.0E+40 if <paramref name="NormDouble"/> = <c>Double.NaN</c>. </returns>
            Private Function getVEDoubleFromDouble(NormDouble As Double) As Double
                Return IIf(Double.IsNaN(NormDouble), 1.0E+40, NormDouble)
            End Function
            
            ''' <summary> Gets point ID from a raw numeric ID. </summary>
             ''' <param name="RawID"> Raw numeric ID. </param>
             ''' <returns> Unchanged input value if <paramref name="RawID"/> is less than 100, otherwise <paramref name="RawID"/> / 100000; or 1.0E+40 if <paramref name="RawID"/> = <c>Double.NaN</c>. </returns>
            Private Function getIDSmart(RawID As Double) As Double
                Dim RetID As Double = RawID
                If (Double.IsNaN(RawID)) Then
                    RetID = 1.0E+40
                ElseIf (RawID > 99.999995)
                    RetID = RawID / PointNoFactor
                End If
                Return RetID
            End Function
            
            Private Function getDefaultKFHeader() As GeoVEPoint
                Dim p As New GeoVEPoint()
                
                p.ID                 = getVEDoubleFromDouble(Double.NaN)
                p.HeightSys          = "R00"
                p.Info               = "ERSION 7.0  R"
                p.MarkHints          = "1"
                p.PositionPreInfo    = "V"c
                p.TrackPos.RailsCode = "5"
                p.mh                 = 3.0
                p.mp                 = 3.0
                p.sh                 = "R"c
                p.sp                 = "R"c
                
                Return p
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
