
Imports System

Namespace Domain
    
    ''' <summary> A Node of a topology (node-edge model). </summary>
     <SerializableAttribute> _
    Public Class TopologyNode
        
        ''' <summary> Code (or shortcut title) of the node. </summary>
        Public Property Code            As String = String.Empty
        
        ''' <summary> Title of the node. </summary>
        Public Property Title           As String = String.Empty
        
        ''' <summary> Description of the node. </summary>
        Public Property Description     As String = String.Empty
        
        ''' <summary> Operation point the node belogs to. </summary>
        Public Property OperationPoint  As String = String.Empty
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
