
'Namespace Arrays
    
    ''' <summary> Static utility methods for dealing with arrays. </summary>
    Public NotInheritable Class ArrayUtils
        
        #Region "Private Fields"
            
            'Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(MyClass.GetType.FullName)
            Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.ArrayUtils")
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "Enums"
            
            ''' <summary> Sorting types supported by sorting methods of <see cref="ArrayUtils" /> class. </summary>
            Public Enum SortType As Integer
                
                ''' <summary> Numeric if possible, otherwise alphanumeric </summary>
                Numeric      = 0
                
                ''' <summary> Alphanumeric </summary>
                Alphanumeric = 1
            End Enum
            
        #End Region
        
        #Region "Public Static Methods"
            
            ''' <summary> Winds up a one-staged encapsulated Array into a flat Array. </summary>
             ''' <param name="encapsulatedArray"> 1d-Array, whose elements can be 1d-Arrays. </param>
             ''' <returns> A flat 1d-Array (provides the input encapsulation has been one-staged only).
             ''' If an error occures, the input Array is returned. </returns>
            Public Shared Function getFlatArray(byRef encapsulatedArray As Object()) As Object()
                'Deklarationen
                  Dim i            As Long
                  Dim j            As Long
                  Dim k            As Long
                  Dim ub1          As Long
                  Dim ub2          As Long
                  Dim CountParms1  As Long
                  Dim CountParms2  As Long
                  Dim EmbedCount() As Long
                  Dim isEmbedded   As Boolean
                  Dim tmpArray     As Object = new Object() {}
                  Dim flatParms    As Object = new Object() {}
                  
                Try
                    'Initialisierung
                      ub1 = UBound(encapsulatedArray)
                      CountParms1 = ub1 + 1
                      CountParms2 = 0
                      ReDim EmbedCount(ub1)
                    
                    'Feststellen, ob und an welchen Stellen das Eingabe-Array verschachtelte Parameter enthält
                    For i = 0 To ub1
                        If (IsArray(encapsulatedArray(i))) Then
                            isEmbedded = True
                            If (not encapsulatedArray(i) is Nothing) then
                                ub2 = UBound(encapsulatedArray(i))
                                EmbedCount(i) = ub2 + 1
                            Else
                                EmbedCount(i) = 0
                            End If
                        Else
                            EmbedCount(i) = 1
                        End If
                        CountParms2 = CountParms2 + EmbedCount(i)
                    Next
                    
                    'ggf. das übernommene ParamArray in ein nicht verschachteltes Array wandeln
                    If (Not isEmbedded) Then
                        flatParms = encapsulatedArray
                    Else
                        ReDim flatParms(CountParms2 - 1)
                        j = 0
                        For i = 0 To ub1
                            If (EmbedCount(i) > 0) Then
                                If (IsArray(encapsulatedArray(i))) Then
                                    tmpArray = encapsulatedArray(i)
                                    For k = 0 To UBound(tmpArray)
                                      flatParms(j) = tmpArray(k)
                                      j = j + 1
                                    Next
                                Else
                                    flatParms(j) = encapsulatedArray(i)
                                    j = j + 1
                                End If
                            End If
                        Next
                    End If
                    tmpArray = flatParms
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "getFlatArray(): unbekannter Fehler")
                    tmpArray = encapsulatedArray
                End Try
                
                Return tmpArray
            End Function
            
            ''' <summary>
            ''' Sorts the the whole 2d-Array "Matrix" by a given column by invoking <see cref="ArrayUtils.QuickSort2d" />.
            ''' </summary>
             ''' <param name="Matrix">      The Array to sort. </param>
             ''' <param name="Key_Dim">     Determines the column to sort by (in conjunction with "Key_Idx"). </param>
             ''' <param name="Key_Idx">     Determines the column to sort by (in conjunction with "Key_Dim"). </param>
             ''' <param name="SortingType"> Numeric or alfanumeric </param>
             ''' <param name="Descending">  If True, sorting is performed descending. </param>
             ''' <remarks></remarks>
            Public Shared Sub SortArray2d(byRef Matrix As Object(,), byVal Key_Dim As Long, byVal Key_Idx As Long, byVal SortingType As SortType, byVal Descending As Boolean)
                Dim LowerIndex  As Long
                Dim UpperIndex  As Long
                Dim sec_Dim     As Long
                
                Try
                    if (Key_Dim = 1) then sec_Dim = 2 else sec_Dim = 1
                    LowerIndex = lbound(Matrix, sec_Dim)
                    UpperIndex = ubound(Matrix, sec_Dim)
                    Logger.logDebug(StringUtils.sprintf("SortArray2d(): Initialisierung QuickSort2d(Matrix,%s,%s,%s,%s,%s,%s", Key_Dim, Key_Idx, SortingType.ToDisplayString(), Descending, LowerIndex, UpperIndex))
                    QuickSort2d(Matrix, Key_Dim, Key_Idx, SortingType, Descending, LowerIndex, UpperIndex)
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "SortArray2d(): unbekannter Fehler")
                End Try
            end Sub
            
            ''' <summary> Sorts a given range of a 2d-Array "Matrix" by a given column. </summary>
             ''' <param name="Matrix">      The Array to sort. </param>
             ''' <param name="Key_Dim">     Determines the column to sort by (in conjunction with "Key_Idx"). </param>
             ''' <param name="Key_Idx">     Determines the column to sort by (in conjunction with "Key_Dim"). </param>
             ''' <param name="SortingType"> Numeric or alfanumeric </param>
             ''' <param name="Descending">  If True, sorting is performed descending. </param>
             ''' <param name="LowerIndex">  Start of range to sort (for QuickSort internal partitioning). </param>
             ''' <param name="UpperIndex">  End of range to sort (for QuickSort internal partitioning). </param>
             ''' <remarks> 
             ''' <para>
             ''' To sort the entire array <see cref="ArrayUtils.SortArray2d" /> should be used which in turn utilizes this method. 
             ''' </para>
             ''' <para>
             ''' Based on a <a href="http://www.activevb.de/tutorials/tut_sortalgo/sortalgo.html" > tutorial for 1d-Array QuickSort by Klaus Neumann </a>
             ''' </para>
             ''' </remarks>
            Public Shared Sub QuickSort2d(byRef Matrix As Object(,), _
                                          byVal Key_Dim As Long, _
                                          byVal Key_Idx As Long, _ 
                                          byVal SortingType As SortType, _
                                          byVal Descending As Boolean, _
                                          byVal LowerIndex As Long, _
                                          byVal UpperIndex As Long)
                Dim sec_Dim     As Long
                Dim i           As Long
                Dim k           As Long
                Dim idxM        As Long
                Dim idxColumn   As Long
                Dim Value_idxM  As Object
                Dim temp        As Object
                
                Try
                    'Logger.logDebug("QuickSort2d(): Starte QuickSort2d(Matrix," & Key_Dim & "," & Key_Idx & "," & SortingType & "," & Descending & "," & LowerIndex & "," & UpperIndex & ")") 
                    if ((LowerIndex < 0) or (UpperIndex < 0)) then
                        Logger.logDebug("QuickSort2d(): Sortieren unmöglich, da mindestens ein Index < 0!")
                    else
                        if (Key_Dim = 1) then sec_Dim = 2 else sec_Dim = 1
                        
                        idxM = System.Math.Truncate((LowerIndex + UpperIndex) / 2)
                        i    = LowerIndex
                        k    = UpperIndex
                        
                        'Pivotelement: Wert ca. in der Mitte der Schlüsselspalte.
                        Value_idxM = ArrayValue(Matrix, Key_Dim, Key_Idx, idxM)
                        
                        Do
                            Do While isLesser(ArrayValue(Matrix, Key_Dim, Key_Idx, i), Value_idxM, SortingType, Descending)
                                i = i + 1
                                'Schleifenausgang spätestens mit i = idxM + 1
                            Loop
                            
                            Do While isLesser(Value_idxM, ArrayValue(Matrix, Key_Dim, Key_Idx, k), SortingType, Descending)
                                k = k - 1
                                'Schleifenausgang spätestens mit k = idxM - 1
                            Loop
                            
                            If (i <= k) Then
                                'Arraywerte der Indizes i und k tauschen.
                                For idxColumn = lbound(Matrix, Key_Dim) To ubound(Matrix, Key_Dim)
                                    if (Key_Dim = 1) then
                                        temp = Matrix(idxColumn, k)
                                        Matrix(idxColumn, k) = Matrix(idxColumn, i)
                                        Matrix(idxColumn, i) = temp
                                    else
                                        temp = Matrix(k, idxColumn)
                                        Matrix(k, idxColumn) = Matrix(i, idxColumn)
                                        Matrix(i, idxColumn) = temp
                                    end if
                                Next
                                
                                i = i + 1
                                k = k - 1
                            End If
                            
                        Loop Until (i > k)
                        
                        If (LowerIndex < k) Then QuickSort2d(Matrix, Key_Dim, Key_Idx, SortingType, Descending, LowerIndex, k)
                        If (i < UpperIndex) Then QuickSort2d(Matrix, Key_Dim, Key_Idx, SortingType, Descending, i, UpperIndex)
                    end if
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "QuickSort2d(): unbekannter Fehler")
                End Try
            End Sub
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Returns the value of a given matrix element (Helper for QuickSort2d). </summary>
             ''' <param name="Matrix">  2d-Array to sort.                                                 </param>
             ''' <param name="Key_Dim"> (1|2)   No. of Dimension, containing the key column.              </param>
             ''' <param name="Key_Idx"> (0,1,.) No. of Index (in Key_Dim) which points to the key column. </param>
             ''' <param name="Sek_Idx"> (0,1,.) No. of the other Index.                                   </param>
             ''' <returns> Value of a given matrix element. </returns>
            Private Shared Function ArrayValue(byRef Matrix As Object(,), byVal Key_Dim As Long, byVal Key_Idx As Long, byVal Sek_Idx As Long) As Object
                Dim RetValue  As Object = Nothing
                
                Try
                    if (Key_Dim = 1) then
                      RetValue = Matrix(Key_Idx, Sek_Idx)
                    else
                      RetValue = Matrix(Sek_Idx, Key_Idx)
                    end if
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "ArrayValue(): unbekannter Fehler")
                End Try
                
                Return RetValue
            end function
            
            ''' <summary> Compares two object values </summary>
             ''' <param name="Value1"> First value </param>
             ''' <param name="Value2"> Second value </param>
             ''' <param name="SortingType"> Numeric or alfanumeric </param>
             ''' <param name="Reverse"> If true, the return value means "isGreater" </param>
             ''' <returns> True, if first value is lesser than second otherwise false. </returns>
             ''' <remarks> A Null value is lesser than all other values. </remarks>
            Private Shared Function isLesser(ByRef Value1 As Object, ByRef Value2 As Object, ByVal SortingType As SortType, ByVal Reverse As Boolean) As Boolean
                dim CompareResult     As Boolean = false
                dim intRev            As Integer
                
                Try
                    if (Reverse) then intRev = -1 else intRev = 1
                    
                    if (isNothing(Value1) OrElse isNothing(Value2)) then
                      'Ein NULL-Wert ist immer kleiner als alle anderen Werte
                      if (isNothing(Value1) And isNothing(Value2)) then
                        CompareResult = false
                      elseif (isNothing(Value1)) then
                        if (not Reverse) then CompareResult = true else CompareResult = false
                      else
                        if (Reverse) then CompareResult = true else CompareResult = false
                      end if
                      
                    else
                      if (SortingType = SortType.Numeric) then
                        'Wenn numerisch nicht möglich, dann Alphanumerisch => Texte werden ans Ende sortiert.
                        if (not (isNumeric(Value1) and isNumeric(Value2))) then SortingType = SortType.Alphanumeric
                      end if
                      
                      select case SortingType
                        
                        case SortType.Numeric
                            Value1 = cDbl(Value1)
                            Value2 = cDbl(Value2)
                            if (not Reverse) then CompareResult = (Value1 < Value2) else CompareResult = (Value2 < Value1)
                            
                        case else  ' Alphanumeric
                            Value1 = cStr(Value1)
                            Value2 = cStr(Value2)
                            if (strComp(Value1, Value2, vbTextCompare) * intRev = -1) then CompareResult = true
                      end select
                    end if
                    
                Catch ex As System.Exception
                    Logger.logError(ex, "isLesser(): unbekannter Fehler")
                End Try
                
                Return CompareResult
            end function
            
        #End Region
        
    End Class
    
    ' <summary> Extension methods for Arrays or other types dealing with Arrays. </summary>
    'Public Module ArrayExtensions
        
        '<System.Runtime.CompilerServices.Extension()> 
        'Public Function ToArray(ByRef Value As System.Collections.Generic.IEnumerable(Of T)) As Array(Of T)
        '    ' Convert FileFilters IEnumerable to an Array
        '    Dim i As Integer = 0
        '    For Each FileFilter As String In FileFilters
        '        FileFilterArray(++i) = FileFilter
        '        i += 1
        '    Next
        '    Return Value
        'End Function
        
        
    'End Module
    
'End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
