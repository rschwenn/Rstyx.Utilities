
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq

Namespace Collections
    
    ''' <summary> A generic <see cref="System.Collections.ObjectModel.KeyedCollection(Of TKey, TItem)"/> base class that keeps the items sorted. </summary>
     ''' <typeparam name="TKey">  Type of keys. </typeparam>
     ''' <typeparam name="TItem"> Type of items. </typeparam>
     ''' <remarks>
     ''' <para>
     ''' <list type="bullet">
     ''' <listheader><description> <b>Features:</b> </description></listheader>
     ''' <item><description> <c>Add</c> silently ignores an Item with an already existing key and also an Item that is Null. </description></item>
     ''' <item><description> An <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/> can be set for comparing keys (check for existence). </description></item>
     ''' <item><description> By default the <see cref="System.Collections.Generic.Comparer(Of TKey)"/>.Default comparer is used for sorting. </description></item>
     ''' <item><description> The <see cref="SortedKeyedCollectionBase.KeyComparer"/> property can be set to another <see cref="System.Collections.Generic.IComparer(Of TKey)"/> to use for sorting. </description></item>
     ''' <item><description> The <see cref="SortedKeyedCollectionBase(Of TKey, TItem).Keys"/> property implements the same functionality as the <see cref="Dictionary(Of TKey, TValue).Keys"/> property. </description></item>
     ''' <item><description> The <see cref="System.Collections.Specialized.INotifyCollectionChanged"/> interface is provided: Use <see cref="SortedKeyedCollectionBase(Of TKey, TItem).OnCollectionChanged"/> to notify the binding system about collection changes. </description></item>
     ''' <item><description> The <see cref="System.ComponentModel.INotifyPropertyChanged"/> interface is provided: Use <see cref="SortedKeyedCollectionBase(Of TKey, TItem).OnPropertyChanged"/> to notify the binding system about collection changes. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public MustInherit Class SortedKeyedCollectionBase(Of TKey, TItem)
        Inherits   KeyedCollection(Of TKey, TItem)
        Implements System.Collections.Specialized.INotifyCollectionChanged
        Implements System.ComponentModel.INotifyPropertyChanged
        
        Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Collections.SortedKeyedCollectionBase")
        
        Private _KeyComparer  As IComparer(Of TKey) = Nothing
        
        ''' <summary>  Initializes the SortedKeyedCollection with a default <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/>. </summary>
        Protected Sub New()
            MyBase.New()
        End Sub
        
        ''' <summary> Initializes the SortedKeyedCollection with a given <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/>. </summary>
         ''' <param name="comparer"> An instance of <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/>. </param>
        Protected Sub New(comparer As System.Collections.Generic.IEqualityComparer(Of TKey))
            MyBase.New(comparer)
        End Sub
        
        ''' <summary> Gets or sets the <see cref="System.Collections.Generic.IComparer(Of TKey)"/> that is used to sort this collection by keys. </summary>
         ''' <value>   A class that implements the <see cref="System.Collections.Generic.IComparer(Of TKey)"/> interface. </value>
         ''' <returns> The <see cref="System.Collections.Generic.IComparer(Of TKey)"/> that has been set, or the <see cref="System.Collections.Generic.Comparer(Of TKey)"/>.Default comparer, if the property has been set to Null. </returns>
        Protected Overridable Property KeyComparer As IComparer(Of TKey)
            Get
                If (_KeyComparer Is Nothing) Then
                    KeyComparer = System.Collections.Generic.Comparer(Of TKey).Default
                Else
                    KeyComparer = _KeyComparer
                End If
            End Get
            Set(value As IComparer(Of TKey))
                _KeyComparer = value
            End Set
        End Property
        
        ''' <summary> This is called by <see cref="System.Collections.ObjectModel.KeyedCollection(Of TKey, TItem)"/>.Add and changes it's default behavior. </summary>
         ''' <param name="Index"> Collection index, will be ignored and automatically determined because of the nature of this collection. </param>
         ''' <param name="Item">  The item to add. </param>
         ''' <remarks>            If the key of the item already exists or is <see langword="null"/>, nothing is done. Otherwise the Item is added at the position determined by the KeyComparer property. </remarks>
         ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="Index"/> is less than 0, or greater than <see cref="SortedKeyedCollectionBase(Of TKey, TItem).Count"/>. </exception>
        Protected Overrides Sub InsertItem(Index As Integer, Item As TItem)
            
            Dim NewItemKey  As TKey = GetKeyForItem(Item)
            
            If (Not ((Item IsNot Nothing) AndAlso MyClass.Contains(NewItemKey))) Then

                Dim insertIndex As Integer
                
                If (Me.Count = 0) Then
                    insertIndex = 0
                ElseIf (Me.KeyComparer.Compare(NewItemKey, GetKeyForItem(Me.Item(0))) < 0) Then
                    insertIndex = 0
                ElseIf (Me.KeyComparer.Compare(NewItemKey, GetKeyForItem(Me.Item(Me.Count - 1))) > 0) Then
                    insertIndex = Me.Count
                Else
                    Dim PartitionMid   As Integer = 0
                    Dim PartitionBegin As Integer = 0
                    Dim PartitionEnd   As Integer = Me.Count - 1
                    
                    ' Kind of "binary search".
                    Do While (PartitionBegin < (PartitionEnd - 1) )

                        PartitionMid = Int( (PartitionBegin + PartitionEnd) / 2)

                        If (Me.KeyComparer.Compare(NewItemKey, GetKeyForItem(Me.Item(PartitionMid))) < 0) Then
                            PartitionEnd = PartitionMid
                        Else
                            PartitionBegin = PartitionMid
                        End If
                    Loop
                    insertIndex = PartitionEnd
                End If
                
                MyBase.InsertItem(insertIndex, Item)
            End If
        End Sub
        
        #Region "INotifyCollectionChanged Members"
            
            ''' <summary> Should be raised when the collection changes. </summary>
            Public Event CollectionChanged As System.Collections.Specialized.NotifyCollectionChangedEventHandler Implements System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged
            
            ''' <summary> [Helper] Raises this object's CollectionChanged event. </summary>
             ''' <param name="ChangeType"> Indicates the type of changes. </param>
            Protected Overridable Sub OnCollectionChanged(ByVal ChangeType As System.Collections.Specialized.NotifyCollectionChangedAction)
                Try
                    RaiseEvent CollectionChanged(Me, New System.Collections.Specialized.NotifyCollectionChangedEventArgs(ChangeType))
                Catch ex As System.Exception
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
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
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromCalledEventHandler)
                End Try
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Returns a collection, that contains the keys of all items of this KeyedCollection. </summary>
            Public ReadOnly Property Keys() As Dictionary(Of TKey, TItem).KeyCollection
                 Get
                    Dim InternalSearchDictionary As Dictionary(Of TKey, TItem)
                    If (Me.Dictionary Is Nothing) Then
                        InternalSearchDictionary = New Dictionary(Of TKey, TItem)
                    Else
                        InternalSearchDictionary = New Dictionary(Of TKey, TItem)(Me.Dictionary)
                    End If
                    Return New Dictionary(Of TKey, TItem).KeyCollection(InternalSearchDictionary)
                 End Get
            End Property
            
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
