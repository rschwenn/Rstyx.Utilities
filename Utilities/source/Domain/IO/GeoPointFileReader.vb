
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
    
    ''' <summary> Supported file formats for reading and writing a <see cref="GeoPointList"/> to or from. </summary>
    Public Enum GeoPointFileFormat As Integer
        
        ''' <summary> No format specified. </summary>
        None = 0
        
        ''' <summary> VE Ascii. </summary>
        KV = 1
        
        ''' <summary> VE binary. </summary>
        KF = 2
        
        ''' <summary> iPkt (Ascii). </summary>
        iPkt = 3
        
    End Enum
    
    ''' <summary> Types of points implementing <see cref="IGeoPoint"/>. </summary>
    Public Enum GeoPointType As Integer
        
        ''' <summary> No format specified (hence default <see cref="GeoPoint"/>). </summary>
        None = 0
        
        ''' <summary> Verm.esn (<see cref="GeoVEPoint"/>). </summary>
        VE = 1
        
        ''' <summary> iPkt (<see cref="GeoIPoint"/>). </summary>
        iPkt = 2
        
        ''' <summary> Point with track geometry values (<see cref="GeoTcPoint"/>). </summary>
        TC = 3
        
    End Enum
    
    ''' <summary> The base class for reading GeoPoint files. </summary>
     ''' <remarks>
     ''' A derived class will be support reading exactly one discrete file type.
     ''' <para>
     ''' <b>Features:</b>
     ''' <list type="bullet">
     ''' <item><description>  </description></item>
     ''' <item><description> The read points will be returned by the Load method as <see cref="GeoPointList"/>. </description></item>
     ''' <item><description> Implements <see cref="IParseErrors"/> in order to support error handling. </description></item>
     ''' <item><description> Provides the <see cref="GeoPointFileReader.Constraints"/> property in order to outline constraints violation in source file. </description></item>
     ''' </list>
     ''' </para>
     ''' </remarks>
    Public MustInherit Class GeoPointFileReader
        Implements IParseErrors
        
        #Region "Private Fields"
            
            Private Shared Logger   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.IO.GeoPointFileReader")
            
        #End Region
        
        #Region "Protected Fields"
            
            ''' <summary> If this string is found at line start, the whole line will be treated as comment line. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comment lines won't be recognized. </remarks>
            Protected LineStartCommentToken  As String = Nothing
            
            ''' <summary> If this string is found anywhere in the line, all following characters will be treated as line end comment. Defaults to <see langword="null"/>. </summary>
             ''' <remarks> If <see langword="null"/> or empty, comments at line end won't be recognized. </remarks>
            Protected LineEndCommentToken    As String = Nothing
            
            ''' <summary> If <see langword="true"/>, leading comment lines will be separated from the data and provided as the <see cref="GeoPointList.Header"/>. Defaults to <see langword="true"/>. </summary>
            Protected SeparateHeader         As Boolean = True
            
            ''' <summary> The encoding to use for reading the file. </summary>
            Protected FileEncoding           As Encoding = Encoding.Default
            
        #End Region
        
        #Region "Constuctors"
            
            ''' <summary> Creates a new instance. </summary>
            Public Sub New()
                Logger.logDebug("New(): GeoPointFileReader instantiated")
            End Sub
            
        #End Region
        
        #Region "Properties"
            
           ''' <summary> Determines logical constraints to be considered for the intended usage of read points. Defaults to <c>None</c>. </summary>
            ''' <remarks>
            ''' This value will be forwarded to <see cref="GeoPointList.Constraints"/> of the point list created by <see cref="GeoPointFileReader.Load"/>.
            ''' If any of these contraints is injured while loading file, a <see cref="ParseError"/> will be created.
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
             ''' If this method fails, <see cref="GeoPointFileReader.ParseErrors"/> should provide the parse errors occurred."
             ''' </remarks>
             ''' <exception cref="ParseException">  At least one error occurred while parsing, hence <see cref="GeoPointFileReader.ParseErrors"/> isn't empty. </exception>
             ''' <exception cref="RemarkException"> Wraps any other exception. </exception>
            Public MustOverride Function Load(FilePath As String) As GeoPointList
            
        #End Region
        
        #Region "Overrides"
            
            ' ''' <summary> Returns a list of all points in one string. </summary>
            'Public Overrides Function ToString() As String
            '    
            '    Return PointList.ToString()
            'End Function
            
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
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
