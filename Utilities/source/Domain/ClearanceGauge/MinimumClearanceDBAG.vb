
Imports System
Imports System.Math
Imports System.Collections.Generic

Imports Rstyx.Utilities.Math


Namespace Domain.ClearanceGauge
    
    ''' <summary> Minimum clearance outline for DBAG railway track. </summary>
    Public Class MinimumClearanceDBAG
        
        #Region "Private Fields"
            
            'Private Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.ClearanceGauge.MinimumClearanceDBAG")
            
            Const S0                    As Double  = 0.005   ' Influence of track gauge widening / deviation. Corresponds to EBO appendix 2 table 2.1.1 (gauge = 1.445).
            Const hc                    As Double  = 0.500   ' Wankpolhöhe.
            
            Const MinCantBase           As Double  = 0.01
            Const CantZeroTol           As Double  = 0.001
            Const RadiusInfinityTol     As Double  = 0.001
            
            Private Shared Table212i    As LinearTabFunction
            Private Shared Table212a    As LinearTabFunction
            Private Shared MinBaseLine  As Polygon
            
            Private Shared RHO          As Double = 180 / PI
            Private Shared T3i0         As Double = Tan(0.2 / RHO)
            Private Shared T3a0         As Double = Tan(1.0 / RHO)
            Private Shared T40          As Double = Tan(1.0 / RHO) * 50 / 65
            Private Shared T50          As Double = Tan(1.0 / RHO) * 15 / 65
            
            Private IsMainOutlineValid  As Boolean = False
            Private IsOHLOutlineValid   As Boolean = False
            Private IsSmallRadius250    As Boolean = False
            
            Private NormalizedCant      As Double = Double.NaN
            Private RelativeCant        As Double = Double.NaN
            Private UnsignedCant        As Double = Double.NaN
            Private AbsRadius           As Double = Double.NaN
            Private u0                  As Double = Double.NaN
            Private uf                  As Double = Double.NaN
            Private Q0                  As Double = Double.NaN
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Shared initialization of MinimumClearanceDBAG. </summary>
            Shared Sub New()
                _ReferenceLineG2 = CreateG2()
                MinBaseLine      = CreateMinBaseLine()
                CreateTables()
            End Sub
            
            ''' <summary> Creates a new, empty MinimumClearanceDBAG. </summary>
            Public Sub New()
            End Sub
            
        #End Region
        
        #Region "Result Properties"
            
            Private Shared ReadOnly _ReferenceLineG2 As Polygon
            Private _MainOutline    As Polygon
            Private _OHLOutline     As Polygon
            
            ''' <summary> Gets the static definition of G2. </summary>
             ''' <remarks> The lower part is for all rails that may contain changes in vertical curve. </remarks>
            Shared ReadOnly Property ReferenceLineG2()  As Polygon
                Get
                    Return _ReferenceLineG2
                End Get
            End Property
            
            ''' <summary> Returns the main outline of minimum clearance. </summary>
             ''' <remarks>  </remarks>
            Public ReadOnly Property MainOutline()  As Polygon
                Get
                    If (Not IsMainOutlineValid) Then
                        _MainOutline = CalculateMainOutline()
                        IsMainOutlineValid = True
                    End If
                    Return _MainOutline
                End Get
            End Property
            
            ''' <summary> Returns the outline for overhead line area of minimum clearance. </summary>
             ''' <remarks>  </remarks>
            Public ReadOnly Property OHLOutline()   As Polygon
                Get
                    If (Not IsOHLOutlineValid) Then
                        _OHLOutline = CalculateOHLOutline()
                        IsOHLOutlineValid = True
                    End If
                    Return _OHLOutline
                End Get
            End Property
            
        #End Region
        
        #Region "Input Properties"
            
            Dim _Speed          As Double  = Double.NaN
            Dim _Radius         As Double  = Double.NaN
            Dim _Cant           As Double  = Double.NaN
            Dim _CantBase       As Double  = Double.NaN
            Dim _OverHeadLine   As ClearanceOptionalPart = ClearanceOptionalPart.None
            
            ''' <summary> Gets or sets the Speed in [km/h]. </summary>
            Public Property Speed As Double
                Get 
                    Return _Speed
                End Get
                Set(value As Double)
                    _Speed = value
                    SetAuxValues()
                End Set
            End Property
            
            ''' <summary> Gets or sets the Radius in [m]. </summary>
             ''' <remarks> Trying to set a value close to <c>Zero</c> (0.0+-0.001) results in a value of <c>Double.PositiveInfinity</c> </remarks>
            Public Property Radius As Double
                Get 
                    Return _Radius
                End Get
                Set(value As Double)
                    If (Double.IsNaN(value)) Then
                        _Radius   = Double.NaN
                        AbsRadius = Double.NaN
                        
                    ElseIf (value.EqualsTolerance(0.0, RadiusInfinityTol)) Then
                        _Radius   = Double.PositiveInfinity
                        AbsRadius = Double.PositiveInfinity
                        IsSmallRadius250 = False
                    Else
                        _Radius   = value
                        AbsRadius = Abs(value)
                        IsSmallRadius250 = (Round(AbsRadius, 3) < 250.0)
                    End If
                    
                    SetAuxValues()
                End Set
            End Property
            
            ''' <summary> Gets or sets the Absolute Cant in [m] (negative, if right running surface is higher). </summary>
            Public Property Cant As Double
                Get 
                    Return _Cant
                End Get
                Set(value As Double)
                    _Cant = value
                    SetAuxValues()
                End Set
            End Property
            
            ''' <summary> Gets or sets the CantBase in [m]. </summary>
            Public Property CantBase As Double
                Get 
                    Return _CantBase
                End Get
                Set(value As Double)
                    _CantBase = value
                    SetAuxValues()
                End Set
            End Property
            
            ''' <summary> Gets or sets the type of OverHeadLine. </summary>
            Public Property OverHeadLine() As ClearanceOptionalPart
                Get 
                    Return _OverHeadLine
                End Get
                Set(value As ClearanceOptionalPart)
                    If (Not (value = _OverHeadLine)) Then
                        _OverHeadLine = value
                        IsOHLOutlineValid  = False
                    End If
                End Set
            End Property
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Sets geometry input values from a <see cref="GeoTcPoint"/>. </summary>
             ''' <param name="GeometryConditions"> The point which provides cant, cant base and radius. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="GeometryConditions"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentException"> Cant     (<paramref name="GeometryConditions.Ueb"/>) is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> CantBase (<paramref name="GeometryConditions.CantBase"/>) is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> Radius   (<paramref name="GeometryConditions.Ra"/>) is <c>Double.NaN</c> or Zero, but Cant isn't zero. </exception>
            Public Sub SetGeometry(GeometryConditions As GeoTcPoint)
                
                If (GeometryConditions Is Nothing)              Then Throw New System.ArgumentNullException("GeometryConditions")
                If (Double.IsNaN(GeometryConditions.Ueb))       Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_SetGeometry_UnknownCant)
                If (Double.IsNaN(GeometryConditions.CantBase))  Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_SetGeometry_UnknownCantBase)
                
                Me.Radius   = GeometryConditions.Ra
                Me.CantBase = GeometryConditions.CantBase
                
                If (GeometryConditions.Ueb < CantZeroTol) Then
                    Me.Cant = 0.0
                Else
                    If (Double.IsNaN(GeometryConditions.Ra) OrElse GeometryConditions.Ra.EqualsTolerance(0.0, RadiusInfinityTol)) Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_SetGeometry_UnknownRadius)
                    Me.Cant = GeometryConditions.Ueb * Sign(GeometryConditions.Ra)
                End If
            End Sub
            
        #End Region
        
        #Region "Definition of polygons"
            
            ''' <summary> Creates the definition of reference line G2. </summary>
            ''' <remarks> See EBO, appendix 8 (http://www.gesetze-im-internet.de/ebo/anlage_8.html) </remarks>
            Private Shared Function CreateG2() As Polygon
                
                Dim PolygonG2 As New Polygon()
                PolygonG2.IsClosed = True
                
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.6785, .Y= 0.080})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.6785, .Y= 0.030})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.8475, .Y= 0.030})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.8475, .Y= 0.060})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.8730, .Y= 0.060})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.8730, .Y= 0.073})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.9175, .Y= 0.073})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.9175, .Y= 0.080})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.1750, .Y= 0.100})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.2500, .Y= 0.130})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.5200, .Y= 0.400})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.6200, .Y= 0.400})
                
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.620 , .Y= 1.170})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.645 , .Y= 1.170})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.645 , .Y= 3.530})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-1.470 , .Y= 3.835})
                PolygonG2.Vertices.Add(New MathPoint With {.X=-0.785 , .Y= 4.680})
                
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.785 , .Y= 4.680})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.470 , .Y= 3.835})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.645 , .Y= 3.530})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.645 , .Y= 1.170})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.620 , .Y= 1.170})
                
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.6200, .Y= 0.400})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.5200, .Y= 0.400})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.2500, .Y= 0.130})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 1.1750, .Y= 0.100})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.9175, .Y= 0.080})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.9175, .Y= 0.073})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.8730, .Y= 0.073})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.8730, .Y= 0.060})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.8475, .Y= 0.060})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.8475, .Y= 0.030})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.6785, .Y= 0.030})
                PolygonG2.Vertices.Add(New MathPoint With {.X= 0.6785, .Y= 0.080})
                
                Return PolygonG2
            End Function
            
            ''' <summary> Creates the definition of the base polygon for calculation of minimum clearance. </summary>
             ''' <remarks> 
             ''' This polygon is based on G2, but:
             ''' - Heights are changed corresponding to EBO appendix 2 Pt3 - so they match EBO appendix 1 Illustration 1.
             ''' - Corresponding to EBO appendix 2 Pt4 the lower part is replaced to match EBO appendix 1 Illustration 2.
             ''' See EBO, appendix 1 (http://www.gesetze-im-internet.de/ebo/anlage_1.html).
             ''' See EBO, appendix 2 (http://www.gesetze-im-internet.de/ebo/anlage_2.html).
             ''' </remarks>
            Private Shared Function CreateMinBaseLine() As Polygon
                
                Dim BasePolygon As New Polygon()
                BasePolygon.IsClosed = True
                
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.6475, .Y= 0.080})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.6475, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.7175, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.7175, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.8735, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.8735, .Y= 0.080})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.2750, .Y= 0.080})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.520 , .Y= 0.380})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.620 , .Y= 0.380})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.620 , .Y= 1.150})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.645 , .Y= 1.150})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.645 , .Y= 3.590})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.470 , .Y= 3.895})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.785 , .Y= 4.740})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.785 , .Y= 4.740})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.470 , .Y= 3.895})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.645 , .Y= 3.590})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.645 , .Y= 1.150})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.620 , .Y= 1.150})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.620 , .Y= 0.380})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.520 , .Y= 0.380})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.2750, .Y= 0.080})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.8735, .Y= 0.080})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.8735, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.7175, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.7175, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.6475, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.6475, .Y= 0.080})
                
                Return BasePolygon
            End Function
            
        #End Region
        
        #Region "Definition of Tables"
            
            Private Shared Sub CreateTables()
                
                Table212i = New LinearTabFunction()
                Table212a = New LinearTabFunction()
                
                ' Inside of curve.
                Table212i.AddBasePoint(100.0, 0.560)
                Table212i.AddBasePoint(120.0, 0.365)
                Table212i.AddBasePoint(150.0, 0.165)
                Table212i.AddBasePoint(170.0, 0.130)
                Table212i.AddBasePoint(180.0, 0.110)
                Table212i.AddBasePoint(190.0, 0.095)
                Table212i.AddBasePoint(200.0, 0.085)
                Table212i.AddBasePoint(225.0, 0.055)
                Table212i.AddBasePoint(250.0, 0.020)
                
                ' Outside of curve.
                Table212a.AddBasePoint(100.0, 0.600)
                Table212a.AddBasePoint(120.0, 0.395)
                Table212a.AddBasePoint(150.0, 0.195)
                Table212a.AddBasePoint(170.0, 0.145)
                Table212a.AddBasePoint(180.0, 0.130)
                Table212a.AddBasePoint(190.0, 0.110)
                Table212a.AddBasePoint(200.0, 0.095)
                Table212a.AddBasePoint(225.0, 0.060)
                Table212a.AddBasePoint(250.0, 0.020)
            End Sub
            
        #End Region
        
        #Region "Private Calculation Methods"
            
            ''' <summary> Calculates the minimum clearance main outline. </summary>
             ''' <remarks></remarks>
             ''' <exception cref="System.InvalidOperationException"> Cant     is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> CantBase is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> Radius   is <c>Double.NaN</c> or Zero, but Cant isn't zero. </exception>
            Private Function CalculateMainOutline() As Polygon
                
                If (Double.IsNaN(_Cant))     Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownCant)
                If (Double.IsNaN(_CantBase)) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownCantBase)
                If (Double.IsNaN(_Radius))   Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownRadius)
                If (Double.IsNaN(_Speed))    Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownSpeed)
                If (_CantBase < MinCantBase) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_InvalidCantBase)
                
                Dim MinimumOutline As Polygon = New Polygon()
                MinimumOutline.IsClosed = True
                
                For Each BasePoint As MathPoint In MinBaseLine.Vertices
                    
                    If (BasePoint.Y < 0.125) Then
                        MinimumOutline.Vertices.Add(BasePoint)
                    Else
                        Dim IsPointAboveHc      As Boolean = (BasePoint.Y > hc)
                        Dim PointHeightAboveHc  As Double  = BasePoint.Y - hc
                        
                        Dim S  As Double = Calc_S(BasePoint)
                        Dim Q  As Double = Calc_Q(BasePoint, IsPointAboveHc, PointHeightAboveHc)
                        Dim T  As Double = Calc_T(BasePoint, IsPointAboveHc, PointHeightAboveHc)
                        Dim dX As Double = Sign(BasePoint.X) * (S + Q + T)
                        
                        MinimumOutline.Vertices.Add(New MathPoint With {.X=BasePoint.X + dX, .Y=BasePoint.Y})
                    End If
                Next
                
                Return MinimumOutline
            End Function
            
            ''' <summary> Calculates the minimum clearance outline for overhead line. </summary>
             ''' <remarks></remarks>
            Private Function CalculateOHLOutline() As Polygon
                
                Dim OHLOutline As Polygon = New Polygon()
                
                Return OHLOutline
            End Function
            
            ''' <summary> Calculates the delta according to EBO, appendix2, Pt. 1.2, named "S" (Ausladung). </summary>
             ''' <param name="Point"> A point in canted rails system. </param>
             ''' <returns> The horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function Calc_S(Point As MathPoint) As Double
                
                Dim RetValue As Double = 0.0
                
                If (Not IsSmallRadius250) Then
                    ' Radius >= 250 m.
                    'RetValue = (3.750 / AbsRadius) + (TrackGauge - 1.435) / 2
                    RetValue = (3.750 / AbsRadius) + S0
                Else
                    ' Radius < 250 m.
                    Dim IsPointInsideCurve  As Boolean = (Sign(Point.X) = Sign(_Radius))
                    
                    If (IsPointInsideCurve) Then
                        ' Inside of curve.
                        RetValue = Table212i.EvaluateFx(AbsRadius)
                    Else
                        ' Outside of curve.
                        RetValue = Table212a.EvaluateFx(AbsRadius)
                    End If
                    
                    If (Point.Y <= 0.400) Then
                        RetValue -= 0.005
                    End If
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Calculates the delta according to EBO, appendix2, Pt. 1.3, named "Q" (Quasistatische Seitenneigung). </summary>
             ''' <param name="Point">              A point in canted rails system. </param>
             ''' <param name="IsPointAboveHc">     Determines whether or not the point is above hc (for performance). </param>
             ''' <param name="PointHeightAboveHc"> The height of the point above hc (negative means under hc) (for performance). </param>
             ''' <returns> The horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function Calc_Q(Point As MathPoint, IsPointAboveHc As Boolean, PointHeightAboveHc As Double) As Double
                
                Return If(IsPointAboveHc, Q0 * PointHeightAboveHc, 0.0)
            End Function
            
            ''' <summary> Calculates the delta according to EBO, appendix2, Pt. 1.4, named "T" (Zufallsbedingte Verschiebung). </summary>
             ''' <param name="Point">              A point in canted rails system. </param>
             ''' <param name="IsPointAboveHc">     Determines whether or not the point is above hc (for performance). </param>
             ''' <param name="PointHeightAboveHc"> The height of the point above hc (negative means under hc) (for performance). </param>
             ''' <returns> The horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function Calc_T(Point As MathPoint, IsPointAboveHc As Boolean, PointHeightAboveHc As Double) As Double
                
                Dim RetValue As Double
                Dim T2g      As Double = Point.Y / 100
                
                If (IsPointAboveHc) Then
                    Dim IsPointInsideCant As Boolean = (Sign(Point.X) = Sign(_Cant))
                    
                    Dim T2d As Double = PointHeightAboveHc * 0.004
                    Dim T3  As Double = PointHeightAboveHc * If(IsPointInsideCant, T3i0, T3a0)
                    Dim T4  As Double = PointHeightAboveHc * T40
                    Dim T5  As Double = PointHeightAboveHc * T50
                    
                    RetValue = 1.2 * Sqrt( Pow(T2g + T2d, 2) + (T3 * T3) + (T4 * T4) + (T5 * T5) )
                Else
                    RetValue = 1.2 * T2g
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Private Helper Methods"
            
            ''' <summary> Calculates a normalized cant which match a cant base of 1.500 m. </summary>
             ''' <remarks> This is for supporting other cant bases than 1.500 m. </remarks>
            Private Function NormalizeCant() As Double
                Dim RetValue As Double = Double.NaN
                If ( (Not Double.IsNaN(_Cant)) AndAlso (Not Double.IsNaN(_CantBase)) AndAlso (Not (_CantBase < MinCantBase)) ) Then
                    RetValue = _Cant * 1.5 / _CantBase
                End If
                Return RetValue
            End Function
            
            Private Sub SetAuxValues()
                
                ' Invalidate actual clearance outlines.
                IsMainOutlineValid = False
                IsOHLOutlineValid  = False
                
                ' Default values.
                NormalizedCant = Double.NaN
                UnsignedCant   = Double.NaN
                RelativeCant   = Double.NaN
                
                ' Consider cant base.
                NormalizedCant = NormalizeCant()
                
                ' Cant: Unsigned + relative.
                If (Not Double.IsNaN(NormalizedCant)) Then

                    UnsignedCant = Abs(NormalizedCant)
                    
                    If (Not Double.IsNaN(_Radius)) Then
                        RelativeCant = Sign(_Radius) * _Cant
                    End If
                End If
                
                ' Cant deficiency.
                u0 = 11.8 * _Speed * _Speed / AbsRadius / 1000  ' Always positive (relative cant) in [m].
                uf = u0 - RelativeCant                          ' In [m].
                
                ' Point independent part of Q.
                Dim ufu50 As Double = Max(uf, UnsignedCant) - 0.050
                Q0 = If(ufu50 > 0.0, 0.267 * ufu50, 0)
                
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ' ''' <inheritdoc/>
            ' Public Overrides Function ToString() As String
            '     Dim PositionString As New Collection(Of String)
            '     If (Me.TrackNo.HasValue)                     Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_Track     & ": " & Me.TrackNo)
            '     If (Me.Kilometer.HasValue())                 Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_Kilometer & ": " & Me.Kilometer.ToString())
            '     If (Me.RailsNameNo.IsNotEmptyOrWhiteSpace()) Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_RailsName & ": " & Me.RailsNameNo)
            '     If (Me.RailsCode.IsNotEmptyOrWhiteSpace())   Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_RailsCode & "="  & Me.RailsCode)
            '     If (Me.Side.IsNotEmptyOrWhiteSpace())        Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_Side      & ": " & Me.Side)
            '     If (PositionString.Count = 0)                Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_UnKnownPosition)
            '     Return PositionString.JoinIgnoreEmpty(", ")
            ' End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
