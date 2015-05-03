
Imports System
Imports System.Math
Imports System.Windows

Imports Rstyx.Utilities.Math

Namespace Domain
    
    ''' <summary> A pair of rails at a discrete spot (cross section). </summary>
     ''' <remarks> To be valid, the <see cref="RailPair.reconfigure"/> method has to be applied successfuly. </remarks>
    Public Class RailPair
        Inherits Cinch.ValidatingObject
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Microstation.Addin.Lichtraum.RailPair")
            
            Private Shared UnknownConfigurationRule As Cinch.SimpleRule
            Private Shared NonPositiveSpeed         As Cinch.SimpleRule
            
            Const MinCantBase               As Double  = 0.01
            Const CantZeroTol               As Double  = 0.001
            Const RadiusInfinityTol         As Double  = 0.001
            
            Private IsCantDeficiencyValid   As Boolean = False
            Private AbsRadius               As Double = Double.NaN
            
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
                Me.reset()
                Me.AddRule(UnknownConfigurationRule)
                Me.AddRule(NonPositiveSpeed)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _Speed          As Double = Double.NaN
            Private _Radius         As Double = Double.NaN
            Private _CantDeficiency As Double = Double.NaN
            Private _Cant           As Double = Double.NaN
            Private _CantBase       As Double = Double.NaN
            Private _RSLeft         As Point
            Private _RSRight        As Point
            Private _IsConfigured   As Boolean
            
            ''' <summary> Gets or sets the speed. </summary>
            Public Property Speed()  As Double
                Get
                    Return _Speed
                End Get
                Set(value As Double)
                    _Speed = value
                    IsCantDeficiencyValid = False
                End Set
            End Property
            
            ''' <summary> Gets or sets the radius. </summary>
            Public Property Radius()  As Double
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
                    Else
                        _Radius   = value
                        AbsRadius = Abs(value)
                    End If
                    
                    IsCantDeficiencyValid = False
                End Set
            End Property
            
            ''' <summary> Returns the main outline of minimum clearance. </summary>
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
             ''' <remarks> This value can e set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property Cant() As Double
                Get
                    Return _Cant
                End Get
            End Property
            
            ''' <summary> Gets the CantBase. </summary>
             ''' <remarks> This value can e set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property CantBase() As Double
                Get
                    Return _CantBase
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Left Running Surface in track system. </summary>
             ''' <remarks> This value can e set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property RSLeft() As Point
                Get
                    Return _RSLeft
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Higher Running Surface in track system. </summary>
             ''' <remarks> This value can e set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property RSHigher() As Point
                Get
                    Dim RetRS As Point = If((_Cant < 0), RSRight, RSLeft)
                    Return RetRS
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Lower Running Surface in track system. </summary>
             ''' <remarks> This value can e set via <see cref="RailPair.reconfigure"/>. </remarks>
            Public ReadOnly Property RSLower() As Point
                Get
                    Dim RetRS As Point = If((_Cant < 0), RSLeft, RSRight)
                    Return RetRS
                End Get
            End Property
            
            ''' <summary> Gets coordinates of Right Running Surface in track system. </summary>
             ''' <remarks> This value can e set via <see cref="RailPair.reconfigure"/>. </remarks>
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
        
        #Region "Methods"
            
            ''' <summary> Re-configures this RailPair based on running surfaces. </summary>
             ''' <param name="RunningSurface1"> Coordinates of one RunningSurface in track system. </param>
             ''' <param name="RunningSurface2"> Coordinates of the other RunningSurface in track system. </param>
             ''' <exception cref="System.ArgumentException"> At least one coordinate of the points is <c>Dougble.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> RunningSurface1 and RunningSurface2 are equal. </exception>
             ''' <remarks> Right and left running surface are determined automatically. Cant and cantbase will be calculated. Radius will be set to +/- 1, if it's still <c>Double.NaN</c>. </remarks>
            Public Sub reconfigure(RunningSurface1 As Point, RunningSurface2 As Point)
                
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
                If (Abs(_Cant) < CantZeroTol) Then _Cant = 0.0
                
                If (Double.IsNaN(_Radius)) Then Me.Radius = 1 * Sign(_Cant)
                
                IsCantDeficiencyValid = False
                _IsConfigured  = True
            End Sub
            
            ''' <summary> Re-configures this RailPair based on cant and cantbase. </summary>
             ''' <param name="Cant"> Cant in [m]. </param>
             ''' <param name="CantBase"> CantBase in [m]. </param>
             ''' <exception cref="System.ArgumentException"> Cant is <c>Double.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> CantBase is <c>Double.NaN</c> or less than 0.001. </exception>
             ''' <remarks> Right and left running surface are determined automatically. Cant and cantbase will be calculated. Radius will be set to +/- 1, if it's still <c>Double.NaN</c>. </remarks>
            Public Sub reconfigure(Cant As Double, CantBase As Double)
                
                If (Double.IsNaN(Cant))     Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownCant, "Cant")
                If (Double.IsNaN(CantBase)) Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_UnknownCantBase, "CantBase")
                If (CantBase < MinCantBase) Then Throw New System.ArgumentException(Rstyx.Utilities.Resources.Messages.RailPair_InvalidCantBaseNegative, "CantBase")
                
                ' Check close to zero.
                If (Abs(Cant) < CantZeroTol) Then
                    _Cant = 0.0
                Else
                    _Cant = Cant
                End If
                
                Dim yl  As Double
                Dim yr  As Double
                Dim xl  As Double
                Dim xr  As Double
                'Dim hd2 As Double = Sqrt(Pow(CantBase, 2) - Pow(Cant, 2)) / 2
                Dim cbh As Double = Sqrt(Pow(CantBase, 2) - Pow(_Cant, 2))
                
                If (_Cant < 0) Then
                    Yl = 0.0
                    yr = Abs(_Cant)
                    xl = -CantBase / 2
                    xr = cbh + xl
                Else
                    Yl = _Cant
                    yr = 0.0
                    xr = CantBase / 2
                    xl = -cbh + xr
                End If
                
                _CantBase = CantBase
                _RSLeft   = New Point(xl, yl)
                _RSRight  = New Point(xr, yr)
                '_RSLeft   = New Point(-hd2, yl)
                '_RSRight  = New Point(hd2, yr)
                If (Double.IsNaN(_Radius)) Then Me.Radius = 1 * Sign(_Cant)
                
                IsCantDeficiencyValid = False
                _IsConfigured  = True
            End Sub
            
            ''' <summary> Resets the configuration of this RailPair to "unknown". </summary>
            Public Sub reset()
                Me.Speed        = Double.NaN
                Me.Radius       = Double.NaN
                _CantDeficiency = Double.NaN
                _Cant           = Double.NaN
                _CantBase       = Double.NaN
                _RSLeft         = New Point(Double.NaN, Double.NaN)
                _RSRight        = New Point(Double.NaN, Double.NaN)
                _IsConfigured   = False
                IsCantDeficiencyValid = False
            End Sub
            
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
             ''' <returns> Cant Deficiency in [m]. </returns>
            Private Function CalculateCantDeficiency() As Double
            
                Dim uf  As Double = Double.NaN
                
                If (Not (Double.IsNaN(_Cant) OrElse Double.IsNaN(_Radius) OrElse Double.IsNaN(_Speed) )) Then
                    
                    uf = (11.8 * _Speed * _Speed / AbsRadius / 1000) - (Sign(_Radius) * _Cant)   ' In [m].
                End If
                
                Return uf
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
