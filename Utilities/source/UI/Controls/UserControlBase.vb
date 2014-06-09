
Imports System
Imports System.ComponentModel
Imports System.Linq
Imports System.Diagnostics
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data

Imports Rstyx.Utilities.Validation

Namespace UI.Controls
    
    ''' <summary> Base class for UserControls including support for property change notifications of custom CLR properties. </summary>
    ''' <remarks>
    ''' <para>
    ''' - Whenever a UI Validation Error occurs (Binding exception while updating source), the ViewModel's <see cref="ViewModel.ViewModelBase.UIValidationErrorCount"/> and <see cref="ViewModel.ViewModelBase.UIValidationErrorUserMessages"/> properties are updated.
    ''' </para>
    ''' <para>
    ''' - The "INotifyPropertyChanged" interface is provided: Use "OnClrPropertyChanged(propertyName)" to notify the binding system about CLR property changes.
    ''' </para>
    ''' <para>
    ''' - The "My.Settings" object is watched for changes. If a My.Settings setting is changed which corresponds 
    '''   to a local property, then the PropertyChangedEvent for this property is automatically raised. 
    ''' </para>
    ''' <para>
    ''' "Corresponding" means one of these:
    ''' <list type="bullet">
    ''' <item><description> Names of property and setting are identical. </description></item>
    ''' <item><description> Name of property is the setting name prefixed with class name and "_". </description></item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Class UserControlBase
        Inherits System.Windows.Controls.UserControl
        Implements INotifyPropertyChanged
        
        #Region "Private Fields"
        
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UI.Controls.UserControlBase")
            
            Private _DisplayName As String = Nothing
            Private _DisplayNameLong As String = Nothing
            
            Private IsCompletelyLoaded  As Boolean = False
            
        #End Region
        
        #Region "Initializing and Finalizing"
            
            Protected Sub New()
            End Sub
            
            ''' <summary> Custom Initializing: register event handlers. </summary>
            Private Sub UserControlBase_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
                Try
                    If (Not IsCompletelyLoaded) Then
                        IsCompletelyLoaded = True
                        'Subscribe for notifications of user settings changes.
                        'AddHandler My.Settings.PropertyChanged, AddressOf OnUserSettingsChanged
                        '
                        ' TODO: Check MakeWeak!
                        Dim WeakPropertyChangedListener As Cinch.WeakEventProxy(Of PropertyChangedEventArgs) = New Cinch.WeakEventProxy(Of PropertyChangedEventArgs)(AddressOf OnUserSettingsChanged)
                        AddHandler My.Settings.PropertyChanged, AddressOf WeakPropertyChangedListener.Handler
                        
                        ' This will listen for each control that has the NotifyOnValidationError=True and a ValidationError occurs.
                        ErrorEventRoutedEventHandler = New RoutedEventHandler(AddressOf ExceptionValidationErrorHandler)
                        Me.AddHandler(System.Windows.Controls.Validation.ErrorEvent, ErrorEventRoutedEventHandler, True)
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                End Try
            End Sub
            
            ''' <summary> Unregister event handlers. </summary>
            Private Sub UserControlBase_Unloaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Unloaded
                Try
                    If (IsCompletelyLoaded) Then
                        IsCompletelyLoaded = False
                        
                        RemoveHandler My.Settings.PropertyChanged, AddressOf OnUserSettingsChanged
                        
                        Me.RemoveHandler(System.Windows.Controls.Validation.ErrorEvent, ErrorEventRoutedEventHandler)
                        ErrorEventRoutedEventHandler = Nothing
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                End Try
            End Sub
            
            #If DEBUG Then
                ''' <summary> Finalizes the object. </summary>
                Protected Overrides Sub Finalize()
                    Dim msg As String = String.Format("Finalized {0} ({1})  (Hashcode {2})", Me.GetType().Name, Me.GetType().FullName, Me.GetHashCode())
                    System.Diagnostics.Debug.WriteLine(msg)
                    MyBase.Finalize()
                End Sub
            #End If
        
        #End Region
        
        #Region "DisplayName"
            
            ''' <summary> The user-friendly short name of this object. Defaults to assembly title and version. </summary>
            Public Overridable Property DisplayName() As String
                Get
                    If (_DisplayName Is Nothing) Then
                        Dim oAppInfo As New Rstyx.Utilities.Apps.AppInfo(System.Reflection.Assembly.GetEntryAssembly())
                        _DisplayName = oAppInfo.Title & " " & oAppInfo.Version.ToString(3)
                    End If
                    Return _DisplayName
                End Get
                Set(ByVal value As String)
                    If (Not (value = _DisplayName)) Then
                        _DisplayName = value
                        Me.OnClrPropertyChanged("DisplayName")
                    End If
                End Set
            End Property
            
            ''' <summary> The user-friendly long name of this object. Defaults to the short name. </summary>
            Public Overridable Property DisplayNameLong() As String
                Get
                    If (_DisplayNameLong Is Nothing) Then
                        _DisplayNameLong = Me.DisplayName
                    End If
                    Return _DisplayNameLong
                End Get
                Set(ByVal value As String)
                    If (Not (value = _DisplayNameLong)) Then
                        _DisplayNameLong = value
                        Me.OnClrPropertyChanged("DisplayNameLong")
                    End If
                End Set
            End Property
            
        #End Region
        
        #Region "Event Handlers"
            
            ''' <summary> Raises "PropertyChanged()" for a local property when a coresponding My.Settings setting is changed. </summary>
            ''' <remarks>
            ''' "Corresponding" means one of these:
            ''' <list type="bullet">
            ''' <item><description> Names of property and setting are identical. </description></item>
            ''' <item><description> Name of property is the setting name prefixed with class name and "_". </description></item>
            ''' </list>
            ''' </remarks>
            Private Sub OnUserSettingsChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
                Try
                    Dim ActualClassType As System.Type = MyClass.GetType
                    Dim Properties() As System.Reflection.PropertyInfo = ActualClassType.GetProperties()
                    Dim ProjectSettingName As String = e.PropertyName
                    Dim ClassPropertyName As String = ProjectSettingName.Right(ActualClassType.Name & "_")
                    
                    If ((From pi In Properties Where pi.Name = ClassPropertyName).Count > 0) Then
                        Try
                            Me.OnClrPropertyChanged(ClassPropertyName)
                        Catch ex As System.Exception
                            Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
                        End Try
                    End If
                Catch ex As System.Exception
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromInsideEventHandler)
                End Try
            End Sub
            
        #End Region
        
        #Region "INotifyPropertyChanged Members"
            
            ''' <summary>  Raised when a property on this object has a new value. </summary>
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            
            ''' <summary> Raises this object's <c>PropertyChanged</c> event. </summary>
            ''' <param name="propertyName"> The property that has a new value. </param>
            Protected Overridable Sub OnClrPropertyChanged(ByVal propertyName As String)
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
        
        #Region "Debug Dependency Attributes"
            
            ''' <summary> If a debugger is attached, the changed value of the dependency property is logged to the trace listeners. </summary>
            ''' <param name="d"> The DependencyObject. </param>
            ''' <param name="e"> The event args. </param>
            Protected Shared Sub debugChangedDP(d As System.Windows.DependencyObject, e As System.Windows.DependencyPropertyChangedEventArgs)
                If (System.Diagnostics.Debugger.IsAttached) Then
                    
                    Dim NewValueString As String = "Null"
                    Dim OldValueString As String = "Null"
                    If (e.NewValue IsNot Nothing) Then NewValueString = e.NewValue.ToString()
                    If (e.OldValue IsNot Nothing) Then OldValueString = e.OldValue.ToString()
                    
                    System.Diagnostics.Debug.Print("DP changed: " & d.DependencyObjectType.Name & "." & e.Property.Name & " = " & NewValueString & "  (old value = " & OldValueString & ")")
                End If
            End Sub
            
        #End Region
        
        #Region "UI Validation Errors (resp. Binding exceptions)"
            
            ' See http://karlshifflett.wordpress.com/archive/mvvm/input-validation-ui-exceptions-model-validation-errors/
            
            Private ErrorEventRoutedEventHandler As RoutedEventHandler
            
            Private Sub ExceptionValidationErrorHandler(ByVal sender As Object, ByVal e As RoutedEventArgs)
                
                Dim args As System.Windows.Controls.ValidationErrorEventArgs = DirectCast(e, System.Windows.Controls.ValidationErrorEventArgs)
                
                If (TypeOf args.Error.RuleInError Is System.Windows.Controls.ExceptionValidationRule) Then
                    
                    Dim oViewModelBase As Rstyx.Utilities.UI.ViewModel.ViewModelBase = TryCast(Me.DataContext, Rstyx.Utilities.UI.ViewModel.ViewModelBase)
                    
                    If (oViewModelBase IsNot Nothing) Then
                        
                        ' We only want to work with validation errors that are Exceptions because the business object has already recorded the business rule violations using IDataErrorInfo.
                        Dim oBindingExpression As BindingExpression = DirectCast(args.Error.BindingInError, System.Windows.Data.BindingExpression)
                        Dim strDataItemName As String = oBindingExpression.DataItem.ToString()
                        Dim strPropertyName As String = oBindingExpression.ParentBinding.Path.Path
                        
                        Dim sb As New System.Text.StringBuilder(1024)
                        
                        For Each oValidationError As ValidationError In System.Windows.Controls.Validation.GetErrors(DirectCast(args.OriginalSource, DependencyObject))
                            
                            If (TypeOf oValidationError.RuleInError Is System.Windows.Controls.ExceptionValidationRule) Then
                                'sb.AppendFormat("{0} has error: ", strPropertyName)
                            
                                If (oValidationError.Exception Is Nothing OrElse oValidationError.Exception.InnerException Is Nothing) Then
                                    sb.AppendLine(oValidationError.ErrorContent.ToString())
                                Else
                                    sb.AppendLine(oValidationError.Exception.InnerException.Message)
                                End If
                            End If
                        Next
                        
                        If (args.Action = ValidationErrorEventAction.Added) Then
                            oViewModelBase.AddUIValidationError(New UIValidationError(strDataItemName, strPropertyName, sb.ToString() ))
                        
                        ElseIf args.Action = ValidationErrorEventAction.Removed Then
                            oViewModelBase.RemoveUIValidationError(New UIValidationError(strDataItemName, strPropertyName, sb.ToString() ))
                        Else
                            ' This can only happen if the .NET Framework changes.  Better to put a sanity check in now that have a very hard to find bug later.
                            Throw New ArgumentOutOfRangeException("Action", args.Action, "Action value was not programmed.")
                        End If
                    End If
                End If
            End Sub
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
