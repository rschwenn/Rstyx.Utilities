
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq

Imports Rstyx.Utilities
Imports Rstyx.Utilities.Collections

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
     ''' <item><description> Detect Changes of My.Settings and raises <c>PropertyChanged()</c> for a corresponding local property. </description></item>
     ''' <item><description> <c>RaiseCanExecuteChangedForAll</c> method forces status update of all commands that are available via view model properties. </description></item>
     ''' <item><description> <c>TrySetProperty</c> methods for safely setting a property by checking it against a provided list of valid values. </description></item>
     ''' </list>
     ''' </remarks>
    Public MustInherit Class ViewModelBase
        Inherits   Cinch.ViewModelBase
        Implements IStatusIndicator
        Implements Cinch.IViewStatusAwareInjectionAware
        
        #Region "Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UI.ViewModel.ViewModelBase")
            
        #End Region
        
        #Region "Initializing and Finalizing"
            
            Protected Sub New()
                MyBase.New()
                
                MyBase.DisplayName = "** DisplayName not set **"
                
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
        
        #Region "Commands"
            
            #Region "Command Support"
                
                ''' <summary> Calls the <c>RaiseCanExecuteChanged</c> method for all ICommands that are available via view model properties (and provide this method). </summary>
                 ''' <remarks> Since the command properties are not all read-only, they have to be re-queried every time. </remarks>
                Protected Sub RaiseCanExecuteChangedForAll()
                    
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
                        End Try
                    Next
                End Sub
                
            #End Region
            
            #Region "CloseView Command"
                
                Private _CloseViewCommand   As DelegateUICommand = Nothing
                
                ''' <summary> Provides an UI Command which raises the Cinch.ViewModelBase.CloseRequest event. </summary>
                Public ReadOnly Property CloseViewCommand() As DelegateUICommand
                    Get
                        If (_CloseViewCommand Is Nothing) Then
                            
                            Dim Decoration As New UICommandDecoration()
                            Decoration.Caption     = "Schließen" 
                            Decoration.Description = "Fenster schließen"
                            Decoration.IconBrush   = UI.Resources.UIResources.IconBrush("Handmade_Power4")
                            
                            _CloseViewCommand = New DelegateUICommand(AddressOf Me.closeView, Decoration)
                        End If
                        
                        Return _CloseViewCommand
                    End Get
                End Property
                
                ''' <summary> Raises the Cinch.ViewModelBase.CloseRequest event. </summary>
                Private Sub closeView()
                    MyBase.RaiseCloseRequest(Nothing)
                End Sub
                
            #End Region
            
            #Region "ShowHelpFile Command"
                
                Private _ShowHelpFileCommand    As DelegateUICommand = Nothing
                
                ''' <summary> Provides an UI Command that calls <c>showHelpFile()</c> which has to be overridden to show the help file. </summary>
                Public ReadOnly Property ShowHelpFileCommand() As DelegateUICommand
                    Get
                        If (_ShowHelpFileCommand Is Nothing) Then
                            
                            Dim Decoration As New UICommandDecoration()
                            Decoration.Caption     = "Hilfe"
                            Decoration.Description = "Hilfedatei anzeigen"
                            Decoration.IconBrush   = UI.Resources.UIResources.IconBrush("Tango_Help1")
                            
                            _ShowHelpFileCommand = New DelegateUICommand(AddressOf Me.showHelpFile, Decoration)
                        End If
                        
                        Return _ShowHelpFileCommand
                    End Get
                End Property
                
                ''' <summary> Shows the help file if overridden in a derived class. </summary>
                ''' <remarks> i.e.:  Rstyx.Microstation.Utilities.FileUtils.showHelpFile(HelpFileName) </remarks>
                Protected Overridable Sub showHelpFile()
                    Throw New System.NotImplementedException("Rstyx.Utilities.UI.ViewModel.ViewModelBase.showHelpFile()")
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
            
            '' <summary> Should be invoked to dispose unmanaged resources, so this object will be subject to garbage collection. </summary>
            'Public Sub Dispose() Implements IDisposable.Dispose
            '    RemoveHandler My.Settings.PropertyChanged, AddressOf OnUserSettingsChanged
            '    Me.OnDispose()
            'End Sub
            
            '' <summary> Child classes can override this method to perform clean-up logic, such as removing event handlers. </summary>
            'Protected Overridable Sub OnDispose()
            'End Sub
            
        #End Region
        
        #Region "IStatusIndicator Members"
            
            Private _Progress                   As Double = 0
            Private _StatusText                 As String = String.Empty
            Private _StatusTextDefault          As String = String.Empty
            
            Private VisibleProgressStepCount    As Double = 90
            Private NextProgressThreshold       As Double = 0
            
            Private DeferredSetProgressAction   As Rstyx.Utilities.DeferredAction = New Rstyx.Utilities.DeferredAction(AddressOf setProgressTo100, System.Windows.Threading.Dispatcher.CurrentDispatcher)
            Private ReadOnly SetProgressDelay   As System.TimeSpan = System.TimeSpan.FromMilliseconds(300)
            
            Private DeferredResetStateAction    As Rstyx.Utilities.DeferredAction = New Rstyx.Utilities.DeferredAction(AddressOf resetStateIndication, System.Windows.Threading.Dispatcher.CurrentDispatcher)
            
            Private Sub setProgressTo100()
                _Progress = 100
                NextProgressThreshold = 0
                MyBase.NotifyPropertyChanged("Progress")
            End Sub
            
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
                        If (value < 100) Then
                            _Progress = value
                        Else
                            _Progress = 99.8
                            ' TODO: Replace by Reactive Extensions (?)
                            DeferredSetProgressAction.Defer(SetProgressDelay)
                        End if
                        
                        ' Notify about change: only at discrete values.
                        If (_Progress >= NextProgressThreshold) Then
                            MyBase.NotifyPropertyChanged("Progress")
                            Cinch.ApplicationHelper.DoEvents()
                            NextProgressThreshold += 100 / VisibleProgressStepCount
                        End if
                    End if
                End Set
            End Property
            
            ''' <summary> A status text (i.e for displaying in status bar). </summary>
             ''' <remarks> In order to reflect value change on UI, <c>Cinch.ApplicationHelper.DoEvents()</c> is called. </remarks>
            Public Property StatusText() As String Implements IStatusIndicator.StatusText
                Get
                    Return _StatusText
                End Get
                Set(value As String)
                    If (Not (value = _StatusText)) Then
                        _StatusText = value
                        MyBase.NotifyPropertyChanged("StatusText")
                        Cinch.ApplicationHelper.DoEvents()
                    End if
                End Set
            End Property
            
            ''' <summary> A default value for <see cref="StatusText"/>. Deaults to String.Empty. </summary>
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
            
            ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero. </summary>
            Public Sub resetStateIndication() Implements IStatusIndicator.resetStateIndication
                Me.StatusText = Me.StatusTextDefault
                Me.Progress = 0
            End Sub
            
            ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero (after a delay). </summary>
             ''' <param name="Delay"> Delay for reset (in Milliseconds). </param>
            Public Sub resetStateIndication(Delay As Long) Implements IStatusIndicator.resetStateIndication
                DeferredResetStateAction.Defer(System.TimeSpan.FromMilliseconds(Delay))
            End Sub
            
        #End Region
        
        #Region "TrySetProperty"
            
            ''' <summary> Tries to set a view model's property to a given value, if this is a supported value. </summary>
             ''' <typeparam name="TProperty">           Type of the target property. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredValue">         Value that the target property should be set to. </param>
             ''' <param name="SupportedValues">         List of supported property values (i.e. the item source of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If True and the property has really changed, this method calls "OnPropertyChanged(PropertyName)". </param>
             ''' <returns>                              True, if NewDesiredValue is supported (and the property may have changed). </returns>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetProperty(Of TProperty) _
                                             (PropertyName As String, _
                                              NewDesiredValue As TProperty, _
                                              ByRef SupportedValues As IList(Of TProperty), _
                                              Optional NotifyOnPropertyChanged As Boolean = False _
                                              ) As Boolean
                Dim success As Boolean = False
                Try
                    If (String.IsNullOrWhiteSpace(PropertyName)) Then
                        'Throw New System.ArgumentNullException("PropertyName")
                        Logger.logError("TrySetProperty(): Argument 'PropertyName' ist NULL oder leer!")
                        
                    ElseIf (SupportedValues is Nothing) Then 
                        'Throw New System.ArgumentNullException("SupportedValues")
                        Logger.logError("TrySetProperty(): Argument 'SupportedValues' ist NULL!")
                        
                    Else
                        Dim PropertyValueType  As System.Type = GetType(TProperty)
                        Dim TargetProperty     As System.Reflection.PropertyInfo = Me.GetType.GetProperty(PropertyName, PropertyValueType)
                        
                        If (TargetProperty Is Nothing) Then
                            Logger.logError(StringUtils.sprintf("TrySetProperty(): Zu setzende Eigenschaft '%s' existiert nicht!", PropertyName))
                        ElseIf (Not TargetProperty.CanWrite) Then
                            Logger.logError(StringUtils.sprintf("TrySetProperty(): Zu setzende Eigenschaft '%s' ist schreibgaschützt!", PropertyName))
                        Else
                            ' Look up the list for the desired property value.
                            success = SupportedValues.Contains(NewDesiredValue)
                            
                            If (success) Then
                                Dim NewPropertyValue As TProperty = NewDesiredValue
                                Dim OldPropertyValue As TProperty = TargetProperty.GetValue(Me, Nothing)
                                Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                                
                                If (IsDifferentValue) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        MyBase.NotifyPropertyChanged(PropertyName)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "TrySetProperty(): Unerwarteter Fehler.")
                End Try
                
                Return success
            End Function
            
            ''' <summary> Tries to set a view model's property to a given value, if this is a supported value. </summary>
             ''' <typeparam name="TProperty">           Type of the target property. </typeparam>
             ''' <typeparam name="TCollectionItem">     Type of the KeyedCollection items. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredValue">         Value that the target property should be set to. </param>
             ''' <param name="SupportedValues">         KeyedCollection where the keys are the supported property values (i.e. the item source of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If True and the property has really changed, this method calls "OnPropertyChanged(PropertyName)". </param>
             ''' <returns>                              True, if NewDesiredValue is supported (and the property may have changed). </returns>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetProperty(Of TProperty, TCollectionItem) _
                                             (PropertyName As String, _
                                              NewDesiredValue As TProperty, _
                                              ByRef SupportedValues As KeyedCollection(Of TProperty, TCollectionItem), _
                                              Optional NotifyOnPropertyChanged As Boolean = False _
                                              ) As Boolean
                Dim success As Boolean = False
                Try
                    If (String.IsNullOrWhiteSpace(PropertyName)) Then
                        'Throw New System.ArgumentNullException("PropertyName")
                        Logger.logError("TrySetProperty(): Argument 'PropertyName' ist NULL oder leer!")
                        
                    ElseIf (SupportedValues is Nothing) Then 
                        'Throw New System.ArgumentNullException("SupportedValues")
                        Logger.logError("TrySetProperty(): Argument 'SupportedValues' ist NULL!")
                        
                    Else
                        Dim PropertyValueType  As System.Type = GetType(TProperty)
                        Dim TargetProperty     As System.Reflection.PropertyInfo = Me.GetType.GetProperty(PropertyName, PropertyValueType)
                        
                        If (TargetProperty Is Nothing) Then
                            Logger.logError(StringUtils.sprintf("TrySetProperty(): Zu setzende Eigenschaft '%s' existiert nicht!", PropertyName))
                        ElseIf (Not TargetProperty.CanWrite) Then
                            Logger.logError(StringUtils.sprintf("TrySetProperty(): Zu setzende Eigenschaft '%s' ist schreibgaschützt!", PropertyName))
                        Else
                            ' Look up the keys for the desired property value.
                            success = SupportedValues.Contains(NewDesiredValue)
                            
                            If (success) Then
                                Dim NewPropertyValue As TProperty = NewDesiredValue
                                Dim OldPropertyValue As TProperty = TargetProperty.GetValue(Me, Nothing)
                                Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                                
                                If (IsDifferentValue) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        MyBase.NotifyPropertyChanged(PropertyName)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "TrySetProperty(): Unerwarteter Fehler.")
                End Try
                
                Return success
            End Function
            
            ''' <summary> Tries to set a view model's property to a given value, if this is a supported value. </summary>
             ''' <typeparam name="TProperty">           Type of the target property. </typeparam>
             ''' <typeparam name="TCollectionItem">     Type of the IDictionary items. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredValue">         Value that the target property should be set to. </param>
             ''' <param name="SupportedValues">         IDictionary where the keys are the supported property values (i.e. the item source of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If True and the property has really changed, this method calls "OnPropertyChanged(PropertyName)". </param>
             ''' <returns>                              True, if NewDesiredValue is supported (and the property may have changed). </returns>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetProperty(Of TProperty, TCollectionItem) _
                                             (PropertyName As String, _
                                              NewDesiredValue As TProperty, _
                                              ByRef SupportedValues As IDictionary(Of TProperty, TCollectionItem), _
                                              Optional NotifyOnPropertyChanged As Boolean = False _
                                              ) As Boolean
                Dim success As Boolean = False
                Try
                    If (String.IsNullOrWhiteSpace(PropertyName)) Then
                        'Throw New System.ArgumentNullException("PropertyName")
                        Logger.logError("TrySetProperty(): Argument 'PropertyName' ist NULL oder leer!")
                        
                    ElseIf (SupportedValues is Nothing) Then 
                        'Throw New System.ArgumentNullException("SupportedValues")
                        Logger.logError("TrySetProperty(): Argument 'SupportedValues' ist NULL!")
                        
                    Else
                        Dim PropertyValueType  As System.Type = GetType(TProperty)
                        Dim TargetProperty     As System.Reflection.PropertyInfo = Me.GetType.GetProperty(PropertyName, PropertyValueType)
                        
                        If (TargetProperty Is Nothing) Then
                            Logger.logError(StringUtils.sprintf("TrySetProperty(): Zu setzende Eigenschaft '%s' existiert nicht!", PropertyName))
                        ElseIf (Not TargetProperty.CanWrite) Then
                            Logger.logError(StringUtils.sprintf("TrySetProperty(): Zu setzende Eigenschaft '%s' ist schreibgaschützt!", PropertyName))
                        Else
                            ' Look up the keys for the desired property value.
                            success = SupportedValues.ContainsKey(NewDesiredValue)
                            
                            If (success) Then
                                Dim NewPropertyValue As TProperty = NewDesiredValue
                                Dim OldPropertyValue As TProperty = TargetProperty.GetValue(Me, Nothing)
                                Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                                
                                If (IsDifferentValue) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        MyBase.NotifyPropertyChanged(PropertyName)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "TrySetProperty(): Unerwarteter Fehler.")
                End Try
                
                Return success
            End Function
            
            ''' <summary> Tries to set a view model's property to a given value's associated display string, if this is a supported value. </summary>
             ''' <typeparam name="TProperty">           Type of the target property. </typeparam>
             ''' <param name="PropertyName">            Name of the target property. </param>
             ''' <param name="NewDesiredDisplayValue">  Display string, whose corresponding dictionary key should become the new property value. </param>
             ''' <param name="SupportedValues">         IDictionary where the keys are the supported property values and the items the display strings (of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If True and the property has really changed, this method calls "OnPropertyChanged(PropertyName)". </param>
             ''' <returns>                              True, if NewDesiredValue is supported (and the property may have changed). </returns>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetPropertyByDisplayString(Of TProperty) _
                                                            (PropertyName As String, _
                                                             NewDesiredDisplayValue As String, _
                                                             ByRef SupportedValues As IDictionary(Of TProperty, String), _
                                                             Optional NotifyOnPropertyChanged As Boolean = False _
                                                             ) As Boolean
                Dim success As Boolean = False
                Try
                    If (String.IsNullOrWhiteSpace(PropertyName)) Then
                        'Throw New System.ArgumentNullException("PropertyName")
                        Logger.logError("TrySetPropertyByDisplayString(): Argument 'PropertyName' ist NULL oder leer!")
                        
                    ElseIf (SupportedValues is Nothing) Then 
                        'Throw New System.ArgumentNullException("SupportedValues")
                        Logger.logError("TrySetPropertyByDisplayString(): Argument 'SupportedValues' ist NULL!")
                        
                    Else
                        Dim PropertyValueType  As System.Type = GetType(TProperty)
                        Dim TargetProperty     As System.Reflection.PropertyInfo = Me.GetType().GetProperty(PropertyName, PropertyValueType)
                        
                        If (TargetProperty Is Nothing) Then
                            Logger.logError(StringUtils.sprintf("TrySetPropertyByDisplayString(): Zu setzende Eigenschaft '%s' existiert nicht!", PropertyName))
                        ElseIf (Not TargetProperty.CanWrite) Then
                            Logger.logError(StringUtils.sprintf("TrySetPropertyByDisplayString(): Zu setzende Eigenschaft '%s' ist schreibgaschützt!", PropertyName))
                        Else
                            ' Look up for the display string and get the corresponding property value
                            Dim NewPropertyValue  As TProperty = Nothing
                            success = SupportedValues.findKeyByValue(NewDesiredDisplayValue, NewPropertyValue)
                            
                            If (success) Then
                                Dim OldPropertyValue As TProperty = TargetProperty.GetValue(Me, Nothing)
                                Dim IsDifferentValue As Boolean   = (Not EqualityComparer(Of TProperty).Default.Equals(OldPropertyValue, NewPropertyValue))
                                
                                If (IsDifferentValue) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        MyBase.NotifyPropertyChanged(PropertyName)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "TrySetPropertyByDisplayString(): Unerwarteter Fehler.")
                End Try
                
                Return success
            End Function
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Raises "PropertyChanged()" for a local property when a corresponding My.Settings setting is changed. </summary>
             ''' <remarks>
             ''' "Corresponding" means one of these:
             ''' <list type="bullet">
             ''' <item><description> Names of property and setting are identical. </description></item>
             ''' <item><description> Name of setting is the property name prefixed with class name and "_". </description></item>
             ''' </list>
             ''' </remarks>
            Private Sub OnUserSettingsChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
                
                Dim ActualClassType     As System.Type = MyClass.GetType()
                Dim Properties()        As System.Reflection.PropertyInfo = ActualClassType.GetProperties()
                Dim ProjectSettingName  As String = e.PropertyName
                Dim ClassPropertyName   As String = ProjectSettingName.Right(ActualClassType.Name & "_")
                
                If ((From pi In Properties Where pi.Name = ClassPropertyName).Count > 0) Then 
                    MyBase.NotifyPropertyChanged(ClassPropertyName)
                End If
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=4::tabSize=4:
