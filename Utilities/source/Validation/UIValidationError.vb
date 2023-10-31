
Namespace Validation

  ''' <summary> Represents an UI Validation error. </summary>
  ''' <remarks> This is related to an error that occurs when updating a binding source from target fails with a ValidationError. </remarks>
  Public Class UIValidationError
    
    #Region "Declarations"
    
        Private ReadOnly _DataItemName As String
        Private ReadOnly _ErrorMessage As String
        Private ReadOnly _PropertyName As String
        Private ReadOnly _Label        As String
    
    #End Region
    
    #Region "Constructor"
        
        ''' <summary> Creates a new UIValidationError. </summary>
         ''' <param name="DataItemName"> Name of the class that hosts the bound property. </param>
         ''' <param name="PropertyName"> Name of bound property, caused the validation error on updating. </param>
         ''' <param name="Label">        An optional label for displaying to the user instead of the property name. </param>
         ''' <param name="ErrorMessage"> The error Message(s). </param>
        Public Sub New(ByVal DataItemName As String, ByVal PropertyName As String, ByVal Label As String, ByVal ErrorMessage As String)
            _DataItemName = DataItemName
            _PropertyName = PropertyName
            _ErrorMessage = ErrorMessage
            _Label        = Label
        End Sub
    
    #End Region
    
    #Region "Properties"
        
        ''' <summary> Name of the class that hosts the bound property. </summary>
        Public ReadOnly Property DataItemName() As String
            Get
                Return _DataItemName
            End Get
        End Property
    
        ''' <summary> The error Message(s). </summary>
        Public ReadOnly Property ErrorMessage() As String
            Get
                Return _ErrorMessage
            End Get
        End Property
    
        ''' <summary> Key for the internal errors dictionary. </summary>
        Public ReadOnly Property Key() As String
            Get
                Return String.Format("{0}:{1}", _DataItemName, _PropertyName)
            End Get
        End Property
    
        ''' <summary> Name of bound property, caused the validation error on updating. </summary>
        Public ReadOnly Property PropertyName() As String
            Get
                Return _PropertyName
            End Get
        End Property
    
        ''' <summary> An optional label for displaying to the user instead of the property name. </summary>
        Public ReadOnly Property Label() As String
            Get
                Return _Label
            End Get
        End Property
    
    #End Region
    
    #Region "Overrides"

        ''' <inheritdoc/>
        Public Overrides Function ToString() As String
            Return String.Format("DataItem: {0}, PropertyName: {1}, Error: {2}", _DataItemName, _PropertyName, _ErrorMessage)
        End Function
    
    #End Region
    
  End Class
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
