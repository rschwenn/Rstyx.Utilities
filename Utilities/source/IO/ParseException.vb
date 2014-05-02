
Imports System
Imports System.Runtime.Serialization

Namespace IO
    
    ''' <summary>
    ''' Represents an exception that occurs while parsing a string - usually a line of a text file or a part of it.
    ''' The error information will be passed in a <see cref="ParseError"/>.
    ''' </summary>
    <Serializable> _
    Public Class ParseException
        Inherits Exception
        
        #Region "Fields"
            
            Private _ParseError As ParseError
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Initializes a new instance of the ParseException class. </summary>
            Public Sub New()
                Me.New(String.Empty, Nothing)
            End Sub
            
            ''' <summary> Initializes a new instance of the ParseException class with a specified error message. </summary>
             ''' <param name="message">The user friendly message that describes the error resp. it's impacts.</param>
            Public Sub New(message As String)
                Me.New(message, Nothing)
            End Sub
            
            ''' <summary>
             ''' Initializes a new instance of the ParseException class with a specified error message
             ''' and a reference to the inner exception that is the cause of this exception.
             ''' </summary>
             ''' <param name="message">The user friendly message that describes the error resp. it's impacts.</param>
             ''' <param name="innerException">The exception that is the cause of the current exception.</param>
            Public Sub New(message As String, innerException As Exception)
                MyBase.New(message, innerException)
            End Sub
            
            ''' <summary> Initializes a new instance of the ParseException class with a specified <see cref="ParseError"/> </summary>
             ''' <param name="ParseError"> The complete parsing error information. </param>
            Public Sub New(ParseError As ParseError)
                MyBase.New(ParseError.Message)
                _ParseError = ParseError
            End Sub
            
            ''' <summary>
             ''' Initializes a new instance of the ParseException class with a specified <see cref="ParseError"/>
             ''' and a reference to the inner exception that is the cause of this exception.
             ''' </summary>
             ''' <param name="ParseError"> The complete parsing error information. </param>
             ''' <param name="innerException">The exception that is the cause of the current exception.</param>
            Public Sub New(ParseError As ParseError, innerException As Exception)
                MyBase.New(ParseError.Message, innerException)
                _ParseError = ParseError
            End Sub
            
            
            ''' <summary> Initializes a new instance of the ParseException class with serialized data. </summary>
             ''' <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
             ''' <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
            Protected Sub New(info As SerializationInfo, context As StreamingContext)
                MyBase.New(info, context)
                
                _ParseError = New ParseError(info.GetInt32("ParseError.Level"),
                                             info.GetInt32("ParseError.LineNo"),
                                             info.GetInt32("ParseError.StartColumn"),
                                             info.GetInt32("ParseError.EndColumn"),
                                             info.GetString("ParseError.Message"),
                                             info.GetString("ParseError.Hints"), 
                                             info.GetString("ParseError.FilePath")
                                            )
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Gets detailed information about the error occured. </summary>
            ''' <value> Information about the error occured. </value>
            Public ReadOnly Property ParseError() As ParseError
                Get
                    Return _ParseError
                End Get
            End Property
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Sets the <see cref="SerializationInfo"/> with information about the exception. </summary>
             ''' <param name="info"> The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param>
             ''' <param name="context"> The <see cref="StreamingContext"/> that contains contextual information about the source or destination. </param>
            Public Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
                MyBase.GetObjectData(info, context)
                
                info.AddValue("ParseError.EndColumn", _ParseError.EndColumn)
                info.AddValue("ParseError.FilePath", _ParseError.FilePath)
                info.AddValue("ParseError.Hints", _ParseError.Hints)
                info.AddValue("ParseError.Level", _ParseError.Level)
                info.AddValue("ParseError.LineNo", _ParseError.LineNo)
                info.AddValue("ParseError.Message", _ParseError.Message)
                info.AddValue("ParseError.StartColumn", _ParseError.StartColumn)
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
