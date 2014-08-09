
Imports System

''' <summary>  A timer that performs an action on a certain thread when time elapses. Rescheduling is supported. </summary>
 ''' <remarks> 
 ''' <para>
 ''' By default that works on the current thread. By specifying a dispatcher another thread can be determined.
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
 ''' Current thread: <c>Dim DeferredScrollAction As DeferredAction = New DeferredAction(AddressOf scrollToEndOfLog)</c>
 ''' </para>
 ''' <para>
 ''' Specified thread (WPF Application's UI tread in this case): <c>Dim DeferredScrollAction As DeferredAction = New DeferredAction(AddressOf scrollToEndOfLog, System.Windows.Application.Current.Dispatcher)</c>
 ''' </para>
 ''' <para>
 ''' Schedule the action: <c>DeferredScrollAction.Defer(TimeSpan.FromMilliseconds(100))</c>
 ''' </para>
 ''' </remarks>
Public Class DeferredAction
    Implements IDisposable
    
    Private timer As System.Threading.Timer
    
    #Region "Constuctor"
        
        ''' <summary> Creates a new DeferredAction running in current thread. </summary>
         ''' <param name="action"> The action that is intended to be invoked deferred. </param>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="action"/> is <see langword="null"/>. </exception>
        Public Sub New(action As Action)
            Me.New(action, System.Windows.Threading.Dispatcher.CurrentDispatcher)
        End Sub
        
        ''' <summary> Creates a new DeferredAction running in a given thread. </summary>
         ''' <param name="action">     The action that is intended to be invoked deferred. </param>
         ''' <param name="dispatcher"> The dispatcher that will invoke the action (when time has come). </param>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="action"/> is <see langword="null"/>. </exception>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="dispatcher"/> is <see langword="null"/>. </exception>
        Public Sub New(action As Action, dispatcher As System.Windows.Threading.Dispatcher)
            
            If (action Is Nothing) Then Throw New ArgumentNullException("action")
            If (dispatcher Is Nothing) Then Throw New ArgumentNullException("dispatcher")
            
            Me.timer = New System.Threading.Timer(New System.Threading.TimerCallback(Function() dispatcher.Invoke(action) ))
        End Sub
        
    #End Region
    
    #Region "Public Methods"
        
        ''' <summary>
        ''' Schedules the action for performing once after the specified delay.
        ''' Repeated calls will reschedule the action, if it has not already been performed.
        ''' </summary>
         ''' <param name="delay"> The amount of time to wait before performing the action. </param>
         ''' <exception cref="System.ObjectDisposedException">     This <see cref="DeferredAction"/> (resp. it's timer) has been already disposed of. </exception>
         ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="delay"/> is less than -1. </exception>
         ''' <exception cref="System.NotSupportedException">       <paramref name="delay"/> is greater than 4294967294 milliseconds. </exception>
        Public Sub Defer(delay As TimeSpan)
            Me.timer.Change(delay, TimeSpan.FromMilliseconds(-1))
        End Sub
        
        ''' <summary>
        ''' Schedules the action for performing once after the specified delay.
        ''' Repeated calls will reschedule the action, if it has not already been performed.
        ''' </summary>
         ''' <param name="delay"> The amount of time (in milliseconds) to wait before performing the action. </param>
         ''' <exception cref="System.ObjectDisposedException">     This <see cref="DeferredAction"/> (resp. it's timer) has been already disposed of. </exception>
         ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="delay"/> is less than -1. </exception>
         ''' <exception cref="System.NotSupportedException">       <paramref name="delay"/> is greater than 4294967294 milliseconds. </exception>
        Public Sub Defer(delay As Double)
            Me.timer.Change(TimeSpan.FromMilliseconds(delay), TimeSpan.FromMilliseconds(-1))
        End Sub
        
        ''' <summary> Aborts an active schedule, if there is one. </summary>
         ''' <exception cref="System.ObjectDisposedException"> This <see cref="DeferredAction"/> (resp. it's timer) has been already disposed of. </exception>
        Public Sub Abort()
            Me.timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1))
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

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
