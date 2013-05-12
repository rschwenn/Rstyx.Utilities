
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
        
        #Region "Fields"
            
            ''' <summary>
            ''' Contains the message that describes the error.
            ''' </summary>
            Private _message As String
            
            ''' <summary>
            ''' Contains the raw data when the error occured.
            ''' </summary>
            Private _rawData As String
            
            ''' <summary>
            ''' Contains the current field index.
            ''' </summary>
            Private _currentFieldIndex As Integer
            
            ''' <summary>
            ''' Contains the current record index.
            ''' </summary>
            Private _currentRecordIndex As Long
            
            ''' <summary>
            ''' Contains the current position in the raw data.
            ''' </summary>
            Private _currentPosition As Integer
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary>
            ''' Initializes a new instance of the RemarkException class.
            ''' </summary>
            Public Sub New()
                Me.New(String.Empty, Nothing)
            End Sub
            
            ''' <summary>
            ''' Initializes a new instance of the RemarkException class with a specified error message.
            ''' </summary>
            ''' <param name="message">The user friendly message that describes the error resp. it's impacts.</param>
            Public Sub New(message As String)
                Me.New(message, Nothing)
            End Sub
            
            ''' <summary>
            ''' Initializes a new instance of the RemarkException class with a specified error message
            ''' and a reference to the inner exception that is the cause of this exception.
            ''' </summary>
            ''' <param name="message">The user friendly message that describes the error resp. it's impacts.</param>
            ''' <param name="innerException">The exception that is the cause of the current exception.</param>
            Public Sub New(message As String, innerException As Exception)
                MyBase.New(message, innerException)
                'MyBase.New(String.Empty, innerException)
                '_message = If(message Is Nothing, String.Empty, message)
                '
                '_rawData = String.Empty
                '_currentPosition = -1
                '_currentRecordIndex = -1
                '_currentFieldIndex = -1
            End Sub
            
            
            ' ''' <summary>
            ' ''' Initializes a new instance of the RemarkException class with serialized data.
            ' ''' </summary>
            ' ''' <param name="info">The <see cref="T:SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
            ' ''' <param name="context">The <see cref="T:StreamingContext"/> that contains contextual information about the source or destination.</param>
            ' Protected Sub New(info As SerializationInfo, context As StreamingContext)
            '     MyBase.New(info, context)
            '     _message = info.GetString("MyMessage")
            '     
            '     _rawData = info.GetString("RawData")
            '     _currentPosition = info.GetInt32("CurrentPosition")
            '     _currentRecordIndex = info.GetInt64("CurrentRecordIndex")
            '     _currentFieldIndex = info.GetInt32("CurrentFieldIndex")
            ' End Sub
            
        #End Region
        
    End Class
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
