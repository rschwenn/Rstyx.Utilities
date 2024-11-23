﻿
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Linq

Imports Rstyx.Utilities
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ''' <summary> A reader for DBB files (track network data) At this point only topology is supported. </summary>
    Public Class DbbFileReader
        Inherits DataFile
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.DbbFileReader")
            
        #End Region
        
        #Region "Public Fields"
            
            Public ReadOnly FormatVersion As DbbFormatVersion
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                'Me.LineStartCommentToken = "#"
                FormatVersion = DbbFormatVersion.AGON6
                Logger.logDebug("New(): DbbFileReader instantiated")
            End Sub
                
            ''' <summary> Creates a new instance with a given file path. </summary>
             ''' <param name="FilePath"> The file path of the file to be read or write. May be <see langword="null"/>. </param>
            Public Sub New(FilePath As String)
                Me.New()
                Me.FilePath = FilePath
                FormatVersion = GetDbbVersion()
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Dim _GeneralNodeInfo As SortedDictionary(Of String, NodeInfo)
            Dim _NodesAtTracks   As SortedDictionary(Of String, IEnumerable(Of NodeAtTrack))
            Dim _EdgesAtTracks   As SortedDictionary(Of String, Collection(Of EdgeAtTrack))
            Dim _CoordSystems    As Collection(Of String)
            
            ''' <summary> Determines the coordinate system, for which point coordinates are read from DBB. Defaults to "?". </summary>
            Public Property CoordSys() As String = "?"
            
            ''' <summary> Gets a list of all coordinate systems used in this DBB file. </summary>
            Public ReadOnly Property CoordSystems() As Collection(Of String)
                Get
                    If (_CoordSystems Is Nothing) Then
                        SurveyData()
                    End If
                    Return _CoordSystems
                End Get
            End Property
            
            ''' <summary> Gets a list of all nodes in this DBB file with general info. </summary>
             ''' <remarks> The dictionary key is the full node name (15 characters long). </remarks>
            Public ReadOnly Property GeneralNodeInfo() As SortedDictionary(Of String, NodeInfo)
                Get
                    If (_GeneralNodeInfo Is Nothing) Then
                        LoadTopology()
                    End If
                    Return _GeneralNodeInfo
                End Get
            End Property
            
            'Dim SortedPoints As IOrderedEnumerable(Of IGeoPoint) = InputBlock.Points.OrderBy(Function(ByVal p) p.ID)

            ''' <summary> Gets all nodes in this DBB file, grouped by tracks. </summary>
             ''' <returns> A Dictionary with a collection of topology nodes for each track. It won't be <see langword="null"/>, but may be empty. </returns>
             ''' <remarks> The nodes collection for a certain track can be accessed via dictionary key resp. "TrackKey", created by <see cref="GetTrackKey"/>. </remarks>
            Public ReadOnly Property NodesAtTracks() As SortedDictionary(Of String, IEnumerable(Of NodeAtTrack))
                Get
                    If (_NodesAtTracks Is Nothing) Then
                        LoadTopology()
                    End If
                    Return _NodesAtTracks
                End Get
            End Property

            ''' <summary> Gets all edges in this DBB file, grouped by tracks. </summary>
             ''' <returns> A Dictionary with a collection of topology edges for each track. It won't be <see langword="null"/>, but may be empty. </returns>
             ''' <remarks> The edges collection for a certain track can be accessed via dictionary key resp. "TrackKey", created by <see cref="GetTrackKey"/>. </remarks>
            Public ReadOnly Property EdgesAtTracks() As SortedDictionary(Of String, Collection(Of EdgeAtTrack))
                Get
                    If (_EdgesAtTracks Is Nothing) Then
                        LoadTopology()
                    End If
                    Return _EdgesAtTracks
                End Get
            End Property

            ''' <summary> Gets all nodes for the given track. </summary>
             ''' <param name="TrackNo">   The track number or rails name for the Item. </param>
             ''' <param name="RailsCode"> The rails code for the Item. </param>
             ''' <returns> Collection of topology nodes for the desired track, ordered by kilometer. It won't be <see langword="null"/>, but may be empty. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="TrackNo"/> is <see langword="null"/> or empty or white space. </exception>
            Public ReadOnly Property NodesAtTrack(TrackNo As String, RailsCode As Integer) As IEnumerable(Of NodeAtTrack)
                Get
                    If (TrackNo.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("TrackNo")

                    If (_NodesAtTracks Is Nothing) Then
                        LoadTopology()
                    End If

                    Dim TrackKey As String = GetTrackKey(TrackNo, RailsCode)
                    If (Not _NodesAtTracks.ContainsKey(TrackKey)) Then
                        _NodesAtTracks.Add(TrackKey, New Collection(Of NodeAtTrack))
                    End If

                    Return _NodesAtTracks.Item(TrackKey)
                End Get
            End Property
            
            ''' <summary> Gets all edges for the given track. </summary>
             ''' <param name="TrackNo">   The track number or rails name for the Item. </param>
             ''' <param name="RailsCode"> The rails code for the Item. </param>
             ''' <returns> Collection of topology edges for the desired track. It won't be <see langword="null"/>, but may be empty. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="TrackNo"/> is <see langword="null"/> or empty or white space. </exception>
            Public ReadOnly Property EdgesAtTrack(TrackNo As String, RailsCode As Integer) As Collection(Of EdgeAtTrack)
                Get
                    If (TrackNo.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("TrackNo")

                    If (_EdgesAtTracks Is Nothing) Then
                        LoadTopology()
                    End If

                    Dim TrackKey As String = GetTrackKey(TrackNo, RailsCode)
                    If (Not _EdgesAtTracks.ContainsKey(TrackKey)) Then
                        _EdgesAtTracks.Add(TrackKey, New Collection(Of EdgeAtTrack))
                    End If

                    Return _EdgesAtTracks.Item(TrackKey)
                End Get
            End Property
           
        #End Region
        
        #Region "I/O Operations"
            
            ''' <summary> Surveys the DBB file for available coordinate systems. </summary>
             ''' <remarks>
             ''' Fills <see cref="CoordSystems"/> and sets <see cref="CoordSys"/> to the first found system.
             ''' </remarks>
             ''' <exception cref="ParseException">  Raises at the first error occurred while parsing, hence <see cref="CollectParseErrors"/> isn't recognized. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub SurveyData()
                Try 
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_SurveyData, Me.FilePath))
                    
                    Dim RecDefSet As New RecordDefinitionSetDBB(Me.FormatVersion)
                    Dim RecDef12 As RecDefDbbSA12 = DirectCast(RecDefSet.RecType.Item(12), RecDefDbbSA12)
                    
                    _CoordSystems = New Collection(Of String)
                    
                    For Each DataLine As DataTextLine In Me.DataLineStream
                        
                        ' Get record type.
                        Dim RecordTypeField As DataField(Of Integer) = DataLine.ParseField(RecDefSet.SAxx.SA_TYP)
                        Dim SA_TYP As Integer = RecordTypeField.Value
                        
                        ' Point coordinates
                        If (SA_TYP = 12) Then
                            'Dim SA12_LSYS As DataField(Of String) = DataLine.ParseField(RecDef12.LSYS)
                            Dim CooSys As String = DataLine.ParseField(RecDef12.LSYS).Value
                            If (Not _CoordSystems.Contains(CooSys)) Then _CoordSystems.Add(CooSys)
                        End If
                    Next
                    
                    ' Set current coordinates system.
                    If (_CoordSystems.Count > 0) Then
                        CoordSys = _CoordSystems(0)
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_SurveyDataSuccess, Me.CoordSys, _CoordSystems?.ToCSV()))
                    Else
                        Logger.logWarning(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_SurveyDataNoCoordSystems, Me.FilePath))
                    End If
                    
                Catch ex As ParseException
                    If (ex.ParseError IsNot Nothing) Then Me.ParseErrors.Add(ex.ParseError)
                    Throw New ParseException(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_SurveyDataParsingFailed, Me.FilePath))
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_SurveyDataFailed, Me.FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
            ''' <summary> Reads topology from the DBB file. </summary>
             ''' <remarks>
             ''' Sets properties <see cref="GeneralNodeInfo"/>, <see cref="NodesAtTrack"/> and <see cref="EdgesAtTrack"/>.
             ''' </remarks>
             ''' <exception cref="ParseException">  Raises at the first error occurred while parsing, hence <see cref="CollectParseErrors"/> isn't recognized. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public Sub LoadTopology()
                Try 
                    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopology, Me.FilePath))
                    
                    _GeneralNodeInfo = New SortedDictionary(Of String, NodeInfo)
                    _NodesAtTracks   = New SortedDictionary(Of String, IEnumerable(Of NodeAtTrack))
                    _EdgesAtTracks   = New SortedDictionary(Of String, Collection(Of EdgeAtTrack))

                    Dim NodePointIDs As New Dictionary(Of String, String)  ' Key=PointID, Item=NodeFullName

                    Dim RecDefSet As New RecordDefinitionSetDBB(Me.FormatVersion)
                    Dim RecDef11  As RecDefDbbSA11 = DirectCast(RecDefSet.RecType.Item(11), RecDefDbbSA11)
                    Dim RecDef12  As RecDefDbbSA12 = DirectCast(RecDefSet.RecType.Item(12), RecDefDbbSA12)
                    Dim RecDef31  As RecDefDbbSA31 = DirectCast(RecDefSet.RecType.Item(31), RecDefDbbSA31)
                    Dim RecDef33  As RecDefDbbSA33 = DirectCast(RecDefSet.RecType.Item(33), RecDefDbbSA33)

                    Dim EdgesCount        As Integer = 0
                    Dim NodesGeneralCount As Integer = 0
                    Dim NodesCoordCount   As Integer = 0
                    Dim NodesAtTrackCount As Integer = 0
                    
                    ' Collect track independent node and edge information.
                    For Each DataLine As DataTextLine In Me.DataLineStream
                        
                        ' Get and validate record type.
                        Dim RecordTypeField As DataField(Of Integer) = DataLine.ParseField(RecDefSet.SAxx.SA_TYP)
                        Dim SA_TYP As Integer = RecordTypeField.Value
                        'If (Not RecDefSet.RecType.ContainsKey(SA_TYP)) Then
                         '    Throw New ParseException(New ParseError(ParseErrorLevel.Error,
                         '                                            DataLine.SourceLineNo,
                         '                                            RecordTypeField.Source.Column,
                         '                                            RecordTypeField.Source.Column + RecordTypeField.Source.Length,
                         '                                            Sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_InvalidRecordType, RecordTypeField.Source.Value),
                         '                                            Sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_ValidRecordTypes, RecDefSet.RecType.Keys.ToCSV()),
                         '                                            Nothing
                         '                                           ))
                        'End If
                        

                        Select Case SA_TYP
                            
                            Case 31  ' Node
                                
                                Dim SA31_KNOTEN As DataField(Of String)  = DataLine.ParseField(RecDef31.KNOTEN)
                                Dim SA31_KNTYP  As DataField(Of Integer) = DataLine.ParseField(RecDef31.KNTYP)
                                Dim SA31_PAD    As DataField(Of String)  = DataLine.ParseField(RecDef31.PAD)
                                Dim SA31_KNBE   As DataField(Of String)  = DataLine.ParseField(RecDef31.KNBE)
                                
                                ' Create new node.
                                Dim Node As New NodeInfo()
                                Node.FullName    = SA31_KNOTEN.Value
                                Node.Type        = SA31_KNTYP.Value
                                Node.PointID     = SA31_PAD.Value
                                Node.Description = SA31_KNBE.Value
                                
                                _GeneralNodeInfo.Add(Node.FullName, Node)
                                NodePointIDs.Add(Node.PointID, Node.FullName)
                                NodesGeneralCount += 1
                                
                                
                            Case 33  ' Edge
                                
                                Dim SA33_AKNOTEN As DataField(Of String)  = DataLine.ParseField(RecDef33.AKNOTEN)
                                Dim SA33_EKNOTEN As DataField(Of String)  = DataLine.ParseField(RecDef33.EKNOTEN)
                                Dim SA33_STRECKE As DataField(Of String)  = DataLine.ParseField(RecDef33.STRECKE)
                                Dim SA33_STRRIKZ As DataField(Of Integer) = DataLine.ParseField(RecDef33.STRRIKZ)                                                   
                                
                                ' Create and new edge to given track.
                                Dim Edge As New EdgeAtTrack()
                                Edge.ANodeFullName = SA33_AKNOTEN.Value
                                Edge.BNodeFullName = SA33_EKNOTEN.Value
                                Edge.RailsNameNo   = SA33_STRECKE.Value
                                Edge.RailsCode     = SA33_STRRIKZ.Value

                                Dim TrackKey As String = GetTrackKey(Edge.RailsNameNo, Edge.RailsCode)
                                If (Not _EdgesAtTracks.ContainsKey(TrackKey)) Then
                                    _EdgesAtTracks.Add(TrackKey, New Collection(Of EdgeAtTrack))
                                End If
                                
                                _EdgesAtTracks(TrackKey).Add(Edge)
                                EdgesCount += 1
                        End Select
                    Next
                    

                    ' Collect track and point related node information.
                    For Each DataLine As DataTextLine In Me.DataLineStream
                        
                        ' Get and validate record type.
                        Dim RecordTypeField As DataField(Of Integer) = DataLine.ParseField(RecDefSet.SAxx.SA_TYP)
                        Dim SA_TYP As Integer = RecordTypeField.Value
                        
                        Select Case SA_TYP
                            
                            Case 11  ' Track related point info
                                
                                Dim SA11_PAD      As DataField(Of String)    = DataLine.ParseField(RecDef11.PAD)
                                Dim SA11_STATION  As DataField(Of Kilometer) = DataLine.ParseField(RecDef11.STATION)
                                Dim SA11_PSTRECKE As DataField(Of String)    = DataLine.ParseField(RecDef11.PSTRECKE)
                                Dim SA11_PSTRRIKZ As DataField(Of Integer)   = DataLine.ParseField(RecDef11.PSTRRIKZ)        
                                
                                Dim PointID As String = SA11_PAD.Value
                                
                                ' Create new track related node info.
                                If (NodePointIDs.ContainsKey(PointID)) Then
                                    
                                    Dim TrackID   As String    = SA11_PSTRECKE.Value
                                    Dim RailsCode As Integer   = SA11_PSTRRIKZ.Value
                                    Dim Km        As Kilometer = SA11_STATION.Value
                                    Dim TrackKey  As String    = GetTrackKey(TrackID, RailsCode)
                                    
                                    Dim Node As New NodeAtTrack()
                                    Node.FullName    = NodePointIDs(PointID)
                                    Node.Kilometer   = Km
                                    Node.RailsNameNo = TrackID
                                    Node.RailsCode   = RailsCode
                                    
                                    If (Not _NodesAtTracks.ContainsKey(TrackKey)) Then
                                        _NodesAtTracks.Add(TrackKey, New Collection(Of NodeAtTrack))
                                    End If

                                    '_NodesAtTracks(TrackKey).Add(Node)
                                    DirectCast(_NodesAtTracks(TrackKey), Collection(Of NodeAtTrack)).Add(Node)
                                    NodesAtTrackCount += 1
                                End If
                                
                                
                            Case 12  ' Point coordinates
                                
                                Dim SA12_PAD  As DataField(Of String) = DataLine.ParseField(RecDef12.PAD)
                                Dim SA12_LSYS As DataField(Of String) = DataLine.ParseField(RecDef12.LSYS)
                                Dim SA12_Y    As DataField(Of Double) = DataLine.ParseField(RecDef12.Y)
                                Dim SA12_X    As DataField(Of Double) = DataLine.ParseField(RecDef12.X)
                                
                                Dim PointID As String = SA12_PAD.Value
                                Dim CooSys  As String = SA12_LSYS.Value
                                
                                ' Complete node with coordinates.
                                If ((CooSys = Me.CoordSys) AndAlso NodePointIDs.ContainsKey(PointID)) Then
                                    Dim Node As NodeInfo =_GeneralNodeInfo.Item(NodePointIDs(PointID))
                                    Node.CoordSys = CooSys
                                    Node.Y = SA12_Y.Value
                                    Node.X = SA12_X.Value
                                    NodesCoordCount += 1
                                End If
                        End Select
                    Next

                    ' Sort _NodesAtTracks by kilometer.
                    For Each TrackKey As String In _NodesAtTracks.Keys.ToArray()
                        _NodesAtTracks(TrackKey) = _NodesAtTracks(TrackKey).OrderBy(Of Kilometer)(Function(ByVal Node) Node.Kilometer)
                    Next
                    
                    ' Summary
                    If (NodesGeneralCount = 0) Then
                        Logger.logWarning(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologyNoNodes, Me.FilePath))
                    Else
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologySuccess, Me.FilePath))
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologyNodesCount1, NodesGeneralCount, NodesCoordCount, Me.CoordSys))
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologyNodesCount2, NodesAtTrackCount, _NodesAtTracks.Count, _NodesAtTracks.Keys.ToCSV()))
                        Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologyEdgesCount,  EdgesCount,        _EdgesAtTracks.Count, _EdgesAtTracks.Keys.ToCSV()))
                    End If
                    
                Catch ex As ParseException
                    If (ex.ParseError IsNot Nothing) Then Me.ParseErrors.Add(ex.ParseError)
                    Throw New ParseException(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologyParsingFailed, Me.FilePath))
                Catch ex as System.Exception
                    Throw New RemarkException(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologyFailed, Me.FilePath), ex)
                Finally
                    Me.ParseErrors.ToLoggingConsole()
                    If (Me.ShowParseErrorsInJedit) Then Me.ParseErrors.ShowInJEdit()
                End Try
            End Sub
            
            ''' <summary> Creates a key for a given track in order to access dictionaries. </summary>
             ''' <param name="TrackNo">   The track number or rails name for the Item. </param>
             ''' <param name="RailsCode"> The rails code for the Item. </param>
             ''' <returns> Track key (pattern "1234@1" or "13@4") for accessing dictionary properties <see cref="NodesAtTracks"/> and <see cref="EdgesAtTracks"/>. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="TrackNo"/> is <see langword="null"/> or empty or white space. </exception>
            Public Function GetTrackKey(TrackNo As String, RailsCode As Integer) As String
                If (TrackNo.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("TrackNo")
                Return TrackNo.Trim() & "@" & CStr(RailsCode)
            End Function
            
            ''' <summary> Creates a detailed list of topology. </summary>
             ''' <returns> Detailed list of topology. </returns>
            Public Function GetTopologyList() As String
                Dim TopoList As New System.Text.StringBuilder()

                #

                Return TopoList.ToString()
            End Function
            
            
        #End Region
        
        #Region "Record Definitions"
            
            ''' <summary> Set of Record Definitions of a DBB file. </summary>
            Protected Class RecordDefinitionSetDBB
                
                ''' <summary> Sets the field definitions for DBB source records. </summary>
                Public Sub New(FormatVersion As DbbFormatVersion)
                    SAxx    = New RecDefDbbSAxx()
                    RecType = New Dictionary(Of Integer, RecDefDbbSAxx)()
                    
                    RecType.Add(11, New RecDefDbbSA11())
                    RecType.Add(12, New RecDefDbbSA12())
                    RecType.Add(31, New RecDefDbbSA31(FormatVersion))
                    RecType.Add(33, New RecDefDbbSA33())
                End Sub
                
                ''' <summary> Definition of every record's begin (Record type and version). </summary>
                Public SAxx As RecDefDbbSAxx
                
                ''' <summary> Definitions of all supported record types. </summary>
                Public RecType As Dictionary(Of Integer, RecDefDbbSAxx)
                
            End Class
            
            ''' <summary> Definition of a DBB general source record begin (record type). </summary>
            Protected Class RecDefDbbSAxx
                
                ''' <summary> Sets the field definitions. </summary>
                Public Sub New()
                    ' Column definitions are zero-ased!
                    Me.SA_TYP = New DataFieldDefinition(Of Integer)("SA_TYP", DataFieldPositionType.ColumnAndLength, 0, 2) 
                End Sub
                
                ''' <summary> Record type. </summary>
                Public SA_TYP   As DataFieldDefinition(Of Integer)
            End Class
            
            ''' <summary> Definition of a DBB SA11 source record (point info). </summary>
            Protected Class RecDefDbbSA11
                Inherits RecDefDbbSAxx
                
                ''' <summary> Sets the field definitions. </summary>
                Public Sub New()
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    
                    Me.PAD      = New DataFieldDefinition(Of String)("PAD"       , DataFieldPositionType.ColumnAndLength,  2, 11)  
                    Me.STATION  = New DataFieldDefinition(Of Kilometer)("STATION", DataFieldPositionType.ColumnAndLength, 21, 13, DataFieldOptions.NotRequired)
                    Me.PSTRECKE = New DataFieldDefinition(Of String)("PSTRECKE"  , DataFieldPositionType.ColumnAndLength, 86,  4, DataFieldOptions.Trim)  
                    Me.PSTRRIKZ = New DataFieldDefinition(Of Integer)("PSTRRIKZ" , DataFieldPositionType.ColumnAndLength, 90,  1)                                    
                End Sub
                
                #Region "Public Fields"
                    Public PAD      As DataFieldDefinition(Of String)
                    Public STATION  As DataFieldDefinition(Of Kilometer)
                    Public PSTRECKE As DataFieldDefinition(Of String)
                    Public PSTRRIKZ As DataFieldDefinition(Of Integer)                                                    
                #End Region
            End Class
            
            ''' <summary> Definition of a DBB SA12 source record (coordinates). </summary>
            Protected Class RecDefDbbSA12
                Inherits RecDefDbbSAxx
                
                ''' <summary> Sets the field definitions. </summary>
                Public Sub New()
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    
                    Me.PAD    = New DataFieldDefinition(Of String)("PAD"   , DataFieldPositionType.ColumnAndLength,  2, 11)  
                    Me.LSTAT  = New DataFieldDefinition(Of String)("LSTAT" , DataFieldPositionType.ColumnAndLength, 13,  1, DataFieldOptions.NotRequired)
                    Me.LSYS   = New DataFieldDefinition(Of String)("LSYS"  , DataFieldPositionType.ColumnAndLength, 14,  3)
                    Me.Y      = New DataFieldDefinition(Of Double)("Y"     , DataFieldPositionType.ColumnAndLength, 31, 14)                                    
                    Me.X      = New DataFieldDefinition(Of Double)("X"     , DataFieldPositionType.ColumnAndLength, 45, 14)                                    
                    Me.MP     = New DataFieldDefinition(Of Integer)("MP"   , DataFieldPositionType.ColumnAndLength, 59,  3)                                    
                    Me.MPEXP  = New DataFieldDefinition(Of Integer)("MPEXP", DataFieldPositionType.ColumnAndLength, 62,  2)                                    
                End Sub
                
                #Region "Public Fields"
                    Public PAD    As DataFieldDefinition(Of String)
                    Public LSTAT  As DataFieldDefinition(Of String)
                    Public LSYS   As DataFieldDefinition(Of String)
                    Public Y      As DataFieldDefinition(Of Double)      
                    Public X      As DataFieldDefinition(Of Double)      
                    Public MP     As DataFieldDefinition(Of Integer)                                    
                    Public MPEXP  As DataFieldDefinition(Of Integer)                                                    
                #End Region
            End Class
            
            ''' <summary> Definition of a DBB SA31 source record (topology node). </summary>
            Protected Class RecDefDbbSA31
                Inherits RecDefDbbSAxx
                
                ''' <summary> Sets the field definitions. </summary>
                Public Sub New(FormatVersion As DbbFormatVersion)
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    
                    Me.KNOTEN = New DataFieldDefinition(Of String)("KNOTEN", DataFieldPositionType.ColumnAndLength,  2, 15)  
                    Me.KNTYP  = New DataFieldDefinition(Of Integer)("KNTYP", DataFieldPositionType.ColumnAndLength, 17,  2)                                    
                    Me.PAD    = New DataFieldDefinition(Of String)("PAD"   , DataFieldPositionType.ColumnAndLength, 19, 11)
                    Me.KNBE   = New DataFieldDefinition(Of String)("KNBE" , DataFieldPositionType.ColumnAndLength, If(FormatVersion=DbbFormatVersion.DBGIS,75,90), 40, DataFieldOptions.NotRequired)
                End Sub
                
                #Region "Public Fields"
                    Public KNOTEN As DataFieldDefinition(Of String)
                    Public KNTYP  As DataFieldDefinition(Of Integer)                                                    
                    Public PAD    As DataFieldDefinition(Of String)
                    Public KNBE   As DataFieldDefinition(Of String)                                                    
                #End Region
            End Class
            
            ''' <summary> Definition of a DBB SA33 source record (topology edge). </summary>
            Protected Class RecDefDbbSA33
                Inherits RecDefDbbSAxx
                
                ''' <summary> Sets the field definitions. </summary>
                Public Sub New()
                    MyBase.New()
                    ' Column definitions are zero-ased!
                    
                    Me.AKNOTEN = New DataFieldDefinition(Of String)("AKNOTEN" , DataFieldPositionType.ColumnAndLength,  2, 15)  
                    Me.EKNOTEN = New DataFieldDefinition(Of String)("EKNOTEN" , DataFieldPositionType.ColumnAndLength, 17, 15)  
                    Me.STRECKE = New DataFieldDefinition(Of String)("STRECKE" , DataFieldPositionType.ColumnAndLength, 32,  4, DataFieldOptions.Trim)  
                    Me.STRRIKZ = New DataFieldDefinition(Of Integer)("STRRIKZ", DataFieldPositionType.ColumnAndLength, 36,  1)                                    
                End Sub
                
                #Region "Public Fields"
                    Public AKNOTEN As DataFieldDefinition(Of String)
                    Public EKNOTEN As DataFieldDefinition(Of String)
                    Public STRECKE As DataFieldDefinition(Of String)
                    Public STRRIKZ As DataFieldDefinition(Of Integer)                                                    
                #End Region
            End Class
        
        #End Region
        
        #Region "Nested Data Structures"
                
            ''' <summary> Format version of DBB file. </summary>
            Public Enum DbbFormatVersion As Integer
                
                ''' <summary> Not defined. </summary>
                None = 0
                
                ''' <summary> Format version "DBGIS". </summary>
                DBGIS = 1
                
                ''' <summary> Format version "AVANI". </summary>
                AGON6 = 2
                
            End Enum
            
            ''' <summary> Known node types. </summary>
            Public Enum NodeType As Integer
                
                ''' <summary> Not defined. </summary>
                None = 0
                
                ''' <summary> Junction. </summary>
                Junction = 1
                
                ''' <summary> End of rails. </summary>
                EndOfRails = 2
                
                ''' <summary> Track Change. </summary>
                TrackChange = 3
                
                ''' <summary> Rails crossing. </summary>
                Crossing = 4
                
            End Enum
            
            ''' <summary> General info of a topology node. </summary>
            Public Class NodeInfo
                
                ''' <summary> Node full name resp. ID. </summary>
                Public Property FullName()      As String
                
                ''' <summary> Node type. </summary>
                Public Property Type()          As Integer
                
                ''' <summary> Node description, i.e. junction designation. </summary>
                Public Property Description()   As String
                
                ''' <summary> Determines the coordinates system. May be <see langword="null"/>, if coordinates are unknown. </summary>
                Public Property CoordSys()      As String
                
                ''' <summary> ID of underlying point ("PAD"). </summary>
                Public Property PointID()       As String
                
                ''' <summary>  The easting coordinate in [m]. </summary>
                Public Property Y()             As Double = Double.NaN
                
                ''' <summary>  The northing coordinate in [m]. </summary>
                Public Property X()             As Double = Double.NaN
                    
                ''' <inheritdoc/>
                Public Overrides Function ToString() As String
                    Return Sprintf(Rstyx.Utilities.Resources.Messages.DbbNodeInfo_ToString, Me.FullName, Me.Type, Me.Description)
                End Function
                
            End Class
            
            ''' <summary> Track related info of a topology node. </summary>
            Public Class NodeAtTrack
                
                ''' <summary> Node full name resp. ID. </summary>
                Public Property FullName        As String
                
                ''' <summary> Kilometer of node. </summary>
                Public Property Kilometer       As Kilometer = New Kilometer()
                
                ''' <summary> Code of track rails (right, left, single, station). </summary>
                Public Property RailsCode       As String = String.Empty
                
                ''' <summary> Name or number of track or rails. </summary>
                Public Property RailsNameNo     As String = String.Empty
                    
                ''' <inheritdoc/>
                Public Overrides Function ToString() As String
                    Return Sprintf(Rstyx.Utilities.Resources.Messages.DbbNodeAtTrack_ToString, Me.FullName, Me.RailsNameNo, Me.RailsCode, Me.Kilometer.ToString())
                End Function
                
            End Class
            
            ''' <summary>  Track related info of a topology edge. </summary>
            Public Class EdgeAtTrack
                
                ''' <summary> A-Node full name resp. ID. </summary>
                Public Property ANodeFullName   As String
                
                ''' <summary> B-Node full name resp. ID. </summary>
                Public Property BNodeFullName   As String
                
                ''' <summary> Code of track rails (right, left, single, station). </summary>
                Public Property RailsCode       As String = String.Empty
                
                ''' <summary> Name or number of track or rails. </summary>
                Public Property RailsNameNo     As String = String.Empty
                    
                ''' <inheritdoc/>
                Public Overrides Function ToString() As String
                    Return Sprintf(Rstyx.Utilities.Resources.Messages.DbbEdgeAtTrack_ToString, Me.ANodeFullName, Me.BNodeFullName, Me.RailsNameNo, Me.RailsCode)
                End Function
                
            End Class
           
        #End Region
            
        #Region "Private Shared Members"
            
            '''' <summary> Parses the given ID as an <see cref="GeoVEPoint"/> ID, if it's not <see langword="null"/>. </summary>
            ' ''' <param name="xID"> The cross field point ID to parse. May be <see langword="null"/>. </param>
            ' ''' <returns> A cross field point ID, or an empty string if ID is <see langword="null"/>. </returns>
            ' ''' <remarks> <paramref name="xID"/> may be prefixed by "*" (auxiliary point). </remarks>
            ' ''' <exception cref="InvalidIDException"> <paramref name="xID"/> has an invalid format. </exception>
            'Private Shared Function ParseID(xID As String) As String
            '    Dim ParsedID As String = xID
            '    If (xID.IsNotEmptyOrWhiteSpace()) Then
            '        Dim VEPoint As New GeoVEPoint()
            '        If (CrossField.IsAuxPointID(xID)) Then
            '            VEPoint.ID = CrossField.GetUnflaggedID(xID)
            '            ParsedID   = CrossField.GetFlaggedID(VEPoint.ID)
            '        Else
            '            VEPoint.ID = xID
            '            ParsedID   = VEPoint.ID
            '        End If
            '    End If
            '    Return ParsedID
            'End Function
            '
            '''' <summary> Verifies the given ID to be compatible for an <see cref="GeoVEPoint"/>, if it's not <see langword="null"/>. </summary>
            ' ''' <param name="xID"> Point ID. </param>
            ' ''' <remarks> <paramref name="xID"/> may be prefixed by "*" (auxiliary point). </remarks>
            ' ''' <exception cref="InvalidIDException"> <paramref name="xID"/> has an invalid format. </exception>
            'Public Shared Sub VerifyID(xID As String)
            '    If (xID.IsNotEmptyOrWhiteSpace()) Then
            '        Dim VEPoint As New GeoVEPoint()
            '        If (CrossField.IsAuxPointID(xID)) Then
            '            VEPoint.ID = CrossField.GetUnflaggedID(xID)
            '        Else
            '            VEPoint.ID = xID
            '        End If
            '    End If
            'End Sub
            '
            '''' <summary> Gets the usual (decimal) VE notation from a given ID. </summary>
            ' ''' <param name="xID"> The cross field point ID to format. May be <see langword="null"/>. </param>
            ' ''' <returns> I.e. "12.34567", "1.23000" , or an empty string if ID is <see langword="null"/>. </returns>
            ' ''' <remarks> <paramref name="xID"/> may be prefixed by "*" (auxiliary point). </remarks>
            ' ''' <exception cref="InvalidIDException"> <paramref name="xID"/> has an invalid format. </exception>
            'Private Shared Function FormatVEID(xID As String) As String
            '    Dim FormattedID As String = xID
            '    If (xID.IsNotEmptyOrWhiteSpace()) Then
            '        If (CrossField.IsAuxPointID(xID)) Then
            '            FormattedID = sprintf("%s %8s", CrossField.AuxPointFlag, GeoVEPoint.FormatID(CrossField.GetUnflaggedID(xID)))
            '        Else
            '            FormattedID = GeoVEPoint.FormatID(xID)
            '        End If
            '    End If
            '    Return FormattedID
            'End Function
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Determines DBB file format version by reading the first file line. </summary>
             ''' <returns> DBB file format version of <see cref="DataFile.FilePath"/>. </returns>
            Private Function GetDbbVersion() As DbbFormatVersion
                Dim RetValue As DbbFormatVersion = DbbFormatVersion.DBGIS
                For Each DataLine As DataTextLine In Me.DataLineStream
                    If (DataLine.Data.StartsWith("00 6AGON")) Then
                        RetValue = DbbFormatVersion.AGON6
                    End If
                    Exit for
                Next
                Logger.LogDebug(Sprintf("GetDbbVersion(): DBB format version ='%s' (File: %s).", RetValue.ToDisplayString(), Me.FilePath))
                Return RetValue
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4: