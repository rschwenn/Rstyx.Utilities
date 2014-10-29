
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Namespace Collections
    
    ''' <summary> A generic <see cref="System.Collections.ObjectModel.KeyedCollection(Of TKey, TItem)"/> base class. </summary>
     ''' <typeparam name="TKey">  Type of keys. </typeparam>
     ''' <typeparam name="TItem"> Type of items. </typeparam>
     ''' <remarks>
     ''' <para>
     ''' <list type="bullet">
     ''' <listheader><description> <b>Features:</b> </description></listheader>
     ''' <item><description> <c>Add</c> silently ignores an Item with an already existing key and also an Item that is <see langword="null"/>. </description></item>
     ''' <item><description> An <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/> can be set for comparing keys (check for existence). </description></item>
     ''' <item><description> The <see cref="KeyedCollectionBase(Of TKey, TItem).Keys"/> property implements the same functionality as the <see cref="Dictionary(Of TKey, TValue)"/>.Keys property. </description></item>
     ''' <item><description> The <see cref="System.Collections.Specialized.INotifyCollectionChanged"/> interface is provided: Use <c>OnCollectionChanged(ChangeType)</c> to notify the binding system about collection changes. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public MustInherit Class KeyedCollectionBase(Of TKey, TItem)
        Inherits   KeyedCollection(Of TKey, TItem)
        Implements System.Collections.Specialized.INotifyCollectionChanged
        
        Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Collections.KeyedCollectionBase")
        
        Private _KeyComparer  As IComparer(Of TKey) = Nothing
        
        ''' <summary>  Initializes the KeyedCollection with a default <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/>. </summary>
        Protected Sub New()
            MyBase.New()
        End Sub
        
        ''' <summary> Initializes the KeyedCollection with a given <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/>. </summary>
         ''' <param name="comparer"> An instance of <see cref="System.Collections.Generic.IEqualityComparer(Of TKey)"/>. </param>
        Protected Sub New(comparer As System.Collections.Generic.IEqualityComparer(Of TKey))
            MyBase.New(comparer)
        End Sub
        
        ''' <summary> This is called by <see cref="System.Collections.ObjectModel.KeyedCollection(Of TKey, TItem)"/>.Add and changes it's default behavior. </summary>
         ''' <param name="Index"> Collection index. </param>
         ''' <param name="Item">  The item to add. </param>
         ''' <remarks>            If Item is <see langword="null"/> or if the key of the item already exists, silently nothing is done. Otherwise the Item is added at the given index. </remarks>
         ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="Index"/> is less than 0, or greater than <see cref="KeyedCollectionBase(Of TKey, TItem).Count"/>. </exception>
        Protected Overrides Sub InsertItem(Index As Integer, Item As TItem)
                If ((Item IsNot Nothing) AndAlso (Not MyClass.Contains(GetKeyForItem(Item)))) Then
                MyBase.InsertItem(Index, Item)
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
                    Logger.logError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromInsideEventHandler)
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
                        InternalSearchDictionary = DirectCast(Me.Dictionary, Dictionary(Of TKey, TItem))
                    End If
                    Return New Dictionary(Of TKey, TItem).KeyCollection(InternalSearchDictionary)
                 End Get
            End Property
            
        #End Region
        
    End Class 
        
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
