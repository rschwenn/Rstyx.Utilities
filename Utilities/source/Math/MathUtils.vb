
Imports System
Imports System.Math
Imports System.IO
Imports System.Runtime.InteropServices

Namespace Math
    
    ''' <summary> Extension methods for numeric types. </summary>
    Public Module MathExtensions
        
        ''' <summary> Checks two double values for equality considering a tolerance. </summary>
         ''' <param name="Value">        First Value </param>
         ''' <param name="CompareValue"> Second Value </param>
         ''' <param name="Tolerance">    Difference between values to tolerate. (<c>Double.NaN</c> is treated as 0.0) </param>
         ''' <returns> <see langword="true"/> if difference between <paramref name="Value"/> and <paramref name="CompareValue"/> is less than or equal <paramref name="Tolerance"/>. </returns>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function EqualsTolerance(Value As Double, CompareValue As Double, ByVal Tolerance As Double) As Boolean
            Dim IsEqual As Boolean = False
            
            If (Double.IsNaN(Tolerance)) Then
                Tolerance = 0.0
            End If
            
            If (Double.IsNaN(Value)) Then
                If (Double.IsNaN(CompareValue)) Then IsEqual = True

            ElseIf (Not Double.IsNaN(CompareValue)) Then
                IsEqual = (Not (Abs(Value - CompareValue) > Abs(Tolerance)))
            End If
            
            Return IsEqual
        End Function
        
        ''' <summary> A filter which converts an infinite value to Zero. </summary>
         ''' <param name="Value"> The Value </param>
         ''' <returns> <c>Zero</c> if <paramref name="Value"/> is Infinity, otherwise <paramref name="Value"/>. </returns>
        <System.Runtime.CompilerServices.Extension()> 
        Public Function InfinityAsZero(Value As Double) As Double
            Return If(Double.IsInfinity(Value), 0.0, Value)
        End Function
            
        #Region "Parsing"
            
            ''' <summary> Replacement for <c>Double.TryParse</c>. Returns <c>Double.NaN</c> if parsing fails. </summary>
             ''' <param name="Result"> The parsing result. <c>Double.NaN</c> if parsing fails. </param>
             ''' <param name="Value">  String to parse. </param>
             ''' <returns> <see langword="true"/> if <paramref name="Value"/> has been parsed successfull, otherwise <see langword="false"/>. </returns>
             ''' <remarks>
             ''' If <c>Double.TryParse</c> fails, then special parsing will be done for "unendlich", "+unendlich", "-unendlich", 
             ''' "infinity", "+infinity", "-infinity" and the infinity sign (U+221E) with or without a sign.
             ''' </remarks>
            <System.Runtime.CompilerServices.Extension()> 
            Public Function TryParse(<out> ByRef Result As Double, Value As String) As Boolean
                Dim success As Boolean = False
                
                If (Value.IsNotEmptyOrWhiteSpace()) Then
                    success = Double.TryParse(Value, Result)
                    
                    If (Not success) Then
                        Dim Inf    As String = ChrW(&H221E)   ' Infinity sign
                        Dim PosInf As String = "+" & Inf      ' Positive Infinity sign
                        Dim NegInf As String = "-" & Inf      ' Negative Infinity sign
                        
                        Select Case Value.ToLowerInvariant()
                            Case "unendlich", "+unendlich", "infinity", "+infinity", Inf, PosInf :  Result = Double.PositiveInfinity : success = True
                            Case              "-unendlich",             "-infinity",      NegInf :  Result = Double.NegativeInfinity : success = True
                        End Select
                    End If
                End If
                
                If (Not success) Then Result = Double.NaN
                
                Return success
            End Function
            
            ''' <summary> Tries to convert a string into a <c>Nullable(Of Integer)</c>. </summary>
             ''' <param name="Result"> The parsing result. It's <see langword="null"/> if parsing fails. </param>
             ''' <param name="Value">  String to parse. </param>
             ''' <returns> <see langword="true"/> if <paramref name="Value"/> has been parsed successfull, otherwise <see langword="false"/>. </returns>
             ''' <remarks></remarks>
            <System.Runtime.CompilerServices.Extension()> 
            Public Function TryParse(<out> ByRef Result As Nullable(Of Integer), Value As String) As Boolean
                Dim success As Boolean = False
                Dim TestInt As Integer = 0
                
                If (Integer.TryParse(Value, TestInt)) Then
                    Result  = TestInt
                    success = True
                Else
                    Result = Nothing
                End If
                
                Return success
            End Function
            
            ''' <summary> Tries to convert a string into a <c>Nullable(Of Long)</c>. </summary>
             ''' <param name="Result"> The parsing result. It's <see langword="null"/> if parsing fails. </param>
             ''' <param name="Value">  String to parse. </param>
             ''' <returns> <see langword="true"/> if <paramref name="Value"/> has been parsed successfull, otherwise <see langword="false"/>. </returns>
             ''' <remarks></remarks>
            <System.Runtime.CompilerServices.Extension()> 
            Public Function TryParse(<out> ByRef Result As Nullable(Of Long), Value As String) As Boolean
                Dim success As Boolean = False
                Dim TestInt As Long = 0
                
                If (Long.TryParse(Value, TestInt)) Then
                    Result  = TestInt
                    success = True
                Else
                    Result = Nothing
                End If
                
                Return success
            End Function
            
        #End Region
        
    End Module
    
    ''' <summary> Static utility methods for (geodetic) mathematic needs. </summary>
    Public NotInheritable Class MathUtils
        
        #Region "Private Fields"
            
            'Private Shared ReadOnly Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.GeoMath")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Static Methods"
            
            ''' <summary> Angle conversion Radiant => Gon. </summary>
             ''' <param name="Radiant"> Angle in [Rad] </param>
             ''' <returns> Angle in [Gon] between 0 and 400 </returns>
            Public Shared Function Rad2Gon(ByVal Radiant As Double) As Double
                Dim RHO   As Double = 200 / System.Math.PI
                Dim Angle As Double = Radiant * RHO
                Do While (Angle < 0)
                    Angle += 400
                Loop
                Do While (Angle > 400)
                    Angle -= 400
                Loop
                Return Angle
            End Function
            
            ''' <summary> Normalization of an angle given in [Rad]. </summary>
             ''' <param name="Radiant"> Angle in [Rad] </param>
             ''' <returns> Angle in [Rad] between -PI and +PI </returns>
            Public Shared Function normalizeRadiant(ByVal Radiant As Double) As Double
                Dim TwoPI As Double = 2 * System.Math.PI
                Dim Angle As Double = Radiant
                If (Not Double.IsNaN(Angle)) Then
                    Do While (Angle < -System.Math.PI)
                        Angle += TwoPI
                    Loop
                    Do While (Angle > System.Math.PI)
                        Angle -= TwoPI
                    Loop
                End If
                Return Angle
            End Function
            
            ''' <summary> Normalization of an angle given in [Gon]. </summary>
             ''' <param name="Gon"> Angle in [Gon] </param>
             ''' <returns> Angle in [Gon] between 0 and 400 </returns>
            Public Shared Function normalizeGon(ByVal Gon As Double) As Double
                Dim Angle As Double = Gon
                If (Not Double.IsNaN(Angle)) Then
                    Do While (Angle < 0)
                       Angle += 400
                    Loop
                    Do While (Angle > 400)
                        Angle -= 400
                    Loop
                End If
                Return Angle
            End Function
            
            ''' <summary> Convert octal String representation without prefix to decimal Long (i.e "20" => 16). </summary>
             ''' <param name="Octal"> String representation of a number that is to be interpreted as octal. </param>
             ''' <returns> Decimal number or <see langword="null"/> </returns>
            Public Shared Function Oct2Dec(ByVal Octal As String) As Nullable(Of Long)
                Dim OneChar As String
                Dim Dec     As Nullable(Of Long) = 0
                Try
                    While (Not String.IsNullOrEmpty(Octal))
                        OneChar = Octal.Substring(0, 1)
                        Dec = Dec * 8 + CInt(OneChar)
                        Octal = Octal.Substring(1)
                    End While
                Catch ex As System.Exception
                    Dec = Nothing
                End Try
                Return Dec
            End Function
            
            ''' <summary> Convert hexadecimal String representation without prefix to decimal Long (i.e "FF" => 255). </summary>
             ''' <param name="Hexadecimal"> String representation of a number that is to be interpreted as hex. </param>
             ''' <returns> Decimal number or <see langword="null"/>. </returns>
            Public Shared Function Hex2Dec(ByVal Hexadecimal As String) As Nullable(Of Long)
                Dim Dec As Nullable(Of Long)
                Try
                    Dec = CLng("&H" & Hexadecimal)
                Catch ex As System.Exception
                    Dec = Nothing
                End Try
                Return Dec
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
