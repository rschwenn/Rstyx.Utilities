
Imports System
Imports System.Diagnostics
Imports System.Threading
Imports System.Windows.Threading

''' <summary>  A DeferTimer that performs an Action on a certain thread when time elapses. Rescheduling is supported. </summary>
 ''' <remarks> 
 ''' <para>
 ''' By default that works on the current thread. By specifying a Dispatcher another thread can be determined.
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
 ''' Schedule the Action: <c>DeferredScrollAction.Defer(TimeSpan.FromMilliseconds(100))</c>
 ''' </para>
 ''' </remarks>
Public Class DeferredAction
    Implements IDisposable
    
    Protected DeferTimer As Timer

    Protected ReadOnly IsStatusSupported As Boolean
    Protected ReadOnly DelayCounter      As Stopwatch

    #Region "Fields"
        
        ''' <summary> Gets or sets the status of this DeferredAction: <see langword="true"/>, if execution of an action is scheduled. </summary>
         ''' <returns> Status of currently deferring an action. </returns>
         ''' <remarks>
         ''' This is set to <see langword="true"/>, when <see cref="Defer"/> is called. 
         ''' This property is meaningfull only if the deferred action is of type <see cref="Action(Of DeferredAction)"/>,
         ''' which gets this DeferredAction as argument and set this property to <see langword="false"/>, after finished. 
         ''' Otherwise, a given maximum delay can't be respected.
         ''' </remarks>
        Public Property IsDeferring As Boolean = False
        
    #End Region
    
    #Region "Constuctor"
        
        ''' <summary> Creates a new DeferredAction running in current thread. </summary>
         ''' <param name="Action"> The Action that is intended to be invoked deferred. </param>
         ''' <exception cref="ArgumentNullException"> <paramref name="Action"/> is <see langword="null"/>. </exception>
        Public Sub New(Action As Action)
            Me.New(Action, Dispatcher.CurrentDispatcher)
        End Sub
        
        ''' <summary> Creates a new DeferredAction running in a given thread. </summary>
         ''' <param name="Action">     The Action that is intended to be invoked deferred. </param>
         ''' <param name="Dispatcher"> The Dispatcher that will invoke the Action (when time has come). </param>
         ''' <exception cref="ArgumentNullException"> <paramref name="Action"/> is <see langword="null"/>. </exception>
         ''' <exception cref="ArgumentNullException"> <paramref name="Dispatcher"/> is <see langword="null"/>. </exception>
        Public Sub New(Action As Action, Dispatcher As Dispatcher)
            
            If (Action Is Nothing) Then Throw New ArgumentNullException("action")
            If (Dispatcher Is Nothing) Then Throw New ArgumentNullException("dispatcher")
            
            Me.DeferTimer     = New Timer(New TimerCallback(Sub() Dispatcher.Invoke(Action) ))
            IsStatusSupported = False
        End Sub
        

        ''' <summary> Creates a new DeferredAction running in current thread. </summary>
         ''' <param name="Action"> The Action that is intended to be invoked deferred and to signal execution. </param>
         ''' <exception cref="ArgumentNullException"> <paramref name="Action"/> is <see langword="null"/>. </exception>
        Public Sub New(Action As Action(Of DeferredAction))
            Me.New(Action, Dispatcher.CurrentDispatcher)
        End Sub
        
        ''' <summary> Creates a new DeferredAction running in a given thread. </summary>
         ''' <param name="Action">     The Action that is intended to be invoked deferred and to signal execution. </param>
         ''' <param name="Dispatcher"> The Dispatcher that will invoke the Action (when time has come). </param>
         ''' <exception cref="ArgumentNullException"> <paramref name="Action"/> is <see langword="null"/>. </exception>
         ''' <exception cref="ArgumentNullException"> <paramref name="Dispatcher"/> is <see langword="null"/>. </exception>
        Public Sub New(Action As Action(Of DeferredAction), Dispatcher As Dispatcher)
            
            If (Action Is Nothing) Then Throw New ArgumentNullException("Action")
            If (Dispatcher Is Nothing) Then Throw New ArgumentNullException("Dispatcher")
            
            Me.DeferTimer     = New Timer(New TimerCallback(Sub() Dispatcher.Invoke(Action, Me) ))
            IsStatusSupported = True
            DelayCounter      = New Stopwatch()
        End Sub
        
    #End Region
    
    #Region "Public Methods"
        
        ''' <summary>
        ''' Schedules the Action for performing once after the specified Delay.
        ''' Repeated calls will reschedule the Action, if it has not already been performed.
        ''' </summary>
         ''' <param name="Delay"> The amount of time (in milliseconds) to wait before performing the Action. </param>
         ''' <exception cref="ObjectDisposedException">     This <see cref="DeferredAction"/> (resp. it's DeferTimer) has been already disposed of. </exception>
         ''' <exception cref="ArgumentOutOfRangeException"> <paramref name="Delay"/> is less than -1. </exception>
         ''' <exception cref="NotSupportedException">       <paramref name="Delay"/> is greater than 4294967294 milliseconds. </exception>
        Public Sub Defer(Delay As Double)
            Me.Defer(TimeSpan.FromMilliseconds(Delay))
        End Sub
        
        ''' <summary>
        ''' Schedules the Action for performing once after the specified Delay.
        ''' Repeated calls will reschedule the Action, if it has not already been performed.
        ''' </summary>
         ''' <param name="Delay"> The amount of time to wait before performing the Action. </param>
         ''' <exception cref="ObjectDisposedException">     This <see cref="DeferredAction"/> (resp. it's DeferTimer) has been already disposed of. </exception>
         ''' <exception cref="ArgumentOutOfRangeException"> <paramref name="Delay"/> is less than -1. </exception>
         ''' <exception cref="NotSupportedException">       <paramref name="Delay"/> is greater than 4294967294 milliseconds. </exception>
        Public Sub Defer(Delay As TimeSpan)
            Me.DeferTimer.Change(Delay, Timeout.InfiniteTimeSpan)
        End Sub
        
        ''' <summary>
        ''' Schedules the Action for performing once after the specified Delay.
        ''' Repeated calls will reschedule the Action, if it has not already been performed.
        ''' If the action supports status monitoring, then it's invoked after the specified <paramref name="MaxDelay"/>, 
        ''' even if it would have been rescheduled otherwise.
        ''' </summary>
         ''' <param name="Delay"> The amount of time (in milliseconds) to wait before performing the Action. </param>
         ''' <param name="MaxDelay"> The maximum amount of time (in milliseconds) to wait before performing the Action. </param>
         ''' <exception cref="ObjectDisposedException">     This <see cref="DeferredAction"/> (resp. it's DeferTimer) has been already disposed of. </exception>
         ''' <exception cref="ArgumentOutOfRangeException"> <paramref name="Delay"/> is less than -1. </exception>
         ''' <exception cref="NotSupportedException">       <paramref name="Delay"/> is greater than 4294967294 milliseconds. </exception>
         ''' <exception cref="ArgumentException">           <paramref name="MaxDelay"/> isn't greater than <paramref name="Delay"/>. </exception>
        Public Sub Defer(Delay As Double, MaxDelay As Double)
            Me.Defer(TimeSpan.FromMilliseconds(Delay), TimeSpan.FromMilliseconds(MaxDelay))
        End Sub
        
        ''' <summary>
        ''' Schedules the Action for performing once after the specified Delay.
        ''' Repeated calls will reschedule the Action, if it has not already been performed.
        ''' If the action supports status monitoring, then it's invoked after the specified <paramref name="MaxDelay"/>, 
        ''' even if it would have been rescheduled otherwise.
        ''' </summary>
         ''' <param name="Delay"> The amount of time to wait before performing the Action. </param>
         ''' <param name="MaxDelay"> The maximum amount of time to wait before performing the Action. </param>
         ''' <exception cref="ObjectDisposedException">     This <see cref="DeferredAction"/> (resp. it's DeferTimer) has been already disposed of. </exception>
         ''' <exception cref="ArgumentOutOfRangeException"> <paramref name="Delay"/> is less than -1. </exception>
         ''' <exception cref="NotSupportedException">       <paramref name="Delay"/> is greater than 4294967294 milliseconds. </exception>
         ''' <exception cref="ArgumentException">           <paramref name="MaxDelay"/> is greater than Zero, but not greater than <paramref name="Delay"/>. </exception>
        Public Sub Defer(Delay As TimeSpan, MaxDelay As TimeSpan)

            If (MaxDelay.TotalMilliseconds > 0) Then
                
                If (MaxDelay <= Delay) Then Throw New System.ArgumentException("MaxDelay")
                
                If (IsStatusSupported) Then
                    If (Me.IsDeferring) Then
                        If (DelayCounter.ElapsedMilliseconds > MaxDelay.TotalMilliseconds) Then
                            ' Execute now, hence schedule in 0 millisecond.
                            Me.IsDeferring = False
                            Delay = TimeSpan.FromMilliseconds(0)
                        End If
                    Else
                        Me.IsDeferring = True
                        DelayCounter.Restart()
                    End If
                End If
            End If

            Me.DeferTimer.Change(Delay, Timeout.InfiniteTimeSpan)
        End Sub
        

        ''' <summary> Aborts an active schedule, if there is one. </summary>
         ''' <exception cref="ObjectDisposedException"> This <see cref="DeferredAction"/> (resp. it's DeferTimer) has been already disposed of. </exception>
        Public Sub Abort()
            Me.DeferTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan)
            Me.IsDeferring = False
        End Sub
        
    #End Region
    
    #Region "IDisposable Members"
        
        Public Sub Dispose() Implements IDisposable.Dispose
            If (Me.DeferTimer IsNot Nothing) Then
                Me.DeferTimer.Dispose()
                Me.DeferTimer = Nothing
            End If
        End Sub
        
    #End Region

End Class

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
