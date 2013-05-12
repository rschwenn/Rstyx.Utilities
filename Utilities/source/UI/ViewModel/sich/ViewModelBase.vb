
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq

Imports Rstyx.Utilities.Collections

Namespace UI.ViewModel
    
    ''' <summary>
    ''' Abstact base class for ViewModels. 
    ''' It provides support for property (and user settings) change notification, safely setting properties and has a DisplayName property.
    ''' </summary>
     ''' <remarks>
     ''' Origin:  <a href="http://msdn.microsoft.com/magazine/dd419663.aspx" target="_blank"> Josh Smith's MvvmDemoApp </a><br />
     ''' Changes: Detect Changes of My.Settings and raises "PropertyChanged()" for a local property with same name.
     ''' </remarks>
    Public MustInherit Class ViewModelBase
        Implements INotifyPropertyChanged
        Implements IStatusIndicator
        Implements IDisposable
        
        #Region "Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UI.ViewModel.ViewModelBase")
            
            Private _DisplayName                    As String = Nothing
            Private _DisplayNameLong                As String = Nothing
            Private _ThrowOnInvalidPropertyName     As Boolean = False
            
            Private _RequestCloseCommand            As DelegateUICommand
            
            Private _Progress                       As Double = 0
            Private _StatusText                     As String = String.Empty
            
            Private DeferredSetProgressAction       As Rstyx.Utilities.DeferredAction = New Rstyx.Utilities.DeferredAction(AddressOf setProgressTo100, System.Windows.Threading.Dispatcher.CurrentDispatcher)
            Private ReadOnly SetProgressDelay       As System.TimeSpan = System.TimeSpan.FromMilliseconds(500)
            
        #End Region
        
        #Region "Initializing and Finalizing"
            
            Protected Sub New()
                'Subscribe for notifications of user settings changes.
                AddHandler My.Settings.PropertyChanged, AddressOf OnUserSettingsChanged
            End Sub
            
            ''' <summary> Finalizes the object. </summary>
            Protected Overrides Sub Finalize()
                
                #If DEBUG Then
                    'Useful for ensuring that ViewModel objects are properly garbage collected.
                    Dim msg As String = String.Format("Finalized {0} '{1}' (Hashcode {2})", Me.GetType().Name, Me.DisplayName, Me.GetHashCode())
                    System.Diagnostics.Debug.WriteLine(msg)
                #End If
                
                MyBase.Finalize()
            End Sub
            
        #End Region
        
        #Region "DisplayName"
            
            ''' <summary> The user-friendly short name of this object. </summary>
            Public Overridable Property DisplayName() As String
                Get
                    If (_DisplayName is Nothing) Then
                        _DisplayName = "** DisplayName not set **"
                    End If
                    Return _DisplayName
                End Get
                Set(ByVal value As String)
                    If (Not (value =_DisplayName)) Then
                        _DisplayName = value
                        Me.OnPropertyChanged("DisplayName")
                    End If
                End Set
            End Property
            
            ''' <summary> The user-friendly long name of this object. If not set the short name is returned. </summary>
            Public Overridable Property DisplayNameLong() As String
                Get
                    If (_DisplayNameLong is Nothing) Then
                        _DisplayNameLong = Me.DisplayName
                    End If
                    Return _DisplayNameLong
                End Get
                Set(ByVal value As String)
                    If (Not (value =_DisplayNameLong)) Then
                        _DisplayNameLong = value
                        Me.OnPropertyChanged("DisplayNameLong")
                    End If
                End Set
            End Property
            
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
                                
                                If (Not OldPropertyValue.equals(NewPropertyValue)) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        Me.OnPropertyChanged(PropertyName)
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
                                
                                If (Not OldPropertyValue.equals(NewPropertyValue)) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        Me.OnPropertyChanged(PropertyName)
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
                                
                                If (Not OldPropertyValue.equals(NewPropertyValue)) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        Me.OnPropertyChanged(PropertyName)
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
             ''' <param name="NewDesiredValue">         Display string, whose corresponding dictionary key should become the new property value. </param>
             ''' <param name="SupportedValues">         IDictionary where the keys are the supported property values and the items the display strings (of an ItemsControl). </param>
             ''' <param name="NotifyOnPropertyChanged"> If True and the property has really changed, this method calls "OnPropertyChanged(PropertyName)". </param>
             ''' <returns>                              True, if NewDesiredValue is supported (and the property may have changed). </returns>
             ''' <remarks> This is intended for setting a property with integrated validation and binding support for lightweight properties. </remarks>
            Protected Function TrySetPropertyByDisplayString(Of TProperty) _
                                                            (PropertyName As String, _
                                                             NewDesiredValue As String, _
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
                        Dim TargetProperty     As System.Reflection.PropertyInfo = Me.GetType.GetProperty(PropertyName, PropertyValueType)
                        
                        If (TargetProperty Is Nothing) Then
                            Logger.logError(StringUtils.sprintf("TrySetPropertyByDisplayString(): Zu setzende Eigenschaft '%s' existiert nicht!", PropertyName))
                        ElseIf (Not TargetProperty.CanWrite) Then
                            Logger.logError(StringUtils.sprintf("TrySetPropertyByDisplayString(): Zu setzende Eigenschaft '%s' ist schreibgaschützt!", PropertyName))
                        Else
                            ' Look up for the display string and get the corresponding property value
                            Dim NewPropertyValue  As TProperty = Nothing
                            success = SupportedValues.findKeyByValue(NewDesiredValue, NewPropertyValue)
                            
                            If (success) Then
                                Dim OldPropertyValue As TProperty = TargetProperty.GetValue(Me, Nothing)
                                
                                If (Not OldPropertyValue.equals(NewPropertyValue)) Then
                                    TargetProperty.SetValue(Me, NewPropertyValue, Nothing)
                                    If (NotifyOnPropertyChanged AndAlso TypeOf Me Is System.ComponentModel.INotifyPropertyChanged) Then
                                        Me.OnPropertyChanged(PropertyName)
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
        
        #Region "IDisposable Members"
            
            ''' <summary> Should be invoked to dispose unmanaged resources, so this object will be subject to garbage collection. </summary>
            Public Sub Dispose() Implements IDisposable.Dispose
                RemoveHandler My.Settings.PropertyChanged, AddressOf OnUserSettingsChanged
                Me.OnDispose()
            End Sub
            
            ''' <summary> Child classes can override this method to perform clean-up logic, such as removing event handlers. </summary>
            Protected Overridable Sub OnDispose()
            End Sub
            
        #End Region
        
        #Region "INotifyPropertyChanged Members"
            
            ''' <summary>  Raised when a property on this object has a new value. </summary>
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            
            ''' <summary> Raises this object's <c>PropertyChanged</c> event. </summary>
             ''' <param name="propertyName"> The property that has a new value. </param>
            Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
                Me.VerifyPropertyName(propertyName)
                Try
                    'Dim handler As PropertyChangedEventHandler = Me.PropertyChangedEvent
                    'If handler IsNot Nothing Then
                    '    handler.Invoke(Me, New PropertyChangedEventArgs(propertyName))
                    'End If
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
                Catch ex As System.Exception
                    Logger.logError(ex, "OnPropertyChanged(): Fehler in einem Ereignis-Handler.")
                End Try
            End Sub
            
        #End Region
        
        #Region "IStatusIndicator Members"
            
            Private Sub setProgressTo100()
                _Progress = 100
                Me.OnPropertyChanged("Progress")
            End Sub
            
            ''' <summary> The progress in percent. </summary>
            ''' <remarks> Setting the value to 100 is splitted: a) set to 99.8, b) delayed set to 100. </remarks>
            Public Property Progress() As Double Implements IStatusIndicator.Progress
                Get
                    Return _Progress
                End Get
                Set(value As Double)
                    If (Not (value = _Progress)) Then
                        If (value < 100) Then
                            _Progress = value
                        Else
                            _Progress = 99.8
                            DeferredSetProgressAction.Defer(SetProgressDelay)
                        End if
                        Me.OnPropertyChanged("Progress")
                    End if
                End Set
            End Property
            
            ''' <summary> A status text that (i.e for displaying in status bar). </summary>
            Public Property StatusText() As String Implements IStatusIndicator.StatusText
                Get
                    Return _StatusText
                End Get
                Set(value As String)
                    If (Not (value = _StatusText)) Then
                        _StatusText = value
                        Me.OnPropertyChanged("StatusText")
                    End if
                End Set
            End Property
            
            ''' <summary> Sets status text empty and progress to minimum. </summary>
            Sub resetStateIndication() Implements IStatusIndicator.resetStateIndication
                Me.StatusText = String.Empty
                Me.Progress   = 0
            End Sub
            
        #End Region
        
        #Region "RequestClose [Command and Event]"
            
            ''' <summary> Returns a command that raises the RequestCloseEvent. </summary>
            Public ReadOnly Property RequestCloseCommand() As DelegateUICommand
                Get
                    If (_RequestCloseCommand Is Nothing) Then
                        
                        Dim Decoration As New UICommandDecoration()
                        Decoration.Caption     = "Schließen" 
                        Decoration.Description = Me.DisplayNameLong & " schließen"
                        Decoration.IconBrush   = UI.Resources.UIResources.IconBrush("Handmade_Power4")
                        
                        _RequestCloseCommand = New DelegateUICommand(AddressOf OnRequestClose, Decoration)
                    End If
                    Return _RequestCloseCommand
                End Get
            End Property
            
            ''' <summary> Raised when this workspace should be removed from the UI. </summary>
            Public Event RequestClose As EventHandler
            
            Private Sub OnRequestClose()
                Try
                    RaiseEvent RequestClose(Me, System.EventArgs.Empty)
                Catch ex As System.Exception
                    Logger.logError(ex, "OnRequestClose(): Fehler in einem Ereignis-Handler.")
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
                If ((From pi As System.Reflection.PropertyInfo In MyClass.GetType.GetProperties() Where pi.Name = propertyName).Count < 1) Then
                    Dim msg As String = "Invalid property name: " & propertyName
                    
                    If (Me.ThrowOnInvalidPropertyName) Then
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
            Protected Overridable Property ThrowOnInvalidPropertyName() As Boolean
                Get
                    Return _ThrowOnInvalidPropertyName
                End Get
                Set(ByVal value As Boolean)
                    _ThrowOnInvalidPropertyName = value
                End Set
            End Property
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Raises "PropertyChanged()" for a local property when a coresponding My.Settings setting is changed. </summary>
             ''' <remarks>
             ''' "Corresponding" means one of these:
             ''' <list type="bullet">
             ''' <item><description> Names of property and setting are identical. </description></item>
             ''' <item><description> Name of property is the setting name prefixed with class name and "_". </description></item>
             ''' </list>
             ''' </remarks>
            Private Sub OnUserSettingsChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
                
                Dim ActualClassType     As System.Type = MyClass.GetType()
                Dim Properties()        As System.Reflection.PropertyInfo = ActualClassType.GetProperties()
                Dim ProjectSettingName  As String = e.PropertyName
                Dim ClassPropertyName   As String = ProjectSettingName.Right(ActualClassType.Name & "_")
                
                If ((From pi In Properties Where pi.Name = ClassPropertyName).Count > 0) Then 
                    Me.OnPropertyChanged(ClassPropertyName)
                End If
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=4::tabSize=4:
