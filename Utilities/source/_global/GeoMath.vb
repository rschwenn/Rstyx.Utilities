
Imports System
Imports System.IO

Imports PGK.Extensions

'Namespace GeoMath
    
    ''' <summary> Static utility methods for (geodetic) mathematic needs. </summary>
    Public NotInheritable Class GeoMath
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.GeoMath")
            
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
                dim RHO     As Double
                dim Angle   As Double
                RHO = 200 / System.Math.PI
                Angle = Radiant * RHO
                Do While (Angle < 0)
                    Angle = Angle + 400
                Loop
                Do While (Angle > 400)
                    Angle = Angle - 400
                Loop
                Return Angle
            End Function
            
            ''' <summary> Normalization of an angle given in [Rad]. </summary>
             ''' <param name="Radiant"> Angle in [Rad] </param>
             ''' <returns> Angle in [Rad] between -PI and +PI </returns>
            Public Shared Function normalizeRadiant(ByVal Radiant As Double) As Double
                Dim TwoPI   As Double
                Dim Angle   As Double
                TwoPI = 2 * System.Math.PI
                Angle = Radiant
                If (not Double.IsNaN(Angle)) Then
                    Do While (Angle < -System.Math.PI)
                        Angle = Angle + TwoPI
                    Loop
                    Do While (Angle > System.Math.PI)
                        Angle = Angle - TwoPI
                    Loop
                End If
                Return Angle
            End Function
            
            ''' <summary> Normalization of an angle given in [Gon]. </summary>
             ''' <param name="Gon"> Angle in [Gon] </param>
             ''' <returns> Angle in [Gon] between 0 and 400 </returns>
            Public Shared Function normalizeGon(ByVal Gon As Double) As Double
                Dim Angle  As Double
                Angle = Gon
                If (not Double.IsNaN(Angle)) Then
                    Do While (Angle < 0)
                       Angle = Angle + 400
                    Loop
                    Do While (Angle > 400)
                        Angle = Angle - 400
                    Loop
                End If
                Return Angle
            End Function
            
            ''' <summary> Convert octal String representation without prefix to decimal Long (i.e "20" => 16). </summary>
             ''' <param name="Octal"> String representation of a number that is to be interpreted as octal. </param>
             ''' <returns> Decimal number or <see langword="null"/> </returns>
            Public Shared Function Oct2Dec(ByVal Octal As String) As Nullable(Of Long)
                Dim OneChar As String = String.Empty
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
             ''' <returns> Decimal number or <see langword="null"/> </returns>
            Public Shared Function Hex2Dec(ByVal Hexadecimal As String) As Nullable(Of Long)
                Dim Dec     As Nullable(Of Long)
                Try
                    Dec = CLng("&H" & Hexadecimal)
                Catch ex As System.Exception
                    Dec = Nothing
                End Try
                Return Dec
            End Function
            
            
        #End Region
        
    End Class
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
