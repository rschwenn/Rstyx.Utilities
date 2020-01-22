
Imports System
Imports System.Math
Imports System.Windows

Imports Rstyx.Utilities.Math

Namespace Domain
    
    ''' <summary> A pair of rails at a discrete spot (cross section). </summary>
     ''' <remarks>
     ''' Any change to this RailPair is signaled by firing the <see cref="RailPair.RailsConfigChanged"/> event.
     ''' To be valid, the <see cref="RailPair.reconfigure"/> method has to be applied successfuly.
     ''' </remarks>
    Public Class RailPair
        Inherits Cinch.ValidatingObject
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.RailPair")
            
            Private Shared UnknownConfigurationRule As Cinch.SimpleRule
            Private Shared NonPositiveSpeed         As Cinch.SimpleRule
            
            Private IsCantDeficiencyValid   As Boolean = False
            Private AbsRadius               As Double  = Double.NaN
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Static initializations. </summary>
            Shared Sub New()
                UnknownConfigurationRule = New Cinch.SimpleRule("IsConfigured",
                                                                Rstyx.Utilities.Resources.Messages.RailPair_UnknownConfiguration,
                                                                Function (oValidatingObject As Object) (Not DirectCast(oValidatingObject, RailPair).IsConfigured))
                
                NonPositiveSpeed         = New Cinch.SimpleRule("Speed",
                                                                Rstyx.Utilities.Resources.Messages.RailPair_InvalidConfiguration_NonPositiveSpeed,
                                                                Function (oValidatingObject As Object) 
                                                                    Dim Speed As Double = DirectCast(oValidatingObject, RailPair).Speed
                                                                    Return ((Not Double.IsNaN(Speed)) AndAlso (Speed <= 0.0))     
                                                                End Function
                                                               )
            End Sub
            
            ''' <summary> Creates a new RailPair with unknown configuration. </summary>
            Public Sub New()
                Me.Reset()
                Me.AddRule(UnknownConfigurationRule)
                Me.AddRule(NonPositiveSpeed)
            End Sub
            
        #End Region
        
        #Region "Public Shared Fields"
            
            ''' <summary> Determines the minimum value that will be accepted for cant base. </summary>
            Public Shared MinimumCantBase       As Double = 0.1
            
            ''' <summary> A given cant lower than this value, will be snapped to Zero cant. </summary>
            Public Shared CantZeroSnap          As Double = 0.001
            
            ''' <summary> A given radius lower than this value will be snapped to infinity radius resp. tangent (with sign!). </summary>
             ''' <remarks> Default is 100. This way an input radius lower than 100 will be rejected. </remarks>
            Public Shared RadiusInfinitySnap    As Double = 100.0
            
        #End Region
        
        #Region "Properties"
            
            Private _Speed          As Double = Double.NaN
            Private _Radius         As Double = Double.NaN
            Private _VerticalRadius As Double = Double.NaN
            Private _CantDeficiency As Double = Double.NaN
            Private _Cant           As Double = Double.NaN
            Private _CantBase       As Double = Double.NaN
            Private _RSLeft         As Point
            Private _RSRight        As Point
            Private _IsConfigured   As Boolean
            
            ''' <summary> Gets or sets the speed. </summary>
            Public Property Speed() As Double
                Get
                    Return _Speed
                End Get
                Set(value As Double)
                    _Speed = value
                    IsCantDeficiencyValid = False
                    RaiseRailsConfigChanged()
                End Set
            End Property
            
            ''' <summary> Gets or sets the radius of gradient design element (vertical curve set). </summary>
             ''' <remarks>
             ''' Sign:  positive = increasing gradient,  negative = decreasing gradient  (+ = valley, - = hill).  
             ''' <see cref="RadiusInfinitySnap"/> will be applied.
             ''' </remarks>
            Public Property VerticalRadius() As Double
                Get
                    Return _VerticalRadius
                End Get
                Set(value As Double)
                    If (Double.IsNaN(value)) Then
                        _VerticalRadius = Double.NaN
                        
                    ElseIf (Abs(value) < RadiusInfinitySnap) Then
                        _VerticalRadius = If(value < 0, Double.NegativeInfinity, Double.PositiveInfinity)
                    Else
                        _VerticalRadius = value
                    End If
                    RaiseRailsConfigChanged()
                End Set
            End Property
            
            ''' <summary> Gets or sets the radius. </summary>
            ''' <remarks> <see cref="RadiusInfinitySnap"/> will be applied. </remarks>
            Public Property Radius()  As Double
                Get
                    Return _Radius
                End Get
                Set(value As Double)
                    If (Double.IsNaN(value)) Then
                        _Radius   = Double.NaN
                        AbsRadius = Double.NaN
                        
                    ElseIf (Abs(value) < RadiusInfinitySnap) Then
                        _Radius   = If(value < 0, Double.NegativeInfinity, Double.PositiveInfinity)
                        AbsRadius = Double.PositiveInfinity
                    Else
                        _Radius   = value
                        AbsRadius = Abs(value)
                    End If
                    IsCantDeficiencyValid = False
                    RaiseRailsConfigChanged()
                End Set
            End Property
            
            ''' <summary> Gets the cant deficiency for this rails pair in [m]. Maybe <c>Double.NaN</c> </summary>
             ''' <remarks>  </remarks>
            Public ReadOnly Property CantDeficiency() As Double
                Get
                    If (Not IsCantDeficiencyValid) Then
                        _CantDeficiency = CalculateCantDeficiency()
                        IsCantDeficiencyValid = True
                    End If
                    Return _CantDeficiency
                End Get
            End Property
            
            ''' <summary> Gets the Cant (negative, if right running surface is higher). </summary>
             ''' <remarks> This value can be set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property Cant() As Double
                Get
                    Return _Cant
                End Get
            End Property
            
            ''' <summary> Gets the CantBase. </summary>
             ''' <remarks> This value can be set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property CantBase() As Double
                Get
                    Return _CantBase
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Left Running Surface in track system. </summary>
             ''' <remarks> This value can be set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property RSLeft() As Point
                Get
                    Return _RSLeft
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Higher Running Surface in track system. </summary>
             ''' <remarks> This value can be set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property RSHigher() As Point
                Get
                    Dim RetRS As Point = If((_Cant < 0), RSRight, RSLeft)
                    Return RetRS
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Lower Running Surface in track system. </summary>
             ''' <remarks> This value can be set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property RSLower() As Point
                Get
                    Dim RetRS As Point = If((_Cant < 0), RSLeft, RSRight)
                    Return RetRS
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Right Running Surface in track system. </summary>
             ''' <remarks> This value can be set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property RSRight() As Point
                Get
                    Return _RSRight
                End Get
            End Property
            
            ''' <summary> If <see langword="false"/>, the configuration of this RailsPair is unknown. Otherwise all properties are valid. </summary>
            Public ReadOnly Property IsConfigured() As Boolean
                Get
                    Return _IsConfigured
                End Get
            End Property
            
        #End Region
        
        #Region "Events"
            
            Private ReadOnly RailsConfigChangedWeakEvent As New Cinch.WeakEvent(Of EventHandler(Of EventArgs))
            
            ''' <summary> Raises when any aspect of this RailPair has changed. (Internaly managed in a weakly way). </summary>
            Public Custom Event RailsConfigChanged As EventHandler(Of EventArgs)
                
                AddHandler(ByVal value As EventHandler(Of EventArgs))
                    RailsConfigChangedWeakEvent.Add(value)
                End AddHandler
                
                RemoveHandler(ByVal value As EventHandler(Of EventArgs))
                    RailsConfigChangedWeakEvent.Remove(value)
                End RemoveHandler
                
                RaiseEvent(ByVal sender As Object, ByVal e As EventArgs)
                    Try
                        RailsConfigChangedWeakEvent.Raise(sender, e)
                    Catch ex As System.Exception
                        Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
                    End Try
                End RaiseEvent
                
            End Event
            
            ''' <summary> Raises the RailsConfigChanged event. </summary>
            Private Sub RaiseRailsConfigChanged()
                RaiseEvent RailsConfigChanged(Me, EventArgs.Empty)
            End Sub
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Re-configures this RailPair based on running surfaces. </summary>
             ''' <param name="RunningSurface1"> Coordinates of one RunningSurface in track system. </param>
             ''' <param name="RunningSurface2"> Coordinates of the other RunningSurface in track system. </param>
             ''' <remarks> Right and left running surface are determined automatically. Cant and cantbase will be calculated. Radius will be set to +/- 1, if it's still <c>Double.NaN</c>. </remarks>
             ''' <exception cref="System.ArgumentException"> At least one coordinate of the points is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> RunningSurface1 and RunningSurface2 are equal. </exception>
            Public Sub Reconfigure(RunningSurface1 As Point, RunningSurface2 As Point)
                
                If (Double.IsNaN(RunningSurface1.X) OrElse Double.IsNaN(RunningSurface1.Y)) Then Throw New System.ArgumentException("RunningSurface1: at least one coordinate is NaN")
                If (Double.IsNaN(RunningSurface2.X) OrElse Double.IsNaN(RunningSurface2.Y)) Then Throw New System.ArgumentException("RunningSurface2: at least one coordinate is NaN")
                If (RunningSurface1.Equals(RunningSurface2)) Then Throw New System.ArgumentException("RunningSurface1 and RunningSurface2 are equal")
                
                If ((RunningSurface2.X - RunningSurface1.X) > 0) Then
                    _RSRight = RunningSurface2
                    _RSLeft  = RunningSurface1
                Else
                    _RSRight = RunningSurface1
                    _RSLeft  = RunningSurface2
                End If
                
                Dim V As Vector = _RSLeft - _RSRight
                _Cant     = V.Y
                _CantBase = V.Length
                
                ' Check close to zero.
                If (Abs(_Cant) < CantZeroSnap) Then _Cant = 0.0
                
                If (Double.IsNaN(_Radius)) Then Me.Radius = 1 * Sign(_Cant)
                
                IsCantDeficiencyValid = False
                _IsConfigured  = True
                RaiseRailsConfigChanged()
            End Sub
            
            ''' <summary> Re-configures this RailPair based on cant and cantbase. </summary>
             ''' <param name="Cant"> Cant in [m] (negative, if right running surface is higher). </param>
             ''' <param name="CantBase"> CantBase in [m]. </param>
             ''' <exception cref="System.ArgumentException"> Cant is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> CantBase is <c>Double.NaN</c> or less than 0.001. </exception>
             ''' <remarks> Right and left running surface are determined automatically. Cant and cantbase will be calculated. Radius will be set to +/- 1, if it's still <c>Double.NaN</c>. </remarks>
            Public Sub Reconfigure(Cant As Double, CantBase As Double)
                
                If (Double.IsNaN(Cant))         Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownCant, "Cant")
                If (Double.IsNaN(CantBase))     Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownCantBase, "CantBase")
                If (CantBase < MinimumCantBase) Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_InvalidCantBaseNegative, "CantBase")
                
                ' Check close to zero.
                If (Abs(Cant) < CantZeroSnap) Then
                    _Cant = 0.0
                Else
                    _Cant = Cant
                End If
                
                Dim cbh As Double = Sqrt(Pow(CantBase, 2) - Pow(_Cant, 2))
                Dim xl  As Double = -cbh / 2
                Dim xr  As Double = +cbh / 2
                Dim yl  As Double
                Dim yr  As Double
                
                If (_Cant < 0) Then
                    Yl = 0.0
                    yr = Abs(_Cant)
                Else
                    Yl = _Cant
                    yr = 0.0
                End If
                
                _CantBase = CantBase
                _RSLeft   = New Point(xl, yl)
                _RSRight  = New Point(xr, yr)
                If (Double.IsNaN(_Radius)) Then Me.Radius = 1 * Sign(_Cant)
                
                IsCantDeficiencyValid = False
                _IsConfigured  = True
                RaiseRailsConfigChanged()
            End Sub
            
            ''' <summary> Re-configures this RailPair based on a <see cref="GeoTcPoint"/>. </summary>
             ''' <param name="PointGeometry"> The point which provides cant, cant base and radius. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="PointGeometry"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentException"> Cant     (<paramref name="PointGeometry"/><c>.Ueb</c>) is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> CantBase (<paramref name="PointGeometry"/><c>.CantBase</c>) is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> Radius   (<paramref name="PointGeometry"/><c>.Ra</c>) is <c>Double.NaN</c>, but Cant isn't Zero. </exception>
            Public Sub Reconfigure(PointGeometry As GeoTcPoint)
                
                If (PointGeometry Is Nothing)              Then Throw New System.ArgumentNullException("PointGeometry")
                If (Double.IsNaN(PointGeometry.Ueb))       Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownCant)
                If (Double.IsNaN(PointGeometry.CantBase))  Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownCantBase)
                
                Me.Radius         = PointGeometry.Ra
                Me.VerticalRadius = PointGeometry.RaLGS
                
                If (Not PointGeometry.Ueb.EqualsTolerance(0.0, RailPair.CantZeroSnap)) Then
                    If (Double.IsNaN(Me.Radius)) Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_Reconfigure_UnknownRadius)
                End If
                
                Me.Reconfigure(PointGeometry.Ueb * Sign(Me.Radius), PointGeometry.CantBase)
            End Sub
            
            ''' <summary> Tells if a given point is inside of canted train. </summary>
             ''' <param name="Point"> The point, given in canted rails system. </param>
             ''' <returns> <see langword="false"/> if <paramref name="Point"/> is outside cant or cant is Zero, otherwise <see langword="true"/>. </returns>
             ''' <remarks> "Inside cant" means: sign of point's X equals sign of cant. </remarks>
             ''' <exception cref="System.ArgumentException"> The X-coordinate of <paramref name="Point"/> is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> <c>Cant</c> is <c>Double.NaN</c>. </exception>
            Public Function IsPointInsideCant(Point As MathPoint) As Boolean
                
                If (Double.IsNaN(Point.X))  Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownPointDistance, "Point.X")
                If (Double.IsNaN(_Cant))    Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownCant)
                
                Dim RetValue As Boolean
                
                If (Not _Cant.EqualsTolerance(0.0, RailPair.CantZeroSnap)) Then
                    RetValue = (Sign(Point.X) = Sign(_Cant))
                Else
                    RetValue = IsPointInsideCurve(Point)
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Tells if a given point is inside the curve. </summary>
             ''' <param name="Point"> The point, given in canted rails system. </param>
             ''' <returns> <see langword="false"/> if <paramref name="Point"/> is outside curve or radius is infinite (tangent), otherwise <see langword="true"/>. </returns>
             ''' <exception cref="System.ArgumentException"> The X-coordinate of <paramref name="Point"/> is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.InvalidOperationException"> <c>Radius</c> is <c>Double.NaN</c>. </exception>
            Public Function IsPointInsideCurve(Point As MathPoint) As Boolean
                
                If (Double.IsNaN(Point.X))  Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownPointDistance, "Point.X")
                If (Double.IsNaN(_Radius))  Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownRadius)
                
                Dim RetValue As Boolean = False
                
                If (Not Double.IsInfinity(_Radius)) Then
                    RetValue = (Sign(Point.X) = Sign(_Radius))
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Ensures that cant deficiency doesn't exceeds <paramref name="MaxCantDeficiency"/> by reducing speed of this rail pair. </summary>
             ''' <param name="MaxCantDeficiency"> Maximum allowed cant deficiency in [m]. </param>
             ''' <remarks>
             ''' The speed of this railpair will be reduced in steps of 5 km/h until cant deficiency doesn't exceeds <paramref name="MaxCantDeficiency"/>.
             ''' This method has no effect if cant deficiency can't be calculated.
             ''' </remarks>
            Public Sub LimitCantDeficiency(MaxCantDeficiency As Double)
                
                Dim uf As Double = Me.CantDeficiency
                
                If (Not Double.IsNaN(uf)) Then
                    
                    Dim oldSpeed As Double = Me.Speed
                    
                    Do While  (uf > MaxCantDeficiency)
                        _Speed -= 5
                        uf = CalculateCantDeficiency()
                    Loop
                    
                    ' Propagate new speed.
                    If (_Speed < oldSpeed) Then
                        Me.Speed = _Speed
                    End If
                End If
            End Sub
            
            ''' <summary> Resets the configuration of this RailPair to "unknown". </summary>
            Public Sub Reset()
                Me.Speed          = Double.NaN
                Me.VerticalRadius = Double.NaN
                Me.Radius         = Double.NaN
                _CantDeficiency   = Double.NaN
                _Cant             = Double.NaN
                _CantBase         = Double.NaN
                _RSLeft           = New Point(Double.NaN, Double.NaN)
                _RSRight          = New Point(Double.NaN, Double.NaN)
                _IsConfigured     = False
                IsCantDeficiencyValid = False
                RaiseRailsConfigChanged()
            End Sub
            
            ''' <summary> Creates a <see cref="GeoTcPoint"/> whith cant, cant base, vertical radius and radius of this rail pair. </summary>
             ''' <returns> The "GeometryPoint". </returns>
            Public Function ToGeometryPoint() As GeoTcPoint
                
                Dim GeometryPoint As New GeoTcPoint()
                
                ' Radius specials.
                If ( (Not (Double.IsNaN(Me.Cant) OrElse Me.Cant.EqualsTolerance(0, RailPair.CantZeroSnap))) AndAlso Double.IsNaN(Me.Radius)) Then
                    ' Ensure that sign of cant is determinable by setting a special radius.
                    GeometryPoint.Ra = If(Me.Cant < 0, Double.NegativeInfinity, Double.PositiveInfinity)
                Else
                    GeometryPoint.Ra = Me.Radius
                End If
                
                GeometryPoint.ID       = "GeometryPoint"
                GeometryPoint.Ueb      = Me.Cant * Sign(Me.Radius)
                GeometryPoint.CantBase = Me.CantBase
                GeometryPoint.RaLGS    = Me.VerticalRadius
                
                Return GeometryPoint
            End Function
            
        #End Region
        
        #Region "Overrides"
            
           ''' <inheritdoc/>
            Public Overrides Function ToString() As String
                Dim RetValue As String
                If (Not Me.IsConfigured) Then 
                    RetValue = MyBase.ToString()
                Else
                    RetValue = Rstyx.Utilities.StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.RailPair_ToString, Me.Cant * 1000, Me.CantBase)
                End If
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Private Methods"
            
            ''' <summary> Calculates cant Deficiency in [m]. Maybe <c>Double.NaN</c>. </summary>
             ''' <returns> Cant Deficiency in [m]. Maybe <c>Double.NaN</c> </returns>
            Private Function CalculateCantDeficiency() As Double
            
                Dim uf  As Double = Double.NaN
                
                If (Not (Double.IsNaN(_Cant) OrElse Double.IsNaN(_Radius) OrElse Double.IsNaN(_Speed) )) Then
                    
                    If (Double.IsInfinity(_Radius)) Then
                        If (_Cant.EqualsTolerance(0.0, RailPair.CantZeroSnap)) Then uf = 0.0
                    Else
                        uf = (11.8 * _Speed * _Speed / AbsRadius / 1000)  -  (Sign(_Radius) * _Cant)   ' In [m].
                    End If
                End If
                
                Return uf
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
