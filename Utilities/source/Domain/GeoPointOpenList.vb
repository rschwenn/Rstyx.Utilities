
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> An ordinary collection of <see cref="IGeoPoint"/>'s, which can hold multiple points with same <see cref="IGeoPoint.ID"/> property. </summary>
     ''' <remarks>
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> Every collection item has to implement the <see cref="IGeoPoint"/> interface. There are no more restrictions. </description></item>
     ''' <item><description> Multiple point's of the list may have the same <see cref="IGeoPoint.ID"/>. </description></item>
     ''' <item><description> Manipulation method for changing the point ID's according to a point change table. </description></item>
     ''' <item><description> The <see cref="IHeader.Header"/> property can carry some text information related to the list. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class GeoPointOpenList
        Inherits   Collection(Of IGeoPoint)
        Implements IHeader
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger  As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Domain.GeoPointOpenList")
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.LogDebug("New(): GeoPointOpenList instantiated")
            End Sub
            
            ''' <summary> Creates a new GeoPointOpenList and inititializes it's items from any given <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
             ''' <param name="SourcePointList"> The source point list to get initial points from. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' If <paramref name="SourcePointList"/> is of type <see cref="IHeader"/>
             ''' the <see cref="GeoPointOpenList.Header"/> will be set, too.
             ''' </remarks>
            Public Sub New(SourcePointList As IEnumerable(Of IGeoPoint))
                Me.New()
                If (SourcePointList IsNot Nothing) Then
                    For Each SourcePoint As IGeoPoint In SourcePointList
                        Me.Add(SourcePoint)
                    Next
                    If (TypeOf SourcePointList Is IHeader) Then
                        Me.Header = DirectCast(SourcePointList, IHeader).Header
                    End If
                End If
            End Sub
            
            ''' <summary> Creates a new GeoPointOpenList and inititializes it's items from any given <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
             ''' <param name="SourcePointList"> The source point list to get initial points from. May be <see langword="null"/>. </param>
             ''' <param name="MetaData">        An object providing the header for this list. May be <see langword="null"/>. </param>
            Public Sub New(SourcePointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
                Me.New()
                If (SourcePointList IsNot Nothing) Then
                    For Each SourcePoint As IGeoPoint In SourcePointList
                        Me.Add(SourcePoint)
                    Next
                    If (MetaData IsNot Nothing) Then
                        Me.Header = MetaData.Header
                    End If
                End If
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _Header As Collection(Of String)
            
            ''' <summary> Gets or sets header text lines for the list. </summary>
             ''' <remarks> This may be used for a text file. </remarks>
            Public Property Header() As Collection(Of String) Implements IHeader.Header
                Get
                    If _Header Is Nothing Then
                        _Header = New Collection(Of String)
                    End If
                    Return _Header
                End Get
                Set(value As Collection(Of String))
                    _Header = value
                End Set
            End Property
            
        #End Region
        
        #Region "Collection Implementation"
            
            ''' <summary> This is called by <see cref="IDCollection(Of String, TItem).Add"/> and changes it's default behavior. </summary>
             ''' <param name="Index"> Collection index. </param>
             ''' <param name="Item">  The item to add. </param>
             ''' <remarks>            Checks if <paramref name="Item"/> is <see langword="null"/>, but allows every (even empty) <see cref="IGeoPoint.ID"/>. </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Item"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="Index"/> is less than 0, or greater than <see cref="KeyedCollectionBase(Of TKey, TItem).Count"/>. </exception>
            Protected Overrides Sub InsertItem(Index As Integer, Item As IGeoPoint)
                
                If (Item Is Nothing) Then Throw New System.ArgumentNullException("Item")
                
                ' <exception cref="InvalidIDException"> <paramref name="Item"/>.<c>ID</c> is empty. </exception>
                'If (Item.ID.ToString().IsEmptyOrWhiteSpace()) Then Throw New InvalidIDException(Rstyx.Utilities.Resources.Messages.IDCollection_MissingID)
                
                MyBase.InsertItem(Index, Item)
            End Sub
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Clears this collection as well as the <see cref="GeoPointOpenList.Header"/>. </summary>
            Protected Overrides Sub ClearItems()
                MyBase.ClearItems()
                Me.Header.Clear()
            End Sub
            
            ''' <summary> Returns a list of all points in one string. </summary>
            Public Overrides Function ToString() As String
                
                Dim PointFmt  As String = " %20s %15.5f%15.5f%10.4f %5.0f %5.0f  %-10s %-4s %4s %3d  %-6s %-6s  %-25s %-13s"
                Dim PointList As New System.Text.StringBuilder()
                
                ' Header lines.
                If (Me.Header.Count > 0) Then
                    For Each HeaderLine As String In Me.Header
                        PointList.AppendLine(HeaderLine)
                    Next
                Else
                    ' Default Header.
                    'PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoPointList_Label_KvDefaultHeader1)
                    'PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoPointList_Label_KvDefaultHeader2)
                    'PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoPointList_Label_KvDefaultHeader3)
                    'PointList.AppendLine("#-----------------------------------------------------------------------------------------------------------------------------------------------------")
                End If
                
                PointList.AppendLine(Rstyx.Utilities.Resources.Messages.GeoPointList_TableHeader.ToHeadLine("-", Padding:=False))
                
                ' Points.
                For Each p As IGeoPoint In Me
                    
                    PointList.AppendLine(sprintf(PointFmt, P.ID, If(Double.IsNaN(P.Y), "", P.Y), If(Double.IsNaN(P.X), "", P.X), If(Double.IsNaN(P.Z), "", P.Z),
                                         p.ActualCant * 1000, p.ActualCantAbs * 1000, P.Kind.ToDisplayString(), P.KindText, P.MarkType, p.Attributes?.Count,
                                         P.CoordSys, P.HeightSys, P.Info, P.HeightInfo
                                        ))
                Next
                
                Return PointList.ToString()
            End Function
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Changes point ID's inside this list according to a ID change table. </summary>
             ''' <param name="IDChangeTab"> Table with Point ID pairs (source => target). </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="IDChangeTab"/> is <see langword="null"/>. </exception>
             ''' <exception cref="InvalidIDException"> Attempt to assign an ID of invalid format. </exception>
            Public Sub ChangeIDs(IDChangeTab As Dictionary(Of String, String))
                
                If (IDChangeTab Is Nothing) Then Throw New System.ArgumentNullException("PointChangeTab")
                
                Dim SourcePoint As IGeoPoint = Nothing
                Try
                    Dim ChangeCount As Long = 0
                    
                    If (IDChangeTab.Count < 1) then
                        Logger.LogWarning(Rstyx.Utilities.Resources.Messages.GeoPointList_EmptyIDChangeTab)
                    Else
                        For i As Integer = 0 To Me.Count
                            
                            SourcePoint = Me.Item(i)
                            
                            If (IDChangeTab.ContainsKey(SourcePoint.ID)) Then
                                SourcePoint.ID = IDChangeTab(SourcePoint.ID)
                                ChangeCount += 1
                            End If
                        Next
                        Logger.LogInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_ChangeIDSuccess, ChangeCount))
                    End If
                Catch ex As InvalidIDException
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_ChangeIDInvalidID, SourcePoint.ID, ex.Message))
                End Try
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
