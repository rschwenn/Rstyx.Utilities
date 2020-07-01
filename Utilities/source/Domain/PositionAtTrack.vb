
Imports System
Imports System.Collections.ObjectModel

Namespace Domain
    
    ''' <summary> Specification of track side. </summary>
    Public Enum TrackSide As Integer
        
        ''' <summary> Side isn't determined. </summary>
        None  = 0
        
        ''' <summary> Right side of track. </summary>
        Right = 1
        
        ''' <summary> Left side of track. </summary>
        Left  = 2
        
        ''' <summary> Right and left side of track. </summary>
        Both  = 3
        
    End Enum
    
    ''' <summary> General position description related to a railway track. </summary>
    Public Class PositionAtTrack
        
        ''' <summary> Kilometer of object. </summary>
        Public Property Kilometer       As Kilometer = New Kilometer()
        
        ''' <summary> Railway track number. </summary>
        Public Property TrackNo         As Nullable(Of Integer) = Nothing
        
        ''' <summary> Railway track zone (or segment or section). </summary>
        Public Property TrackZone       As String = String.Empty
        
        ''' <summary> Code of track rails (right, left, single). </summary>
        Public Property RailsCode       As String = String.Empty
        
        ''' <summary> Name or number of track rails (at a railway station). </summary>
        Public Property RailsNameNo     As String = String.Empty
        
        ''' <summary> Specification of track side. </summary>
        Public Property Side            As TrackSide = TrackSide.None
                
         ''' <inheritdoc/>
        Public Overrides Function ToString() As String
            Dim PositionString As New Collection(Of String)
            If (Me.TrackNo.HasValue)                     Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_Track     & ": " & Me.TrackNo)
            If (Me.Kilometer.HasValue())                 Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_Kilometer & ": " & Me.Kilometer.ToString())
            If (Me.RailsNameNo.IsNotEmptyOrWhiteSpace()) Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_RailsName & ": " & Me.RailsNameNo)
            If (Me.RailsCode.IsNotEmptyOrWhiteSpace())   Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_RailsCode & "="  & Me.RailsCode)
            If (Me.Side <> TrackSide.None)               Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_Side      & ": " & Me.Side.ToDisplayString())
            If (PositionString.Count = 0)                Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_UnKnownPosition)
            Return PositionString.JoinIgnoreEmpty(", ")
        End Function
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
