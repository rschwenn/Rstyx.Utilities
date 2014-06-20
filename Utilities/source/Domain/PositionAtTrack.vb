
Namespace Domain
    
    ''' <summary> Position description related to a railway track. </summary>
    Public Class PositionAtTrack
        
        ''' <summary> Kilometer of object. </summary>
        Public Property Kilometer           As Kilometer = New Kilometer()
        
        '' <summary> Railway track number or similar code. </summary>
        'Public Property TrackNo             As String = String.Empty
        
        ''' <summary> Railway track title. </summary>
        Public Property TrackTitle          As TrackTitle = New TrackTitle()
        
        ''' <summary> Railway track zone (or segment or section). </summary>
        Public Property TrackZone           As String = String.Empty
        
        ''' <summary> Code of track rails (right, left, single). </summary>
        Public Property RailsCode           As String = String.Empty
        
        ''' <summary> Number of track rails (at a railway mstation). </summary>
        Public Property RailsNo             As String = String.Empty
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
