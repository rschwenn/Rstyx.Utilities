
Imports System
'Imports System.Collections.ObjectModel

Namespace Domain.ClearanceGauge
    
    ''' <summary> Minimum clearance outline for DBAG railway track. </summary>
    Public Class MinimumClearanceDBAG
        
        ''' <summary> Kilometer of object. </summary>
        Public Property Kilometer       As Kilometer = New Kilometer()
        
        ''' <summary> Railway track number. </summary>
        Public Property TrackNo         As Nullable(Of Integer) = Nothing
        
        ''' <summary> Railway track zone (or segment or section). </summary>
        Public Property TrackZone       As String = String.Empty
        
        ' ''' <inheritdoc/>
        ' Public Overrides Function ToString() As String
        '     Dim PositionString As New Collection(Of String)
        '     If (Me.TrackNo.HasValue)                     Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_Track     & ": " & Me.TrackNo)
        '     If (Me.Kilometer.HasValue())                 Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_Kilometer & ": " & Me.Kilometer.ToString())
        '     If (Me.RailsNameNo.IsNotEmptyOrWhiteSpace()) Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_RailsName & ": " & Me.RailsNameNo)
        '     If (Me.RailsCode.IsNotEmptyOrWhiteSpace())   Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_RailsCode & "="  & Me.RailsCode)
        '     If (Me.Side.IsNotEmptyOrWhiteSpace())        Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_Side      & ": " & Me.Side)
        '     If (PositionString.Count = 0)                Then PositionString.Add(Rstyx.Utilities.Resources.Messages.MinimumClearanceDBAG_Label_UnKnownPosition)
        '     Return PositionString.JoinIgnoreEmpty(", ")
        ' End Function
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
