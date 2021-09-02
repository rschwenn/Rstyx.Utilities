
Imports System
Imports System.Collections.Generic
Imports System.IO

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader and writer for iPkt GeoPoint files - see <see cref="GeoPointFile"/>. </summary>
    Public Class iPktFile
        Inherits GeoPointFile
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.iPktFile")
            
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
                
                Logger.logDebug("New(): iPktFile instantiated")
            End Sub
            
            ''' <summary> Creates a new instance with a given file path. </summary>
             ''' <param name="FilePath"> The file path of the file to be read or write. May be <see langword="null"/>. </param>
            Public Sub New(FilePath As String)
                Me.New()
                Me.FilePath = FilePath
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Reads the point file and provides the points as lazy established enumerable. </summary>
             ''' <returns> All points of <see cref="DataFile.FilePath"/> as lazy established enumerable. </returns>
             ''' <remarks>
             ''' <para>
             ''' If a file header is recognized it will be stored in <see cref="GeoPointFile.Header"/> property before yielding the first point.
             ''' </para>
             ''' <para>
             ''' If this method fails, <see cref="GeoPointFile.ParseErrors"/> should provide the parse errors occurred."
             ''' </para>
             ''' <para>
             ''' The following point attributes will be converted to properties:
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
                    Try 
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadStart, Me.FilePath))
                        Logger.logInfo(Me.GetPointEditOptionsLogText())
                        
                        Dim UniqueID    As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
                        Dim RecDef      As New RecordDefinition()
                        Dim PointCount  As Integer = 0
                        Dim ParseResult As GeoPoint.ParseInfoTextResult
                        
                        For Each DataLine As DataTextLine In Me.DataLineStream
                            
                            Dim FieldID  As DataField(Of String) = Nothing
                            
                            If (DataLine.HasData) Then
                                Try
    		                        Dim p As New GeoIPoint()
                                    
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
                                    p.CoordSys     = DataLine.ParseField(RecDef.CoordSys   ).Value
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
                                    p.ParseFreeData(DataLine.ParseField(RecDef.FreeData).Value)
                    
                                    ' Convert selected attributes to properties, which don't belong to .ipkt file.
                                    Dim PropertyName   As String
                                    Dim AttStringValue As String
                                    PropertyName   = "HeightSys"
                                    AttStringValue = p.GetAttValueByPropertyName(PropertyName)
                                    If (AttStringValue IsNot Nothing) Then
                                        P.HeightSys = AttStringValue.Trim()
                                        p.Attributes.Remove(GeoPoint.AttributeNames(PropertyName))
                                    End If
                                    PropertyName   = "KindText"
                                    AttStringValue = p.GetAttValueByPropertyName(PropertyName)
                                    If (AttStringValue IsNot Nothing) Then
                                        P.KindText = AttStringValue.Trim()
                                        p.Attributes.Remove(GeoPoint.AttributeNames(PropertyName))
                                        p.SetKindFromKindText()
                                    End If
                                    
                                    ' Info and point kinds (maybe with related data: MarkTypeAB, MarkType, ActualCant).
                                    ParseResult = p.ParseInfoTextInput(Me.EditOptions)
                                    If (ParseResult.HasConflict) Then
                                        Me.ParseErrors.Add(New ParseError(ParseErrorLevel.Warning, DataLine.SourceLineNo, 0, 0, ParseResult.Message, ParseResult.Hints, FilePath))
                                    End If
                                    
                                    ' Verifying.
                                    If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                    p.VerifyConstraints(Me.Constraints, FieldX, FieldY, FieldZ)
                                    PointCount += 1
                                    
                                    Yield p
                                    
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
                        ElseIf (PointCount = 0) Then
                            Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                        End If
                        
                        'Logger.logDebug(PointList.ToString())
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadSuccess, PointCount, FilePath))
                        
                    Catch ex As ParseException
                        Throw
                    Catch ex as System.Exception
                        Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_LoadFailed, FilePath), ex)
                    Finally
                        Me.ParseErrors.ToLoggingConsole()
                        If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                    End Try
                End Get
            End Property
            
            ''' <summary> Writes the points collection to the point file. </summary>
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
             ''' </remarks>
            Public Overrides Sub Store(PointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreStart, Me.FilePath))
                    Logger.logInfo(Me.GetPointOutputOptionsLogText)
                    If (Me.FilePath.IsEmptyOrWhiteSpace()) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.DataFile_MissingFilePath)
                    
                    Me.Reset(Nothing)
                    
                    Dim PointFmt   As String  = " %0.6d|%+2s|%+6s|%+2s|%6.3f|%6.3f|%+20s|%+3s|%+14s|%+14s|%+14s|%19s|%+6s|%+4s|%4.1f|%4.1f|%-25s|%2s|%-25s|%2s|%-25s|%s"
                    Dim CoordFmt   As String  = "%14.5f"
                    Dim PointCount As Integer = 0
                    Dim UniqueID   As Boolean = True  ' iGeo ignores all but the first point with same ID => hence don't write more than once.
                    
                    Using oSW As New StreamWriter(Me.FilePath, append:=False, encoding:=Me.FileEncoding)
                        
                        ' Points.
                        For Each SourcePoint As IGeoPoint In PointList
                            Try
                                ' Header.
                                If (PointCount = 0) Then
                                    ' At this point, the header of a GeoPointFile has been read and coud be written.
                                    Dim HeaderLines As String = Me.CreateFileHeader(PointList, MetaData).ToString()
                                    If (HeaderLines.IsNotEmptyOrWhiteSpace()) Then oSW.Write(HeaderLines)
                                End If
                                
                                ' Convert point: This verifies the ID and provides all fields for writing.
                                Dim p As GeoIPoint = SourcePoint.AsGeoIPoint()
                                
                                ' Check for unique ID.
                                If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                
                                PointCount += 1
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
                                
                                ' Convert properties to attributes in order to be placed in .ipkt.
                                '   (More candidates: HeightInfo, mp, mh, sp, sh, MarkHints, Job)
                                Dim PropertyName   As String
                                Dim AttributeName  As String
                                PropertyName = "HeightSys"
                                If (p.HeightSys.IsNotEmptyOrWhiteSpace()) Then
                                    AttributeName = GeoPoint.AttributeNames(PropertyName) 
                                    If (Not p.Attributes.ContainsKey(AttributeName)) Then
                                        p.Attributes.Add(AttributeName, p.HeightSys)
                                    End If
                                End If
                                PropertyName = "KindText"
                                If (p.KindText.IsNotEmptyOrWhiteSpace()) Then 
                                    AttributeName = GeoPoint.AttributeNames(PropertyName) 
                                    If (Not p.Attributes.ContainsKey(AttributeName)) Then
                                        p.Attributes.Add(AttributeName, sprintf("%-4s", p.KindText))
                                    End If
                                End If
                                
                                ' Write line.
                                oSW.WriteLine(sprintf(PointFmt, PointCount,
                                                      p.CalcCode.TrimToMaxLength(2),
                                                      KeyText.TrimToMaxLength(6),
                                                      p.GraficsCode.TrimToMaxLength(2),
                                                      p.GraficsDim,
                                                      p.GraficsEcc,
                                                      p.ID.TrimToMaxLength(20),
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
                                                     ))
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(New ParseError(ParseErrorLevel.[Error], SourcePoint.SourceLineNo, 0, 0, ex.Message, SourcePoint.SourcePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
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
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                    End If
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.iPktFile_StoreSuccess, PointCount, Me.FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
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
