
Namespace Domain
    
    ''' <summary> Position description related to a railway track. </summary>
    Public Class PositionAtTrack
        
        ''' <summary> Kilometer of object. </summary>
        Public Property Kilometer           As Kilometer = New Kilometer()
        
        ''' <summary> Railway track title. </summary>
        Public Property TrackTitle          As TrackTitle = New TrackTitle()
        
        ''' <summary> Railway track zone (or segment or section). </summary>
        Public Property TrackZone           As String = String.Empty
        
        ''' <summary> Code of track rails (right, left, single). </summary>
        Public Property RailsCode           As String = String.Empty
        
        ''' <summary> Name or umber of track rails (at a railway mstation). </summary>
        Public Property RailsNameNo         As String = String.Empty
                
         ''' <inheritdoc/>
        Public Overrides Function ToString() As String
            Return Rstyx.Utilities.StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.PositionAtTrack_ToString, Me.TrackTitle.Number, Me.Kilometer.ToString(), Me.RailsNameNo, Me.RailsCode)
        End Function
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
