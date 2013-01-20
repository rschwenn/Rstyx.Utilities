
Imports System
Imports System.IO
Imports System.Text.RegularExpressions

'Namespace GeoMath
    
    ''' <summary> Static utility methods for mathematic or geodetic needs. </summary>
    Public NotInheritable Class GeoMath
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(MyClass.GetType.FullName)
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
                dim TwoPI   As Double
                dim Angle   As Double
                TwoPI = 2 * System.Math.PI
                Angle = Radiant
                if (not Double.IsNaN(Angle)) then
                    do While (Angle < -System.Math.PI)
                        Angle = Angle + TwoPI
                    Loop
                    do While (Angle > System.Math.PI)
                        Angle = Angle - TwoPI
                    Loop
                end if
                Return Angle
            End Function
            
            ''' <summary> Normalization of an angle given in [Gon]. </summary>
             ''' <param name="Gon"> Angle in [Gon] </param>
             ''' <returns> Angle in [Gon] between 0 and 400 </returns>
            Public Shared Function normalizeGon(ByVal Gon As Double) As Double
                Dim Angle  As Double
                Angle = Gon
                if (not Double.IsNaN(Angle)) then
                    do While (Angle < 0)
                       Angle = Angle + 400
                    Loop
                    do While (Angle > 400)
                        Angle = Angle - 400
                    Loop
                end if
                Return Angle
            End Function
            
            ''' <summary> Convert octal String representation without prefix to decimal Long (i.e "20" => 16). </summary>
             ''' <param name="Octal"> String representation of a number that is to be interpreted as octal. </param>
             ''' <returns> Decimal number </returns>
            Public Shared Function Oct2Dec(ByVal Octal As String) As Nullable(Of Long)
                Dim OneChar As String = String.Empty
                Dim Dec     As Nullable(Of Long) = 0
                Try
                    While (Not String.IsNullOrEmpty(Octal))
                        OneChar = Octal.Substring(0, 1)
                        Dec = Dec * 8 + OneChar
                        Octal = Octal.Substring(1)
                    End While
                Catch ex As System.Exception
                    Dec = Nothing
                End Try
                Return Dec
            End Function
            
            ''' <summary> Convert hexadecimal String representation without prefix to decimal Long (i.e "FF" => 255). </summary>
             ''' <param name="Hexadecimal"> String representation of a number that is to be interpreted as hex. </param>
             ''' <returns> Decimal number </returns>
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
            
            ''' <summary> Formats a numeric Kilometer value to a usual DBAG Kilometer notation (i.e 123456.789 => "123.4+56.789") </summary>
             ''' <param name="Kilometer">   Value to format (in [m]) </param>
             ''' <param name="Precision">   Desired output precison </param>
             ''' <param name="PrefixMeter"> Prefix for Meters if &lt; 10 m  (useful are only "", " ", "0") </param>
             ''' <returns> Usual Kilometer String </returns>
             ''' <remarks> Example output: "12.3 + 05.67", "12.3 + 5.67", "12.3 +  5.67" </remarks>
            Public Shared Function formatKilometer(byVal Kilometer As Double, byVal Precision As Integer, byVal PrefixMeter As String) As String
                dim HektoMeter As Double
                dim Meter      As Double
                dim Part1      As String
                dim Part2      As String
                
                ' Format without blanks
                HektoMeter = System.Math.Truncate(Kilometer / 100)
                Meter  = Kilometer - HektoMeter * 100
                
                Part1 = StringUtils.sprintf("%.1f", HektoMeter/10)
                Part2 = StringUtils.sprintf("%+." & Precision & "f", Meter)
                
                ' Tuning part 2
                if (Kilometer >= 0) then
                    if ((Meter < 10) and (PrefixMeter <> "")) then Part2 = replace(Part2, "+", "+" & PrefixMeter)
                    Part2 = replace(Part2, "+", " + ")
                else
                    if ((Meter > -10) and (PrefixMeter <> "")) then Part2 = replace(Part2, "-", "-" & PrefixMeter)
                    Part2 = replace(Part2, "-", " - ")
                end if
                
                Return Part1 & Part2
            End Function
            
            ''' <summary> Converts a usual DBAG Kilometer notation into the corresponding numerical value (i.e "123.4+56.789" => 123456.789). </summary>
             ''' <param name="KilometerString"> A usual DBAG Kilometer notation or a numerical String. </param>
             ''' <returns> The corresponding numerical value if possible or NULL. </returns>
            Public Shared Function getKilometer(ByVal KilometerString As String) As Nullable(Of Double)
                Dim Kilometer     As String
                Dim Meter         As String
                Dim MiddleSign    As String
                Dim Pattern       As String
                Dim SignKm        As Integer
                Dim SignM         As Integer
                Dim SignTotal     As Integer
                Dim KmReal        As Nullable(Of Double) = Nothing
                Dim oMatch        As Match
                
                Pattern = "^ *([+\-]? *[0-9]*[.]*[0-9]+)([-+ ]+)([0-9]*[.]*[0-9]+) *$"
                oMatch = Regex.Match(KilometerString, Pattern, RegexOptions.IgnoreCase)
                
                If (Not oMatch.Success) Then
                    ' No valid Kilometer notation => maybe numeric?
                    If (IsNumeric(KilometerString)) Then KmReal = CDbl(KilometerString)
                Else
                    Kilometer = oMatch.Groups(1).Value
                    MiddleSign = oMatch.Groups(2).Value
                    Meter = oMatch.Groups(3).Value   ' without Sign
                    Kilometer = Replace(Kilometer, " ", "")
                    SignKm = System.Math.Sign(Kilometer.ConvertTo(Of Double))
                    If (InStr(MiddleSign, "-") > 0) Then SignM = -1 Else SignM = 1
                    If ((SignM = -1) Or (SignKm = -1)) Then SignTotal = -1 Else SignTotal = 1
                    KmReal = SignTotal * (System.Math.Abs(Kilometer.ConvertTo(Of Double)) * 1000 + Meter)
                End If
                
                Return KmReal
            End Function
            
            ''' <summary> Gets Cant value from a Pointinfo string. </summary>
             ''' <param name="Pointinfo">     [Input and Output] The Pointinfo string to parse. </param>
             ''' <param name="Cant">          [Output] parsed Cant value. </param>
             ''' <param name="strict">        If True, only the pattern "u=..." is recognized. Otherwise, if this pattern isn't found, the first number is used. </param>
             ''' <param name="absolute">      If True, the returned Cant value is always positive. </param>
             ''' <param name="editPointInfo"> If True, the Cant pattern substring is removed from Pointinfo. </param>
             ''' <returns> The found Cant value or NULL </returns>
             ''' <remarks> 
             ''' <para>
             ''' Es wird versucht, der Punktinfo die gemessene Ist-Überhöhung nach folgenden Regeln zu entnehmen:
             ''' </para>
             ''' <para>
             ''' Falls die Zeichenkette "u= xxx" (an irgendeiner Stelle) enthalten
             ''' ist, so wird "xxx" als Ist-Überhöhung angesehen.
             ''' </para>
             ''' <para>
             ''' Falls Variante 1 nicht zum Erfolg führt und in den Einstellungen
             ''' nicht nur die strenge Variante erlaubt ist, wird
             ''' die erste Zahl als Ist-Überhöhung verwendet.
             ''' </para>
             ''' </remarks>
            Public Shared Function getCantFromPointinfo(byRef Pointinfo As String, _
                                                        byRef Cant As Nullable(Of Double), _
                                                        Optional byVal strict As Boolean = true, _
                                                        Optional byVal absolute As Boolean = false, _
                                                        Optional byVal editPointInfo As Boolean = false) As Boolean
                Dim Success       As Boolean
                Dim ui            As String = String.Empty
                Dim Pattern       As String
                Dim oMatch        As Match
                
                Pointinfo = Pointinfo.Trim()
                
                ' 1. search for "u=..."
                Pattern = "u *= *([-|+]? *[0-9]+)"
                oMatch = Regex.Match(Pointinfo, Pattern, RegexOptions.IgnoreCase)
                
                If (oMatch.Success) Then
                    ui = oMatch.Groups(1).Value
                    ui = ui.Replace(" ", String.Empty)
                End If
                
                If (ui.IsEmptyOrWhiteSpace() and (not strict)) Then
                    ' 2. "u=..." not found, look for first number
                    Pattern = "[-|+]? *[0-9]+"
                    oMatch = Regex.Match(Pointinfo, Pattern, RegexOptions.IgnoreCase)
                    If (oMatch.Success) Then
                      ui = oMatch.Value
                      ui = ui.Replace(" ", String.Empty)
                    End If
                End If
                
                if (ui.IsNotEmpty()) then
                    ' 3. Cant value found.
                    Cant = cdbl(ui)
                    if (absolute) then Cant = System.Math.Abs(Cant.ConvertTo(Of Double))
                    if (editPointInfo) then Pointinfo = Pointinfo.replace(oMatch.Groups(1).Value, String.Empty)
                    Success = true
                else
                    Success = false
                    Cant = Nothing
                end if
                
                Return Success
            End Function
            
            ''' <summary> Determines the title of a DBAG rail line according to the line number. </summary>
             ''' <param name="LineNo"> DBAG rail line no. </param>
             ''' <returns> A <see cref="DBAGLineInfo"/> record. </returns>
             ''' <remarks> 
             ''' <para>
             ''' The line titles are read from a file with format "number Line title here"
             ''' </para>
             ''' <para>
             ''' The file path is specified by application settings "GeoMath_DBAGLinesFile" or "GeoMath_DBAGLinesFileFallback"
             ''' which may contain environment variables like %variable%. If the first one doesn't determine an existing file
             ''' the second is tried.
             ''' </para>
             ''' </remarks>
            Public Shared Function getDBAGLineTitle(byVal LineNo As Integer) As DBAGLineInfo
              ' Declarations:
                Dim LineFromFile     As String = String.Empty
                dim LineTextShort    As String = String.Empty
                dim FirstString      As String = String.Empty
                dim WorkLine         As String = String.Empty
                dim FromTo()         As String
                dim tmp()            As String
                Dim NR               As Long    = 0
                dim LineNoFound      As Boolean = false
                Dim oSR              As StreamReader = Nothing
                Dim LineInfo         As New DBAGLineInfo With {.Number = LineNo}
                
              ' Path of File with DB lines list
                LineInfo.SourceFile = Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGLinesFile)
                If (not File.Exists(LineInfo.SourceFile)) Then
                    LineInfo.SourceFile = Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGLinesFileFallback)
                End If 
                
              ' Process LineInfo.SourceFile
                if (not File.Exists(LineInfo.SourceFile)) then
                    Logger.logError(StringUtils.sprintf("getDBAGLineTitle(): Streckenliste nicht gefunden: '%s' ('%s')!", My.Settings.GeoMath_DBAGLinesFile, Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGLinesFile)))
                    Logger.logError(StringUtils.sprintf("getDBAGLineTitle(): Streckenliste nicht gefunden: '%s' ('%s')!", My.Settings.GeoMath_DBAGLinesFileFallback, Environment.ExpandEnvironmentVariables(My.Settings.GeoMath_DBAGLinesFileFallback)))
                    
                    LineInfo.LongTitle  = "Streckenliste '" & LineInfo.SourceFile & "' nicht gefunden!"
                    LineInfo.ShortTitle = LineInfo.LongTitle
                    LineInfo.ShortDescription = LineInfo.LongTitle
                    LineInfo.LongDescription  = LineInfo.LongTitle
                else
                  ' Open File in default ANSII encoding ("Windows-1252" alias "iso-8859-1") and read until LineNo found.
                    'oSR = New StreamReader(LineInfo.SourceFile, System.Text.Encoding.GetEncoding("Windows-1252"))
                    oSR = New StreamReader(LineInfo.SourceFile, System.Text.Encoding.Default)
                    Do while (not (oSR.EndOfStream or LineNoFound))
                        WorkLine = oSR.ReadLine
                        NR = NR + 1
                        if (WorkLine.IsNotEmptyOrWhiteSpace) then
                            FirstString = WorkLine.Trim().Left(4)
                            if (FirstString = LineNo) then 
                                LineNoFound = true
                                LineInfo.SourceLine = NR
                            End If
                        end If
                    Loop
                    if (LineNoFound) then LineFromFile = WorkLine.Trim()
                    oSR.Close
                  
                  ' Determine output text.
                    If (LineFromFile.IsEmptyOrWhiteSpace) Then
                        LineInfo.LongTitle = "Strecke '" & LineNo & "' nicht gefunden! "
                        LineInfo.ShortTitle = LineInfo.LongTitle
                        LineInfo.ShortDescription = LineInfo.LongTitle
                        LineInfo.LongDescription  = LineInfo.LongTitle
                    else
                        LineInfo.LongTitle = LineFromFile.Substring(4).Trim()
                        LineInfo.LongDescription = "Strecke " & LineNo & "  " & LineInfo.LongTitle
                        
                        ' Short Line title
                        FromTo = LineInfo.LongTitle.Split(" - ")
                        tmp    = FromTo(0).Split(",")
                        LineTextShort = tmp(0)
                        
                        if (ubound(FromTo) > 0) then
                            for i As Integer = 1 to ubound(FromTo)
                                tmp = FromTo(i).Split(",")
                                LineTextShort = LineTextShort & " - " & tmp(0)
                            next
                        end if
                        LineInfo.ShortTitle = LineTextShort
                        LineInfo.ShortDescription = "Strecke " & LineNo & "  " & LineInfo.ShortTitle
                        LineInfo.isValid = True
                    end if
                end if
                
                Return LineInfo
            End Function
            
        #End Region
        
        #Region "Data Structures, Nested Classes"
            
            ''' <summary> DBAG rail line info record </summary>
            Public Class DBAGLineInfo
                
                ''' <summary> Rail line number </summary>
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
        
        #Region "Private Members"
            
        #End Region
        
    End Class
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
