
Imports System.Math
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.Math.MathExtensions

Namespace Domain
    
    ''' <summary> Representation of a geodetic point with track geometry values. </summary>
     ''' <remarks></remarks>
    Public Class GeoTcPoint
        Inherits GeoPoint
        
        Implements IDTMatPoint
        Implements IPointAtTrackGeometry
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoTcPoint")
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new <see cref="GeoTcPoint"/>. </summary>
            Public Sub New()
                PropertyAttributes.Add("Km", Rstyx.Utilities.Resources.Messages.Domain_AttName_TrackKm)   ' "StrKm"
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoTcPoint"/> and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks> For details see <see cref="GetPropsFromIGeoPoint"/>. </remarks>
             ''' <exception cref="ParseException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point (The <see cref="ParseError"/> only contains a message.). </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                
                Me.New()
                Me.GetPropsFromIGeoPoint(SourcePoint)
            End Sub
            
        #End Region
        
        #Region "IDTMatPoint Members"
            
            ''' <inheritdoc/>
            Public Property ZDGM()          As Double = Double.NaN   Implements IDTMatPoint.ZDGM
            
            ''' <inheritdoc/>
            Public Property HDGM()          As Double = Double.NaN   Implements IDTMatPoint.HDGM
            
            ''' <inheritdoc/>
            Public Property NameOfDTM()     As String = String.Empty Implements IDTMatPoint.NameOfDTM
            
        #End Region
        
        #Region "IPointAtTrackGeometry Members"
            
            ''' <inheritdoc/>
            Public Property St()            As Kilometer = New Kilometer() Implements IPointAtTrackGeometry.St
            
            ''' <inheritdoc/>
            Public Property Km()            As Kilometer = New Kilometer() Implements IPointAtTrackGeometry.Km
            
            ''' <inheritdoc/>
            Public Property Q()             As Double = Double.NaN Implements IPointAtTrackGeometry.Q
            
            ''' <inheritdoc/>
            Public Property QKm()           As Double = Double.NaN Implements IPointAtTrackGeometry.QKm
            
            
            ''' <inheritdoc/>
            Public Property HSOK()          As Double = Double.NaN Implements IPointAtTrackGeometry.HSOK
            
            ''' <inheritdoc/>
            Public Property H()             As Double = Double.NaN Implements IPointAtTrackGeometry.H
            
            ''' <inheritdoc/>
            Public Property G()             As Double = Double.NaN Implements IPointAtTrackGeometry.G
            
            ''' <inheritdoc/>
            Public Property ZSOK()          As Double = Double.NaN Implements IPointAtTrackGeometry.ZSOK
            
            ''' <inheritdoc/>
            Public Property ZLGS()          As Double = Double.NaN Implements IPointAtTrackGeometry.ZLGS
            
            ''' <inheritdoc/>
            Public Property RaLGS()         As Double = Double.NaN Implements IPointAtTrackGeometry.RaLGS
            
            ''' <inheritdoc/>
            Public Property AbLGS()         As Double = Double.NaN Implements IPointAtTrackGeometry.AbLGS
            
            
            ''' <inheritdoc/>
            Public Property QG()            As Double = Double.NaN Implements IPointAtTrackGeometry.QG
            
            ''' <inheritdoc/>
            Public Property HG()            As Double = Double.NaN Implements IPointAtTrackGeometry.HG
            
            ''' <inheritdoc/>
            Public Property LG()            As Double = Double.NaN Implements IPointAtTrackGeometry.LG
            
            ''' <inheritdoc/>
            Public Property RG()            As Double = Double.NaN Implements IPointAtTrackGeometry.RG
            
            
            ''' <inheritdoc/>
            Public Property V()             As Double = Double.NaN Implements IPointAtTrackGeometry.V
            
            ''' <inheritdoc/>
            Public Property QGS()           As Double = Double.NaN Implements IPointAtTrackGeometry.QGS
            
            ''' <inheritdoc/>
            Public Property HGS()           As Double = Double.NaN Implements IPointAtTrackGeometry.HGS
            
            
            ''' <inheritdoc/>
            Public Property R()             As Double = Double.NaN Implements IPointAtTrackGeometry.R
            
            ''' <inheritdoc/>
            Public Property L()             As Double = Double.NaN Implements IPointAtTrackGeometry.L
            
            ''' <inheritdoc/>
            Public Property Tm()            As Double = Double.NaN Implements IPointAtTrackGeometry.Tm
            
            ''' <inheritdoc/>
            Public Property QT()            As Double = Double.NaN Implements IPointAtTrackGeometry.QT
            
            ''' <inheritdoc/>
            Public Property QGT()           As Double = Double.NaN Implements IPointAtTrackGeometry.QGT
            
            ''' <inheritdoc/>
            Public Property HGT()           As Double = Double.NaN Implements IPointAtTrackGeometry.HGT
            
            
            ''' <inheritdoc/>
            Public Property Ra()            As Double = Double.NaN Implements IPointAtTrackGeometry.Ra
            
            ''' <inheritdoc/>
            Public Property Ri()            As Double = Double.NaN Implements IPointAtTrackGeometry.Ri
            
            
            ''' <inheritdoc/>
            Public Property Ueb()           As Double = Double.NaN Implements IPointAtTrackGeometry.Ueb
            
            ''' <inheritdoc/>
            Public Property Heb()           As Double = Double.NaN Implements IPointAtTrackGeometry.Heb
            
            
            ''' <inheritdoc/>
            ''' <remarks> This value defaults to 1.500. </remarks>
            Public Property CantBase()      As Double = 1.500 Implements IPointAtTrackGeometry.CantBase
            
            ''' <inheritdoc/>
            Public Property Speed()         As Double = Double.NaN Implements IPointAtTrackGeometry.Speed
            
            
            ''' <inheritdoc/>
            Public Property YA()            As Double = Double.NaN Implements IPointAtTrackGeometry.YA
            
            ''' <inheritdoc/>
            Public Property XA()            As Double = Double.NaN Implements IPointAtTrackGeometry.XA
            
            ''' <inheritdoc/>
            Public Property ZA()            As Double = Double.NaN Implements IPointAtTrackGeometry.ZA
            
            
            ''' <inheritdoc/>
            Public Property MiniR()         As Double = Double.NaN Implements IPointAtTrackGeometry.MiniR
            
            ''' <inheritdoc/>
            Public Property MiniUeb()       As Double = Double.NaN Implements IPointAtTrackGeometry.MiniUeb
            
            
            ''' <inheritdoc/>
            Public Property TrackRef()      As TrackGeometryInfo = Nothing Implements IPointAtTrackGeometry.TrackRef
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Transforms <see cref="GeoTcPoint.Q"/> and <see cref="GeoTcPoint.HSOK"/> to <see cref="GeoTcPoint.QG"/> and <see cref="GeoTcPoint.HG"/> if possible. </summary>
             ''' <para>
             ''' Sign of cant: Positive is treated as "normal", negative as "inverse".
             ''' </para>
             ''' <para>
             ''' If this transformation isn't possible, the target values will be set to <c>Double.NaN</c>.
             ''' </para>
            Public Sub TransformHorizontalToCanted()
                
                If (Me.Ueb.EqualsTolerance(0, 0.0005)) Then
                    
                    Me.QG = Me.Q
                    Me.HG = Me.HSOK
                    
                ElseIf (Not (Double.IsNaN(Me.Ueb) OrElse Double.IsNaN(Me.CantBase) OrElse Double.IsNaN(Me.Ra) OrElse Me.Ra.EqualsTolerance(0, 0.001))) Then
                    
                    'Dim sf      As Integer = Sign(Me.Ra) * Sign(Me.Ueb)
                    'Dim cbh     As Double  = Sqrt(Pow(Me.CantBase, 2) - Pow(Me.Ueb, 2))
                    'Dim phi     As Double  = sf * Atan(Abs(Me.Ueb) / cbh) * (-1)
                    'Dim X0      As Double  = Abs(Me.Ueb) / 2
                    'Dim Y0      As Double  = sf * (Me.CantBase - cbh) / 2
                    '
                    'Me.QG = (Me.Q - Y0) * Cos(phi) + (Me.HSOK - X0) * Sin(phi)
                    'Me.HG = (Me.HSOK - X0) * Cos(phi) - (Me.Q - Y0) * Sin(phi)
                    
                    ' 19.08.2019 (Angleichung an iGeo und VermEsn:  Nullpunkt wird nur in der Höhe verschoben um u/2)
                    Dim sf      As Integer = Sign(Me.Ra) * Sign(Me.Ueb)
                    Dim phi     As Double  = sf * Asin(Abs(Me.Ueb) / Me.CantBase) * (-1)
                    Dim CosPhi  As Double  = Cos(phi)
                    Dim SinPhi  As Double  = Sin(phi)
                    Dim X0      As Double  = Abs(Me.Ueb) / 2
                    Dim Y0      As Double  = 0.0
                    
                    Me.QG = ((Me.Q - Y0) * CosPhi) + ((Me.HSOK - X0) * SinPhi)
                    Me.HG = ((Me.HSOK - X0) * CosPhi) - ((Me.Q - Y0) * SinPhi)
            Else
                    Me.QG = Double.NaN
                    Me.HG = Double.NaN
                End If
            End Sub
            
            ''' <summary> Transforms <see cref="GeoTcPoint.QG"/> and <see cref="GeoTcPoint.HG"/> to <see cref="GeoTcPoint.Q"/> and <see cref="GeoTcPoint.HSOK"/> if possible. </summary>
             ''' <remarks>
             ''' <para>
             ''' Sign of cant: Positive is treated as "normal", negative as "inverse".
             ''' </para>
             ''' <para>
             ''' If this transformation isn't possible, the target values will be set to <c>Double.NaN</c>.
             ''' </para>
             ''' </remarks>
            Public Sub TransformCantedToHorizontal()
                
                If (Me.Ueb.EqualsTolerance(0, 0.0005)) Then
                    
                    Me.Q    = Me.QG
                    Me.HSOK = Me.HG
                    
                ElseIf (Not (Double.IsNaN(Me.Ueb) OrElse Double.IsNaN(Me.CantBase) OrElse Double.IsNaN(Me.Ra) OrElse Me.Ra.EqualsTolerance(0, 0.001))) Then
                    
                    'Dim sf      As Integer = Sign(Me.Ra) * Sign(Me.Ueb)
                    'Dim cbh     As Double  = Sqrt(Pow(Me.CantBase, 2) - Pow(Me.Ueb, 2))
                    'Dim phi     As Double  = sf * Atan(Abs(Me.Ueb) / cbh) * (-1)
                    'Dim CosPhi  As Double  = Cos(phi)
                    'Dim SinPhi  As Double  = Sin(phi)
                    'Dim X0      As Double  = Abs(Me.Ueb) / 2
                    'Dim Y0      As Double  = sf * (Me.CantBase - cbh) / 2
                    '
                    'Me.HSOK = X0 + (Me.QG / CosPhi + Me.HG / SinPhi) / (CosPhi / SinPhi + SinPhi / CosPhi)
                    'Me.Q    = Y0 + (Me.QG - (Me.HSOK - X0) * SinPhi) / CosPhi
                    
                    ' 19.08.2019 (Angleichung an iGeo und VermEsn:  Nullpunkt wird nur in der Höhe verschoben um u/2)
                    Dim sf      As Integer = Sign(Me.Ra) * Sign(Me.Ueb)
                    Dim phi     As Double  = sf * Asin(Abs(Me.Ueb) / Me.CantBase) * (-1)
                    Dim CosPhi  As Double  = Cos(phi)
                    Dim SinPhi  As Double  = Sin(phi)
                    Dim X0      As Double  = Abs(Me.Ueb) / 2
                    Dim Y0      As Double  = 0.0
                    
                    Me.HSOK = X0 + (((Me.QG / CosPhi) + (Me.HG / SinPhi)) / ((CosPhi / SinPhi) + (SinPhi / CosPhi)))
                    Me.Q    = Y0 + ((Me.QG - ((Me.HSOK - X0) * SinPhi)) / CosPhi)
            Else
                    Me.Q    = Double.NaN
                    Me.HSOK = Double.NaN
                End If
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Sets this point's <see cref="IGeoPoint"/> properties from a given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoTcPoint"/>, then all properties will be taken.
             ''' Otherwise, all <see cref="IGeoPoint"/> interface properties (including <see cref="Attributes"/>) will be assigned 
             ''' to properties of this point, and selected other properties will be converted to attributes.
             ''' </para>
             ''' <para>
             ''' Selected attributes from <paramref name="SourcePoint"/>, matching properties that don't belong to <see cref="IGeoPoint"/> interface,
             ''' and should be declared in <see cref="PropertyAttributes"/>, will be <b>converted to properties</b>, if the properties have no value yet:
             ''' <list type="table">
             ''' <listheader> <term> <b>Attribute Name</b> </term>  <description> <b>Property Name</b> </description></listheader>
             ''' <item> <term> StrKm   </term>  <description> Km </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Protected Overrides Sub GetPropsFromIGeoPoint(SourcePoint As IGeoPoint)
                
                If (SourcePoint IsNot Nothing) Then
                    
                    MyBase.GetPropsFromIGeoPoint(SourcePoint)
                
                    If (TypeOf SourcePoint Is GeoTcPoint) Then
                        
                        Dim SourceTcPoint As GeoTcPoint = DirectCast(SourcePoint, GeoTcPoint)
                        
                        Me.CantBase   = SourceTcPoint.CantBase
                        Me.AbLGS      = SourceTcPoint.AbLGS
                        Me.G          = SourceTcPoint.G
                        Me.H          = SourceTcPoint.H
                        Me.HDGM       = SourceTcPoint.HDGM
                        Me.HG         = SourceTcPoint.HG
                        Me.HGS        = SourceTcPoint.HGS
                        Me.HGT        = SourceTcPoint.HGT
                        Me.HSOK       = SourceTcPoint.HSOK
                        Me.Heb        = SourceTcPoint.Heb
                        Me.Km         = SourceTcPoint.Km.Clone()
                        Me.L          = SourceTcPoint.L
                        Me.LG         = SourceTcPoint.LG
                        Me.NameOfDTM  = SourceTcPoint.NameOfDTM
                        Me.Q          = SourceTcPoint.Q
                        Me.QG         = SourceTcPoint.QG
                        Me.QGS        = SourceTcPoint.QGS
                        Me.QGT        = SourceTcPoint.QGT
                        Me.QKm        = SourceTcPoint.QKm
                        Me.QT         = SourceTcPoint.QT
                        Me.R          = SourceTcPoint.R
                        Me.RG         = SourceTcPoint.RG
                        Me.Ra         = SourceTcPoint.Ra
                        Me.RaLGS      = SourceTcPoint.RaLGS
                        Me.Ri         = SourceTcPoint.Ri
                        Me.St         = SourceTcPoint.St.Clone()
                        Me.TM         = SourceTcPoint.TM
                        Me.TrackRef   = SourceTcPoint.TrackRef.Clone()
                        Me.Ueb        = SourceTcPoint.Ueb
                        Me.V          = SourceTcPoint.V
                        Me.ZDGM       = SourceTcPoint.ZDGM
                        Me.ZLGS       = SourceTcPoint.ZLGS
                        Me.ZSOK       = SourceTcPoint.ZSOK
                    
                        Me.RemovePropertyAttributes()
                    
                    Else
                        Me.ConvertPropertyAttributes()
                    End If
                End If
            End Sub
            
            ''' <summary> Returns a formatted output of most fields of this GeoTcPoint. </summary>
             ''' <returns> Formatted output. </returns>
            Public Overrides Function ToString() As String
                Return StringUtils.sprintf("%+20s %11.3f (%2d) %10.3f %8.3f %8.3f   %8.3f %8.3f %8.3f %11.3f  %4.0f   %4.0f   %4.0f %8.3f %7.0f %9.3f  %-16s %-10s %12.3f %12.3f%9.3f",
                                           Me.ID, Me.Km.Value, Me.Km.Status, Me.St.Value, Me.Q, Me.HSOK, Me.QG, Me.HG, Me.RG, Me.Ra, Me.Ueb * 1000, 
                                           Me.ActualCant * 1000, Me.ActualCantAbs * 1000, Me.ZSOK, Me.RaLGS, Me.ZDGM, Me.Info, Me.Kind.ToDisplayString(), Me.Y, Me.X, Me.Z)
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
