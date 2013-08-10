
'Namespace System
    
    ''' <summary> A provider of properties to indicate program state (i.e. for binding to status bar). </summary>
    Public Interface IStatusIndicator
        
        ''' <summary> A status text (i.e for displaying in status bar). </summary>
        Property StatusText() As String
        
        ''' <summary> A default value for <see cref="StatusText"/>. Deaults to String.Empty. </summary>
        Property StatusTextDefault() As String
        
        ''' <summary> The current progress in percent. </summary>
        Property Progress() As Double
        
        ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero. </summary>
        Sub resetStateIndication()
        
        ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero (after a delay). </summary>
        Sub resetStateIndication(Delay As Long)
        
    End Interface
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
