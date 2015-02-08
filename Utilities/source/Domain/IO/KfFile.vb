
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.Math.MathExtensions
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader and writer for KF GeoPoint files (VE Binary) - see <see cref="GeoPointFile"/>. </summary>
    Public Class KfFile
        Inherits GeoPointFile
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.KfFile")
            
            Private RecordLength    As Integer = 102
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): KfFile instantiated")
            End Sub
            
            ''' <summary> Creates a new instance with a given file path. </summary>
             ''' <param name="FilePath"> The file path of the file to be read or write. May be <see langword="null"/>. </param>
            Public Sub New(FilePath As String)
                Me.New()
                Me.FilePath = FilePath
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _HeaderKF  As GeoVEPoint
            
            ''' <summary> Gets or sets the Header of the file, which represents the first file record. It returns the default header, if not changed before (i.e. read from binary file). </summary>
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
             ''' <exception cref="System.InvalidOperationException"> <see cref="DataFile.FilePath"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public ReadOnly Overrides Iterator Property PointStream() As IEnumerable(Of IGeoPoint)
                Get
                    Try 
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KfFile_LoadStart, FilePath))
                        If (Me.FilePath.IsEmptyOrWhiteSpace()) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.DataFile_MissingFilePath)
                        
                        Dim UniqueID    As Boolean = (Constraints.HasFlag(GeoPointConstraints.UniqueID) OrElse Constraints.HasFlag(GeoPointConstraints.UniqueIDPerBlock))
 		                Dim PointCount  As Integer = 0
                        
                        Using oBR As New BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read), FileEncoding)
    		                
                            oBR.BaseStream.Seek(0, SeekOrigin.Begin)
                            
    		                ' Points count.
    		                Dim KfPointCount As Integer = CInt(oBR.BaseStream.Length / RecordLength) - 1
                            
    		                ' Read header and points.
    		                For i As Integer = 0 To KfPointCount
    		                	
    		                    Dim p As New GeoVEPoint()
                                Dim PointID As String
    		                    
                                PointID              = oBR.ReadDouble().ToString()
                                p.Y                  = getDoubleFromVEDouble(oBR.ReadDouble())
                                p.X                  = getDoubleFromVEDouble(oBR.ReadDouble())
                                p.Z                  = getDoubleFromVEDouble(oBR.ReadDouble())
                                p.TrackPos.Kilometer = New Kilometer(getDoubleFromVEDouble(oBR.ReadDouble()))
                                
                                p.PositionPreInfo    = CChar(FileEncoding.GetString(oBR.ReadBytes(1)))
                                p.Info               = Trim(FileEncoding.GetString(oBR.ReadBytes(13)))
                                p.PositionPostInfo   = CChar(FileEncoding.GetString(oBR.ReadBytes(1)))
                                
                                p.HeightPreInfo      = CChar(FileEncoding.GetString(oBR.ReadBytes(1)))
                                p.HeightInfo         = Trim(FileEncoding.GetString(oBR.ReadBytes(13)))
                                p.HeightPostInfo     = CChar(FileEncoding.GetString(oBR.ReadBytes(1)))
                                
                                p.Kind               = Trim(FileEncoding.GetString(oBR.ReadBytes(4)))
                                p.MarkType           = CStr(oBR.ReadByte)
                                p.mp                 = CDbl(oBR.ReadInt16())
                                p.mh                 = CDbl(oBR.ReadInt16())
                                p.ObjectKey          = oBR.ReadInt32()
                                p.MarkHints          = Trim(FileEncoding.GetString(oBR.ReadBytes(1)))  ' Stability Code
                                p.HeightSys          = Trim(FileEncoding.GetString(oBR.ReadBytes(3)))
                                p.Job                = Trim(FileEncoding.GetString(oBR.ReadBytes(8)))
                                p.sp                 = Trim(FileEncoding.GetString(oBR.ReadBytes(1)))
                                p.sh                 = Trim(FileEncoding.GetString(oBR.ReadBytes(1)))
                                
                                Dim TrackNo As String = Trim(FileEncoding.GetString(oBR.ReadBytes(4)))
                                p.TrackPos.TrackNo.TryParse(TrackNo)
                                
                                p.TrackPos.RailsCode = FileEncoding.GetString(oBR.ReadBytes(1))
                                
                                If (i = 0) Then
                                    Me.HeaderKF = p
                                Else
                                    Try
                                        p.ID = PointID
                                        If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                        p.VerifyConstraints(Me.Constraints)
                                        PointCount += 1
                                        
                                        Yield p
                                        
                                    Catch ex As InvalidIDException
                                        Me.ParseErrors.Add(New ParseError(ParseErrorLevel.Error, ex.Message))
                                        If (Not CollectParseErrors) Then
                                            Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KfFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                        End If
                                        
                                    Catch ex As ParseException When (ex.ParseError IsNot Nothing)
                                        Me.ParseErrors.Add(ex.ParseError)
                                        If (Not CollectParseErrors) Then
                                            Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KfFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                                        End If
                                    End Try
                                End If
    		                Next
                        End Using
                        
                        ' Throw exception if parsing errors (constraints) has been collected.
                        If (Me.ParseErrors.HasErrors) Then
                            Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KfFile_LoadParsingFailed, Me.ParseErrors.ErrorCount, FilePath))
                        ElseIf (PointCount = 0) Then
                            Logger.logWarning(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_NoPoints, FilePath))
                        End If
                        
                        'Logger.logDebug(PointList.ToString())
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KfFile_LoadSuccess, PointCount, FilePath))
                        
                    Catch ex As ParseException
                        Throw
                    Catch ex as System.Exception
                        Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.KfFile_LoadFailed, FilePath), ex)
                    Finally
                        Me.ParseErrors.ToLoggingConsole()
                        'If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                    End Try
                End Get
            End Property
            
            ''' <summary> Writes the points collection to the point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <param name="MetaData">  Ignored. </param>
             ''' <remarks>
             ''' <para>
             ''' It's ensured that the point ID's written to the file are unique.
             ''' If there is at least one repeated or an invalid ID, this method fails with an exception.
             ''' </para>
             ''' <para>
             ''' <see cref="KfFile.HeaderKF"/> will be stored as the first record.
             ''' </para>
             ''' <para>
             ''' If this method fails, <see cref="GeoPointFile.ParseErrors"/> should provide the parse errors occurred."
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.InvalidOperationException"> <see cref="DataFile.FilePath"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Overrides Sub Store(PointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KfFile_StoreStart, Me.FilePath))
                    If (Me.FilePath.IsEmptyOrWhiteSpace()) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.DataFile_MissingFilePath)
                    
                    Me.Reset(Nothing)
                    
                    Dim PointCount As Integer = 0
                    Dim UniqueID   As Boolean = True  ' General Constraint for KF: Ensure unique ID.
                    
                    Using oBW As New BinaryWriter(File.Open(Me.FilePath, FileMode.Create, FileAccess.Write), FileEncoding)
                        
                        oBW.BaseStream.Seek(0, SeekOrigin.Begin)
                        
                        ' Write Header.
                        WriteRecord(oBW, Me.HeaderKF)
                        
                        ' Write Points.
                        For Each SourcePoint As IGeoPoint In PointList
                            Try
                                ' Convert point: This verifies the ID and provides all fields for writing.
                                Dim p As GeoVEPoint = SourcePoint.AsGeoVEPoint()
                                
                                ' Check for unique ID.
                                If (UniqueID) Then Me.VerifyUniqueID(p.ID)
                                
                                ' Write Point.
                                WriteRecord(oBW, p)
                                PointCount += 1
                                
                            Catch ex As InvalidIDException
                                Me.ParseErrors.Add(New ParseError(ParseErrorLevel.[Error], SourcePoint.SourceLineNo, 0, 0, ex.Message, SourcePoint.SourcePath))
                                If (Not Me.CollectParseErrors) Then
                                    Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KfFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                                End If
                            End Try
                        Next
                    End Using
                    
                    ' Throw exception if parsing errors has been collected.
                    If (Me.ParseErrors.HasErrors) Then
                        Throw New ParseException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.KfFile_StoreParsingFailed, Me.ParseErrors.ErrorCount, Me.FilePath))
                    End If
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.KfFile_StoreSuccess, PointCount, Me.FilePath))
                    
                Catch ex As ParseException
                    Throw
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.KfFile_StoreFailed, Me.FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Gets a normalized Double from a VE flavoured storage. </summary>
             ''' <param name="VEDouble"> Double from VE file. </param>
             ''' <returns> Unchanged input value or <c>Double.NaN</c> if <paramref name="VEDouble"/> = 1.0E+40. </returns>
            Private Function getDoubleFromVEDouble(VEDouble As Double) As Double
                Return If(VEDouble = 1.0E+40, Double.NaN, VEDouble)
            End Function
            
            ''' <summary> Gets a Double for VE flavoured storage from a normalized Double. </summary>
             ''' <param name="NormDouble"> Double from VE file. </param>
             ''' <returns> Unchanged input value or 1.0E+40 if <paramref name="NormDouble"/> = <c>Double.NaN</c>. </returns>
            Private Function getVEDoubleFromDouble(NormDouble As Double) As Double
                Return If(Double.IsNaN(NormDouble), 1.0E+40, NormDouble)
            End Function
            
            ''' <summary> Gets the default header record for KF. </summary>
            Private Function getDefaultKFHeader() As GeoVEPoint
                Dim p As New GeoVEPoint()
                
                'p.ID  (defaults to Null, which is o.k. for the header)
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
            
            ''' <summary> Writes one record to KF. </summary>
            Private Sub WriteRecord(oBW As BinaryWriter, p As GeoVEPoint)
                
                ' Conversions.
                Dim MarkType    As Byte = 0
                Dim mp          As Int16 = 0
                Dim mh          As Int16 = 0
                Dim ObjectKey   As Int32 = 0
                
                Byte.TryParse(p.MarkType, MarkType)
                If (Not Double.IsNaN(p.mp)) Then mp = CInt(p.mp)
                If (Not Double.IsNaN(p.mh)) Then mh = CInt(p.mh)
                Int32.TryParse(p.ObjectKey, ObjectKey)
                
                oBW.Write(p.IDToVEDouble())
                oBW.Write(getVEDoubleFromDouble(p.Y))
                oBW.Write(getVEDoubleFromDouble(p.X))
                oBW.Write(getVEDoubleFromDouble(p.Z))
                oBW.Write(getVEDoubleFromDouble(p.TrackPos.Kilometer.Value))
                
                oBW.Write(CByte(Asc(p.PositionPreInfo)))
                oBW.Write(GetByteArray(FileEncoding, p.Info, 13, " "c))
                oBW.Write(CByte(Asc(p.PositionPostInfo)))
                
                oBW.Write(CByte(Asc(p.HeightPreInfo)))
                oBW.Write(GetByteArray(FileEncoding, p.HeightInfo, 13, " "c))
                oBW.Write(CByte(Asc(p.HeightPostInfo)))
                
                oBW.Write(GetByteArray(FileEncoding, p.Kind, 4, " "c))
                oBW.Write(MarkType)
                oBW.Write(mp)
                oBW.Write(mh)
                oBW.Write(ObjectKey)
                oBW.Write(GetByteArray(FileEncoding, p.MarkHints, 1, " "c))
                oBW.Write(GetByteArray(FileEncoding, p.HeightSys, 3, " "c))
                oBW.Write(GetByteArray(FileEncoding, p.Job, 8, " "c))
                oBW.Write(If(p.sp.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.sp))))
                oBW.Write(If(p.sh.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.sh))))
                oBW.Write(GetByteArray(FileEncoding, CStr(p.TrackPos.TrackNo), 4, " "c, AdjustAtRight:=True))
                oBW.Write(If(p.TrackPos.RailsCode.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.TrackPos.RailsCode))))
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
