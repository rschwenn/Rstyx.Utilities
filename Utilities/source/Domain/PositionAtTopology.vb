
Imports System

Namespace Domain
    
    ''' <summary> Position description related to a topology (node-edge model). </summary>
     <SerializableAttribute> _
    Public Class PositionAtTopology
        
        ''' <summary> A-node. </summary>
        Public Property NodeA               As TopologyNode = New TopologyNode()
        
        ''' <summary> B-node. </summary>
        Public Property NodeB               As TopologyNode = New TopologyNode()
        
        ''' <summary> Distance of A-node to position. </summary>
        Public Property DistanceToNodeA     As Double = Double.NaN
        
        ''' <summary> Distance of B-Node to position. </summary>
        Public Property DistanceToNodeB     As Double = Double.NaN
        
        ''' <summary> Date of assignment of <see cref="NodeA"/>. </summary>
        Public Property AssignDateNodeA     As String
        
        ''' <summary> Date of assignment of <see cref="NodeB"/>. </summary>
        Public Property AssignDateNodeB     As String
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
