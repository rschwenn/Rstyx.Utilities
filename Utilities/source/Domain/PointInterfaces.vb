
Namespace Domain
    
    ''' <summary> Specifies that the implementing object is identified by an object of specified generic parameter. </summary>
     ''' <typeparam name="TKey"> Type of the Identifier. </typeparam>
    Public Interface IIdentifiable(Of TKey)
        
        ''' <summary> An ID of a given Type. </summary>
        Property ID             As Tkey
        
    End Interface
    
    
    ''' <summary> A set of properties describing a geodetic point. </summary>
    Public Interface IGeoPointInfo
        
        ''' <summary> A comment. </summary>
        Property Comment()      As String
        
        ''' <summary> An arbitrary information text. </summary>
        Property Info()         As String
        
        ''' <summary> The point's kind or type. </summary>
        Property Kind()         As String
        
        ''' <summary> The type of marking used to realize the point. </summary>
        Property MarkType()     As String
        
        ''' <summary> Hints for marking. </summary>
        Property MarkHints()    As String
        
        ''' <summary> A unique object key (or feature key). </summary>
        Property ObjectKey()    As String
        
    End Interface
    
    
    ''' <summary> A cartesian coordinates triple with specified reference systems and precision. </summary>
    Public Interface ICartesianCoordinates3D
        
        ''' <summary> The easting coordinate. </summary>
        Property Y()            As Double
        
        ''' <summary> The northing coordinate. </summary>
        Property X()            As Double
        
        ''' <summary> The height coordinate. </summary>
        Property Z()            As Double
        
        
        ''' <summary> Mean error of position. </summary>
        Property mp()           As Double
        
        ''' <summary> Mean error of height. </summary>
        Property mh()           As Double
        
        
        ''' <summary> Specifies the reference system of easting and northing coordinates. </summary>
        Property CoordSys()     As String
        
        ''' <summary> Specifies the reference system of height coordinate. </summary>
        Property HeightSys()    As String
        
    End Interface
    
    
    ''' <summary> Digital terrain model values at a given point. </summary>
    Public Interface IDTMatPoint
        
        ''' <summary> Height of the model surface. </summary>
        Property ModelZ()       As Double
        
        ''' <summary> Height of the point above the model surface. </summary>
         ''' <remarks> This is the difference between Z coordinates of model and point. </remarks>
        Property dZ()           As Double
        
        ''' <summary> Model name. </summary>
        Property NameOfDTM()    As String
        
    End Interface
    
    
    ''' <summary> Point values relative to a track geometry and also geometry design parameters. </summary>
    Public Interface IPointAtTrackGeometry
        
        ''' <summary> Station along the alignment. </summary>
        Property St()           As Double
        
        ''' <summary> Kilometer along a separate kilometer leading alignment. </summary>
        Property Km()           As Double
        
        ''' <summary> Perpendicular distance in XY-plane to the alignment. </summary>
        Property Q()            As Double
        
        ''' <summary> Perpendicular distance in XY-plane to the kilometer leading alignment. </summary>
        Property QKm()          As Double
        
        ''' <summary> Height above running surface of rail. </summary>
        Property HSOK()         As Double
        
        ''' <summary> Height above gradient design (vertical curve set). </summary>
        Property H()            As Double
        
        ''' <summary> Actual cant. </summary>
        Property ActualCant()   As Double
        
        
        ''' <summary> Height above road cross section shape. </summary>
        Property V()            As Double
        
        ''' <summary> Radial distance to the tunnel cross section shape. </summary>
        Property R()            As Double
        
        ''' <summary> Lenght along the tunnel cross section shape. </summary>
        Property L()            As Double
        
        
        ''' <summary> Twisted track coordinate system: Perpendicular distance in XY-plane to the alignment. </summary>
        Property QG()           As Double
        
        ''' <summary> Twisted track coordinate system: Height above running surface of rail. </summary>
        Property HG()           As Double
        
        
        ''' <summary> Radius. </summary>
        Property Ra()           As Double
        
        ''' <summary> Azimuth of alignment tangent. </summary>
        Property Ri()           As Double
        
        ''' <summary> Cant (Superelevation). </summary>
        Property Ueb()          As Double
        
        ''' <summary> Height difference between gradient design (vertical curve set) and running surface of rail (at special geometry). </summary>
        Property Heb()          As Double     'Hebewert in der Schere [mm]
        
        ''' <summary> Height coordinate of running surface of rail. </summary>
        Property ZSOK()         As Double
        
        
        ''' <summary> Determines the reference frame of the track geometry. </summary>
        Property TrackRef()     As TrackGeometryInfo
        
    End Interface
    
    
    ''' <summary> Determines the source of a point in a file. </summary>
    Public Interface IFileSource
            
        ''' <summary> An index into a list of source files (which may be mmaintained in a parent object). </summary>
        Property SourceFileIndex    As Long
        
        ''' <summary> Line number in source file, where the data has been read from. </summary>
         ''' <remarks> When the source record is multi-line, then this points to the first line. </remarks>
        Property SourceLineNo()     As Long
        
    End Interface
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
