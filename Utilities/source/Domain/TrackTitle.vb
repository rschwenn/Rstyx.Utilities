
Imports System
Imports System.IO

Namespace Domain
    
    ''' <summary> Rail track title record. </summary>
    Public Class TrackTitle
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.Domain.TrackTitle")
            
        #End Region
        
        #Region "Constructor"
            
            ''' <summary> Creates a new, empty instance. </summary>
            Public Sub New
            End Sub
            
        #End Region
        
        #Region "Public Fields"
            
            ''' <summary> Rail track number </summary>
            Public Number            As Nullable(Of Integer) = Nothing
            
            ''' <summary> <see langword="true"/>, if the track info is determined successfully. </summary>
            Public IsValid           As Boolean = false
            
            ''' <summary> Composited string like "1234  {short title}" </summary>
            Public ShortDescription  As String  = String.Empty
            
            ''' <summary> Composited string like "1234  {long title}" </summary>
            Public LongDescription   As String  = String.Empty
            
            ''' <summary> Shortened Track title (begin and end strings truncated at comma). </summary>
            Public ShortTitle        As String  = String.Empty
            
            ''' <summary> Track title as read from source file. </summary>
            Public LongTitle         As String  = String.Empty
            
        #End Region
        
        #Region "Static Methods"
            
            ''' <summary> Determines the title of a DBAG rail track according to the track number. </summary>
             ''' <param name="TrackNo"> DBAG rail track number. </param>
             ''' <returns> A <see cref="TrackTitle"/> record. </returns>
             ''' <remarks> 
             ''' <para>
             ''' The track title will be read from a file with format "number [track title]"
             ''' </para>
             ''' <para>
             ''' The file path is specified by application settings "TrackTitle_DBAGTracksFile" or "TrackTitle_DBAGTrackFileFallback"
             ''' which may contain environment variables like %variable%. If the first one doesn't determine an existing file
             ''' the second is tried.
             ''' </para>
             ''' </remarks>
            Public Shared Function GetDBAGTrackTitle(byVal TrackNo As Integer) As TrackTitle
              ' Declarations:
                Dim SourceFilePath   As String = String.Empty
                Dim LineFromFile     As String = String.Empty
                Dim LineTextShort    As String = String.Empty
                Dim FirstString      As String = String.Empty
                Dim WorkLine         As String = String.Empty
                Dim FromTo()         As String
                Dim Tmp()            As String
                Dim NR               As Long    = 0
                Dim TrackNoFound     As Boolean = false
                Dim oTrackTitle      As New TrackTitle() With {.Number = TrackNo}
                
              ' Path of File with DB lines list
                SourceFilePath = Environment.ExpandEnvironmentVariables(My.Settings.TrackTitle_DBAGTracksFile)
                If (not File.Exists(SourceFilePath)) Then
                    SourceFilePath = Environment.ExpandEnvironmentVariables(My.Settings.TrackTitle_DBAGTrackFileFallback)
                End If 
                
              ' Process SourceFilePath
                If (Not File.Exists(SourceFilePath)) Then
                    Logger.LogError(StringUtils.Sprintf("getDBAGTrackTitle(): Streckenliste nicht gefunden: '%s' ('%s')!", My.Settings.TrackTitle_DBAGTracksFile, Environment.ExpandEnvironmentVariables(My.Settings.TrackTitle_DBAGTracksFile)))
                    Logger.LogError(StringUtils.Sprintf("getDBAGTrackTitle(): Streckenliste nicht gefunden: '%s' ('%s')!", My.Settings.TrackTitle_DBAGTrackFileFallback, Environment.ExpandEnvironmentVariables(My.Settings.TrackTitle_DBAGTrackFileFallback)))
                    
                    oTrackTitle.LongTitle  = "Streckenliste '" & SourceFilePath & "' nicht gefunden!"
                    oTrackTitle.ShortTitle = oTrackTitle.LongTitle
                    oTrackTitle.ShortDescription = oTrackTitle.LongTitle
                    oTrackTitle.LongDescription  = oTrackTitle.LongTitle
                Else
                    'Using oSR As New System.IO.StreamReader(Path, Encoding, DetectEncodingFromByteOrderMarks, BufferSize)
                    Using oSR As New StreamReader(SourceFilePath, System.Text.Encoding.Default)
                        Do While (Not (oSR.EndOfStream Or TrackNoFound))
                            WorkLine = oSR.ReadLine()
                            NR += 1
                            If (WorkLine.IsNotEmptyOrWhiteSpace()) Then
                                FirstString = WorkLine.Trim().Left(4)
                                If (CDbl(FirstString) = TrackNo) Then
                                    TrackNoFound = true
                                End If
                            End If
                        Loop
                        If (TrackNoFound) Then LineFromFile = WorkLine.Trim()
                    End Using
                    
                    ' Determine output text.
                    If (LineFromFile.IsEmptyOrWhiteSpace()) Then
                        oTrackTitle.LongTitle = "Strecke '" & TrackNo & "' nicht gefunden! "
                        oTrackTitle.ShortTitle = oTrackTitle.LongTitle
                        oTrackTitle.ShortDescription = oTrackTitle.LongTitle
                        oTrackTitle.LongDescription  = oTrackTitle.LongTitle
                    Else
                        oTrackTitle.LongTitle = LineFromFile.Substring(4).Trim()
                        oTrackTitle.LongDescription = "Strecke " & TrackNo & "  " & oTrackTitle.LongTitle
                        
                        ' Short Track title
                        FromTo = oTrackTitle.LongTitle.Split(" - ")
                        tmp    = FromTo(0).Split(",")
                        LineTextShort = Tmp(0)
                        
                        If (ubound(FromTo) > 0) Then
                            For i As Integer = 1 to Ubound(FromTo)
                                tmp = FromTo(i).Split(",")
                                LineTextShort = LineTextShort & " - " & Tmp(0)
                            Next
                        End If
                        oTrackTitle.ShortTitle = LineTextShort
                        oTrackTitle.ShortDescription = "Strecke " & TrackNo & "  " & oTrackTitle.ShortTitle
                        oTrackTitle.IsValid = True
                    End If
                End If
                
                Return oTrackTitle
            End Function
                
             ''' <inheritdoc/>
            Public Overrides Function ToString() As String
                Return Rstyx.Utilities.StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.TrackTitle_ToString, Me.Number)
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
