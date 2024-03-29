﻿
Imports System.Collections.ObjectModel

Namespace IO
    
    ''' <summary> Defines properties for handling parse errors. </summary>
     ''' <remarks> This interface should be implemented by a class that is able to read or write a file and can parse and check it's content. </remarks>
    Public Interface IParseErrors
            
            ''' <summary> Provides access to the errors occurred while parsing file data. </summary>
            Property ParseErrors() As ParseErrorCollection
            
            ''' <summary> Determines if parse errors should be collected. Defaults to <see langword="false"/>. </summary>
             ''' <remarks>
             ''' If <see langword="false"/>, the first parse error should break an operation immediately.
             ''' Otherwise, all data will be parsed in order to get a survey of all errors.
             ''' Nevertheless the first error in a line usually aborts parsing this line. Hence, only one error per line will be stored.
             ''' </remarks>
            Property CollectParseErrors() As Boolean
            
            ''' <summary> Determines if parse errors should be shown in jEdit after parsing. Defaults to <see langword="true"/>. </summary>
            Property ShowParseErrorsInJedit() As Boolean
        
    End Interface
    
    ''' <summary> Defines an interface for maintaining simple text header meta data. </summary>
    Public Interface IHeader
            
            ''' <summary> Holds text header lines. Defaults to an empty collection. </summary>
            Property Header() As Collection(Of String)
        
    End Interface
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
