
Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Input
Imports System.Windows.Media.Imaging


Namespace UI.ViewModel
    
    ''' <summary> A Delegate Command for use in UI elements, that executes an action in a separate thread. </summary>
     ''' <remarks>
     ''' <para>
     ''' While the real action is executing, this class changes it's provided command 
     ''' to a cancel action, which can signal a cancel request to the threaded action.
     ''' </para>
     ''' <para>
     ''' The target command has to be packed as <see cref="DelegateUICommandInfo"/> and passed to the constructor.
     ''' </para>
     ''' </remarks>
    Public NotInheritable Class RelayAsyncCommand
        Implements ICommand
        Implements INotifyPropertyChanged
        
        #Region "Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UI.ViewModel.RelayAsyncCommand")
            
            Public ReadOnly TargetCommandInfo       As DelegateUICommandInfo(Of Object) = Nothing
            Public ReadOnly CancelCallback          As Action = Nothing
            
            Private CancelTaskCommandInfo           As DelegateUICommandInfo = Nothing
            Private CmdTask                         As Task = Nothing
            Private CmdTaskCancelTokenSource        As CancellationTokenSource = Nothing
            
            Private _ThrowOnInvalidPropertyName     As Boolean = False
            Private _IsCancellationSupported        As Boolean = False
            Private _IsBusy                         As Boolean = False
            Private _Decoration                     As UICommandDecoration = Nothing
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new asyncronous command that supports cancellation. </summary>
             ''' <param name="TargetCommand"> The target <see cref="DelegateUICommandInfo(Of System.Threading.CancellationToken)" /> for the desired action. </param>
            Public Sub New(TargetCommand As DelegateUICommandInfo(Of Object))
                Me.New(TargetCommand, Nothing, True)
            End Sub
            
            ''' <summary> Creates a new asyncronous command. </summary>
             ''' <param name="TargetCommand">        The target <see cref="DelegateUICommandInfo(Of System.Threading.CancellationToken)" /> for the desired action. </param>
             ''' <param name="SupportsCancellation"> Should be False, when the target action doesn't worry about cancellation requests. </param>
            Public Sub New(TargetCommand As DelegateUICommandInfo(Of Object), SupportsCancellation As Boolean)
                Me.New(TargetCommand, Nothing, SupportsCancellation)
            End Sub
            
            ''' <summary> Creates a new asyncronous command. </summary>
             ''' <param name="TargetCommand">        The target <see cref="DelegateUICommandInfo(Of System.Threading.CancellationToken)" /> for the desired action. </param>
             ''' <param name="SupportsCancellation"> Should be False, when the target action doesn't worry about cancellation requests. </param>
            Public Sub New(TargetCommand As DelegateUICommandInfo(Of Object), CancelCallback As Action, SupportsCancellation As Boolean)
                
                If (TargetCommand Is Nothing) Then
                    Throw New ArgumentNullException("TargetCommand", "TargetCommand can not be null")
                End If
                
                TargetCommandInfo = TargetCommand
                Me.CancelCallback = CancelCallback
                
                _IsCancellationSupported = SupportsCancellation
                Me.OnPropertyChanged("IsCancellationSupported")
                
                Me.Decoration = TargetCommandInfo.Decoration
                CancelTaskCommandInfo = createCancelTaskCommandInfo()
                'CommandManager.InvalidateRequerySuggested()
            End Sub
            
        #End Region
        
        #Region "Properties" 
            
            ''' <summary> Gets or sets the Command's UI Decoration information. </summary>
            Public Property Decoration As UICommandDecoration
                Get
                    Return _Decoration
                End Get
                Set(value As UICommandDecoration)
                    _Decoration = value
                    Me.OnPropertyChanged("Decoration")
                End Set
            End Property
            
            
            '' <summary> Returns the info of the target command. </summary>
            'Public ReadOnly Property TargetCommandInfo() As DelegateUICommandInfo(Of Object)
            '    Get
            '        Return TargetCommandInfo
            '    End Get
            'End Property
            '
            '' <summary> Returns the info of the command that cancels the task. </summary>
            'Public ReadOnly Property CancelTaskCommandInfo() As DelegateUICommandInfo
            '    Get
            '        Return CancelTaskCommandInfo
            '    End Get
            'End Property
            
            ''' <summary> Returns True if the target command is currently executed. </summary>
            Public ReadOnly Property IsBusy() As Boolean
                Get
                    Return _IsBusy
                End Get
            End Property
            
            ''' <summary> Returns whether or not cancellation of the running target action is supported. </summary>
            Public ReadOnly Property IsCancellationSupported() As Boolean
                Get
                    Return _IsCancellationSupported
                End Get
            End Property
            
        #End Region
        
        #Region "ICommand Members"
            
            ''' <summary>
            ''' See <a href="http://msdn.microsoft.com/de-de/library/system.windows.input.icommand.aspx" target="_blank"> System.Windows.Input.ICommand </a>
            ''' </summary>
            Public Custom Event CanExecuteChanged As EventHandler Implements System.Windows.Input.ICommand.CanExecuteChanged
                
                AddHandler(ByVal value As EventHandler)
                    AddHandler CommandManager.RequerySuggested, value
                End AddHandler
                
                RemoveHandler(ByVal value As EventHandler)
                    RemoveHandler CommandManager.RequerySuggested, value
                End RemoveHandler
                
                RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)
                    CommandManager.InvalidateRequerySuggested()
                End RaiseEvent
            End Event
            
            ''' <summary>
            ''' See <a href="http://msdn.microsoft.com/de-de/library/system.windows.input.icommand.aspx" target="_blank"> System.Windows.Input.ICommand </a>
            ''' </summary>
             ''' <param name="parameter"> data for the command </param>
             ''' <returns> Boolean </returns>
            Public Function CanExecute(ByVal parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute
                Dim RetValue  As Boolean = True
                If (Me.IsBusy AndAlso (CancelTaskCommandInfo.CanExecutePredicate IsNot Nothing)) Then
                    RetValue = CancelTaskCommandInfo.CanExecutePredicate.Invoke()
                ElseIf ((Not Me.IsBusy) AndAlso (TargetCommandInfo.CanExecutePredicate IsNot Nothing)) Then
                    RetValue = TargetCommandInfo.CanExecutePredicate.Invoke(Nothing)
                End If
                Return RetValue
            End Function
            
            ''' <summary>
            ''' See <a href="http://msdn.microsoft.com/de-de/library/system.windows.input.icommand.aspx" target="_blank"> System.Windows.Input.ICommand </a>
            ''' </summary>
             ''' <param name="parameter"> data for the command </param>
            Public Sub Execute(ByVal parameter As Object) Implements System.Windows.Input.ICommand.Execute
                Try
                    If (Me.IsBusy) Then
                        ' Cancel the task.
                        If (Me.IsCancellationSupported AndAlso (CancelTaskCommandInfo.ExecuteAction IsNot Nothing)) Then
                            CancelTaskCommandInfo.ExecuteAction()
                        End If
                    Else
                        ' Start the task.
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
                            
                            ' Create and start task and register a continuation Callback
                            CmdTask = Task.Factory.StartNew(TargetCommandInfo.ExecuteAction, CmdTaskCancelToken, CmdTaskCancelToken)
                            CmdTask.ContinueWith(AddressOf finishTask)
                        End If
                    End If
                    
                    CommandManager.InvalidateRequerySuggested()
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "Execute(): Fehler beim Starten des Task.")
                End Try
                
            End Sub
            
        #End Region
        
        #Region "INotifyPropertyChanged Members"
            
            ''' <summary>  Raised when a property on this object has a new value. </summary>
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            
            ''' <summary> Raises this object's PropertyChanged event. </summary>
             ''' <param name="propertyName"> The property that has a new value. </param>
            Private Sub OnPropertyChanged(ByVal propertyName As String)
                Me.VerifyPropertyName(propertyName)
                
                Dim handler As PropertyChangedEventHandler = Me.PropertyChangedEvent
                If handler IsNot Nothing Then
                    handler.Invoke(Me, New PropertyChangedEventArgs(propertyName))
                End If
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
                If ((From pi As System.Reflection.PropertyInfo In MyClass.GetType.GetProperties() Where pi.Name = propertyName).Count < 1) Then
                    Dim msg As String = "Invalid property name: " & propertyName
                    
                    If Me.ThrowOnInvalidPropertyName Then
                        Throw New Exception(msg)
                    Else
                        Debug.Fail(msg)
                    End If
                End If
            End Sub
            
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
                    Logger.logError(ex, "CancelTask(): Fehler beim Erzeugen des Abbruch-Befehls.")
                End Try
                Return CmdInfo
            End Function
            
            ''' <summary> Checks if the running task could be cancelled. </summary>
             ''' <returns> Boolean </returns>
            Private Function CanCancelTask() As Boolean
                Dim RetValue  As Boolean = False
                Try
                    If (Me.IsCancellationSupported AndAlso (CmdTask IsNot Nothing) AndAlso (CmdTaskCancelTokenSource IsNot Nothing)) Then
                        Dim TaskNotFinished  As Boolean = False
                        
                        Select Case CmdTask.Status
                            
                            Case TaskStatus.Created, TaskStatus.Running, TaskStatus.WaitingForActivation, 
                                 TaskStatus.WaitingForChildrenToComplete, TaskStatus.WaitingToRun
                                 
                                TaskNotFinished = True
                        End Select
                        
                        RetValue = (TaskNotFinished And (Not CmdTaskCancelTokenSource.IsCancellationRequested))
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, "CanCancelTask(): unerwateter Fehler.")
                    'Debug.Fail("CanCancelTask(): unerwateter Fehler.")
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
                    Logger.logError(ex, "cancelTask(): Fehler beim Abbruch des Task.")
                End Try
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Finish completed task (change decoration and busy status). </summary>
            Private Sub finishTask()
                Try
                    Me.Decoration = TargetCommandInfo.Decoration
                    
                    _IsBusy = False
                    Me.OnPropertyChanged("IsBusy")
                    
                    CommandManager.InvalidateRequerySuggested()
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "finishTask(): Fehler bei Anschlussbearbeitung nach Task.")
                End Try
            End Sub
            
        #End Region
    
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
