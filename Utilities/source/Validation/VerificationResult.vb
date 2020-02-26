
Namespace Validation
    
    ''' <summary> Results of a verification. </summary>
    Public Enum VerificationResult As Integer
        
        ''' <summary> Verification hasn't been done yet. </summary>
        None            = 0
        
        ''' <summary> Verification wasn't possible. </summary>
        NotVerifiable   = 1
        
        ''' <summary> Verification has resulted in success. </summary>
        Success         = 2
        
        ''' <summary> Verification has resulted in a warning. </summary>
        Warning         = 3
        
        ''' <summary> Verification has found an error. </summary>
        [Error]         = 4
        
    End Enum
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
