
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO

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
            
            Dim _CoordSystems    As Collection(Of String)
            Dim _GeneralNodeInfo As Dictionary(Of String, NodeInfo)
            Dim _NodesAtTrack    As Dictionary(Of String, Collection(Of NodeAtTrack))
            Dim _EdgesAtTrack    As Dictionary(Of String, Collection(Of EdgeAtTrack))
            
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
            Public ReadOnly Property GeneralNodeInfo() As Dictionary(Of String, NodeInfo)
                Get
                    If (_GeneralNodeInfo Is Nothing) Then
                        LoadTopology()
                    End If
                    Return _GeneralNodeInfo
                End Get
            End Property
            
            ''' <summary> Gets all nodes in this DBB file, grouped by rails. </summary>
            ''' <remarks>
            ''' Fills <see cref="CoordSystems"/> and sets <see cref="CoordSys"/> to the first found system.
            ''' </remarks>
            Public ReadOnly Property NodesAtTrack() As Dictionary(Of String, Collection(Of NodeAtTrack))
                Get
                    If (_NodesAtTrack Is Nothing) Then
                        LoadTopology()
                    End If
                    Return _NodesAtTrack
                End Get
            End Property
            
            ''' <summary> Gets a list of all nodes in this DBB file with general info. </summary>
            Public ReadOnly Property EdgesAtTrack() As Dictionary(Of String, Collection(Of EdgeAtTrack))
                Get
                    If (_EdgesAtTrack Is Nothing) Then
                        LoadTopology()
                    End If
                    Return _EdgesAtTrack
                End Get
            End Property

            'Dim SortedPoints As IOrderedEnumerable(Of IGeoPoint) = InputBlock.Points.OrderBy(Function(ByVal p) p.ID)
            
           ''' <summary> Determines the coordinate system, for which point coordinates are read from DBB. Defaults to "?". </summary>
           Public Property CoordSys() As String = "?"
           
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
                        
                        ' Point coordinates
                        If (SA_TYP = 12) Then
                            'Dim SA12_LSYS As DataField(Of String) = DataLine.ParseField(RecDef12.LSYS)
                            Dim CoordSys As String = DataLine.ParseField(RecDef12.LSYS).Value?.Trim()
                            If (Not _CoordSystems.Contains(CoordSys)) Then _CoordSystems.Add(CoordSys)
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
                    
                    'Dim RecDefSet As New RecordDefinitionSetDBB(Me.FormatVersion)
                    'Dim RecDef12 As RecDefDbbSA12 = DirectCast(RecDefSet.RecType.Item(12), RecDefDbbSA12)
                    '
                    '_CoordSystems = New Collection(Of String)
                    '
                    'For Each DataLine As DataTextLine In Me.DataLineStream
                    '    
                    '    ' Get and validate record type.
                    '    Dim RecordTypeField As DataField(Of Integer) = DataLine.ParseField(RecDefSet.SAxx.SA_TYP)
                    '    Dim SA_TYP As Integer = RecordTypeField.Value
                    '    'If (Not RecDefSet.RecType.ContainsKey(SA_TYP)) Then
                    '    '    Throw New ParseException(New ParseError(ParseErrorLevel.Error,
                    '    '                                            DataLine.SourceLineNo,
                    '    '                                            RecordTypeField.Source.Column,
                    '    '                                            RecordTypeField.Source.Column + RecordTypeField.Source.Length,
                    '    '                                            Sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_InvalidRecordType, RecordTypeField.Source.Value),
                    '    '                                            Sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_ValidRecordTypes, RecDefSet.RecType.Keys.ToCSV()),
                    '    '                                            Nothing
                    '    '                                           ))
                    '    'End If
                    '    
                    '    ' Point coordinates
                    '    If (SA_TYP = 12) Then
                    '        'Dim SA12_LSYS As DataField(Of String) = DataLine.ParseField(RecDef12.LSYS)
                    '        Dim CoordSys As String = DataLine.ParseField(RecDef12.LSYS).Value?.Trim()
                    '        If (Not _CoordSystems.Contains(CoordSys)) Then _CoordSystems.Add(CoordSys)
                    '    End If
                    'Next
                    '
                    '' Set current coordinates system.
                    'If (_CoordSystems.Count > 0) Then
                    '    CoordSys = _CoordSystems(0)
                    '    Logger.logInfo(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologySuccess, Me.CoordSys, _CoordSystems?.ToCSV()))
                    'Else
                    '    Logger.logWarning(sprintf(Rstyx.Utilities.Resources.Messages.DbbFileReader_LoadTopologyNoCoordSystems, Me.FilePath))
                    'End If
                    
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
                    Me.PSTRECKE = New DataFieldDefinition(Of String)("STRECKE"   , DataFieldPositionType.ColumnAndLength, 86,  4)  
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
                    Me.KNBE   = New DataFieldDefinition(Of Integer)("KNBE" , DataFieldPositionType.ColumnAndLength, If(FormatVersion=DbbFormatVersion.DBGIS,75,90), 40, DataFieldOptions.NotRequired)
                End Sub
                
                #Region "Public Fields"
                    Public KNOTEN As DataFieldDefinition(Of String)
                    Public KNTYP  As DataFieldDefinition(Of Integer)                                                    
                    Public PAD    As DataFieldDefinition(Of String)
                    Public KNBE   As DataFieldDefinition(Of Integer)                                                    
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
                    Me.STRECKE = New DataFieldDefinition(Of String)("STRECKE" , DataFieldPositionType.ColumnAndLength, 32,  4)  
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
                DBGIS  = 1
                
                ''' <summary> Format version "AVANI". </summary>
                AGON6  = 2
                
            End Enum
            
            ''' <summary> General topology node info. </summary>
            Public Class NodeInfo
                
                ''' <summary> Node full name resp. ID. </summary>
                Public Property FullName()      As String
                
                ''' <summary> Node type. </summary>
                Public Property Type()          As Integer
                
                ''' <summary> Node description, i.e. junction designation. </summary>
                Public Property Description()   As String
                
                ''' <summary> Determines the coordinates system. </summary>
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
            
            ''' <summary> General topology node info. </summary>
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
            
            ''' <summary> General topology node info. </summary>
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
