
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls

Namespace UI.Controls
    
    ' <remarks>
    ' <para>
    ' The <see cref="LooseExpanderToggle"/> is a <see cref="System.Windows.Controls.Primitives.ToggleButton"/> containing a small triangle,
    ' that toggles the <see cref="UIElement.Visibility"/> of the given <see cref="P:Target"/> Element.
    ' </para>
    ' <para>
    ' The visual style is like a <see cref="System.Windows.Controls.Primitives.ToggleButton"/> on a <see cref="System.Windows.Controls.ToolBar"/>.
    ' The content is a usual small triangle whose null direction is determined by the <see cref="P:ExpandDirection"/> property.
    ' The content determined by the <see cref="P:Content"/> property is placed beside the triangle.
    ' </para>
    ' <para>
    ' </para>
    ' </remarks>
    Public Class LooseExpanderToggle
        Inherits System.Windows.Controls.Primitives.ToggleButton
        
        #Region "Dependency Properties"
            
            #Region "Target"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.LooseExpanderToggle.Target Dependency Property. </summary>
                Public Shared ReadOnly TargetProperty As DependencyProperty =
                    DependencyProperty.Register("Target",
                                                GetType(UIElement),
                                                GetType(LooseExpanderToggle),
                                                New FrameworkPropertyMetadata(Nothing)
                                                )
                                                'New FrameworkPropertyMetadata(Nothing, New PropertyChangedCallback(AddressOf OnTargetChanged))
                                                'New FrameworkPropertyMetadata(Nothing, AddressOf OnTargetChanged)
                '
                'Private Shared Sub OnTargetChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
                'End Sub
                
                ''' <summary> Determines the UIElement whose Visibility can be toggled by this <c>LooseExpanderToggle</c>. Defaults to <c>Null</c>. </summary>
		        <System.ComponentModel.TypeConverter(GetType(System.Windows.Markup.NameReferenceConverter))>
		        Public Property Target() As UIElement
		        	Get
		        		Return CType(GetValue(TargetProperty), UIElement)
		        	End Get
		        	Set(value As UIElement)
		        		SetValue(TargetProperty, value)
		        	End Set
		        End Property
                
            #End Region
            
            #Region "ExpandDirection"
                
                ''' <summary> Designates the Rstyx.Utilities.UI.Controls.LooseExpanderToggle.ExpandDirection Dependency Property. </summary>
                Public Shared ReadOnly ExpandDirectionProperty As DependencyProperty =
                    DependencyProperty.Register("ExpandDirection",
                                                GetType(ExpandDirection),
                                                GetType(LooseExpanderToggle),
                                                New FrameworkPropertyMetadata(ExpandDirection.Down, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault),
                                                AddressOf IsValidExpandDirection
                                                )
                '
		        Private Shared Function IsValidExpandDirection(o As Object) As Boolean
		        	Dim ExpDir As ExpandDirection = CType(o, ExpandDirection)
		        	Return ( (ExpDir = ExpandDirection.Down) OrElse (ExpDir = ExpandDirection.Left) OrElse (ExpDir = ExpandDirection.Right) OrElse (ExpDir = ExpandDirection.Up) )
		        End Function
                
                ''' <summary> Determines the direction of the toggle symbol (triangle) when <see cref="P:IsChecked"/> (alias "IsExpanded") property is <c>False</c>. Defaults to <c>Down</c>. </summary>
		        <Bindable(True), Category("Behavior")>
		        Public Property ExpandDirection() As ExpandDirection
		        	Get
		        		Return CType(GetValue(ExpandDirectionProperty), ExpandDirection)
		        	End Get
		        	Set(value As ExpandDirection)
		        		SetValue(ExpandDirectionProperty, value)
		        	End Set
		        End Property
                
            #End Region
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
