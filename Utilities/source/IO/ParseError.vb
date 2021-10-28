
Imports System

Namespace IO
    
    #Region "Enums"
        
        ''' <summary> Degree of severity of a parse error. </summary>
        Public Enum ParseErrorLevel As Integer
            
            ''' <summary> Low level. </summary>
            Warning = 0
            
            ''' <summary> High level. </summary>
            [Error] = 1
            
        End Enum
        
    #End Region
    
    ''' <summary>
    ''' Represents an error that has been occurred while parsing a text file,
    ''' or while verifying logical constraints of an arbitrary file.
    ''' </summary>
     ''' <remarks>
     ''' The error holds at least a severity level and a message.
     ''' Optional for text files is source field information stored, that enables to highlight the error source in the source text file.
     ''' </remarks>
    Public Class ParseError
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.ParseError")
            
        #End Region
        
        #Region "Constructors"
            
            Private Sub New()
            End Sub
            
            ''' <summary> Creates a new instance of ParseError without source information. </summary>
             ''' <param name="Level">   The degree of severity of the error. </param>
             ''' <param name="Message"> The error Message. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
            Public Sub New(Level   As ParseErrorLevel,
                           Message As String
                          )
                Me.New(Level, Message, Nothing, Nothing)
            End Sub
            
            ''' <summary> Creates a new instance of ParseError without source information. </summary>
             ''' <param name="Level">    The degree of severity of the error. </param>
             ''' <param name="Message">  The error Message. </param>
             ''' <param name="Hints">    Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath"> Full path of the source file. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
            Public Sub New(Level    As ParseErrorLevel,
                           Message  As String,
                           Hints    As String,
                           FilePath As String
                          )
                If (Message.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("Message")
                
                Me.Level       = Level
                Me.Message     = Message
                Me.Hints       = Hints
                Me.FilePath    = FilePath
                
                Me.HasSource   = False
            End Sub
            
            ''' <summary> Creates a new instance of ParseError with source information. </summary>
             ''' <param name="Level">       The degree of severity of the error. </param>
             ''' <param name="LineNo">      The line number in the source file, starting at 1. </param>
             ''' <param name="StartColumn"> The colulmn number in the source line determining the start of faulty string. </param>
             ''' <param name="EndColumn">   The colulmn number in the source line determining the end of faulty string. </param>
             ''' <param name="Message">     The error Message. </param>
             ''' <param name="FilePath">    Full path of the source file. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="LineNo"/> is less than 1. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="StartColumn"/> or <paramref name="EndColumn"/> is less than Zero. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="EndColumn"/> is not greater than <paramref name="StartColumn"/>. </exception>
            Public Sub New(Level       As ParseErrorLevel,
                           LineNo      As Long,
                           StartColumn As Long,
                           EndColumn   As Long,
                           Message     As String,
                           FilePath    As String
                          )
                Me.New(Level, LineNo, StartColumn, EndColumn, Message, Nothing, FilePath)
            End Sub
            
            ''' <summary> Creates a new instance of ParseError with source information. </summary>
             ''' <param name="Level">       The degree of severity of the error. </param>
             ''' <param name="LineNo">      The line number in the source file, starting at 1. </param>
             ''' <param name="StartColumn"> The colulmn number in the source line determining the start of faulty string. </param>
             ''' <param name="EndColumn">   The colulmn number in the source line determining the end of faulty string. </param>
             ''' <param name="Message">     The error Message. </param>
             ''' <param name="Hints">       Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath">    Full path of the source file. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="LineNo"/> is less than 1 (only if <paramref name="FilePath"/> is <see langword="null"/>). </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="StartColumn"/> or <paramref name="EndColumn"/> is less than Zero (only if <paramref name="FilePath"/> is <see langword="null"/>). </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="EndColumn"/> is less than <paramref name="StartColumn"/> (only if <paramref name="FilePath"/> is <see langword="null"/>). </exception>
            Public Sub New(Level       As ParseErrorLevel,
                           LineNo      As Long,
                           StartColumn As Long,
                           EndColumn   As Long,
                           Message     As String,
                           Hints       As String,
                           FilePath    As String
                          )
                If (Message.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("Message")
                If (FilePath IsNot Nothing) Then
                    Me.HasSource = True
                    If (LineNo < 1)              Then Throw New System.ArgumentException("LineNo")
                    If (StartColumn < 0)         Then Throw New System.ArgumentException("StartColumn")
                    If (EndColumn < 0)           Then Throw New System.ArgumentException("EndColumn")
                    If (EndColumn < StartColumn) Then Throw New System.ArgumentException("EndColumn")
                Else
                    Me.HasSource = False
                End If
                
                Me.Level       = Level
                Me.LineNo      = LineNo
                Me.StartColumn = StartColumn
                Me.EndColumn   = EndColumn
                Me.Message     = Message
                Me.Hints       = Hints
                Me.FilePath    = FilePath
                
            End Sub
            
        #End Region
        
        #Region "Public Fields"
            
            ''' <summary> The degree of severity of the error. </summary>
            Public ReadOnly Level           As ParseErrorLevel
            
            ''' <summary> The error message (may be multi-line). </summary>
             ''' <remarks>
             ''' It cannot be <see langword="null"/> or <c>String.Empty</c> or whitespace only.
             ''' Multiple lines has to be separated by <b>Environment.NewLine</b>.
             ''' </remarks>
            Public ReadOnly Message         As String
            
            
            ''' <summary> Determines if source information (line, columns) is available. </summary>
            Public ReadOnly HasSource       As Boolean
            
            ''' <summary> The line number in the source file, starting at 1. </summary>
            Public ReadOnly LineNo          As Long
            
            ''' <summary> The zero-based colulmn number in the source line determining the start of faulty string. </summary>
            Public ReadOnly StartColumn     As Long
            
            ''' <summary> The zero-based colulmn number in the source line determining the end of faulty string. </summary>
             ''' <remarks> Must not be less than <see cref="ParseError.StartColumn"/>. </remarks>
            Public ReadOnly EndColumn       As Long
            
            
            ''' <summary> Hints that could help the user to understand the error. </summary>
             ''' <remarks>
             ''' It may be <see langword="null"/>.
             ''' Multiple lines has to be separated by <b>Environment.NewLine</b>.
             ''' </remarks>
            Public ReadOnly Hints           As String
            
            ''' <summary> Full path of the file this error is related to. </summary>
             ''' <remarks> May be <see langword="null"/>. In this case the consumer has to know the file path by itself (i.e. <see cref="ParseErrorCollection.FilePath"/>). </remarks>
            Public ReadOnly FilePath        As String
            
        #End Region
        
        #Region "Static Factory Methods"
            
            ''' <summary> Ceates a new ParseError. Source information may come from a DataField. </summary>
             ''' <returns> A new ParseError </returns>
             ''' <typeparam name="TSourceField"> The type of <paramref name="SourceField"/>. </typeparam>
             ''' <param name="Level">       The degree of severity of the error. </param>
             ''' <param name="LineNo">      The line number in the source file, starting at 1. Values less than 1 will be treated as Zero. </param>
             ''' <param name="SourceField"> The parsed data field of point ID for precise error source hints. May be <see langword="null"/>. </param>
             ''' <param name="Message">     The error Message. </param>
             ''' <param name="Hints">       Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath">    Full path of the source file. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="LineNo"/> is less than 1. </exception>
            Public Shared Function Create(Of TSourceField)(Level       As ParseErrorLevel,
                                                           LineNo      As Long,
                                                           SourceField As DataField(Of TSourceField),
                                                           Message     As String,
                                                           Optional Hints    As String = Nothing,
                                                           Optional FilePath As String = Nothing
                                                          ) As ParseError
                Dim RetValue As ParseError
                Dim StartCol As Integer = 0
                Dim EndCol   As Integer = 0
                
                If (LineNo > 0) Then
                    If ((SourceField IsNot Nothing) AndAlso SourceField.HasSource) Then
                        StartCol = SourceField.Source.Column
                        EndCol   = SourceField.Source.Column + SourceField.Source.Length
                    End If
                    RetValue = New ParseError(Level, LineNo, StartCol, EndCol, Message, Hints, FilePath)
                Else
                    RetValue = New ParseError(Level, Message, Hints, FilePath)
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Returns a formatted error message (without column values and hints). </summary>
            Public Overrides Function ToString() As String
                Dim RetValue As String
                
                If (Me.HasSource) Then
                    RetValue = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.ParseError_ErrorLevelInLineNo, Me.Level.ToDisplayString(), Me.LineNo, Me.Message.Replace(Environment.NewLine, "  " & Environment.NewLine))
                Else
                    RetValue = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.ParseError_ErrorLevelWithoutLineNo, Me.Level.ToDisplayString(), Me.Message.Replace(Environment.NewLine, "  " & Environment.NewLine))
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
