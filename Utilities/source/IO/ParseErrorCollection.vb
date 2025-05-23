﻿
Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Namespace IO
    
    ''' <summary> A collection of <see cref="Rstyx.Utilities.IO.ParseError"/> objects. </summary>
     ''' <remarks> 
     ''' <para>
     ''' There are methods to show the errors in jEdit and to log them to LoggingConsole.
     ''' </para>
     ''' <para>
     ''' This collection should be thread-safe.
     ''' </para>
     ''' </remarks>
    Public Class ParseErrorCollection
        Inherits System.Collections.ObjectModel.Collection(Of Rstyx.Utilities.IO.ParseError)
        
        #Region "Private Fields"
            
            Private ReadOnly Logger         As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.IO.ParseErrorCollection")
            
            Protected ReadOnly SyncHandle   As New Object()

            Protected _HasErrors            As Boolean
            Protected _HasWarnings          As Boolean
            Protected _HasItemsWithSource   As Boolean
            
            Protected _ErrorCount           As Long
            Protected _WarningCount         As Long
            
            Protected IndexOfLineNo         As New Dictionary(Of Long, Integer)
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Creates a new, empty ParseErrorCollection. </summary>
            Public Sub New
                MyBase.New()
            End Sub
            
            ''' <summary> Creates a new, empty ParseErrorCollection related to <paramref name="FilePath"/>. </summary>
            Public Sub New(FilePath As String)
                MyBase.New()
                Me.FilePath = FilePath
            End Sub
            
        #End Region
        
        #Region "Collection Implementation"

            ''' <summary> Adds a given error to the collection only if there is no error yet, hence <see cref="ErrorCount"/> is zero. </summary>
             ''' <param name="Item"> The error to add. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Item"/> is <see langword="null"/>. </exception>
             ''' <returns> <see langword="true"/>, if the <paramref name="Item"/> has been added, otherwise <see langword="false"/>. </returns>
             ''' <remarks>
             ''' Using this method, two threads can avoid adding errors at the same time, though only one of the errors should be added.
             ''' </remarks>
            Public Function AddIfNoError(Item As ParseError) As Boolean
                SyncLock (SyncHandle)
                    Dim RetValue As Boolean = False
                    If (_ErrorCount = 0) Then
                        Me.Add(Item)
                        RetValue = True
                    End If
                    Return RetValue
                End SyncLock
            End Function
            
            ''' <summary> Adds a new error without source information to the collection. </summary>
             ''' <param name="Message">  The error Message. </param>
             ''' <param name="Hints">    Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath"> Full path of the source file. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
            Public Sub AddError(Message     As String,
                                Optional Hints    As String = Nothing,
                                Optional FilePath As String = Nothing
                               )
                Me.Add(New ParseError(ParseErrorLevel.[Error], Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Adds a new warning without source information to the collection. </summary>
             ''' <param name="Message">  The error Message. </param>
             ''' <param name="Hints">    Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath"> Full path of the source file. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
            Public Sub AddWarning(Message     As String,
                                  Optional Hints    As String = Nothing,
                                  Optional FilePath As String = Nothing
                                 )
                Me.Add(New ParseError(ParseErrorLevel.Warning, Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Adds a new error with source information to the collection. </summary>
             ''' <param name="LineNo">      The line number in the source file, starting at 1. </param>
             ''' <param name="StartColumn"> The colulmn number in the source line determining the start of faulty string. </param>
             ''' <param name="EndColumn">   The colulmn number in the source line determining the end of faulty string. </param>
             ''' <param name="Message">     The error Message. </param>
             ''' <param name="Hints">       Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath">    Full path of the source file. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="LineNo"/> is less than 1. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="StartColumn"/> or <paramref name="EndColumn"/> is less than Zero. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="EndColumn"/> is not greater than <paramref name="StartColumn"/>. </exception>
            Public Sub AddError(LineNo      As Long,
                                StartColumn As Long,
                                EndColumn   As Long,
                                Message     As String,
                                Optional Hints    As String = Nothing,
                                Optional FilePath As String = Nothing
                               )
                Me.Add(New ParseError(ParseErrorLevel.[Error], LineNo, StartColumn, EndColumn, Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Adds a new warning with source information to the collection. </summary>
             ''' <param name="LineNo">      The line number in the source file, starting at 1. </param>
             ''' <param name="StartColumn"> The colulmn number in the source line determining the start of faulty string. </param>
             ''' <param name="EndColumn">   The colulmn number in the source line determining the end of faulty string. </param>
             ''' <param name="Message">     The error Message. </param>
             ''' <param name="Hints">       Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath">    Full path of the source file. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="LineNo"/> is less than 1. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="StartColumn"/> or <paramref name="EndColumn"/> is less than Zero. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="EndColumn"/> is not greater than <paramref name="StartColumn"/>. </exception>
            Public Sub AddWarning(LineNo      As Long,
                                  StartColumn As Long,
                                  EndColumn   As Long,
                                  Message     As String,
                                  Optional Hints    As String = Nothing,
                                  Optional FilePath As String = Nothing
                                 )
                Me.Add(New ParseError(ParseErrorLevel.Warning, LineNo, StartColumn, EndColumn, Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Adds a new Item without source information to the collection. </summary>
             ''' <param name="Level">    The degree of severity of the error. </param>
             ''' <param name="Message">  The error Message. </param>
             ''' <param name="Hints">    Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath"> Full path of the source file. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
            Public Overloads Sub Add(Level              As ParseErrorLevel,
                                     Message            As String,
                                     Optional Hints     As String = Nothing,
                                     Optional FilePath  As String = Nothing
                                    )
                Me.Add(New ParseError(Level, Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Adds a new Item with source information to the collection. </summary>
             ''' <param name="Level">       The degree of severity of the error. </param>
             ''' <param name="LineNo">      The line number in the source file, starting at 1. </param>
             ''' <param name="StartColumn"> The colulmn number in the source line determining the start of faulty string. </param>
             ''' <param name="EndColumn">   The colulmn number in the source line determining the end of faulty string. </param>
             ''' <param name="Message">     The error Message. </param>
             ''' <param name="Hints">       Hints that could help the user to understand the error. </param>
             ''' <param name="FilePath">    Full path of the source file. </param>
             ''' <remarks></remarks>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Message"/> is <see langword="null"/> or empty or whitespace only. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="LineNo"/> is less than 1. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="StartColumn"/> or <paramref name="EndColumn"/> is less than Zero. </exception>
             ''' <exception cref="System.ArgumentException"> <paramref name="EndColumn"/> is not greater than <paramref name="StartColumn"/>. </exception>
            Public Overloads Sub Add(Level       As ParseErrorLevel,
                                     LineNo      As Long,
                                     StartColumn As Long,
                                     EndColumn   As Long,
                                     Message     As String,
                                     Optional Hints    As String = Nothing,
                                     Optional FilePath As String = Nothing
                                    )
                Me.Add(New ParseError(Level, LineNo, StartColumn, EndColumn, Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Inserts a new error to the collection at the given Index. </summary>
             ''' <param name="Index"> The Index to insert item at. </param>
             ''' <param name="Item"> The <see cref="ParseError"/> to add. </param>
             ''' <exception cref="System.ArgumentNullException"> <paramref name="Item"/> is <see langword="null"/>. </exception>
            Protected Overrides Sub InsertItem(ByVal Index As Integer, ByVal Item As ParseError)
                
                If (Item Is Nothing) Then Throw New System.ArgumentNullException("Item")

                SyncLock (SyncHandle)
                    ' Track Error Level.
                    If (Item.Level = ParseErrorLevel.Error) Then
                        _HasErrors = True
                        _ErrorCount += 1
                    Else
                        _HasWarnings = True
                        _WarningCount += 1
                    End If
                    
                    ' Track availability of error source information.
                    If (Item.HasSource) Then
                        _HasItemsWithSource = True
                    End If
                    
                    ' Track Line Numbers.
                    If (Not IndexOfLineNo.ContainsKey(Item.LineNo)) Then
                        IndexOfLineNo.Add(Item.LineNo, Me.Count)
                    End If
                    
                    ' Add Item.
                    MyBase.InsertItem(Index, Item)
                End SyncLock
            End Sub
            
            ''' <summary> Removes all errors and warnings and also clears status information. </summary>
            Protected Overrides Sub ClearItems()
                SyncLock (SyncHandle)
                    MyBase.ClearItems()
                    
                    FilePath            = Nothing
                    _HasErrors          = False
                    _HasWarnings        = False
                    _HasItemsWithSource = False
                    _ErrorCount         = 0
                    _WarningCount       = 0
                    
                    IndexOfLineNo.Clear()
                End SyncLock
            End Sub
            
            ''' <summary> Hides the inherited method. </summary>
             ''' <param name="Item"> The <see cref="ParseError"/> to remove. </param>
            Protected Shadows Function Remove(Item As Rstyx.Utilities.IO.ParseError) As Boolean
                SyncLock (SyncHandle)
                    Return MyBase.Remove(Item)
                End SyncLock
            End Function
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Full path to the file, the errors are related to. </summary>
            ''' <remarks> This will be treated as the error's source file, if the <b>FilePath</b> field of a particular error is <see langword="null"/>. </remarks>
            Public Property FilePath() As String
            
            ''' <summary> Gets the info whether or not there is at least one error level item. </summary>
            Public ReadOnly Property HasErrors() As Boolean
                Get
                    Return _HasErrors
                End Get
            End Property
            
            ''' <summary> Gets the info whether or not there is at least one warning. </summary>
            Public ReadOnly Property HasWarnings() As Boolean
                Get
                    Return _HasWarnings
                End Get
            End Property
            
            ''' <summary> Gets the info whether or not there is at least one item with source line information. </summary>
            Public ReadOnly Property HasItemsWithSource() As Boolean
                Get
                    Return _HasItemsWithSource
                End Get
            End Property
            
            ''' <summary> Gets the number of Errors. </summary>
            Public ReadOnly Property ErrorCount() As Long
                Get
                    Return _ErrorCount
                End Get
            End Property
            
            ''' <summary> Gets the number of Warnings. </summary>
            Public ReadOnly Property WarningCount() As Long
                Get
                    Return _WarningCount
                End Get
            End Property
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Logs all messages to LoggingConsole considering the error level (without column values and hints). </summary>
            Public Sub ToLoggingConsole()
                SyncLock (SyncHandle)
                    For i As Integer = 0 to Me.Count - 1
                        If (Me.Item(i).Level = ParseErrorLevel.Error) Then
                            Logger.LogError(Me.Item(i).ToString())
                        Else
                            Logger.LogWarning(Me.Item(i).ToString())
                        End If
                    Next
                End SyncLock
            End Sub
            
            ''' <summary> Returns a list of formatted error messages (without column values and hints). </summary>
            Public Overrides Function ToString() As String
                SyncLock (SyncHandle)
                    Dim Builder  As New System.Text.StringBuilder()
                    
                    For i As Integer = 0 to Me.Count - 1
                        Builder.AppendLine(Me.Item(i).ToString())
                    Next
                    
                    Return Builder.ToString()
                End SyncLock
            End Function
            
            ''' <summary> Shows errors and warnings in jEdit's error list (if possible). </summary>
             ''' <remarks> 
             ''' <para>
             ''' The number of items in jEdit's error list will be limited to 100..150 in order to keep jEdit operable.
             ''' </para>
             ''' <para>
             ''' If this method fails, an error is logged but no exception is thrown.
             ''' </para>
             ''' </remarks>
            Public Sub ShowInJEdit()
                SyncLock (SyncHandle)
                    If (Me.HasItemsWithSource) Then
                        ' Create Beanshell script.
                        Dim BshPath As String = Me.ToJeditBeanshell()
                        
                        Try
                            ' Start jEdit and run Beanshell script.
                            Apps.AppUtils.StartEditor(Apps.AppUtils.SupportedEditors.jEdit, "-run=""" & BshPath & """")
                            
                        Catch ex As Exception
                            Logger.LogError(ex, Rstyx.Utilities.Resources.Messages.ParseErrorCollection_ErrorShowInJEdit)
                        End Try
                    End If
                End SyncLock
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Creates a beanshell script file for jEdit that shows max. 100..150 errors and warnings in jEdit's error list. </summary>
             ''' <returns> Full path to the created beanshell script. </returns>
            Private Function ToJeditBeanshell() As String
                
                Dim oErr        As ParseError
                Dim Msg()       As String
                Dim Level       As String
                Dim SourcePath  As String
                Dim SourceText  As String
                Dim ScriptPath  As String = System.IO.Path.GetTempFileName()

                Dim LimitMax    As Integer = 150
                Dim LimitNorm   As Integer = 100
                Dim LimitItems  As Boolean = (Me.Count > LimitMax)
                Dim ItemCount   As Integer = If(LimitItems, LimitNorm, Me.Count)
                
                Using oBsh As New System.IO.StreamWriter(ScriptPath, append:=False, encoding:=System.Text.Encoding.Default)
                    
                    ' Header.
                    oBsh.WriteLine("// :mode=beanshell:")
                    oBsh.WriteLine("import errorlist.*;")
                    oBsh.WriteLine("")
                    oBsh.WriteLine("// Globals")
                    oBsh.WriteLine("final String scriptName = """ & String2Java(ScriptPath) & """;  // scriptPath isn't defined for startup script.")
                    oBsh.WriteLine("final int MaxLoopCount  = 500000000;")
                    oBsh.WriteLine("")
                    oBsh.WriteLine("// Helper: Wait for jedit's last view. Returns true, if found.")
                    oBsh.WriteLine("private boolean WaitForLastView(String scriptName, int MaxLoopCount) {")
                    oBsh.WriteLine("  boolean success = true;")
                    oBsh.WriteLine("  int LoopCount = 0;")
                    oBsh.WriteLine("  Log.log(Log.MESSAGE, scriptName, scriptName + "": Waiting for last view..."");")
                    oBsh.WriteLine("	while ((jEdit.getLastView() == null) && (LoopCount < MaxLoopCount)) {")
                    oBsh.WriteLine("	  LoopCount ++;")
                    oBsh.WriteLine("	}")
                    oBsh.WriteLine("	if (jEdit.getLastView() != null) {")
                    oBsh.WriteLine("	  Log.log(Log.MESSAGE, scriptName, scriptName + "": Got last view. LoopCount="" + LoopCount);")
                    oBsh.WriteLine("	} else {  ")
                    oBsh.WriteLine("	  success = false;")
                    oBsh.WriteLine("	  Log.log(Log.MESSAGE, scriptName, scriptName + "": Didn't got last view. LoopCount="" + LoopCount);")
                    oBsh.WriteLine("	}")
                    oBsh.WriteLine("  return success;")
                    oBsh.WriteLine("}")
                    oBsh.WriteLine("")
                    oBsh.WriteLine("// Helper: Wait for error-list plugin. Returns true, if found.")
                    oBsh.WriteLine("private boolean WaitForErrorList(String scriptName, int MaxLoopCount) {")
                    oBsh.WriteLine("  boolean success = true;")
                    oBsh.WriteLine("  int LoopCount = 0;")
                    oBsh.WriteLine("  Log.log(Log.MESSAGE, scriptName, scriptName + "": Waiting for ErrorList plugin..."");")
                    oBsh.WriteLine("	while ((jEdit.getPlugin(""errorlist.ErrorListPlugin"") == null) && (LoopCount < MaxLoopCount)) {")
                    oBsh.WriteLine("	  LoopCount ++;")
                    oBsh.WriteLine("	}")
                    oBsh.WriteLine("	if (jEdit.getPlugin(""errorlist.ErrorListPlugin"") != null) {")
                    oBsh.WriteLine("	  Log.log(Log.MESSAGE, scriptName, scriptName + "": Found ErrorList plugin. LoopCount="" + LoopCount);")
                    oBsh.WriteLine("	} else {  ")
                    oBsh.WriteLine("	  success = false;")
                    oBsh.WriteLine("	  Log.log(Log.MESSAGE, scriptName, scriptName + "": Didn't found ErrorList plugin. LoopCount="" + LoopCount);")
                    oBsh.WriteLine("	}")
                    oBsh.WriteLine("  return success;")
                    oBsh.WriteLine("}")
                    oBsh.WriteLine("")
                    oBsh.WriteLine("")
                    oBsh.WriteLine("void addErrorToList() {")
                    oBsh.WriteLine("  ")
                    oBsh.WriteLine("  void run() {")
                    oBsh.WriteLine("    // preparations for startup script")
                    oBsh.WriteLine("    boolean success = WaitForLastView(scriptName, MaxLoopCount) && WaitForErrorList(scriptName, MaxLoopCount);")
                    oBsh.WriteLine("	  ")
                    oBsh.WriteLine("    // ")
                    oBsh.WriteLine("	  if (!success) {")
                    oBsh.WriteLine("	    Log.log(Log.MESSAGE, scriptName, scriptName + "": failed."");")
                    oBsh.WriteLine("	  } else {")
                    oBsh.WriteLine("	    Log.log(Log.MESSAGE, scriptName, scriptName + "": working..."");")
                    oBsh.WriteLine("	    ")
                    oBsh.WriteLine("      // get a valid view at jedit's startup")
                    oBsh.WriteLine("      view = jEdit.getLastView();")
                    oBsh.WriteLine("      ")
                    oBsh.WriteLine("      // open error-list dockable and clear list of errors by invoking actions.")
                    oBsh.WriteLine("      jEdit.getAction(""error-list-clear"").invoke(view);")
                    oBsh.WriteLine("      jEdit.getAction(""error-list"").invoke(view);")
                    oBsh.WriteLine("      ")
                    oBsh.WriteLine("      // Create and register DefaultErrorSource")
                    oBsh.WriteLine("      DefaultErrorSource errsrc = new DefaultErrorSource(""Rstyx.Utilities.IO.ParseErrorCollection.ToJeditBeanshell"");")
                    oBsh.WriteLine("      ErrorSource.registerErrorSource(errsrc);")
                    oBsh.WriteLine("      ")
                    oBsh.WriteLine("      ")
                    oBsh.WriteLine("      // *********************************************************************************************")
                    
                    ' Body (variable).
                    For i As Integer = 0 To ItemCount - 1
                        
                        oErr = Me.Item(i)
                        
                        If (oErr.HasSource) Then
                            ' Error details.
                            Level      = If(oErr.Level = ParseErrorLevel.Error, "ErrorSource.ERROR", "ErrorSource.WARNING")
                            SourcePath = String2Java(If(oErr.FilePath IsNot Nothing, oErr.FilePath, Me.FilePath))
                            Msg        = oErr.Message.SplitLines()
                            
                            ' Create new error with main message.
                            SourceText = StringUtils.Sprintf("      DefaultErrorSource.DefaultError err = new DefaultErrorSource.DefaultError(errsrc, %s, ""%s"", %d, %d, %d, ""%s  [Zeile %d]"");",
                                         Level, SourcePath, oErr.LineNo - 1, oErr.StartColumn, oErr.EndColumn, String2Java(Msg(0)), oErr.LineNo)
                            oBsh.WriteLine(SourceText)
                            
                            ' Extra message lines.
                            For k As Integer = 1 To Msg.Length - 1
                                oBsh.WriteLine(StringUtils.Sprintf("      err.addExtraMessage(""%s"");", String2Java(Msg(k))))
                            Next
                            If (oErr.Hints IsNot Nothing) Then
                                Msg = oErr.Hints.Split("\r?\n")
                                For k As Integer = 0 To Msg.Length - 1
                                    oBsh.WriteLine(StringUtils.Sprintf("      err.addExtraMessage(""%s"");", String2Java(Msg(k))))
                                Next
                            End If
                            
                            ' Commit newly created error to list.
                            oBsh.WriteLine("      errsrc.addError(err);" & Environment.NewLine)
                        End If
                    Next

                    ' Message if item count has been limited.
                    If (LimitItems) Then
                        oBsh.WriteLine(StringUtils.Sprintf(Rstyx.Utilities.Resources.Messages.ParseErrorCollection_LimitItemsInJEdit, LimitNorm, Me.Count - LimitNorm))
                    End If
                    
                    ' Footer.
                    oBsh.WriteLine("      //*********************************************************************************************")
                    oBsh.WriteLine("      ")
                    oBsh.WriteLine("      ")
                    oBsh.WriteLine("      // Do not unregister - so the errors stay deleteable by the errorlist plugin itself (see above)")
                    oBsh.WriteLine("      // ErrorSource.unregisterErrorSource(errsrc);")
                    oBsh.WriteLine("      errsrc = null;")
                    oBsh.WriteLine("      ")
                    oBsh.WriteLine("	    Log.log(Log.MESSAGE, scriptName, scriptName + "": finished."");")
                    oBsh.WriteLine("    }")
                    oBsh.WriteLine("  }")
                    oBsh.WriteLine("  ")
                    oBsh.WriteLine("  // Run in background, since script should wait until jedit is ready to run it.")
                    oBsh.WriteLine("  ThreadUtilities.runInBackground(this);")
                    oBsh.WriteLine("}")
                    oBsh.WriteLine("")
                    oBsh.WriteLine("addErrorToList();")
                    
                    oBsh.Close()
                End Using
                
                Return ScriptPath
            End Function
            
            ''' <summary> Converts a String for use in Java source. </summary>
             ''' <param name="Text"> Input String. </param>
             ''' <returns>           Converted String for use in Java source. </returns>
             ''' <remarks> Replaces backslash by double backslash <b>\\"</b> and double quote by <b>\"</b>. </remarks>
            Private Function String2Java(Text As String) As String
                Return Text.Replace("\", "\\").Replace("""", "\""")
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
