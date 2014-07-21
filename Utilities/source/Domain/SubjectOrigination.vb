
Namespace Domain
    
    ''' <summary> Information about origination of any subject. </summary>
    Public Class SubjectOrigination
        
        ''' <summary> Date of measure. </summary>
        Public Property MeasureDate         As String = String.Empty
        
        ''' <summary> Name of person who has measured. </summary>
        Public Property MeasureName         As String = String.Empty
        
        ''' <summary> Date of creation. </summary>
        Public Property CreateDate          As String = String.Empty
        
        ''' <summary> Name of editor of creation. </summary>
        Public Property CreateName          As String = String.Empty
        
        ''' <summary> Date of last change. </summary>
        Public Property ChangeDate          As String = String.Empty
        
        ''' <summary> Name of editor of last change. </summary>
        Public Property ChangeName          As String = String.Empty
        
        ''' <summary> Date of audition. </summary>
        Public Property AuditDate           As String = String.Empty
        
        ''' <summary> Name of auditor. </summary>
        Public Property AuditName           As String = String.Empty
        
        
        ''' <summary> Comment. </summary>
        Public Property Comment             As String = String.Empty
        
        ''' <summary> Related job title. </summary>
        Public Property Job                 As String = String.Empty
        
        ''' <summary> Incorporated program. </summary>
        Public Property Program             As String = String.Empty
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
