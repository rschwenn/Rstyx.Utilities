
Imports System.Collections.Generic
Imports System.Text

Imports PGK.Extensions

'Namespace Strings
    
    ''' <summary> Static utility methods for dealing with strings. </summary>
    Public NotInheritable Class StringUtils
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(MyClass.GetType.FullName)
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.StringUtils")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> String formatting like in C or awk (does not support %e, %E, %g, %G). </summary>
             ''' <param name="FormatString"> A String like "test: %s  %11.3f" </param>
             ''' <param name="Parms">        Parameter list or Array (Nested 1d-Arrays as single parameters are supported). </param>
             ''' <returns>                   The <paramref name="FormatString"/> with expanded variables, or an empty string if an error occurs. </returns>
            Public Shared Function sprintf(ByVal FormatString As String, ParamArray Parms() As Object) As String
                Return sprintf(False, FormatString, Parms)
            End Function
            
            ''' <summary> String formatting like in C or awk (does not support %e, %E, %g, %G). </summary>
             ''' <param name="FormatString"> A String like "test: %s  %11.3f" </param>
             ''' <param name="Parms">        Parameter list or Array (Nested 1d-Arrays as single parameters are supported). </param>
             ''' <param name="ThrowOnError"> If <see langword="true"/>, an occuring exception is thrown. Otherwise it's catched and logged and an empty string is returned. </param>
             ''' <returns>
             ''' The <paramref name="FormatString"/> with expanded variables, 
             ''' or an empty string if an error occurs and <paramref name="ThrowOnError"/> is <see langword="false"/>.
             ''' </returns>
             ''' <exception cref="System.FormatException"> <paramref name="FormatString"/> is not well formed. </exception>
             ''' <remarks> 
              ''' <para>
              ''' Original written in VB6 by Phlip Bradbury (phlipping@yahoo.com) 
              ''' (http://www.freevbcode.com/ShowCode.asp?ID=5014)
              ''' </para>
              ''' <para>
              ''' <list type="table">
              ''' <listheader> <description> <b>Escape sequences:</b> </description></listheader>
              ''' <item> <term> \a    </term>  <description> Alert (Bel)           </description></item>
              ''' <item> <term> \b    </term>  <description> Backspace             </description></item>
              ''' <item> <term> \f    </term>  <description> Form Feed             </description></item>
              ''' <item> <term> \n    </term>  <description> Newline (Line Feed)   </description></item>
              ''' <item> <term> \r    </term>  <description> Carriage Return       </description></item>
              ''' <item> <term> \t    </term>  <description> Horizontal Tab        </description></item>
              ''' <item> <term> \v    </term>  <description> Verical Tab           </description></item>
              ''' <item> <term> \ddd  </term>  <description> Octal character       </description></item>
              ''' <item> <term> \xdd  </term>  <description> Hexadecimal character </description></item>
              ''' <item> <term> \"    </term>  <description> not supported         </description></item>
              ''' <item> <term> \'    </term>  <description> not supported         </description></item>
              ''' </list>
              ''' </para>
              ''' <para>
              ''' <b>Parameters:</b> <br />
              ''' %[flags][width][.precision]formattype
              ''' </para>
              ''' <para>
              ''' <list type="table">
              ''' <listheader> <description> <b>Flags:</b> </description></listheader>
              ''' <item> <term> -     </term>  <description> left justify                </description></item>
              ''' <item> <term> +     </term>  <description> prefix with sign            </description></item>
              ''' <item> <term> #     </term>  <description> prefixes o,x,X with 0 or 0x </description></item>
              ''' <item> <term> blank </term>  <description> Prefixes a space character to the result if the first character of 
              '''                                            a signed conversion is not a sign. Ignored if the "+"-option appear. </description></item>
              ''' </list>
              ''' </para>
              ''' <para>
              ''' <list type="table">
              ''' <listheader> <description> <b>Format types:</b> </description></listheader>
              ''' <item> <term> %d, %i </term>  <description> signed number                          </description></item>
              ''' <item> <term> %u     </term>  <description> unsigned number                        </description></item>
              ''' <item> <term> %o     </term>  <description> unsigned octal number                  </description></item>
              ''' <item> <term> %x, %X </term>  <description> unsigned hexadecimal number            </description></item>
              ''' <item> <term> %f     </term>  <description> floating point number without exponent </description></item>
              ''' <item> <term> %c     </term>  <description> single character (from ASCII value)    </description></item>
              ''' <item> <term> %s     </term>  <description> String                                 </description></item>
              ''' <item> <term> %e, %E </term>  <description> not supported                          </description></item>
              ''' <item> <term> %g, %G </term>  <description> not supported                          </description></item>
              ''' </list>
              ''' </para>
              ''' <para>
              ''' <b>Hints:</b>
              ''' </para>
              ''' <para>
              ''' Values of <see langword="false"/> or <c>Double.NaN</c> will result in an empty string.
              ''' </para>
              ''' <para>
              ''' So e.g. %-6.3d is a number with a minimum of 3 digits left justified in a field a minimum of 6 characters wide.
              ''' </para>
              ''' <para>
              ''' Use \\ to type a backslash and either \% or %% to type a percent sign.
              ''' </para>
              ''' <para>
              ''' Note: %u treats as short (VB As Integer) when converting negative numbers. Values below -32768 will look odd.
              ''' %o, %x and %X, however, treat as long.
              ''' </para>
              ''' <para>
              ''' Finally %c is sent the ascii value of the character to print, if you want to send a single-character string 
              ''' use Asc() or %s, so:
              ''' </para>
              ''' <para>
              ''' SPrintF("%s", Char) = SPrintF("%c", Asc(Char)) and
              ''' </para>
              ''' <para>
              ''' SPrintF("%s", Chr(Num)) = SPrintF("%c", Num)
              ''' </para>
             ''' </remarks>
            Public Shared Function sprintf(ByVal ThrowOnError As Boolean, ByVal FormatString As String, ParamArray Parms() As Object) As String
                
                Dim Ret           As String = String.Empty
                Dim FormatSave    As String = FormatString
                Try
                    Dim OneChar       As Char
                    Dim NumberBuffer  As String
                    Dim ParamUpTo     As Long
                    Dim Flags         As String
                    Dim Width         As String
                    Dim Precision     As String
                    Dim Prec          As String
                    Dim Value         As Double
                    Dim AddStr        As String
                    Dim ParmX         As Object = Nothing
                    'for calculating %e and %g
                    'Dim Mantissa      As 
                    'Dim Exponent      As 
                    'for calculating %g
                    'Dim AddStrPercentF , AddStrPercentE
                    Dim FlatParms()   As Object = new Object() {}
                    Dim VParms()      As Object
                    
                    ' Unwind encapsulated arrays into one flat parameter array.
                    If (Parms Is Nothing) Then
                        VParms = New Object() {Nothing}
                    Else
                        VParms = Parms
                    End If
                    FlatParms = ArrayUtils.getFlatArray(VParms)
                    ParamUpTo = LBound(FlatParms)
                    
                    While (Not String.IsNullOrEmpty(FormatString))
                      OneChar = NextChar(FormatString)
                      Select Case OneChar
                        
                        Case "\"c
                          OneChar = NextChar(FormatString)
                          Select Case OneChar
                            Case "a"c 'alert (bell)
                                Ret = Ret & Chr(7)
                            Case "b"c 'backspace
                                Ret = Ret & vbBack
                            Case "f"c 'formfeed
                                Ret = Ret & vbFormFeed
                            Case "n"c 'newline (linefeed)
                                Ret = Ret & vbNewLine
                            Case "r"c 'carriage return
                                Ret = Ret & vbCr
                            Case "t"c 'horizontal tab
                                Ret = Ret & vbTab
                            Case "v"c 'vertical tab
                                Ret = Ret & vbVerticalTab
                            Case "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c   'octal character
                                NumberBuffer = OneChar
                                While ((InStr("01234567", FormatString.Left(1)) > 0) And (FormatString.Length > 0))
                                    NumberBuffer = NumberBuffer & NextChar(FormatString)
                                End While
                                Ret = Ret & CStr(GeoMath.Oct2Dec(NumberBuffer))
                            Case "x"c 'hexadecimal character
                                NumberBuffer = ""
                                While ((InStr("0123456789ABCDEFabcdef", FormatString.Left(1)) > 0) And (FormatString.Length > 0))
                                    NumberBuffer = NumberBuffer & NextChar(FormatString)
                                End While
                                Ret = Ret & CStr(GeoMath.Hex2Dec(NumberBuffer))
                            Case "\"c 'backslash
                                Ret = Ret & "\"
                            Case "%"c 'percent
                                Ret = Ret & "%"
                            Case Else 'unrecognised
                                Ret = Ret & OneChar
                                Throw New System.FormatException(String.Format(Rstyx.Utilities.Resources.Messages.Sprintf_InvalidEscapeSequence, OneChar))
                          End Select
                        
                        Case "%"c
                          OneChar = NextChar(FormatString)
                          If OneChar = "%" Then
                            Ret = Ret & "%"
                          Else
                            Flags = ""
                            Width = ""
                            Precision = ""
                            While (OneChar = "-" Or OneChar = "+" Or OneChar = "#" Or OneChar = " ")
                              Flags = Flags & OneChar
                              OneChar = NextChar(FormatString)
                            End While
                            'While IsNumeric(OneChar)
                            While (Char.IsNumber(OneChar))
                              Width = Width & OneChar
                              OneChar = NextChar(FormatString)
                            End While
                            If OneChar = "." Then
                              OneChar = NextChar(FormatString)
                              'While IsNumeric(OneChar)
                              While (Char.IsNumber(OneChar))
                                Precision = Precision & OneChar
                                OneChar = NextChar(FormatString)
                              End While
                            End If
                            
                            ParmX = FlatParms(CInt(ParamUpTo))
                            if ((ParmX Is Nothing) OrElse ParmX.ToString().IsEmpty()) then
                              AddStr = ""
                            else
                                Select Case OneChar
                                  
                                  Case "d"c, "i"c 'signed decimal
                                    Value = CLng(ParmX)
                                    'Value = System.Convert.ChangeType(ParmX, GetType(Long))
                                    AddStr = CStr(System.Math.Abs(Value))
                                    If (Not Precision.IsEmpty()) Then
                                      If CDbl(Precision) > AddStr.Length Then
                                        'AddStr = String(CDbl(Precision) - AddStr.Length, "0") & AddStr
                                        AddStr = "0".Repeat(CInt(CDbl(Precision) - AddStr.Length)) & AddStr
                                      End If
                                    End If
                                    If Value < 0 Then
                                      AddStr = "-" & AddStr
                                    ElseIf (InStr(Flags, "+") > 0) Then
                                      AddStr = "+" & AddStr
                                    ElseIf (InStr(Flags, " ") > 0) Then
                                      AddStr = " " & AddStr
                                    End If
                                  
                                  Case "u"c 'unsigned decimal
                                    Value = CLng(ParmX)
                                    If Value < 0 Then Value = Value + 65536
                                    AddStr = CStr(Value)
                                    If Precision <> "" Then
                                      If CDbl(Precision) > AddStr.Length Then
                                        AddStr = "0".Repeat(CInt(CDbl(Precision) - AddStr.Length)) & AddStr
                                      End If
                                    End If
                                  
                                  Case "o"c 'unsigned octal value
                                    Value = CLng(ParmX)
                                    AddStr = Oct(Value)
                                    If Precision <> "" Then
                                      If CDbl(Precision) > AddStr.Length Then
                                        AddStr = "0".Repeat(CInt(CDbl(Precision) - AddStr.Length)) & AddStr
                                      End If
                                    End If
                                    If (InStr(Flags, "#") > 0) Then AddStr = "0" & AddStr
                                  
                                  Case "x"c, "X"c 'unsigned hexadecimal value
                                    Value = CLng(ParmX)
                                    AddStr = Hex(Value)
                                    If OneChar = "x" Then AddStr = LCase(AddStr)
                                    If Precision <> "" Then
                                      If CDbl(Precision) > AddStr.Length Then
                                        AddStr = "0".Repeat(CInt(CDbl(Precision) - AddStr.Length)) & AddStr
                                      End If
                                    End If
                                    If (InStr(Flags, "#") > 0) Then AddStr = "0x" & AddStr
                                  
                                  Case "f"c 'float w/o exponent
                                    Value = CDbl(ParmX)
                                    If (Double.IsNaN(Value)) Then
                                        'AddStr = "NaN"
                                        AddStr = ""
                                    Else
                                        If (Precision = "") Then Precision = "6"
                                        'AddStr = Format(Abs(Value), "0." & String(Precision, "0"))
                                        AddStr = FormatNumber(System.Math.Abs(Value), CInt(Precision), TriState.True, TriState.False, TriState.False)
                                                'FormatNumber(Ausdruck[, AnzDezimalstellen[, FührendeNull[, KlammernFürNegativeWerte[, ZiffernGruppieren]]]])
                                                'Die letzten 3 Parameter akzeptieren folgende Werte:
                                                '-1 = true
                                                ' 0 = false
                                                '-2 = Ländereinstellungen des Computers verwenden
                                    End If
                                    If (Value < 0) Then
                                      AddStr = "-" & AddStr
                                    ElseIf (InStr(Flags, "+") > 0) Then
                                      AddStr = "+" & AddStr
                                    ElseIf (InStr(Flags, " ") > 0) Then
                                      AddStr = " " & AddStr
                                    End If
                                  
                                  'Case "e", "E" 'float w/ exponent
                                    'Value = CDbl(ParmX)
                                    'Mantissa = Abs(Value)
                                    'Exponent = 0
                                    'If Mantissa > 10 Then
                                    '  While Mantissa >= 10
                                    '    Mantissa = Mantissa / 10
                                    '    Exponent = Exponent + 1
                                    '  End While
                                    'Else
                                    '  While Mantissa < 1
                                    '    Mantissa = Mantissa * 10
                                    '    Exponent = Exponent - 1
                                    '  End While
                                    'End If
                                    'If Precision = "" Then Precision = "6"
                                    ''AddStr = Format(Mantissa, "0." & String(Precision, "0"))
                                    'AddStr = FormatNumber(Mantissa, Precision, -1)
                                    'If Right(AddStr, 1) = "." Then AddStr = Left(AddStr, AddStr.Length - 1)
                                    'AddStr = AddStr & OneChar & IIf(Exponent < 0, "-", "+") & Format(Exponent, "000")
                                    'If Value < 0 Then
                                    '  AddStr = "-" & AddStr
                                    'ElseIf InStr(Flags, "+") Then
                                    '  AddStr = "+" & AddStr
                                    'ElseIf InStr(Flags, "-") = 0 Then
                                    '  AddStr = " " & AddStr
                                    'End If
                                  'Case "g", "G" 'float w/ or w/o exponent, shorter
                                    ''first calculate without
                                    'Value = CDbl(ParmX)
                                    'If Precision = "" Then Precision = "6"
                                    'AddStrPercentF = Format(Abs(Value), "0." & String(Precision, "#"))
                                    'If Value < 0 Then
                                    '  AddStrPercentF = "-" & AddStrPercentF
                                    'ElseIf InStr(Flags, "+") Then
                                    '  AddStrPercentF = "+" & AddStrPercentF
                                    'ElseIf InStr(Flags, "-") = 0 Then
                                    '  AddStrPercentF = " " & AddStrPercentF
                                    'End If
                                    ''then calculate with
                                    'Value = CDbl(ParmX)
                                    'Mantissa = Abs(Value)
                                    'Exponent = 0
                                    'If Mantissa > 10 Then
                                    '  While Mantissa >= 10
                                    '    Mantissa = Mantissa / 10
                                    '    Exponent = Exponent + 1
                                    '  End While
                                    'Else
                                    '  While Mantissa < 1
                                    '    Mantissa = Mantissa * 10
                                    '    Exponent = Exponent - 1
                                    '  End While
                                    'End If
                                    'If Precision = "" Then Precision = "6"
                                    'AddStrPercentE = Format(Mantissa, "0." & String(Precision, "#"))
                                    'If Right(AddStrPercentE, 1) = "." Then AddStrPercentE = Left(AddStrPercentE, AddStrPercentE.Length - 1)
                                    'AddStrPercentE = AddStrPercentE & IIf(OneChar = "G", "E", "e") & IIf(Exponent < 0, "-", "+") & Format(Exponent, "000")
                                    'If Value < 0 Then
                                    '  AddStrPercentE = "-" & AddStrPercentE
                                    'ElseIf InStr(Flags, "+") Then
                                    '  AddStrPercentE = "+" & AddStrPercentE
                                    'ElseIf InStr(Flags, "-") = 0 Then
                                    '  AddStrPercentE = " " & AddStrPercentE
                                    'End If
                                    ''find shortest
                                    'AddStr = IIf(AddStrPercentF.Length > AddStrPercentE.Length, AddStrPercentE, AddStrPercentF)
                                    
                                  Case "c"c 'single character, passed ASCII value
                                    AddStr = Chr(CByte(ParmX))
                                  
                                  Case "s"c 'string
                                    AddStr = CStr(ParmX)
                                  
                                  Case Else
                                    If (Precision <> "") then prec = "." & Precision else prec = ""
                                    Throw New System.FormatException(String.Format(Rstyx.Utilities.Resources.Messages.Sprintf_InvalidParameterSequence, Flags, Width, prec, OneChar))
                                    AddStr = "%" & Flags & Width & prec & OneChar
                                End Select
                              
                            end if
                            
                            If (Width <> "") Then
                              If cint(Width) > AddStr.Length Then
                                If (InStr(Flags, "-") > 0) Then
                                  AddStr = AddStr & Space(CInt(Width) - AddStr.Length)
                                Else
                                  AddStr = Space(CInt(Width) - AddStr.Length) & AddStr
                                End If
                              End If
                            End If
                            ParamUpTo = ParamUpTo + 1
                            Ret = Ret & AddStr
                          End If
                        Case Else
                          Ret = Ret & OneChar
                      End Select
                    End While
                    
                Catch ex As System.Exception
                    If (ThrowOnError) Then
                        Throw
                    Else
                        Ret = String.Empty
                        Logger.logError(ex, String.Format(Rstyx.Utilities.Resources.Messages.Sprintf_UnexpectedError, FormatSave))
                    End If
                End Try
                
                Return Ret
            End Function
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Returns the first character from a buffer string and removes it from the buffer. </summary>
             ''' <param name="Buffer"> The working string. </param>
            Private Shared Function NextChar(ByRef Buffer As String) As Char
                Dim FirstChar As Char = CChar(Buffer.Substring(0, 1))
                Buffer = Buffer.Substring(1)
                Return FirstChar
            End Function
            
        #End Region
        
    End Class
    
    ''' <summary> Extension methods for strings or other types dealing with strings. </summary>
    Public Module StringExtensions
        
        ''' <summary> Joins strings only if not <see langword="null"/>, empty or whitespace. </summary>
         ''' <param name="SourceStrings"> The source Strings. </param>
         ''' <param name="JoinString">    The join string. </param>
         ''' <returns> The joined string. May be empty (but not <see langword="null"/>). </returns>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="SourceStrings"/> or <paramref name="TestStrings"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function JoinIgnoreEmpty(SourceStrings As IEnumerable(Of String), JoinString As String) As String
            
            If (SourceStrings Is Nothing) Then Throw New System.ArgumentNullException("SourceStrings")
            If (JoinString Is Nothing)    Then Throw New System.ArgumentNullException("JoinString")
            
            Dim Builder As New StringBuilder()
            
            For Each SourceString As String In SourceStrings
                If (SourceString.IsNotEmptyOrWhiteSpace()) Then
                    Builder.Append(SourceString)
                    Builder.Append(JoinString)
                End If
            Next
            ' Remove trailing join string.
            If (Builder.Length > 0) Then
                Builder.Remove(Builder.Length - JoinString.Length, JoinString.Length)
            End If
            Return Builder.ToString()
        End Function
        
        ''' <summary> Check whether or not the string containes any of given test strings. </summary>
         ''' <param name="Value">       Input String. </param>
         ''' <param name="TestStrings"> Array with test strings. </param>
         ''' <returns> <see langword="true"/>, if <paramref name="Value"/> containes any of the test strings. </returns>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="Value"/> or <paramref name="TestStrings"/> is <see langword="null"/>. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ContainsAny(ByVal Value As String, TestStrings() As String) As Boolean
            
            If (Value Is Nothing) Then Throw New System.ArgumentNullException("Value")
            If (TestStrings Is Nothing) Then Throw New System.ArgumentNullException("TestStrings")
            
            Dim RetValue As Boolean = False
            
            For Each TestString As String In TestStrings
                If ((TestString IsNot Nothing) AndAlso (Value.Contains(TestString))) Then
                    RetValue = True
                    Exit For
                End If
            Next
            Return RetValue
        End Function
        
        ''' <summary>
        ''' Returns the left part of the string up to the first occurrence of the Delimiter character.
        ''' If Delimiter isn't found the whole string is returned.
        ''' </summary>
         ''' <param name="Value">            Input String. </param>
         ''' <param name="Delimiter">        String to stop at. </param>
         ''' <param name="IncludeDelimiter"> If <see langword="true"/>, the returned string containes the <paramref name="Delimiter"/>, otherwise it doesn't. </param>
         ''' <param name="GetMaximumMatch">  If <see langword="true"/>, the last occurrence of <paramref name="Delimiter"/> will be used, otherwise the first. </param>
         ''' <returns> Truncated string, or <see langword="null"/> if <paramref name="Value"/> is <see langword="null"/>. </returns>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function Left(ByVal Value As String, Delimiter As String, Optional IncludeDelimiter As Boolean = False, Optional GetMaximumMatch As Boolean = False) As String
            If (Value IsNot Nothing) Then
                Dim idx As Integer = 0
                If (GetMaximumMatch) Then
                    idx = Value.LastIndexOf(Delimiter, comparisonType:=System.StringComparison.Ordinal)
                Else
                    idx = Value.IndexOf(Delimiter, comparisonType:=System.StringComparison.Ordinal)
                End If
                If (idx >= 0) Then
                    If (IncludeDelimiter) Then idx = idx + Delimiter.Length
                    Value = Value.Substring(0, idx)
                End If
            End If
            Return Value
        End Function
        
        ''' <summary>
        ''' Returns the right part of the string up to the first occurrence of the Delimiter character.
        ''' If Delimiter isn't found the whole string is returned.
        ''' </summary>
         ''' <param name="Value">            Input String. </param>
         ''' <param name="Delimiter">        String to stop at. </param>
         ''' <param name="IncludeDelimiter"> If <see langword="true"/>, the returned string containes the <paramref name="Delimiter"/>, otherwise it doesn't. </param>
         ''' <param name="GetMaximumMatch">  If <see langword="true"/>, the first occurrence of <paramref name="Delimiter"/> will be used, otherwise the last. </param>
         ''' <returns> Truncated string, or <see langword="null"/> if <paramref name="Value"/> is <see langword="null"/>. </returns>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function Right(ByVal Value As String, Delimiter As String, Optional IncludeDelimiter As Boolean = False, Optional GetMaximumMatch As Boolean = False) As String
            If (Value IsNot Nothing) Then
                Dim idx As Integer = 0
                If (GetMaximumMatch) Then
                    idx = Value.IndexOf(Delimiter, comparisonType:=System.StringComparison.Ordinal)
                Else
                    idx = Value.LastIndexOf(Delimiter, comparisonType:=System.StringComparison.Ordinal)
                End If
                If (idx >= 0) Then
                    If (Not IncludeDelimiter) Then idx = idx + Delimiter.Length
                    Value = Value.Substring(idx)
                End If
            End If
            Return Value
        End Function
        
        ''' <summary> Returns the substring between the two Delimiter characters. </summary>
         ''' <param name="Value">             Input String. </param>
         ''' <param name="LeftDelimiter">     String to start. </param>
         ''' <param name="RightDelimiter">    String to stop at. </param>
         ''' <param name="IncludeDelimiters"> If <see langword="true"/>, the returned string containes the Delimiters, otherwise it doesn't. </param>
         ''' <returns> Truncated string, or <see langword="null"/> if <paramref name="Value"/> is <see langword="null"/>. </returns>
         ''' <remarks>
         ''' Returns the substring between the first occurrence of the <paramref name="LeftDelimiter"/> string
         ''' and the following first occurrence of the <paramref name="RightDelimiter"/> string.
         ''' If left Delimiter isn't found the result string starts at the beginning of input string.
         ''' If right Delimiter isn't found the result string ends at the end of input string.
         ''' </remarks>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function Substring(ByVal Value As String, LeftDelimiter As String, RightDelimiter As String, Optional IncludeDelimiters As Boolean = False) As String
            If (Value IsNot Nothing) Then
                Dim idx  As Integer = Value.IndexOf(LeftDelimiter, comparisonType:=System.StringComparison.Ordinal)
                ' Cut left
                If (idx >= 0) Then
                    If (Not IncludeDelimiters) Then idx = idx + LeftDelimiter.Length
                    Value = Value.Substring(idx)
                End If
                ' Cut right
                If (IncludeDelimiters) Then
                    Value = LeftDelimiter & Value.Substring(LeftDelimiter.Length).Left(RightDelimiter, IncludeDelimiters)
                Else
                    Value = Value.Left(RightDelimiter, IncludeDelimiters)
                End If
            End If
            
            Return Value
        End Function
        
        ''' <summary> Encloses the string in extra 2 lines that consist of "line characters" and optionally adds an empty line above and below. </summary>
         ''' <param name="Value">    One text line meant as head line. </param>
         ''' <param name="LineChar"> The character that builds the lines. </param>
         ''' <param name="Padding">  If <see langword="true"/> an empty line is added above and below the header. </param>
         ''' <returns>               The Headline string. </returns>
         ''' <exception cref="System.ArgumentNullException"> <paramref name="Value"/> is <see langword="null"/> or empty. </exception>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ToHeadLine(Value As String, LineChar As Char, Optional Padding As Boolean = True) As String
            
            If (Value.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("Value")
            
            Dim Pad  As String = If(Padding, vbNewLine, "")
            Dim Line As String = LineChar.ToString().Repeat(Value.Length + 2)
            Return Pad & Line & vbNewLine & " " & Value & vbNewLine & Line & Pad
        End Function
        
        ''' <summary> Converts this Boolean value to "Ja" / "Nein" instead of "True" / "False". </summary>
         ''' <param name="Value"> The boolean value to convert. </param>
         ''' <param name="dummy"> This parameter only exists to resolve the overloads to this method. </param>
         ''' <returns> "Ja" / "Nein" </returns>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function ToString(Value As Boolean, dummy As Integer) As String
            If (Value) Then
                Return "Ja"
            Else
                Return "Nein"
            End If
        End Function
        
        ''' <summary> Splits the string into lines. Delimiters are <c>vbCrLf</c>, <c>vbLf</c> and <c>vbCr</c> - in this order. </summary>
         ''' <param name="Value"> The string to split. </param>
         ''' <returns>            A String array containing all lines. </returns>
         ''' <remarks>            If the trimmed input string is empty, the returned array will contain one empty line. </remarks>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function splitLines(Value As String) As String()
            Return Value.Split({vbCrLf, vbLf, vbCr}, System.StringSplitOptions.None)
        End Function
        
        ''' <summary> Awk like splitting: Delimiter is whole whitespace. A word cannot contain white space. Words are trimmed. </summary>
         ''' <param name="Value"> The string to split. </param>
         ''' <returns>            A String array containing all data words. </returns>
         ''' <remarks>            If the trimmed input string is empty, the returned array will be of zero length. </remarks>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function splitWords(Value As String) As String()
            Return Value.Trim().Split("\s+")
        End Function
        
        ''' <summary> Indents the single lines of the string. </summary>
         ''' <param name="Value">                     The string to split. </param>
         ''' <param name="Width">                     The width or count of spaces to prepend every line with. </param>
         ''' <param name="IncludeFirstline">          If <see langword="true"/>, the first line will be indented too. </param>
         ''' <param name="PrependNewlineIfMultiline"> If <see langword="true"/> and <paramref name="Value"/> is multi-line, then <paramref name="Value"/> will be prepended by a new line. </param>
         ''' <returns>                                A String with  indented lines. </returns>
         ''' <remarks> </remarks>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function indent(Value As String, Width As Integer, IncludeFirstline As Boolean, PrependNewlineIfMultiline As Boolean) As String
            Dim RetValue As String = Value
            Dim Prepend  As String = " ".Repeat(Width)
            
            If (IncludeFirstline) Then RetValue = Prepend & RetValue
            
            If (RetValue.Contains(vbNewLine)) Then
                RetValue = RetValue.Replace(vbNewLine, vbNewLine & Prepend)
                If (PrependNewlineIfMultiline) Then
                    RetValue = vbNewLine & RetValue
                End If
            End If
            
            Return RetValue
        End Function
        
    End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
