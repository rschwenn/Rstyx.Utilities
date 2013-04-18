'   LumenWorks.Framework.IO.CSV.MissingFieldAction
'   Copyright (c) 2006 Sébastien Lorion
'
'   MIT license (http://en.wikipedia.org/wiki/MIT_License)
'
'   Permission is hereby granted, free of charge, to any person obtaining a copy
'   of this software and associated documentation files (the "Software"), to deal
'   in the Software without restriction, including without limitation the rights 
'   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
'   of the Software, and to permit persons to whom the Software is furnished to do so, 
'   subject to the following conditions:
'
'   The above copyright notice and this permission notice shall be included in all 
'   copies or substantial portions of the Software.
'
'   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
'   INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
'   PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
'   FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
'   ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


Namespace IO.CSV
    
    ''' <summary>
    ''' Specifies the action to take when a field is missing.
    ''' </summary>
    Public Enum MissingFieldAction
        ''' <summary>
        ''' Treat as a parsing error.
        ''' </summary>
        ParseError = 0
        
        ''' <summary>
        ''' Replaces by an empty value.
        ''' </summary>
        ReplaceByEmpty = 1
        
        ''' <summary>
        ''' Replaces by a null value (<see langword="null"/>).
        ''' </summary>
        ReplaceByNull = 2
    End Enum
    
    ''' <summary>
    ''' Specifies the action to take when a parsing error has occured.
    ''' </summary>
    Public Enum ParseErrorAction
        ''' <summary>
        ''' Raises the <see cref="M:CsvReader.ParseError"/> event.
        ''' </summary>
        [RaiseEvent] = 0
        
        ''' <summary>
        ''' Tries to advance to next line.
        ''' </summary>
        AdvanceToNextLine = 1
        
        ''' <summary>
        ''' Throws an exception.
        ''' </summary>
        ThrowException = 2
    End Enum
    
    ''' <summary>
    ''' Specifies which values should be trimmed.
    ''' </summary>
    <System.Flags> _
    Public Enum ValueTrimmingOptions
        
        ''' <summary>
        ''' No trimming is applied.
        ''' </summary>
        None = 0
        
        ''' <summary>
        ''' Only unquoted values are trimmed.
        ''' </summary>
        UnquotedOnly = 1
        
        ''' <summary>
        ''' Only quoted values are trimmed.
        ''' </summary>
        QuotedOnly = 2
        
        ''' <summary>
        ''' All values are trimmed.
        ''' </summary>
        All = (UnquotedOnly Or QuotedOnly)
    End Enum
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
