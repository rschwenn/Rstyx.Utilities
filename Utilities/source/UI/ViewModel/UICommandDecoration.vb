
Imports System.Windows.Media.Imaging

Namespace UI.ViewModel
    
    ''' <summary> Decoration for a Command used in UI elements. </summary>
    Public Structure UICommandDecoration
        
        Private _Caption        As String
        Private _Description    As String
        
        
        ''' <summary> A bitmap Icon for the command, i.e. to display on a button. </summary>
        Public Property IconBitmap                 As BitmapSource
        
        ''' <summary> A vector brush for drawing an Icon for the command, i.e. to display on a button. </summary>
        Public Property IconBrush                  As System.Windows.Media.DrawingBrush
        
        ''' <summary> A vector Icon for the command, i.e. to display on a button. </summary>
        ''' <remarks> Deprecated: The rectangle isn't a freezable and can be used only once! </remarks>
        Public Property IconRectangle              As System.Windows.Shapes.Rectangle
        
        ''' <summary> A Name of a String Resource to retrieve the Caption. </summary>
        Public Property CaptionResourceName        As String
        
        ''' <summary> A Name of a String Resource to retrieve the Description. </summary>
        Public Property DescriptionResourceName    As String
        
        
        ''' <summary> A Caption for the command, i.e. to display on a button. </summary>
         ''' <remarks> If <see cref="CaptionResourceName" /> is not empty, then the Caption is read from resources. </remarks>
        Public Property Caption() As String
            Get
                If (Not String.IsNullOrEmpty(Me.CaptionResourceName)) Then
                    Caption = My.Resources.Resources.ResourceManager.GetString(Me.CaptionResourceName, My.Resources.Resources.Culture)
                Else
                    Caption = _Caption
                End if
            End Get
            Set(value As String)
                _Caption = value
            End Set
        End Property
        
        ''' <summary> A Description for the command, i.e. to display as tooltip. </summary>
         ''' <remarks> If <see cref="DescriptionResourceName" /> is not empty, then the Description is read from resources. </remarks>
        Public Property Description() As String
            Get
                If (Not String.IsNullOrEmpty(Me.DescriptionResourceName)) Then
                    Description = My.Resources.Resources.ResourceManager.GetString(Me.DescriptionResourceName, My.Resources.Resources.Culture)
                Else
                    Description = _Description
                End if
            End Get
            Set(value As String)
                _Description = value
            End Set
        End Property
        
    End Structure
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
