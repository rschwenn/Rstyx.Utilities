
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text

Imports PGK.Extensions
Imports Rstyx.Utilities
Imports Rstyx.Utilities.Collections
Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain.IO
    
    ' ''' <summary> Supported file formats for reading and writing a <see cref="GeoPointList"/> to or from. </summary>
    ' Public Enum GeoPointFileFormat As Integer
    '     
    '     ''' <summary> No format specified. </summary>
    '     None = 0
    '     
    '     ''' <summary> VE Ascii. </summary>
    '     KV = 1
    '     
    '     ''' <summary> VE binary. </summary>
    '     KF = 2
    '     
    '     ''' <summary> iPkt (Ascii). </summary>
    '     iPkt = 3
    '     
    ' End Enum
    ' 
    ' ''' <summary> Types of points implementing <see cref="IGeoPoint"/>. </summary>
    ' Public Enum GeoPointType As Integer
    '     
    '     ''' <summary> No format specified (hence default <see cref="GeoPoint"/>). </summary>
    '     None = 0
    '     
    '     ''' <summary> Verm.esn (<see cref="GeoVEPoint"/>). </summary>
    '     VE = 1
    '     
    '     ''' <summary> iPkt (<see cref="GeoIPoint"/>). </summary>
    '     iPkt = 2
    '     
    '     ''' <summary> Point with track geometry values (<see cref="GeoTcPoint"/>). </summary>
    '     TC = 3
    '     
    ' End Enum
    
    ''' <summary> The base class for reading and writing GeoPoint files. </summary>
     ''' <remarks>
     ''' A derived class will be support to read and write exactly one discrete file type.
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description>  </description></item>
     ''' <item><description> The <see cref="GeoPointFile.Load"/> method reads the file and returnes the read points as <see cref="GeoPointList"/>. </description></item>
     ''' <item><description> The <see cref="GeoPointFile.Store"/> method writes a given <see cref="GeoPointList"/> to the file. </description></item>
     ''' <item><description> Implements <see cref="IParseErrors"/> in order to support error handling. </description></item>
     ''' <item><description> Provides the <see cref="GeoPointFile.Constraints"/> property in order to outline constraints violation in source file. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public MustInherit Class GeoPointFile
        Implements IParseErrors
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.GeoPointFile")
            
        #End Region
        
        #Region "Protected Fields"
            
            ''' <summary> If this string is found at line start, the whole line will be treated as comment line. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comment lines won't be recognized. </remarks>
            Protected LineStartCommentToken     As String = Nothing
            
            ''' <summary> If this string is found anywhere in the line, all following characters will be treated as line end comment. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comments at line end won't be recognized. </remarks>
            Protected LineEndCommentToken       As String = Nothing
            
            ''' <summary> If <see langword="true"/>, leading comment lines will be separated from the data and provided as the <see cref="GeoPointList.Header"/>. Defaults to <see langword="true"/>. </summary>
            Protected SeparateHeader            As Boolean = True
            
            ''' <summary> The encoding to use for the file. </summary>
            Protected FileEncoding              As Encoding = Encoding.Default
            
            ''' <summary> The default header lines for the file. </summary>
            Protected ReadOnly DefaultHeader    As New Collection(Of String)
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): GeoPointFile instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
           ''' <summary> Determines logical constraints for the intended usage of points. Defaults to <c>None</c>. </summary>
            ''' <remarks>
            ''' <para>
            ''' This property takes only effect when loading a file.
            ''' </para>
            ''' <para>
            ''' If any of these contraints is violated while loading the file, a <see cref="ParseError"/> will be created.
            ''' </para>
            ''' </remarks>
           Public Property Constraints() As GeoPointConstraints
            
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
            Public Property ShowParseErrorsInJedit() As Boolean = False Implements IParseErrors.ShowParseErrorsInJedit

        #End Region
        
        #Region "Methods"
            
            ''' <summary> Reads the point file and fills the points collection. </summary>
             ''' <param name="FilePath"> File to read from. </param>
             ''' <returns> All read points as <see cref="GeoPointList"/>. </returns>
             ''' <remarks>
             ''' If this method fails, <see cref="GeoPointFile.ParseErrors"/> should provide the parse errors occurred."
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFile.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public MustOverride Function Load(FilePath As String) As GeoPointList
            
            ''' <summary> Writes the points collection to the point file. </summary>
             ''' <param name="PointList"> The points to store. </param>
             ''' <param name="FilePath">  File to store the points into. </param>
             ''' <exception cref="RemarkException"> Wraps any exception. </exception>
            Public MustOverride Sub Store(PointList As GeoPointList, FilePath As String)
            
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
             ''' <param name="PointList"> The point list to get individual header lines from. </param>
            Protected Overridable Function CreateFileHeader(PointList As GeoPointList) As StringBuilder
                
                Dim HeaderLines As New StringBuilder()
                
                ' Individual Header.
                For Each HeaderLine As String In PointList.Header
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
