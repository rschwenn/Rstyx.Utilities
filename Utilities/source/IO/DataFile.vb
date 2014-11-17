
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Imports Rstyx.Utilities

Namespace IO
    
    ''' <summary> The base class for reading and writing data files. </summary>
     ''' <remarks>
     ''' A derived class will support to read and write exactly one discrete file type.
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description> Implements <see cref="IParseErrors"/> in order to support error handling. </description></item>
     ''' <item><description> Some settings as protected fields. </description></item>
     ''' <item><description> The <see cref="DataFile.CreateFileHeader"/> method provides basic support in creating a standard header for a target text file. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public Class DataFile
        Implements IParseErrors
        Implements IHeader
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.DataFile")
            
            ''' <summary> Indicates whether or not <see cref="DataFile.FilePath"/> has changed since last reading. </summary>
            Protected IsFilePathChanged As Boolean = False
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): DataFile instantiated")
            End Sub
            
            ''' <summary> Creates a new instance with a given file path. </summary>
             ''' <param name="FilePath"> The file path of the data file to be read or write. </param>
            Public Sub New(FilePath As String)
                _FilePath = FilePath
                IsFilePathChanged = True
                Logger.logDebug("New(): DataFile instantiated for file: " & FilePath)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Dim _FilePath As String = Nothing
            
            ''' <summary> The file path of the data file to read or write. </summary>
            Public Property FilePath() As String
                Get
                    Return _FilePath
                End Get
                Set(Value As String)
                    If (Not (value = _FilePath)) then
                        _FilePath = Value
                        IsFilePathChanged = True
                    End If
                End Set
            End Property
            
            ''' <summary> The encoding to use for the file.  Defaults to <see cref="Encoding.Default"/> </summary>
            Public Property FileEncoding()          As Encoding = Encoding.Default
            
            ''' <summary> The minimum buffer size, in number of 16-bit characters. Defaults to 1024. </summary>
            Public Property BufferSize()            As Integer = 1024
            
            ''' <summary> Indicates whether to look for byte order marks at the beginning of the file. Defaults to <see langword="false"/>. </summary>
            Public Property DetectEncodingFromByteOrderMarks() As Boolean = False
            
            ''' <summary> If this string is found at line start, the whole line will be treated as comment line. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comment lines won't be recognized. </remarks>
            Public Property LineStartCommentToken() As String = Nothing
            
            ''' <summary> If this string is found anywhere in the line, all following characters will be treated as line end comment. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comments at line end won't be recognized. </remarks>
            Public Property LineEndCommentToken()   As String = Nothing
            
            ''' <summary> If <see langword="true"/>, leading comment lines will be separated from the data and provided as the <see cref="DataFile.Header"/>. Defaults to <see langword="true"/>. </summary>
            Public Property SeparateHeader()        As Boolean = True
            
            ''' <summary> The default header lines for the file. Defaults to an empty collection. </summary>
            Public Property DefaultHeader()         As New Collection(Of String)
            
            ' ''' <summary> A character that delimits words in <see cref="DataTextLine.Data"/>. </summary>
            ' ''' <remarks> A whitespace character means the whole whitespace between words. </remarks>
            'Public Property FieldDelimiter() As Char
            
        #End Region
        
        #Region "IHeader Members"
            
            ''' <summary> Gets or sets the Header lines of the text file. </summary>
            Public Property Header() As Collection(Of String) Implements IHeader.Header
            
        #End Region
        
        #Region "IParseErrors Members"
            
            Private _ParseErrors As ParseErrorCollection
            
            ''' <inheritdoc/>
            Public Property ParseErrors() As ParseErrorCollection Implements IParseErrors.ParseErrors
                Get
                    If _ParseErrors Is Nothing Then
                        _ParseErrors = New ParseErrorCollection()
                    End If
                    Return _ParseErrors
                End Get
                Set(value As ParseErrorCollection)
                    _ParseErrors = value
                End Set
            End Property
            
            ''' <inheritdoc/>
            Public Property CollectParseErrors() As Boolean = False Implements IParseErrors.CollectParseErrors
            
            ''' <inheritdoc/>
            Public Property ShowParseErrorsInJedit() As Boolean = True Implements IParseErrors.ShowParseErrorsInJedit

        #End Region
        
        #Region "I/O Operations"
            
            ''' <summary> Reads the file as text file and provides the lines as lazy established enumerable. </summary>
             ''' <remarks>
             ''' <para>
             ''' If a file header is recognized it will be stored in <see cref="DataFile.Header"/> property before yielding the first data line.
             ''' </para>
             ''' </remarks>
             ''' <returns> All lines of <see cref="DataFile.FilePath"/> except header lines as lazy established enumerable. </returns>
             ''' <exception cref="System.InvalidOperationException">     <see cref="DataFile.FilePath"/> is <see langword="null"/> or empty. </exception>
             ''' <exception cref="System.InvalidOperationException">     <see cref="DataFile.FileEncoding"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.IO.FileNotFoundException">      The file cannot be found. </exception>
             ''' <exception cref="System.IO.DirectoryNotFoundException"> The specified path is invalid, such as being on an unmapped drive. </exception>
             ''' <exception cref="System.NotSupportedException">         <see cref="DataFile.FilePath"/> includes an incorrect or invalid syntax for file name, directory name, or volume label. </exception>
             ''' <exception cref="System.ArgumentOutOfRangeException">   <see cref="DataFile.BufferSize"/> is less than or equal to zero. </exception>
             ''' <exception cref="System.OutOfMemoryException">          There is insufficient memory to allocate a buffer for the returned string. </exception>
             ''' <exception cref="System.IO.IOException">                An I/O error occurs. </exception>
            Public ReadOnly Iterator Property DataLineStream() As IEnumerable(Of DataTextLine)
                Get
                    If (Me.FilePath.IsEmptyOrWhiteSpace()) Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.DataFile_MissingFilePath)
                    If (Me.FileEncoding Is Nothing)        Then Throw New System.InvalidOperationException(Rstyx.Utilities.Resources.Messages.DataFile_MissingEncoding)
                    
                    Me.Reset (Me.FilePath)
                    
                    Using oSR As New System.IO.StreamReader(Me.FilePath, Me.FileEncoding, Me.DetectEncodingFromByteOrderMarks, Me.BufferSize)
                        
                        Dim LineCount       As Long = 0
                        Dim CurrentLine     As String
                        Dim DataLine        As DataTextLine
                        Dim CheckHeaderLine As Boolean = Me.SeparateHeader
                        Dim IsHeaderLine    As Boolean = False
                        
                        ' Read file.
                        Do While (Not oSR.EndOfStream)
                            
                            CurrentLine = oSR.ReadLine()
                            
                            LineCount += 1
                            DataLine = New DataTextLine(CurrentLine, Me.LineStartCommentToken, Me.LineEndCommentToken)
                            DataLine.SourceLineNo = LineCount
                            DataLine.SourceFileIndex = Me.FilePath
                            
                            ' Store header.
                            If (CheckHeaderLine) Then
                                If (DataLine.IsCommentLine) Then
                                    IsHeaderLine = True
                                    If (Not Me.DefaultHeader.Contains(DataLine.Comment)) Then
                                        _Header.Add(DataLine.Comment)
                                    End If
                                Else
                                    CheckHeaderLine = False
                                    IsHeaderLine = False
                                End If
                            End If
                            
                            ' Yield a data line.
                            If (Not IsHeaderLine) Then
                                Yield DataLine
                            End If
                        Loop
                    End Using
                End Get
            End Property
            
            Private _DataLineBuffer As Collection(Of DataTextLine) = Nothing
            
            ''' <summary> Returns the buffered data lines of the whole text file. </summary>
             ''' <remarks>
             ''' If the buffer is empty or <see cref="DataFile.FilePath"/> has changed after last invoking this property,
             ''' the file will be read.
             ''' </remarks>
            Public Property DataLineBuffer() As Collection(Of DataTextLine)
                Get
                    If (IsFilePathChanged OrElse (_DataLineBuffer Is Nothing)) Then
                        _DataLineBuffer = New Collection(Of DataTextLine)
                        For Each DataLine As DataTextLine In Me.DataLineStream
                            _DataLineBuffer.Add(DataLine)
                        Next
                        IsFilePathChanged = False
                    End If
                    Return _DataLineBuffer
                End Get
                Set(Value As Collection(Of DataTextLine))
                    _DataLineBuffer = Value
                End Set
            End Property
            
        #End Region
        
        #Region "Protected Members"
            
            ''' <summary> Creates a byte array from a string. </summary>
             ''' <param name="TheEncoding"> The encoding to use. </param>
             ''' <param name="text">        Input string </param>
             ''' <param name="Length">      Given length of the byte array to return. </param>
             ''' <param name="FillChar">    If <paramref name="text"/> is shorter than <paramref name="Length"/>, it will be filled with this character. </param>
             ''' <returns> A byte array with given <paramref name="Length"/>. </returns>
             ''' <remarks> The input string will be trimmed to <paramref name="Length"/>. </remarks>
            Protected Function GetByteArray(TheEncoding As Encoding, text As String, Length As Integer, FillChar As Char, Optional AdjustAtRight As Boolean = False) As Byte()
                Dim TrimmedInput As String = text
                If (TrimmedInput.Length > Length) Then
                    TrimmedInput = text.Left(Length)
                ElseIf (TrimmedInput.Length < Length) Then
                    If (AdjustAtRight) Then
                        TrimmedInput = text.PadLeft(Length, FillChar)
                    Else
                        TrimmedInput = text.PadRight(Length, FillChar)
                    End If
                End If
                Return TheEncoding.GetBytes(TrimmedInput)
            End Function
            
            ''' <summary> Creates the header for the file to write. </summary>
             ''' <param name="MetaData"> The object to get individual header lines from. </param>
            Protected Overridable Function CreateFileHeader(MetaData As IHeader) As StringBuilder
                
                Dim HeaderLines As New StringBuilder()
                
                ' Individual Header.
                For Each HeaderLine As String In MetaData.Header
                    HeaderLines.Append(LineStartCommentToken)
                    HeaderLines.AppendLine(HeaderLine)
                Next
                
                ' Default Header.
                For Each DefaultHeaderLine As String In Me.DefaultHeader
                    HeaderLines.Append(LineStartCommentToken)
                    HeaderLines.AppendLine(DefaultHeaderLine)
                Next
                
                Return HeaderLines
            End Function
            
            ''' <summary> Resets data in this <see cref="DataFile"/> and re-initializes it with a new file path. </summary>
             ''' <param name="FilePath"> The File path for <see cref="ParseErrorCollection.FilePath"/>. May be <see langword="null"/>. </param>
            Protected Overridable Sub Reset(FilePath As String)
                _DataLineBuffer = Nothing
                Me.Header.Clear()
                Me.ParseErrors.Clear()
                Me.ParseErrors.FilePath = FilePath
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
