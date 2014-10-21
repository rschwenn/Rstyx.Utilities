
Imports System
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> Representation of a geodetic point including some point info. </summary>
     ''' <remarks></remarks>
    Public Class GeoPoint
        Implements IGeoPoint
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPoint")
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new GeoPoint. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new GeoPoint and inititializes it's properties from any given <see cref="GeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                
                If (SourcePoint IsNot Nothing) Then
                    Me.ID              = SourcePoint.ID
                    
                    Me.Info            = SourcePoint.Info
                    Me.HeightInfo      = SourcePoint.HeightInfo
                    Me.Comment         = SourcePoint.Comment
                    Me.Kind            = SourcePoint.Kind
                    Me.MarkType        = SourcePoint.MarkType
                    Me.MarkHints       = SourcePoint.MarkHints
                    Me.ObjectKey       = SourcePoint.ObjectKey
                    Me.Job             = SourcePoint.Job
                    Me.TimeStamp       = SourcePoint.TimeStamp
                    
                    Me.X               = SourcePoint.X
                    Me.Y               = SourcePoint.Y
                    Me.Z               = SourcePoint.Z
                    Me.mp              = SourcePoint.mp
                    Me.mh              = SourcePoint.mh
                    Me.wp              = SourcePoint.wp
                    Me.wh              = SourcePoint.wh
                    Me.sp              = SourcePoint.sp
                    Me.sh              = SourcePoint.sh
                    Me.CoordSys        = SourcePoint.CoordSys
                    Me.HeightSys       = SourcePoint.HeightSys
                    
                    Me.SourceFileIndex = SourcePoint.SourceFileIndex
                    Me.SourceLineNo    = SourcePoint.SourceLineNo
                End If
            End Sub
            
        #End Region
        
        #Region "IIdentifiable Members"
            
            Dim _ID As String = Nothing
            
            ''' <summary> The point ID (It will be verified while setting it). </summary>
             ''' <remarks> The setter fails if given ID isn't valid for this point type. </remarks>
             ''' <exception cref="ParseException"> <paramref name="TargetID"/> isn't a valid ID for this point (The <see cref="ParseError"/> only contains a message.). </exception>
            Public Property ID() As String Implements IIdentifiable(Of String).ID
            Get
                Return _ID
            End Get
            Set(value As String)
                _ID = Me.ParseID(value)
            End Set
        End Property
            
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
        
        #Region "ID Validation"
            
            ''' <summary> Parses the given string as new <see cref="GeoPoint.ID"/> for this GeoPoint. </summary>
             ''' <param name="TargetID"> The string which is intended to become the new point ID. It will be trimmed here. </param>
             ''' <returns> The parsed ID. </returns>
             ''' <remarks>
             ''' <para>
             ''' This method will be called when setting the <see cref="GeoPoint.ID"/> property.
             ''' It allows for special format conversions and for validation of <paramref name="TargetID"/>.
             ''' </para>
             ''' <para>
             ''' For the base class <see cref="GeoPoint"/>, this method returns the trimmed <paramref name="TargetID"/>
             ''' and validation only complains about the ID, if it is <see langword="null"/>, empty or contains whitespace only.
             ''' Derived classes should override this behavior to implement more restrictive constraints.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> isn't a valid ID for this point. </exception>
            Protected Overridable Function ParseID(TargetID As String) As String
                
                If (TargetID.IsEmptyOrWhiteSpace()) Then
                    Throw New InvalidIDException(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPointID)
                End If
                
                Return TargetID.Trim()
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns point ID and info. </summary>
            Public Overrides Function ToString() As String
                Return Me.ID & "  (" & Me.Info & ")"
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
