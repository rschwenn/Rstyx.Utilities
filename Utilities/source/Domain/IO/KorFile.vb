
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader and writer for KOR GeoPoint files (general Ascii) - see <see cref="GeoPointFile"/>. </summary>
    Public Class KorFile
        Inherits GeoPointFile
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.KorFile")
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Me.LineStartCommentToken = "#"
                
                'Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.KorFile_Label_DefaultHeader1)
                'Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.KorFile_Label_DefaultHeader2)
                'Me.DefaultHeader.Add(Rstyx.Utilities.Resources.Messages.KorFile_Label_DefaultHeader3)
                'Me.DefaultHeader.Add("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")
                
                'Me.HeaderDiscardLines.Add("@Kommentar=")
                
                Logger.logDebug("New(): KorFile instantiated")
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
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public ReadOnly Overrides Iterator Property PointStream() As IEnumerable(Of IGeoPoint)
                Get
                    Try 
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KorFile_LoadStart, Me.FilePath))
                        Logger.logInfo(Me.GetPointEditOptionsLogText())
                        
                        Dim UniqueID    As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
                        Dim RecDef      As New RecordDefinition()
                        Dim PointCount  As Integer = 0
                        Dim ParseResult As GeoPoint.ParseInfoTextResult
                        
                        For Each DataLine As DataTextLine In Me.DataLineStream
                            
                            Dim FieldID As DataField(Of String) = Nothing
                            
                            If (DataLine.HasData) Then
                                Try
    		                        Dim p As New GeoPoint()
                                    
                                    ' Keep some data fields for later issue tracking.
                                    FieldID                            = DataLine.ParseField(RecDef.PointID)
                                    Dim FieldY As DataField(Of Double) = DataLine.ParseField(RecDef.Y)
                                    Dim FieldX As DataField(Of Double) = DataLine.ParseField(RecDef.X)
                                    Dim FieldZ As DataField(Of Double) = DataLine.ParseField(RecDef.Z)
                                    
                                    p.ID = FieldID.Value
                                    p.Y  = FieldY.Value
                                    p.X  = FieldX.Value
                                    p.Z  = FieldZ.Value
                                    
                                    p.Info         = DataLine.ParseField(RecDef.PositionInfo).Value
                                    p.Comment      = DataLine.Comment
                                    
                                    p.SourcePath   = FilePath
                                    p.SourceLineNo = DataLine.SourceLineNo
                                    
                                    ' Editing.
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
                                    Me.ParseErrors.Add(ParseError.Create(ParseErrorLevel.[Error], DataLine.SourceLineNo, FieldID, ex.Message, Nothing, FilePath))
                                    If (Not Me.CollectParseErrors) Then
                                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KorFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                    End If
                                    
                                Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                    Me.ParseErrors.Add(ex.ParseError)
                                    If (Not Me.CollectParseErrors) Then
                                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KorFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                    End If
                                End Try
                            End If
                        Next
                        
                        ' Throw exception if parsing errors has been collected.
                        If (Me.ParseErrors.HasErrors) Then
                            Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KorFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                        ElseIf (PointCount = 0) Then
                            Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                        End If
                        
                        'Logger.logDebug(PointList.ToString())
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KorFile_LoadSuccess, PointCount, FilePath))
                        
                    Catch ex As ParseException
                        Throw
                    Catch ex as System.Exception
                        Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.KorFile_LoadFailed, FilePath), ex)
                    Finally
                        Me.ParseErrors.ToLoggingConsole()
                        If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                    End Try
                End Get
            End Property
            
            ''' <summary> Writes the points collection to the point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <param name="MetaData">  An object providing the header for <paramref name="PointList"/>. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="PointList"/> is a <see cref="GeoPointList"/> then
             ''' it's ensured that the point ID's written to the file are unique.
             ''' Otherwise point ID's may be not unique.
             ''' </para>
             ''' <para>
             ''' Spaces in point ID's will be replaced to underscores!
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.InvalidOperationException"> <see cref="DataFile.FilePath"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Sub Store(PointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KorFile_StoreStart, Me.FilePath))
                    Logger.logInfo(Me.GetPointOutputOptionsLogText)
                    If (Me.FilePath.IsEmptyOrWhiteSpace()) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.DataFile_MissingFilePath)
                    
                    Me.Reset(Nothing)
                    
                    Dim PointFmt   As String = "%+20s %14.4f %14.4f %14.4f     %-30s %-s"
                    Dim PointCount As Integer = 0
                    Dim UniqueID   As Boolean = (TypeOf PointList Is GeoPointList)
                    
                    Using oSW As New StreamWriter(Me.FilePath, append:=False, encoding:=Me.FileEncoding)
                        
                        For Each SourcePoint As IGeoPoint In PointList
                            Try
                                ' Header.
                                If (PointCount = 0) Then
                                    ' At this point, the header of a GeoPointFile has been read and coud be written.
                                    Dim HeaderLines As String = Me.CreateFileHeader(PointList, MetaData).ToString()
                                    If (HeaderLines.IsNotEmptyOrWhiteSpace()) Then oSW.Write(HeaderLines)
                                End If
                                
                                ' Get access to point methods (which are not interface members).
                                Dim p As New GeoPoint(SourcePoint)
                                
                                ' Verify, that ID doesn't contain whitespace.
                                Me.VerifyID(p.ID)
                                
                                ' Check for unique ID, if PointList is unique (since Point ID may have changed while converting to VE point).
                                If (UniqueID) Then Me.VerifyUniqueID(SourcePoint.ID)
                                
                                ' Write line.
                                oSW.WriteLine(sprintf(PointFmt,
                                                      p.ID.Replace(" "c, "_"c),
                                                      If(Double.IsNaN(p.Y), 0, p.Y),
                                                      If(Double.IsNaN(p.X), 0, p.X),
                                                      If(Double.IsNaN(p.Z), 0, p.Z),
                                                      p.CreateInfoTextOutput(Me.OutputOptions),
                                                      If(p.Comment.IsEmptyOrWhiteSpace(), String.Empty, " # " & p.Comment)
                                                     ))
                                PointCount += 1
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(New ParseError(ParseErrorLevel.[Error], SourcePoint.SourceLineNo, 0, 0, ex.Message, SourcePoint.SourcePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KorFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                                End If
                                
                            'Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                            '    Me.ParseErrors.Add(ex.ParseError)
                            '    If (Not Me.CollectParseErrors) Then
                            '        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KorFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                            '    End If
                            End Try
                        Next
                    End Using
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KorFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                    End If
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KorFile_StoreSuccess, PointCount, Me.FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.KorFile_StoreFailed, Me.FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
        #End Region
        
        #Region "Protected Members"
            
            ''' <summary> Verifies that the given <paramref name="ID"/> doesn't contain whitespace. </summary>
             ''' <param name="ID"> The ID to check. </param>
             ''' <exception cref="InvalidIDException"> The given <paramref name="ID"/> contains whitespace. </exception>
            Protected Sub VerifyID(ID As String)
                If (ID.IsMatchingTo("\s+")) Then
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.KorFile_InvalidID, ID))
                End If
            End Sub
            
        #End Region
        
        #Region "Record Definitions"
            
            ''' <summary> Definition of a source record. </summary>
            Protected Class RecordDefinition
                
                ''' <summary> Initializes the field definition. </summary>
                Public Sub New()
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    Me.PointID      = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_PointID, DataFieldPositionType.WordNumber,            1, 0)
                    Me.Y            = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Y      , DataFieldPositionType.WordNumber,            2, 0, DataFieldOptions.ZeroAsNaN)
                    Me.X            = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_X      , DataFieldPositionType.WordNumber,            3, 0, DataFieldOptions.ZeroAsNaN)
                    Me.Z            = New DataFieldDefinition(Of Double)(Rstyx.Utilities.Resources.Messages.Domain_Label_Z      , DataFieldPositionType.WordNumber,            4, 0, DataFieldOptions.ZeroAsNaN)
                    Me.PositionInfo = New DataFieldDefinition(Of String)(Rstyx.Utilities.Resources.Messages.Domain_Label_Info   , DataFieldPositionType.WordNumberAndRemains , 5, 0, DataFieldOptions.NotRequired + DataFieldOptions.Trim)
                End Sub
                
                #Region "Public Fields"
                    Public PointID      As DataFieldDefinition(Of String)
                    Public Y            As DataFieldDefinition(Of Double)
                    Public X            As DataFieldDefinition(Of Double)
                    Public Z            As DataFieldDefinition(Of Double)
                    Public PositionInfo As DataFieldDefinition(Of String)
                #End Region
            End Class
        
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
