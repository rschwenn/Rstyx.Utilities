'Option Compare Text

Imports System
Imports System.IO
Imports System.Linq
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

'Imports Rstyx.Utilities.Files

Namespace IO
    
    ''' <summary> Static utility methods for dealing with the file system and file or path names. </summary>
    Public NotInheritable Class FileUtils
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(MyClass.GetType.FullName)
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Files.FileUtils")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Methods - File Search"
            
            ''' <summary> Returns the full path of the file that matches one of a given file filters and is found first in the Folders list. </summary>
             ''' <param name="FileFilters">    File filters without path (wildcards allowed), delimited by a given delimiter. </param>
             ''' <param name="Folders">        Folders that should be searched. Absolute or relative (but not "..\" or ".\"), delimited by a given delimiter. Embedded Environment variables (quoted by "%") are expanded! </param>
             ''' <param name="DelimiterRegEx"> The Delimiter for both the FileFilter and Folder lists (Regular Expression). Defaults to ";" (if it's Null). </param>
             ''' <param name="SearchOptions">  Available System.IO.SearchOptions (mainly recursive or not). </param>
             ''' <returns>                     The full path of the found file or Null. </returns>
            Public Shared Function findFile(FileFilters    As String,
                                            Folders        As String,
                                            DelimiterRegEx As String,
                                            SearchOptions  As System.IO.SearchOption
                                            ) As FileInfo
                Logger.logDebug(StringUtils.sprintf("findFile(): FileFilter: '%s', Verzeichnisse: '%s'.", FileFilters, Folders))
                Dim foundFile   As FileInfo = Nothing
                
                Dim FoundFiles  As FileInfoCollection = findFiles(FileFilters, Folders, DelimiterRegEx, SearchOptions, OnlyOneFile:=True)
                
                ' Result
                If (FoundFiles.Count < 1) Then
                    Logger.logDebug("findFile(): keine Datei gefunden!")
                Else
                    foundFile = FoundFiles(0)
                    Logger.logDebug(StringUtils.sprintf("findFile(): Datei gefunden: '%s'.", foundFile.FullName))
                End If
                
                Return foundFile
            End Function
            
            ''' <summary> Searches for files in a list of directories, matching a list of file filters. </summary>
             ''' <param name="FileFilters">    File filters without path (wildcards allowed), delimited by a given delimiter. </param>
             ''' <param name="Folders">        Folders that should be searched. Absolute or relative (but not "..\" or ".\"), delimited by a given delimiter. Embedded Environment variables (quoted by "%") are expanded! </param>
             ''' <param name="DelimiterRegEx"> The Delimiter for both the FileFilter and Folder lists (Regular Expression). Defaults to ";" (if it's Null). </param>
             ''' <param name="SearchOptions">  Available System.IO.SearchOptions (mainly recursive or not). </param>
             ''' <param name="OnlyOneFile">    If True, the search is canceled when the first file is found. Defaults to False. </param>
             ''' <returns>                     The resulting list with found files. </returns>
            Public Shared Function findFiles(FileFilters    As String,
                                             Folders        As String,
                                             DelimiterRegEx As String,
                                             SearchOptions  As System.IO.SearchOption,
                                             Optional OnlyOneFile As Boolean = false
                                            ) As FileInfoCollection
                Dim FoundFiles  As New FileInfoCollection
                
                If (FileFilters.IsNull()) Then
                    Logger.logWarning("findFiles(): Keine Dateimaske(n) angegeben!")
                ElseIf (Folders.IsNull()) Then
                    Logger.logWarning("findFiles(): Keine zu durchsuchenden Verzeichnisse angegeben!")
                Else
                    If (DelimiterRegEx.IsNull()) Then DelimiterRegEx = ";"
                    FoundFiles = findFiles(FileFilters.Split(DelimiterRegEx), Folders.Split(DelimiterRegEx), SearchOptions, OnlyOneFile)
                End If
                
                Return FoundFiles
            End Function
            
            ''' <summary> Searches for files in a list of directories, matching a list of file filters. </summary>
             ''' <param name="FileFilters">   File filters without path (wildcards allowed). </param>
             ''' <param name="Folders">       Folders that should be searched. Absolute or relative (but not "..\" or ".\"). Embedded Environment variables (quoted by "%") are expanded! </param>
             ''' <param name="SearchOptions"> Available System.IO.SearchOptions (mainly recursive or not). </param>
             ''' <param name="OnlyOneFile">   If True, the search is canceled when the first file is found. Defaults to False. </param>
             ''' <returns>                    The resulting list with found files. </returns>
            Public Shared Function findFiles(FileFilters    As IEnumerable(Of String), 
                                             Folders        As IEnumerable(Of DirectoryInfo), 
                                             SearchOptions  As System.IO.SearchOption,
                                             Optional OnlyOneFile As Boolean = false
                                             ) As FileInfoCollection
                Dim FoundFiles  As New FileInfoCollection
                
                If (FileFilters.IsNull()) Then
                    Logger.logWarning("findFiles(): Keine Dateimaske(n) angegeben!")
                ElseIf (Folders.IsNull()) Then
                    Logger.logWarning("findFiles(): Keine zu durchsuchenden Verzeichnisse angegeben!")
                Else
                    ' Unify list of folders.
                    Dim StringFolders As New Collection(Of String)
                    For Each di As DirectoryInfo In Folders
                        StringFolders.Add(di.FullName)
                    Next
                    
                    FoundFiles = findFiles(FileFilters, StringFolders, SearchOptions, OnlyOneFile)
                End If
                
                Return FoundFiles
            End Function
            
            ''' <summary> Searches for files in a list of directories, matching a list of file filters. </summary>
             ''' <param name="FileFilters">   File filters without path (wildcards allowed). </param>
             ''' <param name="Folders">       Folders that should be searched. Absolute or relative (but not "..\" or ".\"). Embedded Environment variables (quoted by "%") are expanded! </param>
             ''' <param name="SearchOptions"> Available System.IO.SearchOptions (mainly recursive or not). </param>
             ''' <param name="OnlyOneFile">   If True, the search is canceled when the first file is found. Defaults to False. </param>
             ''' <returns>                    The resulting list with found files. </returns>
            Public Shared Function findFiles(FileFilters    As IEnumerable(Of String), 
                                             Folders        As IEnumerable(Of String), 
                                             SearchOptions  As System.IO.SearchOption,
                                             Optional OnlyOneFile As Boolean = false
                                             ) As FileInfoCollection
                Dim FoundFiles  As New FileInfoCollection
                
                If (FileFilters.IsNull()) Then
                    Logger.logWarning("findFiles(): Keine Dateimaske(n) angegeben!")
                ElseIf (Folders.IsNull()) Then
                    Logger.logWarning("findFiles(): Keine zu durchsuchenden Verzeichnisse angegeben!")
                Else
                    
                    Dim oStopwatch  As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
                    
                    Logger.logDebug("\nfindFiles(): Start search with these settings:")
                    Logger.logDebug(StringUtils.sprintf("findFiles(): - Only one file:     %s", OnlyOneFile))
                    Logger.logDebug(StringUtils.sprintf("findFiles(): - Options:           %s", SearchOptions.ToString()))
                    
                    Logger.logDebug(StringUtils.sprintf("findFiles(): - FileFilters (%d):", FileFilters.Count))
                    For Each FileFilter As String in FileFilters
                        Logger.logDebug(StringUtils.sprintf("findFiles():   - FileFilter:      %s", FileFilter))
                    Next
                    
                    Logger.logDebug(StringUtils.sprintf("findFiles(): - Folders (%d):", Folders.Count))
                    For Each Folder As String in Folders
                        Logger.logDebug(StringUtils.sprintf("findFiles():   - Folder:          %s", Folder))
                    Next
                    Logger.logDebug("")
                    findAddFiles(FoundFiles, FileFilters, Folders, SearchOptions, OnlyOneFile)
                    
                    oStopwatch.Stop()
                    Logger.logDebug(StringUtils.sprintf("\nfindFiles(): Found %d files (in %.3f sec.)\n", FoundFiles.Count, oStopwatch.ElapsedMilliseconds/1000))
                End If
                
                Return FoundFiles
            End Function
            
            ''' <summary> Backend of findFiles(): Searches for files in a list of directories, matching a list of file filters. </summary>
             ''' <param name="FoundFiles">    The Collection to add the found files to. If Null it will be created. </param>
             ''' <param name="FileFilters">   File filters without path (wildcards allowed). </param>
             ''' <param name="Folders">       Folders that should be searched. Absolute or relative (but not "..\" or ".\"). Embedded Environment variables (quoted by "%") are expanded! </param>
             ''' <param name="SearchOptions"> Available System.IO.SearchOptions (mainly recursive or not). </param>
             ''' <param name="OnlyOneFile">   If True, the search is canceled when the first file is found. Defaults to False. </param>
            Private Shared Sub findAddFiles(FoundFiles     As FileInfoCollection, 
                                            FileFilters    As IEnumerable(Of String), 
                                            Folders        As IEnumerable(Of String), 
                                            SearchOptions  As System.IO.SearchOption,
                                            Optional OnlyOneFile As Boolean = false
                                            )
                Dim Files()         As FileInfo
                Dim SearchFinished  As Boolean = False
                
                ' Ensure that the resulting collection is never returned as Null (for convenience).
                If (FoundFiles.IsNull()) Then FoundFiles = New FileInfoCollection
                
                If (FileFilters.IsNull()) Then
                    Logger.logWarning("findAddFiles(): Keine Dateifilter angegeben!")
                ElseIf (Folders.IsNull()) Then
                    Logger.logWarning("findAddFiles(): Keine zu durchsuchenden Verzeichnisse angegeben!")
                ElseIf (SearchOptions.IsNull()) Then
                    Logger.logWarning("findAddFiles(): Keine Suchoptionen angegeben!")
                Else
                    ' Consolidate file filters
                    Dim ConsolidatedFileFilters As New Collection(Of String)
                    For Each FileFilter As String in FileFilters
                        FileFilter = FileFilter.Trim()
                        
                        If (Not isValidFileNameFilter(FileFilter)) Then
                            Logger.logWarning("findAddFiles(): Dateifilter für Dateisuche enthält ungültige Zeichen und wird ignoriert: '" & FileFilter & "'.")
                        ElseIf (FileFilter.IsEmptyOrWhiteSpace) Then
                            Logger.logWarning("findAddFiles(): Dateifilter für Dateisuche ist leer und wird ignoriert.")
                        ElseIf (ConsolidatedFileFilters.Contains(FileFilter)) Then
                            Logger.logWarning("findAddFiles(): Dateifilter für Dateisuche ist wiederholt angegeben und wird ignoriert: '" & FileFilter & "'.")
                        Else
                            ConsolidatedFileFilters.Add(FileFilter)
                        End If
                    Next
                    
                    If (ConsolidatedFileFilters.Count < 1) Then
                        Logger.logWarning("findAddFiles(): Keine gültigen Dateifilter verfügbar!")
                    Else
                        ' Search each folder of given folder list.
                        For Each FolderName As String in Folders
                            
                            FolderName = Environment.ExpandEnvironmentVariables(FolderName.Trim())
                            
                            If (not Directory.Exists(FolderName)) Then
                                If (FolderName.IsEmptyOrWhiteSpace()) Then
                                    Logger.logDebug("findAddFiles(): Leeres Verzeichnis für Dateisuche => wird ignoriert.")
                                ElseIf (FolderName.IndexOfAny(System.IO.Path.GetInvalidPathChars()) > -1) Then
                                    Logger.logWarning("findAddFiles(): Verzeichnis für Dateisuche enthält ungültige Zeichen: '" & FolderName & "'.")
                                Else
                                    Logger.logWarning(StringUtils.sprintf("findAddFiles(): Verzeichnis für Dateisuche existiert nicht: '%s' (Arbeitsverzeichnis: '%s').", FolderName, Directory.GetCurrentDirectory))
                                End If 
                            else
                                Dim SearchDir As DirectoryInfo = New DirectoryInfo(FolderName)
                                Logger.logDebug(StringUtils.sprintf("\nfindAddFiles(): Suche in Verzeichnis: '%s'.", Path.GetFullPath(FolderName)))
                                
                                ' Search files of current folder.
                                For Each FileFilter As String in ConsolidatedFileFilters
                                    Try
                                        FileFilter = FileFilter.Trim()
                                        
                                        If (Not isValidFileNameFilter(FileFilter)) Then
                                            Logger.logWarning("findAddFiles(): Dateifilter für Dateisuche enthält ungültige Zeichen: '" & FileFilter & "'.")
                                        ElseIf (FileFilter.IsEmptyOrWhiteSpace) Then
                                            Logger.logWarning("findAddFiles(): Dateifilter für Dateisuche ist leer und wird ignoriert.")
                                        Else
                                            ' Get the matching files of this directory, possibly inclusive subdirectories.
                                            Files = SearchDir.GetFiles(FileFilter, System.IO.SearchOption.TopDirectoryOnly)
                                            
                                            For Each fi As FileInfo in Files
                                                If (Not FoundFiles.Contains(fi)) Then
                                                    FoundFiles.Add(fi)
                                                    Logger.logDebug(StringUtils.sprintf("findAddFiles(): %6d. Datei gefunden:   '%s'", FoundFiles.Count, FoundFiles(FoundFiles.Count - 1).FullName))
                                                    If (OnlyOneFile) Then
                                                        SearchFinished = True
                                                        Exit For
                                                    End If
                                                End If
                                            Next
                                        End If
                                        
                                    Catch ex as System.Security.SecurityException
                                        Logger.logDebug(StringUtils.sprintf("findAddFiles(): SecurityException at processing files of %s.", FolderName))
                                        
                                    Catch ex as System.UnauthorizedAccessException
                                        Logger.logDebug(StringUtils.sprintf("findAddFiles(): UnauthorizedAccessException at processing files of %s.", FolderName))
                                        
                                    Catch ex as System.Exception
                                        Logger.logError(ex, StringUtils.sprintf("findAddFiles(): Fehler bei Verarbeitung der Dateien des Ordners .", FolderName))
                                    End Try
                                    
                                    If (SearchFinished) Then Exit For
                                Next
                                
                                ' Optional: Search subfolders of current folder (*** recursive ***).
                                If (Not SearchFinished AndAlso (SearchOptions = System.IO.SearchOption.AllDirectories)) Then
                                    Try
                                        Dim SubFolders()  As DirectoryInfo = SearchDir.GetDirectories()
                                        If (SubFolders.Length > 0) Then
                                            ' Unify list of folders.
                                            Dim StringSubFolders As New Collection(Of String)
                                            For Each di As DirectoryInfo In SubFolders
                                                StringSubFolders.Add(di.FullName)
                                            Next
                                            
                                            findAddFiles(FoundFiles, ConsolidatedFileFilters, StringSubFolders, SearchOptions, OnlyOneFile)
                                        End If 
                                        
                                    Catch ex as System.Security.SecurityException
                                        Logger.logDebug(StringUtils.sprintf("findAddFiles(): SecurityException at processing subfolders of %s.", FolderName))
                                        
                                    Catch ex as System.UnauthorizedAccessException
                                        Logger.logDebug(StringUtils.sprintf("findAddFiles(): UnauthorizedAccessException at processing subfolders of %s.", FolderName))
                                        
                                    Catch ex as System.Exception
                                        Logger.logError(ex, StringUtils.sprintf("findAddFiles(): Fehler bei Verarbeitung der Unterverzeichnisse des Ordners .", FolderName))
                                    End Try
                                End If
                                
                            End If
                            If (SearchFinished) Then Exit For
                        Next
                    End If
                End If
            End Sub
            
        #End Region
        
        #Region "Methods - File Name Handling"
            
            ''' <summary> Returns a desired part of a given file name string. </summary>
             ''' <param name="givenFilename">   File name, absolute or relative, file doesn't have to exist. </param>
             ''' <param name="desiredFilePart"> Determines the parts of the filename to return. </param>
             ''' <param name="Absolute">        If True, and a given file name is relative, it will be "rooted". If False, returned drive and directory may be empty. Defaults to True. </param>
             ''' <param name="ClassLength">     Number of characters to define the class name (at end of base name) - defaults to 2. </param>
             ''' <returns> A string with the desired parts of the input path name, or String.Empty. </returns>
             ''' <remarks>
             ''' <para>
             ''' I.e. for a given file name of "D:\daten\Test_QP\qp_ueb.dgn", the results are:
             ''' </para>
             ''' <para>
             ''' <list type="table">
             ''' <listheader> <term> <b>desiredFilePart</b> </term>  <description> Result </description></listheader>
             ''' <item> <term> FilePart.Drive        </term>  <description> D:                          </description></item>
             ''' <item> <term> FilePart.Dir          </term>  <description> D:\daten\Test_QP            </description></item>
             ''' <item> <term> FilePart.Base         </term>  <description> qp_ueb                      </description></item>
             ''' <item> <term> FilePart.Ext          </term>  <description> dgn                         </description></item>
             ''' <item> <term> FilePart.Proj         </term>  <description> qp_u                        </description></item>
             ''' <item> <term> FilePart.Class        </term>  <description> eb                          </description></item>
             ''' <item> <term> FilePart.Base_Ext     </term>  <description> qp_ueb.dgn                  </description></item>
             ''' <item> <term> FilePart.Dir_Base     </term>  <description> D:\daten\Test_QP\qp_ueb     </description></item>
             ''' <item> <term> FilePart.Dir_Base_Ext </term>  <description> D:\daten\Test_QP\qp_ueb.dgn </description></item>
             ''' <item> <term> FilePart.Dir_Proj     </term>  <description> D:\daten\Test_QP\qp_u       </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Shared Function getFilePart(byVal givenFilename As String, _
                                               byVal desiredFilePart As FilePart, _
                                               byVal Optional Absolute As Boolean = true, _
                                               byVal Optional ClassLength As Integer = 2) As String
                Dim extractedFilePart     As String = String.Empty
                Try
                    Dim msg                   As String = String.Empty
                    Dim Drive                 As String = String.Empty
                    Dim AbsolutePath          As String = String.Empty
                    Dim ParentFolder          As String = String.Empty
                    Dim FileName              As String = String.Empty
                    Dim BaseName              As String = String.Empty
                    Dim Extension             As String = String.Empty
                    Dim ProjectName           As String = String.Empty
                    Dim DgnClass              As String = String.Empty
                    Dim ProjectLength         As Integer
                    
                    Logger.logDebug(StringUtils.sprintf("getFilePart(): gesuchter Namensteil: %s.", desiredFilePart.ToDisplayString()))
                    Logger.logDebug(StringUtils.sprintf("getFilePart(): gegeben : %s.", givenFilename))
                    
                    If (givenFilename.IsNotEmptyOrWhiteSpace()) Then
                        
                      'Einzelteile normal
                        AbsolutePath = iif(not Absolute, givenFilename, System.IO.Path.GetFullPath(givenFilename))
                        
                        Drive        = System.IO.Path.GetPathRoot(AbsolutePath).TrimEnd(Path.DirectorySeparatorChar)
                        ParentFolder = System.IO.Path.GetDirectoryName(AbsolutePath)
                        If (ParentFolder Is Nothing) Then
                            ParentFolder = AbsolutePath
                        End If
                        ParentFolder = ParentFolder.TrimEnd(Path.DirectorySeparatorChar)
                        
                        FileName     = System.IO.Path.GetFileName(givenFilename)
                        BaseName     = System.IO.Path.GetFileNameWithoutExtension(givenFilename)
                        Extension    = System.IO.Path.GetExtension(givenFilename).TrimStart("."c)
                      
                      'Einzelteile iProjekt, iKlasse
                        if (ClassLength < 0) then ClassLength = 2
                        
                        If (BaseName.Length > (ProjectLength + ClassLength)) Then
                            ProjectLength = BaseName.Length - ClassLength
                            ProjectName   = BaseName.left(ProjectLength)
                            DgnClass      = BaseName.right(ClassLength)
                        End If
                      
                      'Debug-Message
                        msg = vbNewLine & _
                              "getFilePart(): AbsolutePath = " & AbsolutePath  & vbNewLine & _
                              "getFilePart(): Drive        = " & Drive         & vbNewLine & _
                              "getFilePart(): ParentFolder = " & ParentFolder  & vbNewLine & _
                              "getFilePart(): FileName     = " & FileName      & vbNewLine & _
                              "getFilePart(): BaseName     = " & BaseName      & vbNewLine & _
                              "getFilePart(): Extension    = " & Extension     & vbNewLine & _
                              "getFilePart(): ProjectName  = " & ProjectName   & vbNewLine & _
                              "getFilePart(): DgnClass     = " & DgnClass
                        'Logger.logDebug(msg)
                      
                      'Ergebnis zusammenstellen
                        Select Case desiredFilePart
                            Case FilePart.Drive:         extractedFilePart = Drive
                            Case FilePart.Dir:           extractedFilePart = ParentFolder
                            Case FilePart.Base:          extractedFilePart = BaseName
                            Case FilePart.Ext:           extractedFilePart = Extension
                            Case FilePart.Proj:          extractedFilePart = ProjectName
                            Case FilePart.Class:         extractedFilePart = DgnClass
                            Case FilePart.Base_Ext:      extractedFilePart = FileName
                            Case FilePart.Dir_Base:      extractedFilePart = ParentFolder & Path.DirectorySeparatorChar & BaseName
                            Case FilePart.Dir_Base_Ext:  extractedFilePart = ParentFolder & Path.DirectorySeparatorChar & FileName
                            Case FilePart.Dir_Proj:      extractedFilePart = ParentFolder & Path.DirectorySeparatorChar & ProjectName
                            Case Else
                                extractedFilePart = ""
                                Logger.logError("getFilePart(): Programmfehler: Aufgerufen mit nicht unterstütztem FilePart-Enum-Wert: " & cStr(desiredFilePart))
                        End Select
                        
                    End If
                Catch ex As System.NotSupportedException
                    Logger.logDebug("getFilePart(): Der gegebene Pfad ist ungültig")
                    
                Catch ex As System.ArgumentException
                    Logger.logDebug("getFilePart(): Der gegebene Pfad ist ungültig")
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "getFilePart(): unbekannter Fehler")
                End Try
                
                Logger.logDebug(StringUtils.sprintf("getFilePart(): Ergebnis: %s.", extractedFilePart))
                
                Return extractedFilePart
            End Function
            
            ''' <summary> Converts a file path that may contain wildcards to a Regular expression pattern. </summary>
             ''' <param name="FilePath"> Regular Windows file path (may contain wildcards). </param>
             ''' <returns> The Regular expression pattern which corresponds to the file path. </returns>
            Public Shared Function FilePath2RegExp(ByVal FilePath As String) As String
                Dim Pattern  As String
                Pattern = FilePath.ReplaceWith("\\", "\\")
                Pattern = Pattern.ReplaceWith("[.(){}[\]$^]", "\$&")
                Pattern = Pattern.ReplaceWith("\?", ".")
                Pattern = Pattern.ReplaceWith("\*", ".*")
                Return Pattern
            End Function
            
            ''' <summary> Checks if a given file name is valid for file system operations. It doesn't have to exist. </summary>
             ''' <param name="FileName"> The file name to validate (without path). </param>
             ''' <returns> True, if file system or file name operations shouldn't complain about this name, otherwise False. </returns>
            Public Shared Function isValidFileName(byVal FileName As String) As Boolean
                Dim isValid  As Boolean = True
                If (Not (FileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0)) Then
                    ' FileName contains invalid characters.
                    isValid = False
                Else
                    Try 
                        Dim AbsolutePath As String = Path.GetFullPath(FileName)
                    Catch ex As System.Exception
                        ' FileName is invalid for other reasons.
                        isValid = False
                    End Try
                End If
                Return isValid
            End Function
            
            ''' <summary> Checks if a given file name filter seems to be valid. </summary>
             ''' <param name="FileNameFilter"> The file name filter to validate (without path). </param>
             ''' <returns> True, if file name filter operations shouldn't complain about this name filter, otherwise False. </returns>
            Public Shared Function isValidFileNameFilter(byVal FileNameFilter As String) As Boolean
                Dim isValid  As Boolean = True
                
                For Each TestChar As Char In System.IO.Path.GetInvalidFileNameChars()
                    If (Not ((TestChar = "*") Or (TestChar = "?"))) Then
                        If (FileNameFilter.Contains(TestChar)) Then
                            isValid = False
                        End If
                    End If
                Next 
                
                Return isValid
            End Function
            
            ''' <summary> Checks if a given file path is valid for file system operations. It doesn't have to exist. </summary>
             ''' <param name="FilePath"> The file path to validate (absolute or relative). </param>
             ''' <returns> True, if file system or path name operations shouldn't complain about this path, otherwise False. </returns>
            Public Shared Function isValidFilePath(byVal FilePath As String) As Boolean
                Dim isValid  As Boolean = True
                If (Not (FilePath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) < 0)) Then
                    ' FilePath contains invalid characters.
                    isValid = False
                Else
                    Try 
                        Dim AbsolutePath As String = Path.GetFullPath(FilePath)
                    Catch ex As System.Exception
                        ' FilePath is invalid for other reasons.
                        isValid = False
                    End Try
                End If
                Return isValid
            End Function
            
            ''' <summary> Replaces invalid characters in a given file name string. </summary>
             ''' <param name="FileName"> The file name without path. </param>
             ''' <param name="ReplaceString"> [Optional] The replace string for invalid characters (defaults to "_"). </param>
             ''' <returns> A given file name with invalid characters replaced by the ReplaceString. </returns>
            Public Shared Function validateFileNameSpelling(byVal FileName As String, Optional byVal ReplaceString As String = "_") As String
                Dim invalidString  As String
                For Each invalidChar As Char In System.IO.Path.GetInvalidFileNameChars()
                    invalidString = Char.Parse(invalidChar)
                    If (invalidString.Length > 0) Then
                        FileName = FileName.Replace(invalidString, ReplaceString)
                    End If
                Next
                Return FileName
            End Function
            
            ''' <summary> Replaces invalid characters in a given file path string. </summary>
             ''' <param name="FilePath"> The file path to validate (absolute or relative). </param>
             ''' <param name="ReplaceString"> [Optional] The replace string for invalid characters (defaults to "_"). </param>
             ''' <returns> A given file path with invalid characters replaced by the ReplaceString. </returns>
            Public Shared Function validateFilePathSpelling(byVal FilePath As String, Optional byVal ReplaceString As String = "_") As String
                Dim invalidString  As String
                For Each invalidChar As Char In System.IO.Path.GetInvalidPathChars()
                    invalidString = Char.Parse(invalidChar)
                    If (invalidString.Length > 0) Then
                        FilePath = FilePath.Replace(invalidString, ReplaceString)
                    End If
                Next
                Return FilePath
            End Function
            
        #End Region
        
        #Region "Enums"
            
            ''' <summary> Determines a File path partial string supported by <see cref="getFilePart" />. </summary>
            Public Enum FilePart As Byte
                ''' <summary> Drive letter, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "D:" </summary>
                Drive        = 0   ' "D:"
                
                ''' <summary> Directory, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "D:\daten\Test_QP" </summary>
                Dir          = 1   ' "D:\daten\Test_QP"
                
                ''' <summary> File base name, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "qp_ueb" </summary>
                Base         = 2   ' "qp_ueb"
                
                ''' <summary> File extension, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "dgn" </summary>
                Ext          = 3   ' "dgn"
                
                ''' <summary> DGN project name, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "qp_u" </summary>
                Proj         = 4   ' "qp_u"
                
                ''' <summary> DGN class, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "eb" </summary>
                [Class]      = 5   ' "eb"
                
                ''' <summary> File base name and extension, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "qp_ueb.dgn" </summary>
                Base_Ext     = 6   ' "qp_ueb.dgn"
                
                ''' <summary> Directory and File base name, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "D:\daten\Test_QP\qp_ueb" </summary>
                Dir_Base     = 7   ' "D:\daten\Test_QP\qp_ueb"
                
                ''' <summary> Full absolute path, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "D:\daten\Test_QP\qp_ueb.dgn" </summary>
                Dir_Base_Ext = 8   ' "D:\daten\Test_QP\qp_ueb.dgn"
                
                ''' <summary> Directory and DGN project name, i.e.: "D:\daten\Test_QP\qp_ueb.dgn" =&gt; "D:\daten\Test_QP\qp_u" </summary>
                Dir_Proj     = 9   ' "D:\daten\Test_QP\qp_u"
            End Enum
            
        #End Region
        
    End Class
        
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
