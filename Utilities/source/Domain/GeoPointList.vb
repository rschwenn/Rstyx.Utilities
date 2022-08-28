
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Threading

Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> A sorted collection of <see cref="IGeoPoint"/>'s, which allows direct access via <see cref="IGeoPoint.ID"/> property. </summary>
     ''' <remarks>
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> Every collection item has to implement the <see cref="IGeoPoint"/> interface. There are no more restrictions. </description></item>
     ''' <item><description> The key for the keyed collection is the <see cref="IGeoPoint.ID"/> property of their items. </description></item>
     ''' <item><description> Every <see cref="IGeoPoint.ID"/> has to be unique in the list. </description></item>
     ''' <item><description> Manipulation method for changing the point ID's according to a point change table. </description></item>
     ''' <item><description> The <see cref="IHeader.Header"/> property can carry some text information related to the list. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class GeoPointList
        Inherits IDCollection(Of IGeoPoint)
        Implements IHeader
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPointList")
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                ' Inits a new SortedKeyedCollection: 1. ignoring case when checking for key equality, 2. sorting keys alphanumeric.
                'MyBase.New(System.StringComparer.InvariantCultureIgnoreCase)
                'MyBase.KeyComparer = New Rstyx.Utilities.Collections.AlphanumericComparer(IgnoreCase:=True)
                Logger.logDebug("New(): GeoPointList instantiated")
            End Sub
            
            ''' <summary> Creates a new GeoPointList and inititializes it's items from a <see cref="IEnumerable(Of IGeoPoint)"/>. </summary>
             ''' <param name="SourcePointList"> The source point list to get initial points from. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' If <paramref name="SourcePointList"/> is of type <see cref="IHeader"/>
             ''' the <see cref="GeoPointList.Header"/> will be setfrom <paramref name="SourcePointList"/><c>.Header</c>.
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> There are at least two points in <paramref name="SourcePointList"/>'s with same <see cref="IGeoPoint.ID"/>. </exception>
            Public Sub New(SourcePointList As IEnumerable(Of IGeoPoint))
                Me.New(SourcePointList:=SourcePointList, MetaData:=Nothing)
            End Sub
            
            ''' <summary> Creates a new GeoPointList, inititializes it's items from a <see cref="IEnumerable(Of IGeoPoint)"/> and takes a given header. </summary>
             ''' <param name="SourcePointList"> The source point list to get initial points from. May be <see langword="null"/>. </param>
             ''' <param name="MetaData">        An object providing the header for this list. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' If <paramref name="MetaData"/> is <see langword="null"/> and <paramref name="SourcePointList"/> is of type <see cref="IHeader"/>
             ''' the <see cref="GeoPointList.Header"/> will be set from <paramref name="SourcePointList"/><c>.Header</c>.
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> There are at least two points in <paramref name="SourcePointList"/>'s with same <see cref="IGeoPoint.ID"/>. </exception>
            Public Sub New(SourcePointList As IEnumerable(Of IGeoPoint), MetaData As IHeader)
                Me.New(SourcePointList:=SourcePointList, MetaData:=MetaData, CancelToken:=Nothing, StatusIndicator:=Nothing)
            End Sub
            
            ''' <summary> Creates a new GeoPointList, inititializes it's items from a <see cref="IEnumerable(Of IGeoPoint)"/> and takes a given header. </summary>
             ''' <param name="SourcePointList"> The source point list to get initial points from. May be <see langword="null"/>. </param>
             ''' <param name="MetaData">        An object providing the header for this list. May be <see langword="null"/>. </param>
             ''' <param name="CancelToken">     A CancellationToken that can signal a cancel request (may be CancellationToken.None). </param>
             ''' <remarks>
             ''' If <paramref name="MetaData"/> is <see langword="null"/> and <paramref name="SourcePointList"/> is of type <see cref="IHeader"/>
             ''' the <see cref="GeoPointList.Header"/> will be set from <paramref name="SourcePointList"/><c>.Header</c>.
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> There are at least two points in <paramref name="SourcePointList"/>'s with same <see cref="IGeoPoint.ID"/>. </exception>
             ''' <exception cref="System.OperationCanceledException"> This method has been cancelled. </exception>
            Public Sub New(SourcePointList As IEnumerable(Of IGeoPoint), MetaData As IHeader, CancelToken As CancellationToken)
                Me.New(SourcePointList:=SourcePointList, MetaData:=MetaData, CancelToken:=CancelToken, StatusIndicator:=Nothing)
            End Sub
            
            ''' <summary> Creates a new GeoPointList, inititializes it's items from a <see cref="IEnumerable(Of IGeoPoint)"/> and takes a given header. </summary>
             ''' <param name="SourcePointList"> The source point list to get initial points from. May be <see langword="null"/>. </param>
             ''' <param name="MetaData">        An object providing the header for this list. May be <see langword="null"/>. </param>
             ''' <param name="CancelToken">     A CancellationToken that can signal a cancel request (may be CancellationToken.None). </param>
             ''' <param name="StatusIndicator"> The object that will get status reporting (i.g. the view model). May be <see langword="null"/> </param>
             ''' <remarks>
             ''' If <paramref name="MetaData"/> is <see langword="null"/> and <paramref name="SourcePointList"/> is of type <see cref="IHeader"/>
             ''' the <see cref="GeoPointList.Header"/> will be set from <paramref name="SourcePointList"/><c>.Header</c>.
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> There are at least two points in <paramref name="SourcePointList"/>'s with same <see cref="IGeoPoint.ID"/>. </exception>
             ''' <exception cref="System.OperationCanceledException"> This method has been cancelled. </exception>
            Public Sub New(SourcePointList As IEnumerable(Of IGeoPoint), MetaData As IHeader, CancelToken As CancellationToken, StatusIndicator As IStatusIndicator)
                Me.New()
                If (SourcePointList IsNot Nothing) Then
                    
                    Dim IsStatusReporting = (StatusIndicator IsNot Nothing)
                    If (IsStatusReporting) Then StatusIndicator.IsInProgress = True
                    
                    For Each SourcePoint As IGeoPoint In SourcePointList
                        CancelToken.ThrowIfCancellationRequested()
                        Me.Add(SourcePoint)
                        If (IsStatusReporting) Then
                            StatusIndicator.StatusText = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_Status_Constructing, Me.Count, SourcePoint.ID)
                        End If
                    Next
                    
                    If (MetaData IsNot Nothing) Then
                        Me.Header = MetaData.Header
                    ElseIf (TypeOf SourcePointList Is IHeader) Then
                        Me.Header = DirectCast(SourcePointList, IHeader).Header
                    End If
                    
                    If (IsStatusReporting) Then StatusIndicator.IsInProgress = False
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
        
        #Region "Overrides"
            
            ''' <summary> Clears this collection as well as the <see cref="GeoPointList.Header"/>. </summary>
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
            
            ''' <summary> Changes point ID's of this list according to a ID change table. </summary>
             ''' <param name="IDChangeTab"> Table with Point ID pairs (source => target). </param>
             ''' <returns> A new copy of this <see cref="GeoPointList"/> with changed point ID's. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="IDChangeTab"/> is <see langword="null"/>. </exception>
             ''' <exception cref="InvalidIDException"> Attempt to assign an ID of invalid format or an ID which has been already assigned to another point. </exception>
            Public Function ChangeIDs(IDChangeTab As Dictionary(Of String, String)) As GeoPointList
                Return ChangeIDs(IDChangeTab:=IDChangeTab, CancelToken:=Nothing, StatusIndicator:=Nothing)
            End Function
            
            ''' <summary> Changes point ID's of this list according to a ID change table. </summary>
             ''' <param name="IDChangeTab"> Table with Point ID pairs (source => target). </param>
             ''' <param name="CancelToken">     A CancellationToken that can signal a cancel request (may be CancellationToken.None). </param>
             ''' <returns> A new copy of this <see cref="GeoPointList"/> with changed point ID's. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="IDChangeTab"/> is <see langword="null"/>. </exception>
             ''' <exception cref="InvalidIDException"> Attempt to assign an ID of invalid format or an ID which has been already assigned to another point. </exception>
             ''' <exception cref="System.OperationCanceledException"> This method has been cancelled. </exception>
            Public Function ChangeIDs(IDChangeTab As Dictionary(Of String, String), CancelToken As CancellationToken) As GeoPointList
                Return ChangeIDs(IDChangeTab:=IDChangeTab, CancelToken:=CancelToken, StatusIndicator:=Nothing)
            End Function
            
            ''' <summary> Changes point ID's of this list according to an ID change table. </summary>
             ''' <param name="IDChangeTab">     Table with Point ID pairs (source => target). </param>
             ''' <param name="CancelToken">     A CancellationToken that can signal a cancel request (may be CancellationToken.None). </param>
             ''' <param name="StatusIndicator"> The object that will get status reporting (i.g. the view model). May be <see langword="null"/>. </param>
             ''' <returns> A new copy of this <see cref="GeoPointList"/> with changed point ID's. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="IDChangeTab"/> is <see langword="null"/>. </exception>
             ''' <exception cref="InvalidIDException"> Attempt to assign an ID of invalid format or an ID which has been already assigned to another point. </exception>
             ''' <exception cref="System.OperationCanceledException"> This method has been cancelled. </exception>
            Public Function ChangeIDs(IDChangeTab As Dictionary(Of String, String), CancelToken As CancellationToken, StatusIndicator As IStatusIndicator) As GeoPointList
                
                If (IDChangeTab Is Nothing) Then Throw New System.ArgumentNullException("PointChangeTab")
                
                Dim NewList     As New GeoPointList()
                Dim SourcePoint As IGeoPoint = Nothing
                Try
                    Dim ChangeCount As Long = 0
                    NewList.Header = Me.Header
                    
                    Dim IsStatusReporting = (StatusIndicator IsNot Nothing)
                    If (IsStatusReporting) Then StatusIndicator.ProgressTickRangeCount = Me.Count
                    
                    If (IDChangeTab.Count < 1) then
                        Logger.logWarning(Rstyx.Utilities.Resources.Messages.GeoPointList_EmptyIDChangeTab)
                        If (IsStatusReporting) Then StatusIndicator.ProgressTick()
                    Else
                        For Each SourcePoint In Me
                            
                            CancelToken.ThrowIfCancellationRequested()
                            
                            Dim NewPoint As IGeoPoint = DirectCast(Activator.CreateInstance(SourcePoint.GetType(), SourcePoint), IGeoPoint)
                            
                            If (IDChangeTab.ContainsKey(SourcePoint.ID)) Then
                                
                                Dim NewID As String = IDChangeTab(SourcePoint.ID)
                                
                                ' Repeated ID: More precise hint (if possible) than the default exception of IDCollection.
                                If (NewList.Contains(NewID)) Then
                                    Dim Message     As String    = sprintf(Rstyx.Utilities.Resources.Messages.IDCollection_RepeatedID, NewID)
                                    Dim OriginPoint As IGeoPoint = NewList.Item(NewID)
                                    If ((OriginPoint.SourcePath IsNot Nothing) AndAlso (OriginPoint.SourceLineNo > 0)) Then
                                        Message &= sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_ChangeIDRepeatedIDSource, OriginPoint.SourceLineNo, OriginPoint.SourcePath)
                                    End If
                                    Throw New InvalidIDException(Message)
                                End If
                                
                                NewPoint.ID = NewID
                                ChangeCount += 1

                                If (Not NewPoint.Attributes.ContainsKey(Rstyx.Utilities.Resources.Messages.Domain_AttName_OriginID)) Then
                                    NewPoint.Attributes.Add(Rstyx.Utilities.Resources.Messages.Domain_AttName_OriginID, SourcePoint.ID)
                                End If
                            End If
                            
                            NewList.Add(NewPoint)
                            
                            If (IsStatusReporting) Then
                                StatusIndicator.ProgressTick()
                                StatusIndicator.StatusText = sprintf(Rstyx.Utilities.Resources.Messages.IDCollection_ChangeIDStatus, ChangeCount)
                            End If
                        Next
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_ChangeIDSuccess, ChangeCount))
                    End If
                Catch ex As InvalidIDException
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointList_ChangeIDInvalidID, SourcePoint.ID, ex.Message))
                End Try
                Return NewList
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
