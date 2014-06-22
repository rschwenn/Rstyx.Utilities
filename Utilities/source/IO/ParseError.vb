﻿
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
    
    ''' <summary> Represents an error that has been occurred while parsing a text file. </summary>
    Public Class ParseError
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.ParseError")
            
        #End Region
        
        #Region "Constructors"
            
            Private Sub New()
            End Sub
            
            ''' <summary> Creates a new instance of ParseError. </summary>
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
            
            ''' <summary> Creates a new instance of ParseError. </summary>
             ''' <param name="Level">       The degree of severity of the error. </param>
             ''' <param name="LineNo">      The line number in the source file, starting at 1. </param>
             ''' <param name="StartColumn"> The colulmn number in the source line determining the start of faulty string. </param>
             ''' <param name="EndColumn">   The colulmn number in the source line determining the end of faulty string. </param>
             ''' <param name="Message">     The error Message. </param>
             ''' <param name="Hints">       Hints that could help the user to understand the error. </param>
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
                           Hints       As String,
                           FilePath    As String
                          )
                If (Message.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("Message")
                If (LineNo < 1) Then Throw New System.ArgumentException("LineNo")
                If (StartColumn < 0) Then Throw New System.ArgumentException("StartColumn")
                If (EndColumn < 0) Then Throw New System.ArgumentException("EndColumn")
                If ((StartColumn > 0) AndAlso (EndColumn <= StartColumn)) Then Throw New System.ArgumentException("EndColumn")
                
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
            
            ''' <summary> The line number in the source file, starting at 1. </summary>
            Public ReadOnly LineNo          As Long
            
            ''' <summary> The colulmn number in the source line determining the start of faulty string. </summary>
             ''' <remarks> The first column is 1. Zero means "not determined". </remarks>
            Public ReadOnly StartColumn     As Long
            
            ''' <summary> The colulmn number in the source line determining the end of faulty string (starting at 1). </summary>
             ''' <remarks> Zero means "not determined". If not zero, it has to be greater than <see cref="ParseError.StartColumn"/>. </remarks>
            Public ReadOnly EndColumn       As Long
            
            ''' <summary> The error message (may be multi-line). </summary>
             ''' <remarks>
             ''' It cannot be <see langword="null"/> or <c>String.Empty</c> or whitespace only.
             ''' Multiple lines has to be separated by <b>Environment.NewLine</b>.
             ''' </remarks>
            Public ReadOnly Message         As String
            
            ''' <summary> Hints that could help the user to understand the error. </summary>
             ''' <remarks>
             ''' It may be <see langword="null"/>.
             ''' Multiple lines has to be separated by <b>Environment.NewLine</b>.
             ''' </remarks>
            Public ReadOnly Hints           As String
            
            ''' <summary> Full path of the file this error is related to. </summary>
             ''' <remarks> May be <see langword="null"/>. In this case the consumer has to know the file path by itself. </remarks>
            Public ReadOnly FilePath        As String
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Returns a formatted eror message (without column values and hints). </summary>
            Public Overrides Function ToString() As String
                Dim RetValue As String = String.Empty
                
                RetValue = StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.ParseError_ErrorLevelInLineNo, Me.Level.ToDisplayString(), Me.LineNo, Me.Message.Replace(Environment.NewLine, "  " & Environment.NewLine))
                
                '' Hints.
                'If (Me.Hints.IsNotEmptyOrWhiteSpace()) Then
                '    RetValue &= Environment.NewLine & "  => " & Me.Hints.Replace(Environment.NewLine, "  " & Environment.NewLine)
                'End If
                
                Return RetValue
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4: