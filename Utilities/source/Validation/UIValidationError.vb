
Namespace Validation

  ''' <summary> Represents an UI Validation error. </summary>
  ''' <remarks> This is related to an error that occurs when updating a binding source from target fails whith a binding exception. </remarks>
  Public Class UIValidationError
    
    #Region " Declarations "
    
        Private _strDataItemName As String = String.Empty
        Private _strErrorMessage As String = String.Empty
        Private _strPropertyName As String = String.Empty
    
    #End Region
    
    #Region " Constructor"
    
        Public Sub New(ByVal strDataItemName As String, ByVal strPropertyName As String, ByVal strErrorMessage As String)
            _strDataItemName = strDataItemName
            _strPropertyName = strPropertyName
            _strErrorMessage = strErrorMessage
        End Sub
    
    #End Region
    
    #Region " Properties "
    
        Public ReadOnly Property DataItemName() As String
            Get
                Return _strDataItemName
            End Get
        End Property
    
        Public ReadOnly Property ErrorMessage() As String
            Get
                Return _strErrorMessage
            End Get
        End Property
    
        Public ReadOnly Property Key() As String
            Get
                Return String.Format("{0}:{1}", _strDataItemName, _strPropertyName)
            End Get
        End Property
    
        Public ReadOnly Property PropertyName() As String
            Get
                Return _strPropertyName
            End Get
        End Property
    
    #End Region
    
    #Region " Methods "
    
        Public Overrides Function ToString() As String
            Return String.Format("DataItem: {0}, PropertyName: {1}, Error: {2}", _strDataItemName, _strPropertyName, _strErrorMessage)
        End Function
    
    #End Region
    
  End Class
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
