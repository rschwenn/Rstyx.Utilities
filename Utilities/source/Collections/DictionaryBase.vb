
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq

Namespace Collections
    
    ''' <summary> A generic <see cref="System.Collections.Generic.Dictionary(Of TKey, TItem)"/> base class that allows for change notifications. </summary>
     ''' <typeparam name="TKey">  Type of keys. </typeparam>
     ''' <typeparam name="TItem"> Type of items. </typeparam>
     ''' <remarks>
     ''' <para>
     ''' <list type="bullet">
     ''' <listheader><description> <b>Features:</b> </description></listheader>
     ''' <item><description> The <c>INotifyCollectionChanged</c> interface is provided: Use "OnCollectionChanged(ChangeType)" to notify the binding system about collection changes. </description></item>
     ''' <item><description> The <c>INotifyPropertyChanged</c> interface is provided: Use "OnPropertyChanged(propertyName)" to notify the binding system about changes of property values. </description></item>
     ''' <item><description> Every time an Item is changed, "OnPropertyChanged(KeyName)" is called to notify the binding system about changes of this key's value. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public MustInherit Class DictionaryBase(Of TKey, TItem)
        Inherits   Dictionary(Of TKey, TItem)
        Implements System.Collections.Specialized.INotifyCollectionChanged
        Implements System.ComponentModel.INotifyPropertyChanged
        
        Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Collections.DictionaryBase")
        
        #Region "Properties"
            
            ''' <summary> Sets or gets the Dictionary item for a given Key. </summary>
            ''' <param name="key"> The Key for the Item. </param>
            Public Shadows Default Property Item(key As TKey) As TItem
                Get
                    Return MyBase.Item(key)
                End Get
                Set(value As TItem)
                    MyBase.Item(key) = value
                    MyClass.OnPropertyChanged(System.Windows.Data.Binding.IndexerName)
                End Set
            End Property
            
        #End Region
        
        #Region "INotifyCollectionChanged Members"
            
            ''' <summary> Should be raised when the collection changes. </summary>
            Public Event CollectionChanged As System.Collections.Specialized.NotifyCollectionChangedEventHandler Implements System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged
            
            ''' <summary> [Helper] Raises this object's CollectionChanged event. </summary>
             ''' <param name="ChangeType"> Indicates the type of changes. </param>
            Protected Overridable Sub OnCollectionChanged(ByVal ChangeType As System.Collections.Specialized.NotifyCollectionChangedAction)
                Try
                    'Dim handler As System.Collections.Specialized.NotifyCollectionChangedEventHandler = Me.CollectionChangedEvent
                    'If (handler IsNot Nothing) Then
                    '    handler.Invoke(Me, New System.Collections.Specialized.NotifyCollectionChangedEventArgs(ChangeType))
                    'End If
                    RaiseEvent CollectionChanged(Me, New System.Collections.Specialized.NotifyCollectionChangedEventArgs(ChangeType))
                Catch ex As System.Exception
                    Logger.LogError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
                End Try
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
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
                Catch ex As System.Exception
                    Logger.LogError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
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
        
    End Class
        
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
