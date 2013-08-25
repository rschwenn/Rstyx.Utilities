
Imports System

Namespace Domain
    
    ''' <summary> Shortcut for a <see cref="GeoTcPoint(Of String)"/>, representing the most usual case: a string identifier. </summary>
    Public Class GeoTcPoint
        Inherits GeoTcPoint(Of String)
    End Class
    
    ''' <summary> Representation of a geodetic point with track geometry values. </summary>
     ''' <typeparam name="TPointID"> Type of the Point ID. </typeparam>
     ''' <remarks></remarks>
    Public Class GeoTcPoint(Of TPointID)
        Inherits GeoPoint(Of TPointID)
        
        Implements IDTMatPoint
        Implements IPointAtTrackGeometry
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoTcPoint")
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new GeoTcPoint. </summary>
            Public Sub New()
            End Sub
            
        #End Region
        
        #Region "IDTMatPoint Members"
            
            ''' <inheritdoc/>
            Public Property ModelZ()        As Double = Double.NaN   Implements IDTMatPoint.ModelZ
            
            ''' <inheritdoc/>
            Public Property dZ()            As Double = Double.NaN   Implements IDTMatPoint.dZ
            
            ''' <inheritdoc/>
            Public Property NameOfDTM()     As String = String.Empty Implements IDTMatPoint.NameOfDTM
            
        #End Region
        
        #Region "IPointAtTrackGeometry Members"
            
            ''' <inheritdoc/>
            Public Property St()            As Double = Double.NaN Implements IPointAtTrackGeometry.St
            
            ''' <inheritdoc/>
            Public Property Km()            As Double = Double.NaN Implements IPointAtTrackGeometry.Km
            
            ''' <inheritdoc/>
            Public Property Q()             As Double = Double.NaN Implements IPointAtTrackGeometry.Q
            
            ''' <inheritdoc/>
            Public Property QKm()           As Double = Double.NaN Implements IPointAtTrackGeometry.QKm
            
            ''' <inheritdoc/>
            Public Property HSOK()          As Double = Double.NaN Implements IPointAtTrackGeometry.HSOK
            
            ''' <inheritdoc/>
            Public Property H()             As Double = Double.NaN Implements IPointAtTrackGeometry.H
            
            ''' <inheritdoc/>
            Public Property ActualCant()    As Double = Double.NaN Implements IPointAtTrackGeometry.ActualCant
            
            
            ''' <inheritdoc/>
            Public Property V()             As Double = Double.NaN Implements IPointAtTrackGeometry.V
            
            ''' <inheritdoc/>
            Public Property R()             As Double = Double.NaN Implements IPointAtTrackGeometry.R
            
            ''' <inheritdoc/>
            Public Property L()             As Double = Double.NaN Implements IPointAtTrackGeometry.L
            
            
            ''' <inheritdoc/>
            Public Property QG()            As Double = Double.NaN Implements IPointAtTrackGeometry.QG
            
            ''' <inheritdoc/>
            Public Property HG()            As Double = Double.NaN Implements IPointAtTrackGeometry.HG
            
            
            ''' <inheritdoc/>
            Public Property Ra()            As Double = Double.NaN Implements IPointAtTrackGeometry.Ra
            
            ''' <inheritdoc/>
            Public Property Ri()            As Double = Double.NaN Implements IPointAtTrackGeometry.Ri
            
            ''' <inheritdoc/>
            Public Property Ueb()           As Double = Double.NaN Implements IPointAtTrackGeometry.Ueb
            
            ''' <inheritdoc/>
            Public Property Heb()           As Double = Double.NaN Implements IPointAtTrackGeometry.Heb
            
            ''' <inheritdoc/>
            Public Property ZSOK()          As Double = Double.NaN Implements IPointAtTrackGeometry.ZSOK
            
                
            ''' <inheritdoc/>
            ''' <remarks> This value defaults to 1.500. </remarks>
            Property CantBase()             As Double = 1.500 Implements IPointAtTrackGeometry.CantBase
            
            
            ''' <inheritdoc/>
            Public Property TrackRef()      As TrackGeometryInfo = Nothing Implements IPointAtTrackGeometry.TrackRef
            
        #End Region
        
        #Region "Public members"
            
            ''' <summary> Transforms <see cref="P:Q"/> and <see cref="P:HSOK"/> to <see cref="P:QG"/> and <see cref="P:HG"/> if possible. </summary>
             ''' <remarks> If this transformation isn't possible, the target values will be <c>Double.NaN</c>. </remarks>
            Public Sub transformHorizontalToCanted()
                
                If ((Me.Ueb > -0.0005) And (Me.Ueb < -0.0005)) Then
                    Me.QG = Me.Q
                    Me.HG = Me.HSOK
                    
                ElseIf (Not (Double.IsNaN(Me.Ra) OrElse (Me.Ra = 0.0))) Then
                    Dim sf  As Integer = Math.Sign(Me.Ra)
                    Dim phi As Double = sf * Math.Atan(Me.Ueb / Me.CantBase) * (-1)
                    Dim X0  As Double = Math.Abs(Me.CantBase / 2 * Math.Sin(phi))
                    Dim Y0  As Double = sf * (Me.CantBase / 2 - (Me.CantBase / 2 * Math.Cos(phi)))
                    
                    Me.QG = (Me.Q - Y0) * Math.Cos(phi) + (Me.HSOK - X0) * Math.Sin(phi)
                    Me.HG = (Me.HSOK - X0) * Math.Cos(phi) - (Me.Q - Y0) * Math.Sin(phi)
                Else
                    Me.QG = Double.NaN
                    Me.HG = Double.NaN
                End If
            End Sub
            
            ''' <summary> Transforms <see cref="P:QG"/> and <see cref="P:HG"/> to <see cref="P:Q"/> and <see cref="P:HSOK"/> if possible. </summary>
             ''' <remarks> If this transformation isn't possible, the target values will be <c>Double.NaN</c>. </remarks>
            Public Sub transformCantedToHorizontal()
                
                If ((Me.Ueb > -0.0005) And (Me.Ueb < -0.0005)) Then
                    Me.Q    = Me.QG
                    Me.HSOK = Me.HG
                    
                ElseIf (Not (Double.IsNaN(Me.Ra) OrElse (Me.Ra = 0.0))) Then
                    Dim sf  As Integer = Math.Sign(Me.Ra)
                    Dim phi     As Double = sf * Math.Atan(Me.Ueb / Me.CantBase) * (-1)
                    Dim CosPhi  As Double = Math.Cos(phi)
                    Dim SinPhi  As Double = Math.Sin(phi)
                    Dim X0      As Double = Math.Abs(Me.CantBase / 2 * Math.Sin(phi))
                    Dim Y0      As Double = sf * (Me.CantBase / 2 - (Me.CantBase / 2 * Math.Cos(phi)))
                    
                    Me.HSOK = X0 + (Me.QG / CosPhi + Me.HG / SinPhi) / (CosPhi / SinPhi + SinPhi / CosPhi)
                    Me.Q = Y0 + Me.QG / CosPhi - (Me.HSOK - X0) * SinPhi
                Else
                    Me.Q    = Double.NaN
                    Me.HSOK = Double.NaN
                End If
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns a formatted output of most fields of this GeoTcPoint. </summary>
             ''' <returns> Formatted output. </returns>
            Public Overrides Function ToString() As String
                Return StringUtils.sprintf("%+15s %10.3f %10.3f %8.3f %8.3f   %8.3f %8.3f %11.3f  %4.0f   %4.0f %8.3f   %-13s  %12.3f %12.3f%9.3f",
                                           Me.ID, Me.Km, Me.St, Me.Q, Me.HSOK, Me.QG, Me.HG, Me.Ra, Me.Ueb, Me.ActualCant, Me.ZSOK, Me.Info, Me.Y, Me.X, Me.Z)
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
