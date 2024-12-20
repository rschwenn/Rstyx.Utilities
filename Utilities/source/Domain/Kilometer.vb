﻿
Imports System
Imports System.ComponentModel
Imports System.Globalization
Imports System.Math
Imports System.Text.RegularExpressions

Namespace Domain
    
    ''' <summary> Status of a Kilometer Value. </summary>
     <Serializable> _
    Public Enum KilometerStatus As Integer
        
        ''' <summary> Status (ambiguity) is unknown. </summary>
        Unknown = -1
        
        ''' <summary> Unambiguous Kilometer Value (at least not located in the incoming sector of a Kilometer skip of overlegth). </summary>
        Normal = 0
        
        ''' <summary> Ambiguous Kilometer Value. It's located in the incoming sector of a Kilometer skip of overlegth. </summary>
        SkipIncoming = 1
        
        ''' <summary> Ambiguous Kilometer Value. It's located in the outgoing sector of a Kilometer skip of overlegth. </summary>
        SkipOutgoing = 2
        
    End Enum

    ''' <summary> Provides Conversions for a <see cref="Kilometer"/>. </summary>
    Public Class KilometerConverter
        Inherits TypeConverter
            
        ''' <summary> Creates an instance of this converter. </summary>
        Public Sub New()
        End Sub
        
        ''' <summary> Determines whether this converter can convert an object of given type to a <see cref="Kilometer"/>. </summary>
         ''' <param name="Context">    An <see cref="ITypeDescriptorContext"/>, that provides a format context. </param>
         ''' <param name="SourceType"> The Type to convert a <see cref="Kilometer"/>  from. </param>
         ''' <returns> <see langword="true"/>, if this converter can perform the conversion, otherwise <see langword="false"/>. </returns>
        Public Overrides Overloads Function CanConvertFrom(Context As ITypeDescriptorContext, SourceType As Type) As Boolean
            Return ((SourceType Is GetType(String)) OrElse MyBase.CanConvertFrom(Context, SourceType))
        End Function
        
        ''' <summary> Converts a given object of any type into a <see cref="Kilometer"/>. </summary>
         ''' <param name="Context"> An <see cref="ITypeDescriptorContext"/>, that provides a format context. </param>
         ''' <param name="Culture"> The <see cref="CultureInfo"/>  to use as the current culture. </param>
         ''' <param name="Value">   The source object to convert. </param>
         ''' <returns> A new <see cref="Kilometer"/>, created from <paramref name="Value"/>. </returns>
        Public Overrides Overloads Function ConvertFrom(Context As ITypeDescriptorContext, Culture As CultureInfo, Value As Object) As Object
            Dim RetValue As New Kilometer()
            If (TypeOf Value Is String) Then
                RetValue.Parse(CStr(Value))
            Else
                RetValue = MyBase.ConvertFrom(Context, Culture, Value)
            End If
            Return RetValue
        End Function
        
        ''' <summary> Converts a <see cref="Kilometer"/>  into an object of given type. </summary>
         ''' <param name="Context"> An <see cref="ITypeDescriptorContext"/>, that provides a format context. </param>
         ''' <param name="Culture"> The <see cref="CultureInfo"/>  to use as the current culture. </param>
         ''' <param name="Value">   The <see cref="Kilometer"/>  to convert. </param>
         ''' <param name="DestinationType"> The type to convert the <see cref="Kilometer"/>  into. </param>
         ''' <returns> The converted <see cref="Kilometer"/>. </returns>
        Public Overrides Overloads Function ConvertTo(Context As ITypeDescriptorContext, Culture As CultureInfo, Value As Object, DestinationType As Type) As Object
            ' Should be default behavior (?):
            'If (DestinationType Is GetType(String)) Then
            '   Return DirectCast(Value, Kilometer).ToString()
            'End If
            Return MyBase.ConvertTo(Context, Culture, Value, DestinationType)
        End Function
       
    End Class    
    
    ''' <summary> Represents a Kilometer, supporting several notations. </summary>
     ''' <remarks>
     ''' The properties of this class (except <see cref="Kilometer.Text"/>) are read-only. They can be set at construction 
     ''' or by the <see cref="Kilometer.Parse(String)"/> or <see cref="Kilometer.TryParse(String)"/> methods only.
     ''' </remarks>
     <Serializable> _
     <TypeConverter(GetType(KilometerConverter))> _
    Public Class Kilometer
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Domain.Kilometer")
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new Kilometer. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new Kilometer, initialized from a number. </summary>
             ''' <param name="Number"> A floating point number. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="Number"/> is greater than 90.000.000, it will be treated as TDB notation.
             ''' In this case, <see cref="Kilometer.Status"/> may be recognized to be <see cref="Kilometer.Status"/><c>.SkipIncoming</c>
             ''' or <see cref="Kilometer.Status"/><c>.Normal</c>
             ''' Otherwise it will be left at <see cref="Kilometer.Status"/><c>.Unknown</c>.
             ''' </para>
             ''' </remarks>
            Public Sub New(ByVal Number As Double)
                ParseNumber(Number)
            End Sub
            
            ''' <summary> Creates a new Kilometer, initialized from a number with given <see cref="KilometerStatus"/>. </summary>
             ''' <param name="Number"> A floating point number. </param>
             ''' <param name="Status"> The known <see cref="KilometerStatus"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="Number"/> is greater than 90.000.000, it will be treated as TDB notation.
             ''' In this case, <see cref="Kilometer.Status"/> may be recognized to be <see cref="Kilometer.Status"/><c>.SkipIncoming</c>.
             ''' Otherwise it will be set to <paramref name="Status"/>.
             ''' </para>
             ''' </remarks>
            Public Sub New(ByVal Number As Double, ByVal Status As KilometerStatus)
                
                ParseNumber(Number)
                
                ' Status hasn't been determined by TDB as "Incoming" yet.
                If (Not (_Status = KilometerStatus.SkipIncoming)) Then
                    _Status = Status
                End If
                
                ' Re-create Me.Text with newly set Me.Status.
                _Text = ToString()
            End Sub
            
            ''' <summary> Creates a new Kilometer, initialized from a string. </summary>
             ''' <param name="KilometerString"> A usual Kilometer notation or a (special) numerical String. </param>
             ''' <remarks>
             ''' <para>
             ''' If parsing has been successful, the properties provide the recognized values. Otherwise they will be set to <c>Double.NaN</c>.
             ''' </para>
             ''' <para>
             ''' First, the string will be tried to be recognized as Kilometer notation, on failure as double number.
             ''' The double number may be preceeded or followed by an asterisk in order to set <see cref="Kilometer.Status"/>.
             ''' If double number is greater than 90.000.000 it will be treated as TDB notation.
             ''' </para>
             ''' <para>
             ''' Examples for <paramref name="KilometerString"/>: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Input Value</b>  </term>  <description> Result </description></listheader>
             ''' <item> <term> 12.3 + 45.678    </term>  <description> 12345.678, staus = normal   </description></item>
             ''' <item> <term> -0.1 - 212.13    </term>  <description>   -312.13, staus = incoming </description></item>
             ''' <item> <term> -0.1 - 12.13     </term>  <description>   -112.13, staus = normal   </description></item>
             ''' <item> <term>  12.3456         </term>  <description>   12.3456, staus = normal   </description></item>
             ''' <item> <term> *12.3456         </term>  <description>   12.3456, staus = incoming </description></item>
             ''' <item> <term>  12.3456*        </term>  <description>   12.3456, staus = outgoing </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="KilometerString"/> is <see langword="null"/> or <c>String.Empty</c>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="KilometerString"/> isn't a valid Kilometer notation. </exception>
            Public Sub New(ByVal KilometerString As String)
                
                If (KilometerString.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("KilometerString")
                
                If (Not TryParse(KilometerString)) Then
                    Throw New System.ArgumentException(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.Kilometer_InvalidKilometerNotation, KilometerString), "KilometerString")
                End If
            End Sub
            
            ''' <summary> Creates a new Kilometer, initialized from a string. </summary>
             ''' <param name="KilometerString"> A usual Kilometer notation or a (special) numerical String. </param>
             ''' <param name="Status"> The known <see cref="KilometerStatus"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If parsing has been successful, the properties provide the recognized values. Otherwise they will be set to <c>Double.NaN</c>.
             ''' </para>
             ''' <para>
             ''' First, the string will be tried to be recognized as Kilometer notation, on failure as double number.
             ''' The double number may be preceeded or followed by an asterisk in order to set <see cref="Kilometer.Status"/>.
             ''' If double number is greater than 90.000.000 it will be treated as TDB notation.
             ''' </para>
             ''' <para>
             ''' Examples for <paramref name="KilometerString"/>: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Input Value</b>  </term>  <description> Result </description></listheader>
             ''' <item> <term> 12.3 + 45.678    </term>  <description> 12345.678, staus = normal   </description></item>
             ''' <item> <term> -0.1 - 212.13    </term>  <description>   -312.13, staus = incoming </description></item>
             ''' <item> <term> -0.1 - 12.13     </term>  <description>   -112.13, staus = normal   </description></item>
             ''' <item> <term>  12.3456         </term>  <description>   12.3456, staus = normal   </description></item>
             ''' <item> <term> *12.3456         </term>  <description>   12.3456, staus = incoming </description></item>
             ''' <item> <term>  12.3456*        </term>  <description>   12.3456, staus = outgoing </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="KilometerString"/> is <see langword="null"/> or <c>String.Empty</c>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="KilometerString"/> isn't a valid Kilometer notation. </exception>
            Public Sub New(ByVal KilometerString As String, ByVal Status As KilometerStatus)
                
                Me.New(KilometerString)
                
                ' Status hasn't been determined as "Incoming" yet.
                If (Not (_Status = KilometerStatus.SkipIncoming)) Then
                    _Status = Status
                End If
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _Value      As Double = Double.NaN
            Private _TDBValue   As Double = Double.NaN
            Private _SD         As Double = Double.NaN
            Private _Status     As KilometerStatus = KilometerStatus.Unknown
            Private _Text       As String = Nothing
            
            ''' <summary> The ordinary Kilometer Value. Defaults to <c>Double.NaN</c>. </summary>
            Public ReadOnly Property Value() As Double
                Get
                    Return _Value
                End Get
            End Property
            
            ''' <summary> The Kilometer Value in TDB format. Defaults to <c>Double.NaN</c>. </summary>
            Public ReadOnly Property TDBValue()  As Double
                Get
                    Return _TDBValue
                End Get
            End Property
            
            ''' <summary> The Kilometer status. Tells about skip overlength zone. </summary>
            Public ReadOnly Property Status() As KilometerStatus
                Get
                    Return _Status
                End Get
            End Property
            
            ''' <summary> The Kilometer notation text, i.e. "12.3 + 45.678" </summary>
            ''' <remarks>
            ''' This property will be set at construction or by the <see cref="Kilometer.Parse(String)"/> 
            ''' or <see cref="Kilometer.TryParse(String)"/> methods, as all other properties.
            ''' However, in contrast, this property also may be set directly.
            ''' </remarks>
            Public Property Text() As String
                Get
                    Return _Text
                End Get
                Set(ByVal KmText As String)
                    _Text = KmText
                End Set
            End Property

            ''' <summary> The standard deviation of Kilometer Value. Defaults to <c>Double.NaN</c>. </summary>
            Public Property SD() As Double
                Get
                    Return _SD
                End Get
                Set(ByVal KmSD As Double)
                    _SD = KmSD
                End Set
            End Property
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Formats this Kilometer object to a usual Kilometer notation (i.e 123456.789 => "123.4+56.789"). </summary>
             ''' <param name="Precision">   Desired output precison. </param>
             ''' <returns> Usual Kilometer Notation. </returns>
             ''' <remarks> Example output: "12.3 + 5.67". </remarks>
            Public Function ToKilometerNotation(byVal Precision As Integer) As String
                Return ToKilometerNotation(Precision, Nothing)
            End Function
            
            ''' <summary> Formats this Kilometer object to a usual Kilometer notation (i.e 123456.789 => "123.4+56.789"). </summary>
             ''' <param name="Precision">   Desired output precison. </param>
             ''' <param name="PrefixMeter"> Prefix for Meters if &lt; 10 m  (useful seem only to be: "", " ", "0"). </param>
             ''' <returns> Usual Kilometer Notation. </returns>
             ''' <remarks> Example output: "12.3 + 05.67", "12.3 + 5.67", "12.3 +  5.67". </remarks>
            Public Function ToKilometerNotation(byVal Precision As Integer, byVal PrefixMeter As String) As String
                
                Dim HektoMeter  As Double
                Dim Meter       As Double
                Dim Part1       As String = String.Empty
                Dim Part2       As String = String.Empty
                
                If (Not Double.IsNaN(_Value)) Then
                    
                    If (Not Double.IsNaN(_TDBValue)) Then
                        HektoMeter = Truncate((_TDBValue - 100000000) / 10000)
                        Meter      = _TDBValue - 100000000 - (HektoMeter * 10000)
                    Else
                        HektoMeter = Truncate(_Value / 100)
                        Meter      = _Value - (HektoMeter * 100)
                    End If
                    
                    ' Format without blanks.
                    Part1 = StringUtils.Sprintf("%.1f", HektoMeter / 10)
                    Part2 = StringUtils.Sprintf("%+." & Precision & "f", Meter)
                    
                    ' Insert blanks and maybe prefix for meters.
                    If (_Value >= 0) Then
                        If ((Meter < 10) AndAlso PrefixMeter.IsNotEmpty()) Then Part2 = Replace(Part2, "+", "+" & PrefixMeter)
                        Part2 = Replace(Part2, "+", " + ")
                    Else
                        If ((Meter > -10) AndAlso PrefixMeter.IsNotEmpty()) Then Part2 = Replace(Part2, "-", "-" & PrefixMeter)
                        Part2 = Replace(Part2, "-", " - ")
                    End If
                End If
                
                Return Part1 & Part2
            End Function
            
            ''' <summary>
            ''' Formats this Kilometer object to a usual Kilometer notation inclusive standard deviation, if available.
            ''' (i.e 123456.789 => "123.4+56.789 (±0.01)"). 
            ''' </summary>
             ''' <param name="Precision">   Desired output precison. </param>
             ''' <returns> Usual Kilometer Notation. </returns>
             ''' <remarks> Example output: "12.3 + 05.67 (± 0.01)", "12.3 + 5.67 (± 100.00)", "12.3 +  5.67". </remarks>
            Public Function ToKilometerNotationSD(byVal Precision As Integer) As String
                Return ToKilometerNotationSD(Precision, Nothing)
            End Function
            
            ''' <summary>
            ''' Formats this Kilometer object to a usual Kilometer notation inclusive standard deviation, if available.
            ''' (i.e 123456.789 => "123.4+56.789 (±0.01)"). 
            ''' </summary>
             ''' <param name="Precision">   Desired output precison. </param>
             ''' <param name="PrefixMeter"> Prefix for Meters if &lt; 10 m  (useful seem only to be: "", " ", "0"). </param>
             ''' <returns> Usual Kilometer Notation inclusive standard deviation, if available. </returns>
             ''' <remarks>
             ''' <para>
             ''' A negative value of <see cref="SD"/> is treated like <c>NaN</c>.
             ''' </para>
             ''' <para>
             ''' Example output: "12.3 + 05.67 (±0.01)", "12.3 + 5.67 (±100.00)", "12.3 +  5.67". 
             ''' </para>
             ''' </remarks>
            Public Function ToKilometerNotationSD(byVal Precision As Integer, byVal PrefixMeter As String) As String
                
                Dim RetValue As String = ToKilometerNotation(Precision, PrefixMeter)

                If (RetValue.IsNotEmptyOrWhiteSpace()) Then
                    If ((Not Double.IsNaN(_SD)) AndAlso (_SD >= 0.0) ) Then
                        RetValue += StringUtils.Sprintf(" (±%." & CStr(Precision) & "f)", _SD)
                    End If
                End If

                Return RetValue
            End Function
            
            ''' <summary> Parses a string as usual Kilometer notation. </summary>
             ''' <param name="KilometerString"> A usual Kilometer notation or a (special) numerical String. </param>
             ''' <remarks>
             ''' <para>
             ''' If parsing has been successful, the properties provide the recognized values. Otherwise they will be set to <c>Double.NaN</c>.
             ''' </para>
             ''' <para>
             ''' First, the string will be tried to be recognized as Kilometer notation, on failure as double number.
             ''' The double number may be preceeded or followed by an asterisk in order to set <see cref="Kilometer.Status"/>.
             ''' If double number is greater than 90.000.000 it will be treated as TDB notation.
             ''' </para>
             ''' <para>
             ''' Examples for <paramref name="KilometerString"/>: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Input Value</b>  </term>  <description> Result </description></listheader>
             ''' <item> <term> 12.3 + 45.678    </term>  <description> 12345.678, staus = normal   </description></item>
             ''' <item> <term> -0.1 - 212.13    </term>  <description>   -312.13, staus = incoming </description></item>
             ''' <item> <term> -0.1 - 12.13     </term>  <description>   -112.13, staus = normal   </description></item>
             ''' <item> <term>  12.3456         </term>  <description>   12.3456, staus = normal   </description></item>
             ''' <item> <term> *12.3456         </term>  <description>   12.3456, staus = incoming </description></item>
             ''' <item> <term>  12.3456*        </term>  <description>   12.3456, staus = outgoing </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="KilometerString"/> is <see langword="null"/> or empty or whitespace only. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="KilometerString"/> can't be parsed into a <see cref="Kilometer"/>. </exception>
            Public Sub Parse(ByVal KilometerString As String)
                If (KilometerString.IsEmptyOrWhiteSpace())  Then Throw New System.ArgumentNullException("KilometerString")
                If (Not TryParse(KilometerString)) Then Throw New System.ArgumentException("KilometerString")
            End Sub
            
            ''' <summary> Tries to parse a string as usual Kilometer notation. </summary>
             ''' <param name="KilometerString"> A usual Kilometer notation or a (special) numerical String. </param>
             ''' <returns> <see langword="true"/>, if the string has been parsed successful as Kilometer, otherwise <see langword="false"/>. </returns>
             ''' <remarks>
             ''' <para>
             ''' If parsing has been successful, the properties provide the recognized values. Otherwise they will be set to <c>Double.NaN</c>.
             ''' </para>
             ''' <para>
             ''' First, the string will be tried to be recognized as Kilometer notation, on failure as double number.
             ''' The double number may be preceeded or followed by an asterisk in order to set <see cref="Kilometer.Status"/>.
             ''' If double number is greater than 90.000.000 it will be treated as TDB notation.
             ''' </para>
             ''' <para>
             ''' Examples for <paramref name="KilometerString"/>: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Input Value</b>  </term>  <description> Result </description></listheader>
             ''' <item> <term> 12.3 + 45.678    </term>  <description> 12345.678, staus = normal   </description></item>
             ''' <item> <term> -0.1 - 212.13    </term>  <description>   -312.13, staus = incoming </description></item>
             ''' <item> <term> -0.1 - 12.13     </term>  <description>   -112.13, staus = normal   </description></item>
             ''' <item> <term>  12.3456         </term>  <description>   12.3456, staus = normal   </description></item>
             ''' <item> <term> *12.3456         </term>  <description>   12.3456, staus = incoming </description></item>
             ''' <item> <term>  12.3456*        </term>  <description>   12.3456, staus = outgoing </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Function TryParse(ByVal KilometerString As String) As Boolean
                Dim success As Boolean = False
                Reset()
                
                If (KilometerString.IsNotEmptyOrWhiteSpace()) Then
                    
                    Dim Pattern As String = "^ *([+\-]? *[0-9]*[.]*[0-9]+)([-+ ]+)([0-9]*[.]*[0-9]+) *$"
                    Dim oMatch  As Match  = Regex.Match(KilometerString, Pattern, RegexOptions.IgnoreCase)
                    
                    If (Not oMatch.Success) Then
                        ' No valid Kilometer notation => maybe numeric or special numeric notation (*<number> or <number>*).
                        Dim DoubleValue As Double
                        Dim KmString    As String = KilometerString.Trim()
                        
                        If (KmString.Left(1) = "*") Then
                            _Status  = KilometerStatus.SkipIncoming
                            KmString = KmString.Substring(1)
                        ElseIf (KmString.Right(1) = "*") Then
                            _Status  = KilometerStatus.SkipOutgoing
                            KmString = KmString.Substring(0, KmString.Length - 1)
                        End If
                        
                        If (Double.TryParse(KmString, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, DoubleValue)) Then
                            ' Numeric string.
                            success = True
                            ParseNumber(DoubleValue)
                        End If
                    Else
                        ' Valid Kilometer notation.
                        Dim Kilometer  As Double  = oMatch.Groups(1).Value.Replace(" ", "").ConvertTo(Of Double)  ' signed
                        Dim MiddleSign As String  = oMatch.Groups(2).Value
                        Dim Meter      As Double  = oMatch.Groups(3).Value.ConvertTo(Of Double)   ' unsigned
                        Dim SignKm     As Integer = Sign(Kilometer)
                        Dim SignM      As Integer = If(InStr(MiddleSign, "-") > 0, -1, 1)
                        Dim SignTotal  As Integer = If(((SignM = -1) Or (SignKm = -1)), -1, 1)
                        
                        _Value    =  SignTotal * ((Abs(Kilometer) * 1000) + Meter)
                        _TDBValue = (SignTotal * ((Abs(Kilometer) * 100000) + Meter)) + 100000000
                        
                        If (Meter >= 100) Then
                            _Status = KilometerStatus.SkipIncoming
                        Else
                            _Status = KilometerStatus.Normal
                        End If
                        
                        _Text = KilometerString.Replace("   ", " ")
                        
                        success = True
                    End If
                End If
                
                Return success
            End Function
            
            ''' <summary> Checks if <see cref="Value"/> isn't <c>Double.NAN</c>. </summary>
             ''' <returns> <see langword="true"/>, if this Kilometer has a Value. </returns>
            Public Function HasValue() As Boolean
                Return (Not Double.IsNaN(_Value))
            End Function
            
        #End Region

        #Region "Operators"
            
            ''' <summary> Calculates the sum of two <see cref="Kilometer"/>'s. </summary>
             ''' <param name="Km1"> The first operand. </param>
             ''' <param name="Km2"> The second operand. </param>
             ''' <returns> A <see cref="Kilometer"/> which represents the sum of the operands. </returns>
             ''' <remarks>
             ''' CAUTION: 
             ''' The <see cref="Kilometer.Status"/> and <see cref="Kilometer.Text"/> properties of the operands will be ignored for this operation.
             ''' The resulting  <see cref="Kilometer.Status"/> will be <see cref="KilometerStatus.Unknown"/>.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Km1"/> or <paramref name="Km2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator +(ByVal Km1 As Kilometer, ByVal Km2 As Kilometer) As Kilometer
                
                If (Km1 Is Nothing) Then Throw New System.ArgumentNullException("Km1")
                If (Km2 Is Nothing) Then Throw New System.ArgumentNullException("Km2")
                
                Return New Kilometer(Km1.Value + Km2.Value)
            End Operator
            
            ''' <summary> Calculates the sum of a <see cref="Kilometer"/> and a <c>Double</c>. </summary>
             ''' <param name="Km1"> The first operand. </param>
             ''' <param name="Km2"> The second operand. </param>
             ''' <returns> A <see cref="Kilometer"/> which represents the sum of the operands. </returns>
             ''' <remarks>
             ''' CAUTION: 
             ''' The <see cref="Kilometer.Status"/> and <see cref="Kilometer.Text"/> properties of the operands will be ignored for this operation.
             ''' The resulting  <see cref="Kilometer.Status"/> will be <see cref="KilometerStatus.Unknown"/>.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Km1"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator +(ByVal Km1 As Kilometer, ByVal Km2 As Double) As Kilometer
                
                If (Km1 Is Nothing) Then Throw New System.ArgumentNullException("Km1")
                
                Return New Kilometer(Km1.Value + Km2)
            End Operator
            
            ''' <summary> Calculates the difference of two <see cref="Kilometer"/>'s. </summary>
             ''' <param name="Km1"> The first operand. </param>
             ''' <param name="Km2"> The second operand. </param>
             ''' <returns> A <see cref="Kilometer"/> which represents the difference of the operands. </returns>
             ''' <remarks>
             ''' CAUTION: 
             ''' The <see cref="Kilometer.Status"/> and <see cref="Kilometer.Text"/> properties of the operands will be ignored for this operation.
             ''' The resulting  <see cref="Kilometer.Status"/> will be <see cref="KilometerStatus.Unknown"/>.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Km1"/> or <paramref name="Km2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator -(ByVal Km1 As Kilometer, ByVal Km2 As Kilometer) As Kilometer
                
                If (Km1 Is Nothing) Then Throw New System.ArgumentNullException("Km1")
                If (Km2 Is Nothing) Then Throw New System.ArgumentNullException("Km2")
                
                Return New Kilometer(Km1.Value - Km2.Value)
            End Operator
            
            ''' <summary> Calculates the difference of a <see cref="Kilometer"/> and a <c>Double</c>. </summary>
             ''' <param name="Km1"> The first operand. </param>
             ''' <param name="Km2"> The second operand. </param>
             ''' <returns> A <see cref="Kilometer"/> which represents the difference of the operands. </returns>
             ''' <remarks>
             ''' CAUTION: 
             ''' The <see cref="Kilometer.Status"/> and <see cref="Kilometer.Text"/> properties of the operands will be ignored for this operation.
             ''' The resulting  <see cref="Kilometer.Status"/> will be <see cref="KilometerStatus.Unknown"/>.
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Km1"/> or <paramref name="Km2"/> is <see langword="null"/>. </exception>
            Public Shared Overloads Operator -(ByVal Km1 As Kilometer, ByVal Km2 As Double) As Kilometer
                
                If (Km1 Is Nothing) Then Throw New System.ArgumentNullException("Km1")
                
                Return New Kilometer(Km1.Value - Km2)
            End Operator
            

            ''' <summary> Converts a <see cref="Kilometer"/>  into a String. </summary>
             ''' <param name="Km"> The <see cref="Kilometer"/>  to convert. </param>
             ''' <returns> A usual Kilometer notation. </returns>
            Public Shared Overloads Widening Operator CType(ByVal Km As Kilometer) As String
                Return Km.ToString()
            End Operator

            ''' <summary> Converts a String into a <see cref="Kilometer"/>. </summary>
             ''' <param name="KilometerString"> A usual Kilometer notation or a (special) numerical String. </param>
             ''' <returns> The matching Kilometer. </returns>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="KilometerString"/> is <see langword="null"/> or <c>String.Empty</c>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="KilometerString"/> isn't a valid Kilometer notation. </exception>
            Public Shared Overloads Widening Operator CType(ByVal KilometerString As String) As Kilometer
                Return New Kilometer(KilometerString)
            End Operator

        #End Region


        #Region "Overrides"
            
            ''' <summary> Returns a formatted Kilometer output. </summary>
             ''' <returns> A Kilometer notation with precision set to 3. </returns>
            Public Overrides Function ToString() As String
                Return Me.ToKilometerNotation(Precision:=3)
            End Function
            
        #End Region
        
        #Region "Private Methods"
            
            ''' <summary> Parses a number as kilometer and sets all properties. </summary>
             ''' <param name="Number"> A floating point number. </param>
             ''' <remarks>
             ''' <para>
             ''' If double number is greater than 90.000.000, it will be treated as TDB notation.
             ''' </para>
             ''' </remarks>
            Private Sub ParseNumber(ByVal Number As Double)
                
                If (Number > 90000000) Then
                    ParseTDB(Number)
                Else
                    Dim Kilometer  As Double = Truncate(Number / 100) / 10
                    Dim Meter      As Double = Number - (Kilometer * 1000)
                    _Value    = Number
                    _TDBValue = (Kilometer * 100000) + Meter + 100000000
                    _Text     = ToString()
                End If
            End Sub
            
            ''' <summary> Parses a double TDB notation and sets all properties. </summary>
             ''' <param name="TDB"> A usual TDB Kilometer notation. </param>
            Private Sub ParseTDB(ByVal TDB As Double)
                
                Dim Kilometer As Double = Truncate((TDB - 100000000) / 10000) / 10
                Dim Meter     As Double = TDB - 100000000 - (Kilometer * 100000)
                
                _TDBValue = TDB
                _Value    = (Kilometer * 1000) + Meter
                
                If (Meter >= 100) Then
                    _Status = KilometerStatus.SkipIncoming
                Else
                    _Status = KilometerStatus.Normal
                End If
                
                _Text = ToString()
            End Sub
            
            ''' <summary> Resets all Properties to <c>Double.NaN</c> or unknown. </summary>
            Public Sub Reset()
                _Status   = KilometerStatus.Unknown
                _TDBValue = Double.NaN
                _Value    = Double.NaN
                _SD       = Double.NaN
                _Text     = Nothing
            End Sub
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
