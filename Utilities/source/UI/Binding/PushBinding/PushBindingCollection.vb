
Imports System.Windows
Imports System.Collections.Specialized

Namespace UI.Binding.PushBinding
    
    ''' <summary>  Part of PushBindingExtension, which allows to bind a read-only dependency property in mode "OneWayToSource" </summary>
     ''' <remarks> Found at http://meleak.wordpress.com/ (many thanks!) and converted to VisualBasic. </remarks>
    Public Class PushBindingCollection
        Inherits FreezableCollection(Of PushBinding)
        
        Private m_TargetObject As FrameworkElement
        
        Public Sub New(target As FrameworkElement)
            m_TargetObject = target
            AddHandler DirectCast(Me, INotifyCollectionChanged).CollectionChanged, AddressOf OnCollectionChanged
        End Sub
        
        Private Sub OnCollectionChanged(sender As Object, e As NotifyCollectionChangedEventArgs)
            If (e.Action = NotifyCollectionChangedAction.Add) Then
                For Each pushBinding As PushBinding In e.NewItems
                    pushBinding.SetupTargetBinding(TargetObject)
                Next
            End If
        End Sub
        
        Public ReadOnly Property TargetObject() As FrameworkElement
            Get
                Return m_TargetObject
            End Get
        End Property
        
        'Public Shadows Sub Add(value As PushBinding)
        '	MyBase.Add(value)
        '	value.SetupTargetBinding(TargetObject)
        'End Sub
        
        'Protected Overrides Function CreateInstanceCore() As Freezable
        '    Return New FreezableCollection(Of PushBinding)()
        'End Function
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
