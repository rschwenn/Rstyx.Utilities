
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel

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
     ''' <item><description> The <c>KeyComparer</c> property can be set to another <see cref="System.Collections.Generic.IComparer(Of TKey)"/> to use for sorting. </description></item>
     ''' <item><description> The <c>INotifyCollectionChanged</c> interface is provided: Use <c>OnCollectionChanged(ChangeType)</c> to notify the binding system about collection changes. </description></item>
     ''' <item><description> The <c>Keys</c> property implements the same functionality as the <see cref="Dictionary(Of TKey, TValue)"/>.Keys property. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public MustInherit Class SortedKeyedCollectionBase(Of TKey, TItem)
        Inherits   KeyedCollection(Of TKey, TItem)
        Implements System.Collections.Specialized.INotifyCollectionChanged
        Implements System.ComponentModel.INotifyPropertyChanged
        
        Private _KeyComparer    As IComparer(Of TKey) = Nothing
        
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
                If (_KeyComparer.isNull()) Then
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
         ''' <param name="Index"> Collection index. </param>
         ''' <param name="Item">  The item to add. </param>
         ''' <remarks>            If the key of the item already exists, nothing is done. Otherwise the Item is added at the position determined by the KeyComparer property. </remarks>
        Protected Overrides Sub InsertItem(Index As Integer, Item As TItem)
            Dim insertIndex As Integer = Index
            Try
                If (Not (Item.IsNotNull() AndAlso MyClass.Contains(GetKeyForItem(Item)))) Then
                    Dim retrievedItem As TItem
                    
                    For i As Integer = 0 To (Count - 1 )
                        retrievedItem = Me(i)
                        if (Me.KeyComparer.Compare(GetKeyForItem(Item), GetKeyForItem(retrievedItem)) < 0) Then
                            insertIndex = i 
                            Exit For
                        End If
                    Next
                    
                    MyBase.InsertItem(insertIndex, Item)
                End If
            Catch ex As System.Exception
            End Try
        End Sub
        
        #Region "INotifyCollectionChanged Members"
            
            ''' <summary> Should be raised when the collection changes. </summary>
            Public Event CollectionChanged As System.Collections.Specialized.NotifyCollectionChangedEventHandler Implements System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged
            
            ''' <summary> [Helper] Raises this object's CollectionChanged event. </summary>
             ''' <param name="ChangeType"> Indicates the type of changes. </param>
            Protected Overridable Sub OnCollectionChanged(ByVal ChangeType As System.Collections.Specialized.NotifyCollectionChangedAction)
                
                Dim handler As System.Collections.Specialized.NotifyCollectionChangedEventHandler = Me.CollectionChangedEvent
                If handler IsNot Nothing Then
                    handler.Invoke(Me, New System.Collections.Specialized.NotifyCollectionChangedEventArgs(ChangeType))
                End If
            End Sub
            
        #End Region
        
        #Region "INotifyPropertyChanged Members"
            
            ''' <summary>  Raised when a property on this object has a new value. </summary>
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
            
            ''' <summary> Raises this object's PropertyChanged event. </summary>
             ''' <param name="propertyName"> The property that has a new value. </param>
            Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
                Dim handler As PropertyChangedEventHandler = Me.PropertyChangedEvent
                If handler IsNot Nothing Then
                    handler.Invoke(Me, New PropertyChangedEventArgs(propertyName))
                End If
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
                        InternalSearchDictionary = Me.Dictionary
                    End If
                    Return New Dictionary(Of TKey, TItem).KeyCollection(InternalSearchDictionary)
                 End Get
            End Property
            
        #End Region
        
        
    End Class 
        
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
