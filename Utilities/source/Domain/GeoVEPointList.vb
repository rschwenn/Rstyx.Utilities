
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
     ''' <item><description> Manipulation methods: - changing the point numbers according to a point change table. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class GeoVEPointList
        Inherits IDCollection(Of Double, GeoVEPoint)
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Apps.VEPoints")
            
            Private VEencoding      As Encoding = Encoding.Default
            Private PointNoFactor   As Integer = 100000
            Private RecordLength    As Integer = 102
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): VEPoints instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _HeaderKF  As GeoVEPoint
            
            ''' <summary> Gets or sets the Header of the binary point file. It returns the default header, if not changed before (i.e. read from binary file). </summary>
            Public Property HeaderKF() As GeoVEPoint
                Get
                    If _HeaderKF Is Nothing Then
                        _HeaderKF = getDefaultHeader()
                    End If
                    Return _HeaderKF
                End Get
                Set(value As GeoVEPoint)
                    _HeaderKF = value
                End Set
            End Property
            
            ''' <summary> Gets or sets the Header lines of the ascii point file. </summary>
            Public Property HeaderAscii() As Collection(Of String)
            
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
                            p.MarkHints          = Trim(VEencoding.GetString(oBR.ReadBytes(1)))  ' Stabilitätskennzeichen
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
                                Me.Add(p)
                            End If
    		            Next
                    End Using
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadBinarySuccess, Me.Count, FilePath))
                    
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadBinaryFailed, FilePath), ex)
                End Try
            End Sub
            
            ''' <summary> Reads the ascii "KV" point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to read from. </param>
             ''' <remarks>
             ''' 
             ''' </remarks>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub readFromKvFile(FilePath As String)
                Try
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvStart, FilePath))
                    
                    Me.Clear()
                    
                    Dim FileReader As New DataTextFileReader()
                    FileReader.LineStartCommentToken = "#"
                    FileReader.Load(FilePath)
                    
                    For i As Integer = 0 To FileReader.DataCache.Count - 1
                        
                        Dim SplitLine As DataTextLine = FileReader.DataCache(i)
                        
                        If (SplitLine.HasData) Then
    		            	
    		                Dim p As New GeoVEPoint()
    		                
                            p.ID                 = getDoubleFromVEDouble(oSR.ReadDouble())
                            p.Y                  = getDoubleFromVEDouble(oSR.ReadDouble())
                            p.X                  = getDoubleFromVEDouble(oSR.ReadDouble())
                            p.Z                  = getDoubleFromVEDouble(oSR.ReadDouble())
                            p.TrackPos.Kilometer = New Kilometer(getDoubleFromVEDouble(oSR.ReadDouble()))
                            
                            p.PositionPreInfo    = CChar(VEencoding.GetString(oSR.ReadBytes(1)))
                            p.Info               = Trim(VEencoding.GetString(oSR.ReadBytes(13)))
                            p.PositionPostInfo   = CChar(VEencoding.GetString(oSR.ReadBytes(1)))
                            
                            p.HeightPreInfo      = CChar(VEencoding.GetString(oSR.ReadBytes(1)))
                            p.HeightInfo         = Trim(VEencoding.GetString(oSR.ReadBytes(13)))
                            p.HeightPostInfo     = CChar(VEencoding.GetString(oSR.ReadBytes(1)))
                            
                            p.Kind               = Trim(VEencoding.GetString(oSR.ReadBytes(4)))
                            p.MarkType           = CStr(oSR.ReadByte)
                            p.mp                 = CDbl(oSR.ReadInt16())
                            p.mh                 = CDbl(oSR.ReadInt16())
                            p.ObjectKey          = oSR.ReadInt32()
                            p.MarkHints          = Trim(VEencoding.GetString(oSR.ReadBytes(1)))  ' Stabilitätskennzeichen
                            p.HeightSys          = Trim(VEencoding.GetString(oSR.ReadBytes(3)))
                            p.Job                = Trim(VEencoding.GetString(oSR.ReadBytes(8)))
                            p.sp                 = Trim(VEencoding.GetString(oSR.ReadBytes(1)))
                            p.sh                 = Trim(VEencoding.GetString(oSR.ReadBytes(1)))
                            
                            Dim TrackNo As String = Trim(VEencoding.GetString(oSR.ReadBytes(4)))
                            Integer.TryParse(TrackNo, p.TrackPos.TrackNo)
                            
                            p.TrackPos.RailsCode = VEencoding.GetString(oSR.ReadBytes(1))
                            
                            If (i = 0) Then
                                _HeaderKF = p
                            Else
                                Me.Add(p)
                            End If
                        End If
                    Next
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvSuccess, Me.Count, FilePath))
                    
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_ReadKvFailed, FilePath), ex)
                End Try
            End Sub
            
            ''' <summary> Writes the binary Verm.esn point file from the points collection. </summary>
             ''' <param name="FilePath"> File to write. </param>
             ''' <remarks>
             ''' The first point represents the file header (<see cref="GeoVEPointList.Header"/>).
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
                            oBW.Write(GetByteArray(p.Info, 13, " "c))
                            oBW.Write(CByte(Asc(p.PositionPostInfo)))
                            
                            oBW.Write(CByte(Asc(p.HeightPreInfo)))
                            oBW.Write(GetByteArray(p.HeightInfo, 13, " "c))
                            oBW.Write(CByte(Asc(p.HeightPostInfo)))
                            
                            oBW.Write(GetByteArray(p.Kind, 4, " "c))
                            oBW.Write(MarkType)
                            oBW.Write(mp)
                            oBW.Write(mh)
                            oBW.Write(ObjectKey)
                            oBW.Write(GetByteArray(p.MarkHints, 1, " "c))
                            oBW.Write(GetByteArray(p.HeightSys, 3, " "c))
                            oBW.Write(GetByteArray(p.Job, 8, " "c))
                            oBW.Write(IIf(p.sp.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.sp))))
                            oBW.Write(IIf(p.sh.IsEmptyOrWhiteSpace(), " "c, CByte(Asc(p.sh))))
                            oBW.Write(GetByteArray(CStr(p.TrackPos.TrackNo), 4, " "c, AdjustAtRight:=True))
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
                    
                    Using oSW As StreamWriter = System.IO.File.CreateText(FilePath)
                        oSW.Write(Me.ToKV())
                    End Using
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteKvSuccess, Me.Count, FilePath))
                    
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.GeoVEPointList_WriteKvFailed, FilePath), ex)
                End Try
            End Sub
            
            ''' <summary> Changes the point numbers according to the the point change table. </summary>
             ''' <param name="PointChangeTab"> Table with Point pairs (source => target). </param>
             ''' <remarks></remarks>
            Public Sub changePointNumbers(PointChangeTab As Dictionary(Of Double, Double))
                Dim dblPointNo  As Double
                Dim ChangeCount As Long = 0
                
                If (PointChangeTab.Count < 1) then
                    Logger.logInfo("Zuordnungstabelle ist leer => nichts zu tun.")
                Else
                    For Each Point As GeoVEPoint In Me
                        dblPointNo = Math.Round(Point.ID, 5)
                        If (PointChangeTab.ContainsKey(dblPointNo)) Then
                            Point.ID = PointChangeTab(dblPointNo)
                            ChangeCount += 1
                        End If
                    Next
                    
                    Logger.logInfo(sprintf("=> %d Punktnummern geändert.", ChangeCount))
                End If
            End Sub
            
            ''' <summary> Returns a list of all points in one KV formatted string. </summary>
            Public Function ToKV() As String
                Dim KvFmt As String = "%7.0f %15.5f%15.5f%10.4f %12.4f  %-13s %-13s %-4s %4d %1s  %3s %5.0f %5.0f  %1s %+3s  %1s%1s  %-8s %7s"
                Dim PointList As New System.Text.StringBuilder()
                PointList.AppendLine("#                                                                                                                     Vermarkungsart    ")
                PointList.AppendLine("#                                                                                                 RiKz            StabilKz   |  LH-Status")
                PointList.AppendLine("# PktNr  ____Rechtswert ______Hochwert ____Hoehe _____Station  Erlaeute_Lage Erlaeut_Hoehe PArt Str. 5  HSy ___mp ___mh  S __V  12  Auftrag# OSKA-Nr")
                PointList.AppendLine("#-----------------------------------------------------------------------------------------------------------------------------------------------------")
                
                For Each p As GeoVEPoint In Me
                    
                    PointList.AppendLine(sprintf(KvFmt, P.ID * PointNoFactor, IIf(Double.IsNaN(P.Y), 0, P.Y), IIf(Double.IsNaN(P.X), 0, P.X), P.TrackPos.Kilometer.Value, P.Info.TrimToMaxLength(13), P.HeightInfo.TrimToMaxLength(13),
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
                PointList.AppendLine("                                                                                                                      Vermarkungsart    ")
                PointList.AppendLine("                                                                                                  RiKz            StabilKz   |  LH-Status")
                PointList.AppendLine("  PktNr  ____Rechtswert ______Hochwert ____Hoehe _____Station  Erlaeute_Lage Erlaeut_Hoehe PArt Str. 5  HSy ___mp ___mh  S __V  12  Auftrag# OSKA-Nr")
                PointList.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------")
                
                For Each p As GeoVEPoint In Me
                    PointList.AppendLine(p.ToString())
                Next
                PointList.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------")
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Gets a normalized Double from a VE flavoured storege. </summary>
             ''' <param name="VEDouble"> Double from VE file. </param>
             ''' <returns> Unchanged input value or <c>Double.NaN</c> if <paramref name="VEDouble"/> = 1.0E+40. </returns>
            Private Function getDoubleFromVEDouble(VEDouble As Double) As Double
                Return IIf(VEDouble = 1.0E+40, Double.NaN, VEDouble)
            End Function
            
            ''' <summary> Gets a normalized Double from a VE flavoured storege. </summary>
             ''' <param name="NormDouble"> Double from VE file. </param>
             ''' <returns> Unchanged input value or 1.0E+40 if <paramref name="NormDouble"/> = <c>Double.NaN</c>. </returns>
            Private Function getVEDoubleFromDouble(NormDouble As Double) As Double
                Return IIf(Double.IsNaN(NormDouble), 1.0E+40, NormDouble)
            End Function
            
            Private Function getDefaultHeader() As GeoVEPoint
                Dim p As New GeoVEPoint()
                
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
            
            '' <summary> Creates a byte array from a string. </summary>
             ''' <param name="text">     Input string </param>
             ''' <param name="Length">   Given length of the byte array to return. </param>
             ''' <param name="FillChar"> If <paramref name="text"/> is shorter than <paramref name="Length"/>, it will be filled with this character. </param>
             ''' <returns> A byte array with given <paramref name="Length"/>. </returns>
             ''' <remarks> The input string will be trimmed to <paramref name="Length"/>. </remarks>
            Private Function GetByteArray(text As String, Length As Integer, FillChar As Char, Optional AdjustAtRight As Boolean = False) As Byte()
                Dim TrimmedInput As String = text
                If (TrimmedInput.Length > Length) Then
                    TrimmedInput = text.Left(Length)
                ElseIf (TrimmedInput.Length < Length) Then
                    If (AdjustAtRight) Then
                        TrimmedInput = text.PadLeft(Length, FillChar)
                    Else
                        TrimmedInput = text.PadRight(Length, FillChar)
                    End If
                End If
                Return VEencoding.GetBytes(TrimmedInput)
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
