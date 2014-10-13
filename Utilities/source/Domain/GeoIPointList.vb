
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
    
    ''' <summary> The GeoIPointList is a List of GeoIPoint's and contains all related data and manipulation methods. </summary>
     ''' <remarks>
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> Property for accessing the Header. </description></item>
     ''' <item><description> Methods for reading and writing the ascii ipkt file. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class GeoIPointList
        Inherits GeoPointListBase(Of String, GeoIPoint)
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoIPointList")
            
            Private iPktEncoding    As Encoding = Encoding.Default
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                LineStartCommentToken = "#"
                Logger.logDebug("New(): GeoIPointList instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ' Private _HeaderKF  As GeoIPoint
            ' 
            ' ''' <summary> Gets or sets the Header of the binary point file. It returns the default header, if not changed before (i.e. read from binary file). </summary>
            ' Public Property HeaderKF() As GeoIPoint
            '     Get
            '         If _HeaderKF Is Nothing Then
            '             _HeaderKF = getDefaultKFHeader()
            '         End If
            '         Return _HeaderKF
            '     End Get
            '     Set(value As GeoIPoint)
            '         _HeaderKF = value
            '     End Set
            ' End Property
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Reads the ascii "iPkt" point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to read from. </param>
             ''' <remarks>
             ''' If this method fails, the points collection won't be cleared, so parse errors stay available."
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoIPointList.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub readFromIpktFile(FilePath As String)
                Try 
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_ReadIpktStart, FilePath))
                    
                    Me.Clear()
                    Me.ParseErrors.FilePath = FilePath
                    Dim RecDef As New RecDefIpkt()
                    
                    Dim FileReader As New DataTextFileReader()
                    FileReader.LineStartCommentToken = LineStartCommentToken
                    FileReader.Load(FilePath, Encoding:=iPktEncoding, DetectEncodingFromByteOrderMarks:=False, BufferSize:=1024)
                    Me.Header = FileReader.Header
                    
                    For i As Integer = 0 To FileReader.DataCache.Count - 1
                        
                        Dim DataLine As DataTextLine = FileReader.DataCache(i)
                        
                        If (DataLine.HasData) Then
                            
                            Try
    		                    Dim p As New GeoIPoint()
                                
                                Dim FieldID    As DataField(Of String) = DataLine.ParseField(RecDef.PointID)
                                Dim FieldY     As DataField(Of Double) = DataLine.ParseField(RecDef.Y)
                                Dim FieldX     As DataField(Of Double) = DataLine.ParseField(RecDef.X)
                                Dim FieldZ     As DataField(Of Double) = DataLine.ParseField(RecDef.Z)
                                Dim FieldTime  As DataField(Of String) = DataLine.ParseField(RecDef.TimeStamp)
    		                    
                                p.ID           = FieldID.Value
                                p.Y            = FieldY.Value
                                p.X            = FieldX.Value
                                p.Z            = FieldZ.Value
                                
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
                                p.SourceLineNo = DataLine.SourceLineNo
                                
                                ' Parse time stamp if given (DataLine.ParseField is unable to do it).
                                If (FieldTime.Value.IsNotEmptyOrWhiteSpace()) Then
                                    Dim TimeStamp As DateTime
                                    Dim success   As Boolean = DateTime.TryParseExact(FieldTime.Value, "s", Nothing, Globalization.DateTimeStyles.None, TimeStamp)
                                    If (success) Then
                                        p.TimeStamp = TimeStamp
                                    Else
                                        Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                                p.SourceLineNo,
                                                                                FieldTime.Source.Column,
                                                                                FieldTime.Source.Column + FieldTime.Source.Length,
                                                                                sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_InvalidFieldNotTimeStamp, FieldTime.Definition.Caption, FieldTime.Value),
                                                                                sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_HintValidTimeStampFormat, "2012-04-11T15:23:01"),
                                                                                Nothing))
                                    End If
                                End If
                                
                                Me.VerifyConstraints(p, FieldID, FieldX, FieldY, FieldZ)
                                Me.Add(p)
                                
                            Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                Me.ParseErrors.Add(ex.ParseError)
                                If (Not CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_ReadIpktParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                End If
                            End Try
                        End If
                    Next
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_ReadIpktParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                    ElseIf (Me.Count = 0) Then
                        Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                    End If
                    
                    Logger.logDebug(Me.ToString())
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_ReadIpktSuccess, Me.Count, FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_ReadIpktFailed, FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
            ''' <summary> Writes the ascii "iPkt" point file from the points collection. </summary>
             ''' <param name="FilePath"> File to write. </param>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub writeToIpktFile(FilePath As String)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_WriteIpktStart, FilePath))
                    
                    Using oSW As New StreamWriter(FilePath, append:=False, encoding:=iPktEncoding)
                        oSW.Write(Me.ToIpkt())
                    End Using
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_WriteIpktSuccess, Me.Count, FilePath))
                    
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoIPointList_WriteIpktFailed, FilePath), ex)
                End Try
            End Sub
            
            ''' <summary> Returns a list of all points in one iPkt formatted string. </summary>
            Public Function ToIpkt() As String
                
                Dim IpktFmt   As String = " %0.6d|%+2s|%+6s|%+2s|%6.3f|%6.3f|%+20s|%+3s|%14.5f|%14.5f|%14.5f|%19s|%+6s|%+4s|%4.1f|%4.1f|%-25s|%2s|%-25s|%2s|%-25s|%s"
                Dim PointList As New System.Text.StringBuilder()
                
                ' Header lines.
                If (Me.Header.Count > 0) Then
                    For Each HeaderLine As String In Me.Header
                        PointList.Append(LineStartCommentToken)
                        PointList.AppendLine(HeaderLine)
                    Next
                Else
                    ' Default Header.
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoIPointList_Label_iPktDefaultHeader1)
                    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoIPointList_Label_iPktDefaultHeader2)
                End If
                
                ' Points.
                For i As Integer = 0 To Me.Count - 1
                    
                    Dim p As GeoIPoint = Me.Item(i)
                    
                    Dim TimeStamp As String = Nothing
                    If (p.TimeStamp.HasValue) Then
                        TimeStamp = p.TimeStamp.Value.ToString("s")
                    End If
                    
                    PointList.AppendLine(sprintf(IpktFmt, i + 1,
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
                Next
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns a list of all points in one string. </summary>
            Public Overrides Function ToString() As String
                
                Dim PointList As New System.Text.StringBuilder()
                
                ' Header lines.
                PointList.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------")
                'If (Me.Header.Count > 0) Then
                '    For Each HeaderLine As String In Me.Header
                '        PointList.Append(LineStartCommentToken)
                '        PointList.AppendLine(HeaderLine)
                '    Next
                'Else
                '    ' Default Header.
                '    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoIPointList_Label_iPktDefaultHeader1)
                '    PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoIPointList_Label_iPktDefaultHeader2)
                'End If
                
                ' Points.
                For Each p As GeoIPoint In Me
                    PointList.AppendLine(p.ToString())
                Next
                PointList.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------")
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Record Definitions"
            
            ''' <summary> Definition of a Ipkt source record. </summary>
            Protected Class RecDefIpkt
                
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
                    Me.Info         = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Info       , DataFieldPositionType.ColumnAndLength, 147, 25, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttKey1      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttKey1    , DataFieldPositionType.ColumnAndLength, 173,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttValue1    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttValue1  , DataFieldPositionType.ColumnAndLength, 176, 25, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttKey2      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttKey2    , DataFieldPositionType.ColumnAndLength, 202,  2, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.AttValue2    = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_AttValue2  , DataFieldPositionType.ColumnAndLength, 205, 25, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                    Me.Comment      = New DataFieldDefinition(Of String)   (Rstyx.Utilities.Resources.Messages.Domain_Label_Comment    , DataFieldPositionType.ColumnAndLength, 231, Integer.MaxValue, DataFieldOptions.NotRequired)
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
        
        #Region "Private Members"
            
           '  ''' <summary> Gets point ID from a raw numeric ID. </summary>
           '   ''' <param name="RawID"> Raw numeric ID. </param>
           '   ''' <returns> Unchanged input value if <paramref name="RawID"/> is less than 100, otherwise <paramref name="RawID"/> / 100000; or 1.0E+40 if <paramref name="RawID"/> = <c>Double.NaN</c>. </returns>
           '  Private Function getIDSmart(RawID As Double) As Double
           '      Dim RetID As Double = RawID
           '      If (Double.IsNaN(RawID)) Then
           '          RetID = 1.0E+40
           '      ElseIf (RawID > 99.999995)
           '          RetID = RawID / PointIDFactor
           '      End If
           '      Return RetID
           '  End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
