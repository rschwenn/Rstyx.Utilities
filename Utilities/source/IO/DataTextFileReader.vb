
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Namespace IO
    
    ''' <summary>  Represents a reader that can read and cache one or more whole data text files. </summary>
     ''' <remarks> The file(s) could be separated into header and data. Each source line will be pre-splitted into data and comment. </remarks>
    Public Class DataTextFileReaderDeprecated
        
        #Region "Private Fields"
            
            Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.DataTextFileReaderDeprecated")
            
            Private _Header     As New Collection(Of String)
            Private _DataCache  As New Collection(Of DataTextLine)
            Private _FilePaths  As New Collection(Of String)
            
            Private _CommentLinesCount  As Integer
            Private _DataLinesCount     As Integer
            Private _EmptyLinesCount    As Integer
            Private _TotalLinesCount    As Integer
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new instance of DataTextFileReaderDeprecated with default settings. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new instance of DataTextFileReaderDeprecated with specified settings. </summary>
             ''' <param name="LineStartCommentToken"> A string preluding a comment line. May be <see langword="null"/>. </param>
             ''' <param name="LineEndCommentToken">   A string preluding a comment at line end. May be <see langword="null"/>. </param>
             ''' <param name="SeparateHeader">        If <see langword="true"/>, leading comment lines will be separated from the data. </param>
            Public Sub New(LineStartCommentToken As String, LineEndCommentToken As String, SeparateHeader As Boolean)
                
                Me.LineStartCommentToken = LineStartCommentToken
                Me.LineEndCommentToken = LineEndCommentToken
                Me.SeparateHeader = SeparateHeader
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            #Region "Settings"
                
                ''' <summary> If this string is found at line start, the whole line will be treated as comment line. Defaults to <see langword="null"/>. </summary>
                 ''' <remarks> If <see langword="null"/> or empty, comment lines won't be recognized. </remarks>
                Public Property LineStartCommentToken() As String = Nothing
                
                ''' <summary> If this string is found anywhere in the line, all following characters will be treated as line end comment. Defaults to <see langword="null"/>. </summary>
                 ''' <remarks> If <see langword="null"/> or empty, comments at line end won't be recognized. </remarks>
                Public Property LineEndCommentToken() As String = Nothing
                
                ''' <summary> If <see langword="true"/>, leading comment lines will be separated from the data and provided as the <see cref="DataTextFileReaderDeprecated.Header"/>. Defaults to <see langword="true"/>. </summary>
                Public Property SeparateHeader() As Boolean = True
                
            #End Region
            
            #Region "Results"
                
                ''' <summary> Returns the chached data of the whole text file. </summary>
                Public ReadOnly Property DataCache() As Collection(Of DataTextLine)
                    Get
                        Return _DataCache
                    End Get
                End Property
                
                ''' <summary> Returns the Header lines of the text file. </summary>
                Public ReadOnly Property Header() As Collection(Of String)
                    Get
                        Return _Header
                    End Get
                End Property
                
                ''' <summary> Returns the list of read file paths. </summary>
                Public ReadOnly Property FilePaths() As Collection(Of String)
                    Get
                        Return _FilePaths
                    End Get
                End Property
                
            #End Region
            
            #Region "Statistics"
                
                ''' <summary> Returns the count of comment lines. </summary>
                Public ReadOnly Property CommentLinesCount() As Integer
                    Get
                        Return _CommentLinesCount
                    End Get
                End Property
                
                ''' <summary> Returns the count of lines containing data. </summary>
                Public ReadOnly Property DataLinesCount() As Integer
                    Get
                        Return _DataLinesCount
                    End Get
                End Property
                
                ''' <summary> Returns the count of empty lines. </summary>
                Public ReadOnly Property EmptyLinesCount() As Integer
                    Get
                        Return _EmptyLinesCount
                    End Get
                End Property
                
                ''' <summary> Returns the total count of read lines. </summary>
                Public ReadOnly Property TotalLinesCount() As Integer
                    Get
                        Return _TotalLinesCount
                    End Get
                End Property
                
            #End Region
            
        #End Region
        
        #Region "Public Members"
            
            ''' <summary> Loads the file using default settings for <see cref="StreamReader"/>. </summary>
             ''' <param name="Path"> The complete path of to the data text file to be read (for <see cref="StreamReader"/>). </param>
             ''' <remarks>
             ''' <para>
             ''' The default settings for <see cref="StreamReader"/> are: UTF-8, not detect encoding, buffersize 1024.
             ''' </para>
             ''' <para>
             ''' The loaded data will be appended to the <see cref="DataTextFileReaderDeprecated.DataCache"/> and  <see cref="DataTextFileReaderDeprecated.Header"/> properties.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.ArgumentException">             <paramref name="Path"/> is empty. </exception>
             ''' <exception cref="System.ArgumentNullException">         <paramref name="Path"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.IO.FileNotFoundException">      The file cannot be found. </exception>
             ''' <exception cref="System.IO.DirectoryNotFoundException"> The specified path is invalid, such as being on an unmapped drive. </exception>
             ''' <exception cref="System.NotSupportedException">         <paramref name="Path"/> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
             ''' <exception cref="System.OutOfMemoryException">          There is insufficient memory to allocate a buffer for the returned string. </exception>
             ''' <exception cref="System.IO.IOException">                An I/O error occurs. </exception>
            Public Sub Load(Path As String)
                Me.Load(Path, Encoding.UTF8, DetectEncodingFromByteOrderMarks:=False, BufferSize:=1024)
            End Sub
            
            ''' <summary> Loads the file using specified settings for the used <see cref="StreamReader"/>. </summary>
             ''' <param name="Path">                             The complete path to the data text file to be read. </param>
             ''' <param name="Encoding">                         The character encoding to use. </param>
             ''' <param name="DetectEncodingFromByteOrderMarks"> Indicates whether to look for byte order marks at the beginning of the file. </param>
             ''' <param name="BufferSize">                       The minimum buffer size, in number of 16-bit characters. </param>
             ''' <remarks> The loaded data will be appended to the <see cref="DataTextFileReaderDeprecated.DataCache"/> and  <see cref="DataTextFileReaderDeprecated.Header"/> properties. </remarks>
             ''' <exception cref="System.ArgumentException">             <paramref name="Path"/> is empty. </exception>
             ''' <exception cref="System.ArgumentNullException">         <paramref name="Path"/> or <paramref name="Encoding"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.IO.FileNotFoundException">      The file cannot be found. </exception>
             ''' <exception cref="System.IO.DirectoryNotFoundException"> The specified path is invalid, such as being on an unmapped drive. </exception>
             ''' <exception cref="System.NotSupportedException">         <paramref name="Path"/> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
             ''' <exception cref="System.ArgumentOutOfRangeException">   <paramref name="BufferSize"/> is less than or equal to zero. </exception>
             ''' <exception cref="System.OutOfMemoryException">          There is insufficient memory to allocate a buffer for the returned string. </exception>
             ''' <exception cref="System.IO.IOException">                An I/O error occurs. </exception>
            Public Sub Load(Path As String,
                            Encoding As Encoding,
                            DetectEncodingFromByteOrderMarks As Boolean,
                            BufferSize As Integer
                           )
                Using oSR As New System.IO.StreamReader(Path, Encoding, DetectEncodingFromByteOrderMarks, BufferSize)
                    
                    Dim CurrentLine     As String
                    Dim DataLine        As DataTextLine
                    Dim CheckHeaderLine As Boolean = Me.SeparateHeader
                    Dim IsHeaderLine    As Boolean = False
                    Dim FileIndex       As Integer = Me.FilePaths.Count
                    
                    ' Register file.
                    Me.FilePaths.Add(Path)
                    
                    ' Read file.
                    Do While (Not oSR.EndOfStream)
                        CurrentLine = oSR.ReadLine()
                        
                        _TotalLinesCount += 1
                        DataLine = New DataTextLine(CurrentLine, Me.LineStartCommentToken, Me.LineEndCommentToken)
                        DataLine.SourceLineNo = _TotalLinesCount
                        'DataLine.SourceFileIndex = FileIndex
                        
                        ' Process header.
                        If (CheckHeaderLine) Then
                            If (DataLine.IsCommentLine) Then
                                IsHeaderLine = True
                                _Header.Add(DataLine.Comment)
                            Else
                                CheckHeaderLine = False
                                IsHeaderLine = False
                            End If
                        End If
                        
                        ' Process data lines.
                        If (Not IsHeaderLine) Then
                            _DataCache.Add(DataLine)
                            
                            If (DataLine.IsEmpty) Then
                                _EmptyLinesCount += 1
                            ElseIf (DataLine.IsCommentLine) Then
                                _CommentLinesCount += 1
                            ElseIf (DataLine.HasData) Then
                                _DataLinesCount += 1
                            End If
                        End If
                    Loop
                End Using
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
