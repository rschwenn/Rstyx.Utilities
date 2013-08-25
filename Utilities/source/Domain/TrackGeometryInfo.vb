
Namespace Domain
    
    ''' <summary> Info record that identifies a track geometry. </summary>
    ''' <remarks> To be valid, the <see cref="P:NameOfAlignment"/> property must not be empty. </remarks>
    Public Class TrackGeometryInfo
        Inherits Cinch.ValidatingObject
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.TrackGeometryInfo")
            
            Private Shared MissingNameOfAlignmentRule As Cinch.SimpleRule
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Static initializations. </summary>
            Shared Sub New()
                ' Create Validation Rules.
                
                MissingNameOfAlignmentRule = New Cinch.SimpleRule("NameOfAlignment",
                                                                  Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_MissingNameOfAlignment,
                                                                  Function (oValidatingObject As Object) (DirectCast(oValidatingObject, TrackGeometryInfo).NameOfAlignment.IsEmptyOrWhiteSpace()))
                '
            End Sub
            
            ''' <summary> Creates a new TrackGeometryInfo. </summary>
            Public Sub New()
                Me.AddRule(MissingNameOfAlignmentRule)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Name of alignment (horizontal curve set). </summary>
            Public Property NameOfAlignment()       As String = String.Empty
            
            ''' <summary> Name of kilometer leading alignment (horizontal curve set). </summary>
            Public Property NameOfKmAlignment()     As String = String.Empty
            
            ''' <summary> Name of gradient line (vertical curve set). </summary>
            Public Property NameOfGradientLine()    As String = String.Empty
            
            ''' <summary> Name of cant line (Cant model for the whole alignment). </summary>
            Public Property NameOfCantLine()        As String = String.Empty
            
            ''' <summary> Name of line defining names and scopes of road scross sections. </summary>
            Public Property NameOfRoadSections()    As String = String.Empty
            
            ''' <summary> Name of line defining names and scopes of tunnel scross sections. </summary>
            Public Property NameOfTunnelSections()  As String = String.Empty
            
        #End Region
        
        #Region "Overrides"
            
            ''' <inheritdoc/>
            Public Overrides Function ToString() As String
                Dim List As New System.Text.StringBuilder()
                
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfAlignment     , Me.NameOfAlignment))
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfKmAlignment   , Me.NameOfKmAlignment))
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfGradientLine  , Me.NameOfGradientLine))
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfCantLine      , Me.NameOfCantLine))
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfRoadSections  , Me.NameOfRoadSections))
                List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfTunnelSections, Me.NameOfTunnelSections))
                
                Return List.ToString()
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
