
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
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoTcPoint"/> and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks>  </remarks>
             ''' <exception cref="ParseException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point (The <see cref="ParseError"/> only contains a message.). </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                MyBase.New(SourcePoint)
                
                ' TC + KV specials.
                If (TypeOf SourcePoint Is GeoTcPoint) Then
                    
                    Dim SourceTcPoint As GeoTcPoint = DirectCast(SourcePoint, GeoTcPoint)
                    
                    Me.CantBase   = SourceTcPoint.CantBase
                    Me.ActualCant = SourceTcPoint.ActualCant
                    Me.G          = SourceTcPoint.G
                    Me.H          = SourceTcPoint.H
                    Me.HDGM       = SourceTcPoint.HDGM
                    Me.HG         = SourceTcPoint.HG
                    Me.HGS        = SourceTcPoint.HGS
                    Me.HGT        = SourceTcPoint.HGT
                    Me.HSOK       = SourceTcPoint.HSOK
                    Me.Heb        = SourceTcPoint.Heb
                    Me.Km         = SourceTcPoint.Km
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
                    Me.Ri         = SourceTcPoint.Ri
                    Me.St         = SourceTcPoint.St
                    Me.TM         = SourceTcPoint.TM
                    Me.TrackRef   = SourceTcPoint.TrackRef
                    Me.Ueb        = SourceTcPoint.Ueb
                    Me.V          = SourceTcPoint.V
                    Me.ZDGM       = SourceTcPoint.ZDGM
                    Me.ZLGS       = SourceTcPoint.ZLGS
                    Me.ZSOK       = SourceTcPoint.ZSOK
                
                ElseIf (TypeOf SourcePoint Is GeoVEPoint) Then
                    
                    Dim SourceVEPoint As GeoVEPoint = DirectCast(SourcePoint, GeoVEPoint)
                    
                    Me.Km = SourceVEPoint.TrackPos.Kilometer
                End If
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
            Public Property TM()            As Double = Double.NaN Implements IPointAtTrackGeometry.Tm
            
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
            Public Property ActualCant()    As Double = Double.NaN Implements IPointAtTrackGeometry.ActualCant
            
            ''' <inheritdoc/>
            Public Property Ueb()           As Double = Double.NaN Implements IPointAtTrackGeometry.Ueb
            
            ''' <inheritdoc/>
            Public Property Heb()           As Double = Double.NaN Implements IPointAtTrackGeometry.Heb
            
            ''' <inheritdoc/>
            ''' <remarks> This value defaults to 1.500. </remarks>
            Public Property CantBase()      As Double = 1.500 Implements IPointAtTrackGeometry.CantBase
            
            
            ''' <inheritdoc/>
            Public Property TrackRef()      As TrackGeometryInfo = Nothing Implements IPointAtTrackGeometry.TrackRef
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Transforms <see cref="GeoTcPoint.Q"/> and <see cref="GeoTcPoint.HSOK"/> to <see cref="GeoTcPoint.QG"/> and <see cref="GeoTcPoint.HG"/> if possible. </summary>
             ''' <para>
             ''' Sign of cant: Positive is treated as "normal", negative as "inverse".
             ''' </para>
             ''' <para>
             ''' If this transformation isn't possible, the target values will be <c>Double.NaN</c>.
             ''' </para>
            Public Sub transformHorizontalToCanted()
                
                If (Me.Ueb.EqualsTolerance(0, 0.0005)) Then
                    
                    Me.QG = Me.Q
                    Me.HG = Me.HSOK
                    
                ElseIf (Not (Double.IsNaN(Me.Ueb) OrElse Double.IsNaN(Me.CantBase) OrElse Double.IsNaN(Me.Ra) OrElse Me.Ra.EqualsTolerance(0, 0.001))) Then
                    
                    Dim sf  As Integer = Sign(Me.Ra) * Sign(Me.Ueb)
                    Dim cbh As Double  = Sqrt(Pow(Me.CantBase, 2) - Pow(Me.Ueb, 2))
                    Dim phi As Double  = sf * Atan(Abs(Me.Ueb) / cbh) * (-1)
                    Dim X0  As Double  = Abs(Me.Ueb) / 2
                    Dim Y0  As Double  = sf * (Me.CantBase - cbh) / 2
                    
                    Me.QG = (Me.Q - Y0) * Cos(phi) + (Me.HSOK - X0) * Sin(phi)
                    Me.HG = (Me.HSOK - X0) * Cos(phi) - (Me.Q - Y0) * Sin(phi)
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
             ''' If this transformation isn't possible, the target values will be <c>Double.NaN</c>.
             ''' </para>
             ''' </remarks>
            Public Sub transformCantedToHorizontal()
                
                If (Me.Ueb.EqualsTolerance(0, 0.0005)) Then
                    
                    Me.Q    = Me.QG
                    Me.HSOK = Me.HG
                    
                ElseIf (Not (Double.IsNaN(Me.Ueb) OrElse Double.IsNaN(Me.CantBase) OrElse Double.IsNaN(Me.Ra) OrElse Me.Ra.EqualsTolerance(0, 0.001))) Then
                    
                    Dim sf      As Integer = Sign(Me.Ra) * Sign(Me.Ueb)
                    Dim cbh     As Double  = Sqrt(Pow(Me.CantBase, 2) - Pow(Me.Ueb, 2))
                    Dim phi     As Double  = sf * Atan(Abs(Me.Ueb) / cbh) * (-1)
                    Dim CosPhi  As Double  = Cos(phi)
                    Dim SinPhi  As Double  = Sin(phi)
                    Dim X0      As Double  = Abs(Me.Ueb) / 2
                    Dim Y0      As Double  = sf * (Me.CantBase - cbh) / 2
                    
                    Me.HSOK = X0 + (Me.QG / CosPhi + Me.HG / SinPhi) / (CosPhi / SinPhi + SinPhi / CosPhi)
                    Me.Q    = Y0 + (Me.QG - (Me.HSOK - X0) * SinPhi) / CosPhi
                Else
                    Me.Q    = Double.NaN
                    Me.HSOK = Double.NaN
                End If
            End Sub
            
            ''' <summary> Parses actual cant value from <see cref=" GeoTcPoint.Info"/> and maybe <see cref=" GeoTcPoint.Comment"/> property. </summary>
             ''' <param name="TryComment">     If <see langword="true"/> and no cant has been found in <see cref=" GeoTcPoint.Info"/>, the <see cref=" GeoTcPoint.Comment"/> will be parsed, too. </param>
             ''' <param name="Strict">         If <see langword="true"/>, only the pattern "u=..." is recognized. Otherwise, if this pattern isn't found, the first integer number is used. </param>
             ''' <param name="Absolute">       If <see langword="true"/>, the parsed Cant value is always positive. </param>
             ''' <param name="EditCantSource"> If <see langword="true"/>, the Cant pattern substring is removed from <see cref=" GeoTcPoint.Info"/>. </param>
             ''' <returns> <see langword="true"/> on success. </returns>
             ''' <remarks> 
             ''' <para>
             ''' The measured cant will be parsed from <see cref=" GeoTcPoint.Info"/> following these rules:
             ''' </para>
             ''' <para>
             ''' Case 1: If the string "u= xxx" is found anywhere then "xxx" will be treated as measured cant.
             ''' </para>
             ''' <para>
             ''' If case 1 didn't succeeded and <paramref name="Strict"/> is <see langword="false"/>,
             ''' then the first integer number will be used, if any.
             ''' </para>
             ''' <para>
             ''' The found cant will be assigned to the <see cref=" GeoTcPoint"/><c>.ActualCant</c> property.
             ''' </para>
             ''' </remarks>
            Public Function TryParseActualCant(Optional byVal TryComment As Boolean = False,
                                               Optional byVal Strict As Boolean = True,
                                               Optional byVal Absolute As Boolean = False,
                                               Optional byVal EditCantSource As Boolean = False
                                              ) As Boolean
                ' Parse point info.
                Me.ActualCant = TryParseActualCant(Pointinfo:=Me.Info, Strict:=Strict, Absolute:=Absolute, EditCantSource:=EditCantSource)
                
                ' Fallback: Parse point comment if needed.
                If (TryComment AndAlso Double.IsNaN(Me.ActualCant)) Then
                    Me.ActualCant = TryParseActualCant(Pointinfo:=Me.Comment, Strict:=Strict, Absolute:=Absolute, EditCantSource:=EditCantSource)
                End If
                
                Return (Not Double.IsNaN(Me.ActualCant))
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns a formatted output of most fields of this GeoTcPoint. </summary>
             ''' <returns> Formatted output. </returns>
            Public Overrides Function ToString() As String
                Return StringUtils.sprintf("%+20s %10.3f %10.3f %8.3f %8.3f   %8.3f %8.3f %8.3f %11.3f  %4.0f   %4.0f %8.3f   %-16s %12.3f %12.3f%9.3f",
                                           Me.ID, Me.Km.Value, Me.St.Value, Me.Q, Me.HSOK, Me.QG, Me.HG, Me.RG, Me.Ra, Me.Ueb * 1000, Me.ActualCant * 1000, Me.ZSOK, Me.Info, Me.Y, Me.X, Me.Z)
            End Function
            
        #End Region
        
        #Region "Private Methods"
            
            ''' <summary> Parses Cant value from a Pointinfo string. </summary>
             ''' <param name="Pointinfo">      [Input and Output] The Pointinfo string to parse. </param>
             ''' <param name="strict">         If <see langword="true"/>, only the pattern "u=..." is recognized. Otherwise, if this pattern isn't found, the first integer number is used. </param>
             ''' <param name="absolute">       If <see langword="true"/>, the returned Cant value is always positive. </param>
             ''' <param name="EditCantSource"> If <see langword="true"/>, the Cant pattern substring is removed from Pointinfo. </param>
             ''' <returns> The found Cant value or <c>Double.NaN</c>. </returns>
             ''' <remarks> 
             ''' <para>
             ''' The measured cant will be parsed from <paramref name="Pointinfo"/> following these rules:
             ''' </para>
             ''' <para>
             ''' Case 1: If the string "u= xxx" is found anywhere then "xxx" will be treated as measured cant.
             ''' </para>
             ''' <para>
             ''' If case 1 didn't succeeded and <paramref name="strict"/> is <see langword="false"/>,
             ''' then the first integer number will be used, if any.
             ''' </para>
             ''' </remarks>
            Private Function TryParseActualCant(<[In]><Out> ByRef Pointinfo As String,
                                                byVal Strict As Boolean,
                                                byVal Absolute As Boolean,
                                                byVal EditCantSource As Boolean
                                               ) As Double
                Dim Cant    As Double  = Double.NaN
                Dim ui      As String  = String.Empty
                Dim success As Boolean = false
                
                If (Pointinfo.IsNotEmptyOrWhiteSpace()) Then
                    ' 1. search for "u=..."
                    Dim Pattern As String = "u *= *([-|+]? *[0-9]+)"
                    Dim oMatch  As Match = Regex.Match(Pointinfo, Pattern, RegexOptions.IgnoreCase)
                    If (oMatch.Success) Then
                        ui = oMatch.Groups(1).Value
                        ui = ui.Replace(" ", String.Empty)
                    End If
                    
                    ' 2. "u=..." not found, look for first integer number.
                    If (ui.IsEmptyOrWhiteSpace() AndAlso (Not Strict)) Then
                        Pattern = "[-|+]? *[0-9]+"
                        oMatch = Regex.Match(Pointinfo, Pattern, RegexOptions.IgnoreCase)
                        If (oMatch.Success) Then
                          ui = oMatch.Value
                          ui = ui.Replace(" ", String.Empty)
                        End If
                    End If
                    
                    ' Result.
                    If (ui.IsNotEmpty()) Then
                        ' 3. Cant value found.
                        Cant = cdbl(ui)
                        If (Absolute) Then Cant = Abs(Cant.ConvertTo(Of Double))
                        If (EditCantSource) Then Pointinfo = Pointinfo.replace(oMatch.Groups(0).Value, String.Empty)
                    End If
                End If
                
                Return Cant / 1000
            End Function
            
        #End Region
        
        #Region "Backup (Old Methods)"
            
            ''' <summary> Transforms <see cref="GeoTcPoint.Q"/> and <see cref="GeoTcPoint.HSOK"/> to <see cref="GeoTcPoint.QG"/> and <see cref="GeoTcPoint.HG"/> if possible. </summary>
             ''' <para>
             ''' Sign of cant: Positive is treated as "normal", negative as "inverse".
             ''' </para>
             ''' <para>
             ''' If this transformation isn't possible, the target values will be <c>Double.NaN</c>.
             ''' </para>
            Private Sub transformHorizontalToCanted_old()
                
                If ((Me.Ueb > -0.0005) And (Me.Ueb < 0.0005)) Then
                    Me.QG = Me.Q
                    Me.HG = Me.HSOK
                    
                ElseIf (Not (Double.IsNaN(Me.Ra) OrElse (Me.Ra = 0.0))) Then
                    Dim sf  As Integer = Sign(Me.Ra)
                    Dim phi As Double  = sf * Atan(Me.Ueb / Me.CantBase) * (-1)
                    Dim X0  As Double  = Abs(Me.CantBase / 2 * Sin(phi))
                    Dim Y0  As Double  = sf * (Me.CantBase / 2 - (Me.CantBase / 2 * Cos(phi)))
                    
                    Me.QG = (Me.Q - Y0) * Cos(phi) + (Me.HSOK - X0) * Sin(phi)
                    Me.HG = (Me.HSOK - X0) * Cos(phi) - (Me.Q - Y0) * Sin(phi)
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
             ''' If this transformation isn't possible, the target values will be <c>Double.NaN</c>.
             ''' </para>
             ''' </remarks>
            Private Sub transformCantedToHorizontal_old()
                
                If ((Me.Ueb > -0.0005) And (Me.Ueb < 0.0005)) Then
                    Me.Q    = Me.QG
                    Me.HSOK = Me.HG
                    
                ElseIf (Not (Double.IsNaN(Me.Ra) OrElse (Me.Ra = 0.0))) Then
                    Dim sf      As Integer = Sign(Me.Ra)
                    Dim phi     As Double = sf * Atan(Me.Ueb / Me.CantBase) * (-1)
                    Dim CosPhi  As Double = Cos(phi)
                    Dim SinPhi  As Double = Sin(phi)
                    Dim X0      As Double = Abs(Me.CantBase / 2 * Sin(phi))
                    Dim Y0      As Double = sf * (Me.CantBase / 2 - (Me.CantBase / 2 * Cos(phi)))
                    
                    Me.HSOK = X0 + (Me.QG / CosPhi + Me.HG / SinPhi) / (CosPhi / SinPhi + SinPhi / CosPhi)
                    Me.Q = Y0 + Me.QG / CosPhi - (Me.HSOK - X0) * SinPhi
                Else
                    Me.Q    = Double.NaN
                    Me.HSOK = Double.NaN
                End If
            End Sub
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
