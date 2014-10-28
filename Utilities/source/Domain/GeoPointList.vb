
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Imports Rstyx.Utilities.Collections
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
    
    
    ''' <summary> A collection of GeoPoint's. </summary>
     ''' <remarks>
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> The key for the collection will always be the <b>ID</b> property of <b>TItem</b>. </description></item>
     ''' <item><description> Every collection item has to implement the <see cref="IGeoPoint"/> interface. There are no more restrictions. </description></item>
     ''' <item><description> Manipulation method for changing the point ID's according to a point change table. </description></item>
     ''' <item><description>  </description></item>
     ''' <item><description>  </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class GeoPointList
        Inherits IDCollection(Of IGeoPoint)
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPointList")
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): GeoPointList instantiated")
            End Sub
            
            ''' <summary> Creates a new GeoPointList and inititializes it's items from any given <see cref="IDCollection(Of IGeoPoint)"/>. </summary>
             ''' <param name="SourcePointList"> The source point list to get initial points from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of at least one <paramref name="SourcePoint"/> isn't a valid ID for the target point. </exception>
            Public Sub New(SourcePointList As GeoPointList)
                If (SourcePointList IsNot Nothing) Then
                    For Each SourcePoint As IGeoPoint In SourcePointList
                        Me.Add(SourcePoint)
                    Next
                End If
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _Header As Collection(Of String)
            
            ''' <summary> Gets or sets header text lines for the list. </summary>
             ''' <remarks> This may be used for a text file. </remarks>
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
             ''' <remarks> Used by <see cref="GeoPointList.VerifyConstraints"/>. </remarks>
            Public Property Constraints() As GeoPointConstraints
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Changes the point numbers of this list according to a point change table. </summary>
             ''' <param name="PointChangeTab"> Table with Point pairs (source => target). </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="PointChangeTab"/> is <see langword="null"/>. </exception>
            Public Sub changePointNumbers(PointChangeTab As Dictionary(Of String, String))
                
                If (PointChangeTab Is Nothing) Then Throw New System.ArgumentNullException("PointChangeTab")
                
                Dim ChangeCount As Long = 0
                
                If (PointChangeTab.Count < 1) then
                    Logger.logWarning(Rstyx.Utilities.Resources.Messages.GeoPointList_EmptyPointChangeTab)
                Else
                    For Each Point As GeoPoint In Me
                        
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
            
            ''' <summary> Clears this collection as well as the <see cref="GeoPointList.Header"/>. </summary>
            Protected Overrides Sub ClearItems()
                MyBase.ClearItems()
                Me.Header.Clear()
            End Sub
            
            ''' <summary> Returns a list of all points in one string. </summary>
            Public Overrides Function ToString() As String
                
                Dim PointFmt  As String = " %20s %15.5f%15.5f%10.4f  %-13s %-13s %-4s  %8s %8s %5.0f %5.0f  %5s %5s  %5s %5s  %-8s %7s"
                Dim PointList As New System.Text.StringBuilder()
                
                ' Header lines.
                If (Me.Header.Count > 0) Then
                    For Each HeaderLine As String In Me.Header
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
                For Each p As GeoPoint In Me
                    
                    PointList.AppendLine(sprintf(PointFmt, P.ID, If(Double.IsNaN(P.Y), 0, P.Y), If(Double.IsNaN(P.X), 0, P.X), If(Double.IsNaN(P.Z), 0, P.Z),
                                         P.Info.TrimToMaxLength(13), P.HeightInfo.TrimToMaxLength(13),
                                         P.Kind.TrimToMaxLength(4), P.CoordSys.TrimToMaxLength(8), P.HeightSys.TrimToMaxLength(8), P.mp, P.mh, 
                                         P.MarkHints.TrimToMaxLength(5), P.MarkType.TrimToMaxLength(5), P.sp.TrimToMaxLength(5), P.sh.TrimToMaxLength(5),
                                         P.Job.TrimToMaxLength(8), P.ObjectKey.TrimToMaxLength(7)
                                        ))
                Next
                'PointList.AppendLine("------------------------------------------------------------------------------------------------------------------------------------------------------")
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Protected Members"
            
            ''' <summary> Verifies that <paramref name="p"/> has a unique ID and also fulfills all given <see cref="Constraints"/>. </summary>
             ''' <param name="Point"> The point to verify. It should has set it's <see cref="GeoPoint.SourceLineNo"/> to suport creation of a <see cref="ParseError"/>. </param>
             ''' <remarks>
             ''' If the list contained already a point with the ID of <paramref name="p"/>
             ''' or any of the <see cref="Constraints"/> is injured, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Point"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ParseException"> At least one constraint is injured. </exception>
            Public Sub VerifyConstraints(Point As GeoPoint)
                Me.VerifyConstraints(Point, Nothing, Nothing, Nothing, Nothing)
            End Sub
            
            ''' <summary> Verifies that <paramref name="p"/> has a unique ID and also fulfills all given <see cref="Constraints"/>. </summary>
             ''' <param name="Point">   The point to verify. It should has set it's <see cref="GeoPoint.SourceLineNo"/> to suport creation of a <see cref="ParseError"/>. </param>
             ''' <param name="FieldID"> The parsed data field of point ID. May be <see langword="null"/>. </param>
             ''' <param name="FieldX">  The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldY">  The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldZ">  The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' If the list contained already a point with the ID of <paramref name="p"/>
             ''' or any of the <see cref="Constraints"/> is injured, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Point"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ParseException"> At least one constraint is injured. </exception>
            Public Sub VerifyConstraints(Point   As GeoPoint,
                                         FieldID As DataField(Of String),
                                         FieldX  As DataField(Of Double),
                                         FieldY  As DataField(Of Double),
                                         FieldZ  As DataField(Of Double)
                                        )
                
                If (Point  Is Nothing) Then Throw New System.ArgumentNullException("Point")
                
                Dim PointID  As String  = Point.ID
                Dim StartCol As Integer = 0
                Dim EndCol   As Integer = 0
                
                ' Unique Point ID.
                If (Me.Contains(Point.ID)) Then
                    Throw New ParseException(ParseError.Create(ParseErrorLevel.[Error],
                                                               Point.SourceLineNo,
                                                               FieldID,
                                                               sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_RepeatedPointID, PointID),
                                                               Nothing,
                                                               Nothing
                                                              ))
                End If
                
                ' Position missing.
                If (Me.Constraints.HasFlag(GeoPointConstraints.KnownPosition)) Then
                    If (Double.IsNaN(Point.X) OrElse Double.IsNaN(Point.Y)) Then
                        If ((Point.SourceLineNo > 0) AndAlso (FieldX IsNot Nothing) AndAlso (FieldY IsNot Nothing) AndAlso FieldX.HasSource AndAlso FieldY.HasSource) Then
                            If (FieldX.Source.Column < FieldY.Source.Column) Then
                                StartCol = FieldX.Source.Column
                                EndCol   = FieldY.Source.Column + FieldY.Source.Length
                            Else
                                StartCol = FieldY.Source.Column
                                EndCol   = FieldX.Source.Column + FieldX.Source.Length
                            End If
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                    Point.SourceLineNo, StartCol, EndCol,
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
                        Throw New ParseException(ParseError.Create(ParseErrorLevel.[Error],
                                                                   Point.SourceLineNo,
                                                                   FieldZ,
                                                                   sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingHeight, PointID),
                                                                   Nothing,
                                                                   Nothing
                                                                  ))
                    End If
                End If
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
