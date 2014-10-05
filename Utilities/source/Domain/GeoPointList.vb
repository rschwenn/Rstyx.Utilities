
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Imports PGK.Extensions
Imports Rstyx.Utilities
Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> Constraints to point values that are required for a certain purpose. </summary>
    <Flags>
    Public Enum GeoPointConstraints As Integer
        
        ''' <summary> No constraints. </summary>
        None = 0
        
        ''' <summary> The point's position has to be known, hence X and Y must not be <c>Double.NaN</c>. </summary>
        KnownPosition = 1
        
        ''' <summary> The point's height has to be known, hence Z does must not be <c>Double.NaN</c>. </summary>
        KnownHeight = 2
        
    End Enum
    
    
    ''' <summary> A generic, ready for use keyed collection for GeoPoint's. </summary>
     ''' <typeparam name="TPointID">  Type of Point ID. </typeparam>
     ''' <typeparam name="Point">     Type of collection items. It has to be or inherit from the <see cref="GeoPoint(Of TPointID)"/> interface. </typeparam>
     ''' <remarks>
     ''' The key for the collection will always be the <b>ID</b> property of <b>TItem</b>.
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> Basic suport for reading an ascii file. </description></item>
     ''' <item><description> Implements <see cref="IParseErrors"/> in order to support error handling. </description></item>
     ''' <item><description>  </description></item>
     ''' <item><description>  </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class GeoPointList(Of TPointID, Point As GeoPoint(Of TPointID))
        Inherits IDCollection(Of TPointID, GeoPoint(Of TPointID))
        Implements IParseErrors
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPointList")
            
        #End Region
        
        #Region "Protected Fields"
            
            ''' <summary> This will be used for dealing with text data. </summary>
            Protected LineStartCommentToken   As String = "#"
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): GeoPointList instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _Header As Collection(Of String)
            
            ''' <summary> Gets or sets header text lines. </summary>
             ''' <remarks> This is used for a text file. </remarks>
            Public Property Header() As Collection(Of String)
                Get
                    If _Header Is Nothing Then
                        _Header = New Collection(Of String)
                    End If
                    Return _Header
                End Get
                Set(value As Collection(Of String))
                    _Header = value
                End Set
            End Property
            
            ''' <summary> Determines logical constraints to be considered for the intended usage of points. Defaults to <c>None</c>. </summary>
             ''' <remarks> If any of these contraints is injured while reading from file, a <see cref="ParseError"/> will be created. </remarks>
            Public Property Constraints() As GeoPointConstraints
            
        #End Region
        
        #Region "IParseErrors Members"
            
            Private _ParseErrors As ParseErrorCollection
            
            ''' <inheritdoc/>
            Public Property ParseErrors() As ParseErrorCollection Implements IParseErrors.ParseErrors
                Get
                    If _ParseErrors Is Nothing Then
                        _ParseErrors = New ParseErrorCollection()
                    End If
                    Return _ParseErrors
                End Get
                Set(value As ParseErrorCollection)
                    _ParseErrors = value
                End Set
            End Property
            
            ''' <inheritdoc/>
            Public Property CollectParseErrors() As Boolean = False Implements IParseErrors.CollectParseErrors
            
            ''' <inheritdoc/>
            Public Property ShowParseErrorsInJedit() As Boolean = False Implements IParseErrors.ShowParseErrorsInJedit

        #End Region
        
        #Region "Methods"
            
            ''' <summary> Changes the point numbers according to a point change table. </summary>
             ''' <param name="PointChangeTab"> Table with Point pairs (source => target). </param>
             ''' <remarks></remarks>
            Public Sub changePointNumbers(PointChangeTab As Dictionary(Of TPointID, TPointID))
                'Dim dblPointNo  As Double
                Dim ChangeCount As Long = 0
                
                If (PointChangeTab.Count < 1) then
                    Logger.logInfo(Rstyx.Utilities.Resources.Messages.GeoPointList_EmptyPointChangeTab)
                Else
                    For Each Point As GeoPoint(Of TPointID) In Me
                        'dblPointNo = Math.Round(Point.ID, 5)
                        If (PointChangeTab.ContainsKey(Point.ID)) Then
                            Point.ID = PointChangeTab(Point.ID)
                            ChangeCount += 1
                        End If
                    Next
                    
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_ChangePointNumbersSuccess, ChangeCount))
                End If
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Clears this collection as well as <see cref="GeoPointList.ParseErrors"/> and <see cref="GeoPointList.Header"/>. </summary>
            Protected Overrides Sub ClearItems()
                MyBase.ClearItems()
                Me.ParseErrors.Clear()
                Me.Header.Clear()
            End Sub
            
            ''' <summary> Returns a list of all points in one string. </summary>
            Public Overrides Function ToString() As String
                
                Dim KvFmt As String = "%20s %15.5f%15.5f%10.4f  %-13s %-13s %-4s  %8s %8s %5.0f %5.0f  %5s %5s  %5s %5s  %-8s %7s"
                Dim PointList As New System.Text.StringBuilder()
                
                ' Header lines.
                If (Me.Header.Count > 0) Then
                    For Each HeaderLine As String In Me.Header
                        PointList.Append(LineStartCommentToken)
                        PointList.AppendLine(HeaderLine)
                    Next
                Else
                    ' Default Header.
                    'PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoPointList_Label_KvDefaultHeader1)
                    'PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoPointList_Label_KvDefaultHeader2)
                    'PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoPointList_Label_KvDefaultHeader3)
                    'PointList.AppendLine("#-----------------------------------------------------------------------------------------------------------------------------------------------------")
                End If
                
                ' Points.
                For Each p As GeoPoint(Of TPointID) In Me
                    
                    PointList.AppendLine(sprintf(KvFmt, P.ID.ToString(), IIf(Double.IsNaN(P.Y), 0, P.Y), IIf(Double.IsNaN(P.X), 0, P.X), IIf(Double.IsNaN(P.Z), 0, P.Z),
                                P.Info.TrimToMaxLength(13), P.HeightInfo.TrimToMaxLength(13),
                                P.Kind.TrimToMaxLength(4), P.CoordSys.TrimToMaxLength(8), P.HeightSys.TrimToMaxLength(8), P.mp, P.mh, 
                                P.MarkHints.TrimToMaxLength(5), P.MarkType.TrimToMaxLength(5), P.sp.TrimToMaxLength(5), P.sh.TrimToMaxLength(5),
                                P.Job.TrimToMaxLength(8), P.ObjectKey.TrimToMaxLength(7)))
                Next
                'PointList.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------")
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Protected Members"
            
            ''' <summary> Creates a byte array from a string. </summary>
             ''' <param name="TheEncoding"> The encoding to use. </param>
             ''' <param name="text">        Input string </param>
             ''' <param name="Length">      Given length of the byte array to return. </param>
             ''' <param name="FillChar">    If <paramref name="text"/> is shorter than <paramref name="Length"/>, it will be filled with this character. </param>
             ''' <returns> A byte array with given <paramref name="Length"/>. </returns>
             ''' <remarks> The input string will be trimmed to <paramref name="Length"/>. </remarks>
            Protected Function GetByteArray(TheEncoding As Encoding, text As String, Length As Integer, FillChar As Char, Optional AdjustAtRight As Boolean = False) As Byte()
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
                Return TheEncoding.GetBytes(TrimmedInput)
            End Function
            
            ''' <summary> Verifies that <paramref name="p"/> has a unique ID and also fulfills all given <see cref="Constraints"/>. </summary>
             ''' <param name="Point"> The point to verify. It should has set it's <see cref="GeoPoint(Of TPointID).SourceLineNo"/> to suport creation of a <see cref="ParseError"/>. </param>
             ''' <remarks>
             ''' If the list contained already a point with the ID of <paramref name="p"/>
             ''' or any of the <see cref="Constraints"/> is injured, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Point"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ParseException"> At least one constraint is injured. </exception>
            Protected Sub VerifyConstraints(Point As GeoPoint(Of TPointID))
                Me.VerifyConstraints(Point, Nothing, Nothing, Nothing, Nothing)
            End Sub
            
            ''' <summary> Verifies that <paramref name="p"/> has a unique ID and also fulfills all given <see cref="Constraints"/>. </summary>
             ''' <param name="Point">  The point to verify. It should has set it's <see cref="GeoPoint(Of TPointID).SourceLineNo"/> to suport creation of a <see cref="ParseError"/>. </param>
             ''' <param name="FieldX"> The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldY"> The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldZ"> The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' If the list contained already a point with the ID of <paramref name="p"/>
             ''' or any of the <see cref="Constraints"/> is injured, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Point"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ParseException"> At least one constraint is injured. </exception>
            Protected Sub VerifyConstraints(Point   As GeoPoint(Of TPointID),
                                            FieldID As DataField(Of TPointID),
                                            FieldX  As DataField(Of Double),
                                            FieldY  As DataField(Of Double),
                                            FieldZ  As DataField(Of Double)
                                           )
                
                If (Point  Is Nothing) Then Throw New System.ArgumentNullException("Point")
                
                Dim PointID  As String  = Point.ID.ToString()
                Dim StartCol As Integer = 0
                Dim Length   As Integer = 0
                
                ' Unique Point ID
                If (Me.Contains(Point.ID)) Then
                    If ((Point.SourceLineNo > 0) AndAlso (FieldID IsNot Nothing) AndAlso FieldID.HasSource) Then
                        Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                Point.SourceLineNo,
                                                                FieldID.Source.Column,
                                                                FieldID.Source.Column + FieldID.Source.Length,
                                                                sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_RepeatedPointID, PointID),
                                                                Nothing))
                    Else
                        Throw New ParseException(New ParseError(ParseErrorLevel.[Error], sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_RepeatedPointID, PointID)))
                    End If
                End If
                
                ' Position missing.
                If (Me.Constraints.HasFlag(GeoPointConstraints.KnownPosition)) Then
                    If (Double.IsNaN(Point.X) OrElse Double.IsNaN(Point.Y)) Then
                        If ((Point.SourceLineNo > 0) AndAlso (FieldX IsNot Nothing) AndAlso (FieldY IsNot Nothing) AndAlso FieldX.HasSource AndAlso FieldY.HasSource) Then
                            If (FieldX.Source.Column < FieldY.Source.Column) Then
                                StartCol = FieldX.Source.Column
                                Length   = FieldY.Source.Column + FieldY.Source.Length
                            Else
                                StartCol = FieldY.Source.Column
                                Length   = FieldX.Source.Column + FieldX.Source.Length
                            End If
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                    Point.SourceLineNo, StartCol, Length,
                                                                    sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPosition, PointID),
                                                                    Nothing))
                        Else
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error], sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPosition, PointID)))
                        End If
                    End If
                End If
                
                ' Heigt missing.
                If (Me.Constraints.HasFlag(GeoPointConstraints.KnownHeight)) Then
                    If (Double.IsNaN(Point.Z)) Then
                        If ((Point.SourceLineNo > 0) AndAlso (FieldZ IsNot Nothing) AndAlso FieldZ.HasSource) Then
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                    Point.SourceLineNo,
                                                                    FieldZ.Source.Column,
                                                                    FieldZ.Source.Column + FieldZ.Source.Length,
                                                                    sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingHeight, PointID),
                                                                    Nothing))
                        Else
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error], sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingHeight, PointID)))
                        End If
                    End If
                End If
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
