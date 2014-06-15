
Namespace Domain
    
    ''' <summary> Position description related to a topology (node-edge model). </summary>
    Public Class PositionAtTopology
        
        ''' <summary> Code or shortcut title of A-node. </summary>
        Public Property ANodeCode           As String = String.Empty
        
        ''' <summary> Title of A-node. </summary>
        Public Property ANodeTitle          As String = String.Empty
        
        ''' <summary> Description of A-node. </summary>
        Public Property ANodeDescription    As String = String.Empty
        
        ''' <summary> Distance of A-node to bottleneck. </summary>
        Public Property ANodeDistance       As Double = Double.NaN
        
        ''' <summary> Operation point of A-node. </summary>
        Public Property ANodeOperationPoint As String = String.Empty
        
        ''' <summary> Code or shortcut title of B-Node. </summary>
        Public Property BNodeCode           As String = String.Empty
        
        ''' <summary> Title of B-Node. </summary>
        Public Property BNodeTitle          As String = String.Empty
        
        ''' <summary> Description of B-Node. </summary>
        Public Property BNodeDescription    As String = String.Empty
        
        ''' <summary> Distance of B-Node to bottleneck. </summary>
        Public Property BNodeDistance       As Double = Double.NaN
        
        ''' <summary> Operation point of B-Node. </summary>
        Public Property BNodeOperationPoint As String = String.Empty
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
