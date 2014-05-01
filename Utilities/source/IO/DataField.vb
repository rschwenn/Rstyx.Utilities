
Imports System

Namespace IO
    
    ''' <summary> Kind of determining a data field in a string. </summary>
    Public Enum DataFieldPositionType As Integer
        
        ''' <summary> The field won't be parsed at all. </summary>
        Ignore = 0

        ''' <summary> The field is determined by a serial number (starting at 1), related to a list of words delimited by white space. </summary>
        WordNumber = 1
        
        ''' <summary> The field is determined by a start column and a field length. </summary>
        ColumnAndLength = 2
        
    End Enum
    
    ''' <summary> Options for a data field definition. </summary>
     ''' <remarks> These are flags that can be combined in order to control the parsing process. </remarks>
     <FlagsAttribute>
    Public Enum DataFieldOptions As Integer
        
        ''' <summary> No options. </summary>
        None = 0
        
        ''' <summary> Determines that, for a target type of <c>Double</c>, the source string may be a kilometer notation, i.e. <c>12.3 + 45.68</c>. </summary>
        AllowKilometerNotation = 1
        
        ''' <summary> Determines that leading asterisks ("*") will be ignored while numeric parsing. </summary>
        IgnoreLeadingAsterisks = 2
        
        ''' <summary> Determines that, for a target type of <c>Double</c>, a missing value will be treated as <c>Zero</c>. </summary>
        MissingAsZero = 4
        
        ''' <summary> Determines that, for a target type of <c>Double</c>, a non-numeric value will be treated as <c>Double.NAN</c>. </summary>
        NonNumericAsNaN = 8
        
        ''' <summary> Determines that the field isn't required to be found in the source string. This way a missing field won't lead parsing to fail. </summary>
        NotRequired = 16
        
        ''' <summary> Determines that a string field will be trimmed. </summary>
        Trim = 32
        
        ''' <summary> Determines that, for a target type of <c>Double</c>, a value of zero will be treated as <c>Double.NAN</c>. </summary>
        ZeroAsNaN = 64
        
    End Enum
    
    ''' <summary> Represents a data field's source, which is a part of a string. </summary>
    Public Class DataFieldSource
        
        #Region "Constructors"
            
            Protected Sub New()
            End Sub
             
            ''' <summary> Creates a new instance of DataWord. </summary>
             ''' <param name="Column"> The zero-based colulmn number in the source string determining the start of this data field. </param>
             ''' <param name="Length"> The length of this data field. </param>
             ''' <param name="Value">  The string representing the field's value. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceValue"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="Column"/> is less than Zero. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="Length"/> is less than 1. </exception>
            Public Sub New(Column As Integer,
                           Length As Integer,
                           Value  As String
                          )
                If (Value Is Nothing) Then Throw New System.ArgumentNullException("SourceValue")
                If (Column < 0) Then Throw New System.ArgumentException("Column")
                If (Length < 1) Then Throw New System.ArgumentException("Length")
                
                Me.Column      = Column
                Me.Length      = Length
                Me.Value = Value
            End Sub
            
        #End Region
        
        #Region "Public Fields"
            
            ''' <summary> The zero-based colulmn number in the source string determining the start of this data field. </summary>
             ''' <remarks> If <b>PositionType</b> is <see cref="DataFieldPositionType.WordNumber"/>, this has been determined while splitting the base string. </remarks>
            Public ReadOnly Column  As Integer = -1
            
            ''' <summary> The length of this data field. </summary>
             ''' <remarks> If <b>PositionType</b> is <see cref="DataFieldPositionType.WordNumber"/>, this has been determined while splitting the base string. </remarks>
            Public ReadOnly Length  As Integer = -1
            
            ''' <summary> The source string representing the field's value. </summary>
            Public ReadOnly Value   As String = String.Empty
            
        #End Region
        
    End Class
    
    ''' <summary> Represents a parsed data field: It's source in a string, the parsed value and maybe the error occurred while parsing. </summary>
     ''' <typeparam name="TFieldValue"> The type of the field value. </typeparam>
     ''' <remarks></remarks>
    Public Class DataField(Of TFieldValue As IConvertible)
        
        #Region "Constructors"
             
            ''' <summary> Creates a new instance of DataField(Of TFieldValue). </summary>
             ''' <param name="Value">       The field's value. </param>
             ''' <param name="FieldSource"> The field's string source description. May be <see langword="null"/>. </param>
             ''' <param name="ParseError">  The parsing error, if any has been occurred while parsing. May be <see langword="null"/>. </param>
             ''' <param name="Definition">  The parsing instructions, that has been used to create this <see cref="DataField"/>. </param>
             ''' <remarks>
             ''' If both <paramref name="FieldSource"/> and <paramref name="ParseError"/> are <see langword="null"/>, 
             ''' then <paramref name="Value"/> is a default value. This means that the field hasn't been found in the source string and it wasn't required to be found.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Definition"/> is <see langword="null"/>. </exception>
            Public Sub New(Value       As TFieldValue,
                           FieldSource As DataFieldSource,
                           ParseError  As ParseError,
                           Definition  As DataFieldDefinition(Of TFieldValue)
                           )
                If (Definition Is Nothing) Then Throw New System.ArgumentNullException("Definition")
                
                Me.HasSource  = (FieldSource IsNot Nothing)
                Me.HasValue   = (ParseError Is Nothing)
                
                Me.Definition = Definition
                Me.ParseError = ParseError
                Me.Source     = FieldSource
                Me.Value      = Value
            End Sub
            
        #End Region
        
        #Region "Public Fields"
            
            ''' <summary> The parsing instructions, that has been used to create this <see cref="DataField"/>. </summary>
            Public ReadOnly Definition  As DataFieldDefinition(Of TFieldValue) = Nothing
            
            ''' <summary> <see langword="true"/> if this <see cref="DataField"/> has been parsed successfull, otherwise <see langword="false"/>. </summary>
             ''' <remarks> If <see langword="true"/>, the <c>ParseError</c> field is <see langword="null"/>. </remarks>
            Public ReadOnly HasValue    As Boolean = False
            
            ''' <summary> <see langword="true"/> if this <see cref="DataField"/> has been found in the source string, otherwise <see langword="false"/>. </summary>
             ''' <remarks> If <see langword="true"/>, the <c>Source</c> field is not <see langword="null"/>. </remarks>
            Public ReadOnly HasSource   As Boolean = False
            
            ''' <summary> If <c>HasValue</c> is <see langword="false"/>, this contains the occurred error. </summary>
            Public ReadOnly ParseError  As ParseError = Nothing
            
            ''' <summary> The field's string source description. May be <see langword="null"/>. </summary>
             ''' <remarks> 
             ''' This is <see langword="null"/>, if the field hasn't been found in the source string.
             ''' This <see cref="DataField"/> may be valid though, if the field wasn't required to be found.
             ''' </remarks>
            Public ReadOnly Source      As DataFieldSource = Nothing
            
            ''' <summary> The field's value. </summary>
             ''' <remarks> If <c>HasValue</c> is <see langword="false"/>, this value has no meaning. </remarks>
            Public ReadOnly Value       As TFieldValue = Nothing
            
        #End Region
        
    End Class
    
    ''' <summary> Represents the definition of a <see cref="DataField"/>. </summary>
     ''' <typeparam name="TFieldValue"> The target type of the field value. </typeparam>
     ''' <remarks> This definition is intended to be a parsing instruction on how to pick a piece of data out of a string. </remarks>
    Public Class DataFieldDefinition(Of TFieldValue As IConvertible)
        
        #Region "Constructors"
            
            Private Sub New()
            End Sub
            
            ''' <summary> Creates a new instance of DataWordDefinition using default options. </summary>
             ''' <param name="Caption">      The Caption for the the data field. </param>
             ''' <param name="PositionType"> Kind of determining a data field in a string. </param>
             ''' <param name="ColumnOrWord"> The <b>start colulmn number</b> or <b>word number</b> pointing to the data field in the base string. </param>
             ''' <param name="Length">       The length of the data field, if <see cref="P:PositionType"/> is <see cref="DataFieldPositionType.ColumnAndLength"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Caption"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="PositionType"/> is not a member of <see cref="PositionType"/>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="ColumnOrWord"/> is less than zero (Word) or less than 1 (Column). </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="Length"/> is less than zero (if <see cref="P:PositionType"/> is <see cref="DataFieldPositionType.ColumnAndLength"/>). </exception>
            Public Sub New(Caption      As String,
                           PositionType As DataFieldPositionType,
                           ColumnOrWord As Integer,
                           Length       As Integer
                          )
                Me.New(Caption, PositionType, ColumnOrWord, Length, DataFieldOptions.None)
            End Sub
            
            ''' <summary> Creates a new instance of DataWordDefinition. </summary>
             ''' <param name="Caption">      The Caption for the the data field. </param>
             ''' <param name="PositionType"> Kind of determining a data field in a string. </param>
             ''' <param name="ColumnOrWord"> The <b>start colulmn number</b> or <b>word number</b> pointing to the data field in the base string. </param>
             ''' <param name="Length">       The length of the data field, if <see cref="P:PositionType"/> is <see cref="DataFieldPositionType.ColumnAndLength"/>. </param>
             ''' <param name="Options">      Option flags to control parsing. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Caption"/> is <see langword="null"/>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="PositionType"/> is not a member of <see cref="PositionType"/>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="ColumnOrWord"/> is less than zero (Word) or less than 1 (Column). </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="Length"/> is less than zero (if <see cref="P:PositionType"/> is <see cref="DataFieldPositionType.ColumnAndLength"/>). </exception>
            Public Sub New(Caption      As String,
                           PositionType As DataFieldPositionType,
                           ColumnOrWord As Integer,
                           Length       As Integer,
                           Options      As DataFieldOptions
                          )
                If (Caption Is Nothing) Then Throw New System.ArgumentNullException("Caption")
                
                If (Not [Enum].IsDefined(GetType(DataFieldPositionType), PositionType)) Then Throw New System.ArgumentException("PositionType")
                
                If (PositionType = DataFieldPositionType.WordNumber) Then
                    If (ColumnOrWord < 1) Then Throw New System.ArgumentException("ColumnOrWord")
                Else
                    If (ColumnOrWord < 0) Then Throw New System.ArgumentException("ColumnOrWord")
                    If (Length < 1) Then Throw New System.ArgumentException("Length")
                End If
                
                Me.Caption      = Caption
                Me.PositionType = PositionType
                Me.ColumnOrWord = ColumnOrWord
                Me.Length       = Length
                Me.Options      = Options
            End Sub
            
        #End Region
        
        #Region "Public Fields"
            
            ''' <summary> The Caption for the the data field. </summary>
            Public ReadOnly Caption         As String = "DummyWord"
            
            
            ''' <summary> Kind of determining a data field in a string. </summary>
            Public ReadOnly PositionType    As DataFieldPositionType = DataFieldPositionType.WordNumber
            
            ''' <summary> Depending on <see cref="P:PositionType"/> this is the <b>start colulmn number</b> or <b>word number</b> pointing to the data field in the base string. </summary>
             ''' <remarks> The Column number is zero-based, while the Word number starts at 1. </remarks>
            Public ReadOnly ColumnOrWord    As Integer = -1
            
            ''' <summary> The length of the data field, if <see cref="P:PositionType"/> is <see cref="DataFieldPositionType.ColumnAndLength"/>. </summary>
             ''' <remarks> If <see cref="P:PositionType"/> is <see cref="DataFieldPositionType.WordNumber"/>, this is not used. </remarks>
            Public ReadOnly Length          As Integer = -1
            
            
            ''' <summary> Options for influencing the parsing process. </summary>
            Public ReadOnly Options         As DataFieldOptions = DataFieldOptions.None
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
