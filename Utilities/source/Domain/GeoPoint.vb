
Imports System

Namespace Domain
    
    ''' <summary> Shortcut for a <see cref="GeoPoint(Of String)"/>, representing the most usual case: a string identifier. </summary>
    Public Class GeoPoint
        Inherits GeoPoint(Of String)
    End Class
    
    ''' <summary> Representation of a geodetic point including some point info. </summary>
     ''' <typeparam name="TPointID"> Type of the Point ID. </typeparam>
     ''' <remarks></remarks>
    Public Class GeoPoint(Of TPointID)
        Implements IIdentifiable(Of TPointID)
        Implements IGeoPointInfo
        Implements ICartesianCoordinates3D
        Implements IFileSource
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPoint")
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new GeoPoint. </summary>
            Public Sub New()
            End Sub
            
        #End Region
        
        #Region "IIdentifiable Members"
            
            ''' <inheritdoc/>
            Public Property ID()    As TPointID = Nothing Implements IIdentifiable(Of TPointID).ID
            
        #End Region
        
        #Region "IGeoPointInfo Members"
            
            ''' <inheritdoc/>
            Public Property Info()          As String = String.Empty Implements IGeoPointInfo.Info
            
            ''' <inheritdoc/>
            Public Property HeightInfo()    As String = String.Empty Implements IGeoPointInfo.HeightInfo
            
            ''' <inheritdoc/>
            Public Property Comment()       As String = String.Empty Implements IGeoPointInfo.Comment
            
            ''' <inheritdoc/>
            Public Property Kind()          As String = String.Empty Implements IGeoPointInfo.Kind
            
            ''' <inheritdoc/>
            Public Property MarkType()      As String = String.Empty Implements IGeoPointInfo.MarkType
            
            ''' <inheritdoc/>
            Public Property MarkHints()     As String = String.Empty Implements IGeoPointInfo.MarkHints
            
            ''' <inheritdoc/>
            Public Property ObjectKey()     As String = String.Empty Implements IGeoPointInfo.ObjectKey
            
            ''' <inheritdoc/>
            Public Property Job()           As String = String.Empty Implements IGeoPointInfo.Job
            
            ''' <inheritdoc/>
            Public Property TimeStamp       As Nullable(Of DateTime) = Nothing Implements IGeoPointInfo.TimeStamp
            
        #End Region
        
        #Region "ICartesianCoordinates3D Members"
            
            ''' <inheritdoc/>
            Public Property X()             As Double = Double.NaN   Implements ICartesianCoordinates3D.X
            
            ''' <inheritdoc/>
            Public Property Y()             As Double = Double.NaN   Implements ICartesianCoordinates3D.Y
            
            ''' <inheritdoc/>
            Public Property Z()             As Double = Double.NaN   Implements ICartesianCoordinates3D.Z
            
            ''' <inheritdoc/>
            Public Property mp()            As Double = Double.NaN   Implements ICartesianCoordinates3D.mp
            
            ''' <inheritdoc/>
            Public Property mh()            As Double = Double.NaN   Implements ICartesianCoordinates3D.mh
            
            ''' <inheritdoc/>
            Public Property wp()            As Double = Double.NaN   Implements ICartesianCoordinates3D.wp
            
            ''' <inheritdoc/>
            Public Property wh()            As Double = Double.NaN   Implements ICartesianCoordinates3D.wh
            
            ''' <inheritdoc/>
            Public Property sp()            As String = String.Empty Implements ICartesianCoordinates3D.sp
            
            ''' <inheritdoc/>
            Public Property sh()            As String = String.Empty Implements ICartesianCoordinates3D.sh
            
            ''' <inheritdoc/>
            Public Property CoordSys()      As String = String.Empty Implements ICartesianCoordinates3D.CoordSys
            
            ''' <inheritdoc/>
            Public Property HeightSys()     As String = String.Empty Implements ICartesianCoordinates3D.HeightSys
            
        #End Region
        
        #Region "IFileSource Members"
            
            ''' <inheritdoc/>
            Public Property SourceFileIndex()   As Long = -1 Implements IFileSource.SourceFileIndex
            
            ''' <inheritdoc/>
            Public Property SourceLineNo()      As Long = 0 Implements IFileSource.SourceLineNo
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns point ID and info. </summary>
            Public Overrides Function ToString() As String
                Return Me.ID.ToString() & "  (" & Me.Info & ")"
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
