
Imports System.Windows

Namespace UI.Binding.PushBinding
    
    ''' <summary>  Part of PushBindingExtension, which allows to bind a read-only dependency property in mode "OneWayToSource" </summary>
     ''' <remarks> Found at http://meleak.wordpress.com/ (many thanks!) and converted to VisualBasic. </remarks>
    Public Class PushBindingManager
        
        Public Shared PushBindingsProperty As DependencyProperty =
            DependencyProperty.RegisterAttached("PushBindingsInternal",
                                                GetType(PushBindingCollection),
                                                GetType(PushBindingManager),
                                                New UIPropertyMetadata(Nothing)
                                                )
        '
        Public Shared Function GetPushBindings(obj As FrameworkElement) As PushBindingCollection
            If (obj.GetValue(PushBindingsProperty) Is Nothing) Then
                obj.SetValue(PushBindingsProperty, New PushBindingCollection(obj))
            End If
            Dim RetValue As PushBindingCollection = DirectCast(obj.GetValue(PushBindingsProperty), PushBindingCollection)
            Return RetValue
        End Function
        
        Public Shared Sub SetPushBindings(obj As FrameworkElement, value As PushBindingCollection)
            obj.SetValue(PushBindingsProperty, value)
        End Sub
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
