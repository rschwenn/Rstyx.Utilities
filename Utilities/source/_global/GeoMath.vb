
Imports System
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Imports PGK.Extensions

'Namespace GeoMath
    
    ''' <summary> Static utility methods for mathematic or geodetic needs. </summary>
    Public NotInheritable Class GeoMath
        
        #Region "Private Fields"
            
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.GeoMath")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Static Methods - General"
            
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
        
        #Region "Static Methods - Rail related"
            
            ''' <summary> Converts a usual DBAG Kilometer notation into the corresponding numerical value (i.e "123.4+56.789" => 123456.789). </summary>
             ''' <param name="KilometerString"> A usual DBAG Kilometer notation or a numerical String. </param>
             ''' <param name="KilometerValue">  If successfull, the resulting numerical Kilometer value in [Meter]. </param>
             ''' <returns> <see langword="true"/> if a value has been parsed successfull, otherwise <see langword="false"/>. </returns>
            Public Shared Function TryParseKilometer(ByVal KilometerString As String, <Out> ByRef KilometerValue As Double) As Boolean
                Dim success As Boolean = False
                
                KilometerValue = Double.NaN
                
                Dim Pattern As String = "^ *([+\-]? *[0-9]*[.]*[0-9]+)([-+ ]+)([0-9]*[.]*[0-9]+) *$"
                Dim oMatch  As Match  = Regex.Match(KilometerString, Pattern, RegexOptions.IgnoreCase)
                
                If (Not oMatch.Success) Then
                    ' No valid Kilometer notation => maybe numeric?
                    Dim tmp As Double
                    If (Double.TryParse(KilometerString, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, tmp)) Then
                        success = True
                        KilometerValue = tmp
                    End If
                Else
                    Dim Kilometer  As String  = oMatch.Groups(1).Value.Replace(" ", "")
                    Dim MiddleSign As String  = oMatch.Groups(2).Value
                    Dim Meter      As String  = oMatch.Groups(3).Value   ' without Sign
                    Dim SignKm     As Integer = System.Math.Sign(Kilometer.ConvertTo(Of Double))
                    Dim SignM      As Integer = If(InStr(MiddleSign, "-") > 0, SignM = -1, SignM = 1)
                    Dim SignTotal  As Integer = If(((SignM = -1) Or (SignKm = -1)), -1, 1)
                    KilometerValue = SignTotal * (System.Math.Abs(Kilometer.ConvertTo(Of Double)) * 1000 + CDbl(Meter))
                    success = True
                End If
                
                Return success
            End Function
            
            ''' <summary> Parses Cant value from a Pointinfo string. </summary>
             ''' <param name="Pointinfo">     [Input and Output] The Pointinfo string to parse. </param>
             ''' <param name="strict">        If <see langword="true"/>, only the pattern "u=..." is recognized. Otherwise, if this pattern isn't found, the first integer number is used. </param>
             ''' <param name="absolute">      If <see langword="true"/>, the returned Cant value is always positive. </param>
             ''' <param name="editPointInfo"> If <see langword="true"/>, the Cant pattern substring is removed from Pointinfo. </param>
             ''' <returns> The found Cant value or <c>Double.NaN</c>. </returns>
             ''' <remarks> 
             ''' <para>
             ''' The measured cant will be parsed from <paramref name="Pointinfo"/> following these rules:
             ''' </para>
             ''' <para>
             ''' Case 1: If the string "u= xxx" is found anywhere then "xxx" will be treated as measured cant.
             ''' </para>
             ''' <para>
             ''' If case 1 didn't succeeded and <paramref name="strict"/> is <see langword="false"/>,
             ''' then the first integer number will be used, if any.
             ''' </para>
             ''' </remarks>
            Public Shared Function parseCant(<[In]><Out> ByRef Pointinfo As String, _
                                             Optional byVal strict As Boolean = true, _
                                             Optional byVal absolute As Boolean = false, _
                                             Optional byVal editPointInfo As Boolean = false) As Double
                Dim Cant As Double = Double.NaN
                Dim ui   As String = String.Empty
                
                If (Pointinfo.IsNotEmptyOrWhiteSpace()) Then
                    ' 1. search for "u=..."
                    Dim Pattern As String = "u *= *([-|+]? *[0-9]+)"
                    Dim oMatch  As Match = Regex.Match(Pointinfo, Pattern, RegexOptions.IgnoreCase)
                    If (oMatch.Success) Then
                        ui = oMatch.Groups(1).Value
                        ui = ui.Replace(" ", String.Empty)
                    End If
                    
                    ' 2. "u=..." not found, look for first integer number.
                    If (ui.IsEmptyOrWhiteSpace() AndAlso (Not strict)) Then
                        Pattern = "[-|+]? *[0-9]+"
                        oMatch = Regex.Match(Pointinfo, Pattern, RegexOptions.IgnoreCase)
                        If (oMatch.Success) Then
                          ui = oMatch.Value
                          ui = ui.Replace(" ", String.Empty)
                        End If
                    End If
                    
                    ' Result.
                    If (ui.IsNotEmpty()) Then
                        ' 3. Cant value found.
                        Cant = cdbl(ui)
                        If (absolute) Then Cant = System.Math.Abs(Cant.ConvertTo(Of Double))
                        If (editPointInfo) Then Pointinfo = Pointinfo.replace(oMatch.Groups(0).Value, String.Empty)
                    End If
                End If
                
                Return Cant
            End Function
            
            ''' <summary> Determines the title of a DBAG rail track according to the line number. </summary>
             ''' <param name="LineNo"> DBAG rail track no. </param>
             ''' <returns> A <see cref="DBAGTrackInfo"/> record. </returns>
             ''' <remarks> 
             ''' <para>
             ''' The line titles are read from a file with format "number Line title here"
             ''' </para>
             ''' <para>
             ''' The file path is specified by application settings "GeoMath_DBAGTracksFile" or "GeoMath_DBAGTracksFileFallback"
             ''' which may contain environment variables like %variable%. If the first one doesn't determine an existing file
             ''' the second is tried.
             ''' </para>
             ''' </remarks>
            Public Shared Function getDBAGTrackTitle(byVal LineNo As Integer) As DBAGTrackInfo
              ' Declarations:
                Dim LineFromFile     As String = String.Empty
                Dim LineTextShort    As String = String.Empty
                Dim FirstString      As String = String.Empty
                Dim WorkLine         As String = String.Empty
                Dim FromTo()         As String
                Dim tmp()            As String
                Dim NR               As Long    = 0
                Dim LineNoFound      As Boolean = false
                'Dim oSR              As StreamReader = Nothing
                Dim LineInfo         As New DBAGTrackInfo() With {.Number = LineNo}
                
              ' Path of File with DB lines list
                LineInfo.SourceFile = Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGTracksFile)
                If (not File.Exists(LineInfo.SourceFile)) Then
                    LineInfo.SourceFile = Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGTrackFileFallback)
                End If 
                
              ' Process LineInfo.SourceFile
                If (Not File.Exists(LineInfo.SourceFile)) Then
                    Logger.logError(StringUtils.sprintf("getDBAGTrackTitle(): Streckenliste nicht gefunden: '%s' ('%s')!", My.Settings.GeoMath_DBAGTracksFile, Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGTracksFile)))
                    Logger.logError(StringUtils.sprintf("getDBAGTrackTitle(): Streckenliste nicht gefunden: '%s' ('%s')!", My.Settings.GeoMath_DBAGTrackFileFallback, Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGTrackFileFallback)))
                    
                    LineInfo.LongTitle  = "Streckenliste '" & LineInfo.SourceFile & "' nicht gefunden!"
                    LineInfo.ShortTitle = LineInfo.LongTitle
                    LineInfo.ShortDescription = LineInfo.LongTitle
                    LineInfo.LongDescription  = LineInfo.LongTitle
                Else
                    Dim oSR As New StreamReader(LineInfo.SourceFile, System.Text.Encoding.Default)
                    Do While (Not (oSR.EndOfStream Or LineNoFound))
                        WorkLine = oSR.ReadLine()
                        NR = NR + 1
                        If (WorkLine.IsNotEmptyOrWhiteSpace) Then
                            FirstString = WorkLine.Trim().Left(4)
                            If (CDbl(FirstString) = LineNo) Then
                                LineNoFound = true
                                LineInfo.SourceLine = NR
                            End If
                        End If
                    Loop
                    If (LineNoFound) Then LineFromFile = WorkLine.Trim()
                    oSR.Close()
                    
                    ' Determine output text.
                    If (LineFromFile.IsEmptyOrWhiteSpace()) Then
                        LineInfo.LongTitle = "Strecke '" & LineNo & "' nicht gefunden! "
                        LineInfo.ShortTitle = LineInfo.LongTitle
                        LineInfo.ShortDescription = LineInfo.LongTitle
                        LineInfo.LongDescription  = LineInfo.LongTitle
                    Else
                        LineInfo.LongTitle = LineFromFile.Substring(4).Trim()
                        LineInfo.LongDescription = "Strecke " & LineNo & "  " & LineInfo.LongTitle
                        
                        ' Short Line title
                        FromTo = LineInfo.LongTitle.Split(" - ")
                        tmp    = FromTo(0).Split(",")
                        LineTextShort = tmp(0)
                        
                        If (ubound(FromTo) > 0) Then
                            For i As Integer = 1 to ubound(FromTo)
                                tmp = FromTo(i).Split(",")
                                LineTextShort = LineTextShort & " - " & tmp(0)
                            Next
                        End If
                        LineInfo.ShortTitle = LineTextShort
                        LineInfo.ShortDescription = "Strecke " & LineNo & "  " & LineInfo.ShortTitle
                        LineInfo.isValid = True
                    End If
                End If
                
                Return LineInfo
            End Function
            
        #End Region
        
        #Region "Data Structures, Nested Classes"
            
            ''' <summary> DBAG rail track info record </summary>
            Public Class DBAGTrackInfo
                
                ''' <summary> Rail track number </summary>
                Public Number            As Integer = 0
                
                ''' <summary> True, if the line info is dertermined successfully. </summary>
                Public isValid           As Boolean = false
                
                ''' <summary> Composited string like "1234  {short title}" </summary>
                Public ShortDescription  As String  = String.Empty
                
                ''' <summary> Composited string like "1234  {long title}" </summary>
                Public LongDescription   As String  = String.Empty
                
                ''' <summary> Shortened line title (begin and end strings truncated at comma). </summary>
                Public ShortTitle        As String  = String.Empty
                
                ''' <summary> Line title as read from source file. </summary>
                Public LongTitle         As String  = String.Empty
                
                ''' <summary> Path of source file used to read the line info. </summary>
                Public SourceFile        As String  = String.Empty
                
                ''' <summary> Line number in source file, where the line number has been found. </summary>
                Public SourceLine        As Long    = 0
            End Class
            
        #End Region
        
    End Class
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
