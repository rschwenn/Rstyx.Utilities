
Namespace IO
    
    ''' <summary> Defines properties for handling parse errors. </summary>
     ''' <remarks> This interface should be implemented by a class that is able to (read and) parse a text file. </remarks>
    Public Interface IParseErrors
            
            ''' <summary> Provides access to the errors occurred while parsing the file. </summary>
            ReadOnly Property ParseErrors() As ParseErrorCollection
            
            ''' <summary> Determines if parse errors should be collected. Defaults to <see langword="false"/>. </summary>
             ''' <remarks>
             ''' If <see langword="false"/>, the first parse error breaks reading the file immediately.
             ''' Otherwise, all lines will be parsed in order to get a survey of all lines with errors.
             ''' Nevertheless the first error in a line usually aborts parsing this line. Hence, only one error per line will be stored.
             ''' </remarks>
            Property CollectParseErrors() As Boolean
            
            ''' <summary> Determines if parse errors should be shown in jEdit after parsing. Defaults to <see langword="false"/>. </summary>
            Property ShowParseErrorsInJedit() As Boolean
        
    End Interface
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
