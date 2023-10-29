
Imports System
Imports System.Math
Imports System.Collections.Generic

Imports Rstyx.Utilities.Math


Namespace Domain.ClearanceGauge
    
    ''' <summary> Provides minimum clearance outline for DBAG railway track ("Grenzlinie"). </summary>
     ''' <remarks> The outlines are provided in canted rails system. </remarks>
    Public Class MinimumClearanceDBAG
        
        #Region "Private Fields"
            
            'Private Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Domain.ClearanceGauge.MinimumClearanceDBAG")
            
            Const S0    As Double  = 0.005   ' Influence of track gauge widening / deviation. Corresponds to EBO appendix 2 table 2.1.1 (gauge = 1.445).
            Const hc    As Double  = 0.500   ' Wankpolhöhe.
            Const bw2   As Double  = 0.975   ' Half width of pantograph.

            Const HeightReserveTop  As Double = 0.050   ' Heihtening reserve at top.
            Const HeightReserveDown As Double = 0.010   ' Rails wearing and setting.
            
            Private Shared ReadOnly OHLCharacteristics  As Dictionary(Of ClearanceOptionalPart , OHLKeyData)
            Private Shared ReadOnly MinBaseLine         As Polygon
            
            Private Shared Table212i            As LinearTabFunction
            Private Shared Table212a            As LinearTabFunction
            Private Shared Table23aF0           As LinearTabFunction
            Private Shared Table23aF1           As LinearTabFunction
            Private Shared Table23aF2           As LinearTabFunction
            Private Shared Table23bF0           As LinearTabFunction
            Private Shared Table23bF1           As LinearTabFunction
            Private Shared Table23bF2           As LinearTabFunction
            Private Shared OHLTable22           As LinearTabFunction
            Private Shared Table23a             As Dictionary(Of RailFixing , LinearTabFunction)
            Private Shared Table23b             As Dictionary(Of RailFixing , LinearTabFunction)
            
            Private IsMainOutlineValid          As Boolean = False
            Private IsOHLOutlineValid           As Boolean = False
            Private IsSmallRadius250            As Boolean = False
            
            'Private NormalizedCant              As Double = Double.NaN
            Private RelativeCant                As Double = Double.NaN
            Private AbsRadius                   As Double = Double.NaN
            Private AbsVRadius                  As Double = Double.NaN
            Private oQi0                        As Double = Double.NaN
            Private oQa0                        As Double = Double.NaN
            Private Qi0                         As Double = Double.NaN
            Private Qa0                         As Double = Double.NaN
            Private dH_VRadius                  As Double = Double.NaN
            
            Private OHLKeys                     As OHLKeyData
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Shared initialization of MinimumClearanceDBAG. </summary>
            Shared Sub New()
                _ReferenceLineG2    = CreateG2()
                MinBaseLine         = CreateMinBaseLine()
                OHLCharacteristics  = CreateOHLCharacteristics()
                
                CreateTables()
            End Sub
            
            ''' <summary> Creates a new, empty MinimumClearanceDBAG. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new MinimumClearanceDBAG and sets geometry by referencing an existing <see cref="RailPair"/>. </summary>
             ''' <param name="RailsConfig"> The <see cref="RailPair"/> which provides cant, cant base, radius and speed. </param>
             ''' <remarks> If <paramref name="RailsConfig"/> is <see langword="null"/>, a default <see cref="RailPair"/> will be used. </remarks>
            Public Sub New(RailsConfig As RailPair)
                Me.RailsConfig = RailsConfig
            End Sub
            
        #End Region
        
        #Region "Result Properties"
            
            Private Shared ReadOnly _ReferenceLineG2 As Polygon
            Private _MainOutline    As Polygon
            Private _OHLOutline     As Polygon
            
            ''' <summary> Gets the static definition of G2. </summary>
             ''' <remarks> The lower part is for all rails that may contain changes in vertical curve. </remarks>
            Shared ReadOnly Property ReferenceLineG2() As Polygon
                Get
                    Return _ReferenceLineG2
                End Get
            End Property
            
            ''' <summary> Returns the main outline of minimum clearance. </summary>
             ''' <remarks> The outline will be calculated if it isn't yet. </remarks>
            Public ReadOnly Property MainOutline() As Polygon
                Get
                    If (Not IsMainOutlineValid) Then
                        _MainOutline = CalculateMainOutline()
                        IsMainOutlineValid = True
                    End If
                    Return _MainOutline
                End Get
            End Property
            
            ''' <summary> Returns the outline for overhead line area of minimum clearance. </summary>
             ''' <remarks> The outline will be calculated if it isn't yet. It may be an empty polygon. </remarks>
            Public ReadOnly Property OHLOutline() As Polygon
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
            
            Private _RailsConfig    As RailPair
            Private _OHLType        As ClearanceOptionalPart = ClearanceOptionalPart.None
        
            ''' <summary> Gets or sets the rails configuration (geometry and speed). </summary>
             ''' <remarks> The getter never returns <see langword="null"/>. </remarks>
            Public Property RailsConfig() As RailPair
                Get
                    If (_RailsConfig Is Nothing) Then
                        _RailsConfig = New RailPair()
                        AddHandler _RailsConfig.RailsConfigChanged, AddressOf RailsConfigChanged
                    End If
                    Return _RailsConfig
                End Get
                Set(value As RailPair)
                    If (_RailsConfig IsNot Nothing) Then
                        RemoveHandler _RailsConfig.RailsConfigChanged, AddressOf RailsConfigChanged
                    End If
                    _RailsConfig = value
                    If (_RailsConfig IsNot Nothing) Then
                        AddHandler _RailsConfig.RailsConfigChanged, AddressOf RailsConfigChanged
                    End If
                    SetAuxValues()
                End Set
            End Property
            
            ''' <summary> Gets or sets the type of OverHeadLine. </summary>
             ''' <remarks> Setting this to an unsupported value will result in setting it to <see cref="ClearanceOptionalPart.None"/> </remarks>
            Public Property OHLType() As ClearanceOptionalPart
                Get 
                    Return _OHLType
                End Get
                Set(value As ClearanceOptionalPart)
                    If (Not (value = _OHLType)) Then
                        _OHLType = value
                        
                        ' Validate for supported values.
                        If (OHLCharacteristics.ContainsKey(_OHLType)) Then
                            
                            OHLKeys = OHLCharacteristics(_OHLType)
                        Else
                            OHLKeys = Nothing 
                            
                            If (Not (value = ClearanceOptionalPart.None)) Then
                                _OHLType = ClearanceOptionalPart.None
                            End If
                        End If
                    End If
                    SetAuxValues()
                End Set
            End Property
            
        #End Region
        
        #Region "Nested Types"
            
            ''' <summary> Internal used data record for configuring OHL minimum clearance. </summary>
             ''' <remarks> See http://www.gesetze-im-internet.de/ebo/anlage_1.html </remarks>
            Private Structure OHLKeyData
                
                ''' <summary> Minimal distance to over head line. </summary>
                Public MinimumDistance  As Double
                
                ''' <summary> Minimal height of OHL minimum clearance. </summary>
                Public MinimumHeight    As Double
                
                ''' <summary> Horizontal bevel. </summary>
                Public HorizontalBevel  As Double
                
                ''' <summary> Vertical bevel. </summary>
                Public VerticalBevel    As Double
                
            End Structure
            
        #End Region
        
        #Region "Definition of Polygons"
            
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
             ''' - Corresponding to EBO appendix 2 Pt 4 the lower part is replaced to match EBO appendix 1 Illustration 2 a)
             '''   with not changing gradient.
             ''' See EBO, appendix 1 (http://www.gesetze-im-internet.de/ebo/anlage_1.html).
             ''' See EBO, appendix 2 (http://www.gesetze-im-internet.de/ebo/anlage_2.html).
             ''' </remarks>
            Private Shared Function CreateMinBaseLine() As Polygon
                
                Dim BasePolygon   As New Polygon()
                BasePolygon.IsClosed = True
                
                ' Lower part is for non-changing gradient (Y= 0.080).
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.6475, .Y= 0.080})  ' fix distances
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.6475, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.7175, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.7175, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.8735, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.8735, .Y= 0.080})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.2120, .Y= 0.080})  ' variable distances
                
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.250 , .Y= 0.130})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.520 , .Y= 0.400})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.620 , .Y= 0.400})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.620 , .Y= 1.170})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.645 , .Y= 1.170})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.645 , .Y= 3.530})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-1.470 , .Y= 3.835})
                BasePolygon.Vertices.Add(New MathPoint With {.X=-0.785 , .Y= 4.680})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.785 , .Y= 4.680})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.470 , .Y= 3.835})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.645 , .Y= 3.530})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.645 , .Y= 1.170})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.620 , .Y= 1.170})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.620 , .Y= 0.400})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.520 , .Y= 0.400})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.250 , .Y= 0.130})
                
                BasePolygon.Vertices.Add(New MathPoint With {.X= 1.2120, .Y= 0.080})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.8735, .Y= 0.080})  ' fix distances
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.8735, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.7175, .Y= 0.000})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.7175, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.6475, .Y=-0.038})
                BasePolygon.Vertices.Add(New MathPoint With {.X= 0.6475, .Y= 0.080})
                
                Return BasePolygon
            End Function
            
            ''' <summary> Creates characteristics table of OHL minimum clearance. </summary>
             ''' <remarks> 
             ''' See EBO, appendix 1, table 1 (http://www.gesetze-im-internet.de/ebo/anlage_1.html).
             ''' See EBO, appendix 3, Pt 1.7  (http://www.gesetze-im-internet.de/ebo/anlage_3.html).
             ''' </remarks>
            Private Shared Function CreateOHLCharacteristics()
                
                Dim OHLKeyDataDict As Dictionary(Of ClearanceOptionalPart , OHLKeyData) = New Dictionary(Of ClearanceOptionalPart , OHLKeyData)
                
                OHLKeyDataDict.Add(ClearanceOptionalPart.OHL_1kV , New OHLKeyData With {.MinimumDistance = 0.035, .MinimumHeight = 5.00, .VerticalBevel = 0.250, .HorizontalBevel = 0.350})
                OHLKeyDataDict.Add(ClearanceOptionalPart.OHL_3kV , New OHLKeyData With {.MinimumDistance = 0.050, .MinimumHeight = 5.03, .VerticalBevel = 0.250, .HorizontalBevel = 0.350})
                OHLKeyDataDict.Add(ClearanceOptionalPart.OHL_15kV, New OHLKeyData With {.MinimumDistance = 0.150, .MinimumHeight = 5.20, .VerticalBevel = 0.300, .HorizontalBevel = 0.400})
                OHLKeyDataDict.Add(ClearanceOptionalPart.OHL_25kV, New OHLKeyData With {.MinimumDistance = 0.220, .MinimumHeight = 5.34, .VerticalBevel = 0.335, .HorizontalBevel = 0.447})
                
                Return OHLKeyDataDict
            End Function

        #End Region
        
        #Region "Definition of interpolation tables"
            
            Private Shared Sub CreateTables()
                
                Table212i  = New LinearTabFunction()
                Table212a  = New LinearTabFunction()
                Table23aF0 = New LinearTabFunction()
                Table23aF1 = New LinearTabFunction()
                Table23aF2 = New LinearTabFunction()
                Table23bF0 = New LinearTabFunction()
                Table23bF1 = New LinearTabFunction()
                Table23bF2 = New LinearTabFunction()
                OHLTable22 = New LinearTabFunction()
                
                ' Table 2.3, inside of curve, not fixed.
                Table23aF0.AddBasePoint(0.000, 0.030)
                Table23aF0.AddBasePoint(0.400, 0.030)
                Table23aF0.AddBasePoint(1.170, 0.037)
                Table23aF0.AddBasePoint(3.535, 0.084)
                Table23aF0.AddBasePoint(3.835, 0.091)
                Table23aF0.AddBasePoint(4.680, 0.110)
                Table23aF0.AddBasePoint(4.999, 0.110)
                
                ' Table 2.3, inside of curve, fixed (platform).
                Table23aF1.AddBasePoint(0.000, 0.006)
                Table23aF1.AddBasePoint(0.400, 0.006)
                Table23aF1.AddBasePoint(1.170, 0.021)
                Table23aF1.AddBasePoint(3.535, 0.078)
                Table23aF1.AddBasePoint(3.835, 0.085)
                Table23aF1.AddBasePoint(4.680, 0.106)
                Table23aF1.AddBasePoint(4.999, 0.106)
                
                ' Table 2.3, inside of curve, strong fixed.
                Table23aF2.AddBasePoint(0.000, 0.002)
                Table23aF2.AddBasePoint(0.400, 0.002)
                Table23aF2.AddBasePoint(1.170, 0.014)
                Table23aF2.AddBasePoint(3.535, 0.057)
                Table23aF2.AddBasePoint(3.835, 0.062)
                Table23aF2.AddBasePoint(4.680, 0.078)
                Table23aF2.AddBasePoint(4.999, 0.078)
                
                ' Table 2.3, outside of curve (or tangent), not fixed.
                Table23bF0.AddBasePoint(0.000, 0.031)
                Table23bF0.AddBasePoint(0.400, 0.031)
                Table23bF0.AddBasePoint(1.170, 0.040)
                Table23bF0.AddBasePoint(3.535, 0.104)
                Table23bF0.AddBasePoint(3.835, 0.114)
                Table23bF0.AddBasePoint(4.680, 0.140)
                Table23bF0.AddBasePoint(4.999, 0.140)
                
                ' Table 2.3, outside of curve (or tangent), fixed (platform).
                Table23bF1.AddBasePoint(0.000, 0.006)
                Table23bF1.AddBasePoint(0.400, 0.006)
                Table23bF1.AddBasePoint(1.170, 0.025)
                Table23bF1.AddBasePoint(3.535, 0.100)
                Table23bF1.AddBasePoint(3.835, 0.110)
                Table23bF1.AddBasePoint(4.680, 0.137)
                Table23bF1.AddBasePoint(4.999, 0.137)
                
                ' Table 2.3, outside of curve (or tangent), strong fixed.
                Table23bF2.AddBasePoint(0.000, 0.003)
                Table23bF2.AddBasePoint(0.400, 0.003)
                Table23bF2.AddBasePoint(1.170, 0.019)
                Table23bF2.AddBasePoint(3.535, 0.084)
                Table23bF2.AddBasePoint(3.835, 0.093)
                Table23bF2.AddBasePoint(4.680, 0.116)
                Table23bF2.AddBasePoint(4.999, 0.116)
                
                ' Collect tables 2.3.
                Table23a = New Dictionary(Of RailFixing , LinearTabFunction)
                Table23b = New Dictionary(Of RailFixing , LinearTabFunction)
                Table23a.Add(RailFixing.None,        Table23aF0)
                Table23a.Add(RailFixing.Fixed,       Table23aF1)
                Table23a.Add(RailFixing.StrongFixed, Table23aF2)
                Table23b.Add(RailFixing.None,        Table23bF0)
                Table23b.Add(RailFixing.Fixed,       Table23bF1)
                Table23b.Add(RailFixing.StrongFixed, Table23bF2)
                
                ' Table 2.1.2, inside of curve.
                Table212i.AddBasePoint(100.0, 0.560)
                Table212i.AddBasePoint(120.0, 0.365)
                Table212i.AddBasePoint(150.0, 0.165)
                Table212i.AddBasePoint(170.0, 0.130)
                Table212i.AddBasePoint(180.0, 0.110)
                Table212i.AddBasePoint(190.0, 0.095)
                Table212i.AddBasePoint(200.0, 0.085)
                Table212i.AddBasePoint(225.0, 0.055)
                Table212i.AddBasePoint(250.0, 0.020)
                
                ' Table 2.1.2, outside of curve.
                Table212a.AddBasePoint(100.0, 0.600)
                Table212a.AddBasePoint(120.0, 0.395)
                Table212a.AddBasePoint(150.0, 0.195)
                Table212a.AddBasePoint(170.0, 0.145)
                Table212a.AddBasePoint(180.0, 0.130)
                Table212a.AddBasePoint(190.0, 0.110)
                Table212a.AddBasePoint(200.0, 0.095)
                Table212a.AddBasePoint(225.0, 0.060)
                Table212a.AddBasePoint(250.0, 0.020)
                
                ' OHL.
                OHLTable22.AddBasePoint(100.0, 0.043)
                OHLTable22.AddBasePoint(120.0, 0.039)
                OHLTable22.AddBasePoint(150.0, 0.034)
                OHLTable22.AddBasePoint(200.0, 0.030)
                OHLTable22.AddBasePoint(250.0, 0.015)
            End Sub
            
        #End Region
        
        #Region "Private Calculation Methods"
            
            ''' <summary> Calculates the minimum clearance main outline. </summary>
             ''' <remarks></remarks>
             ''' <exception cref="System.InvalidOperationException"> Radius   is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> Speed    is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> Cant     is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> CantBase is <c>Double.NaN</c>. </exception>
            Private Function CalculateMainOutline() As Polygon
                
                If (Double.IsNaN(Me.RailsConfig.Radius))   Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownRadius)
                If (Double.IsNaN(Me.RailsConfig.Speed))    Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownSpeed)
                If (Double.IsNaN(Me.RailsConfig.Cant))     Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownCant)
                If (Double.IsNaN(Me.RailsConfig.CantBase)) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownCantBase)
                
                Dim GradientChanging As Boolean = (Double.IsNaN(Me.RailsConfig.VerticalRadius) OrElse (Not Double.IsInfinity(Me.RailsConfig.VerticalRadius)))
                Dim MinimumOutline   As Polygon = New Polygon() With {.IsClosed = True}
                Dim dH_Top           As Double  =   dH_VRadius + HeightReserveTop
                Dim dH_Bottom        As Double  = -(dH_VRadius + HeightReserveDown)

                
                For Each BasePoint As MathPoint In MinBaseLine.Vertices
                    
                    If (BasePoint.Y < 0.01) Then
                        ' Immutual points of lower part of minimum clearance.
                        MinimumOutline.Vertices.Add(BasePoint)
                    Else
                        Dim IsPointAboveHc      As Boolean = (BasePoint.Y > hc)
                        Dim PointHeightAboveHc  As Double  =  BasePoint.Y - hc
                        Dim dX                  As Double  = 0.0
                        Dim dY                  As Double

                        ' Adjust distances (not for lower inner part).
                        If ((BasePoint.Y > 0.1) OrElse (Abs(BasePoint.X) > 1.0)) Then
                            Dim S  As Double = Calc_S(BasePoint)
                            Dim Q  As Double = Calc_Q(BasePoint, IsPointAboveHc, PointHeightAboveHc)
                            Dim T  As Double = Calc_T(BasePoint)
                            dX = Sign(BasePoint.X) * (S + Q + T)
                        End If

                        ' Adjust heights.
                        If (BasePoint.Y > 2.0) Then
                            dY = dH_Top
                        ElseIf (BasePoint.Y > 0.1) Then
                            dY = dH_Bottom
                        Else
                            dY = -dH_VRadius
                        End If
                        
                        MinimumOutline.Vertices.Add(New MathPoint With {.X=BasePoint.X + dX, .Y=BasePoint.Y + dY})
                    End If
                Next
                
                Return MinimumOutline
            End Function
            
            ''' <summary> Calculates the minimum clearance outline for overhead line. </summary>
             ''' <remarks></remarks>
             ''' <exception cref="System.InvalidOperationException"> Radius   is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> Speed    is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> Cant     is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> CantBase is <c>Double.NaN</c>. </exception>
            Private Function CalculateOHLOutline() As Polygon
                
                If (Double.IsNaN(Me.RailsConfig.Radius))    Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownRadius)
                If (Double.IsNaN(Me.RailsConfig.Speed))     Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownSpeed)
                If (Double.IsNaN(Me.RailsConfig.Cant))      Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownCant)
                If (Double.IsNaN(Me.RailsConfig.CantBase))  Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_UnknownCantBase)
                
                Dim OHLOutline As Polygon = New Polygon()
                OHLOutline.IsClosed = True
                
                If (OHLCharacteristics.ContainsKey(Me.OHLType)) Then
                    
                    ' Intersection of side parts with main outline.
                    Dim LeftX  As Double = CalcOHL_dX(New MathPoint With {.X = -bw2, .Y = 4.50}) - bw2
                    Dim RightX As Double = CalcOHL_dX(New MathPoint With {.X = +bw2, .Y = 4.50}) + bw2
                    
                    Dim LeftIntersect  As MathPoint = InterpolPoint(Me.MainOutline.Vertices(13), Me.MainOutline.Vertices(14), LeftX)
                    Dim RightIntersect As MathPoint = InterpolPoint(Me.MainOutline.Vertices(15), Me.MainOutline.Vertices(16), RightX)
                    
                    ' Upper part.
                    Dim OHLBaseLine As Polygon = New Polygon()
                    OHLBaseLine.Vertices.Add(New MathPoint With {.X = -bw2                          , .Y = OHLKeys.MinimumHeight - OHLKeys.VerticalBevel})
                    OHLBaseLine.Vertices.Add(New MathPoint With {.X = -bw2 + OHLKeys.HorizontalBevel, .Y = OHLKeys.MinimumHeight})
                    OHLBaseLine.Vertices.Add(New MathPoint With {.X =  bw2 - OHLKeys.HorizontalBevel, .Y = OHLKeys.MinimumHeight})
                    OHLBaseLine.Vertices.Add(New MathPoint With {.X =  bw2                          , .Y = OHLKeys.MinimumHeight - OHLKeys.VerticalBevel})
                    
                    For Each BasePoint As MathPoint In OHLBaseLine.Vertices
                        OHLOutline.Vertices.Add(New MathPoint With {.X = BasePoint.X + CalcOHL_dX(BasePoint), .Y = BasePoint.Y})
                    Next
                    
                    ' Bottom part (along main outline).
                    OHLOutline.Vertices.Add(RightIntersect)
                    OHLOutline.Vertices.Add(Me.MainOutline.Vertices(15))
                    OHLOutline.Vertices.Add(Me.MainOutline.Vertices(14))
                    OHLOutline.Vertices.Add(LeftIntersect)
                End If
                
                Return OHLOutline
            End Function
            
            ''' <summary> Interpolates a point between two points, at a given X coordinate. </summary>
             ''' <param name="P1"> Point 1. </param>
             ''' <param name="P2"> Point 2. </param>
             ''' <param name="X">  X coordinate for Point to interpolate. </param>
             ''' <returns> The interpolated point. </returns>
            Private Function InterpolPoint(P1 As MathPoint, P2 As MathPoint, X As Double) As MathPoint
                
                Dim Ratio As Double = (X - P1.X) / (P2.X - P1.X)
                Dim Y     As Double = P1.Y + (Ratio * (P2.Y - P1.Y)) - (MathPoint.Resolution / 10)

            Return New MathPoint With {.X = X, .Y = Y}
            End Function
            
            ''' <summary> Calculates the accumulated delta according to EBO, appendix 3. </summary>
             ''' <param name="Point"> A point in canted rails system. </param>
             ''' <returns> The complete horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function CalcOHL_dX(Point As MathPoint) As Double
                
                Dim PointHeightAboveHc As Double = Point.Y - hc
                Dim PointHeightAbove5m As Double = If(Point.Y > 5.0, Point.Y - 5.0, 0.0)
                    
                Dim e  As Double = 0.11 + (0.04 * PointHeightAbove5m)
                Dim S  As Double = If(Not IsSmallRadius250, (2.5 / AbsRadius) + S0, OHLTable22.EvaluateFx(AbsRadius) )
                Dim Q  As Double = CalcOHL_Q(Point, PointHeightAboveHc)
                Dim T  As Double = 0.073 + (0.0144 * PointHeightAbove5m)   ' EBO, appendix 3, Pt. 1.6 / 2.4, named "T" (Zufallsbedingte Verschiebung)
                Dim dX As Double = Sign(Point.X) * (e + S + Q + T + OHLKeys.MinimumDistance)
                
                Return dX
            End Function
            
            ''' <summary> Calculates the delta according to EBO, appendix 3, Pt. 1.5, Table 2.3, named "Q" (Quasistatische Seitenneigung). </summary>
             ''' <param name="Point">              A point in canted rails system. </param>
             ''' <param name="PointHeightAboveHc"> The height of the point above hc (negative means under hc) (for performance). </param>
             ''' <returns> The horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function CalcOHL_Q(Point As MathPoint, PointHeightAboveHc As Double) As Double
                
                Dim RetValue As Double
                
                Dim oQ0 As Double
                If (Double.IsInfinity(Me.RailsConfig.Radius)) Then
                    oQ0 = If(Me.RailsConfig.IsPointInsideCant(Point), oQi0, oQa0)
                Else
                    oQ0 = If(Me.RailsConfig.IsPointInsideCurve(Point), oQi0, oQa0)
                End If
                RetValue = oQ0 * PointHeightAboveHc
                
                Return RetValue
            End Function
            
            ''' <summary> Calculates the delta according to EBO, appendix 2, Pt. 1.2, named "S" (Ausladung). </summary>
             ''' <param name="Point"> A point in canted rails system. </param>
             ''' <returns> The horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function Calc_S(Point As MathPoint) As Double
                
                Dim RetValue As Double
                
                If (Not IsSmallRadius250) Then
                    ' Radius >= 250 m.
                    'RetValue = (3.750 / AbsRadius) + (TrackGauge - 1.435) / 2
                    RetValue = (3.750 / AbsRadius) + S0
                Else
                    ' Radius < 250 m.
                    If (Me.RailsConfig.IsPointInsideCurve(Point)) Then
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
            
            ''' <summary> Calculates the delta according to EBO, appendix 2, Pt. 1.3, named "Q" (Quasistatische Seitenneigung). </summary>
             ''' <param name="Point">              A point in canted rails system. </param>
             ''' <param name="IsPointAboveHc">     Determines whether or not the point is above hc (for performance). </param>
             ''' <param name="PointHeightAboveHc"> The height of the point above hc (negative means under hc) (for performance). </param>
             ''' <returns> The horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function Calc_Q(Point As MathPoint, IsPointAboveHc As Boolean, PointHeightAboveHc As Double) As Double
                
                Dim RetValue As Double = 0.0
                
                If (IsPointAboveHc) Then
                    Dim Q0 As Double
                    If (Double.IsInfinity(Me.RailsConfig.Radius)) Then
                        Q0 = If(Me.RailsConfig.IsPointInsideCant(Point), Qi0, Qa0)
                    Else
                        Q0 = If(Me.RailsConfig.IsPointInsideCurve(Point), Qi0, Qa0)
                    End If
                    RetValue = Q0 * PointHeightAboveHc
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Calculates the delta according to EBO, appendix 2, Pt. 1.4, named "T" (Zufallsbedingte Verschiebung). </summary>
             ''' <param name="Point"> A point in canted rails system. </param>
             ''' <returns> The horizontal delta for <paramref name="Point"/><c>.X</c> </returns>
             ''' <remarks></remarks>
            Private Function Calc_T(Point As MathPoint) As Double
                
                Dim RetValue As Double
                
                If (Me.RailsConfig.IsPointInsideCurve(Point)) Then
                    ' Inside of curve.
                    RetValue = Table23a(RailsConfig.Fixing).EvaluateFx(Point.Y)
                Else
                    ' Tangent or outside of curve.
                    RetValue = Table23b(RailsConfig.Fixing).EvaluateFx(Point.Y)
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Event Handlers"
            
            Private Sub RailsConfigChanged(sender As Object, e As EventArgs)
                If ((_RailsConfig IsNot Nothing) AndAlso sender.Equals(_RailsConfig)) Then
                    SetAuxValues()
                End If
            End Sub 
            
        #End Region
        
        #Region "Private Helper Methods"
            
            ''' <summary> Calculates a normalized cant which matches a cant base of 1.500 m. </summary>
             ''' <remarks> This is for trying to basically support other cant bases than 1.500 m. </remarks>
            Private Function NormalizeCant() As Double
                Dim RetValue As Double = Double.NaN
                If (Me.RailsConfig.IsConfigured) Then
                    RetValue = Me.RailsConfig.Cant * 1.5 / Me.RailsConfig.CantBase
                End If
                Return RetValue
            End Function
            
            Private Sub SetAuxValues()
                
                ' Invalidate actual clearance outlines.
                IsMainOutlineValid = False
                IsOHLOutlineValid  = False
                
                ' Radius.
                If (Double.IsNaN(Me.RailsConfig.Radius)) Then
                    AbsRadius = Double.NaN
                ElseIf (Double.IsInfinity(Me.RailsConfig.Radius)) Then
                    AbsRadius = Double.PositiveInfinity
                    IsSmallRadius250 = False
                Else
                    AbsRadius = Abs(Me.RailsConfig.Radius)
                    IsSmallRadius250 = (Round(AbsRadius, 3) < 250.0)
                End If
                
                ' Vertical radius.
                If (Double.IsNaN(Me.RailsConfig.VerticalRadius)) Then
                    AbsVRadius = 2000
                ElseIf (Double.IsInfinity(Me.RailsConfig.VerticalRadius)) Then
                    AbsVRadius = Double.PositiveInfinity
                Else
                    AbsVRadius = Abs(Me.RailsConfig.VerticalRadius)
                End If

                ' Influence of gradient changing in [m].
                dH_VRadius = 50 / AbsVRadius
                
                ' Consider cant base.
                'NormalizedCant = NormalizeCant()
                
                ' Cant: Relative.
                RelativeCant = Double.NaN
                If (Not Double.IsNaN(Me.RailsConfig.Cant)) Then
                    If (Not Double.IsNaN(Me.RailsConfig.Radius)) Then
                        If (Double.IsInfinity(Me.RailsConfig.Radius)) Then
                            RelativeCant = Abs(Me.RailsConfig.Cant)
                        Else
                            RelativeCant = Sign(Me.RailsConfig.Radius) * Me.RailsConfig.Cant
                        End If
                    End If
                End If
                
                ' Main: Point independent part of Qi and Qa.
                Qi0 = 0.0
                Qa0 = 0.0
                
                ' Main: Impact of standing train.
                Dim Qu0  As Double = 0.267 * Max(Abs(Me.RailsConfig.Cant) - 0.050, 0.0)
                If (Qu0 <> 0.0) Then
                    If (RelativeCant > 0) Then
                        Qi0 = Qu0
                    Else
                        Qa0 = Qu0
                    End If
                End If
                
                ' Main: Impact of moving train.
                If (Not Double.IsNaN(Me.RailsConfig.CantDeficiency)) Then
                    Dim Quf0 As Double = 0.267 * Max(Abs(Me.RailsConfig.CantDeficiency) - 0.050, 0.0)
                    If (Quf0 <> 0.0) Then
                        If (Me.RailsConfig.CantDeficiency > 0) Then
                            Qa0 = Max(Quf0, Qa0)
                        Else
                            Qi0 = Max(Quf0, Qi0)
                        End If
                    End If
                End If
                
                ' OHL: Point independent part of Qi and Qa.
                oQi0 = 0.0
                oQa0 = 0.0
                
                ' OHL: Impact of standing train.
                Dim oQu0  As Double = 0.15 * Max(Abs(Me.RailsConfig.Cant) - 0.066, 0.0)
                If (oQu0 <> 0.0) Then
                    If (RelativeCant > 0) Then
                        oQi0 = oQu0
                    Else
                        oQa0 = oQu0
                    End If
                End If
                
                ' OHL: Impact of moving train.
                If (Not Double.IsNaN(Me.RailsConfig.CantDeficiency)) Then
                    Dim oQuf0 As Double = 0.15 * Max(Abs(Me.RailsConfig.CantDeficiency) - 0.066, 0.0)
                    If (oQuf0 <> 0.0) Then
                        If (Me.RailsConfig.CantDeficiency > 0) Then
                            oQa0 = Max(oQuf0, oQa0)
                        Else
                            oQi0 = Max(oQuf0, oQi0)
                        End If
                    End If
                End If
                
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
