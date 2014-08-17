
Imports System.Collections.ObjectModel

Namespace Domain
    
    ''' <summary> Position description related to a railway track. </summary>
    Public Class PositionAtTrack
        
        ''' <summary> Kilometer of object. </summary>
        Public Property Kilometer       As Kilometer = New Kilometer()
        
        ''' <summary> Railway track title. </summary>
        Public Property TrackTitle      As TrackTitle = New TrackTitle()
        
        ''' <summary> Railway track zone (or segment or section). </summary>
        Public Property TrackZone       As String = String.Empty
        
        ''' <summary> Code of track rails (right, left, single). </summary>
        Public Property RailsCode       As String = String.Empty
        
        ''' <summary> Name or umber of track rails (at a railway mstation). </summary>
        Public Property RailsNameNo     As String = String.Empty
                
         ''' <inheritdoc/>
        Public Overrides Function ToString() As String
            Dim PositionString As New Collection(Of String)
            If (Not (Me.TrackTitle.Number = 0))          Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_Track     & ": " & Me.TrackTitle.Number)
            If (Me.Kilometer.HasValue())                 Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_Kilometer & ": " & Me.Kilometer.ToString())
            If (Me.RailsNameNo.IsNotEmptyOrWhiteSpace()) Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_RailsName & ": " & Me.RailsNameNo)
            If (Me.RailsCode.IsNotEmptyOrWhiteSpace())   Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_RailsCode & "="  & Me.RailsCode)
            If (PositionString.Count = 0)                Then PositionString.Add(Rstyx.Utilities.Resources.Messages.PositionAtTrack_Label_UnKnownPosition)
            Return PositionString.JoinIgnoreEmpty(", ")
        End Function
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
