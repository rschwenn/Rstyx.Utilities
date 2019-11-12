
Imports System
Imports System.Collections.Generic

Namespace Domain
    
    ''' <summary> Representation of a geodetic point with string ID and including some point info. </summary>
     ''' <remarks></remarks>
    Public Interface IGeoPoint
        Inherits IIdentifiable(Of String)
        Inherits IGeoPointInfo
        Inherits ICartesianCoordinates3D
        Inherits IFileSource
        Inherits IGeoPointConversions
    End Interface
    
    ''' <summary> A set of methods providing conversions between different point types. </summary>
    Public Interface IGeoPointConversions
        
        ''' <summary> Returns a <see cref="GeoVEPoint"/> initialized with values of the implementing point. </summary>
         ''' <remarks>
         ''' If the implementing point is already a <see cref="GeoVEPoint"/>, then the same instance will be returned.
         ''' Otherwise a new instance of <see cref="GeoVEPoint"/> will be created.
         ''' </remarks>
        Function AsGeoVEPoint() As GeoVEPoint
        
        ''' <summary> Returns a <see cref="GeoIPoint"/> initialized with values of the implementing point. </summary>
         ''' <remarks>
         ''' If the implementing point is already a <see cref="GeoIPoint"/>, then the same instance will be returned.
         ''' Otherwise a new instance of <see cref="GeoIPoint"/> will be created.
         ''' </remarks>
        Function AsGeoIPoint() As GeoIPoint
        
        ''' <summary> Returns a <see cref="GeoTcPoint"/> initialized with values of the implementing point. </summary>
         ''' <remarks>
         ''' If the implementing point is already a <see cref="GeoTcPoint"/>, then the same instance will be returned.
         ''' Otherwise a new instance of <see cref="GeoTcPoint"/> will be created.
         ''' </remarks>
        Function AsGeoTcPoint() As GeoTcPoint
        
    End Interface
    
    ''' <summary> A set of properties describing a geodetic point. </summary>
    Public Interface IGeoPointInfo
            
        ''' <summary> A bunch of free attributes (key/value pairs). May be <see langword="null"/>. </summary>
        Property Attributes()   As Dictionary(Of String, String)
        
        ''' <summary> A comment. </summary>
        Property Comment()      As String
        
        ''' <summary> A general information text. </summary>
        Property Info()         As String

        ''' <summary> An information text regarding point height. </summary>
        Property HeightInfo     As String
        
        ''' <summary> The point's kind or type. </summary>
        Property Kind()         As String
        
        ''' <summary> The type of marking used to realize the point. </summary>
        Property MarkType()     As String
        
        ''' <summary> Hints for marking, i.e. stability. </summary>
        Property MarkHints()    As String
        
        ''' <summary> A unique object or feature key. </summary>
        Property ObjectKey()    As String
        
        ''' <summary> A Job designation. </summary>
        Property Job()          As String
        
        ''' <summary> A time stamp designating the point's origin or last change. Defaults to time of object creation. </summary>
        Property TimeStamp()    As Nullable(Of DateTime)
        
    End Interface
    
    
    ''' <summary> A cartesian coordinates triple with specified reference systems and precisions. </summary>
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
        
        
        ''' <summary> Weight of position. </summary>
        Property wp()           As Double
        
        ''' <summary> Weight of height. </summary>
        Property wh()           As Double
        
        
        ''' <summary> Status of position. </summary>
        Property sp             As String
        
        ''' <summary> Status of height. </summary>
        Property sh             As String
        
        
        ''' <summary> Specifies the reference system of easting and northing coordinates. </summary>
        Property CoordSys()     As String
        
        ''' <summary> Specifies the reference system of height coordinate. </summary>
        Property HeightSys()    As String
        
    End Interface
    
    
    ''' <summary> Digital terrain model values at a given point. </summary>
    Public Interface IDTMatPoint
        
        ''' <summary> Height of the model surface (Z coordinate). </summary>
        Property ZDGM()         As Double
        
        ''' <summary> Height of the point above the model surface. </summary>
         ''' <remarks> This is the difference between Z coordinates of model and point. </remarks>
        Property HDGM()         As Double
        
        ''' <summary> Model name. </summary>
        Property NameOfDTM()    As String
        
    End Interface
    
    
    ''' <summary> Point values relative to a track geometry and also geometry design parameters. </summary>
    Public Interface IPointAtTrackGeometry
        
        ''' <summary> Station along the alignment. </summary>
        Property St()           As Kilometer
        
        ''' <summary> Kilometer along a separate kilometer leading alignment. </summary>
        Property Km()           As Kilometer
        
        ''' <summary> Perpendicular distance in XY-plane to the alignment. </summary>
        Property Q()            As Double
        
        ''' <summary> Perpendicular distance in XY-plane to the kilometer leading alignment. </summary>
        Property QKm()          As Double
        
        
        ''' <summary> Height above running surface of rail. </summary>
        Property HSOK()         As Double
        
        ''' <summary> Height above gradient design (vertical curve set). </summary>
        Property H()            As Double
        
        ''' <summary> Slope angle of gradient design (vertical curve set) in [rad]. </summary>
        Property G()            As Double
        
        ''' <summary> Height coordinate of running surface of rail. </summary>
        Property ZSOK()         As Double
        
        ''' <summary> Height coordinate of gradient design (vertical curve set). </summary>
        Property ZLGS()         As Double
        
        ''' <summary> Radius of gradient design element (vertical curve set). </summary>
        Property RaLGS()        As Double
        
        ' Tangentenabrückung/Abstichmaß der Gradiente (+ = Wanne, - = Kuppe)  of gradient design (vertical curve set).
        ''' <summary> Height gap between gradient tangent and running surface of rail (+ = valley, - = hill) of gradient design (vertical curve set). </summary>
        Property AbLGS()        As Double
        
        
        ''' <summary> Canted track coordinate system: Perpendicular distance in running surface of rail plane to the alignment. </summary>
        Property QG()           As Double
        
        ''' <summary> Canted track coordinate system: Height above running surface of rail. </summary>
        Property HG()           As Double
        
        ''' <summary> Length along the (canted) rail cross section shape. </summary>
        Property LG()           As Double
        
        ''' <summary> Radial distance to the (canted) rail cross section shape. </summary>
        Property RG()           As Double
        
        
        ''' <summary> Height above road cross section shape. </summary>
        Property V()            As Double
        
        ''' <summary> Road system: Perpendicular distance to the alignment. </summary>
        Property QGS()          As Double
        
        ''' <summary> Road system: Height. </summary>
        Property HGS()          As Double
        
        
        ''' <summary> Radial distance to the tunnel cross section shape. </summary>
        Property R()            As Double
        
        ''' <summary> Length along the tunnel cross section shape. </summary>
        Property L()            As Double
        
        ''' <summary> Tunnel Meters (Station inside a tunnel). </summary>
        Property TM()           As Double
        
        ''' <summary> Perpendicular distance in XY-plane to the alignment in direction of tunnel excavation. </summary>
        Property QT()           As Double
        
        ''' <summary> Tunnel system: Perpendicular distance to the alignment. </summary>
        Property QGT()          As Double
        
        ''' <summary> Tunnel system: Height. </summary>
        Property HGT()          As Double
        
        
        ''' <summary> Radius. </summary>
        Property Ra()           As Double
        
        ''' <summary> Azimuth of alignment tangent in [rad]. </summary>
        Property Ri()           As Double
        
        
        ''' <summary> Actual cant in [m]. </summary>
         ''' <remarks> Sign: Positive is treated as "normal", negative as "inverse". </remarks>
        Property ActualCant()   As Double
        
        ''' <summary> Cant (Superelevation) in [m]. </summary>
         ''' <remarks> Sign: Positive is treated as "normal", negative as "inverse". </remarks>
        Property Ueb()          As Double
        
        ''' <summary> Height difference between gradient design (vertical curve set) and running surface of rail (at special geometry) in [m]. </summary>
        Property Heb()          As Double     'Hebewert in der Schere [m]
        
        ''' <summary> The distance between rails for determining cant (used for calculations). </summary>
        Property CantBase()     As Double
        
        
        ''' <summary> Determines the reference frame of the track geometry. </summary>
        Property TrackRef()     As TrackGeometryInfo
        
    End Interface
    
    
    ''' <summary> Determines the source of a point in a file. </summary>
    Public Interface IFileSource
            
        ''' <summary> The path to the source file, this point has been read from. Defaults to <see langword="null"/>. </summary>
        Property SourcePath()   As String
        
        ''' <summary> Line number in source file, this point has been read from. Defaults to Zero. </summary>
         ''' <remarks> When the source record is multi-line, then this points to the first line. </remarks>
        Property SourceLineNo() As Long
        
    End Interface
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
