
Namespace IO
    
    ''' <summary>  A keyed collection of <see cref="System.IO.FileInfo"/> objects (Key = Full Path). </summary>
     ''' <remarks> Since the key is the full path, so one file can't be added twice. </remarks>
    Public Class FileInfoCollection
        Inherits Rstyx.Utilities.Collections.KeyedCollectionBase(Of String, System.IO.FileInfo)
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Files.FileInfoCollection")
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new, empty FileInfoCollection. </summary>
            Public Sub New
                MyBase.New()
                'Logger.logDebug("New(): Initialize empty FileInfoCollection.")
            End Sub
            
        #End Region
        
        #Region "Collection Implementation"
            
            ''' <summary>  Creates a key for the FileInfo (Full Path). </summary>
             ''' <param name="Item"> The FileInfo item of interest. </param>
             ''' <returns>           The key. </returns>
            Protected Overrides Function GetKeyForItem(ByVal Item As System.IO.FileInfo) As String
                Dim Key  As String = "dummy"
                If (Item IsNot Nothing) Then Key = Item.FullName
                Return Key
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
