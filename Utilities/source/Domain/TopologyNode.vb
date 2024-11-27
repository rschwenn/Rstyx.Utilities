
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
                
        ''' <summary> TrackNode full name resp. ID. May be <see langword="null"/>. </summary>
         ''' <remarks> 
         ''' In DBAG GND this is concatenated from <see cref="OperationPoint"/> (10 characters) and <see cref="Title"/> (5 characters), both right alined. 
         ''' See <see cref="DeriveFullName()"/> and <see cref="DeriveOperationPointAndTitle()"/>.
         ''' </remarks>
        Public Property FullName()          As String
        
        ''' <summary> Operation point the node belogs to (name or numerical ID). May be <see langword="null"/>. </summary>
        Public Property OperationPoint()    As String
        
        ''' <summary> Title of the node (i.e. junction number). May be <see langword="null"/>. </summary>
        Public Property Title()             As String
        
        ''' <summary> TrackNode type. </summary>
        Public Property Type()              As NodeType = NodeType.None
        
        ''' <summary> Description of the node (i.e. junction designation like 49-190-1:9). May be <see langword="null"/>. </summary>
        Public Property Description()       As String
        
        
        ''' <summary> Code (or shortcut title) of the node. May be <see langword="null"/>. </summary>
        Public Property Code()              As String
        
        ''' <summary> Determines the coordinates system. May be <see langword="null"/>, if coordinates are unknown. </summary>
        Public Property CoordSys()          As String
        
        ''' <summary> ID of underlying point. May be <see langword="null"/>. </summary>
        Public Property PointID()           As String
        
        ''' <summary>  The easting coordinate in [m]. </summary>
        Public Property Y()                 As Double = Double.NaN
        
        ''' <summary>  The northing coordinate in [m]. </summary>
        Public Property X()                 As Double = Double.NaN
        


        ''' <summary> Sets <see cref="FullName"/> by concatenating <see cref="OperationPoint"/> (10 characters) and <see cref="Title"/> (5 characters), both right alined. </summary>
        Public Sub DeriveFullName()
            If ((Me.OperationPoint?.Length <= 10) AndAlso (Me.Title?.Length <= 5)) Then
                Me.FullName = Sprintf("%+10s%+5s", Me.OperationPoint, Me.Title)
            End If
        End Sub

        ''' <summary> Sets <see cref="OperationPoint"/> (10 characters) and <see cref="Title"/> (5 characters) by splitting <see cref="FullName"/> if it is 15 characters long. </summary>
        Public Sub DeriveOperationPointAndTitle()
            If (Me.FullName?.Length = 15) Then
                Me.OperationPoint = Me.FullName.Substring(0, 10).Trim()
                Me.Title          = Me.FullName.Substring(10, 5).Trim()
            End If
        End Sub
        
        ''' <summary> Formats a given node's full name to pattern "12345678-12-1234-1". </summary>
         ''' <returns> Formatted node name, if <see cref="FullName"/> is 15 characters long. </returns>
        Public Function GetFormattedNodeName() As String
            Return GetFormattedNodeName(Me.FullName)
        End Function
        
        ''' <inheritdoc/>
        Public Overrides Function ToString() As String
            Return Sprintf(Rstyx.Utilities.Resources.Messages.TopologyNode_ToString, Me.FullName, Me.OperationPoint, Me.Title, Me.Type.ToDisplayString(), Me.Description)
        End Function
        

        ''' <summary> Formats a given node's full name to pattern "12345678-12-1234-1". </summary>
         ''' <param name="NodeFullName"> The rails code for the Item. </param>
         ''' <returns> Formatted node name, if <paramref name="NodeFullName"/> is 15 characters long. </returns>
        Public Shared Function GetFormattedNodeName(NodeFullName As String) As String
            Dim RetValue As String = NodeFullName
            If (NodeFullName.IsNotEmptyOrWhiteSpace() AndAlso NodeFullName.Length = 15) Then
                RetValue = NodeFullName.Substring(0,8) & "-" & NodeFullName.Substring(8,2) & "-" & NodeFullName.Substring(10,4) & "-" & NodeFullName.Substring(14)
            End If
            Return RetValue
        End Function
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
