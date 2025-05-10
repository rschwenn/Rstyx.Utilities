
Imports System
Imports Rstyx.Utilities.StringUtils

Namespace Domain
            
    ''' <summary> Known node types of a topology (node-edge model). </summary>
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
    
    ''' <summary> A Node of a topology (node-edge model). </summary>
     <SerializableAttribute> _
    Public Class TopologyNode
    
        #Region "Properties"
                
            ''' <summary> Node full name resp. wide area ID. May be <see langword="null"/>. </summary>
             ''' <remarks> 
             ''' In DBAG GND this is concatenated from <see cref="OperationPoint"/> (8 characters), <see cref="OperationArea"/> (2 characters) and <see cref="LocalName"/> (5 characters), all right alined and delimited by a "-". 
             ''' DBAG full name schema: "12345678-12-1234-5"
             ''' See <see cref="DeriveFullName()"/> and <see cref="DeriveNameParts()"/>.
             ''' </remarks>
            Public Property FullName()              As String
            
            ''' <summary> Operation point the node belogs to (name or numerical ID). May be <see langword="null"/>. </summary>
            Public Property OperationPoint()        As String
            
            ''' <summary> Description of the node's operation point (i.e. station name). May be <see langword="null"/>. </summary>
            Public Property OperationPointDescr()   As String
            
            ''' <summary> Operation area the node belogs to. May be <see langword="null"/>. </summary>
            Public Property OperationArea()         As String
            
            ''' <summary> Local name of the node (i.e. junction number). May be <see langword="null"/>. </summary>
            Public Property LocalName()             As String
            
            ''' <summary> TrackNode type. </summary>
            Public Property Type()                  As NodeType = NodeType.None
            
            ''' <summary> Description of the node (i.e. junction designation like 49-190-1:9). May be <see langword="null"/>. </summary>
            Public Property Description()           As String
            
            
            ''' <summary> Code (or shortcut title) of the node. May be <see langword="null"/>. </summary>
            Public Property Code()                  As String
            
            ''' <summary> Determines the coordinates system. May be <see langword="null"/>, if coordinates are unknown. </summary>
            Public Property CoordSys()              As String
            
            ''' <summary> ID of underlying point. May be <see langword="null"/>. </summary>
            Public Property PointID()               As String
            
            ''' <summary>  The easting coordinate in [m]. </summary>
            Public Property Y()                     As Double = Double.NaN
            
            ''' <summary>  The northing coordinate in [m]. </summary>
            Public Property X()                     As Double = Double.NaN
           
        #End Region
           
        #Region "Public Methods"
            
            ''' <summary> Sets <see cref="FullName"/> by concatenating <see cref="OperationPoint"/> (8 characters), <see cref="OperationArea"/> (2 characters) and <see cref="LocalName"/> (5 characters), all right alined and delimited by a "-". </summary> 
             ''' <remarks> DBAG full name schema: "12345678-12-1234-5" </remarks>
            Public Sub DeriveFullName()
                Dim OperationPoint_ok As Boolean = ((Me.OperationPoint Is Nothing) OrElse (Me.OperationPoint.Length <= 8))
                Dim OperationArea_ok  As Boolean = ((Me.OperationArea  Is Nothing) OrElse (Me.OperationArea.Length  <= 2))
                Dim LocalName_ok      As Boolean = ((Me.LocalName      Is Nothing) OrElse (Me.LocalName.Length      <= 5))
            
                If (OperationPoint_ok AndAlso OperationArea_ok AndAlso LocalName_ok) Then
                    Dim LocalNameBase   As String = Me.LocalName
                    Dim LocalNameSuffix As String = Nothing
                    If (Me.LocalName?.Length > 0) Then
                        If (Not Me.LocalName.Right(1).IsMatchingTo("[0-9]")) Then
                            LocalNameBase   = LocalNameBase.Substring(0, LocalNameBase.Length - 1)
                            LocalNameSuffix = Me.LocalName.Right(1)
                        End If
                    End If
                    Me.FullName = Sprintf("%+8s-%+2s-%+4s-%+1s", Me.OperationPoint, Me.OperationArea, LocalNameBase, LocalNameSuffix)
                End If
            End Sub
            
            ''' <summary> Sets <see cref="OperationPoint"/> (8 characters), <see cref="OperationArea"/> (2 characters) and <see cref="LocalName"/> (5 characters) by splitting <see cref="FullName"/> if it is 18 characters long. </summary>
            Public Sub DeriveNameParts()
                If (Me.FullName?.Length = 18) Then
                    Me.OperationPoint = Me.FullName.Substring(0 , 8).Trim()
                    Me.OperationArea  = Me.FullName.Substring(9 , 2).Trim()
                    Me.LocalName      = Me.FullName.Substring(12, 4).Trim() & Me.FullName.Substring(17, 1).Trim()
                End If
            End Sub
            
            ''' <summary> Returns <see langword="true"/>, if both <see cref="FullName"/> and <see cref="LocalName"/> are <see langword="null"/> or empty or white space, otherwise <see langword="false"/>. </summary>
            Public Function IsEmpty() As Boolean 
                Return (Me.OperationPoint.IsEmptyOrWhiteSpace() AndAlso Me.OperationArea.IsEmptyOrWhiteSpace() AndAlso Me.LocalName.IsEmptyOrWhiteSpace())
            End Function
            
            ''' <inheritdoc/>
            Public Overrides Function ToString() As String
                Return Sprintf(Rstyx.Utilities.Resources.Messages.TopologyNode_ToString, Me.FullName, Me.Type.ToDisplayString(), Me.Description)
            End Function
           
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
