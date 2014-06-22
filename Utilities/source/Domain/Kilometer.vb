﻿
Imports System.Math
Imports System.Text.RegularExpressions

Namespace Domain
    
    ''' <summary> Status of a Kilometer Value. </summary>
    Public Enum KilometerStatus As Integer
        
        ''' <summary> Status (ambiguity) is unknown. </summary>
        Unknown = -1
        
        ''' <summary> Unambiguous Kilometer value (at least not located in the incoming sector of a Kilometer skip of overlegth). </summary>
        Normal = 0
        
        ''' <summary> Ambiguous Kilometer value. It's located in the incoming sector of a Kilometer skip of overlegth. </summary>
        SkipIncoming = 1
        
        ''' <summary> Ambiguous Kilometer value. It's located in the outgoing sector of a Kilometer skip of overlegth. </summary>
        SkipOutgoing = 2
        
    End Enum
    
    
    ''' <summary> Represents a Kilometer, supporting several notations. </summary>
    Public Class Kilometer
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.Kilometer")
            
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
                parseNumber(Number)
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
                
                parseNumber(Number)
                
                ' Status hasn't been determined by TDB yet.
                If (Not (_Status = KilometerStatus.SkipIncoming)) Then
                    _Status = Status
                End If
            End Sub
            
            ''' <summary> Creates a new Kilometer, initialized from a string. </summary>
             ''' <param name="KilometerString"> A usual Kilometer notation or a numerical String. </param>
             ''' <remarks>
             ''' <para>
             ''' If parsing has been successful, the properties provide the recognized values. Otherwise they will be set to <c>Double.NaN</c>.
             ''' </para>
             ''' <para>
             ''' First, the string will be tried to be recognized as Kilometer notation, on failure as double number.
             ''' If double number is greater than 90.000.000 it will be treated as TDB notation.
             ''' </para>
             ''' <para>
             ''' This constructor throws an exception if <paramref name="KilometerString"/> couldn't be parsed succesfully.
             ''' To avoid an exception, use the parameterless constructor and call <see cref="Kilometer.TryParseKilometer"/>.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="KilometerString"/> is <see langword="null"/> or <c>String.Empty</c>. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="KilometerString"/> isn't a valid Kilometer notation. </exception>
            Public Sub New(ByVal KilometerString As String)
                
                If (KilometerString.IsEmptyOrWhiteSpace()) Then Throw New System.ArgumentNullException("KilometerString")
                
                If (Not TryParseKilometer(KilometerString)) Then
                    Throw New System.ArgumentException(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Kilometer_InvalidKilometerNotation, KilometerString), "KilometerString")
                End If
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            Private _Value      As Double = Double.NaN
            Private _TDBValue   As Double = Double.NaN
            Private _Status     As KilometerStatus = KilometerStatus.Unknown
            
            ''' <summary> The ordinary Kilometer value. Defaults to <c>Double.NaN</c>. </summary>
            Public ReadOnly Property Value() As Double
                Get
                    Return _Value
                End Get
            End Property
            
            ''' <summary> The Kilometer value in TDB format. Defaults to <c>Double.NaN</c>. </summary>
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
                
                Dim HektoMeter As Double
                Dim Meter      As Double
                
                If (Not Double.IsNaN(_TDBValue)) Then
                    HektoMeter = Truncate((_TDBValue - 100000000) / 10000)
                    Meter      = _TDBValue - 100000000 - (HektoMeter * 10000)
                Else
                    HektoMeter = Truncate(_Value / 100)
                    Meter      = _Value - (HektoMeter * 100)
                End If
                
                ' Format without blanks.
                Dim Part1 As String = StringUtils.sprintf("%.1f", HektoMeter / 10)
                Dim Part2 As String = StringUtils.sprintf("%+." & Precision & "f", Meter)
                
                ' Insert blanks and maybe prefix for meters.
                If (_Value >= 0) Then
                    If ((Meter < 10) AndAlso PrefixMeter.IsNotEmptyOrWhiteSpace()) Then Part2 = replace(Part2, "+", "+" & PrefixMeter)
                    Part2 = replace(Part2, "+", " + ")
                Else
                    If ((Meter > -10) AndAlso PrefixMeter.IsNotEmptyOrWhiteSpace()) Then Part2 = replace(Part2, "-", "-" & PrefixMeter)
                    Part2 = replace(Part2, "-", " - ")
                End If
                
                Return Part1 & Part2
            End Function
            
            ''' <summary> Tries to parse a string as usual Kilometer notation. </summary>
             ''' <param name="KilometerString"> A usual Kilometer notation or a numerical String. </param>
             ''' <returns> <see langword="true"/> if the string has been parsed successful as Kilometer, otherwise <see langword="false"/>. </returns>
             ''' <remarks>
             ''' <para>
             ''' If parsing has been successful, the properties provide the recognized values. Otherwise they will be set to <c>Double.NaN</c>.
             ''' </para>
             ''' <para>
             ''' First, the string will be tried to be recognized as Kilometer notation, on failure as double number.
             ''' If double number is greater than 90.000.000 it will be treated as TDB notation.
             ''' </para>
             ''' </remarks>
            Public Function TryParseKilometer(ByVal KilometerString As String) As Boolean
                Dim success As Boolean = False
                reset()
                
                Dim KilometerValue As Double = Double.NaN
                
                Dim Pattern As String = "^ *([+\-]? *[0-9]*[.]*[0-9]+)([-+ ]+)([0-9]*[.]*[0-9]+) *$"
                Dim oMatch  As Match  = Regex.Match(KilometerString, Pattern, RegexOptions.IgnoreCase)
                
                If (Not oMatch.Success) Then
                    ' No valid Kilometer notation => maybe numeric.
                    Dim DoubleValue As Double
                    If (Double.TryParse(KilometerString, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, DoubleValue)) Then
                        success = True
                        parseNumber(DoubleValue)
                    End If
                Else
                    ' Valid Kilometer notation.
                    Dim Kilometer  As Double  = oMatch.Groups(1).Value.Replace(" ", "").ConvertTo(Of Double)  ' signed
                    Dim MiddleSign As String  = oMatch.Groups(2).Value
                    Dim Meter      As Double  = oMatch.Groups(3).Value.ConvertTo(Of Double)   ' unsigned
                    Dim SignKm     As Integer = Sign(Kilometer)
                    Dim SignM      As Integer = If(InStr(MiddleSign, "-") > 0, SignM = -1, SignM = 1)
                    Dim SignTotal  As Integer = If(((SignM = -1) Or (SignKm = -1)), -1, 1)
                    
                    _Value = SignTotal * (Abs(Kilometer) * 1000 + Meter)
                    _TDBValue = (SignTotal * (Abs(Kilometer) * 100000 + Meter)) + 100000000
                    
                    If (Meter >= 100) Then
                        _Status = KilometerStatus.SkipIncoming
                    Else
                        _Status = KilometerStatus.Normal
                    End If
                    
                    success = True
                End If
                
                Return success
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
            Private Sub parseNumber(ByVal Number As Double)
                
                If (Number > 90000000) Then
                    parseTDB(Number)
                Else
                    Dim Kilometer  As Double = Truncate(Number / 100) / 10
                    Dim Meter      As Double = Number - (Kilometer * 1000)
                    _Value    = Number
                    _TDBValue = (Kilometer * 100000) + Meter + 100000000
                End If
            End Sub
            
            ''' <summary> Parses a double TDB notation and sets all properties. </summary>
             ''' <param name="TDB"> A usual Kilometer notation or a numerical String. </param>
            Private Sub parseTDB(ByVal TDB As Double)
                
                Dim Kilometer As Double = Truncate((TDB - 100000000) / 10000) / 10
                Dim Meter     As Double = TDB - 100000000 - (Kilometer * 100000)
                
                _TDBValue = TDB
                _Value    = (Kilometer * 1000) + Meter
                
                If (Meter >= 100) Then
                    _Status = KilometerStatus.SkipIncoming
                Else
                    _Status = KilometerStatus.Normal
                End If
            End Sub
            
            ''' <summary> Resets all Properties to <c>Double.NaN</c> or unknown. </summary>
            Public Sub reset()
                _Status   = KilometerStatus.Unknown
                _TDBValue = Double.NaN
                _Value    = Double.NaN
            End Sub
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4: