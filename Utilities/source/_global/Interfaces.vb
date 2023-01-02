
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
        
        ''' <summary> A default value for <see cref="StatusText"/>. Defaults to String.Empty. </summary>
        Property StatusTextDefault() As String
        
        ''' <summary> A tool tip for status text (i.e for displaying more text than fits in status bar). </summary>
        Property StatusTextToolTip() As String
        
        ''' <summary> A default value for <see cref="StatusTextToolTip"/>. Defaults to <see langword="null"/>. </summary>
        Property StatusTextToolTipDefault() As String
        
        ''' <summary> <see langword="true"/> signals "work in progress" or "busy". </summary>
         ''' <remarks> This is intended to use if a discrete progress is unknown. </remarks>
        Property IsInProgress() As Boolean
        
        ''' <summary> The current progress in percent. </summary>
        Property Progress() As Double
        
        
        ''' <summary> The progress range in percent which is currently used for <see cref="ProgressTick"/>. Defaults to 100. </summary>
        Property ProgressTickRange() As Double
        
        ''' <summary> The count of ticks matching <see cref="ProgressTickRange"/>. Defaults to 100. </summary>
        Property ProgressTickRangeCount() As Double
        
        ''' <summary> Increases <see cref="Progress"/> by <see cref="ProgressTickRange"/> divided by <see cref="ProgressTickRangeCount"/>. </summary>
        Sub ProgressTick()
        
        
        ''' <summary> Sets status text to <see cref="StatusTextDefault"/>, <see cref="Progress"/> to zero and <see cref="IsInProgress"/> to <see langword="false"/> (immediately). </summary>
        Sub ResetStateIndication()
        
        ''' <summary> Sets status text to <see cref="StatusTextDefault"/> and <see cref="Progress"/> to zero (after a delay), but immediately <see cref="IsInProgress"/> to <see langword="false"/>. </summary>
        Sub ResetStateIndication(Delay As Double)
        
    End Interface
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
