
Imports System
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.Runtime.InteropServices

Namespace IO
    
    ''' <summary> Represents a line of a data text file, pre-splitted into data and comment. </summary>
    Public Class PreSplittedTextLine
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.PreSplittedTextLine")
            
            Private _Words As Collection(Of DataFieldSource) = Nothing
            
        #End Region
        
        #Region "Constructors"
            
            Private Sub New()
            End Sub
            
            ''' <summary> Creates a new instance and splits the given <paramref name="TextLine"/> immediately. </summary>
             ''' <param name="TextLine">              The original text line. </param>
             ''' <param name="LineStartCommentToken"> A string preluding a comment line. May be <see langword="null"/>. </param>
             ''' <param name="LineEndCommentToken">   A string preluding a comment at line end. May be <see langword="null"/>. Won't be recognized if <paramref name="TextLine"/> starts with <paramref name="LineStartCommentToken"/>. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="TextLine"/> is <see langword="null"/>. </exception>
            Public Sub New(TextLine As String, LineStartCommentToken As String, LineEndCommentToken As String)
                
                If (TextLine Is Nothing) Then Throw New System.ArgumentNullException("TextLine")
                
                Me.LineStartCommentToken = LineStartCommentToken
                Me.LineEndCommentToken   = LineEndCommentToken
                
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
            Public ReadOnly Comment                 As String
            
            ''' <summary> The data part of this line. Defaults to <c>String.Empty</c>. </summary>
             ''' <remarks> This won't be trimmed, but it's <c>String.Empty</c> if the data part of this line only consists of whitespaces. </remarks>
            Public ReadOnly Data                    As String
            
            ''' <summary> If <see langword="true"/>, the <see cref="P:PreSplittedTextLine.Data"/> property isn't <c>String.Empty</c>. </summary>
            Public ReadOnly HasData                 As Boolean
            
            ''' <summary> If <see langword="true"/>, the <see cref="P:PreSplittedTextLine.Comment"/> property isn't <c>String.Empty</c>. </summary>
            Public ReadOnly HasComment              As Boolean
            
            ''' <summary> If <see langword="true"/>, this line starts whith an comment token. </summary>
            Public ReadOnly IsCommentLine           As Boolean
            
            ''' <summary> If <see langword="true"/>, this line consists of not more than whitespaces. </summary>
            Public ReadOnly IsEmpty                 As Boolean
            
            ''' <summary> A string preluding a comment line. May be <see langword="null"/>. </summary>
            Public ReadOnly LineStartCommentToken   As String
            
            ''' <summary> A string preluding a comment at line end. May be <see langword="null"/>. </summary>
            Public ReadOnly LineEndCommentToken     As String
            
            ''' <summary> The line number in the source file. </summary>
            Public SourceLineNo                     As Integer = 1
            
            ''' <summary> An index into a list of source files (which may be mmaintained in a parent object). </summary>
            Public SourceFileIndex                  As Integer
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Returns all data fields delimited by whitespace. </summary>
             ''' <returns> All data fields delimited by whitespace. </returns>
             ''' <remarks> The collection will be created at firrst access to this property. </remarks>
            Public ReadOnly Property Words() As Collection(Of DataFieldSource)
                Get
                    If (_Words Is Nothing) Then _Words = getWords()
                    Return _Words
                End Get
            End Property
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Re-creates and returns the full (almost original) line. </summary>
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
            
            ''' <summary> Parses <c>Me.Data</c> for a given data field determined by <paramref name="FieldDef"/>. </summary>
             ''' <typeparam name="TFieldValue"> The type to parse the field into. </typeparam>
             ''' <param name="FieldDef">        The parsing instructions. </param>
             ''' <returns>                      The resulting DataField object. </returns>
             ''' <remarks>
             ''' <para>
             ''' If a parsing error occurs, an exception is thrown that containes a <see cref="ParseError"/> (without assigned <c>FilePath</c> field).
             ''' </para>
             ''' <para>
             ''' If the field doesn't exist but <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>,
             ''' then the returned <see cref="DataField(Of TFieldValue)"/><c>.Value</c> will be a default value and the function returns <see langword="true"/> (success).
             ''' </para>
             ''' <para>
             ''' If the field's start column does exist in <c>Me.Data</c>, it doesn't matter if the field's end would be behind <c>Me.Data</c>'s end: 
             ''' The field will be taken shorter than intended without complaing.
             ''' </para>
             ''' <list type="bullet">
             ''' <item><listheader> The following conditions are recognized as parse errors. </listheader></item>
             ''' <item><description> The field doesn't exist (unless <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>). </description></item>
             ''' <item><description> The field isn't numeric while a numeric target type is required. </description></item>
             ''' </list>
             ''' <para>
             ''' If parsing fails or the field is missing these default field values are returned:
             ''' </para>
             ''' <list type="table">
             ''' <listheader> <term> <b>Target Type</b> </term>  <description> Default Value </description></listheader>
             ''' <item> <term> String </term>  <description> <c>String.Empty</c> </description></item>
             ''' <item> <term> Double </term>  <description> <c>Double.NaN</c> </description></item>
             ''' </list>
             ''' </remarks>
             ''' <exception cref="System.ArgumentException"> <paramref name="TValue"/> is not <c>String</c> or <c>Double</c>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="FieldDef"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.InvalidOperationException"> This PreSplittedTextLine doesn't contain data (<see cref="P:HasData"/> is <see langword="false"/>). </exception>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> The data field couldn't be parsed successfully. </exception>
            Public Function ParseField(Of TFieldValue As IConvertible)(FieldDef As DataFieldDefinition(Of TFieldValue)) As DataField(Of TFieldValue)
                Dim Field As DataField(Of TFieldValue) = Nothing
                
                If (Not Me.TryParseField(Of TFieldValue)(FieldDef, Field)) Then
                    If ((Field IsNot Nothing) AndAlso (Field.ParseError IsNot Nothing)) Then
                        Throw New Rstyx.Utilities.IO.ParseException(Field.ParseError)
                    Else
                        Throw New Rstyx.Utilities.IO.ParseException("!!!  parsing data field crashed...")
                    End If
                End If
                
                Return Field
            End Function
            
            ''' <summary> Tries to parse <c>Me.Data</c> for a given data field determined by <paramref name="FieldDef"/>. </summary>
             ''' <typeparam name="TFieldValue"> The type to parse the field into. </typeparam>
             ''' <param name="FieldDef">        The parsing instructions. </param>
             ''' <param name="Result">          The resulting DataField object. </param>
             ''' <returns>                      <see langword="true"/> on success, otherwise <see langword="false"/>. </returns>
             ''' <remarks>
             ''' <para>
             ''' No exception is thrown if a parsing error occurs. Instead a new <see cref="ParseError"/> is created (without assigned <c>FilePath</c> field)
             ''' and passed back as part of <paramref name="Result"/>.
             ''' </para>
             ''' <para>
             ''' If the field doesn't exist but <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>,
             ''' then <paramref name="Result"/><c>.Value</c> will be a default value and the function returns <see langword="true"/> (success).
             ''' </para>
             ''' <para>
             ''' If the field's start column does exist in <c>Me.Data</c>, it doesn't matter if the field's end would be behind <c>Me.Data</c>'s end: 
             ''' The field will be taken shorter than intended without complaing.
             ''' </para>
             ''' <list type="bullet">
             ''' <item><listheader> The following conditions are recognized as parse errors. </listheader></item>
             ''' <item><description> The field doesn't exist (unless <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>). </description></item>
             ''' <item><description> The field isn't numeric while a numeric target type is required. </description></item>
             ''' </list>
             ''' <para>
             ''' If parsing fails or the field is missing these default field values are returned:
             ''' </para>
             ''' <list type="table">
             ''' <listheader> <term> <b>Target Type</b> </term>  <description> Default Value </description></listheader>
             ''' <item> <term> String </term>  <description> <c>String.Empty</c> </description></item>
             ''' <item> <term> Double </term>  <description> <c>Double.NaN</c> </description></item>
             ''' </list>
             ''' </remarks>
             ''' <exception cref="System.ArgumentException"> <paramref name="TValue"/> is not <c>String</c> or <c>Double</c>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="FieldDef"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.InvalidOperationException"> This PreSplittedTextLine doesn't contain data (<see cref="P:HasData"/> is <see langword="false"/>). </exception>
            Public Function TryParseField(Of TFieldValue As IConvertible)(FieldDef As DataFieldDefinition(Of TFieldValue),
                                                                          <Out> ByRef Result As DataField(Of TFieldValue)
                                                                         ) As Boolean
                If (Not ((GetType(TFieldValue) Is GetType(String)) OrElse (GetType(TFieldValue) Is GetType(Double)))) Then
                    Throw New System.ArgumentException("TValue")
                End If
                If (FieldDef Is Nothing) Then Throw New System.ArgumentNullException("FieldDef")
                If (Not Me.HasData) Then Throw New System.InvalidOperationException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.PreSplittedTextLine_EmptyDataLine, Me.SourceLineNo))
                
                ' Default values.
                Const DefaultString As String = ""
                Const DefaultDouble As Double = Double.NaN
                
                Dim success         As Boolean = True
                Dim FieldHasValue   As Boolean = False
                Dim FieldSource     As DataFieldSource = Nothing
                Dim FieldValue      As TFieldValue = Nothing
                Dim ParseError      As ParseError = Nothing
                Dim TargetTypeCode  As TypeCode = Type.GetTypeCode(GetType(TFieldValue))
                
                ' Extract options.
                Dim OptionAllowKilometerNotation    As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.AllowKilometerNotation)
                Dim OptionIgnoreLeadingAsterisks    As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.IgnoreLeadingAsterisks)
                Dim OptionMissingAsZero             As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.MissingAsZero)
                Dim OptionNonNumericAsNaN           As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.NonNumericAsNaN)
                Dim OptionNotRequired               As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.NotRequired)
                Dim OptionTrim                      As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.Trim)
                Dim OptionZeroAsNaN                 As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.ZeroAsNaN)
                
                ' Assign default field value. This will be returned if parsing failes or the field is missing.
                Select Case TargetTypeCode
                    Case TypeCode.String:   FieldValue = Convert.ChangeType(DefaultString, TypeCode.String)
                    Case TypeCode.Double:   FieldValue = Convert.ChangeType(DefaultDouble, TypeCode.Double)
                    Case Else:              Throw New System.ArgumentException("TValue")
                End Select
                
                ' Get the field string and create a DataFieldSource.
                If (FieldDef.PositionType = DataFieldPositionType.WordNumber) Then
                    If (FieldDef.ColumnOrWord > Me.Words.Count) Then
                        If (Not OptionNotRequired) Then
                            success = False
                            ParseError = New ParseError(ParseErrorLevel.[Error], Me.SourceLineNo, 0, 0, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.PreSplittedTextLine_MissingWord, FieldDef.Caption, FieldDef.ColumnOrWord), Nothing)
                        End If
                    Else
                        FieldSource = Me.Words(FieldDef.ColumnOrWord - 1)
                        FieldHasValue = True
                    End If
                    
                ElseIf (FieldDef.PositionType = DataFieldPositionType.ColumnAndLength) Then
                    If (Not (FieldDef.ColumnOrWord < Me.Data.Length)) Then
                        If (Not OptionNotRequired) Then
                            success = False
                            ParseError = New ParseError(ParseErrorLevel.[Error], Me.SourceLineNo, 0, 0, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.PreSplittedTextLine_MissingField, FieldDef.Caption, FieldDef.ColumnOrWord, FieldDef.Length), Nothing)
                        End If
                    Else
                        ' Ensure field length doesn't exceeds Me.Data.
                        Dim Length As Integer = If( (FieldDef.ColumnOrWord + FieldDef.Length) <= Me.Data.Length, FieldDef.Length, Me.Data.Length - FieldDef.ColumnOrWord)
                        
                        ' Read field string.
                        FieldSource = New DataFieldSource(FieldDef.ColumnOrWord, Length, Me.Data.Substring(FieldDef.ColumnOrWord, Length))
                        
                        ' Check for existent value.
                        If (FieldSource.Value.IsEmptyOrWhiteSpace()) Then
                            If (Not OptionNotRequired) Then
                                success = False
                                ParseError = New ParseError(ParseErrorLevel.[Error], Me.SourceLineNo, FieldDef.ColumnOrWord, FieldDef.ColumnOrWord + Length, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.PreSplittedTextLine_MissingField, FieldDef.Caption, FieldDef.ColumnOrWord + 1, FieldDef.Length), Nothing)
                            End If
                        Else
                            FieldHasValue = True
                        End If
                    End If
                End If
                
                ' Default value for missing field.
                If (Not FieldHasValue) Then
                    If (OptionMissingAsZero) Then
                        FieldSource   = New DataFieldSource(0, 1, "0")
                        ParseError    = Nothing
                        FieldHasValue = True
                        success       = True
                    End If
                End If
                
                ' Parse the DataFieldSource.
                If (FieldHasValue) Then
                    
                    Dim FieldString As String = FieldSource.Value
                    
                    Select Case TargetTypeCode
                        
                        Case TypeCode.String
                            
                            If (OptionTrim) Then FieldString = FieldString.Trim()
                            FieldValue = Convert.ChangeType(FieldString, TypeCode.String)
                            
                        Case TypeCode.Double
                            
                            Dim MessageFmt    As String
                            Dim FieldDouble   As Double
                            Dim AllowedStyles As NumberStyles = NumberStyles.Float
                            
                            ' Remove asterisks.
                            If (OptionIgnoreLeadingAsterisks) Then
                                FieldString = FieldString.Trim().ReplaceWith("^\*+", String.Empty)
                            End If
                            
                            ' Parse number allowing or not the kilometer notation.
                            If (OptionAllowKilometerNotation) Then
                                success    = GeoMath.TryParseKilometer(FieldString, FieldDouble)
                                MessageFmt = Rstyx.Utilities.Resources.Messages.PreSplittedTextLine_InvalidFieldNotKilometer
                            Else
                                success    = Double.TryParse(FieldString, AllowedStyles, System.Globalization.NumberFormatInfo.InvariantInfo, FieldDouble)
                                MessageFmt = Rstyx.Utilities.Resources.Messages.PreSplittedTextLine_InvalidFieldNotNumeric
                            End If
                            
                            If ((Not success) AndAlso (Not OptionNonNumericAsNaN)) Then
                                ParseError = New ParseError(ParseErrorLevel.Error, 
                                                            Me.SourceLineNo,
                                                            FieldSource.Column,
                                                            FieldSource.Column + FieldSource.Length,
                                                            StringUtils.sprintf(MessageFmt, FieldDef.Caption, FieldSource.Value),
                                                            Nothing
                                                           )
                            Else
                                If ((Not success) AndAlso OptionNonNumericAsNaN) Then
                                    FieldDouble = Double.NaN
                                    ParseError  = Nothing
                                    success     = True
                                End If
                                If (OptionZeroAsNaN AndAlso (FieldDouble = 0.0)) Then FieldDouble = Double.NaN
                                
                                FieldValue = Convert.ChangeType(FieldDouble, TypeCode.Double)
                            End If
                            
                        Case Else
                            Throw New System.ArgumentException("TValue")
                    End Select
                End If
                
                ' Create the DataField.
                Result = New DataField(Of TFieldValue)(FieldValue, FieldSource, ParseError, FieldDef)
                
                Return success
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
            
            ''' <summary> Splits <c>Me.Data</c> into words separated by whitespace. </summary>
             ''' <returns> The words as <see cref="DataField"/>s. </returns>
             ''' <remarks></remarks>
            Private Function getWords() As Collection(Of DataFieldSource)
                Dim RetValue As New Collection(Of DataFieldSource)
                
                If (Me.HasData) Then
                    Dim Matches As System.Text.RegularExpressions.MatchCollection = Me.Data.GetMatches("\S+")
                    
                    For Each Match As System.Text.RegularExpressions.Match In Matches
                        RetValue.Add(New DataFieldSource(Match.Index, Match.Length, Match.Value))
                    Next
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
