
Imports System
'Imports System.Globalization
'Imports System.Runtime.Serialization
'Imports System.Security.Permissions

'Namespace 
    
    ''' <summary>
    ''' Represents an exception that doesn't determine a certain error.
    ''' It's mainly intended to add a user friendly remark for another exception
    ''' which is wrapped as inner exception.
    ''' </summary>
    <Serializable> _
    Public Class RemarkException
        Inherits Exception
        
        #Region "Constructors"
            
            ''' <summary>
            ''' Initializes a new instance of the RemarkException class.
            ''' </summary>
            Public Sub New()
                Me.New(String.Empty, Nothing)
            End Sub
            
            ''' <summary> Initializes a new instance of the RemarkException class with a specified error message. </summary>
             ''' <param name="message"> The user friendly message that describes the error resp. it's impacts. </param>
            Public Sub New(message As String)
                Me.New(message, Nothing)
            End Sub
            
            ''' <summary>
             ''' Initializes a new instance of the RemarkException class with a specified error message
             ''' and a reference to the inner exception that is the cause of this exception.
             ''' </summary>
             ''' <param name="message"> The user friendly message that describes the error resp. it's impacts. </param>
             ''' <param name="innerException"> The exception that is the cause of the current exception. </param>
            Public Sub New(message As String, innerException As Exception)
                MyBase.New(message, innerException)
            End Sub
            
        #End Region
        
    End Class
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
