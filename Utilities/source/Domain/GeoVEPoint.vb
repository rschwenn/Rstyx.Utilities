
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> One single VE point. </summary>
    Public Class GeoVEPoint
        Inherits GeoPoint(Of Double)
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty VEPoint. </summary>
            Public Sub New()
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> General position description related to a railway track. </summary>
            Public Property TrackPos()          As PositionAtTrack = New PositionAtTrack()
            
            ''' <summary> Pre character of position info. </summary>
            Public Property PositionPreInfo     As Char
            
            ''' <summary> Post character of position info. </summary>
            Public Property PositionPostInfo    As Char
            
            ''' <summary> Pre character of height info. </summary>
            Public Property HeightPreInfo       As Char
            
            ''' <summary> Post character of height info. </summary>
            Public Property HeightPostInfo      As Char
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns a KV output of the point. </summary>
            Public Overrides Function ToString() As String
                
                Dim KvFmt As String = "%7.0f %15.5f%15.5f%10.4f %12.4f  %-13s %-13s %-4s %4d %1s  %3s %5.0f %5.0f  %1s %+3s  %1s%1s  %-8s %7s"
                
                Return sprintf(KvFmt, Me.ID * 100000, Me.Y, Me.X, Me.Z, Me.TrackPos.Kilometer.Value, Me.Info.TrimToMaxLength(13), Me.HeightInfo.TrimToMaxLength(13),
                               Me.Kind.TrimToMaxLength(4), Me.TrackPos.TrackNo, Me.TrackPos.RailsCode.TrimToMaxLength(1), Me.HeightSys.TrimToMaxLength(3), Me.mp, Me.mh, 
                               Me.MarkHints.TrimToMaxLength(1), Me.MarkType.TrimToMaxLength(3), Me.sp.TrimToMaxLength(1), Me.sh.TrimToMaxLength(1),
                               Me.Job.TrimToMaxLength(8), Me.ObjectKey.TrimToMaxLength(7))
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
