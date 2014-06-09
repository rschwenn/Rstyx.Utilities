
Imports System
Imports System.Windows

Namespace Domain
    
    ''' <summary> A pair of Rails at a discrete spot (cross section). </summary>
     ''' <remarks> To be valid, the <see cref="RailPair.reconfigure"/> method has to be applied successfuly. </remarks>
    Public Class RailPair
        Inherits Cinch.ValidatingObject
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Microstation.Addin.Lichtraum.RailPair")
            
            Private Shared UnknownConfigurationRule As Cinch.SimpleRule
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Static initializations. </summary>
            Shared Sub New()
                UnknownConfigurationRule = New Cinch.SimpleRule("IsConfigured",
                                                                Rstyx.Utilities.Resources.Messages.RailPair_UnknownConfiguration,
                                                                Function (oValidatingObject As Object) (Not DirectCast(oValidatingObject, RailPair).IsConfigured))
            End Sub
            
            ''' <summary> Creates a new RailPair with unknown configuration. </summary>
            Public Sub New()
                Me.reset()
                Me.AddRule(UnknownConfigurationRule)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _Cant           As Double
            Private _CantBase       As Double
            Private _RSLeft         As Point
            Private _RSRight        As Point
            Private _IsConfigured   As Boolean
            
            ''' <summary> Gets the Cant (negative, if right running surface is higher). </summary>
            Public ReadOnly Property Cant() As Double
                Get
                    Cant = _Cant
                End Get
            End Property
            
            ''' <summary> Gets the CantBase. </summary>
            Public ReadOnly Property CantBase() As Double
                Get
                    CantBase = _CantBase
                End Get
            End Property
            
            ''' <summary> Coordinates of Left Running Surface in track system. </summary>
            Public ReadOnly Property RSLeft() As Point
                Get
                    RSLeft = _RSLeft
                End Get
            End Property
            
            ''' <summary> Coordinates of Right Running Surface in track system. </summary>
            Public ReadOnly Property RSRight() As Point
                Get
                    RSRight = _RSRight
                End Get
            End Property
            
            ''' <summary> If <see langword="false"/>, the configuration of this RailsPair is unknown. Otherwise all properties are valid. </summary>
            Public ReadOnly Property IsConfigured() As Boolean
                Get
                    IsConfigured = _IsConfigured
                End Get
            End Property
            
        #End Region
        
        #Region "Methods"
            
            ''' <summary> Re-configures this RailPair based on running surfaces. </summary>
             ''' <param name="RunningSurface1"> Coordinates of one RunningSurface in track system. </param>
             ''' <param name="RunningSurface2"> Coordinates of the other RunningSurface in track system. </param>
             ''' <exception cref="System.ArgumentException"> At least one coordinate of the points is <c>Dougble.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> RunningSurface1 and RunningSurface2 are equal. </exception>
             ''' <remarks> Right and left running surface are determined automatically. Cant and cantbase will be calculated. </remarks>
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
                
                _IsConfigured  = True
            End Sub
            
            ''' <summary> Re-configures this RailPair based on cant and cantbase. </summary>
             ''' <param name="Cant"> Cant in [m]. </param>
             ''' <param name="CantBase"> CantBase in [m]. </param>
             ''' <exception cref="System.ArgumentException"> Cant is <c>Dougble.NaN</c>. </exception>
             ''' <exception cref="System.ArgumentException"> CantBase is <c>Dougble.NaN</c> or less than 0.001. </exception>
             ''' <remarks> Right and left running surface are determined automatically. Cant and cantbase will be calculated. </remarks>
            Public Sub reconfigure(Cant As Double, CantBase As Double)
                
                If (Double.IsNaN(Cant))     Then Throw New System.ArgumentException("Cant is NaN")
                If (Double.IsNaN(CantBase)) Then Throw New System.ArgumentException("CantBase is NaN")
                If (CantBase < 0.001)       Then Throw New System.ArgumentException("CantBase is Zero")
                
                Dim yl  As Double
                Dim yr  As Double
                Dim hd2 As Double = Math.Sqrt(Math.Pow(CantBase, 2) - Math.Pow(Cant, 2)) / 2
                
                If (Cant < 0) Then
                    Yl = 0.0
                    yr = Math.Abs(Cant)
                Else
                    Yl = Cant
                    yr = 0.0
                End If
                
                _Cant     = Cant
                _CantBase = CantBase
                _RSLeft   = New Point(-hd2, yl)
                _RSRight  = New Point(hd2, yr)
                
                _IsConfigured  = True
            End Sub
            
            ''' <summary> Resets the configuration of this RailPair to "unknown". </summary>
            Public Sub reset()
                _Cant       = Double.NaN
                _CantBase   = Double.NaN
                _RSLeft     = New Point(Double.NaN, Double.NaN)
                _RSRight    = New Point(Double.NaN, Double.NaN)
                _IsConfigured    = False
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
           ''' <inheritdoc/>
            Public Overrides Function ToString() As String
                Dim RetValue As String
                If (Not Me.IsConfigured) Then 
                    RetValue = MyBase.ToString()
                Else
                    RetValue = Rstyx.Utilities.StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.RailPair_ToString, Me.Cant, Me.CantBase)
                End If
                Return RetValue
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
