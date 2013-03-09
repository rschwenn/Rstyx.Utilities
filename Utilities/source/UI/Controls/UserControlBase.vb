﻿
Imports System
Imports System.ComponentModel
Imports System.Linq

Namespace UI.Controls
    
    ''' <summary> Base class for UserControls including support for property change notifications of custom CLR properties. </summary>
     ''' <remarks>
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
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.UI.Controls.UserControlBase")
            
            Private _DisplayName                 As String = Nothing
            Private _DisplayNameLong             As String = Nothing
            
        #End Region
        
        #Region "Initializing and Finalizing"
            
            Protected Sub New()
            End Sub
            
            ''' <summary> Custom Initializing: register event handlers. </summary>
            Private Sub UserControlBase_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
                'Subscribe for notifications of user settings changes.
                'AddHandler My.Settings.PropertyChanged, AddressOf OnUserSettingsChanged
                '
                ' TODO: Check MakeWeak!
                Dim WeakPropertyChangedListener As Cinch.WeakEventProxy(Of PropertyChangedEventArgs) = New Cinch.WeakEventProxy(Of PropertyChangedEventArgs)(AddressOf OnUserSettingsChanged)
                AddHandler My.Settings.PropertyChanged, AddressOf WeakPropertyChangedListener.Handler
            End Sub
            
            ''' <summary> Unregister event handlers. </summary>
            Private Sub UserControlBase_Unloaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Unloaded
                RemoveHandler My.Settings.PropertyChanged, AddressOf OnUserSettingsChanged
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
                    If (_DisplayName is Nothing) Then
                        Dim oAppInfo As New Rstyx.Utilities.Apps.AppInfo(System.Reflection.Assembly.GetEntryAssembly())
                        _DisplayName = oAppInfo.Title & " " & oAppInfo.Version.ToString(3)
                    End If
                    Return _DisplayName
                End Get
                Set(ByVal value As String)
                    If (Not (value =_DisplayName)) Then
                        _DisplayName = value
                        Me.OnClrPropertyChanged("DisplayName")
                    End If
                End Set
            End Property
            
            ''' <summary> The user-friendly long name of this object. Defaults to the short name. </summary>
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
                
                Dim ActualClassType     As System.Type = MyClass.GetType
                Dim Properties()        As System.Reflection.PropertyInfo = ActualClassType.GetProperties()
                Dim ProjectSettingName  As String = e.PropertyName
                Dim ClassPropertyName   As String = ProjectSettingName.Right(ActualClassType.Name & "_")
                
                If ((From pi In Properties Where pi.Name = ClassPropertyName).Count > 0) Then 
                    Me.OnClrPropertyChanged(ClassPropertyName)
                End If
            End Sub
            
        #End Region
        
        #Region "INotifyPropertyChanged Members"
            
            ''' <summary>  Raised when a property on this object has a new value. </summary>
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            
            ''' <summary> Raises this object's PropertyChanged event. </summary>
             ''' <param name="propertyName"> The property that has a new value. </param>
            Protected Overridable Sub OnClrPropertyChanged(ByVal propertyName As String)
                
                Dim handler As PropertyChangedEventHandler = Me.PropertyChangedEvent
                If handler IsNot Nothing Then
                    handler.Invoke(Me, New PropertyChangedEventArgs(propertyName))
                End If
            End Sub
            
        #End Region
        
        #Region "Debugging"
            
            ''' <summary> If a debugger is attached, the changed value of the dependency property is logged to the trace listeners. </summary>
             ''' <param name="d"> The DependencyObject. </param>
             ''' <param name="e"> The event args. </param>
            Protected Shared Sub debugChangedDP(d As System.Windows.DependencyObject, e As System.Windows.DependencyPropertyChangedEventArgs)
                If (System.Diagnostics.Debugger.IsAttached) Then
                    System.Diagnostics.Debug.Print("DP changed: " & d.DependencyObjectType.Name & "." & e.Property.Name & " = " & e.NewValue.ToString() & "  (old value = " & e.OldValue.ToString() & ")")
                End If
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
