
' Samples from the demo program: DataContext, ElementName, RelativeSource and Source Bindings
'
' <TextBlock Name="myTextBlock"
'            Background="LightBlue"
'            TextWrapping="Wrap"
'            Text="This TextBlock will do OneWayToSource Bindings, using PushBindings, for the ReadOnly DP's ActualWidth and ActualHeight">
'     <pb:PushBindingManager.PushBindings>
'         <!-- DataContext PushBindings -->
'         <pb:PushBinding TargetProperty="ActualHeight" Path="Height"/>
'         <pb:PushBinding TargetProperty="ActualWidth" Path="Width"/>
'         <!-- RelativeSource, ElementName and Source PushBinding -->
'         <pb:PushBinding TargetProperty="ActualHeight"
'                         RelativeSource="{RelativeSource AncestorType={x:Type Window}}"
'                         Path="Title"/>
'         <pb:PushBinding TargetProperty="ActualHeight"
'                         ElementName="textBlock"
'                         Path="Text"/>
'         <pb:PushBinding TargetProperty="ActualHeight"
'                         Source="{StaticResource sourceTextBlock}"
'                         Path="Text"/>
'     </pb:PushBindingManager.PushBindings>
' </TextBlock>

Imports System.Windows
Imports System.Windows.Data

Namespace UI.Binding.PushBinding
    
    ''' <summary>  Part of PushBindingExtension, which allows to bind a read-only dependency property in mode "OneWayToSource" </summary>
     ''' <remarks> Found at http://meleak.wordpress.com/ (many thanks!) and converted to VisualBasic. </remarks>
    Public Class PushBinding
        Inherits FreezableBinding
        
        #Region "Dependency Properties"
            
            Public Shared TargetPropertyMirrorProperty As DependencyProperty = DependencyProperty.Register("TargetPropertyMirror", GetType(Object), GetType(PushBinding))
            Public Shared TargetPropertyListenerProperty As DependencyProperty = DependencyProperty.Register("TargetPropertyListener", GetType(Object), GetType(PushBinding), New UIPropertyMetadata(Nothing, AddressOf OnTargetPropertyListenerChanged))
            
            Private Shared Sub OnTargetPropertyListenerChanged(sender As Object, e As DependencyPropertyChangedEventArgs)
                Dim pushBinding As PushBinding = TryCast(sender, PushBinding)
                pushBinding.TargetPropertyValueChanged()
            End Sub
            
        #End Region
        
        #Region "Constructor"
            
            Public Sub New()
                Mode = BindingMode.OneWayToSource
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Public Property TargetPropertyMirror() As Object
                Get
                    Return GetValue(TargetPropertyMirrorProperty)
                End Get
                Set
                    SetValue(TargetPropertyMirrorProperty, value)
                End Set
            End Property
            Public Property TargetPropertyListener() As Object
                Get
                    Return GetValue(TargetPropertyListenerProperty)
                End Get
                Set
                    SetValue(TargetPropertyListenerProperty, value)
                End Set
            End Property
            
            '<DefaultValue(Nothing)> _
            Public Property TargetProperty() As String
                Get
                    Return m_TargetProperty
                End Get
                Set
                    m_TargetProperty = Value
                End Set
            End Property
            Private m_TargetProperty As String
            
        #End Region
        
        #Region "Methods"
            
            Public Sub SetupTargetBinding(targetObject As FrameworkElement)
                If targetObject Is Nothing Then
                    Return
                End If
                
                ' Prevent the designer from reporting exceptions since
                ' changes will be made of a Binding in use if it is set
                If System.ComponentModel.DesignerProperties.GetIsInDesignMode(Me) = True Then
                    Return
                End If
                
                ' Bind to the selected TargetProperty, e.g. ActualHeight and get
                ' notified about changes in OnTargetPropertyListenerChanged
                Dim listenerBinding As New Data.Binding() With { _
                    .Source = targetObject, _
                    .Path = New PropertyPath(TargetProperty), _
                    .Mode = BindingMode.OneWay _
                }
                BindingOperations.SetBinding(Me, TargetPropertyListenerProperty, listenerBinding)
                
                ' Set up a OneWayToSource Binding with the Binding declared in Xaml from
                ' the Mirror property of this class. The mirror property will be updated
                ' everytime the Listener property gets updated
                BindingOperations.SetBinding(Me, TargetPropertyMirrorProperty, Binding)
                TargetPropertyValueChanged()
            End Sub
            
            Private Sub TargetPropertyValueChanged()
                Dim targetPropertyValue As Object = GetValue(TargetPropertyListenerProperty)
                Me.SetValue(TargetPropertyMirrorProperty, targetPropertyValue)
            End Sub
            
        #End Region
        
		#Region "Freezable overrides"
            
		    Protected Overrides Sub CloneCore(sourceFreezable As Freezable)
		    	Dim pushBinding As PushBinding = TryCast(sourceFreezable, PushBinding)
		    	TargetProperty = pushBinding.TargetProperty
		    	'TargetDependencyProperty = pushBinding.TargetDependencyProperty
		    	MyBase.CloneCore(sourceFreezable)
		    End Sub
            
		    Protected Overrides Function CreateInstanceCore() As Freezable
		    	Return New PushBinding()
		    End Function
		    
		#End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
