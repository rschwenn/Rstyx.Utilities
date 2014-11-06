
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
    Public MustInherit Class DataFile
        Implements IParseErrors
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.DataFile")
            
        #End Region
        
        #Region "Protected Fields"
            
            ''' <summary> If this string is found at line start, the whole line will be treated as comment line. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comment lines won't be recognized. </remarks>
            Protected LineStartCommentToken     As String = Nothing
            
            ''' <summary> If this string is found anywhere in the line, all following characters will be treated as line end comment. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comments at line end won't be recognized. </remarks>
            Protected LineEndCommentToken       As String = Nothing
            
            ''' <summary> If <see langword="true"/>, leading comment lines will be separated from the data and provided as the <see cref="IHeader.Header"/>. Defaults to <see langword="true"/>. </summary>
            Protected SeparateHeader            As Boolean = True
            
            ''' <summary> The encoding to use for the file.  Defaults to <see cref="Encoding.Default"/> </summary>
            Protected FileEncoding              As Encoding = Encoding.Default
            
            ''' <summary> The default header lines for the file. Defaults to an empty collection. </summary>
            Protected ReadOnly DefaultHeader    As New Collection(Of String)
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): DataFile instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            
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
        
        #Region "Methods"
            
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
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
