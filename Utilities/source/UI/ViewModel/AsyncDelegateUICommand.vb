
Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Input
Imports System.Windows.Threading


Namespace UI.ViewModel
    
    ''' <summary> A Delegate Command for use in UI elements, that executes a cancellable action in current or separate thread. </summary>
     ''' <remarks>
     ''' <para>
     ''' While the target command action is executing, the <c>IsBusy</c> property will be <c>True</c> and the provided <c>ICommand.Execute()</c> action 
     ''' will be a cancel action that can signal a cancel request to the threaded action. The Decoration property is changed accordingly.
     ''' This way a button that is bound to this command, changes it's appearance and command while the target command is executing.
     ''' </para>
     ''' <para>
     ''' Construction: The target command has to be packed as <see cref="DelegateUICommandInfo"/> and passed to the constructor.
     ''' </para>
     ''' <para>
     ''' Since the <c>Task.Factory.StartNew()</c> method doesn't allow for argument types other than Object, the target command can't have a strongly typed argument.
     ''' But in order to be cancelable the target execute action has to take an argument of type <see cref="System.Threading.CancellationToken"/>
     ''' and periodically check and react to <c>CancelToken.IsCancellationRequested</c>.
     ''' </para>
     ''' <para>
     ''' <see cref="AsyncDelegateUICommand.RaiseCanExecuteChanged"/> has to be called on this command in order to reflect changes of execution conditions that occur outside this command.
     ''' If the event listener is a dispatcher object (i.e. Button) then the event is raised on it's dispatcher, otherwise on the current thread's dispatcher. 
     ''' </para>
     ''' </remarks>
    Public NotInheritable Class AsyncDelegateUICommand
        Implements ICommand
        Implements INotifyPropertyChanged
        
        #Region "Private Fields"
            
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UI.ViewModel.AsyncDelegateUICommand")
            
            Private CancelTaskCommandInfo           As DelegateUICommandInfo = Nothing
            Private CmdTask                         As Task = Nothing
            Private CmdTaskCancelTokenSource        As CancellationTokenSource = Nothing
            
            Private _IsCancellationSupported        As Boolean = False
            Private _IsBusy                         As Boolean = False
            Private _Decoration                     As UICommandDecoration = Nothing
            
            ''' <summary> Returns the Dispatcher of the tread which has created this view model (and should be the WPF UI thread). </summary>
            Protected ReadOnly WpfUiDispatcher      As Dispatcher
            
        #End Region
        
        #Region "Public Fields"
            
            ''' <summary> Provides access to the target command info. </summary>
            Public ReadOnly TargetCommandInfo       As DelegateUICommandInfo = Nothing
            
            ''' <summary> Provides access to the cancel callback action. </summary>
            Public ReadOnly CancelCallback          As Action = Nothing
            
            ''' <summary> If <see langword="true"/>, the command will be executed in a separate thread, otherwise not. </summary>
            Public ReadOnly IsAsync                 As Boolean = False
            
            ''' <summary> The <see cref="ApartmentState"/> for the separate thread. Only meaningfull if <see cref="IsAsync"/> is <see langword="true"/>. </summary>
            Public ReadOnly ThreadAptState          As ApartmentState = ApartmentState.MTA
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new command that runs in current thread and supports cancellation. </summary>
             ''' <param name="TargetCommandInfo"> The target <see cref="DelegateUICommandInfo" /> for the desired action. </param>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/><b>.ExecuteAction</b> is <see langword="null"/>. </exception>
            Public Sub New(TargetCommandInfo As DelegateUICommandInfo)
                Me.New(TargetCommandInfo, Nothing, SupportsCancellation:=True, runAsync:=False)
            End Sub
            
            ''' <summary> Creates a new command that runs in current thread and may support cancellation. </summary>
             ''' <param name="TargetCommandInfo">    The target <see cref="DelegateUICommandInfo" /> for the desired action. </param>
             ''' <param name="SupportsCancellation"> Should be False, when the target action doesn't worry about cancellation requests. </param>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/><b>.ExecuteAction</b> is <see langword="null"/>. </exception>
            Public Sub New(TargetCommandInfo As DelegateUICommandInfo, SupportsCancellation As Boolean)
                Me.New(TargetCommandInfo, Nothing, SupportsCancellation, runAsync:=False)
            End Sub
            
            ''' <summary> Creates a new command that runs in current thread and may support cancellation and a cancelation callback. </summary>
             ''' <param name="TargetCommandInfo">    The target <see cref="DelegateUICommandInfo" /> for the desired action. </param>
             ''' <param name="CancelCallback">       Action that is invoked after the target command has been cancelled. </param>
             ''' <param name="SupportsCancellation"> Should be False, when the target action doesn't worry about cancellation requests. </param>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/><b>.ExecuteAction</b> is <see langword="null"/>. </exception>
            Public Sub New(TargetCommandInfo As DelegateUICommandInfo, CancelCallback As Action, SupportsCancellation As Boolean)
                Me.New(TargetCommandInfo, CancelCallback, SupportsCancellation, runAsync:=False)
            End Sub
            
            ''' <summary> Creates a new asyncronous command that may run in a separate thread and may support cancellation and a cancelation callback. </summary>
             ''' <param name="TargetCommandInfo">    The target <see cref="DelegateUICommandInfo" /> for the desired action. </param>
             ''' <param name="CancelCallback">       Action that is invoked after the target command has been cancelled. May be <see langword="null"/> </param>
             ''' <param name="SupportsCancellation"> Should be <c>False</c>, when the target action doesn't worry about cancellation requests. </param>
             ''' <param name="runAsync">             If <see langword="true"/>, the command will be executed in a separate MTA thread, otherwise not. </param>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/><b>.ExecuteAction</b> is <see langword="null"/>. </exception>
             ''' <remarks> If <paramref name="SupportsCancellation"/> is <see langword="True"/>, the cancellation command won't be available. </remarks>
            Public Sub New(TargetCommandInfo As DelegateUICommandInfo, CancelCallback As Action, SupportsCancellation As Boolean, runAsync As Boolean)
                Me.New(TargetCommandInfo, CancelCallback, SupportsCancellation, runAsync:=runAsync, ThreadAptState:=ApartmentState.MTA)
            End Sub
            
            ''' <summary> Creates a new asyncronous command that may run in a separate thread and may support cancellation and a cancelation callback. </summary>
             ''' <param name="TargetCommandInfo">    The target <see cref="DelegateUICommandInfo" /> for the desired action. </param>
             ''' <param name="CancelCallback">       Action that is invoked after the target command has been cancelled. May be <see langword="null"/> </param>
             ''' <param name="SupportsCancellation"> Should be <c>False</c>, when the target action doesn't worry about cancellation requests. </param>
             ''' <param name="runAsync">             If <see langword="true"/>, the command will be executed in a separate thread, otherwise not. </param>
             ''' <param name="ThreadAptState">       Sets the <see cref="ApartmentState"/> for the new thread. Only meaningfull if <paramref name="runAsync"/> is <see langword="true"/>. </param>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/> is <see langword="null"/>. </exception>
             ''' <exception cref="ArgumentNullException"> <paramref name="TargetCommandInfo"/><b>.ExecuteAction</b> is <see langword="null"/>. </exception>
             ''' <remarks> If <paramref name="SupportsCancellation"/> is <see langword="True"/>, the cancellation command won't be available. </remarks>
            Public Sub New(TargetCommandInfo As DelegateUICommandInfo, CancelCallback As Action, SupportsCancellation As Boolean, runAsync As Boolean, ThreadAptState As ApartmentState)
                
                If (TargetCommandInfo Is Nothing) Then Throw New ArgumentNullException("TargetCommandInfo")
                If (TargetCommandInfo.ExecuteAction Is Nothing) Then Throw New ArgumentNullException("TargetCommandInfo.ExecuteAction")
                
                WpfUiDispatcher = Dispatcher.CurrentDispatcher
                
                Me.TargetCommandInfo = TargetCommandInfo
                Me.CancelCallback = CancelCallback
                
                Me.IsAsync = runAsync
                Me.ThreadAptState = ThreadAptState
                _IsCancellationSupported = SupportsCancellation
                
                Me.Decoration = TargetCommandInfo.Decoration
                CancelTaskCommandInfo = createCancelTaskCommandInfo()
            End Sub
            
        #End Region
        
        #Region "Properties" 
            
            ''' <summary> Gets or sets the Command's UI Decoration. This is for binding to the UI. </summary>
            Public Property Decoration() As UICommandDecoration
                Get
                    Return _Decoration
                End Get
                Set(value As UICommandDecoration)
                    _Decoration = value
                    Me.OnPropertyChanged("Decoration")
                End Set
            End Property
            
            ''' <summary> Returns True if the target command is currently beeing executed. </summary>
            Public ReadOnly Property IsBusy() As Boolean
                Get
                    Return _IsBusy
                End Get
            End Property
            
            ''' <summary> Gets or sets whether or not cancellation of the running target action is supported. </summary>
            Public Property IsCancellationSupported() As Boolean
                Get
                    Return _IsCancellationSupported
                End Get
                Set(value As Boolean)
                    If (value Xor _IsCancellationSupported) Then
                        _IsCancellationSupported = value
                        Me.OnPropertyChanged("IsCancellationSupported")
                        If (Me.IsBusy) Then RaiseCanExecuteChanged()
                    End If
                End Set
            End Property
            
        #End Region
        
        #Region "ICommand Members"
            
            Private CanExecuteChangedEvents As New System.Collections.ObjectModel.Collection(Of EventHandler)
            
            ''' <summary> Indicates that changes occur which affect whether or not the command could be executed. This isn't raised automatically. </summary>
             ''' <remarks> 
             ''' <para> The class which holds this command has to call RaiseCanExecuteChanged() on this command when conditions has been changed. </para>
             ''' <para>
             ''' If the event listener is a dispatcher object then the event is raised on it's dispatcher, otherwise on the dispatcher
             ''' that has created this command instance (which should be the dispatcher of WPF UI).
             ''' </para>
             ''' </remarks>
            Public Custom Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
                
                AddHandler(ByVal value As EventHandler)
                    If (Not CanExecuteChangedEvents.Contains(value)) Then CanExecuteChangedEvents.Add(value)
                End AddHandler
                
                RemoveHandler(ByVal value As EventHandler)
                    If (CanExecuteChangedEvents.Contains(value)) Then CanExecuteChangedEvents.Remove(value)
                End RemoveHandler
                
                RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)
                    Try
                        For Each Handler as EventHandler In CanExecuteChangedEvents
                            If (Handler IsNot Nothing) Then
                                
                                ' Get the matching dispatcher.
                                Dim Dispatcher As System.Windows.Threading.Dispatcher = Nothing
                                
                                If (TypeOf Handler.Target Is System.Windows.Threading.DispatcherObject) Then
                                    Dispatcher = CType(Handler.Target, System.Windows.Threading.DispatcherObject).Dispatcher
                                    
                                Else
                                    Dispatcher = WpfUiDispatcher
                                    'Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher
                                End If
                                
                                ' Invoke the handler.
                                If (Dispatcher IsNot Nothing) Then
                                    Dispatcher.BeginInvoke(Handler, sender, e)
                                End If
                            End If
                        Next
                    Catch ex As System.Exception
                        Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AsyncDelegateUICommand_ErrorInCalledCanExecuteChangedHandler, Me.TargetCommandInfo.Decoration.Caption))
                    End Try
                End RaiseEvent
            End Event
            
            ''' <summary> Raises the CanExecuteChanged event. </summary>
            Public Sub RaiseCanExecuteChanged()
                
                RaiseEvent CanExecuteChanged(Me, System.EventArgs.Empty)
                
                If (Not Me.IsAsync) Then
                    Try
                        Cinch.ApplicationHelper.DoEvents()
                    Catch ex As System.Exception
                        Logger.logDebug(ex.ToString())
                        Logger.logDebug(Rstyx.Utilities.Resources.Messages.Global_DoEventsFailed)
                    End Try
                End If
            End Sub
            
            ''' <summary> Determines if the currently active command (target or cancel) can execute by invoking the matching Predicate(Of Object). </summary>
             ''' <param name="parameter"> The parameter to use when determining if this command can execute. </param>
             ''' <returns> Returns true if the command can execute, False otherwise. </returns>
            Public Function CanExecute(ByVal parameter As Object) As Boolean Implements ICommand.CanExecute
                Dim RetValue  As Boolean = True
                Try
                    If (Me.IsBusy AndAlso (CancelTaskCommandInfo.CanExecutePredicate IsNot Nothing)) Then
                        RetValue = CancelTaskCommandInfo.CanExecutePredicate.Invoke(Nothing)
                    ElseIf ((Not Me.IsBusy) AndAlso (TargetCommandInfo.CanExecutePredicate IsNot Nothing)) Then
                        RetValue = TargetCommandInfo.CanExecutePredicate.Invoke(Nothing)
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                End Try
                Return RetValue
            End Function
            
            ''' <summary> Executes the currently active command (target or cancel) by invoking the matching Action(Of Object). </summary>
             ''' <param name="parameter"> Data for the command. </param>
            Public Sub Execute(ByVal parameter As Object) Implements ICommand.Execute
                Try
                    If (Me.IsBusy) Then
                        ' Request Cancellation of the running command.
                        If (CanCancelTask()) Then
                            CancelTaskCommandInfo.ExecuteAction(Nothing)
                        End If
                    Else
                        ' Start the target command.
                        If (TargetCommandInfo.ExecuteAction IsNot Nothing) Then
                            
                            ' Always activate CancelTaskCommand. If it isn't supported, then the command will be disabled due to CanCancelTask().
                            Me.Decoration = CancelTaskCommandInfo.Decoration
                            
                            ' Change State.
                            _IsBusy = True
                            Me.OnPropertyChanged("IsBusy")
                            
                            ' Create a CancellationToken and register CancelCallback if available.
                            CmdTaskCancelTokenSource = New CancellationTokenSource()
                            Dim CmdTaskCancelToken As CancellationToken = CmdTaskCancelTokenSource.Token
                            If (CancelCallback IsNot Nothing) Then CmdTaskCancelToken.Register(CancelCallback)
                            
                            If (Me.IsAsync) Then
                                ' Create and start a new task.
                                If (Me.ThreadAptState = ApartmentState.STA) Then
                                    'Dim sta As StaTaskScheduler = New StaTaskScheduler(NumberOfThreads:=1)
                                    'CmdTask = Task.Factory.StartNew(TargetCommandInfo.ExecuteAction, CmdTaskCancelToken, CmdTaskCancelToken, TaskCreationOptions.None, sta)
                                    CmdTask = Task.Factory.StartNew(TargetCommandInfo.ExecuteAction, CmdTaskCancelToken, CmdTaskCancelToken, TaskCreationOptions.None, New StaTaskScheduler(NumberOfThreads:=1))
                                Else
                                    CmdTask = Task.Factory.StartNew(TargetCommandInfo.ExecuteAction, CmdTaskCancelToken, CmdTaskCancelToken)
                                End If
                                
                                #If DEBUG Then
                                    If (System.Windows.Application.Current IsNot Nothing) Then Debug.Print("AsyncDelegateUICommand \ Execute: WPF UI thread ID  = " & System.Windows.Application.Current.Dispatcher.Thread.ManagedThreadId.ToString())
                                    Debug.Print("AsyncDelegateUICommand \ Execute: Current thread ID = " & Thread.CurrentThread.ManagedThreadId.ToString())
                                #End If
                                
                                ' Register the task's continuation callback, which will be invoked in current thread (which shuld be the UI thread since this command is an UI command).
                                Try
                                    CmdTask.ContinueWith(AddressOf finishTask, TaskScheduler.FromCurrentSynchronizationContext())
                                Catch ex As System.ObjectDisposedException
                                    ' Task has been finished already (?)
                                    finishTask(CmdTask)
                                End Try
                            Else
                                ' Start ExecuteAction in current thread.
                                ' Nevertheless pass the cancellation token in order to enable the action to get cancellation requests.
                                RaiseCanExecuteChanged()
                                Try
                                    TargetCommandInfo.ExecuteAction.Invoke(CmdTaskCancelToken)
                                Catch ex As System.Exception
                                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AsyncDelegateUICommand_SyncExecuteFailed, Me.TargetCommandInfo.Decoration.Caption))
                                Finally
                                    finishTask(Nothing)
                                End Try
                            End If
                        End If
                    End If
                    
                    RaiseCanExecuteChanged()
                    
                Catch ex As System.Exception
                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                End Try
            End Sub
            
        #End Region
        
        #Region "INotifyPropertyChanged Members"
            
            ''' <summary>  Raised when a property on this object has a new value. </summary>
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            
            ''' <summary> Raises this object's <c>PropertyChanged</c> event. </summary>
             ''' <param name="propertyName"> The property that has a new value. </param>
            Private Sub OnPropertyChanged(ByVal propertyName As String)
                Me.VerifyPropertyName(propertyName)
                Try
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
                Catch ex As System.Exception
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
                End Try
            End Sub
            
        #End Region
        
        #Region "Debugging Aides"
            
            ''' <summary>
            ''' Warns the developer if this object does not have
            ''' a public property with the specified name. This 
            ''' method does not exist in a Release build.
            ''' </summary>
             ''' <param name="propertyName"> The property that has to be verified. </param>
            <Conditional("DEBUG"), DebuggerStepThrough()> _
            Public Sub VerifyPropertyName(ByVal propertyName As String)
                ' Verify that the property name matches a real,  
                ' public, instance property on this object.
                If ((From pi As System.Reflection.PropertyInfo In MyClass.GetType.GetProperties() Where pi.Name = propertyName.Replace("[]", String.Empty)).Count < 1) Then
                    Dim msg As String = "Invalid property name: " & propertyName
                    
                    If Me.ThrowOnInvalidPropertyName Then
                        Throw New Exception(msg)
                    Else
                        Debug.Fail(msg)
                    End If
                End If
            End Sub
            
            Private _ThrowOnInvalidPropertyName As Boolean = False
            
            ''' <summary>
            ''' Returns whether an exception is thrown, or if a Debug.Fail() is used
            ''' when an invalid property name is passed to the VerifyPropertyName method.
            ''' The default value is false, but subclasses used by unit tests might 
            ''' override this property's getter to return true.
            ''' </summary>
            Public Property ThrowOnInvalidPropertyName() As Boolean
                Get
                    Return _ThrowOnInvalidPropertyName
                End Get
                Set(ByVal value As Boolean)
                    _ThrowOnInvalidPropertyName = value
                End Set
            End Property
            
        #End Region
        
        #Region "CancelTask Command"
            
            ''' <summary> Requests Cancellation of running task. </summary>
            Private Function createCancelTaskCommandInfo() As DelegateUICommandInfo
                Dim CmdInfo  As New DelegateUICommandInfo()
                Try
                    Dim Decoration As New UICommandDecoration()
                    Decoration.Caption     = "Abbruch"
                    Decoration.Description = "Abbruch der laufenden Aktion."
                    Decoration.IconBrush   = Rstyx.Utilities.UI.Resources.UIResources.IconBrush("Tango_Stop1")
                    
                    CmdInfo.ExecuteAction       = AddressOf Me.cancelTask
                    CmdInfo.CanExecutePredicate = AddressOf Me.CanCancelTask
                    CmdInfo.Decoration          = Decoration
                    
                Catch ex As System.Exception
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.AsyncDelegateUICommand_ErrorCreatingCancelCommand)
                End Try
                Return CmdInfo
            End Function
            
            ''' <summary> Checks if the running task could be cancelled. </summary>
             ''' <returns> Boolean </returns>
            Private Function CanCancelTask() As Boolean
                Dim RetValue  As Boolean = False
                Try
                    If (Me.IsBusy AndAlso Me.IsCancellationSupported AndAlso (CmdTaskCancelTokenSource IsNot Nothing) AndAlso (CancelTaskCommandInfo.ExecuteAction IsNot Nothing)) Then
                        
                        If (Not Me.IsAsync) Then
                            
                            RetValue = (Not CmdTaskCancelTokenSource.IsCancellationRequested)
                            
                        ElseIf (CmdTask IsNot Nothing) Then
                            Dim TaskNotFinished  As Boolean = False
                            
                            Select Case CmdTask.Status
                                
                                Case TaskStatus.Created, TaskStatus.Running, TaskStatus.WaitingForActivation, 
                                     TaskStatus.WaitingForChildrenToComplete, TaskStatus.WaitingToRun
                                     
                                     TaskNotFinished = True
                            End Select
                            
                            RetValue = (TaskNotFinished AndAlso (Not CmdTaskCancelTokenSource.IsCancellationRequested))
                        End If
                    End If
                    
                Catch ex As System.Exception
                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                End Try
                Return RetValue
            End Function
            
            ''' <summary> Requests cancellation of running task. </summary>
            Private Sub cancelTask()
                Try
                    If (CmdTaskCancelTokenSource IsNot Nothing) Then
                        CmdTaskCancelTokenSource.Cancel()
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.AsyncDelegateUICommand_ErrorCancellingTask)
                End Try
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ' ''' <summary> Finish completed task (change decoration and busy status). </summary>
            ' Private Sub finishTaskInCurrentThread(FinishedTask As Task)
            '     #If DEBUG Then
            '         If (System.Windows.Application.Current IsNot Nothing) Then Debug.Print("AsyncDelegateUICommand \ finishTaskInCurrentThread: WPF UI thread ID  = " & System.Windows.Application.Current.Dispatcher.Thread.ManagedThreadId.ToString())
            '         Debug.Print("AsyncDelegateUICommand \ finishTaskInCurrentThread: Current thread ID = " & Dispatcher.CurrentDispatcher.Thread.ManagedThreadId.ToString())
            '     #End If
            '     Dispatcher.CurrentDispatcher.Invoke(New Action(Of Task)(AddressOf finishTask), {FinishedTask})
            ' End Sub
            
            ''' <summary> Finish completed task (change decoration and busy status). </summary>
            Private Sub finishTask(FinishedTask As Task)
                Try
                    Me.Decoration = TargetCommandInfo.Decoration
                    
                    _IsBusy = False
                    Me.OnPropertyChanged("IsBusy")
                    
                    RaiseCanExecuteChanged()
                    
                    #If DEBUG Then
                        If (System.Windows.Application.Current IsNot Nothing) Then Debug.Print("AsyncDelegateUICommand \ finishTask: WPF UI thread ID  = " & System.Windows.Application.Current.Dispatcher.Thread.ManagedThreadId.ToString())
                        Debug.Print("AsyncDelegateUICommand \ finishTask: Current thread ID = " & Dispatcher.CurrentDispatcher.Thread.ManagedThreadId.ToString())
                    #End If
                    
                    If (FinishedTask IsNot Nothing) Then
                        If (FinishedTask.Exception IsNot Nothing) Then
                            'For Each ex As Exception In FinishedTask.Exception.InnerExceptions
                            '    Throw ex
                            'Next
                            'FinishedTask.Exception.Handle(Function(x)
                            '                                   Return True
                            '                              End Function)
                            Logger.logError(FinishedTask.Exception.Flatten(), StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AsyncDelegateUICommand_AsyncExecuteFailed, Me.TargetCommandInfo.Decoration.Caption))
                        End If
                    End If
                    
                Catch ex As System.Exception
                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.AsyncDelegateUICommand_TaskFinishingFailed, Me.TargetCommandInfo.Decoration.Caption))
                End Try
            End Sub
            
        #End Region
    
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
