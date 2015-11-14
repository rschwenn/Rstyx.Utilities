
Imports System
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.StringUtils

Namespace IO
    
    ''' <summary> Represents a line of a data text file, pre-splitted into data and comment. </summary>
    Public Class DataTextLine
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.DataTextLine")
            
            Private _FieldDelimiter As Char = " "c
            Private _Words          As Collection(Of DataFieldSource) = Nothing
            
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
            
            ''' <summary> If <see langword="true"/>, the <see cref="DataTextLine.Data"/> property isn't <c>String.Empty</c>. </summary>
            Public ReadOnly HasData                 As Boolean
            
            ''' <summary> If <see langword="true"/>, the <see cref="DataTextLine.Comment"/> property isn't <c>String.Empty</c>. </summary>
            Public ReadOnly HasComment              As Boolean
            
            ''' <summary> If <see langword="true"/>, this line starts whith an comment token. </summary>
            Public ReadOnly IsCommentLine           As Boolean
            
            ''' <summary> If <see langword="true"/>, this line consists of not more than whitespaces. </summary>
            Public ReadOnly IsEmpty                 As Boolean
            
            ''' <summary> A string preluding a comment line. May be <see langword="null"/>. </summary>
            Public ReadOnly LineStartCommentToken   As String
            
            ''' <summary> A string preluding a comment at line end. May be <see langword="null"/>. </summary>
            Public ReadOnly LineEndCommentToken     As String
            
            ''' <summary> The line number in the source file. Defaults to 1. </summary>
            Public SourceLineNo                     As Integer = 1
            
            ''' <summary> The path to the source file, this data line has been read from. Defaults to <see langword="null"/>. </summary>
            Public SourcePath                       As String
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> A character that delimits words in <see cref="DataTextLine.Data"/>. </summary>
            ''' <remarks>
            ''' A whitespace character means the whole whitespace between words.
            ''' Setting this property resets the <see cref="DataTextLine.Words"/> property.
            ''' </remarks>
            Public Property FieldDelimiter() As Char
                Get
                    Return _FieldDelimiter
                End Get
                Set(Value As Char)
                    If (Char.IsWhiteSpace(Value)) Then Value = " "c
                    If (Not (value = _FieldDelimiter)) then
                        _FieldDelimiter = Value
                        _Words = Nothing
                    End If
                End Set
            End Property
            
            ''' <summary> Returns all data fields of <c>Me.Data</c> delimited by <see cref="DataTextLine.FieldDelimiter"/>. </summary>
             ''' <returns> All data fields. </returns>
             ''' <remarks> The collection will be created lazy at access to this property. </remarks>
            Public ReadOnly Property Words() As Collection(Of DataFieldSource)
                Get
                    If (_Words Is Nothing) Then _Words = getWords()
                    Return _Words
                End Get
            End Property
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Re-creates the full (almost original) line. </summary>
             ''' <returns> The full (almost original) line. </returns>
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
             ''' If a parsing error occurs, an exception is thrown that containes the <see cref="ParseError"/> (without assigned <c>FilePath</c> field).
             ''' The <see cref="ParseError"/> always contains error source information (line and columns)
             ''' </para>
             ''' <para>
             ''' If the field doesn't exist but <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>,
             ''' then the <c>.Value</c> of returned data field will be a default value.
             ''' </para>
             ''' <para>
             ''' If the field's start column does exist in <c>Me.Data</c>, it doesn't matter if the field's end would be behind <c>Me.Data</c>'s end: 
             ''' The field will be taken shorter than intended without complaing.
             ''' </para>
             ''' The following conditions are recognized as parse errors.
             ''' <list type="bullet">
             ''' <item><description> The field doesn't exist (unless <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>). </description></item>
             ''' <item><description> The field isn't numeric while a numeric target type is required. </description></item>
             ''' </list>
             ''' <para>
             ''' </para>
             ''' A field is considered to be non-existent, if:
             ''' <list type="bullet">
             ''' <item><description> The field is a word and it's word number is greater than the actual word count. </description></item>
             ''' <item><description> The field is fixed-width and it's start column is behind the actual line end. </description></item>
             ''' <item><description> The field is fixed-width and it's source string is empty or contains white space only. </description></item>
             ''' </list>
             ''' <para>
             ''' If parsing fails or the field is missing these default field values will be assigned:
             ''' </para>
             ''' <list type="table">
             ''' <listheader> <term> <b>Target Type</b> </term>  <description> Default Value </description></listheader>
             ''' <item> <term> String               </term>  <description> <c>String.Empty</c> </description></item>
             ''' <item> <term> Integer              </term>  <description> <c>Zero</c> </description></item>
             ''' <item> <term> Long                 </term>  <description> <c>Zero</c> </description></item>
             ''' <item> <term> Nullable(Of Integer) </term>  <description> <see langword="null"/> </description></item>
             ''' <item> <term> Nullable(Of Long)    </term>  <description> <see langword="null"/> </description></item>
             ''' <item> <term> Double               </term>  <description> <c>Double.NaN</c> </description></item>
             ''' <item> <term> Enum                 </term>  <description> <c>Unknown</c> or <c>None</c> or <c>Default value assigning <see langword="null"/></c> </description></item>
             ''' <item> <term> Kilometer            </term>  <description> New, empty <see cref="Kilometer"/> instance </description></item>
             ''' </list>
             ''' </remarks>
             ''' <exception cref="System.ArgumentException"> Type parameter <c>TValue</c> is not <c>String</c> or <c>Integer</c> or <c>Long</c> or <c>Nullable(Of Integer)</c> or <c>Nullable(Of Long)</c> or <c>Double</c> or <c>Enum</c> or <see cref="Kilometer"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="FieldDef"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.InvalidOperationException"> This DataTextLine doesn't contain data (<see cref="DataTextLine.HasData"/> is <see langword="false"/>). </exception>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> The data field couldn't be parsed successfully. </exception>
            Public Function ParseField(Of TFieldValue)(FieldDef As DataFieldDefinition(Of TFieldValue)) As DataField(Of TFieldValue)
                Dim Field As DataField(Of TFieldValue) = Nothing
                
                If (Not Me.TryParseField(Of TFieldValue)(FieldDef, Field)) Then
                    Throw New Rstyx.Utilities.IO.ParseException(Field.ParseError)
                    'If ((Field IsNot Nothing) AndAlso (Field.ParseError IsNot Nothing)) Then
                    '    Throw New Rstyx.Utilities.IO.ParseException(Field.ParseError)
                    'Else
                    '    Throw New Rstyx.Utilities.IO.ParseException("!!!  parsing data field crashed...")
                    'End If
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
             ''' The <see cref="ParseError"/> always contains error source information (line and columns)
             ''' </para>
             ''' <para>
             ''' If the field doesn't exist but <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>,
             ''' then <paramref name="Result"/><c>.Value</c> will be a default value and the function returns <see langword="true"/> (success).
             ''' </para>
             ''' <para>
             ''' If the field's start column does exist in <c>Me.Data</c>, it doesn't matter if the field's end would be behind <c>Me.Data</c>'s end: 
             ''' The field will be taken shorter than intended without complaing.
             ''' </para>
             ''' The following conditions are recognized as parse errors.
             ''' <list type="bullet">
             ''' <item><description> The field is missing (doesn't exist or has no value) (unless <paramref name="FieldDef"/><c>.Options</c> has the flag <c>NotRequired</c>). </description></item>
             ''' <item><description> The field isn't numeric while a numeric target type is required. </description></item>
             ''' </list>
             ''' <para>
             ''' </para>
             ''' A field is considered missing, if:
             ''' <list type="bullet">
             ''' <item><description> The field is a word and it's word number is greater than the actual word count. </description></item>
             ''' <item><description> The field is fixed-width and it's start column is behind the actual line end. </description></item>
             ''' <item><description> No String type: The field's source string is empty or contains white space only. </description></item>
             ''' <item><description> String type: The field's source string is a) empty or b) contains white space only but no trimming option is enabled. </description></item>
             ''' </list>
             ''' <para>
             ''' If parsing fails or the field is missing these default field values will be assigned:
             ''' </para>
             ''' <list type="table">
             ''' <listheader> <term> <b>Target Type</b> </term>  <description> Default Value </description></listheader>
             ''' <item> <term> String               </term>  <description> <c>String.Empty</c> </description></item>
             ''' <item> <term> Integer              </term>  <description> <c>Zero</c> </description></item>
             ''' <item> <term> Long                 </term>  <description> <c>Zero</c> </description></item>
             ''' <item> <term> Nullable(Of Integer) </term>  <description> <see langword="null"/> </description></item>
             ''' <item> <term> Nullable(Of Long)    </term>  <description> <see langword="null"/> </description></item>
             ''' <item> <term> Double               </term>  <description> <c>Double.NaN</c> </description></item>
             ''' <item> <term> Enum                 </term>  <description> <c>Unknown</c> or <c>None</c> or <c>Default value assigning <see langword="null"/></c> </description></item>
             ''' <item> <term> Kilometer            </term>  <description> New, empty <see cref="Kilometer"/> instance </description></item>
             ''' </list>
             ''' </remarks>
             ''' <exception cref="System.ArgumentException"> Type parameter <c>TValue</c> is not <c>String</c> or <c>Integer</c> or <c>Long</c> or <c>Nullable(Of Integer)</c> or <c>Nullable(Of Long)</c> or <c>Double</c> or <c>Enum</c> or <see cref="Kilometer"/>. </exception>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="FieldDef"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.InvalidOperationException"> This DataTextLine doesn't contain data (<see cref="DataTextLine.HasData"/> is <see langword="false"/>). </exception>
            Public Function TryParseField(Of TFieldValue)(FieldDef As DataFieldDefinition(Of TFieldValue),
                                                          <Out> ByRef Result As DataField(Of TFieldValue)
                                                         ) As Boolean
                
                Dim TargetType           As Type = GetType(TFieldValue)
                Dim TargetTypeIsNullable As Boolean = (TargetType.IsGenericType AndAlso (TargetType.GetGenericTypeDefinition() Is GetType(Nullable(Of))))
                'Dim TargetTypeIsNullable As Boolean = (TargetType.Name = "Nullable`1")
                
                If (Not ((TargetType Is GetType(String)) OrElse
                         (TargetType Is GetType(Integer)) OrElse
                         (TargetType Is GetType(Long)) OrElse
                         (TargetType Is GetType(Nullable(Of Integer))) OrElse
                         (TargetType Is GetType(Nullable(Of Long))) OrElse
                         (TargetType Is GetType(Double)) OrElse
                         (TargetType Is GetType(Kilometer)) OrElse
                         TargetType.IsEnum)
                    ) Then
                    Throw New System.ArgumentException(sprintf(Rstyx.Utilities.Resources.Messages.Global_InvalidTypeArgument, TargetType.Name), "TFieldValue")
                End If
                If (FieldDef Is Nothing) Then Throw New System.ArgumentNullException("FieldDef")
                If (Not Me.HasData) Then Throw New System.InvalidOperationException(sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_EmptyDataLine, Me.SourceLineNo))
                
                ' Default values.
                Const DefaultString      As String               = ""
                Const DefaultInteger     As Integer              = 0
                Const DefaultLong        As Long                 = 0
                Dim   DefaultNullInteger As Nullable(Of Integer) = Nothing
                Dim   DefaultNullLong    As Nullable(Of Long)    = Nothing
                Const DefaultDouble      As Double               = Double.NaN
                Dim   DefaultKilometer   As Kilometer            = New Kilometer()
                Dim   DefaultEnum        As TFieldValue          = Nothing
                
                ' Special Enum defaults.
                If (TargetType.IsEnum) Then
                    If ([Enum].IsDefined(TargetType, "Unknown")) Then
                        DefaultEnum = [Enum].Parse(TargetType, "Unknown", ignoreCase:=True)
                    ElseIf ([Enum].IsDefined(TargetType, "None")) Then
                        DefaultEnum = [Enum].Parse(TargetType, "None", ignoreCase:=True)
                    End If
                End If
                
                Dim success         As Boolean         = True
                Dim FieldHasValue   As Boolean         = False
                Dim FieldSource     As DataFieldSource = Nothing
                Dim FieldValue      As TFieldValue     = Nothing
                Dim oParseError     As ParseError      = Nothing
                'Dim TargetTypeName  As String          = TargetType.Name
                
                ' Helper objects of every supported type.
                Dim TypeString      As Type = GetType(String)
                Dim TypeInteger     As Type = GetType(Integer)
                Dim TypeLong        As Type = GetType(Long)
                Dim TypeNullInteger As Type = GetType(Nullable(Of Integer))
                Dim TypeNullLong    As Type = GetType(Nullable(Of Long))
                Dim TypeDouble      As Type = GetType(Double)
                Dim TypeKilometer   As Type = GetType(Kilometer)
                'Dim TypeEnum       As Type = GetType TFieldValue
                
                ' Extract options.
                Dim OptionAllowKilometerNotation As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.AllowKilometerNotation)
                Dim OptionIgnoreLeadingAsterisks As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.IgnoreLeadingAsterisks)
                Dim OptionMissingAsZero          As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.MissingAsZero)
                Dim OptionNonNumericAsNaN        As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.NonNumericAsNaN)
                Dim OptionNotRequired            As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.NotRequired)
                Dim OptionTrim                   As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.Trim)
                Dim OptionTrimEnd                As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.TrimEnd)
                Dim OptionZeroAsNaN              As Boolean = FieldDef.Options.HasFlag(DataFieldOptions.ZeroAsNaN)
                
                ' Assign default field value. This will be returned if parsing failes or the field is missing.
                Select Case TargetType
                    Case TypeString:      FieldValue = Convert.ChangeType(DefaultString,      TargetType)
                    Case TypeInteger:     FieldValue = Convert.ChangeType(DefaultInteger,     TargetType)
                    Case TypeLong:        FieldValue = Convert.ChangeType(DefaultLong,        TargetType)
                    'Case TypeNullInteger: FieldValue = Convert.ChangeType(DefaultNullInteger, TargetType)
                    'Case TypeNullLong:    FieldValue = Convert.ChangeType(DefaultNullLong,    TargetType)
                    Case TypeDouble:      FieldValue = Convert.ChangeType(DefaultDouble,      TargetType)
                    Case TypeKilometer:   FieldValue = Convert.ChangeType(DefaultKilometer,   TargetType)
                    Case Else:
                        If (TargetType.IsEnum) Then
                            FieldValue = DefaultEnum
                        End If
                End Select
                
                ' Get the field string and create a DataFieldSource.
                If (FieldDef.PositionType = DataFieldPositionType.Ignore) Then
                    FieldSource = New DataFieldSource(0, 1, "0")  ' Dummy.
                    
                ElseIf (FieldDef.PositionType = DataFieldPositionType.WordNumber) Then
                    
                    If (FieldDef.ColumnOrWord > Me.Words.Count) Then
                        If (Not OptionNotRequired) Then
                            success = False
                            oParseError = New ParseError(ParseErrorLevel.[Error], Me.SourceLineNo, 0, 0, sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_MissingWord, FieldDef.Caption, FieldDef.ColumnOrWord), Nothing)
                        End If
                    Else
                        FieldSource = Me.Words(FieldDef.ColumnOrWord - 1)
                        
                        ' Check for existent value.
                        If (OptionTrim OrElse OptionTrimEnd OrElse (Not (TargetType Is TypeString))) Then
                            FieldHasValue = FieldSource.Value.IsNotEmptyOrWhiteSpace()
                        Else
                            FieldHasValue = FieldSource.Value.IsNotEmpty()
                        End If
                        
                        If (Not FieldHasValue) Then
                            If (Not OptionNotRequired) Then
                                success = False
                                oParseError = New ParseError(ParseErrorLevel.[Error], Me.SourceLineNo, FieldSource.Column, FieldSource.Column + FieldSource.Length, sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_MissingWord, FieldDef.Caption, FieldDef.ColumnOrWord), Nothing)
                            End If
                        End If
                    End If
                    
                ElseIf (FieldDef.PositionType = DataFieldPositionType.ColumnAndLength) Then
                    
                    If (Not (FieldDef.ColumnOrWord < Me.Data.Length)) Then
                        If (Not OptionNotRequired) Then
                            success = False
                            oParseError = New ParseError(ParseErrorLevel.[Error], Me.SourceLineNo, 0, 0, sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_MissingField, FieldDef.Caption, FieldDef.ColumnOrWord, FieldDef.Length), Nothing)
                        End If
                    Else
                        ' Ensure field length doesn't exceeds Me.Data.
                        'Length = If( (FieldDef.ColumnOrWord + FieldDef.Length) <= Me.Data.Length, FieldDef.Length, Me.Data.Length - FieldDef.ColumnOrWord)
                        Dim Length As Integer
                        If (FieldDef.Length = Integer.MaxValue) Then
                            Length = Me.Data.Length - FieldDef.ColumnOrWord
                        ElseIf ((FieldDef.ColumnOrWord + FieldDef.Length) > Me.Data.Length) Then
                            Length = Me.Data.Length - FieldDef.ColumnOrWord
                        Else
                            Length = FieldDef.Length
                        End If
                        
                        ' Read field string.
                        FieldSource = New DataFieldSource(FieldDef.ColumnOrWord, Length, Me.Data.Substring(FieldDef.ColumnOrWord, Length))
                        
                        ' Check for existent value.
                        If (OptionTrim OrElse OptionTrimEnd OrElse (Not (TargetType Is TypeString))) Then
                            FieldHasValue = FieldSource.Value.IsNotEmptyOrWhiteSpace()
                        Else
                            FieldHasValue = FieldSource.Value.IsNotEmpty()
                        End If
                        
                        If (Not FieldHasValue) Then
                            If (Not OptionNotRequired) Then
                                success = False
                                oParseError = New ParseError(ParseErrorLevel.[Error], Me.SourceLineNo, FieldDef.ColumnOrWord, FieldDef.ColumnOrWord + Length, sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_MissingField, FieldDef.Caption, FieldDef.ColumnOrWord + 1, FieldDef.Length), Nothing)
                            End If
                        End If
                    End If
                End If
                
                ' Default value for missing field.
                If (Not FieldHasValue) Then
                    If (OptionMissingAsZero) Then
                        FieldSource   = New DataFieldSource(0, 1, "0")  ' Dummy.
                        oParseError    = Nothing
                        FieldHasValue = True
                        success       = True
                    End If
                End If
                
                ' Parse the DataFieldSource.
                If (FieldHasValue) Then
                    
                    Dim FieldString As String = FieldSource.Value
                    
                    Select Case TargetType
                        
                        Case TypeString
                            
                            If (OptionTrim)    Then FieldString = FieldString.Trim()
                            If (OptionTrimEnd) Then FieldString = FieldString.TrimEnd()
                            FieldValue = Convert.ChangeType(FieldString, TargetType)
                            
                        Case TypeInteger, TypeNullInteger
                            
                            Dim FieldInteger  As Integer
                            
                            ' Remove asterisks.
                            If (OptionIgnoreLeadingAsterisks) Then
                                FieldString = FieldString.Trim().ReplaceWith("^\*+", String.Empty)
                            End If
                            
                            ' Parse number
                            success = Integer.TryParse(FieldString, FieldInteger)
                            
                            If ((Not success) AndAlso (Not OptionNonNumericAsNaN)) Then
                                oParseError = New ParseError(ParseErrorLevel.Error, 
                                                             Me.SourceLineNo,
                                                             FieldSource.Column,
                                                             FieldSource.Column + FieldSource.Length,
                                                             sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_InvalidFieldNotInteger, FieldDef.Caption, Integer.MinValue, Integer.MaxValue, FieldSource.Value),
                                                             Nothing
                                                            )
                            Else
                                If ((Not success) AndAlso OptionNonNumericAsNaN) Then
                                    FieldInteger = DefaultInteger
                                    oParseError  = Nothing
                                    success      = True
                                End If
                                
                                If (Not (TargetTypeIsNullable AndAlso OptionZeroAsNaN AndAlso (FieldInteger = DefaultInteger))) Then
                                        FieldValue = Convert.ChangeType(FieldInteger, TypeInteger)
                                End If
                            End If
                            
                        Case TypeLong, TypeNullLong
                            
                            Dim FieldLong  As Long
                            
                            ' Remove asterisks.
                            If (OptionIgnoreLeadingAsterisks) Then
                                FieldString = FieldString.Trim().ReplaceWith("^\*+", String.Empty)
                            End If
                            
                            ' Parse number
                            success = Long.TryParse(FieldString, FieldLong)
                            
                            If ((Not success) AndAlso (Not OptionNonNumericAsNaN)) Then
                                oParseError = New ParseError(ParseErrorLevel.Error, 
                                                             Me.SourceLineNo,
                                                             FieldSource.Column,
                                                             FieldSource.Column + FieldSource.Length,
                                                             sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_InvalidFieldNotLong, FieldDef.Caption, Long.MinValue, Long.MaxValue, FieldSource.Value),
                                                             Nothing
                                                            )
                            Else
                                If ((Not success) AndAlso OptionNonNumericAsNaN) Then
                                    FieldLong = DefaultLong
                                    oParseError = Nothing
                                    success     = True
                                End If
                                
                                If (Not (TargetTypeIsNullable AndAlso OptionZeroAsNaN AndAlso (FieldLong = DefaultLong))) Then
                                    FieldValue = Convert.ChangeType(FieldLong, TypeLong)
                                End If
                            End If
                            
                        Case TypeDouble
                            
                            Dim MessageFmt    As String
                            Dim FieldDouble   As Double
                            Dim AllowedStyles As NumberStyles = NumberStyles.Float
                            
                            ' Remove asterisks.
                            If (OptionIgnoreLeadingAsterisks) Then
                                FieldString = FieldString.Trim().ReplaceWith("^\*+", String.Empty)
                            End If
                            
                            ' Parse number allowing or not the kilometer notation.
                            If (OptionAllowKilometerNotation) Then
                                success = DefaultKilometer.TryParse(FieldString)
                                If (success) Then FieldDouble = DefaultKilometer.Value
                                MessageFmt = Rstyx.Utilities.Resources.Messages.DataTextLine_InvalidFieldNotKilometer
                            Else
                                success    = Double.TryParse(FieldString, AllowedStyles, System.Globalization.NumberFormatInfo.InvariantInfo, FieldDouble)
                                MessageFmt = Rstyx.Utilities.Resources.Messages.DataTextLine_InvalidFieldNotNumeric
                            End If
                            
                            If ((Not success) AndAlso (Not OptionNonNumericAsNaN)) Then
                                oParseError = New ParseError(ParseErrorLevel.Error, 
                                                             Me.SourceLineNo,
                                                             FieldSource.Column,
                                                             FieldSource.Column + FieldSource.Length,
                                                             sprintf(MessageFmt, FieldDef.Caption, FieldSource.Value),
                                                             Nothing
                                                            )
                            Else
                                If ((Not success) AndAlso OptionNonNumericAsNaN) Then
                                    FieldDouble = Double.NaN
                                    oParseError = Nothing
                                    success     = True
                                End If
                                If (OptionZeroAsNaN AndAlso (FieldDouble = 0.0)) Then FieldDouble = Double.NaN
                                
                                FieldValue = Convert.ChangeType(FieldDouble, TargetType)
                            End If
                            
                        Case TypeKilometer
                            
                            Dim FieldKilometer  As Kilometer = New Kilometer()
                            Dim AllowedStyles   As NumberStyles = NumberStyles.Float
                            
                            ' Remove asterisks.
                            If (OptionIgnoreLeadingAsterisks) Then
                                FieldString = FieldString.Trim().ReplaceWith("^\*+", String.Empty)
                            End If
                            
                            ' Parse kilometer.
                            success = FieldKilometer.TryParse(FieldString)
                            
                            If ((Not success) AndAlso (Not OptionNonNumericAsNaN)) Then
                                oParseError = New ParseError(ParseErrorLevel.Error,
                                                             Me.SourceLineNo,
                                                             FieldSource.Column,
                                                             FieldSource.Column + FieldSource.Length,
                                                             sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_InvalidFieldNotKilometer, FieldDef.Caption, FieldSource.Value),
                                                             Nothing
                                                            )
                            Else
                                If ((Not success) AndAlso OptionNonNumericAsNaN) Then
                                    ' Already there: FieldKilometer.Value = Double.NaN
                                    oParseError = Nothing
                                    success     = True
                                End If
                                If (OptionZeroAsNaN AndAlso (FieldKilometer.Value = 0.0)) Then FieldKilometer = DefaultKilometer
                                
                                FieldValue = Convert.ChangeType(FieldKilometer, TargetType)
                            End If
                            
                        Case Else
                            If (TargetType.IsEnum) Then
                                Try
                                    FieldValue = [Enum].Parse(TargetType, FieldString, ignoreCase:=True)
                                    
                                    ' Treat not defined value as parsing error, too.
                                    If (Not [Enum].IsDefined(TargetType, FieldValue)) Then
                                        Throw New Exception()
                                    End If
                                Catch ex As Exception
                                    success = False
                                    Dim ValidValues As String = [Enum].GetNames(TargetType).Join(", ") '& ", " & [Enum].GetValues(TargetType).Cast(Of String).Join(", ")
                                    For Each Value As Integer In [Enum].GetValues(TargetType)
                                        ValidValues &= ", " & CStr(Value)
                                    Next
                                    oParseError = New ParseError(ParseErrorLevel.Error,
                                                                 Me.SourceLineNo,
                                                                 FieldSource.Column,
                                                                 FieldSource.Column + FieldSource.Length,
                                                                 sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_InvalidFieldNotEnumMember, FieldDef.Caption, FieldSource.Value),
                                                                 sprintf(Rstyx.Utilities.Resources.Messages.DataTextLine_ValidValues, ValidValues),
                                                                 Nothing
                                                                )
                                End Try
                            Else
                                Throw New System.ArgumentException("TFieldValue")
                            End If
                    End Select
                End If
                
                ' Create the DataField.
                Result = New DataField(Of TFieldValue)(FieldValue, FieldSource, oParseError, FieldDef)
                
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
                            'If (Comm.StartsWith(" ", System.StringComparison.Ordinal)) Then
                            '    Comment = Comm.Substring(1)
                            'Else
                                Comment = Comm
                            'End If
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
            
            ''' <summary> Splits <c>Me.Data</c> into words separated by <see cref="DataTextLine.FieldDelimiter"/>. </summary>
             ''' <returns> The words as <see cref="DataFieldSource"/>'s. </returns>
             ''' <remarks></remarks>
            Private Function getWords() As Collection(Of DataFieldSource)
                
                Dim RetValue As New Collection(Of DataFieldSource)
                
                If (Me.HasData) Then
                    Dim WordRegEx As String
                    
                    If (Char.IsWhiteSpace(Me.FieldDelimiter)) Then
                        ' Awk-like splitting.
                        WordRegEx = "\S+"
                        Dim Matches As MatchCollection = Me.Data.GetMatches(WordRegEx)
                        
                        For Each Match As Match In Matches
                            RetValue.Add(New DataFieldSource(Match.Index, Match.Length, Match.Value))
                        Next
                    Else
                        ' ********************************************************************************************************************
                        ' TODO: GetWords() can't recognize empty fields, because two subsequent delimiters are treated as part of field value.
                        ' ********************************************************************************************************************
                        WordRegEx = "[^" & Me.FieldDelimiter & "]+"  ' Doesn't find an empty field at line start.
                        
                        ' Ensure first and last fields will be considerd even if empty (that is Me.Data starts and/or ends with delimiter).
                        Dim DataStartsWithDelimiter As Boolean = Me.Data.StartsWith(Me.FieldDelimiter)
                        Dim DataEndsWithDelimiter   As Boolean = Me.Data.EndsWith(Me.FieldDelimiter)
                        Dim DataTuned               As String  = Me.Data
                        Dim dL_Start                As Integer = 0
                        
                        If (DataStartsWithDelimiter) Then
                            DataTuned = " " & DataTuned
                            'dL_Start  = 1
                        End If
                        If (DataEndsWithDelimiter)   Then DataTuned = DataTuned & " "
                        
                        ' Hide masked (doubled) Delimiters.
                        Dim MaskedDelimiter         As String  = Me.FieldDelimiter & Me.FieldDelimiter
                        Dim DoubledMaskedDelimiter  As String  = "D1o2u3b4l5e6D7e8l9i0mM"
                        Dim MaskedDelimitersCount   As Integer = 0
                        Dim dL_Delim                As Integer = DoubledMaskedDelimiter.Length - MaskedDelimiter.Length
                        
                        Dim Matches As MatchCollection = DataTuned.Replace(MaskedDelimiter, DoubledMaskedDelimiter).GetMatches(WordRegEx)
                        
                        ' Add words to return collection.
                        For i As Integer = 0 To Matches.Count - 1
                            
                            Dim Match    As Match   = Matches(i)
                            Dim WordText As String  = Nothing
                            Dim dIndex   As Integer = dL_Start + (dL_Delim * MaskedDelimitersCount)
                            Dim dLength  As Integer = 0
                            
                            ' Correct value and length.
                            If ((i = 0) AndAlso DataStartsWithDelimiter) Then
                                dL_Start = 1
                                WordText = String.Empty
                                dLength  = Match.Length
                            ElseIf ((i = (Matches.Count - 1)) AndAlso DataEndsWithDelimiter) Then
                                WordText = String.Empty
                                dLength  = Match.Length
                            Else
                                ' Change masked delimiters by single delimiters.
                                WordText = Match.Value
                                Dim LocalMaskedDelimitersCount As Integer = WordText.GetMatches(DoubledMaskedDelimiter).Count
                                If (LocalMaskedDelimitersCount > 0) Then
                                    dLength = dL_Delim * LocalMaskedDelimitersCount
                                    MaskedDelimitersCount += LocalMaskedDelimitersCount
                                    WordText = WordText.Replace(DoubledMaskedDelimiter, Me.FieldDelimiter).Trim()
                                End If
                            End If
                            
                            RetValue.Add(New DataFieldSource(Match.Index - dIndex, Match.Length - dLength, WordText))
                        Next
                    End If
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
