
Imports System.Windows
Imports System.Windows.Threading
Imports System.Windows.Media.Animation
Imports System.Diagnostics
Imports System.Windows.Markup

''' <summary> Contains attached properties to activate Trigger Tracing on the specified Triggers. </summary>
 ''' <remarks> See http://www.wpfmentor.com/2009/01/how-to-debug-triggers-using-trigger.html for details and sample app. </remarks>
Public NotInheritable Class TriggerTracing
    
    #Region "Constructors"
        
        Private Sub New()
        End Sub
        
        Shared Sub New()
            ' Initialize WPF Animation tracing and add a TriggerTraceListener
            PresentationTraceSources.Refresh()
            PresentationTraceSources.AnimationSource.Listeners.Clear()
            PresentationTraceSources.AnimationSource.Listeners.Add(New TriggerTraceListener())
            PresentationTraceSources.AnimationSource.Switch.Level = SourceLevels.All
        End Sub
        
    #End Region
    
    #Region "TriggerName attached property"
        
        ''' <summary> Gets the trigger name for the specified trigger. This will be used to identify the trigger in the debug output. </summary>
         ''' <param name="trigger"> The trigger. </param>
        Public Shared Function GetTriggerName(trigger As TriggerBase) As String
            Return DirectCast(trigger.GetValue(TriggerNameProperty), String)
        End Function
        
        ''' <summary> Sets the trigger name for the specified trigger. This will be used to identify the trigger in the debug output. </summary>
        ''' <param name="trigger"> The trigger. </param>
        Public Shared Sub SetTriggerName(trigger As TriggerBase, value As String)
            trigger.SetValue(TriggerNameProperty, value)
        End Sub
        
        Public Shared ReadOnly TriggerNameProperty As DependencyProperty = DependencyProperty.RegisterAttached("TriggerName", GetType(String), GetType(TriggerTracing), New UIPropertyMetadata(String.Empty))
        
    #End Region
    
    #Region "TraceEnabled attached property"
        
        ''' <summary> Gets a value indication whether trace is enabled for the specified trigger. </summary>
         ''' <param name="trigger">The trigger.</param>
        Public Shared Function GetTraceEnabled(trigger As TriggerBase) As Boolean
            Return CBool(trigger.GetValue(TraceEnabledProperty))
        End Function
        
        ''' <summary> Sets a value specifying whether trace is enabled for the specified trigger </summary>
         ''' <param name="trigger"></param>
         ''' <param name="value"></param>
        Public Shared Sub SetTraceEnabled(trigger As TriggerBase, value As Boolean)
            trigger.SetValue(TraceEnabledProperty, value)
        End Sub
        
        Public Shared ReadOnly TraceEnabledProperty As DependencyProperty = DependencyProperty.RegisterAttached("TraceEnabled", GetType(Boolean), GetType(TriggerTracing), New UIPropertyMetadata(False, AddressOf OnTraceEnabledChanged))
        
        Private Shared Sub OnTraceEnabledChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            Dim triggerBase As TriggerBase = TryCast(d, TriggerBase)
        
            If triggerBase Is Nothing Then
                Return
            End If
        
            If Not (TypeOf e.NewValue Is Boolean) Then
                Return
            End If
        
            If CBool(e.NewValue) Then
                ' insert dummy story-boards which can later be traced using WPF animation tracing
        
                Dim storyboard As TriggerTraceStoryboard = New TriggerTraceStoryboard(triggerBase, TriggerTraceStoryboardType.Enter)
                triggerBase.EnterActions.Insert(0, New BeginStoryboard() With { .Storyboard = storyboard })
        
                storyboard = New TriggerTraceStoryboard(triggerBase, TriggerTraceStoryboardType.[Exit])
                triggerBase.ExitActions.Insert(0, New BeginStoryboard() With {.Storyboard = storyboard })
            Else
                ' remove the dummy storyboards
        
                For Each actionCollection As TriggerActionCollection In New TriggerActionCollection() {triggerBase.EnterActions, triggerBase.ExitActions}
                    For Each triggerAction As TriggerAction In actionCollection
                        Dim bsb As BeginStoryboard = TryCast(triggerAction, BeginStoryboard)
        
                        If bsb IsNot Nothing AndAlso bsb.Storyboard IsNot Nothing AndAlso TypeOf bsb.Storyboard Is TriggerTraceStoryboard Then
                            actionCollection.Remove(bsb)
                            Exit For
                        End If
                    Next
                Next
            End If
        End Sub
        
    #End Region
    
    #Region "Nested Types"
        
        Private Enum TriggerTraceStoryboardType
            Enter
            [Exit]
        End Enum
        
        ''' <summary> A dummy storyboard for tracing purposes </summary>
        Private Class TriggerTraceStoryboard
            Inherits Storyboard
        
            Private m_StoryboardType As TriggerTraceStoryboardType
            Private m_TriggerBase As TriggerBase
        
            Public Property StoryboardType() As TriggerTraceStoryboardType
                Get
                    Return m_StoryboardType
                End Get
                Private Set
                    m_StoryboardType = Value
                End Set
            End Property
        
            Public Property TriggerBase() As TriggerBase
                Get
                    Return m_TriggerBase
                End Get
                Private Set
                    m_TriggerBase = Value
                End Set
            End Property
        
            Public Sub New(triggerBase__1 As TriggerBase, storyboardType__2 As TriggerTraceStoryboardType)
                TriggerBase = triggerBase__1
                StoryboardType = storyboardType__2
            End Sub
        End Class
        
        ''' <summary> A custom tracelistener. </summary>
        Private Class TriggerTraceListener
            Inherits TraceListener
            
            Public Overrides Sub TraceEvent(eventCache As TraceEventCache, source As String, eventType As TraceEventType, id As Integer, format As String, ParamArray Args As Object())
                MyBase.TraceEvent(eventCache, source, eventType, id, format, args)
                
                If format.StartsWith("Storyboard has begun;") Then
                    Dim storyboard As TriggerTraceStoryboard = TryCast(args(1), TriggerTraceStoryboard)
                    If storyboard IsNot Nothing Then
                        ' add a breakpoint here to see when your trigger has been
                        ' entered or exited
                        
                        ' the element being acted upon
                        Dim targetElement As Object = Args(5)
                        
                        ' the namescope of the element being acted upon
                        Dim namescope As INameScope = DirectCast(args(7), INameScope)
                        
                        Dim triggerBase As TriggerBase = storyboard.TriggerBase
                        Dim triggerName As String = GetTriggerName(storyboard.TriggerBase)
                        
                        Debug.WriteLine(String.Format("Element: {0}, {1}: {2}: {3}", targetElement, triggerBase.[GetType]().Name, triggerName, storyboard.StoryboardType))
                    End If
                End If
            End Sub
            
            Public Overrides Sub Write(message As String)
            End Sub
            
            Public Overrides Sub WriteLine(message As String)
            End Sub
        End Class
    #End Region
    
End Class

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
