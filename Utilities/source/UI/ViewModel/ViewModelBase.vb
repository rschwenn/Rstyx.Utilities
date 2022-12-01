
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq
Imports System.Threading
Imports System.Windows.Threading

Imports Rstyx.Utilities
Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.Validation

Namespace UI.ViewModel
    
    ''' <summary> Abstact base class for ViewModels. </summary>
     ''' <remarks>
     ''' Adds the following features to the Cinch.ViewModelBase class:
     ''' <list type="bullet">
     ''' <item><description> Implements <see cref="Cinch.IViewStatusAwareInjectionAware"/> with corresponding overridable methods <c>OnViewLoaded()</c> and <c>OnViewUnloaded()</c>. </description></item>
     ''' <item><description> Implements <see cref="IStatusIndicator"/>. Thus provides status indicator properties <c>StatusText</c> and <c>Progress</c> (intended for binding to status bar). </description></item>
     ''' <item><description> <c>QuietMode</c> property. It's intended to signal that the view model isn't monitoring events. </description></item>
     ''' <item><description> <c>CloseViewCommand</c> property: Provides an UI Command which raises the Cinch.ViewModelBase.CloseRequest event. </description></item>
     ''' <item><description> <c>ShowHelpFileCommand</c> property: Provides an UI Command that calls <c>showHelpFile()</c> which has to be overridden to show the help file. </description></item>
     ''' <item><description> A long display name. </description></item>
     ''' <item><description> UIValidationError*** properties and methods for tracking Binding exceptions (via View). </description></item>
     ''' <item><description> Detect Changes of My.Settings and raises <c>PropertyChanged()</c> for a corresponding local property. </description></item>
     ''' <item><description> <c>RaiseCanExecuteChangedForAll</c> method forces status update of all commands that are available via view model properties. </description></item>
     ''' <item><description> <c>TrySetProperty</c> methods for safely setting a property by checking it against a provided list of valid values. </description></item>
     ''' </list>
     ''' </remarks>
    Public MustInherit Class ViewModelBase
        Inherits   Cinch.ValidatingViewModelBase
        Implements IStatusIndicator
        Implements Cinch.IViewStatusAwareInjectionAware
        
        #Region "Fields"
            
            Private ReadOnly Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.UI.ViewModel.ViewModelBase")
            
            'Protected ReadOnly DeferredDoEventsAction   As Rstyx.Utilities.DeferredAction = New Rstyx.Utilities.DeferredAction(AddressOf DoEventsIfWpfUiThread, System.Windows.Threading.Dispatcher.CurrentDispatcher)
            'Protected ReadOnly DoEventsDelay            As System.TimeSpan = System.TimeSpan.FromMilliseconds(250)
            
        #End Region
        
        #Region "Initializing and Finalizing"
            
            Protected Sub New()
                MyBase.New()
                
                MyBase.DisplayName = "** DisplayName not set **"
                
                _WpfUiDispatcher = Dispatcher.CurrentDispatcher
                WpfUiThreadID    = _WpfUiDispatcher.Thread.ManagedThreadId
                
                'Subscribe for notifications of user settings changes.
                Dim WeakPropertyChangedListener As Cinch.WeakEventProxy(Of PropertyChangedEventArgs) = New Cinch.WeakEventProxy(Of PropertyChangedEventArgs)(AddressOf OnUserSettingsChanged)
                AddHandler My.Settings.PropertyChanged, AddressOf WeakPropertyChangedListener.Handler
            End Sub
            
        #End Region
        
        #Region "DisplayName"
            
            Private _DisplayNameLong    As String = Nothing
            
            ''' <summary> The user-friendly long name of this object. If not set the short name is returned. </summary>
            Public Overridable Property DisplayNameLong() As String
                Get
                    If (_DisplayNameLong is Nothing) Then
                        _DisplayNameLong = MyBase.DisplayName
                    End If
                    Return _DisplayNameLong
                End Get
                Set(ByVal value As String)
                    If (Not (value =_DisplayNameLong)) Then
                        _DisplayNameLong = value
                        MyBase.NotifyPropertyChanged("DisplayNameLong")
                    End If
                End Set
            End Property
            
        #End Region
        
        #Region "ViewModel Activity Status"
            
            Private _QuietMode  As Boolean = False
            
            ''' <summary> Indicates wheter or not the view model is in "quiet mode" (monitoring events is suspended). </summary>
            Public Property QuietMode() As Boolean
                Get
                    QuietMode = _QuietMode
                End Get
                Set(value As Boolean)
                    If (value Xor _QuietMode) Then
                        _QuietMode = value
                        MyBase.NotifyPropertyChanged("QuietMode")
                    End if
                End Set
            End Property
            
        #End Region
        
        #Region "WPF Dispatcher related"
            
            ''' <summary> Provides quick access to the ID of the tread of <see cref="WpfUiDispatcher"/>. </summary>
            Protected WpfUiThreadID     As Integer
            
            ''' <summary> Backend field of  of <see cref="WpfUiDispatcher"/>. </summary>
            Protected _WpfUiDispatcher  As Dispatcher
            
            ''' <summary> Returns the dispatcher of WPF UI thread. </summary>
             ''' <returns> The dispatcher of WPF UI thread. </returns>
             ''' <remarks>
             ''' <para>
             ''' After construction of this view model the returned dispatcher is the one which has dispached
             ''' the construction of this view model, which usually should be the dispatcher of WPF UI thread.
             ''' </para>
             ''' <para>
             ''' Every call to <see cref="InitialiseViewAwareService"/> resets this property to the dispatcher
             ''' of <see cref="ViewAwareStatusService"/><c>.View</c> to be really sure ;-)
             ''' </para>
             ''' </remarks>
            Public ReadOnly Property WpfUiDispatcher() As Dispatcher
                Get
                    Return _WpfUiDispatcher
                End Get
            End Property
            
            ''' <summary>
            ''' Forces the WPF message pump to process all enqueued messages that are <c>DispatcherPriority.Background</c> or above
            ''' if the calling thread is the WPF UI thread (the thread created this view model).
            ''' </summary>
            Protected Overridable Sub DoEventsIfWpfUiThread()
                If (Thread.CurrentThread.ManagedThreadId = WpfUiThreadID) Then
                    Try
                        Cinch.ApplicationHelper.DoEvents()
                    Catch ex As System.Exception
                        Logger.logDebug(ex.ToString())
                        Logger.logDebug(Rstyx.Utilities.Resources.Messages.Global_DoEventsFailed)
                    End Try
                End If
            End Sub
            
        #End Region
        
        #Region "Commands"
            
            #Region "Command Support"
                
                ''' <summary> Calls the <c>RaiseCanExecuteChanged</c> method for all <c>ICommands</c> that are available via view model properties (and that provide this method). </summary>
                 ''' <remarks> Since the command properties are not all read-only, they have to be re-queried every time. </remarks>
                Protected Sub RaiseCanExecuteChangedForAll()
                    Try
                        Dim Properties()        As System.Reflection.PropertyInfo = MyClass.GetType().GetProperties()
                        Dim CommandProperties   As IEnumerable(Of System.Reflection.PropertyInfo) = 
                                                   (From pi In Properties Where pi.PropertyType.IsImplementing(GetType(System.Windows.Input.ICommand)))
                        
                        For Each CommandProperty As System.Reflection.PropertyInfo In CommandProperties
                            Try
                                Dim Command As System.Windows.Input.ICommand = CType(CommandProperty.GetValue(Me, Nothing), System.Windows.Input.ICommand)
                                Dim CommandRaiseMethod As System.Reflection.MethodInfo = CommandProperty.PropertyType.GetMethod("RaiseCanExecuteChanged")
                                
                                If (CommandRaiseMethod IsNot Nothing) Then
                                    CommandRaiseMethod.Invoke(Command, Nothing)
                                End If
                                
                            Catch ex As Exception
                                ' Gets here if CommandRaiseMethod is ambiguous.
                                System.Diagnostics.Trace.WriteLine(ex)
                                System.Diagnostics.Trace.WriteLine("RaiseCanExecuteChangedForAll(): 'CommandRaiseMethod' isn't unique, or error in event handling.")
                            End Try
                        Next
                    Catch ex As System.Exception
                        Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                    End Try
                End Sub
                
            #End Region
            
            #Region "CloseView Command"
                
                Private _CloseViewCommand   As DelegateUICommand = Nothing
                
                ''' <summary> Provides an UI Command which raises the <c>Cinch.ViewModelBase.CloseRequest</c> event. </summary>
                Public ReadOnly Property CloseViewCommand() As DelegateUICommand
                    Get
                        Try
                            If (_CloseViewCommand Is Nothing) Then
                                
                                Dim Decoration As New UICommandDecoration()
                                Decoration.Caption     = "Schließen"
                                Decoration.Description = "Fenster schließen"
                                Decoration.IconBrush   = UI.Resources.UIResources.IconBrush("Handmade_Power4")
                                
                                _CloseViewCommand = New DelegateUICommand(AddressOf Me.CloseView, Decoration)
                            End If
                        Catch ex As System.Exception
                            Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_ErrorCreatingCommandIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                        End Try
                        
                        Return _CloseViewCommand
                    End Get
                End Property
                
                ''' <summary> Raises the <c>Cinch.ViewModelBase.CloseRequest</c> event. </summary>
                Private Sub CloseView()
                    Try
                        MyBase.RaiseCloseRequest(Nothing)
                    Catch ex As System.Exception
                        Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
                    End Try
                End Sub
                
            #End Region
            
            #Region "ShowHelpFile Command"
                
                Private _ShowHelpFileCommand    As DelegateUICommand = Nothing
                
                ''' <summary> Provides an UI Command that calls <c>showHelpFile()</c> which has to be overridden to show the help file. </summary>
                Public ReadOnly Property ShowHelpFileCommand() As DelegateUICommand
                    Get
                        Try
                            If (_ShowHelpFileCommand Is Nothing) Then
                                
                                Dim Decoration As New UICommandDecoration()
                                Decoration.Caption     = "Hilfe"
                                Decoration.Description = "Hilfedatei anzeigen"
                                Decoration.IconBrush   = UI.Resources.UIResources.IconBrush("Tango_Help1")
                                
                                _ShowHelpFileCommand = New DelegateUICommand(AddressOf Me.ShowHelpFile, Decoration)
                            End If
                        Catch ex As System.Exception
                            Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_ErrorCreatingCommandIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                        End Try
                        
                        Return _ShowHelpFileCommand
                    End Get
                End Property
                
                ''' <summary> Shows the help file if overridden in a derived class. </summary>
                ''' <remarks> i.e.:  Rstyx.Microstation.Utilities.FileUtils.showHelpFile(HelpFileName) </remarks>
                Protected Overridable Sub ShowHelpFile()
                    Logger.logError(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_NotImplemented, Me.GetType().FullName & "/showHelpFile"))
                End Sub
                
            #End Region
            
        #End Region
        
        #Region "Cinch.IViewStatusAwareInjectionAware Members"
            
            ''' <summary> After <c>InitialiseViewAwareService</c> has been called successful it holds a <see cref="Cinch.IViewAwareStatus"/> object for view connection. </summary>
            Protected ViewAwareStatusService  As Cinch.IViewAwareStatus = Nothing
            
            ''' <summary> Gets the ViewAwareStatusService injected and does default wiring up. </summary>
             ''' <param name="viewAwareStatusService"> The view service object. </param>
            Public Sub InitialiseViewAwareService(viewAwareStatusService As Cinch.IViewAwareStatus) Implements Cinch.IViewStatusAwareInjectionAware.InitialiseViewAwareService
                
                Me.ViewAwareStatusService = viewAwareStatusService
                
                If (Me.ViewAwareStatusService IsNot Nothing) Then
                    
                    ' Store link to WPF UI dispatcher.
                    If (Me.ViewAwareStatusService.ViewsDispatcher IsNot Nothing) Then
                        _WpfUiDispatcher = Me.ViewAwareStatusService.ViewsDispatcher
                        WpfUiThreadID    = _WpfUiDispatcher.Thread.ManagedThreadId
                    End If
                    
                    ' Subscribe to weak events.
                    AddHandler Me.ViewAwareStatusService.ViewLoaded,   AddressOf OnViewLoaded
                    AddHandler Me.ViewAwareStatusService.ViewUnloaded, AddressOf OnViewUnloaded
                End If
            End Sub
            
            ''' <summary> Response to loading the View (provided ViewAwareStatusService has been initialized). </summary>
            Protected Overridable Sub OnViewLoaded()
            End Sub
            
            ''' <summary> Response to unloading the View (provided ViewAwareStatusService has been initialized). </summary>
            Protected Overridable Sub OnViewUnloaded()
            End Sub
            
        #End Region
        
        #Region "IDisposable Members"
            
            ''' <summary> Hooks for <see cref="Cinch.ViewModelBase.Dispose"/>. </summary>
            Protected Overrides Sub OnDispose()
                ' Release timer resources.
                DeferredResetStateAction.Dispose()
                DeferredSetProgressAction.Dispose()
                
                ' All used events should be weak, but ...
                If (Me.ViewAwareStatusService IsNot Nothing) Then
                    RemoveHandler Me.ViewAwareStatusService.ViewLoaded,   AddressOf OnViewLoaded
                    RemoveHandler Me.ViewAwareStatusService.ViewUnloaded, AddressOf OnViewUnloaded
                End If
                
                ' There's no reference to "WeakPropertyChangedListener" => should be o.k.
                'RemoveHandler My.Settings.PropertyChanged, AddressOf WeakPropertyChangedListener.Handler
            End Sub
            
        #End Region
        
        #Region "IStatusIndicator Members"
            
            Private _Progress                           As Double  = 0
            Private _IsInProgress                       As Boolean = False
            Private _StatusText                         As String  = String.Empty
            Private _StatusTextDefault                  As String  = String.Empty
            Private _StatusTextToolTip                  As String  = Nothing
            Private _StatusTextToolTipDefault           As String  = Nothing
            
            Private NextProgressThreshold               As Double  = 0
            Private ReadOnly VisibleProgressStepCount   As Double  = 90
            
            Private ReadOnly DeferredSetProgressAction  As Rstyx.Utilities.DeferredAction = New Rstyx.Utilities.DeferredAction(AddressOf SetProgressTo100, System.Windows.Threading.Dispatcher.CurrentDispatcher)
            Private ReadOnly SetProgressDelay           As System.TimeSpan = System.TimeSpan.FromMilliseconds(300)
            
            Private ReadOnly DeferredResetStateAction   As Rstyx.Utilities.DeferredAction = New Rstyx.Utilities.DeferredAction(AddressOf resetStateIndication, System.Windows.Threading.Dispatcher.CurrentDispatcher)
            
            Private Sub SetProgressTo100()
                _Progress = 100
                NextProgressThreshold = 0
                MyBase.NotifyPropertyChanged("Progress")
            End Sub
            
            ''' <summary> <see langword="true"/> signals "work in progress" or "busy". </summary>
             ''' <remarks> This is intended to use when a discrete progress value is unknown. </remarks>
            Public Property IsInProgress() As Boolean Implements IStatusIndicator.IsInProgress
                Get 
                    Return _IsInProgress
                End Get
                Set(value As Boolean)
                    If (Value Xor _IsInProgress) Then
                        _IsInProgress = Value
                        MyBase.NotifyPropertyChanged("IsInProgress")
                        DoEventsIfWpfUiThread()
                        'DeferredDoEventsAction.Defer(DoEventsDelay)
                    End If
                End Set
            End Property
            
            
            ''' <summary> The progress in percent. </summary>
             ''' <remarks>
             ''' <para>
             ''' Setting the value to 100 is two-staged: a) set to 99.8, b) delayed set to 100.
             ''' </para>
             ''' <para>
             ''' Property changes are only propagated when the value reaches discrete values (90 times).
             ''' </para>
             ''' <para>
             ''' In order to reflect value change on UI, <c>Cinch.ApplicationHelper.DoEvents()</c> is called.
             ''' </para>
             ''' </remarks>
            Public Property Progress() As Double Implements IStatusIndicator.Progress
                Get
                    Return _Progress
                End Get
                Set(value As Double)
                    If (Not (value = _Progress)) Then
                        
                        ' Set value.
                        If (value < 0) Then
                            _Progress = 0.0
                        ElseIf (value < 100) Then
                            _Progress = value
                        Else
                            _Progress = 99.8
                            DeferredSetProgressAction.Defer(SetProgressDelay)
                        End if
                        
                        ' Notify about change: only at discrete values.
                        If (_Progress >= NextProgressThreshold) Then
                            MyBase.NotifyPropertyChanged("Progress")
                            DoEventsIfWpfUiThread()
                            'DeferredDoEventsAction.Defer(DoEventsDelay)
                            NextProgressThreshold += 100 / VisibleProgressStepCount
                        End if
                    End if
                End Set
            End Property
            
            ''' <summary> The progress range in percent which is currently used for <see cref="ProgressTick"/>. Defaults to 100. </summary>
            Public Property ProgressTickRange() As Double Implements IStatusIndicator.ProgressTickRange
            
            ''' <summary> The count of ticks matching <see cref="ProgressTickRange"/>. Defaults to 100. </summary>
            Public Property ProgressTickRangeCount() As Double Implements IStatusIndicator.ProgressTickRangeCount
            
            ''' <summary> Increases <see cref="Progress"/> by <see cref="ProgressTickRange"/> divided by <see cref="ProgressTickRangeCount"/>. </summary>
            Public Sub ProgressTick() Implements IStatusIndicator.ProgressTick
                Me.Progress += Me.ProgressTickRange / Me.ProgressTickRangeCount
            End Sub
            
            
            ''' <summary> A status text (i.e for displaying in status bar). </summary>
             ''' <remarks> In order to reflect value change on UI, <c>Cinch.ApplicationHelper.DoEvents()</c> is called. </remarks>
            Public Property StatusText() As String Implements IStatusIndicator.StatusText
                Get
                    Return _StatusText
                End Get
                Set(value As String)
                    If (Not (value = _StatusText)) Then
                        _StatusText = value
                        _StatusTextToolTip = Nothing
                        MyBase.NotifyPropertyChanged("StatusText")
                        MyBase.NotifyPropertyChanged("StatusTextToolTip")
                        DoEventsIfWpfUiThread()
                        'DeferredDoEventsAction.Defer(DoEventsDelay)
                    End if
                End Set
            End Property
            
            ''' <summary> A default value for <see cref="StatusText"/>. Deaults to String.Empty. </summary>
             ''' <remarks> <see cref="resetStateIndication"/>() sets <see cref="StatusText"/> to this default value. </remarks>
            Public Property StatusTextDefault() As String Implements IStatusIndicator.StatusTextDefault
                Get
                    Return _StatusTextDefault
                End Get
                Set(value As String)
                    If (Not (value = _StatusTextDefault)) Then
                        Dim forward As Boolean = ((Me.StatusText.IsEmptyOrWhiteSpace()) OrElse (Me.StatusText = _StatusTextDefault))
                        _StatusTextDefault = value
                        If (forward) Then Me.StatusText = _StatusTextDefault
                    End if
                End Set
            End Property
            
            ''' <summary> A tool tip for status text (i.e for displaying more text than fits in status bar). </summary>
             ''' <remarks>
             ''' <para>
             ''' HINT: Every time <see cref="StatusText"/> is set, this property is set to <see langword="null"/>!
             ''' </para>
             ''' <para>
             ''' In order to reflect value change on UI, <c>Cinch.ApplicationHelper.DoEvents()</c> is called.
             ''' </para>
             ''' </remarks>
            Public Property StatusTextToolTip() As String Implements IStatusIndicator.StatusTextToolTip
                Get
                    Return _StatusTextToolTip
                End Get
                Set(value As String)
                    If (Not (value = _StatusTextToolTip)) Then
                        _StatusTextToolTip = value
                        MyBase.NotifyPropertyChanged("StatusTextToolTip")
                        DoEventsIfWpfUiThread()
                        'DeferredDoEventsAction.Defer(DoEventsDelay)
                    End if
                End Set
            End Property
            
            ''' <summary> A default value for <see cref="StatusTextToolTip"/>. Deaults to String.Empty. </summary>
             ''' <remarks> <see cref="resetStateIndication"/>() sets <see cref="StatusTextToolTip"/> to this default value. </remarks>
            Public Property StatusTextToolTipDefault() As String Implements IStatusIndicator.StatusTextToolTipDefault
                Get
                    Return _StatusTextToolTipDefault
                End Get
                Set(value As String)
                    If (Not (value = _StatusTextToolTipDefault)) Then
                        Dim forward As Boolean = ((Me.StatusTextToolTip.IsEmptyOrWhiteSpace()) OrElse (Me.StatusTextToolTip = _StatusTextToolTipDefault))
                        _StatusTextToolTipDefault = value
                        If (forward) Then Me.StatusTextToolTip = _StatusTextToolTipDefault
                    End if
                End Set
            End Property
            
            ''' <summary> Sets status text to <see cref="StatusTextDefault"/>, <see cref="Progress"/> to zero and <see cref="IsInProgress"/> to <see langword="false"/> (immediately). </summary>
             ''' <remarks> If there's already a delayed reset pending, it's aborted. </remarks>
            Public Sub resetStateIndication() Implements IStatusIndicator.resetStateIndication
                Debug.Print("ViewModelBase \ resetStateIndication:  Start...")
                DeferredResetStateAction.Abort()
                Me.StatusTextToolTip = Nothing
                Me.StatusText = Me.StatusTextDefault
                Me.Progress = 0
                Me.ProgressTickRange = 100
                Me.ProgressTickRangeCount = 100
                Me.IsInProgress = False
                Debug.Print("ViewModelBase \ resetStateIndication:  End.")
            End Sub
            
            ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero (after a delay), but immediately <see cref="IsInProgress"/> to <see langword="false"/>. </summary>
             ''' <param name="Delay"> Delay for reset (in Milliseconds). </param>
            Public Sub resetStateIndication(Delay As Double) Implements IStatusIndicator.resetStateIndication
                Me.IsInProgress = False
                DeferredResetStateAction.Defer(Delay)
            End Sub
            
        #End Region
        
        #Region "TrySetProperty"
            
            ''' <summary> Sets a view model's property to a given value, if this is a supported value. </summary>
             ''' <typeparam name="TProperty">           Type of the target property designated by <paramref name="PropertyName"/> and also of the <c>IList</c> items of <paramref name="SupportedValues"/>. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredValue">         Value that the target property should be set to. </param>
             ''' <param name="SupportedValues">         <c>IList</c> of supported property values (i.g. the item source of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If <see langword="true"/> and the property has really changed, this method calls <c>NotifyPropertyChanged(PropertyName)</c>. </param>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="PropertyName"/> is <see langword="null"/> or empty or whitespace. </exception>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="SupportedValues"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.MissingMemberException"> <paramref name="PropertyName"/> is not a member of this view model. </exception>
             ''' <exception cref="System.MemberAccessException">  <paramref name="PropertyName"/> is read-only. </exception>
             ''' <returns> <see langword="true"/> if <paramref name="NewDesiredValue"/> is supported (and the property may have changed). </returns>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetProperty(Of TProperty) _
                                             (PropertyName As String, _
                                              NewDesiredValue As TProperty, _
                                              ByRef SupportedValues As IList(Of TProperty), _
                                              Optional NotifyOnPropertyChanged As Boolean = False _
                                              ) As Boolean
                Dim success As Boolean = False
                
                ' Check arguments.
                If (String.IsNullOrWhiteSpace(PropertyName)) Then Throw New System.ArgumentNullException("PropertyName")
                If (SupportedValues Is Nothing) Then Throw New System.ArgumentNullException("SupportedValues")
                
                Dim TargetProperty As System.Reflection.PropertyInfo = Me.GetType().GetProperty(PropertyName, GetType(TProperty))
                If (TargetProperty Is Nothing) Then
                    Throw New System.MissingMemberException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyNotFound, PropertyName))
                End If
                If (Not TargetProperty.CanWrite) Then
                    Throw New System.MemberAccessException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyIsReadOnly, PropertyName))
                End If
                
                ' Look up the list values for the desired property value.
                If (SupportedValues.Contains(NewDesiredValue)) Then
                    
                    Dim NewPropertyValue As TProperty = NewDesiredValue
                    Dim OldPropertyValue As TProperty = CType(TargetProperty.GetValue(Me, Nothing), TProperty)
                    Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                    
                    If (IsDifferentValue) Then
                        TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                        If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                            MyBase.NotifyPropertyChanged(PropertyName)
                        End If
                    End If
                    success = True
                End If
                
                Return success
            End Function
            
            ''' <summary> Sets a view model's property to a given value, if this is a supported value. </summary>
             ''' <typeparam name="TProperty">           Type of the target property designated by <paramref name="PropertyName"/>. </typeparam>
             ''' <typeparam name="TCollectionItem">     Type of the <c>KeyedCollection</c> items of <paramref name="SupportedValues"/>. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredValue">         Value that the target property should be set to. </param>
             ''' <param name="SupportedValues">         <c>KeyedCollection</c> where the keys are the supported property values (i.g. the item source of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If <see langword="true"/> and the property has really changed, this method calls <c>NotifyPropertyChanged(PropertyName)</c>. </param>
             ''' <returns> <see langword="true"/> if <paramref name="NewDesiredValue"/> is supported (and the property may have changed). </returns>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="PropertyName"/> is <see langword="null"/> or empty or whitespace. </exception>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="SupportedValues"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.MissingMemberException"> <paramref name="PropertyName"/> is not a member of this view model. </exception>
             ''' <exception cref="System.MemberAccessException">  <paramref name="PropertyName"/> is read-only. </exception>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetProperty(Of TProperty, TCollectionItem) _
                                             (PropertyName As String, _
                                              NewDesiredValue As TProperty, _
                                              ByRef SupportedValues As KeyedCollection(Of TProperty, TCollectionItem), _
                                              Optional NotifyOnPropertyChanged As Boolean = False _
                                              ) As Boolean
                Dim success As Boolean = False
                
                ' Check arguments.
                If (String.IsNullOrWhiteSpace(PropertyName)) Then Throw New System.ArgumentNullException("PropertyName")
                If (SupportedValues Is Nothing) Then Throw New System.ArgumentNullException("SupportedValues")
                
                Dim TargetProperty As System.Reflection.PropertyInfo = Me.GetType().GetProperty(PropertyName, GetType(TProperty))
                If (TargetProperty Is Nothing) Then
                    Throw New System.MissingMemberException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyNotFound, PropertyName))
                End If
                If (Not TargetProperty.CanWrite) Then
                    Throw New System.MemberAccessException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyIsReadOnly, PropertyName))
                End If
                
                ' Look up the keys for the desired property value.
                If (SupportedValues.Contains(NewDesiredValue)) Then
                    
                    Dim NewPropertyValue As TProperty = NewDesiredValue
                    Dim OldPropertyValue As TProperty = CType(TargetProperty.GetValue(Me, Nothing), TProperty)
                    Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                    
                    If (IsDifferentValue) Then
                        TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                        If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                            MyBase.NotifyPropertyChanged(PropertyName)
                        End If
                    End If
                    success = True
                End If
                
                Return success
            End Function
            
            ''' <summary> Sets a view model's property to a given value, if this is a supported value. </summary>
             ''' <typeparam name="TProperty">           Type of the target property designated by <paramref name="PropertyName"/>. </typeparam>
             ''' <typeparam name="TCollectionItem">     Type of the <c>IDictionary</c> items of <paramref name="SupportedValues"/>. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredValue">         Value that the target property should be set to. </param>
             ''' <param name="SupportedValues">         <c>IDictionary</c> where the keys are the supported property values (i.g. the item source of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If <see langword="true"/> and the property has really changed, this method calls <c>NotifyPropertyChanged(PropertyName)</c>. </param>
             ''' <returns> <see langword="true"/> if <paramref name="NewDesiredValue"/> is supported (and the property may have changed). </returns>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="PropertyName"/> is <see langword="null"/> or empty or whitespace. </exception>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="SupportedValues"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.MissingMemberException"> <paramref name="PropertyName"/> is not a member of this view model. </exception>
             ''' <exception cref="System.MemberAccessException">  <paramref name="PropertyName"/> is read-only. </exception>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetProperty(Of TProperty, TCollectionItem) _
                                             (PropertyName As String, _
                                              NewDesiredValue As TProperty, _
                                              ByRef SupportedValues As IDictionary(Of TProperty, TCollectionItem), _
                                              Optional NotifyOnPropertyChanged As Boolean = False _
                                              ) As Boolean
                Dim success As Boolean = False
                
                ' Check arguments.
                If (String.IsNullOrWhiteSpace(PropertyName)) Then Throw New System.ArgumentNullException("PropertyName")
                If (SupportedValues Is Nothing) Then Throw New System.ArgumentNullException("SupportedValues")
                
                Dim TargetProperty As System.Reflection.PropertyInfo = Me.GetType().GetProperty(PropertyName, GetType(TProperty))
                If (TargetProperty Is Nothing) Then
                    Throw New System.MissingMemberException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyNotFound, PropertyName))
                End If
                If (Not TargetProperty.CanWrite) Then
                    Throw New System.MemberAccessException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyIsReadOnly, PropertyName))
                End If
                
                ' Look up the dictionary keys for the desired property value.
                If (SupportedValues.ContainsKey(NewDesiredValue)) Then
                    Dim NewPropertyValue As TProperty = NewDesiredValue
                    Dim OldPropertyValue As TProperty = CType(TargetProperty.GetValue(Me, Nothing), TProperty)
                    Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                    
                    If (IsDifferentValue) Then
                        TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                        If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                            MyBase.NotifyPropertyChanged(PropertyName)
                        End If
                    End If
                    success = True
                End If
                
                Return success
            End Function
            
            ''' <summary> Tries to set a view model's property indirect by passing the value's associated display string. </summary>
             ''' <typeparam name="TProperty">           Type of the target property designated by <paramref name="PropertyName"/>. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredDisplayValue">  Display string, whose corresponding dictionary key should become the new property value. </param>
             ''' <param name="SupportedValues">         IDictionary where the keys are the supported property values and the items the display strings (of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If <see langword="true"/> and the property has really changed, this method calls <c>NotifyPropertyChanged(PropertyName)</c>. </param>
             ''' <returns> <see langword="true"/> if <paramref name="NewDesiredDisplayValue"/> is supported (and the property may have changed). </returns>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="PropertyName"/> is <see langword="null"/> or empty or whitespace. </exception>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="SupportedValues"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.MissingMemberException"> <paramref name="PropertyName"/> is not a member of this view model. </exception>
             ''' <exception cref="System.MemberAccessException">  <paramref name="PropertyName"/> is read-only. </exception>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetPropertyByDisplayString(Of TProperty) _
                                                            (PropertyName As String, _
                                                             NewDesiredDisplayValue As String, _
                                                             ByRef SupportedValues As IDictionary(Of TProperty, String), _
                                                             Optional NotifyOnPropertyChanged As Boolean = False _
                                                             ) As Boolean
                ' Check arguments.
                If (String.IsNullOrWhiteSpace(PropertyName)) Then Throw New System.ArgumentNullException("PropertyName")
                If (SupportedValues Is Nothing) Then Throw New System.ArgumentNullException("SupportedValues")
                
                Dim TargetProperty As System.Reflection.PropertyInfo = Me.GetType().GetProperty(PropertyName, GetType(TProperty))
                If (TargetProperty Is Nothing) Then
                    Throw New System.MissingMemberException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyNotFound, PropertyName))
                End If
                If (Not TargetProperty.CanWrite) Then
                    Throw New System.MemberAccessException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyIsReadOnly, PropertyName))
                End If
                
                ' Look up for the display string and get the corresponding property value
                Dim NewPropertyValue  As TProperty = Nothing
                Dim success           As Boolean   = SupportedValues.findKeyByValue(NewDesiredDisplayValue, NewPropertyValue)
                
                If (success) Then
                    Dim OldPropertyValue As TProperty = CType(TargetProperty.GetValue(Me, Nothing), TProperty)
                    Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                    
                    If (IsDifferentValue) Then
                        TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                        If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                            MyBase.NotifyPropertyChanged(PropertyName)
                        End If
                    End If
                End If
                
                Return success
            End Function
            
            ''' <summary> Tries to set a view model's property indirect by passing the value's string representation (of .ToString() method). </summary>
             ''' <typeparam name="TProperty">           Type of the target property designated by <paramref name="PropertyName"/>. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredToStringValue"> String, whose corresponding List Item should become the new property value. </param>
             ''' <param name="SupportedValues">         IList with supported property values. </param>
             ''' <param name="NotifyOnPropertyChanged"> If <see langword="true"/> and the property has really changed, this method calls <c>NotifyPropertyChanged(PropertyName)</c>. </param>
             ''' <returns> <see langword="true"/> if <paramref name="NewDesiredToStringValue"/> is a string representation of a supported property value (and the property may have changed). </returns>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="PropertyName"/> is <see langword="null"/> or empty or whitespace. </exception>
             ''' <exception cref="System.ArgumentNullException">  <paramref name="SupportedValues"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.MissingMemberException"> <paramref name="PropertyName"/> is not a member of this view model. </exception>
             ''' <exception cref="System.MemberAccessException">  <paramref name="PropertyName"/> is read-only. </exception>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetPropertyByString(Of TProperty) _
                                                     (PropertyName As String,
                                                      NewDesiredToStringValue As String,
                                                      SupportedValues As ICollection(Of TProperty),
                                                      Optional NotifyOnPropertyChanged As Boolean = False
                                                      ) As Boolean
                ' Check arguments.
                If (String.IsNullOrWhiteSpace(PropertyName)) Then Throw New System.ArgumentNullException("PropertyName")
                If (SupportedValues Is Nothing) Then Throw New System.ArgumentNullException("SupportedValues")
                
                Dim TargetProperty As System.Reflection.PropertyInfo = Me.GetType().GetProperty(PropertyName, GetType(TProperty))
                If (TargetProperty Is Nothing) Then
                    Throw New System.MissingMemberException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyNotFound, PropertyName))
                End If
                If (Not TargetProperty.CanWrite) Then
                    Throw New System.MemberAccessException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_PropertyIsReadOnly, PropertyName))
                End If
                
                ' Look up for the string and get the corresponding property value
                Dim NewPropertyValue  As TProperty = Nothing
                Dim success           As Boolean   = SupportedValues.findItemByString(NewDesiredToStringValue, NewPropertyValue)
                
                If (success) Then
                    Dim OldPropertyValue As TProperty = CType(TargetProperty.GetValue(Me, Nothing), TProperty)
                    Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                    
                    If (IsDifferentValue) Then
                        TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                        If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                            MyBase.NotifyPropertyChanged(PropertyName)
                        End If
                    End If
                End If
                
                Return success
            End Function
            
        #End Region
        
        #Region "UI Validation Errors (resp. Binding exceptions)"
            
            ' See http://karlshifflett.wordpress.com/archive/mvvm/input-validation-ui-exceptions-model-validation-errors/
            
            Private ReadOnly UIValidationErrors As New Dictionary(Of String, UIValidationError)
            
            ''' <summary> Gets the count of current UI validation errors. </summary>
             ''' <returns> Count of current UI validation errors. </returns>
             ''' <remarks>
             ''' <para>
             ''' <b>UI validation errors can be raised by:</b>
             ''' <list type="bullet">
             ''' <item><description> ExceptionValidationRule - Exception before source update (convert failed) - controlled by <c>ValidatesOnExceptions</c>. </description></item>
             ''' <item><description> DataErrorValidationRule - Source's IDataErrorInfo(bound property's name) isn't empty after source update - controlled by <c>ValidatesOnDataErrors</c>. </description></item>
             ''' <item><description> Any <c>ValidationRule</c> of <c>Binding.ValidationRules</c> . </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' Only one single error per property is maintained.
             ''' </para>
             ''' <para>
             ''' In order to work automatically, 
             ''' this requires the view to inherit from <see cref="Rstyx.Utilities.UI.Controls.UserControlBase"/>
             ''' and the binding to have it's following properties set to <see langword="true"/>:
             ''' <c>NotifyOnValidationError</c>, <c>ValidatesOnDataErrors</c> and/or <c>ValidatesOnExceptions</c>.
             ''' </para>
             ''' </remarks>
            Public ReadOnly Property UIValidationErrorCount() As Integer
                Get
                    Return UIValidationErrors.Count
                End Get
            End Property
            
            ''' <summary> Gets the messages of all current UI validation errors (one line per error). </summary>
             ''' <returns> Messages of all current UI validation errors. </returns>
             ''' <remarks>
             ''' <para>
             ''' <b>UI validation errors can be raised by:</b>
             ''' <list type="bullet">
             ''' <item><description> ExceptionValidationRule - Exception before source update (convert failed) - controlled by <c>ValidatesOnExceptions</c>. </description></item>
             ''' <item><description> DataErrorValidationRule - Source's IDataErrorInfo(bound property's name) isn't empty after source update - controlled by <c>ValidatesOnDataErrors</c>. </description></item>
             ''' <item><description> Any <c>ValidationRule</c> of <c>Binding.ValidationRules</c> . </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' Only one single error per property is maintained.
             ''' </para>
             ''' <para>
             ''' In order to work automatically, 
             ''' this requires the view to inherit from <see cref="Rstyx.Utilities.UI.Controls.UserControlBase"/>
             ''' and the binding to have it's following properties set to <see langword="true"/>:
             ''' <c>NotifyOnValidationError</c>, <c>ValidatesOnDataErrors</c> and/or <c>ValidatesOnExceptions</c>.
             ''' </para>
             ''' </remarks>
            Public ReadOnly Property UIValidationErrorUserMessages() As String
                Get
                    Dim sb As New System.Text.StringBuilder(1024)
                    
                    For Each kvp As KeyValuePair(Of String, UIValidationError) In UIValidationErrors
                        sb.AppendLine(String.Format("{0}: {1}", If(kvp.Value.Label.IsNotEmptyOrWhiteSpace(), kvp.Value.Label, kvp.Value.PropertyName), kvp.Value.ErrorMessage))
                    Next
                    
                    Return sb.ToString()
                End Get
            End Property
            
            ''' <summary> Adds or replaces an UI validation error for a certain property of this ViewModel. </summary>
             ''' <param name="e"> The UI error. </param>
             ''' <remarks> See <see cref="ViewModelBase.UIValidationErrorCount"/>. </remarks>
            Public Sub AddUIValidationError(ByVal e As UIValidationError)
                If (UIValidationErrors.ContainsKey(e.Key)) Then
                    UIValidationErrors.Remove(e.Key)
                End If
                UIValidationErrors.Add(e.Key, e)
                NotifyPropertyChanged("UIValidationErrorUserMessages")
                NotifyPropertyChanged("UIValidationErrorCount")
                OnUIValidationErrorsChanged()
            End Sub
            
            ''' <summary> Clears the internal UI validation error dictionary of this ViewModel. </summary>
             ''' <remarks> See <see cref="ViewModelBase.UIValidationErrorCount"/>. </remarks>
            Protected Sub ClearUIValidationErrors()
                UIValidationErrors.Clear()
                NotifyPropertyChanged("UIValidationErrorUserMessages")
                NotifyPropertyChanged("UIValidationErrorCount")
                OnUIValidationErrorsChanged()
            End Sub
            
            ''' <summary> Removes an UI validation error from this ViewModel. </summary>
             ''' <param name="e"> The UI error. </param>
             ''' <remarks> See <see cref="ViewModelBase.UIValidationErrorCount"/>. </remarks>
            Public Sub RemoveUIValidationError(ByVal e As UIValidationError)
                If (UIValidationErrors.ContainsKey(e.Key)) Then
                    UIValidationErrors.Remove(e.Key)
                    NotifyPropertyChanged("UIValidationErrorUserMessages")
                    NotifyPropertyChanged("UIValidationErrorCount")
                    OnUIValidationErrorsChanged()
                End If
            End Sub
            
            
            ''' <summary> Response to changed UIValidationErrors. </summary>
             ''' <remarks>
             ''' The default implementation is to set status text and tooltip to reflect status of UI validation errors.
             ''' Also, <c>Me.RaiseCanExecuteChangedForAll()</c> is called.
             ''' </remarks>
            Protected Overridable Sub OnUIValidationErrorsChanged()
                If (Me.UIValidationErrorCount > 0) Then
                    Me.StatusText = Rstyx.Utilities.Resources.Messages.ViewModelBase_Status_UIValidationErrorsPresent
                    Me.StatusTextToolTip = Me.UIValidationErrorUserMessages
                Else
                    Me.StatusText = Nothing
                    Me.StatusTextToolTip = Nothing
                End If
                Me.RaiseCanExecuteChangedForAll()
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Raises <c>PropertyChanged</c> event for a local property when a corresponding My.Settings setting is changed. </summary>
             ''' <remarks>
             ''' "Corresponding" means one of these:
             ''' <list type="bullet">
             ''' <item><description> Names of property and setting are identical. </description></item>
             ''' <item><description> Name of setting is the property name prefixed with class name and "_". </description></item>
             ''' </list>
             ''' </remarks>
            Private Sub OnUserSettingsChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
                Try
                    Dim ActualClassType     As System.Type = MyClass.GetType()
                    Dim Properties()        As System.Reflection.PropertyInfo = ActualClassType.GetProperties()
                    Dim ProjectSettingName  As String = e.PropertyName
                    Dim ClassPropertyName   As String = ProjectSettingName.Right(ActualClassType.Name & "_")
                    
                    If ((From pi In Properties Where pi.Name = ClassPropertyName).Count > 0) Then
                        Try
                            MyBase.NotifyPropertyChanged(ClassPropertyName)
                        Catch ex As System.Exception
                            Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
                        End Try
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromInsideEventHandler)
                End Try
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
