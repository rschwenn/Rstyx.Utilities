
'Namespace System
    
    ''' <summary> Specifies that the implementing object is identified by an object of specified generic parameter. </summary>
     ''' <typeparam name="TKey"> Type of the Identifier. </typeparam>
    Public Interface IIdentifiable(Of TKey)
        
        ''' <summary> An ID of a given Type. </summary>
        Property ID() As Tkey
        
    End Interface
    
    
    ''' <summary> A provider of properties to indicate program state (i.e. for binding to status bar). </summary>
    Public Interface IStatusIndicator
        
        ''' <summary> A status text (i.e for displaying in status bar). </summary>
        Property StatusText() As String
        
        ''' <summary> A default value for <see cref="StatusText"/>. Deaults to String.Empty. </summary>
        Property StatusTextDefault() As String
        
        ''' <summary> A tool tip for status text (i.e for displaying more text than fits in status bar). </summary>
        Property StatusTextToolTip() As String
        
        ''' <summary> A default value for <see cref="StatusTextToolTip"/>. Deaults to <see langword="null"/>. </summary>
        Property StatusTextToolTipDefault() As String
        
        ''' <summary> The current progress in percent. </summary>
        Property Progress() As Double
        
        ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero (immediately). </summary>
        Sub resetStateIndication()
        
        ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero (after a delay). </summary>
        Sub resetStateIndication(Delay As Double)
        
    End Interface
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
