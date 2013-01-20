
Imports System

''' <summary>  A timer which performs an action on a given thread when time elapses. Rescheduling is supported. </summary>
 ''' <remarks> 
 ''' <para>
 ''' By default that works on the WPF UI thread. By specifying a dispatcher another thread can be selected.
 ''' </para>
 ''' <para>
 ''' Origin: <a href="http://www.codeproject.com/KB/WPF/SnappyFiltering.aspx" target="_blank"> "Deferring ListCollectionView filter updates for a responsive UI" by Matt T Hayes </a> 
 ''' </para>
 ''' <para>
 ''' Changes: Support for use of any thread.
 ''' </para>
 ''' <para>
 ''' Usage example:
 ''' </para>
 ''' <para>
 ''' WPF UI tread: Dim DeferredScrollAction As DeferredAction = New DeferredAction(AddressOf scrollToEndOfLog)
 ''' </para>
 ''' <para>
 ''' Specified thread: Dim DeferredScrollAction As DeferredAction = New DeferredAction(AddressOf scrollToEndOfLog, System.Windows.Threading.Dispatcher.CurrentDispatcher)
 ''' </para>
 ''' <para>
 ''' Schedule the action: DeferredScrollAction.Defer(TimeSpan.FromMilliseconds(100))
 ''' </para>
 ''' </remarks>
Public Class DeferredAction
    Implements IDisposable
    
    Private timer As System.Threading.Timer
    
    #Region "Constuctor"
        
        ''' <summary> Creates a new DeferredAction running in WPF UI thread (works in a WPF application only). </summary>
         ''' <param name="action"> The action that is intended to be invoked deferred. </param>
        Public Sub New(action As Action)
            Me.New(action, System.Windows.Application.Current.Dispatcher)
        End Sub
        
        ''' <summary> Creates a new DeferredAction running in a given thread. </summary>
         ''' <param name="action"> The action that is intended to be invoked deferred. </param>
         ''' <param name="dispatcher"> The dispatcher that will invoke the action (when time has come). </param>
        Public Sub New(action As Action, dispatcher As System.Windows.Threading.Dispatcher)
            If (action Is Nothing) Then
                Throw New ArgumentNullException("action")
            Else If (dispatcher Is Nothing) Then
                Throw New ArgumentNullException("dispatcher")
            End If
            
            Me.timer = New System.Threading.Timer(New System.Threading.TimerCallback(Function() dispatcher.Invoke(action) ))
        End Sub
        
    #End Region
    
    #Region "Public Methods"
        
        ''' <summary>
        ''' Schedules the action for performing after the specified delay.
        ''' Repeated calls will reschedule the action if it has not already been performed.
        ''' </summary>
         ''' <param name="delay">
         ''' The amount of time to wait before performing the action.
         ''' </param>
        Public Sub Defer(delay As TimeSpan)
            ' Fire action when time elapses (with no subsequent calls).
            Me.timer.Change(delay, TimeSpan.FromMilliseconds(-1))
        End Sub
        
    #End Region
    
    #Region "IDisposable Members"
        
        Public Sub Dispose() Implements IDisposable.Dispose
            If (Me.timer IsNot Nothing) Then
                Me.timer.Dispose()
                Me.timer = Nothing
            End If
        End Sub
        
    #End Region

End Class

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
