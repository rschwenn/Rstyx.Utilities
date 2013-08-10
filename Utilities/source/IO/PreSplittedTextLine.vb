
Namespace IO
    
    ''' <summary> Represents a line of a data text file, pre-splitted into data and comment. </summary>
    Public Structure PreSplittedTextLine
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.PreSplittedTextLine")
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new instance and splits the given <paramref name="TextLine"/> immediately. </summary>
             ''' <param name="TextLine">              The original text line. </param>
             ''' <param name="LineStartCommentToken"> A string preluding a comment line. May be <see langword="null"/>. </param>
             ''' <param name="LineEndCommentToken">   A string preluding a comment at line end. May be <see langword="null"/>. Won't be recognized if <paramref name="TextLine"/> starts with <paramref name="LineStartCommentToken"/>. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="TextLine"/> is <see langword="null"/>. </exception>
            Public Sub New(TextLine As String, LineStartCommentToken As String, LineEndCommentToken As String)
                
                If (TextLine Is Nothing) Then Throw New System.ArgumentNullException("TextLine")
                
                parseLine(TextLine, LineStartCommentToken, LineEndCommentToken, 
                          Me.Comment, Me.Data,
                          Me.HasData, Me.HasComment,
                          Me.IsCommentLine, Me.IsEmpty
                         )
            End Sub
            
        #End Region
        
        #Region "Public Fields"
            
            ''' <summary> The comment part of this line. Defaults to <c>String.Empty</c>. </summary>
             ''' <remarks> This won't be trimmed, but it's <c>String.Empty</c> if the comment part of this line only consists of whitespaces. </remarks>
            Public ReadOnly Comment         As String
            
            ''' <summary> The data part of this line. Defaults to <c>String.Empty</c>. </summary>
             ''' <remarks> This won't be trimmed, but it's <c>String.Empty</c> if the data part of this line only consists of whitespaces. </remarks>
            Public ReadOnly Data            As String
            
            ''' <summary> If <see langword="true"/>, the <see cref="P:PreSplittedTextLine.Data"/> property isn't <c>String.Empty</c>. </summary>
            Public ReadOnly HasData         As Boolean
            
            ''' <summary> If <see langword="true"/>, the <see cref="P:PreSplittedTextLine.Comment"/> property isn't <c>String.Empty</c>. </summary>
            Public ReadOnly HasComment      As Boolean
            
            ''' <summary> If <see langword="true"/>, this line starts whith an comment token. </summary>
            Public ReadOnly IsCommentLine   As Boolean
            
            ''' <summary> If <see langword="true"/>, this line consists of not more than whitespaces. </summary>
            Public ReadOnly IsEmpty         As Boolean
            
            ''' <summary> The line number in the source file. </summary>
            Public SourceLineNo             As Long
            
            ''' <summary> A string preluding a comment line. May be <see langword="null"/>. </summary>
            Public LineStartCommentToken    As String
            
            ''' <summary> A string preluding a comment at line end. May be <see langword="null"/>. </summary>
            Public LineEndCommentToken      As String
            
            ''' <summary> An index into a list of source files (which may be mmaintained in a parent object). </summary>
            Public SourceFileIndex          As Long
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Re-creates and returns the full (nearly original) line. </summary>
            Public Function getFullLine() As String
                Dim RetValue As String = String.Empty
                
                If (Not Me.IsEmpty) Then
                    If (Me.IsCommentLine) Then
                        RetValue = Me.LineStartCommentToken & Me.Comment
                    Else
                        If (Me.HasData) Then
                            RetValue = Me.Data
                            If (Me.HasComment) Then
                                RetValue &= Me.LineEndCommentToken & Me.Comment
                            End If
                        Else
                            RetValue = " " & Me.LineEndCommentToken & Me.Comment
                        End If
                    End If
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Private Methods"
            
            ''' <summary> Splits the given <paramref name="TextLine"/> and provides the result via output parameters. </summary>
             ''' <param name="TextLine">              The text line to parse / split </param>
             ''' <param name="LineStartCommentToken"> A string preluding a comment line. May be <see langword="null"/>. </param>
             ''' <param name="LineEndCommentToken">   A string preluding a comment at line end. May be <see langword="null"/>. </param>
             ''' <param name="Comment">               [Out] See matching public field. </param>
             ''' <param name="Data">                  [Out] See matching public field. </param>
             ''' <param name="HasData">               [Out] See matching public field. </param>
             ''' <param name="HasComment">            [Out] See matching public field. </param>
             ''' <param name="IsCommentLine">         [Out] See matching public field. </param>
             ''' <param name="IsEmpty">               [Out] See matching public field. </param>
             ''' <remarks></remarks>
            Private Sub parseLine(TextLine As String,
                                  LineStartCommentToken As String,
                                  LineEndCommentToken As String,
                                  ByRef Comment As String,
                                  ByRef Data As String,
                                  ByRef HasData As Boolean,
                                  ByRef HasComment As Boolean,
                                  ByRef IsCommentLine As Boolean,
                                  ByRef IsEmpty As Boolean
                                 )
                Comment         = String.Empty
                Data            = String.Empty
                HasData         = False
                HasComment      = False
                IsCommentLine   = False
                IsEmpty         = True
                
                If (TextLine.IsNotEmptyOrWhiteSpace()) Then
                    
                    IsEmpty = False
                    
                    If ((LineStartCommentToken IsNot Nothing) AndAlso (TextLine.StartsWith(LineStartCommentToken, System.StringComparison.Ordinal))) Then
                        ' TextLine starts with comment token.
                        IsCommentLine = True
                        
                        Dim Comm As String = TextLine.Substring(LineStartCommentToken.Length)
                        If (Comm.IsNotEmptyOrWhiteSpace()) Then
                            HasComment = True
                            If (Comm.StartsWith(" ", System.StringComparison.Ordinal)) Then
                                Comment = Comm.Substring(1)
                            Else
                                Comment = Comm
                            End If
                        End If
                        
                    ElseIf ((LineEndCommentToken IsNot Nothing) AndAlso (TextLine.Contains(LineEndCommentToken))) Then
                        ' TextLine contains comment token.
                        Dim LocalData As String = TextLine.Left(LineEndCommentToken, IncludeDelimiter:=False)
                        Dim LocalComm As String = TextLine.Right(LineEndCommentToken, IncludeDelimiter:=False)
                        
                        If (LocalData.IsNotEmptyOrWhiteSpace()) Then
                            HasData = True
                            Data = LocalData
                        End If
                        
                        If (LocalComm.IsNotEmptyOrWhiteSpace()) Then
                            HasComment = True
                            If (LocalComm.StartsWith(" ", System.StringComparison.Ordinal)) Then
                                Comment = LocalComm.Substring(1)
                            Else
                                Comment = LocalComm
                            End If
                        End If
                    Else
                        ' TextLine doesn't contain any comment token.
                        HasData = True
                        Data = TextLine
                    End If
                End If
            End Sub
            
        #End Region
        
    End Structure
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
