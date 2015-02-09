
Imports System
Imports System.Collections.ObjectModel
Imports System.Math

Imports Rstyx.Utilities

Namespace Math
    
    ''' <summary> A mathematical polygon. </summary>
     ''' <remarks>  </remarks>
    Public Class Polygon
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty polygon. </summary>
            Public Sub New()
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Dim _Vertices As Collection(Of Point)
            
            ''' <summary> The vertices of the polygon. </summary>
             ''' <remarks>
             ''' First and last vertice shouldn't ever be the same. If the polygon is meant to be closed,
             ''' the <see cref="Polygon.IsClosed"/> property has to be set to <see langword="true"/>.
             ''' </remarks>
             ''' <returns> The verttices of this polygon. Will never be <see langword="null"/>. </returns>
            Public Property Vertices() As Collection(Of Point)
                Get
                    If (_Vertices Is Nothing) Then
                        _Vertices = New Collection(Of Point)
                    End If
                    Return _Vertices
                End Get
                Set(value As Collection(Of Point))
                    _Vertices = value
                End Set
            End Property
            
            ''' <summary> Determines whether or not this polygon is closed. Defaults to <see langword="false"/>. </summary>
            Public IsClosed As Boolean = False
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Checks if <paramref name="TestPoint"/> is located inside a polygon in XY plane. </summary>
             ''' <param name="Pt">        The point of interest. </param>
             ''' <returns> <see langword="true"/> if the point is considered to be inside the polygon. </returns>
             ''' <remarks>
             ''' <see cref=" Point.Resolution"/> is considered as tolerance for treating the point to be on the polygon.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function GetMinimumDistanceToPointXY(Pt As Point) As Boolean
                Return GetMinimumDistanceToPointXY(Pt, Point.Resolution)
            End Function
            
            ''' <summary> Calculates the minimum distance to a point in XY plane. The sign determines the point's position. </summary>
             ''' <param name="Pt">        The target point. </param>
             ''' <param name="Tolerance"> Tolerance for treating the point to be on the polygon. </param>
             ''' <returns> Minimum distance to <paramref name="Pt"/> in XY plane. See Remarks! </returns>
             ''' <remarks>
             ''' <para>
             ''' The return value's sign determines the position of <paramref name="Pt"/> relative to the (closed) polygon:
             ''' <list type="table">
             ''' <listheader> <term> <b>Sign</b> </term>  <description> Position </description></listheader>
             ''' <item> <term> + </term>  <description> Outside polygon </description></item>
             ''' <item> <term> 0 </term>  <description> On the polygon ** </description></item>
             ''' <item> <term> - </term>  <description> Inside  polygon </description></item>
             ''' </list>
             ''' </para>
             ''' ** The point is considered to be on the polygon, if the calculated distance is not greater than <paramref name="Tolerance"/>.
             ''' In this case the return value will be set to <c>Zero</c>.
             ''' <para>
             ''' The distance is calculated perpendicular to the polygon. If this wasn't possible (i.e. near vertex), 
             ''' the distance to the nearest polygon vertex will be returned.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Public Function GetMinimumDistanceToPointXY(Pt As Point, Tolerance As Double) As Double
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("Pt")
                
                Dim MinDistance As Double  = Double.PositiveInfinity
                Dim Distance    As Double  = Double.NaN
                Dim j           As Integer = Me.Vertices.Count - 1
                
                ' Perpendicular distance to polygon edge.
                For i As Integer = 0 To Me.Vertices.Count - 1
                    Distance = Pt.GetDistanceToLineXY(_Vertices(j), _Vertices(i))
                    If ((Not Double.IsNaN(Distance)) AndAlso (Distance < MinDistance)) Then
                        MinDistance = Distance
                    End If
                    j = i
                Next
                
                ' Direct distance to polygon vertex.
                If (Double.IsPositiveInfinity(MinDistance)) Then
                    For i As Integer = 0 To Me.Vertices.Count - 1
                        Distance = Pt.GetDistanceToPointXY(_Vertices(i))
                        If (Distance < MinDistance) Then
                            MinDistance = Distance
                        End If
                        j = i
                    Next
                End If
                
                ' Check success.
                If (Double.IsPositiveInfinity(MinDistance)) Then
                    Throw New RemarkException("Polygon.GetMinimumDistanceToPointXY() failed to calculate a distance!")
                End If

                ' Tune return value.
                If (Not (MinDistance > Abs(Tolerance))) Then
                    MinDistance = 0.0000
                ElseIf (IsPointInsideXY(Pt)) Then
                    MinDistance = - MinDistance
                End If
                
                Return MinDistance
            End Function
            
            ''' <summary> Checks this Polygon for equality of vertices against a given Polygon considering a given tolerance for each point's coordinate. </summary>
             ''' <param name="ComparePolygon"> Polygon to compare with. </param>
             ''' <param name="Tolerance">      Tolerance in [m] for each point's coordinate. </param>
             ''' <remarks>
             ''' The "=" and "&lt;&gt;" operators consider the static <see cref="Point.Resolution"/> value as tolerance.
             ''' This method is intended to use another tolerance.
             ''' </remarks>
             ''' <returns> <see langword="true"/> if this Polygon equals <paramref name="ComparePolygon"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="ComparePolygon"/> is <see langword="null"/>. </exception>
            Public Function EqualsTolerance(ComparePolygon As Polygon, Tolerance As Double) As Boolean
                
                If (ComparePolygon Is Nothing) Then Throw New System.ArgumentNullException("ComparePolygon")
                
                Return Me.Equals(ComparePolygon, Tolerance, XYonly:=False)
            End Function
            
            ''' <summary> Checks two Polygons for equality of vertices in XY plane. <see cref=" Point.Resolution"/> is considered as tolerance for each coordinate. </summary>
             ''' <param name="ComparePolygon"> Polygon to compare with. </param>
             ''' <returns> <see langword="true"/> if this Polygon equals <paramref name="ComparePolygon"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="ComparePolygon"/> is <see langword="null"/>. </exception>
            Public Function EqualsXY(ComparePolygon As Polygon) As Boolean
                
                If (ComparePolygon Is Nothing) Then Throw New System.ArgumentNullException("ComparePolygon")
                
                Return Me.Equals(ComparePolygon, Point.Resolution, XYonly:=True)
            End Function
            
            ''' <summary> Checks two Polygons for equality of vertices in XY plane. <paramref name="Tolerance"/> is considered as tolerance for each coordinate. </summary>
             ''' <param name="ComparePolygon"> Polygon to compare with. </param>
             ''' <param name="Tolerance">      Tolerance in [m] for each coordinate. </param>
             ''' <returns> <see langword="true"/> if this Polygon equals <paramref name="ComparePolygon"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="ComparePolygon"/> is <see langword="null"/>. </exception>
            Public Function EqualsXY(ComparePolygon As Polygon, Tolerance As Double) As Boolean
                
                If (ComparePolygon Is Nothing) Then Throw New System.ArgumentNullException("ComparePolygon")
                
                Return Me.Equals(ComparePolygon, Tolerance, XYonly:=True)
            End Function
            
            ''' <summary> If start and end point of <paramref name="Polygon.Vertices"/> are the same point, the end point will be removed. </summary>
            Public Sub Trim()
                Dim VCount As Integer = Me.Vertices.Count
                If (VCount > 1) Then
                    If (_Vertices(0).EqualsXY(_Vertices(VCount - 1))) Then
                        _Vertices.RemoveAt(VCount - 1)
                    End If
                End If
            End Sub
            
        #End Region
        
        #Region "Operators"
            
            ''' <summary> Checks two poygons for equality of vertices. <see cref=" Point.Resolution"/> is considered as tolerance for each point's coordinate. </summary>
             ''' <param name="P1"> The first operand. </param>
             ''' <param name="P2"> The second operand. </param>
             ''' <returns> <see langword="true"/> if <paramref name="P1"/> equals <paramref name="P2"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="P1"/> or <paramref name="P2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator = (ByVal P1 As Polygon, ByVal P2 As Polygon) As Boolean
                
                If (P1 Is Nothing) Then Throw New System.ArgumentNullException("P1")
                If (P2 Is Nothing) Then Throw New System.ArgumentNullException("P2")
                
                Return ( P1.Equals(P2, Point.Resolution, XYonly:=False) )
            End Operator
            
            ''' <summary> Checks two poygons for unequality of vertices. <see cref=" Point.Resolution"/> is considered as tolerance for each point's coordinate. </summary>
             ''' <param name="P1"> The first operand. </param>
             ''' <param name="P2"> The second operand. </param>
             ''' <returns> <see langword="true"/> if <paramref name="P1"/> doesn't equal <paramref name="P2"/>, otherwise <see langword="false"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="P1"/> or <paramref name="P2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator <> (ByVal P1 As Polygon, ByVal P2 As Polygon) As Boolean
                
                If (P1 Is Nothing) Then Throw New System.ArgumentNullException("P1")
                If (P2 Is Nothing) Then Throw New System.ArgumentNullException("P2")
                
                Return ( Not (P1 = P2) )
            End Operator
            
        #End Region
        
        #Region "Private Methods"
            
            ''' <summary> Checks two Polygons for equality of vertices in XY plane. <paramref name="Tolerance"/> is considered as tolerance for each coordinate. </summary>
             ''' <param name="ComparePolygon"> Polygon to compare with. </param>
             ''' <param name="Tolerance">      Tolerance in [m] for each coordinate. </param>
             ''' <param name="XYonly">         If <see langword="true"/>, the Z coordinate will be ignored. </param>
             ''' <returns> <see langword="true"/> if this Polygon equals <paramref name="ComparePolygon"/>, otherwise <see langword="false"/>. </returns>
            Private Overloads Function Equals(ComparePolygon As Polygon, Tolerance As Double, XYonly As Boolean) As Boolean
                Dim IsEqual As Boolean = False
                
                If (ComparePolygon IsNot Nothing) Then
                    If (Me.Vertices.Count = ComparePolygon.Vertices.Count) Then
                        IsEqual = True
                        For i As Integer = 0 To Me.Vertices.Count - 1
                            If (XYonly) Then
                                IsEqual =  Me.Vertices(i).EqualsXY(ComparePolygon.Vertices(i), Tolerance)
                            Else
                                IsEqual =  Me.Vertices(i).EqualsTolerance(ComparePolygon.Vertices(i), Tolerance)
                            End If
                            If (Not IsEqual) Then
                                Exit For
                            End If
                        Next
                    End If
                End If
                
                Return IsEqual
            End Function
            
            ''' <summary> Checks if <paramref name="TestPoint"/> is located inside a polygon in XY plane. </summary>
             ''' <param name="Pt">        The point of interest. </param>
             ''' <returns> <see langword="true"/> if the point is considered to be inside the polygon. </returns>
             ''' <remarks>
             ''' <para>
             ''' It doesn't matter whether vertices are defined clockwise or anticlockwise.
             ''' </para>
             ''' <para>
             ''' This method doesn't consider any tolerance, but there may be roundoff errors.
             ''' </para>
             ''' <para>
             ''' see http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
             ''' </para>
             ''' <para>
             ''' see http://www.codeproject.com/Tips/84226/Is-a-Point-inside-a-Polygon
             ''' </para>
             ''' <para>
             ''' see http://codekicker.de/fragen/polygon-Liegt-Punkt-Polygon-C%23-punkt
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Private Function IsPointInsideXY(Pt As Point) As Boolean
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("Pt")
                
                Dim IsInside As Boolean = False
                
                'If (MayContainPointXY(Pt)) Then
                    
                    Dim j As Integer = Me.Vertices.Count - 1
                    
                    For i As Integer = 0 To Me.Vertices.Count - 1
                        
                        If ((Not ((_Vertices(i).Y > Pt.Y) = (_Vertices(j).Y > Pt.Y))) AndAlso
                            (Pt.X < (_Vertices(j).X - _Vertices(i).X) * (Pt.Y - _Vertices(i).Y) / (_Vertices(j).Y - _Vertices(i).Y) + _Vertices(i).X)
                            ) Then
                            IsInside = (Not IsInside)
                        End If
                        j = i
                    Next
                'End If
                
                Return IsInside
            End Function
            
            ''' <summary> Checks if <paramref name="TestPoint"/> may be located inside this polygon in XY plane at all. </summary>
             ''' <param name="Pt"> The point of interest. </param>
             ''' <returns> <see langword="false"/> if the point can't be inside the polygon. </returns>
             ''' <remarks> This method compares minimum and maximum coordinates of point and polygon only as a quick test. </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Pt"/> is <see langword="null"/>. </exception>
            Private Function MayContainPointXY(Pt As Point) As Boolean
                
                If (Pt Is Nothing) Then Throw New System.ArgumentNullException("TestPoint")
                
                Dim MayInside As Boolean = True
                Dim PolyMinX  As Double  = Double.MaxValue
                Dim PolyMinY  As Double  = Double.MaxValue
                Dim PolyMaxX  As Double  = Double.MinValue
                Dim PolyMaxY  As Double  = Double.MinValue
                
                ' Polygon range.
                For Each Vertex As Point In Me.Vertices
                    PolyMinX = Min(Vertex.X, PolyMinX)
                    PolyMinY = Min(Vertex.Y, PolyMinY)
                    PolyMaxX = Max(Vertex.X, PolyMaxX)
                    PolyMaxY = Max(Vertex.Y, PolyMaxY)
                Next
                
                ' Check if point is outside Polygon range.
                If ((Pt.X < PolyMinX) OrElse
                    (Pt.Y < PolyMinY) OrElse
                    (Pt.X > PolyMaxX) OrElse
                    (Pt.Y > PolyMaxY)
                    ) Then
                    MayInside = False
                End If
                
                Return MayInside
            End Function

        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
