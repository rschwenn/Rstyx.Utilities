
Imports System.Collections.Generic

Imports Rstyx.Utilities.StringUtils

Namespace Collections
    
    ''' <summary> Shortcut for a <see cref="IDCollection(Of String, TItem)"/>, representing the most usual case: objects with a string identifier. </summary>
     ''' <typeparam name="TItem"> Type of collection items. It has to implement the <see cref="IIdentifiable(Of TKey)"/> interface. </typeparam>
     ''' <remarks> The key for the collection will always be the <b>ID</b> property of <b>TItem</b>, which is a String. </remarks>
    Public Class IDCollection(Of TItem As IIdentifiable(Of String))
        Inherits IDCollection(Of String, TItem)
        
        #Region "Constructors"
            
            ''' <summary> Creates a new, empty IDCollection. </summary>
            Public Sub New
                MyBase.New()
            End Sub
            
            Public Sub New(InitialMembers As IEnumerable(Of IIdentifiable(Of String)))
                MyBase.New(InitialMembers)
            End Sub
            
        #End Region
    End Class
    
    ''' <summary>  A generic, ready for use keyed collection for all objects that implement the <see cref="IIdentifiable(Of TKey)"/> interface. </summary>
     ''' <typeparam name="TKey">  Type of collection keys. It's the type of the item's <see cref="IIdentifiable(Of TKey).ID"/> property. </typeparam>
     ''' <typeparam name="TItem"> Type of collection items. It has to implement the <see cref="IIdentifiable(Of TKey)"/> interface. </typeparam>
     ''' <remarks> The key for the collection will always be the <b>ID</b> property of <b>TItem</b>. </remarks>
    Public Class IDCollection(Of TKey, TItem As IIdentifiable(Of TKey))
        Inherits Rstyx.Utilities.Collections.KeyedCollectionBase(Of TKey, TItem)
        
        #Region "Private Fields"
            
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IDCollection")
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new, empty IDCollection. </summary>
            Public Sub New
                MyBase.New()
            End Sub
            
            ''' <summary> Creates a new IDCollection, initiated with items of <paramref name="InitialMembers"/>. </summary>
            Public Sub New(InitialMembers As IEnumerable(Of IIdentifiable(Of TKey)))
                MyBase.New()
                For Each NewItem As IIdentifiable(Of TKey) In InitialMembers
                    Me.Add(NewItem)
                Next
            End Sub
            
        #End Region
        
        #Region "Collection Implementation"
            
            ''' <summary> Gets the <see cref="IIdentifiable(Of TKey)"/>.<b>ID</b> as collection key. </summary>
             ''' <param name="Item"> The identifyable object of interest. </param>
             ''' <returns>           The ID as key. </returns>
            Protected Overrides Function GetKeyForItem(ByVal Item As TItem) As TKey
                Dim Key  As TKey = Nothing
                If (Item IsNot Nothing) Then Key = Item.ID
                Return Key
            End Function
            
            ''' <summary> This is called by <see cref="IDCollection(Of String, TItem).Add"/> and changes it's default behavior. </summary>
             ''' <param name="Index"> Collection index. </param>
             ''' <param name="Item">  The item to add. </param>
             ''' <remarks>            Checks if <paramref name="Item"/> is <see langword="null"/> or if it's <c>ID</c> property already exists in this list. </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Item"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="Index"/> is less than 0, or greater than <see cref="KeyedCollectionBase(Of TKey, TItem).Count"/>. </exception>
             ''' <exception cref="InvalidIDException"> <paramref name="Item"/>.<c>ID</c> is empty or does already exist. </exception>
            Protected Overrides Sub InsertItem(Index As Integer, Item As TItem)
                
                If (Item Is Nothing) Then Throw New System.ArgumentNullException("Item")

                If (Item.ID.ToString().IsEmptyOrWhiteSpace()) Then Throw New InvalidIDException(Rstyx.Utilities.Resources.Messages.IDCollection_MissingID)
                If (MyClass.Contains(Item.ID))                Then Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.IDCollection_RepeatedID, Item.ID.ToString()))
                
                MyBase.InsertItem(Index, Item)
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
