
Imports System.Collections.Generic
Imports System.Math
Imports System.Text.RegularExpressions

Namespace Collections
    
    ''' <summary>
    ''' A <see cref="System.Collections.Generic.IComparer(Of String)"/> that does numeric comparing 
    ''' for integer numbers and Kilometer notations in the strings.
    ''' </summary>
     ''' <remarks> 
     ''' <para>
     ''' In contrast to standard string comparing this class says that "String 2" &lt; "String 10".
     ''' </para>
     ''' <para>
     ''' Also, one usual <b>Kilometer notation</b> is supported and treated as numeric value.
     ''' Leading and trailing white space belongs to the Kilometer notation, hence doesn't influence sorting of surrounding strings.
     ''' This is a wanted feature for comparing Kilometer notations, but can be irritating when comparing an integer against a Kilometer notation.
     ''' </para>
     ''' <para>
     ''' Working method: The strings are split in chunks of numeric and string types. Numeric chunks at same position in both strins
     ''' are compared numerical, otherwise the chunks are compared as strings.
     ''' </para>
     ''' <para>
     ''' Algorithm base source: http://www.dotnetperls.com/alphanumeric-sorting
     ''' </para>
     ''' </remarks>
    Public Class AlphaNumericKmComparer
        Implements IComparer(Of String)
        
        Private ReadOnly _IgnoreCase  As Boolean = True
        
        #Region "Constructors"
            
            ''' <summary> Hides the default constructor. </summary>
            Private Sub New
            End Sub
            
            ''' <summary> Creates a new instance. </summary>
             ''' <param name="IgnoreCase"> If True, strings are compared case insensitive. </param>
            Public Sub New(IgnoreCase As Boolean)
                Mybase.New()
                _IgnoreCase = IgnoreCase
            End Sub
            
        #End Region
        
        #Region "IComparer Interface"
            
            ''' <summary> Compares two strings. Numbers inside the strings are compared in a numeric way. </summary>
             ''' <param name="x"> First string </param>
             ''' <param name="y"> Second string </param>
             ''' <returns> See: <see cref="System.Collections.Generic.Comparer(Of T)"/> </returns>
            Public Function Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
                
                Dim finished  As Boolean = False
                Dim RetValue  As Integer = 0
                
                If (Not ((x Is Nothing) OrElse (y Is Nothing))) Then
                    
                    Dim s1       As String  = x
                    Dim s2       As String  = y
                    
                    If (_IgnoreCase) Then
                        s1 = s1.ToLower()
                        s2 = s2.ToLower()
                    End If
                    
                    Dim len1     As Integer = s1.Length
                    Dim len2     As Integer = s2.Length
                    Dim marker1  As Integer = 0
                    Dim marker2  As Integer = 0
                    Dim KmInfo1  As StringKmInfo = GetKmInfo(s1)
                    Dim KmInfo2  As StringKmInfo = GetKmInfo(s2)
                    
                    ' Walk through the strings with two markers.
                    Do While (marker1 < len1 AndAlso marker2 < len2)
                        
                        ' First character of current chunks.
                        Dim ch1 As Char = s1(marker1)
                        Dim ch2 As Char = s2(marker2)
                        
                        ' Type of current chunks  ( 0 = String,  1 = Numeric,  2 = Kilometer notation)
                        Dim ChunkType1         As Integer = If(Char.IsDigit(ch1), 1, 0)
                        Dim ChunkType2         As Integer = If(Char.IsDigit(ch2), 1, 0)
                        Dim ChunkType1Changes  As Boolean 
                        Dim ChunkType2Changes  As Boolean 
                        
                        ' Some buffers we can build up characters in for each chunk.
                        Dim loc1               As Integer = 0
                        Dim loc2               As Integer = 0
                        Dim space1(len1 - 1)   As Char
                        Dim space2(len2 - 1)   As Char
                        
                        ' Collect chunk in string 1 to char array.
                        If (KmInfo1.HasKm AndAlso (KmInfo1.StartIndex = marker1)) Then
                            ' Use found Kilometer Notation
                            ChunkType1 = 2
                            For i As Integer = marker1 To marker1 + KmInfo1.Length - 1
                                space1(loc1) = s1(i)
                                loc1 += 1
                                marker1 += 1
                            Next
                        Else
                            ' Walk through all following characters that are digits or
                            ' characters starting at the appropriate marker.
                            Do
                                space1(loc1) = ch1
                                loc1 += 1
                                marker1 += 1
                                
                                If (marker1 < len1) Then
                                    ch1 = s1(marker1)
                                    
                                    ChunkType1Changes = ( (KmInfo1.HasKm AndAlso (KmInfo1.StartIndex = marker1)) OrElse _
                                                        ((ChunkType1 = 1) Xor Char.IsDigit(ch1)) )
                                Else
                                    Exit Do
                                End If
                            Loop While (Not ChunkType1Changes)
                        End If
                        
                        ' Collect chunk in string 2 to char array.
                        If (KmInfo2.HasKm AndAlso (KmInfo2.StartIndex = marker2)) Then
                            ' Use found Kilometer Notation
                            ChunkType2 = 2
                            For i As Integer = marker2 To marker2 + KmInfo2.Length - 1
                                space2(loc2) = s2(i)
                                loc2 += 1
                                marker2 += 1
                            Next
                        Else
                            ' Walk through all following characters that are digits or
                            ' characters starting at the appropriate marker.
                            Do
                                space2(loc2) = ch2
                                loc2 += 1
                                marker2 += 1
                                
                                If (marker2 < len2) Then
                                    ch2 = s2(marker2)
                                    
                                    ChunkType2Changes = ( (KmInfo2.HasKm AndAlso (KmInfo2.StartIndex = marker2)) OrElse _
                                                        ((ChunkType2 = 1) Xor Char.IsDigit(ch2)) )
                                Else
                                    Exit Do
                                End If
                            Loop While (Not ChunkType2Changes)
                        End If
                        
                        ' If we have two numbers resp. Kilometers, compare them numerically, otherwise alphabetically.
                        Dim str1    As New String(space1)
                        Dim str2    As New String(space2)
                        Dim result  As Integer
                        
                        If ((ChunkType1 > 0) AndAlso (ChunkType2 > 0)) Then
                            Dim NumericChunk1 As Double = If(ChunkType1 = 2, KmInfo1.KmValue, Double.Parse(str1))
                            Dim NumericChunk2 As Double = If(ChunkType2 = 2, KmInfo2.KmValue, Double.Parse(str2))
                            result = NumericChunk1.CompareTo(NumericChunk2)
                        Else
                            result = str1.CompareTo(str2)
                        End If
                        
                        If (Not (result = 0)) Then
                            ' Chunks are not eaqual.
                            RetValue = result
                            finished = True
                            Exit Do
                        End If
                    Loop
                    If (Not finished) Then RetValue = len1 - len2
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Private Methods"
            
            ''' <summary> Searches for a usual Kilometer notation in a string. </summary>
             ''' <param name="InputString"> The String to parse. </param>
             ''' <returns> Search result as <see cref="StringKmInfo"/>. </returns>
             ''' <remarks>
             ''' <para>
             ''' Searching is done via regular expression, which includes leading and trailing spaces.
             ''' </para>
             ''' </remarks>
            Private Function GetKmInfo(ByVal InputString As String) As StringKmInfo
                
                Dim RetValue As StringKmInfo = New StringKmInfo()
                
                If (InputString.IsNotEmptyOrWhiteSpace()) Then
                    
                    'Dim Pattern As String = " *([+\-]? *[0-9]*[.]*[0-9]+)([-+ ]+)([0-9]*[.]*[0-9]+) *"
                    Dim Pattern As String = " *([+\-]? *[0-9]+\.[0-9]+)([-+ ]+)([0-9]*[.]*[0-9]+) *"
                    Dim oMatch  As Match  = Regex.Match(InputString, Pattern, RegexOptions.IgnoreCase)
                    
                    If (oMatch.Success) Then
                        ' Valid Kilometer notation.
                        Dim Kilometer  As Double  = oMatch.Groups(1).Value.Replace(" ", "").ConvertTo(Of Double)  ' signed
                        Dim MiddleSign As String  = oMatch.Groups(2).Value
                        Dim Meter      As Double  = oMatch.Groups(3).Value.ConvertTo(Of Double)   ' unsigned
                        Dim SignKm     As Integer = Sign(Kilometer)
                        Dim SignM      As Integer = If(InStr(MiddleSign, "-") > 0, -1, 1)
                        Dim SignTotal  As Integer = If(((SignM = -1) Or (SignKm = -1)), -1, 1)
                        
                        ' TODO: Km innerhalb einer Überlänge vor einem Km-Sprung werden nach hinten sortiert!
                        '_TDBValue = (SignTotal * (Abs(Kilometer) * 100000 + Meter)) + 100000000
                        'If (Meter >= 100) Then
                        '    _Status = KilometerStatus.SkipIncoming
                        'Else
                        '    _Status = KilometerStatus.Normal
                        'End If
                        
                        RetValue.HasKm      = True
                        RetValue.KmValue    = SignTotal * ((Abs(Kilometer) * 1000) + Meter)
                        RetValue.StartIndex = oMatch.Index
                        RetValue.Length     = oMatch.Length
                    End If
                End If
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Nested Class"
    
            ''' <summary> Result of Kilometer parsing of a string. </summary>
            Public Class StringKmInfo
                
                ''' <summary> Determines whether or not a Kilometer notation has been found in the string. </summary>
                Public Property HasKm           As Boolean = False
                
                ''' <summary> Index of Start of Kilometer notation in the string (incl. leading spaces). </summary>
                Public Property StartIndex      As Integer = 0
                
                ''' <summary> Length of Kilometer notation in the string (incl. leading and trailing spaces). </summary>
                Public Property Length          As Integer = 0
                
                ''' <summary> The numeric value of Kilometer notation. </summary>
                Public Property KmValue         As Double = Double.NaN
                
            End Class
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
