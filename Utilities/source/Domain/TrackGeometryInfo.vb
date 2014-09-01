
Namespace Domain
    
    ''' <summary> Info record that identifies a track geometry. </summary>
    ''' <remarks> To be valid, the <see cref="TrackGeometryInfo.NameOfAlignment"/> property must not be empty. </remarks>
    Public Class TrackGeometryInfo
        Inherits Cinch.ValidatingObject
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.TrackGeometryInfo")
            
            Private Shared MissingNameOfAlignmentOrDTMRule As Cinch.SimpleRule
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Static initializations. </summary>
            Shared Sub New()
                ' Create Validation Rules.
                
                MissingNameOfAlignmentOrDTMRule = New Cinch.SimpleRule("NameOfAlignmentOrDTM",
                                                          Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_MissingNameOfAlignmentOrDTM,
                                                          Function (oValidatingObject As Object) (DirectCast(oValidatingObject, TrackGeometryInfo).NameOfAlignment.IsEmptyOrWhiteSpace() AndAlso
                                                                                                  DirectCast(oValidatingObject, TrackGeometryInfo).NameOfDTM.IsEmptyOrWhiteSpace() ))
                '
            End Sub
            
            ''' <summary> Creates a new TrackGeometryInfo. </summary>
            Public Sub New()
                Me.AddRule(MissingNameOfAlignmentOrDTMRule)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Specifies the reference system of easting and northing coordinates. </summary>
            Public Property CoordSys()              As String = String.Empty
            
            ''' <summary> Specifies the reference system of height coordinate. </summary>
            Public Property HeightSys()             As String = String.Empty
            
            ''' <summary> Name of alignment (horizontal curve set). </summary>
            Public Property NameOfAlignment()       As String = String.Empty
            
            ''' <summary> Name of kilometer leading alignment (horizontal curve set). </summary>
            Public Property NameOfKmAlignment()     As String = String.Empty
            
            ''' <summary> Name of gradient line (vertical curve set). </summary>
            Public Property NameOfGradientLine()    As String = String.Empty
            
            ''' <summary> Name of cant line (Cant model for the whole alignment). </summary>
            Public Property NameOfCantLine()        As String = String.Empty
            
            ''' <summary> Name of line defining names and scopes of road cross sections. </summary>
            Public Property NameOfRoadSections()    As String = String.Empty
            
            ''' <summary> Name of line defining names and scopes of tunnel cross sections. </summary>
            Public Property NameOfTunnelSections()  As String = String.Empty
            
            ''' <summary> Name of line defining names and scopes of rail cross sections. </summary>
             ''' <remarks> A rail cross section is defined in canted track coordinate system. </remarks>
            Public Property NameOfRailSections()    As String = String.Empty
            
            ''' <summary> Name of line defining names and scopes of section related points. </summary>
            Public Property NameOfSectionPoints()   As String = String.Empty
            
            ''' <summary> Name of Digital Terrain Model. </summary>
            Public Property NameOfDTM()             As String = String.Empty
            
        #End Region
        
        #Region "Overrides"
            
            ''' <inheritdoc/>
            Public Overrides Function ToString() As String
                Dim List As New System.Text.StringBuilder()
                Dim RetValue As String
                
                If (Not Me.IsValid) Then 
                    RetValue = MyBase.ToString()
                Else
                    ' At least one name is not empty.
                    If (Me.CoordSys.IsNotEmptyOrWhiteSpace())             Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_CoordSys            , Me.CoordSys))
                    If (Me.HeightSys.IsNotEmptyOrWhiteSpace())            Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_HeightSys           , Me.HeightSys))
                    If (Me.NameOfAlignment.IsNotEmptyOrWhiteSpace())      Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfAlignment     , Me.NameOfAlignment))
                    If (Me.NameOfKmAlignment.IsNotEmptyOrWhiteSpace())    Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfKmAlignment   , Me.NameOfKmAlignment))
                    If (Me.NameOfGradientLine.IsNotEmptyOrWhiteSpace())   Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfGradientLine  , Me.NameOfGradientLine))
                    If (Me.NameOfCantLine.IsNotEmptyOrWhiteSpace())       Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfCantLine      , Me.NameOfCantLine))
                    If (Me.NameOfRoadSections.IsNotEmptyOrWhiteSpace())   Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfRoadSections  , Me.NameOfRoadSections))
                    If (Me.NameOfTunnelSections.IsNotEmptyOrWhiteSpace()) Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfTunnelSections, Me.NameOfTunnelSections))
                    If (Me.NameOfRailSections.IsNotEmptyOrWhiteSpace())   Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfRaillSections , Me.NameOfRailSections))
                    If (Me.NameOfSectionPoints.IsNotEmptyOrWhiteSpace())  Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfSectionPoints , Me.NameOfSectionPoints))
                    If (Me.NameOfDTM.IsNotEmptyOrWhiteSpace())            Then List.AppendLine(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.TrackGeometryInfo_NameOfDTM           , Me.NameOfDTM))
                    
                    RetValue = List.ToString()
                    If (RetValue.EndsWith(vbNewLine)) Then
                        RetValue = RetValue.Left(RetValue.Length - vbNewLine.Length)
                    End If
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
