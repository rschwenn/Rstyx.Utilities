
'Namespace System
    
    ''' <summary> A provoider of properties to indicate program state (i.e. for binding to status bar). </summary>
    Public Interface IStatusIndicator
        
        ''' <summary> A status text that (i.e for displaying in status bar). </summary>
        Property StatusText() As String
        
        ''' <summary> The current progress in percent. </summary>
        Property Progress() As Double
        
        ''' <summary> Sets status text empty and progress to minimum. </summary>
        Sub resetStateIndication()
            
        ''' <summary> Clears the status text and sets the progress to minimum. </summary>
         ''' <param name="Delay"> Delay for reset (in Milliseconds). </param>
        Sub resetStateIndication(Delay As Long)
        
    End Interface
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
