
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
            Public Property TrackRef()      As TrackGeometryInfo = Nothing Implements IPointAtTrackGeometry.TrackRef
            
        #End Region
        
        #Region "Private members"
            
            ' <summary> Collects all information. </summary>
            'Private Sub initGeoTcPoint()
                'Try
                    '' Preliminaries
                    '_AssemblyName = New System.Reflection.AssemblyName(_Assembly.FullName)
                    '
                    '' Assembly title
                    'Dim Attributes As Object() = _Assembly.GetCustomAttributes(GetType(System.Reflection.AssemblyTitleAttribute), False)
                    'If (Attributes.Length > 0) Then
                    '    _Title = CType(Attributes(0), System.Reflection.AssemblyTitleAttribute).Title
                    'End If
                    'If (String.IsNullOrWhiteSpace(_Title)) Then
                    '    _Title = System.IO.Path.GetFileNameWithoutExtension(_Assembly.Location)
                    'End If
                    '
                    '' Assembly version
                    '_Version = _AssemblyName.Version
                    
                'Catch ex As System.Exception
                '    Logger.logError(ex, "initGeoTcPoint(): Fehler beim Bestimmen der Anwendungsinformationen.")
                'End Try
            'End Sub
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
