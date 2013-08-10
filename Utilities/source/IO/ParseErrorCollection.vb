
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Namespace IO
    
    ''' <summary>  A collection of <see cref="Rstyx.Utilities.IO.ParseError"/> objects. </summary>
     ''' <remarks>  </remarks>
    Public Class ParseErrorCollection
        Inherits System.Collections.ObjectModel.Collection(Of Rstyx.Utilities.IO.ParseError)
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.IO.ParseErrorCollection")
            
            Protected _HasErrors    As Boolean
            Protected _HasWarnings  As Boolean
            
            Protected _ErrorCount   As Long
            Protected _WarningCount As Long
            
            Protected IndexOfLineNo As New Dictionary(Of Long, Integer)
            
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
            
            ''' <summary> Adds a new error to the collection. </summary>
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
            Public Shadows Sub AddError(LineNo      As Long,
                                        StartColumn As Long,
                                        EndColumn   As Long,
                                        Message     As String,
                                        Optional Hints    As String = Nothing,
                                        Optional FilePath As String = Nothing
                                       )
                MyBase.Add(New ParseError(ParseErrorLevel.[Error], LineNo, StartColumn, EndColumn, Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Adds a new warning to the collection. </summary>
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
            Public Shadows Sub AddWarning(LineNo      As Long,
                                          StartColumn As Long,
                                          EndColumn   As Long,
                                          Message     As String,
                                          Optional Hints    As String = Nothing,
                                          Optional FilePath As String = Nothing
                                         )
                MyBase.Add(New ParseError(ParseErrorLevel.Warning, LineNo, StartColumn, EndColumn, Message, Hints, FilePath))
            End Sub
            
            ''' <summary> Adds a new Item to the collection. </summary>
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
            Public Shadows Sub Add(Level       As ParseErrorLevel,
                                   LineNo      As Long,
                                   StartColumn As Long,
                                   EndColumn   As Long,
                                   Message     As String,
                                   Optional Hints    As String = Nothing,
                                   Optional FilePath As String = Nothing
                                  )
                MyBase.Add(New ParseError(Level, LineNo, StartColumn, EndColumn, Message, Hints, FilePath))
            End Sub
            
            ''' <inheritdoc/>
            Public Shadows Sub Add(Item As Rstyx.Utilities.IO.ParseError)
                'If (Item Is Nothing) Then Then Throw New System.ArgumentNullException("Item")
                
                ' Track Error Level.
                If (Item.Level = ParseErrorLevel.Error) Then
                    _HasErrors = True
                    _ErrorCount += 1
                Else
                    _HasWarnings = True
                    _WarningCount += 1
                End If
                
                ' Track Line Numbers.
                If (Not IndexOfLineNo.ContainsKey(Item.LineNo)) Then
                    IndexOfLineNo.Add(Item.LineNo, Me.Count)
                End If
                
                ' Add Item.
                MyBase.Add(Item)
            End Sub
            
            ''' <inheritdoc/>
            Public Shadows Sub Clear()
                MyBase.Clear()
                
                FilePath      = Nothing
                _HasErrors    = False
                _HasWarnings  = False
                _ErrorCount   = 0
                _WarningCount = 0
                
                IndexOfLineNo.Clear()
            End Sub
            
            ''' <inheritdoc/>
            Protected Shadows Sub Insert(Index As Integer, Item As Rstyx.Utilities.IO.ParseError)
                MyBase.Insert(Index, Item)
            End Sub
            
            ''' <inheritdoc/>
            Protected Shadows Function Remove(Item As Rstyx.Utilities.IO.ParseError) As Boolean
                Return MyBase.Remove(Item)
            End Function
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Full path to the file, the errors are related to. </summary>
            Public Property FilePath   As String
            
            ''' <summary> Gets the info whether or not there is at least one error. </summary>
            Public ReadOnly Property HasErrors  As Boolean
                Get
                    Return _HasErrors
                End Get
            End Property
            
            ''' <summary> Gets the info whether or not there is at least one warning. </summary>
            Public ReadOnly Property HasWarnings  As Boolean
                Get
                    Return _HasWarnings
                End Get
            End Property
            
            ''' <summary> Gets the number of Errors. </summary>
            Public ReadOnly Property ErrorCount  As Long
                Get
                    Return _ErrorCount
                End Get
            End Property
            
            ''' <summary> Gets the number of Warnings. </summary>
            Public ReadOnly Property WarningCount  As Long
                Get
                    Return _WarningCount
                End Get
            End Property
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
